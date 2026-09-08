"""
The single reusable pipeline: scrape the GeM bid list, then download and
parse every bid's PDF concurrently.

The bid-list stage now uses gem_scraper_http.py instead of the old
Playwright-based gem_scraper.py.

Concurrency model:

- Downloading is I/O-bound -> ThreadPoolExecutor
- PDF parsing is CPU-bound -> ProcessPoolExecutor
"""

import time
import asyncio

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
    ministry: str
) -> dict:
    """
    Full pipeline:

        1. Fetch bid list using HTTP scraper
        2. Get PDF URLs
        3. Download PDFs concurrently
        4. Parse PDFs concurrently
        5. Return results

    The scraper itself is executed in a normal thread because it
    performs blocking requests.
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
    # STEP 2 - DOWNLOAD + PARSE
    # ========================================================

    progress = _Progress(
        len(bids),
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
    # RUN ALL BIDS
    # ========================================================

    result = await asyncio.gather(
        *(run_one(bid) for bid in bids)
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
        f"total time "
        f"{format_duration(elapsed)}"
    )

    return {
        "Ministry": ministry,
        "Pages": pages,
        "TotalPdfFound": len(bids),
        "ParsedSuccessfully": parsed,
        "Failed": failed,
        "TimeTaken": format_duration(elapsed),
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