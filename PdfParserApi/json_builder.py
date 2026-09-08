import re
from cleaner import clean

STOP_HEADERS = [
    "Buyer Added Bid Specific Terms and Conditions",
    "Buyer Added Bid Specific ATC",
    "Buyer Added ATC",
    "General Terms and Conditions",
    "Special Terms and Conditions",
    "Disclaimer",
    "ATC",
    "GTC"
]


def clean_key(key):
    key = clean(key)
    key = re.sub(r'^[^A-Za-z0-9]+', '', key)
    key = re.sub(r'\s+', ' ', key)
    return key.strip()


def clean_value(value):
    value = clean(value)
    value = re.sub(r"\s+", " ", value)

    if len(value) > 300:
        stop = value.find("Buyer Added")
        if stop != -1:
            value = value[:stop]
        stop = value.find("Terms and Conditions")
        if stop != -1:
            value = value[:stop]
        stop = value.find("Disclaimer")
        if stop != -1:
            value = value[:stop]

    return value.strip()


def is_valid_key(key):
    if not key:
        return False
    if len(key) < 2 or len(key) > 120:
        return False
    if key.isdigit():
        return False
    return True


def add(result, key, value):
    key = clean_key(key)
    value = clean_value(value)

    if not is_valid_key(key):
        return
    if not value:
        return
    if len(value) > 300:
        return

    if key in result:
        if result[key] == value:
            return

    result[key] = value


def merge_tables(result, tables):
    for table in tables:
        if not table:
            continue

        for row in table:
            if not row:
                continue

            row = [clean(str(cell)) for cell in row if cell]
            if not row:
                continue
            if len(row) < 2:
                continue

            if len(row) >= 5 and "consignee" in row[1].lower():
                continue

            if len(row) > 0 and row[0].isdigit():
                if len(row) > 1:
                    result["Consignee Name"] = row[1]
                if len(row) > 2 and any(c.isalpha() for c in row[2]):
                    result["Consignee Address"] = row[2]
                if len(row) > 3 and row[3].replace(",", "").isdigit():
                    result["Consignee Quantity"] = row[3]
                if len(row) > 4 and (row[4].replace(",", "").isdigit() or row[4].upper() == "N/A"):
                    result["Delivery Days"] = row[4]
                continue

            if len(row) == 3:
                header = row[0].lower()
                if "specification" in header:
                    continue
                result[row[1]] = row[2]
                continue

            if row[0].lower().startswith("product verification"):
                if len(row) > 1:
                    result["Product Verification"] = row[1]
                continue

            if row[0].lower() == "warranty":
                if len(row) > 1:
                    result["Warranty Details"] = row[1]
                continue

            if "operating system support" in row[0].lower():
                if len(row) > 1:
                    result["Operating System Supportability"] = row[1]
                continue

            if "quality assurance plan" in row[0].lower():
                if len(row) > 1:
                    result["Quality Assurance Plan"] = row[1]
                continue

            if "residual shelf life" in row[0].lower():
                if len(row) > 1:
                    result["Residual Shelf Life"] = row[1]
                continue

            if "number of dressings per pack" in row[0].lower():
                if len(row) > 1:
                    result["Packaging"] = row[1]
                continue

            if "medical device license" in row[0].lower():
                if len(row) > 1:
                    result["Medical Device License"] = row[1]
                continue

            i = 0
            while i + 1 < len(row):
                add(result, row[i], row[i + 1])
                i += 2


def merge_layout(result, records):
    for record in records:
        add(result, record["Key"], record["Value"])


def merge_text(result, raw_text):
    if not raw_text:
        return

    for line in raw_text.split("\n"):
        line = clean(line)
        if ":" not in line:
            continue
        key, value = line.split(":", 1)
        add(result, key, value)


def extract_important_fields(result, raw_text):
    patterns = {
        "Bid Number": r"GEM/\d{4}/B/\d+",
        "Bid Date": r"Dated[: ]*([0-9]{2}-[0-9]{2}-[0-9]{4})",
        "Bid End Date/Time": r"Bid End Date/Time[: ]*([0-9:\- ]+)",
        "Bid Opening Date/Time": r"Bid Opening Date/Time[: ]*([0-9:\- ]+)"
    }

    for field, pattern in patterns.items():
        match = re.search(pattern, raw_text, re.IGNORECASE)
        if not match:
            continue
        if match.lastindex:
            result[field] = match.group(1).strip()
        else:
            result[field] = match.group().strip()


def add_common_fields(result):
    aliases = {
        "Consignee Name": ["Consignee", "Consignee Name", "Consignee Reporting/Officer"],
        "Consignee Address": ["Delivery Address", "Address", "Consignee Address"],
        "Consignee Quantity": ["Quantity", "Qty", "Consignee Quantity"],
        "Material": ["Material", "Material Type"],
        "Surface": ["Surface", "Surface Finish", "Finish"],
        "Layers": ["Layer", "Layers"],
        "Warranty Text": ["Warranty", "Warranty Text", "Warranty Period"],
        "Service Requirement": ["Service Requirement", "Requirement"],
        "Service Inclusions": ["Service Inclusions", "Included Services"],
        "Training Module": ["Training", "Training Module"]
    }

    for new_key, keys in aliases.items():
        if new_key in result:
            continue
        for old_key, value in result.items():
            for keyword in keys:
                if keyword.lower() in old_key.lower():
                    result[new_key] = value
                    break
            if new_key in result:
                break


def build_json(extracted, records):
    result = {}
    merge_tables(result, extracted["Tables"])
    merge_layout(result, records)
    merge_text(result, extracted["RawText"])
    extract_important_fields(result, extracted["RawText"])
    add_common_fields(result)
    return result