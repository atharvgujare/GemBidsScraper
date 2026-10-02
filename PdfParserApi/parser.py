import pymupdf

from extractor import extract_pdf
from layout_builder import build_layout, detect_key_values, merge_multiline_records
from json_builder import build_json
from config import get_config

_VERBOSE = get_config().get("verbose", False)


def build_result(doc):
    extracted = extract_pdf(doc)
    layout = build_layout(extracted["Words"])
    records = detect_key_values(layout)
    records = merge_multiline_records(records)
    result = build_json(extracted, records)

    try:
        from consignee_extractor import extract_consignee_location_from_doc
        consignee_info = extract_consignee_location_from_doc(doc)
        if consignee_info.get("location") and not result.get("Location"):
            result["Location"] = consignee_info.get("location")
        if consignee_info.get("consignee_name") and not result.get("Consignee Name"):
            result["Consignee Name"] = consignee_info.get("consignee_name")
        if consignee_info.get("consignee_address") and not result.get("Consignee Address"):
            result["Consignee Address"] = consignee_info.get("consignee_address")
    except Exception:
        pass

    if _VERBOSE:
        print(f"[Parser] Extracted {len(result)} JSON fields")

    return result


def parse_pdf(file_path):
    doc = pymupdf.open(file_path)
    try:
        return build_result(doc)
    finally:
        doc.close()


def parse_pdf_online(pdf_bytes):
    doc = pymupdf.open(stream=pdf_bytes, filetype="pdf")
    try:
        return build_result(doc)
    finally:
        doc.close()