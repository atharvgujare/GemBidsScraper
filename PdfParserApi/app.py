import os
import asyncio
import tempfile

from typing import Optional

from fastapi import (
    FastAPI,
    UploadFile,
    File,
)

from pydantic import (
    BaseModel,
    Field,
    ConfigDict,
)

from config import (
    get_config,
    update_schedule_times,
)

from parser import parse_pdf

from pipeline import (
    run_scrape_pipeline,
    shutdown_pools,
    parse_pool,
)

from scheduler import (
    start_scheduler,
    stop_scheduler,
)


# ============================================================
# APP
# ============================================================

app = FastAPI()

cfg = get_config()


# ============================================================
# REQUEST MODELS
# ============================================================

class OnlinePdfRequest(BaseModel):

    PdfUrls: list[str] = Field(
        ...,
        alias="pdfUrls",
    )

    model_config = ConfigDict(
        populate_by_name=True
    )


class PageRequest(BaseModel):

    Pages: int = Field(
        ...,
        alias="pages",
    )

    Ministry: Optional[str] = Field(
        None,
        alias="ministry",
    )

    # ========================================================
    # IMPORTANT:
    #
    # C# sends:
    #
    # "knownBidInfo": {
    #     "GEM/2026/B/7998407": "2026-09-14T14:30:00"
    # }
    #
    # Therefore the value is a string (or null), NOT:
    #
    # {
    #     "EndDate": "..."
    # }
    #
    # ========================================================

    KnownBidInfo: dict[str, Optional[str]] = Field(
        default_factory=dict,
        alias="knownBidInfo",
    )

    model_config = ConfigDict(
        populate_by_name=True
    )


class ScheduleRequest(BaseModel):

    Times: list[str] = Field(
        ...,
        alias="times",
    )

    model_config = ConfigDict(
        populate_by_name=True
    )


# ============================================================
# STARTUP
# ============================================================

@app.on_event("startup")
def _startup():

    start_scheduler()


# ============================================================
# SHUTDOWN
# ============================================================

@app.on_event("shutdown")
def _shutdown():

    stop_scheduler()

    shutdown_pools()


# ============================================================
# HOME
# ============================================================

@app.get("/")
def home():

    return {
        "message": "Python API is running",
        "config": get_config(),
    }


# ============================================================
# EXTRACT UPLOADED PDF
# ============================================================

@app.post("/extract")
async def extract(
    file: UploadFile = File(...)
):

    suffix = os.path.splitext(
        file.filename
    )[1]

    with tempfile.NamedTemporaryFile(
        delete=False,
        suffix=suffix
    ) as temp:

        temp.write(
            await file.read()
        )

        temp_path = temp.name

    try:

        loop = asyncio.get_running_loop()

        result = await loop.run_in_executor(
            parse_pool,
            parse_pdf,
            temp_path,
        )

        return result

    finally:

        if os.path.exists(temp_path):

            os.remove(temp_path)


# ============================================================
# EXTRACT ALL LOCAL PDFs
# ============================================================

@app.get("/extract-all")
async def extract_all():

    pdf_folder = cfg["pdf_folder"]

    if not os.path.exists(pdf_folder):

        return {
            "error": "PDF folder not found",
            "folder": pdf_folder,
        }

    pdf_files = [
        file
        for file in os.listdir(pdf_folder)
        if file.lower().endswith(".pdf")
    ]

    loop = asyncio.get_running_loop()

    async def run_one(file):

        full_path = os.path.join(
            pdf_folder,
            file,
        )

        try:

            data = await loop.run_in_executor(
                parse_pool,
                parse_pdf,
                full_path,
            )

            return {
                "FileName": file,
                "Data": data,
            }

        except Exception as ex:

            return {
                "FileName": file,
                "Error": str(ex),
            }

    result = await asyncio.gather(
        *(run_one(file) for file in pdf_files)
    )

    return {
        "TotalFiles": len(pdf_files),
        "ParsedFiles": len(result),
        "Result": result,
    }


# ============================================================
# EXTRACT ONLINE PDF URLS
# ============================================================

@app.post("/extract-online")
async def extract_online(
    request: OnlinePdfRequest
):

    from online_parser import download_pdf
    from parser import parse_pdf_online
    from pipeline import download_pool

    loop = asyncio.get_running_loop()

    async def run_one(url):

        try:

            content = await loop.run_in_executor(
                download_pool,
                download_pdf,
                url,
            )

        except Exception as ex:

            return {
                "PdfUrl": url,
                "Error": str(ex),
            }

        try:

            data = await loop.run_in_executor(
                parse_pool,
                parse_pdf_online,
                content,
            )

            return {
                "PdfUrl": url,
                "Data": data,
            }

        except Exception as ex:

            return {
                "PdfUrl": url,
                "Error": str(ex),
            }

    result = await asyncio.gather(
        *(run_one(url) for url in request.PdfUrls)
    )

    return {
        "TotalFiles": len(request.PdfUrls),
        "ParsedFiles": len(result),
        "Result": result,
    }


# ============================================================
# GET PDF URL LIST
# ============================================================

@app.post("/pdf-urls")
async def pdf_urls(
    request: PageRequest
):
    """
    Returns bid list only.

    IMPORTANT:
    Uses the new HTTP GeM scraper.
    """

    # NEW HTTP SCRAPER
    from gem_scraper_http import get_pdf_urls

    import time

    loop = asyncio.get_running_loop()

    ministry = (
        request.Ministry
        or get_config()["ministry"]
    )

    start = time.perf_counter()

    bids = await loop.run_in_executor(
        None,
        get_pdf_urls,
        request.Pages,
        ministry,
    )

    return {
        "Ministry": ministry,
        "TotalBids": len(bids),
        "Bids": bids,
        "TimeTaken": (
            f"{time.perf_counter() - start:.1f}s"
        ),
    }


# ============================================================
# MAIN SCRAPE + PARSE ENDPOINT
# ============================================================

@app.post("/extract-online-pages")
async def extract_online_pages(
    request: PageRequest
):

    print("==========================================")
    print("C# REQUEST RECEIVED BY PYTHON")
    print("==========================================")

    """
    Main endpoint.

    Flow:

        C# known bid information
            ↓
        HTTP GeM API
            ↓
        Bid list
            ↓
        Compare known bids
            ↓
        NEW / CHANGED / UNCHANGED
            ↓
        Download PDFs for NEW + CHANGED
            ↓
        PyMuPDF
            ↓
        JSON
    """

    ministry = (
        request.Ministry
        or get_config()["ministry"]
    )

    return await run_scrape_pipeline(
        request.Pages,
        ministry,
        request.KnownBidInfo,
    )


# ============================================================
# GET SCHEDULE
# ============================================================

@app.get("/schedule")
def get_schedule():

    live_cfg = get_config()

    return {
        "schedule_enabled":
            live_cfg["schedule_enabled"],

        "schedule_times":
            live_cfg["schedule_times"],

        "pages_per_run":
            live_cfg["pages_per_run"],

        "ministry":
            live_cfg["ministry"],

        "note":
            "Restart the app after changing this "
            "for it to take effect.",
    }


# ============================================================
# UPDATE SCHEDULE
# ============================================================

@app.post("/schedule")
def set_schedule(
    request: ScheduleRequest
):

    update_schedule_times(
        request.Times
    )

    return {
        "message":
            "Schedule updated. Restart the app "
            "for it to take effect.",

        "schedule_times":
            request.Times,
    }


# ============================================================
# RUN APPLICATION
# ============================================================

if __name__ == "__main__":

    import uvicorn

    uvicorn.run(
        app,
        host="0.0.0.0",
        port=8000,
    )