import time

from cleaner import clean
from config import get_config

_VERBOSE = get_config().get("verbose", False)

STOP_HEADERS = [
    "Buyer Added Bid Specific Terms and Conditions",
    "Buyer Added Bid Specific ATC",
    "Buyer Added ATC",
    "General Terms and Conditions",
    "Special Terms and Conditions",
    "Special terms and conditions-Version",
    "Disclaimer",
    "Scope of Supply",
    "Service Level Agreement",
    "SLA",
    "ATC",
    "GTC"
]


def remove_terms(text):
    if not text:
        return ""

    lower_text = text.lower()
    stop_index = len(text)
    for header in STOP_HEADERS:
        pos = lower_text.find(header.lower())
        if pos != -1:
            stop_index = min(stop_index, pos)

    return text[:stop_index]


def extract_pdf(doc):
    """
    doc: an already-open fitz.Document (from parser.py).
    Returns: { "Pages": [...], "Tables": [...], "Words": [...], "RawText": "" }
    """
    data = {"Pages": [], "Tables": [], "Words": [], "RawText": ""}
    total_pages = doc.page_count
    start_time = time.perf_counter()

    for page_index in range(total_pages):
        page_number = page_index + 1
        page = doc[page_index]

        text = page.get_text("text") or ""
        text = clean(text)
        text = remove_terms(text)

        data["Pages"].append({"Page": page_number, "Text": text})
        data["RawText"] += text + "\n"

        try:
            found = page.find_tables()
            for tbl in found.tables:
                extracted = tbl.extract()
                clean_table = []
                for row in extracted:
                    if not row:
                        continue
                    clean_row = [clean(cell) for cell in row]
                    clean_table.append(clean_row)
                if clean_table:
                    data["Tables"].append(clean_table)
        except Exception:
            # Table finding is best-effort; never let it fail the page.
            pass

        raw_words = page.get_text("words")
        for x0, y0, x1, y1, wtext, *_ in raw_words:
            wtext = clean(wtext)
            if not wtext:
                continue
            data["Words"].append({
                "x0": x0, "x1": x1, "top": y0, "bottom": y1, "text": wtext
            })

    if _VERBOSE:
        elapsed = time.perf_counter() - start_time
        print(
            f"[Extractor] {len(data['Pages'])} pages, "
            f"{len(data['Tables'])} tables, {len(data['Words'])} words "
            f"in {elapsed:.2f}s"
        )

    return data