import json
import time
from typing import Any, Dict, List
from urllib.parse import urljoin

import requests

from config import get_config


# ============================================================
# CONFIGURATION
# ============================================================

_cfg = get_config()
VERBOSE = _cfg.get("verbose", False)

BASE_URL = "https://bidplus.gem.gov.in/"
ADVANCE_SEARCH_URL = "https://bidplus.gem.gov.in/advance-search"
SEARCH_BIDS_URL = "https://bidplus.gem.gov.in/search-bids"
ALL_BIDS_PAGE_URL = "https://bidplus.gem.gov.in/all-bids"
ALL_BIDS_DATA_URL = "https://bidplus.gem.gov.in/all-bids-data"

REQUEST_TIMEOUT_S = 20

# Small delay between page requests.
REQUEST_DELAY_S = 0.3

MAX_RETRIES_PER_PAGE = 3
RETRY_BACKOFF_S = 2

USER_AGENT = (
    "Mozilla/5.0 (Windows NT 10.0; Win64; x64) "
    "AppleWebKit/537.36 (KHTML, like Gecko) "
    "Chrome/151.0.0.0 Safari/537.36"
)


# ============================================================
# CREATE SESSION
# ============================================================

def _make_session() -> requests.Session:
    """
    Creates a requests session and loads GeM's advance-search page.

    The initial GET establishes the GeM session and obtains the
    csrf_gem_cookie required by the search-bids request.
    """

    session = requests.Session()

    session.headers.update({
        "User-Agent": USER_AGENT,
        "Accept": (
            "text/html,application/xhtml+xml,"
            "application/xml;q=0.9,image/avif,image/webp,"
            "image/apng,*/*;q=0.8"
        ),
        "Accept-Language": "en-US,en;q=0.9",
        "Connection": "keep-alive",
    })

    print("[HTTP] Opening GeM advance-search page...")

    response = session.get(
        ADVANCE_SEARCH_URL,
        timeout=REQUEST_TIMEOUT_S
    )

    response.raise_for_status()

    print("[HTTP] Initial response:", response.status_code)
    print("[HTTP] Cookies:", session.cookies.get_dict())

    if "csrf_gem_cookie" not in session.cookies.get_dict():
        raise RuntimeError(
            "GeM did not set csrf_gem_cookie on initial page load."
        )

    return session


# ============================================================
# GET VALUE FROM GEM JSON FIELD
# ============================================================

def _get_doc_value(
    doc: Dict[str, Any],
    key: str,
    default: Any = ""
) -> Any:
    """
    GeM commonly returns fields as arrays.

    Example:

        "b_bid_number": ["GEM/2026/B/1234567"]

    This helper returns the first value from such arrays.
    """

    value = doc.get(key, default)

    if isinstance(value, list):

        if not value:
            return default

        return value[0]

    return value


# ============================================================
# PARSE GEM JSON RESPONSE
# ============================================================

def _parse_cards(
    response_text: str,
    ministry: str
) -> List[Dict[str, Any]]:
    """
    Parses the JSON returned by GeM's /search-bids endpoint.

    The response structure discovered during testing is:

        response
            -> response
                -> docs

    Each item in docs represents one bid.
    """

    # --------------------------------------------------------
    # Parse JSON
    # --------------------------------------------------------

    try:
        data = json.loads(response_text)

    except json.JSONDecodeError as ex:

        print("[Parser] GeM response is not valid JSON.")
        print("[Parser] JSON error:", ex)
        print("[Parser] Response preview:")
        print(response_text[:1000])

        return []

    # --------------------------------------------------------
    # Get docs
    # --------------------------------------------------------

    try:
        inner_response = data["response"]["response"]

        docs = inner_response["docs"]

        total_found = inner_response.get(
            "numFound",
            0
        )

        start = inner_response.get(
            "start",
            0
        )

    except (KeyError, TypeError):

        print(
            "[Parser] Could not find "
            "response.response.docs in GeM response."
        )

        if isinstance(data, dict):
            print(
                "[Parser] Top-level keys:",
                list(data.keys())
            )

        return []

    if not isinstance(docs, list):

        print(
            "[Parser] GeM docs is not a list."
        )

        return []

    if VERBOSE:

        print(
            f"[Parser] API returned {len(docs)} bids "
            f"(start={start}, total={total_found})"
        )

    # --------------------------------------------------------
    # Parse bids
    # --------------------------------------------------------

    bids: List[Dict[str, Any]] = []

    for doc in docs:

        if not isinstance(doc, dict):
            continue

        # ----------------------------------------------------
        # BID NUMBER
        # ----------------------------------------------------

        bid_number = _get_doc_value(
            doc,
            "b_bid_number"
        )

        if not bid_number:
            continue

        bid_id = _get_doc_value(
            doc,
            "b_id"
        )

        # GeM search can also return reverse-auction records such as
        # GEM/2026/R/123456. In such records, GeM provides
        # b_bid_number_parent and b_id_parent pointing to the underlying
        # bid document (GEM/.../B/...). Resolve to the parent /B/ bid.
        if "/B/" not in str(bid_number):
            parent_bid = _get_doc_value(doc, "b_bid_number_parent")
            parent_id = _get_doc_value(doc, "b_id_parent")
            if parent_bid and "/B/" in str(parent_bid):
                bid_number = parent_bid
                if parent_id:
                    bid_id = parent_id
            else:
                continue

        # ----------------------------------------------------
        # ITEM / CATEGORY
        # ----------------------------------------------------

        item_name = (
            _get_doc_value(
                doc,
                "b_category_name"
            )
            or
            _get_doc_value(
                doc,
                "bd_category_name"
            )
        )

        # ----------------------------------------------------
        # QUANTITY
        # ----------------------------------------------------

        quantity = _get_doc_value(
            doc,
            "b_total_quantity"
        )

        # ----------------------------------------------------
        # MINISTRY
        # ----------------------------------------------------

        ministry_value = (
            _get_doc_value(
                doc,
                "ba_official_details_minName"
            )
            or ministry
        )

        # ----------------------------------------------------
        # DEPARTMENT
        # ----------------------------------------------------

        department = _get_doc_value(
            doc,
            "ba_official_details_deptName"
        )

        # ----------------------------------------------------
        # START DATE
        # ----------------------------------------------------

        start_date = _get_doc_value(
            doc,
            "final_start_date_sort"
        )

        # ----------------------------------------------------
        # END DATE
        # ----------------------------------------------------

        end_date = _get_doc_value(
            doc,
            "final_end_date_sort"
        )

        # ----------------------------------------------------
        # BID ID (already resolved above if parent was used)
        # ----------------------------------------------------
        if not bid_id:
            bid_id = _get_doc_value(
                doc,
                "b_id"
            )

        # ----------------------------------------------------
        # CATEGORY ID
        # ----------------------------------------------------

        category_id = _get_doc_value(
            doc,
            "b_cat_id"
        )

        # ----------------------------------------------------
        # STATUS
        # ----------------------------------------------------

        status = _get_doc_value(
            doc,
            "b_status"
        )

        # ----------------------------------------------------
        # BID TYPE
        # ----------------------------------------------------

        bid_type = _get_doc_value(
            doc,
            "b_bid_type"
        )

        # ----------------------------------------------------
        # CUSTOM ITEM
        # ----------------------------------------------------

        is_custom_item = _get_doc_value(
            doc,
            "b_is_custom_item"
        )

        # ----------------------------------------------------
        # BUNCH
        # ----------------------------------------------------

        is_bunch = _get_doc_value(
            doc,
            "b_is_bunch"
        )

        # ----------------------------------------------------
        # INACTIVE
        # ----------------------------------------------------

        is_inactive = _get_doc_value(
            doc,
            "b_is_inactive"
        )

        # ----------------------------------------------------
        # PDF URL
        # ----------------------------------------------------

        pdf_url = ""

        if bid_id:
            pdf_url = urljoin(
                BASE_URL,
                f"showbidDocument/{bid_id}"
            )

        # ----------------------------------------------------
        # CREATE BID
        # ----------------------------------------------------

        bids.append({
            "bid_number": bid_number,
            "pdf_url": pdf_url,
            "item_name": item_name,
             "quantity": str(quantity or ""),
            "ministry": ministry_value,
            "department": department,
            "start_date": start_date,
            "end_date": end_date,

            # Additional fields available directly from API
            "bid_id": bid_id,
            "category_id": category_id,
            "status": status,
            "bid_type": bid_type,
            "is_custom_item": is_custom_item,
            "is_bunch": is_bunch,
            "is_inactive": is_inactive,
        })

    return bids


# ============================================================
# FETCH ONE PAGE
# ============================================================

def _fetch_page_html(
    session: requests.Session,
    ministry: str,
    page: int
) -> str:
    """
    Calls GeM's /search-bids endpoint.

    Despite the historical function name, the endpoint currently
    returns JSON rather than HTML.
    """

    payload_obj = {
        "searchType": "ministry-search",
        "ministry": ministry,
        "buyerState": "",
        "organization": "",
        "department": "",
        "bidEndFromMin": "",
        "bidEndToMin": "",
        "page": page,
    }

    headers = {
        "X-Requested-With": "XMLHttpRequest",
        "Referer": ADVANCE_SEARCH_URL,
        "Origin": "https://bidplus.gem.gov.in",
        "Content-Type": (
            "application/x-www-form-urlencoded; "
            "charset=UTF-8"
        ),
    }

    last_exception: Exception = RuntimeError(
        "No request attempt was made."
    )

    for attempt in range(
        1,
        MAX_RETRIES_PER_PAGE + 1
    ):

        # ----------------------------------------------------
        # Get current CSRF token
        # ----------------------------------------------------

        csrf_token = session.cookies.get(
            "csrf_gem_cookie",
            ""
        )

        if not csrf_token:

            raise RuntimeError(
                "csrf_gem_cookie is missing from the session."
            )

        # ----------------------------------------------------
        # Form data
        # ----------------------------------------------------

        form_data = {
            "payload": json.dumps(
                payload_obj,
                separators=(",", ":")
            ),
            "csrf_bd_gem_nk": csrf_token,
        }

        try:

            if VERBOSE:

                print(
                    f"[HTTP] Requesting page {page} "
                    f"(attempt {attempt}/"
                    f"{MAX_RETRIES_PER_PAGE})..."
                )

            response = session.post(
                SEARCH_BIDS_URL,
                data=form_data,
                headers=headers,
                timeout=REQUEST_TIMEOUT_S
            )

            response.raise_for_status()

            # ------------------------------------------------
            # Debug first page only
            # ------------------------------------------------

            if page == 1 and attempt == 1:

                print()
                print("=" * 70)
                print("GEM RESPONSE DEBUG")
                print("=" * 70)

                print(
                    "STATUS:",
                    response.status_code
                )

                print(
                    "URL:",
                    response.url
                )

                print(
                    "CONTENT-TYPE:",
                    response.headers.get(
                        "Content-Type"
                    )
                )

                print(
                    "RESPONSE LENGTH:",
                    len(response.text)
                )

                print(
                    "COOKIES:",
                    session.cookies.get_dict()
                )

                print(
                    "RESPONSE TYPE:",
                    "JSON"
                    if response.text.lstrip().startswith("{")
                    else "NON-JSON"
                )

                print("=" * 70)
                print()

            return response.text

        except requests.RequestException as ex:

            last_exception = ex

            print(
                f"[HTTP] Page {page} request failed "
                f"(attempt {attempt}/"
                f"{MAX_RETRIES_PER_PAGE}): {ex}"
            )

            if attempt < MAX_RETRIES_PER_PAGE:

                time.sleep(
                    RETRY_BACKOFF_S * attempt
                )

    raise last_exception


# ============================================================
# CREATE ALL-BIDS SESSION
# ============================================================

def _make_all_bids_session() -> requests.Session:
    """
    Creates a requests session and loads GeM's all-bids page.
    The initial GET establishes the GeM session and obtains the
    csrf_gem_cookie required by the all-bids-data request.
    """
    session = requests.Session()

    session.headers.update({
        "User-Agent": USER_AGENT,
        "Accept": "application/json, text/javascript, */*; q=0.01",
        "X-Requested-With": "XMLHttpRequest",
        "Referer": ALL_BIDS_PAGE_URL,
        "Origin": BASE_URL,
        "Connection": "keep-alive",
    })

    print("[HTTP] Opening GeM all-bids page...")

    response = session.get(
        ALL_BIDS_PAGE_URL,
        timeout=REQUEST_TIMEOUT_S
    )

    response.raise_for_status()

    if "csrf_gem_cookie" not in session.cookies.get_dict():
        raise RuntimeError(
            "GeM did not set csrf_gem_cookie on all-bids page load."
        )

    return session


# ============================================================
# FETCH ONE ALL-BIDS PAGE
# ============================================================

def _fetch_all_bids_page(
    session: requests.Session,
    page: int
) -> str:
    """
    Calls GeM's /all-bids-data endpoint.
    """
    payload_obj = {
        "page": page,
        "param": {
            "searchBid": "",
            "searchType": "fullText"
        },
        "filter": {
            "bidStatusType": "ongoing_bids",
            "byType": "all",
            "highBidValue": "",
            "byEndDate": {
                "from": "",
                "to": ""
            },
            "sort": "Bid-End-Date-Oldest"
        }
    }

    csrf_token = session.cookies.get(
        "csrf_gem_cookie",
        ""
    )

    if not csrf_token:
        raise RuntimeError(
            "csrf_gem_cookie is missing from the session."
        )

    form_data = {
        "payload": json.dumps(
            payload_obj,
            separators=(",", ":")
        ),
        "csrf_bd_gem_nk": csrf_token,
    }

    headers = {
        "X-Requested-With": "XMLHttpRequest",
        "Referer": ALL_BIDS_PAGE_URL,
        "Origin": BASE_URL,
        "Accept": "application/json, text/javascript, */*; q=0.01",
        "Content-Type": (
            "application/x-www-form-urlencoded; "
            "charset=UTF-8"
        ),
    }

    last_exception: Exception = RuntimeError(
        "No request attempt was made."
    )

    for attempt in range(
        1,
        MAX_RETRIES_PER_PAGE + 1
    ):
        try:
            if VERBOSE:
                print(
                    f"[HTTP] Requesting all-bids page {page} "
                    f"(attempt {attempt}/"
                    f"{MAX_RETRIES_PER_PAGE})..."
                )

            response = session.post(
                ALL_BIDS_DATA_URL,
                data=form_data,
                headers=headers,
                timeout=REQUEST_TIMEOUT_S
            )

            response.raise_for_status()
            return response.text

        except requests.RequestException as ex:
            last_exception = ex

            print(
                f"[HTTP] All-bids page {page} request failed "
                f"(attempt {attempt}/"
                f"{MAX_RETRIES_PER_PAGE}): {ex}"
            )

            if attempt < MAX_RETRIES_PER_PAGE:
                time.sleep(
                    RETRY_BACKOFF_S * attempt
                )

    raise last_exception


# ============================================================
# ALL BIDS SCRAPER
# ============================================================

def get_all_bids_urls(
    total_pages: int = -1
) -> List[Dict[str, Any]]:
    """
    Fetches bids directly from GeM's All Bids listing page (/all-bids-data).
    """
    start = time.perf_counter()

    print()
    print(
        f"[Scraper] Searching ALL BIDS listing "
        f"({total_pages} pages requested) "
        f"via direct HTTP requests..."
    )

    try:
        session = _make_all_bids_session()
    except Exception as ex:
        print(
            f"[Scraper] Could not create GeM all-bids session: {ex}"
        )
        return []

    all_bids: Dict[str, Dict[str, Any]] = {}
    pages_fetched = 0
    page = 1

    while True:
        if total_pages > 0 and page > total_pages:
            break

        try:
            response_text = _fetch_all_bids_page(
                session,
                page
            )
        except requests.RequestException as ex:
            print(
                f"[Scraper] Stopped at all-bids page {page}: "
                f"request failed after {MAX_RETRIES_PER_PAGE} retries. Error: {ex}"
            )
            break
        except Exception as ex:
            print(
                f"[Scraper] Stopped at all-bids page {page}: {ex}"
            )
            break

        bids = _parse_cards(
            response_text,
            ministry=""
        )

        if not bids:
            print(
                f"[Scraper] Stopped at all-bids page {page}: "
                "no bid records found in response."
            )
            break

        for bid in bids:
            bid_number = bid.get("bid_number")
            if not bid_number:
                continue
            all_bids[bid_number] = bid

        pages_fetched = page

        if VERBOSE:
            if total_pages > 0:
                print(
                    f"[Scraper] All-Bids Page {page}/{total_pages}: "
                    f"{len(bids)} bids, "
                    f"{len(all_bids)} unique so far"
                )
            else:
                print(
                    f"[Scraper] All-Bids Page {page}: "
                    f"{len(bids)} bids, "
                    f"{len(all_bids)} unique so far"
                )
        elif page % 10 == 0:
            if total_pages > 0:
                print(
                    f"[Scraper] All-Bids Page {page}/{total_pages}, "
                    f"{len(all_bids)} unique bids so far..."
                )
            else:
                print(
                    f"[Scraper] All-Bids Page {page}, "
                    f"{len(all_bids)} unique bids so far..."
                )
        elif page <= 3:
            print(
                f"[Scraper] All-Bids Page {page}: "
                f"{len(bids)} bids, "
                f"{len(all_bids)} unique so far"
            )

        time.sleep(REQUEST_DELAY_S)
        page += 1

    elapsed = time.perf_counter() - start

    print()
    print(
        f"[Scraper] Done: "
        f"{len(all_bids)} unique bids "
        f"from {pages_fetched} all-bids page(s) "
        f"in {elapsed:.1f}s"
    )

    return list(all_bids.values())


# ============================================================
# MAIN SCRAPER
# ============================================================

def get_pdf_urls(
    total_pages: int = -1,
    ministry: str = "Ministry of Defence",
    all_bids: bool = False
) -> List[Dict[str, Any]]:
    """
    Fetches bids from GeM using direct HTTP requests.

    When all_bids is True, it scrapes directly from the All Bids listing page.
    Otherwise, it scrapes for the specified ministry.

    total_pages:
        -1 = automatically walk every available page
        >0 = scrape exactly that many pages (useful for testing)
    """
    if all_bids:
        return get_all_bids_urls(total_pages)

    start = time.perf_counter()

    print()

    print(
        f"[Scraper] Searching '{ministry}' bids "
        f"({total_pages} pages requested) "
        f"via direct HTTP requests..."
    )

    # --------------------------------------------------------
    # CREATE SESSION
    # --------------------------------------------------------

    try:

        session = _make_session()

    except Exception as ex:

        print(
            f"[Scraper] Could not create GeM session: {ex}"
        )

        return []

    # --------------------------------------------------------
    # UNIQUE BIDS
    # --------------------------------------------------------

    all_bids: Dict[str, Dict[str, Any]] = {}

    pages_fetched = 0

    # --------------------------------------------------------
    # FETCH PAGES
    # --------------------------------------------------------

    page = 1

    while True:

        # Limited/test mode.
        if total_pages > 0 and page > total_pages:
            break

        try:

            response_text = _fetch_page_html(
                session,
                ministry,
                page
            )

        except requests.RequestException as ex:

            print(
                f"[Scraper] Stopped at page {page}: "
                f"request failed after "
                f"{MAX_RETRIES_PER_PAGE} retries."
            )

            print(
                "[Scraper] Error:",
                ex
            )

            break

        except Exception as ex:

            print(
                f"[Scraper] Stopped at page {page}: "
                f"{ex}"
            )

            break

        # ----------------------------------------------------
        # PARSE JSON
        # ----------------------------------------------------

        bids = _parse_cards(
            response_text,
            ministry
        )

        # ----------------------------------------------------
        # NO RESULTS
        # ----------------------------------------------------

        if not bids:

            print(
                f"[Scraper] Stopped at page {page}: "
                "no bid records found in response."
            )

            break

        # ----------------------------------------------------
        # STORE UNIQUE BIDS
        # ----------------------------------------------------

        for bid in bids:

            bid_number = bid.get(
                "bid_number"
            )

            if not bid_number:
                continue

            all_bids[bid_number] = bid

        pages_fetched = page

        # ----------------------------------------------------
        # PROGRESS
        # ----------------------------------------------------

        if VERBOSE:

            if total_pages > 0:
                print(
                    f"[Scraper] Page {page}/{total_pages}: "
                    f"{len(bids)} bids, "
                    f"{len(all_bids)} unique so far"
                )
            else:
                print(
                    f"[Scraper] Page {page}: "
                    f"{len(bids)} bids, "
                    f"{len(all_bids)} unique so far"
                )

        elif page % 10 == 0:

            if total_pages > 0:
                print(
                    f"[Scraper] Page {page}/{total_pages}, "
                    f"{len(all_bids)} unique bids so far..."
                )
            else:
                print(
                    f"[Scraper] Page {page}, "
                    f"{len(all_bids)} unique bids so far..."
                )

        elif page <= 3:

            print(
                f"[Scraper] Page {page}: "
                f"{len(bids)} bids, "
                f"{len(all_bids)} unique so far"
            )

        # ----------------------------------------------------
        # DELAY + NEXT PAGE
        # ----------------------------------------------------

        time.sleep(REQUEST_DELAY_S)
        page += 1

    # --------------------------------------------------------
    # FINISH
    # --------------------------------------------------------

    elapsed = (
        time.perf_counter()
        - start
    )

    print()

    print(
        f"[Scraper] Done: "
        f"{len(all_bids)} unique bids "
        f"from {pages_fetched} page(s) "
        f"in {elapsed:.1f}s"
    )

    return list(
        all_bids.values()
    )


# ============================================================
# TEST
# ============================================================
# ============================================================
# TEST
# ============================================================

if __name__ == "__main__":

    print()
    print("=" * 70)
    print("GeM HTTP SCRAPER TEST")
    print("=" * 70)

    # --------------------------------------------------------
    # Fetch ALL pages for the selected ministry.
    #
    # Use a positive number (for example 2) while testing.
    # Use -1 in normal operation to continue until GeM
    # returns no more bids.
    # --------------------------------------------------------

    bids = get_pdf_urls(
        total_pages=-1,
        ministry="Ministry of Defence"
    )

    print()
    print("=" * 70)
    print(
        f"TOTAL BIDS FOUND: {len(bids)}"
    )
    print("=" * 70)

    # --------------------------------------------------------
    # Display first 3 bids
    # --------------------------------------------------------

    for index, bid in enumerate(
        bids[:3],
        start=1
    ):

        print()
        print(
            f"--- BID {index} ---"
        )

        print(
            "Bid Number:",
            bid.get("bid_number")
        )

        print(
            "PDF URL:",
            bid.get("pdf_url")
        )

        print(
            "Item:",
            bid.get("item_name")
        )

        print(
            "Quantity:",
            bid.get("quantity")
        )

        print(
            "Ministry:",
            bid.get("ministry")
        )

        print(
            "Department:",
            bid.get("department")
        )

        print(
            "Start Date:",
            bid.get("start_date")
        )

        print(
            "End Date:",
            bid.get("end_date")
        )

        print(
            "Bid ID:",
            bid.get("bid_id")
        )

        print(
            "Category ID:",
            bid.get("category_id")
        )

        print(
            "Status:",
            bid.get("status")
        )

    print()
    print("=" * 70)
    print("SCRAPER TEST FINISHED")
    print("=" * 70)

    # ========================================================
    # PDF DOWNLOAD TEST
    # ========================================================

    print()
    print("=" * 70)
    print("PDF DOWNLOAD TEST")
    print("=" * 70)

    if bids:

        test_bid = bids[0]

        test_pdf_url = test_bid.get(
            "pdf_url",
            ""
        )

        print(
            "Bid Number:",
            test_bid.get("bid_number")
        )

        print(
            "PDF URL:",
            test_pdf_url
        )

        if not test_pdf_url:

            print(
                "PDF DOWNLOAD: FAILED"
            )

            print(
                "Reason: PDF URL is empty."
            )

        else:

            try:

                pdf_response = session.get(
                    test_pdf_url,
                    headers={
                        "User-Agent": USER_AGENT,
                        "Referer": ADVANCE_SEARCH_URL,
                        "Accept": "application/pdf,*/*",
                    },
                    timeout=60,
                    allow_redirects=True,
                )

                print(
                    "HTTP STATUS:",
                    pdf_response.status_code
                )

                print(
                    "FINAL URL:",
                    pdf_response.url
                )

                print(
                    "CONTENT-TYPE:",
                    pdf_response.headers.get(
                        "Content-Type"
                    )
                )

                print(
                    "CONTENT LENGTH:",
                    len(pdf_response.content)
                )

                print(
                    "PDF HEADER:",
                    pdf_response.content[:10]
                )

                if (
                    pdf_response.status_code == 200
                    and pdf_response.content[:4] == b"%PDF"
                ):

                    print(
                        "PDF DOWNLOAD: SUCCESS"
                    )

                    test_filename = (
                        "test_bid_7469542.pdf"
                    )

                    with open(
                        test_filename,
                        "wb"
                    ) as file:

                        file.write(
                            pdf_response.content
                        )

                    print(
                        "Saved:",
                        test_filename
                    )

                else:

                    print(
                        "PDF DOWNLOAD: FAILED"
                    )

                    print(
                        "Response preview:",
                        pdf_response.text[:500]
                    )

            except requests.RequestException as ex:

                print(
                    "PDF DOWNLOAD ERROR:",
                    ex
                )

    else:

        print(
            "[PDF TEST] No bids available to test."
        )

    print("=" * 70)
    print("PDF TEST FINISHED")
    print("=" * 70)
