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

CANONICAL_KEY_PATTERNS = [
    ("Bid End Date/Time", ["bid end date/time", "bid end date"]),
    ("Bid Opening Date/Time", ["bid opening date/time", "bid opening date"]),
    ("Bid Offer Validity (From End Date)", ["bid offer validity"]),
    ("Ministry/State Name", ["ministry/state name", "ministry name"]),
    ("Department Name", ["department name"]),
    ("Organisation Name", ["organisation name", "organization name"]),
    ("Office Name", ["office name"]),
    ("Contact details of Grievance redressal", ["contact details of grievance"]),
    ("Total Quantity", ["total quantity"]),
    ("Item Category", ["item category"]),
    ("Primary product category", ["primary product category"]),
    ("GeMARPTS / Searched Strings used in GeMARPTS", ["searched strings used in gemarpts"]),
    ("GeMARPTS / Searched Result generated in GeMARPTS", ["searched result generated in gemarpts"]),
    ("Relevant Categories selected for notification", ["relevant categories selected for notification"]),
    ("Document required from seller", ["document required from seller"]),
    ("Minimum number of bids required to disable automatic bid extension", ["minimum number of bids required"]),
    ("Number of days for which Bid would be auto-extended", ["number of days for which bid would be auto"]),
    ("Number of Auto Extension count", ["number of auto extension count"]),
    ("Bid to RA enabled", ["bid to ra enabled"]),
    ("RA Qualification Rule", ["ra qualification rule"]),
    ("ITC available to buyer", ["itc available to buyer"]),
    ("Type of Bid", ["type of bid"]),
    ("Time allowed for Technical Clarifications during technical evaluation", ["time allowed for technical"]),
    ("Inspection Required", ["inspection required"]),
    ("Evaluation Method", ["evaluation method"]),
    ("Estimated Bid Value in INR", ["estimated bid value"]),
    ("EMD Amount (In INR)", ["emd amount"]),
    ("ePBG Percentage(%)", ["epbg percentage"]),
    ("Duration of ePBG required (Months).", ["duration of epbg required"]),
    ("MSE Purchase Preference", ["mse purchase preference"]),
    ("MII Purchase Preference", ["mii purchase preference"]),
    ("Purchase Preference to MII sellers availabele upto price within L1+X%", ["purchase preference to mii sellers"]),
    ("Purchase Preference to MSE OEMs/ Service Provider available upto price within L1+X%", ["purchase preference to mse oems"]),
    ("Percentage of Bid quantity/amount for MSE OEMs/ Service Provider Purchase preference", ["percentage of bid quantity/amount for mse"]),
    ("Maximum Percentage of Bid quantity for MII purchase preference", ["maximum percentage of bid quantity for mii"]),
    ("Arbitration Clause", ["arbitration clause"]),
    ("Mediation Clause", ["mediation clause"]),
    ("Advisory Bank", ["advisory bank"]),
    ("Buyer Specification Document", ["buyer specification document"]),
    ("BOQ Detail Document", ["boq detail document"]),
    ("Consignee Name", ["consignee name", "consignee reporting/officer"]),
    ("Consignee Address", ["consignee address", "delivery address"]),
    ("Consignee Quantity", ["consignee quantity"]),
]


def normalize_for_match(value):
    value = value.lower()
    value = re.sub(r"[^a-z0-9/%+ ]+", " ", value)
    value = re.sub(r"\s+", " ", value)
    return value.strip()


def canonical_key(key):
    normalized = normalize_for_match(key)

    # Many noisy keys look like "T X /Evaluation Method". Prefer the
    # meaningful suffix after the slash when it contains real words.
    if "/" in key:
        suffix = key.rsplit("/", 1)[-1].strip()
        if len(re.findall(r"[A-Za-z]", suffix)) >= 4:
            normalized_suffix = normalize_for_match(suffix)
            for canonical, patterns in CANONICAL_KEY_PATTERNS:
                if any(pattern in normalized_suffix for pattern in patterns):
                    return canonical

    for canonical, patterns in CANONICAL_KEY_PATTERNS:
        if any(pattern in normalized for pattern in patterns):
            return canonical

    return key


def clean_key(key):
    key = clean(key)
    key = re.sub(r'^[^A-Za-z0-9]+', '', key)
    key = re.sub(r'\s+', ' ', key)
    key = key.strip()
    return canonical_key(key)


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
