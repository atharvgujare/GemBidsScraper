import json
import time
from dataclasses import dataclass
from typing import List, Dict, Any, Set
from urllib.parse import urljoin

from playwright.sync_api import (
    sync_playwright,
    TimeoutError as PlaywrightTimeoutError,
)

from config import get_config

_cfg = get_config()
VERBOSE = _cfg.get("verbose", False)

ADVANCE_SEARCH_URL = "https://bidplus.gem.gov.in/advance-search"
BASE_URL = "https://bidplus.gem.gov.in/"

# Headless scraping doesn't render anything visually, so styling/fonts/
# images/media are pure waste - blocking them speeds up every page load.
BLOCKED_RESOURCE_TYPES = {"image", "font", "media", "stylesheet"}

# How long to wait for the next page's bid list to visibly change.
# Deep pagination (page 50+) can be slower than the first few pages,
# so this is generous on purpose - a real dead end (no next button)
# is detected instantly and doesn't wait at all.
PAGINATION_WAIT_TIMEOUT_MS = 45000
PAGINATION_MAX_RETRIES = 4
# Pause before each retry - if a timeout is caused by session/rate
# throttling on GeM's side rather than a stale DOM read, hammering
# the click again immediately just repeats the same failure.
PAGINATION_RETRY_DELAY_S = 2

# Print a status line every N pages instead of every single page, to
# keep the console readable on long (100+ page) scrapes. Full per-page
# detail is still available by setting "verbose": true in config.json.
STATUS_EVERY_N_PAGES = 10


@dataclass
class BidCard:
    bid_number: str
    pdf_url: str
    item_name: str
    quantity: str
    ministry: str
    department: str
    start_date: str
    end_date: str


def _block_unneeded_resources(route):
    if route.request.resource_type in BLOCKED_RESOURCE_TYPES:
        route.abort()
    else:
        route.continue_()


def extract_field(lines, label):
    label = label.lower()
    for i, line in enumerate(lines):
        current = line.strip()
        lower = current.lower()
        if lower == label:
            if i + 1 < len(lines):
                return lines[i + 1].strip()
        if lower.startswith(label + ":"):
            return current.split(":", 1)[1].strip()
    return ""


def extract_by_label(card_text, label):
    try:
        lines = [line.strip() for line in card_text.splitlines() if line.strip()]
        label = label.lower()
        for i, line in enumerate(lines):
            current = line.strip()
            lower = current.lower()
            if lower == label:
                if i + 1 < len(lines):
                    return lines[i + 1]
            if lower.startswith(label + ":"):
                return current.split(":", 1)[1].strip()
    except Exception:
        pass
    return ""


def select_ministry_and_search(page, ministry_name: str):
    page.goto(ADVANCE_SEARCH_URL, wait_until="domcontentloaded")

    ministry_tab = page.locator("#ministry-tab, a[href='#tab1']").first
    if ministry_tab.is_visible():
        ministry_tab.click()

    page.wait_for_function(
        """
        () => {
            const select = document.querySelector("#ministry");
            return select && select.options && select.options.length > 5;
        }
        """,
        timeout=30000,
    )

    page.evaluate(
        """
        (targetMinistry) => {
            const select = document.querySelector("#ministry");
            if (!select) return null;

            let foundVal = null;
            const targetLower = targetMinistry.toLowerCase().trim();

            for (let opt of select.options) {
                const optVal = opt.value ? opt.value.toLowerCase().trim() : "";
                const optText = opt.text ? opt.text.toLowerCase().trim() : "";
                if (optVal === targetLower || optText === targetLower) {
                    foundVal = opt.value;
                    break;
                }
            }

            if (!foundVal) {
                for (let opt of select.options) {
                    if (opt.text.toLowerCase().includes("defence") || opt.value.toLowerCase().includes("defence")) {
                        foundVal = opt.value;
                        break;
                    }
                }
            }

            if (foundVal) {
                select.value = foundVal;
                select.dispatchEvent(new Event("change", { bubbles: true }));
                if (typeof $ !== 'undefined') {
                    $("#ministry").val(foundVal).trigger("change");
                }
                return foundVal;
            }
            return null;
        }
        """,
        ministry_name,
    )

    page.evaluate(
        """
        () => {
            if (typeof searchBid === 'function') {
                searchBid('ministry-search');
            } else {
                const btn = document.querySelector("#ministry-search #searchByBid");
                if (btn) btn.click();
            }
        }
        """
    )

    page.wait_for_selector("div.card", timeout=30000)


def _go_to_next_page(page, prev_bid_numbers: Set[str]) -> bool:
    """
    Returns True if it advanced to a next page, False if there genuinely
    is no next page. Raises PlaywrightTimeoutError only after all retries
    are exhausted, so a slow-but-real next page isn't mistaken for a
    dead end.

    Compares the FULL set of bid numbers on the page (not just the first
    card) so a pinned/sponsored/repeated card at the top of the list
    doesn't get mistaken for "the page never changed."
    """
    next_button = page.locator("a.page-link.next, li.next a, a:has-text('Next')").first

    if not next_button.is_visible():
        if VERBOSE:
            print("[Scraper] Next button not found - end of results.")
        return False

    next_class = next_button.get_attribute("class") or ""
    if "disabled" in next_class:
        if VERBOSE:
            print("[Scraper] Reached last page.")
        return False

    prev_bids_json = json.dumps(list(prev_bid_numbers))

    last_error = None
    for attempt in range(1, PAGINATION_MAX_RETRIES + 1):
        try:
            next_button.scroll_into_view_if_needed()
            next_button.click()
            page.wait_for_function(
                """
                oldBidsJson => {
                    const oldBids = new Set(JSON.parse(oldBidsJson));
                    const els = Array.from(document.querySelectorAll("a.bid_no_hover"));
                    const newBids = els.map(el => el.innerText.trim());
                    // Advanced if the page has cards and at least one
                    // bid on it wasn't on the previous page.
                    return newBids.length > 0 && newBids.some(b => !oldBids.has(b));
                }
                """,
                arg=prev_bids_json,
                timeout=PAGINATION_WAIT_TIMEOUT_MS,
            )
            page.wait_for_selector("div.card", timeout=30000)
            return True
        except PlaywrightTimeoutError as ex:
            last_error = ex
            if VERBOSE:
                print(f"[Scraper] Pagination timeout, retry {attempt}/{PAGINATION_MAX_RETRIES}...")
            if attempt < PAGINATION_MAX_RETRIES:
                time.sleep(PAGINATION_RETRY_DELAY_S)
            next_button = page.locator("a.page-link.next, li.next a, a:has-text('Next')").first

    raise last_error


def get_pdf_urls(total_pages: int, ministry: str = "Ministry of Defence") -> List[Dict[str, Any]]:
    bids = []
    start = time.perf_counter()
    print(f"[Scraper] Searching '{ministry}' bids ({total_pages} pages requested)...")

    with sync_playwright() as p:
        browser = p.chromium.launch(headless=True)
        page = browser.new_page()
        page.route("**/*", _block_unneeded_resources)

        try:
            select_ministry_and_search(page, ministry)
        except Exception as ex:
            print(f"[Scraper] Could not start search: {ex}")
            browser.close()
            return []

        current_page = 0
        for current_page in range(1, total_pages + 1):
            cards_data = page.evaluate(
                """
                () => {
                    const cards = Array.from(document.querySelectorAll("div.card"));
                    return cards.map(card => {
                        const bidLink = card.querySelector("a.bid_no_hover");
                        if (!bidLink) return null;

                        const bidNumber = (bidLink.innerText || "").trim();
                        const href = bidLink.getAttribute("href") || "";
                        if (!href) return null;

                        let itemName = "";
                        const anchors = Array.from(card.querySelectorAll("a"));
                        for (const a of anchors) {
                            const txt = (a.innerText || "").trim();
                            const lower = txt.toLowerCase();
                            if (txt && txt !== bidNumber && !lower.includes("view") && !lower.includes("document")) {
                                itemName = txt;
                                break;
                            }
                        }

                        return {
                            bidNumber: bidNumber,
                            href: href,
                            itemName: itemName,
                            cardText: card.innerText || ""
                        };
                    }).filter(Boolean);
                }
                """
            )

            for cd in cards_data:
                try:
                    bid_number = cd["bidNumber"]
                    href = cd["href"]
                    pdf_url = href if href.startswith("http") else urljoin(BASE_URL, href)
                    item_name = cd["itemName"]
                    card_text = cd["cardText"]

                    quantity = extract_by_label(card_text, "Quantity")
                    lines = [line.strip() for line in card_text.splitlines() if line.strip()]
                    ministry_val = extract_field(lines, "Ministry")
                    department_val = extract_field(lines, "Department")
                    start_date = extract_by_label(card_text, "Start Date")
                    end_date = extract_by_label(card_text, "End Date")

                    bids.append(BidCard(
                        bid_number=bid_number, pdf_url=pdf_url, item_name=item_name,
                        quantity=quantity, ministry=ministry_val or ministry,
                        department=department_val, start_date=start_date, end_date=end_date,
                    ))
                except Exception:
                    pass

            if VERBOSE:
                print(f"[Scraper] Page {current_page}/{total_pages}: {len(cards_data)} cards, "
                      f"{len(set(b.bid_number for b in bids))} unique so far")
            elif current_page % STATUS_EVERY_N_PAGES == 0:
                print(f"[Scraper] Page {current_page}/{total_pages}, "
                      f"{len(set(b.bid_number for b in bids))} unique bids so far...")

            if current_page == total_pages:
                break

            prev_bid_numbers = {cd["bidNumber"] for cd in cards_data}
            try:
                advanced = _go_to_next_page(page, prev_bid_numbers)
            except PlaywrightTimeoutError:
                print(f"[Scraper] Stopped at page {current_page}: pagination kept timing out.")
                break

            if not advanced:
                print(f"[Scraper] Stopped at page {current_page}: no further pages on GeM for this filter.")
                break

        browser.close()

    unique = {bid.bid_number: bid for bid in bids}
    bids = list(unique.values())
    elapsed = time.perf_counter() - start
    print(f"[Scraper] Done: {len(bids)} unique bids from {current_page} page(s) in {elapsed:.1f}s")

    return [bid.__dict__ for bid in bids]