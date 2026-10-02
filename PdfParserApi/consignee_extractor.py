import fitz
import re
from typing import Optional, Dict, Any
from cleaner import clean
from city_resolver import resolve_location, resolve_city_from_text


def extract_consignee_location_from_doc(doc: fitz.Document) -> Dict[str, Any]:
    """
    Specifically targets the 'Consignees/Reporting Officer and Quantity' section in a GeM PDF
    and extracts the clean standardized City name for the Location column.
    """
    consignee_name = None
    consignee_address = None
    resolved_city = None

    total_pages = doc.page_count

    # 1. Search page-by-page specifically for the Consignee table
    for page_index in range(total_pages):
        page = doc[page_index]
        page_text = page.get_text() or ""

        # Quick check if this page could contain the consignee table
        if not any(kw in page_text for kw in ["Consignee", "Consignees", "Reporting/Officer", "परेषिती", "पता/Address"]):
            continue

        try:
            found_tables = page.find_tables()
            for tbl in found_tables.tables:
                extracted_rows = tbl.extract()
                if not extracted_rows:
                    continue

                # Check if this table is the Consignees table
                is_consignee_table = False
                addr_col = 2
                name_col = 1

                for row in extracted_rows:
                    row_str = " ".join([str(c).lower() for c in row if c])
                    if any(kw in row_str for kw in ["consignee", "reporting/officer", "परेषिती"]):
                        is_consignee_table = True
                        for idx, cell in enumerate(row):
                            if not cell:
                                continue
                            c_low = str(cell).lower()
                            if "address" in c_low or "पता" in c_low:
                                addr_col = idx
                            elif "reporting" in c_low or "officer" in c_low:
                                name_col = idx
                        break

                if is_consignee_table:
                    for row in extracted_rows:
                        if not row:
                            continue
                        first_cell = clean(str(row[0])) if len(row) > 0 and row[0] else ""
                        if first_cell == "1" or (first_cell.isdigit() and int(first_cell) <= 10):
                            if len(row) > name_col and row[name_col]:
                                n_val = clean(str(row[name_col]))
                                if n_val and not n_val.isdigit():
                                    consignee_name = n_val

                            if len(row) > addr_col and row[addr_col]:
                                a_val = clean(str(row[addr_col]))
                                if a_val and not any(kw in a_val.lower() for kw in ["delivery schedule", "prarambh", "number of days"]):
                                    consignee_address = a_val
                                    break
                    
                    if consignee_address or consignee_name:
                        break
        except Exception:
            pass

        if consignee_address:
            break

    # 2. Resolve city from the extracted consignee address & name
    if consignee_address or consignee_name:
        resolved_city = resolve_location(
            consignee_address=consignee_address,
            consignee_name=consignee_name
        )

    # 3. Fallback 1: Service / Scope of Work / Transport details
    if not resolved_city:
        for page_index in range(total_pages):
            p_text = doc[page_index].get_text() or ""
            for marker in ["Event premises", "Place of Delivery", "Place of Work", "Location of event", "Drop Location Zipcode", "Start Location Zipcode"]:
                if marker in p_text:
                    m = re.search(re.escape(marker) + r"[\s:/\-]+([^\n\r]+)", p_text, re.IGNORECASE)
                    if m:
                        cand = resolve_city_from_text(m.group(1))
                        if cand:
                            resolved_city = cand
                            break
            if resolved_city:
                break

    # 4. Fallback 2: Check Beneficiary / EMD section across early pages (present in ~95% of tenders)
    if not resolved_city:
        for page_index in range(min(8, total_pages)):
            p_text = doc[page_index].get_text() or ""
            if "Beneficiary" in p_text or "लाभार्थी" in p_text:
                b_match = re.search(r"(?:Beneficiary|लाभार्थी)[\s\S]{1,500}", p_text, re.IGNORECASE)
                b_snippet = b_match.group(0) if b_match else p_text
                cand = resolve_city_from_text(b_snippet)
                if cand:
                    resolved_city = cand
                    break

    # 5. Fallback 3: Page 1 Office Name
    if not resolved_city and total_pages > 0:
        p1_text = doc[0].get_text() or ""
        off_match = re.search(r"(?:Office Name|कार्यालय का नाम)[\s:/\-]+([^\n\r]+)", p1_text, re.IGNORECASE)
        if off_match:
            cand = resolve_city_from_text(off_match.group(1))
            if cand:
                resolved_city = cand

    # 6. Fallback 4: Buyer Added ATC terms (e.g. delivery instructions)
    if not resolved_city:
        for page_index in range(max(0, total_pages - 4), total_pages):
            p_text = doc[page_index].get_text() or ""
            if "Buyer Added" in p_text or "ATC" in p_text:
                cand = resolve_city_from_text(p_text)
                if cand:
                    resolved_city = cand
                    break

    return {
        "location": resolved_city,
        "consignee_name": consignee_name,
        "consignee_address": consignee_address
    }


def extract_consignee_location_from_bytes(pdf_bytes: bytes) -> Dict[str, Any]:
    doc = fitz.open(stream=pdf_bytes, filetype="pdf")
    try:
        return extract_consignee_location_from_doc(doc)
    finally:
        doc.close()
