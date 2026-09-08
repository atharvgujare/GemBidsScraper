import re
from collections import defaultdict

LINE_TOLERANCE = 3

SKIP_KEYWORDS = [
    "buyer added",
    "atc",
    "terms",
    "conditions",
    "disclaimer",
    "general conditions",
    "special conditions",
    "thank you",
    "download",
    "view file",
    "bid document",
    "page",
    "days"
]


def build_layout(words):
    if not words:
        return []

    words = sorted(words, key=lambda w: (round(w["top"]), w["x0"]))

    grouped = defaultdict(list)
    for word in words:
        top = round(word["top"] / LINE_TOLERANCE)
        grouped[top].append(word)

    layout = []
    for _, line in sorted(grouped.items()):
        line = sorted(line, key=lambda w: w["x0"])
        layout.append(line)

    return layout


def lines_to_text(layout):
    rows = []
    for line in layout:
        text = ""
        for word in line:
            text += word["text"] + " "
        rows.append(text.strip())
    return rows


def is_good_key(text):
    if not text:
        return False

    text = text.strip()

    if len(text) < 3 or len(text) > 80:
        return False

    lower = text.lower()
    for word in SKIP_KEYWORDS:
        if word in lower:
            return False

    if re.fullmatch(r"\d+\s*/\s*\d+", text):
        return False

    if text.isdigit():
        return False

    if re.match(r"^\d", text):
        return False

    return True


def is_good_value(value):
    if not value:
        return False

    value = value.strip()

    if len(value) > 500:
        return False

    if value.count("\n") > 5:
        return False

    return True


def detect_key_values(layout):
    records = []

    for line in layout:
        if len(line) < 2:
            continue

        line = sorted(line, key=lambda w: w["x0"])

        max_gap = 0
        split_index = -1
        for i in range(len(line) - 1):
            gap = line[i + 1]["x0"] - line[i]["x1"]
            if gap > max_gap:
                max_gap = gap
                split_index = i

        if split_index == -1 or max_gap < 25:
            continue

        key_words = []
        value_words = []
        for i, word in enumerate(line):
            if i <= split_index:
                key_words.append(word["text"])
            else:
                value_words.append(word["text"])

        key = " ".join(key_words).strip()
        value = " ".join(value_words).strip()

        if not is_good_key(key):
            continue
        if not is_good_value(value):
            continue
        if any(r["Key"] == key for r in records):
            continue

        records.append({"Key": key, "Value": value})

    return records


def merge_multiline_records(records):
    if not records:
        return []

    merged = []
    current = records[0]

    for record in records[1:]:
        key = record["Key"].strip()
        value = record["Value"].strip()

        if not key and not value:
            continue

        if key:
            merged.append(current)
            current = {"Key": key, "Value": value}
        else:
            if value:
                if len(value) > 250:
                    continue

                lower = value.lower()
                if any(word in lower for word in [
                    "buyer added", "terms and conditions", "disclaimer",
                    "atc", "gtc", "special conditions"
                ]):
                    continue

                current["Value"] += " " + value

    merged.append(current)
    return merged


def records_to_json(records):
    result = {}
    for record in records:
        key = record["Key"].strip()
        value = record["Value"].strip()

        if not key or not value:
            continue
        if key.isdigit():
            continue

        key = " ".join(key.split())
        value = " ".join(value.split())

        if key not in result:
            result[key] = value
        elif value not in result[key]:
            result[key] += " " + value

    return result