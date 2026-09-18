"""
The single reusable pipeline: scrape the GeM bid list, identify new/changed
bids, then download and parse only the required PDFs concurrently.

The bid-list stage uses gem_scraper_http.py.

Workflow:

    1. Scrape all GeM bid listings
    2. Compare each BidNumber with known bids from the database
    3. New BidNumber
           -> download PDF
           -> parse PDF
           -> return result
    4. Known BidNumber + same end date
           -> skip PDF download and parsing
    5. Known BidNumber + changed end date
           -> download PDF
           -> parse PDF
           -> return result
    6. Return processed results to C#

Concurrency model:

- Downloading is I/O-bound -> ThreadPoolExecutor
- PDF parsing is CPU-bound -> ProcessPoolExecutor
"""

import time
import asyncio

from datetime import datetime

from concurrent.futures import (
    ThreadPoolExecutor,
    ProcessPoolExecutor,
)

from config import get_config

# IMPORTANT:
# Use the new HTTP scraper.
from gem_scraper_http import get_pdf_urls

from online_parser import download_pdf
from parser import parse_pdf_online


_cfg = get_config()


# ============================================================
# THREAD / PROCESS POOLS
# ============================================================

download_pool = ThreadPoolExecutor(
    max_workers=_cfg["download_workers"],
    thread_name_prefix="download",
)


parse_pool = ProcessPoolExecutor(
    max_workers=_cfg["parse_workers"],
)


# ============================================================
# FORMAT DURATION
# ============================================================

def format_duration(seconds):
    minutes = int(seconds // 60)
    secs = seconds % 60

    if minutes:
        return f"{minutes}m {secs:.1f}s"

    return f"{secs:.1f}s"


# ============================================================
# DATE NORMALIZATION
# ============================================================

def _normalize_date(value):
    """
    Convert different date formats into a comparable datetime.

    C# sends database CardEndDate values as ISO-style datetime strings.

    GeM may return dates in different formats, so several common formats
    are supported.

    If the value cannot be parsed, the original normalized string is
    returned so that it can still be compared safely.
    """

    if value is None:
        return None

    value = str(value).strip()

    if not value:
        return None

    # --------------------------------------------------------
    # ISO / C# DateTime formats
    # --------------------------------------------------------

    try:
        normalized = value.replace("Z", "+00:00")

        dt = datetime.fromisoformat(normalized)

        # Convert timezone-aware datetime to naive UTC so that
        # equivalent timestamps can be compared consistently.
        if dt.tzinfo is not None:
            dt = dt.astimezone().replace(tzinfo=None)

        return dt

    except ValueError:
        pass

    # --------------------------------------------------------
    # Common GeM / Indian date formats
    # --------------------------------------------------------

    formats = [
        "%d-%m-%Y %H:%M:%S",
        "%d-%m-%Y %H:%M",
        "%d/%m/%Y %H:%M:%S",
        "%d/%m/%Y %H:%M",
        "%d-%m-%Y",
        "%d/%m/%Y",
        "%Y-%m-%d %H:%M:%S",
        "%Y-%m-%d %H:%M",
        "%Y-%m-%d",
    ]

    for fmt in formats:
        try:
            return datetime.strptime(value, fmt)
        except ValueError:
            continue

    # --------------------------------------------------------
    # Could not parse.
    #
    # Return the string itself so exact values can still be
    # compared.
    # --------------------------------------------------------

    return value


# ============================================================
# CLASSIFY BIDS
# ============================================================

def _classify_bids(
    bids,
    known_bid_info=None,
):
    """
    Classify scraped bids into:

        new
        unchanged
        changed

    known_bid_info is expected to look like:

        {
            "GEM/2026/B/1234567": "2026-09-10T18:00:00",
            ...
        }

    C# sends BidNumber -> CardEndDate directly, not a nested object.

    Rules:

        1. BidNumber not present in known_bid_info
           -> NEW

        2. BidNumber exists and end date is unchanged
           -> UNCHANGED

        3. BidNumber exists and end date changed
           -> CHANGED

    If an existing bid has no known end date, it is treated as CHANGED
    so that we safely re-download and re-parse the PDF.
    """

    if not known_bid_info:
        known_bid_info = {}

    new_bids = []
    changed_bids = []
    unchanged_bids = []

    for bid in bids:

        bid_number = bid.get("bid_number")

        if bid_number and "/B/" not in str(bid_number):
            bid["_change_type"] = "skipped_reverse_auction"
            unchanged_bids.append(bid)
            continue

        # ----------------------------------------------------
        # Missing BidNumber
        #
        # Such a bid cannot be reliably compared with the DB.
        # Treat it as new so it is not silently ignored.
        # ----------------------------------------------------

        if not bid_number:

            bid["_change_type"] = "new"
            new_bids.append(bid)

            continue

        # ----------------------------------------------------
        # NEW BID
        # ----------------------------------------------------

        if bid_number not in known_bid_info:

            bid["_change_type"] = "new"
            new_bids.append(bid)

            continue

        # ----------------------------------------------------
        # EXISTING BID
        # ----------------------------------------------------

        # C# sends the existing CardEndDate directly as the value:
        #
        #     "GEM/2026/B/1234567": "2026-09-10T18:00:00"
        #
        # Do not expect a nested {"EndDate": ...} object here.

        known_end_date = known_bid_info.get(bid_number)
        current_end_date = bid.get("end_date")

        normalized_known = _normalize_date(
            known_end_date
        )

        normalized_current = _normalize_date(
            current_end_date
        )

        # ----------------------------------------------------
        # Existing bid but no known end date
        #
        # Safest option is to re-process it.
        # ----------------------------------------------------

        if normalized_known is None:

            bid["_change_type"] = "changed"
            changed_bids.append(bid)

            continue

        # ----------------------------------------------------
        # END DATE CHANGED
        # ----------------------------------------------------

        if normalized_known != normalized_current:

            bid["_change_type"] = "changed"
            changed_bids.append(bid)

            continue

        # ----------------------------------------------------
        # UNCHANGED
        # ----------------------------------------------------

        bid["_change_type"] = "unchanged"
        unchanged_bids.append(bid)

    return (
        new_bids,
        changed_bids,
        unchanged_bids,
    )


# ============================================================
# PROGRESS
# ============================================================

class _Progress:
    """One updating console line instead of a print per PDF."""

    def __init__(self, total, label):
        self.total = total
        self.done = 0
        self.failed = 0
        self.label = label
        self._last_len = 0

    def tick(self, ok=True):
        self.done += 1

        if not ok:
            self.failed += 1

        pct = (
            self.done / self.total * 100
            if self.total
            else 100
        )

        line = (
            f"\r{self.label}: "
            f"{self.done}/{self.total} "
            f"({pct:.0f}%) | "
            f"failed: {self.failed}"
        )

        padding = " " * max(
            0,
            self._last_len - len(line)
        )

        print(
            line + padding,
            end="",
            flush=True,
        )

        self._last_len = len(line)

    def finish(self):
        print()


# ============================================================
# MAIN SCRAPE + PARSE PIPELINE
# ============================================================

async def run_scrape_pipeline(
    pages: int,
    ministry: str,
    known_bid_info: dict | None = None,
) -> dict:
    """
    Full pipeline:

        1. Fetch all bid listings using HTTP scraper
        2. Compare scraped bids with known database bids
        3. Skip unchanged bids
        4. Download PDFs for new/changed bids
        5. Parse PDFs concurrently
        6. Return processed results

    The scraper itself is executed in a normal thread because it
    performs blocking requests.

    Parameters
    ----------
    pages:
        Number of GeM pages to scan.

    ministry:
        Ministry filter used by the GeM scraper.

    known_bid_info:
        Dictionary received from C# containing known BidNumbers
        and their existing CardEndDate values.
    """

    start = time.perf_counter()

    loop = asyncio.get_running_loop()

    print()
    print(
        f"[Pipeline] Starting scrape: "
        f"{ministry}, {pages} pages..."
    )

    # ========================================================
    # STEP 1 - GET BID LIST
    # ========================================================

    try:

        bids = await loop.run_in_executor(
            None,
            get_pdf_urls,
            pages,
            ministry,
        )

    except Exception as ex:

        print(
            f"[Pipeline] Scraper failed: {ex}"
        )

        return {
            "Ministry": ministry,
            "Pages": pages,
            "TotalPdfFound": 0,
            "ProcessedBids": 0,
            "NewBids": 0,
            "ChangedBids": 0,
            "UnchangedBids": 0,
            "ParsedSuccessfully": 0,
            "Failed": 0,
            "TimeTaken": format_duration(
                time.perf_counter() - start
            ),
            "Result": [],
            "Error": str(ex),
        }

    # ========================================================
    # NO BIDS
    # ========================================================

    if not bids:

        print(
            "[Pipeline] No bids found - "
            "nothing to download or parse."
        )

        return {
            "Ministry": ministry,
            "Pages": pages,
            "TotalPdfFound": 0,
            "ProcessedBids": 0,
            "NewBids": 0,
            "ChangedBids": 0,
            "UnchangedBids": 0,
            "ParsedSuccessfully": 0,
            "Failed": 0,
            "TimeTaken": format_duration(
                time.perf_counter() - start
            ),
            "Result": [],
        }

    print(
        f"[Pipeline] Found {len(bids)} bids."
    )

    # ========================================================
    # STEP 2 - CLASSIFY BIDS
    # ========================================================

    (
        new_bids,
        changed_bids,
        unchanged_bids,
    ) = _classify_bids(
        bids,
        known_bid_info,
    )

    print(
        f"[Pipeline] New: {len(new_bids)}, "
        f"Changed: {len(changed_bids)}, "
        f"Unchanged: {len(unchanged_bids)}"
    )

    # --------------------------------------------------------
    # Only NEW and CHANGED bids need PDF processing.
    # --------------------------------------------------------

    bids_to_process = (
        new_bids +
        changed_bids
    )

    # ========================================================
    # NOTHING TO PROCESS
    # ========================================================

    if not bids_to_process:

        elapsed = (
            time.perf_counter()
            - start
        )

        print(
            "[Pipeline] All bids are unchanged - "
            "nothing to download or parse."
        )

        print(
            f"[Pipeline] Total time "
            f"{format_duration(elapsed)}"
        )

        return {
            "Ministry": ministry,
            "Pages": pages,
            "TotalPdfFound": len(bids),
            "ProcessedBids": 0,
            "NewBids": len(new_bids),
            "ChangedBids": len(changed_bids),
            "UnchangedBids": len(unchanged_bids),
            "ParsedSuccessfully": 0,
            "Failed": 0,
            "TimeTaken": format_duration(elapsed),
            "Result": [],
        }

    # ========================================================
    # STEP 3 - DOWNLOAD + PARSE
    # ========================================================

    progress = _Progress(
        len(bids_to_process),
        "[Pipeline] Downloading + parsing"
    )

    async def run_one(bid):

        pdf_url = bid.get("pdf_url")

        # ----------------------------------------------------
        # Missing PDF URL
        # ----------------------------------------------------

        if not pdf_url:

            progress.tick(ok=False)

            return {
                **bid,
                "Error": "Missing pdf_url",
            }

        # ----------------------------------------------------
        # DOWNLOAD
        # ----------------------------------------------------

        try:

            content = await loop.run_in_executor(
                download_pool,
                download_pdf,
                pdf_url,
            )

        except Exception as ex:

            progress.tick(ok=False)

            return {
                **bid,
                "Error": f"Download failed: {ex}",
            }

        # ----------------------------------------------------
        # PARSE
        # ----------------------------------------------------

        try:

            data = await loop.run_in_executor(
                parse_pool,
                parse_pdf_online,
                content,
            )

            progress.tick(ok=True)

            return {
                **bid,
                "Data": data,
            }

        except Exception as ex:

            progress.tick(ok=False)

            return {
                **bid,
                "Error": f"Parse failed: {ex}",
            }

    # ========================================================
    # RUN ALL NEW + CHANGED BIDS
    # ========================================================

    result = await asyncio.gather(
        *(run_one(bid) for bid in bids_to_process)
    )

    progress.finish()

    # ========================================================
    # SUMMARY
    # ========================================================

    parsed = sum(
        1
        for item in result
        if "Error" not in item
    )

    failed = len(result) - parsed

    elapsed = (
        time.perf_counter()
        - start
    )

    print(
        f"[Pipeline] {parsed} parsed, "
        f"{failed} failed, "
        f"skipped {len(unchanged_bids)} unchanged, "
        f"total time "
        f"{format_duration(elapsed)}"
    )

    # ========================================================
    # FAILED BID DETAILS
    # ========================================================

    if failed > 0:

        print()
        print(
            "[Pipeline] FAILED BID DETAILS"
        )
        print(
            "----------------------------------------"
        )

        for item in result:

            if "Error" not in item:
                continue

            bid_number = (
                item.get("bid_number")
                or "(unknown)"
            )

            pdf_url = (
                item.get("pdf_url")
                or "(missing)"
            )

            error = (
                item.get("Error")
                or "(unknown error)"
            )

            print(
                f"Bid Number : {bid_number}"
            )

            print(
                f"PDF URL    : {pdf_url}"
            )

            print(
                f"Error      : {error}"
            )

            print(
                "----------------------------------------"
            )

    # ========================================================
    # FINAL RESPONSE
    # ========================================================

    return {
        "Ministry": ministry,
        "Pages": pages,

        # All bids discovered from GeM.
        "TotalPdfFound": len(bids),

        # Bids for which PDF processing was attempted.
        "ProcessedBids": len(bids_to_process),

        # Classification counts.
        "NewBids": len(new_bids),
        "ChangedBids": len(changed_bids),
        "UnchangedBids": len(unchanged_bids),

        # PDF processing results.
        "ParsedSuccessfully": parsed,
        "Failed": failed,

        "TimeTaken": format_duration(elapsed),

        # IMPORTANT:
        # Only NEW and CHANGED bids are returned here.
        # UNCHANGED bids are intentionally omitted because their
        # PDFs were not downloaded or parsed.
        "Result": result,
    }


# ============================================================
# SHUTDOWN
# ============================================================

def shutdown_pools():

    parse_pool.shutdown(
        wait=False,
        cancel_futures=True,
    )

    download_pool.shutdown(
        wait=False,
        cancel_futures=True,
    )
