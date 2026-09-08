import re


def clean_text(text):

    if text is None:
        return ""

    text = text.replace("\n", " ")

    text = re.sub(r"[^\x00-\x7F]+", " ", text)

    text = re.sub(r"\s+", " ", text)

    return text.strip()


def to_bool(value):

    value = clean_text(value).lower()

    return value in [
        "yes",
        "true",
        "required",
        "applicable"
    ]


def to_int(value):

    m = re.search(r"\d+", clean_text(value))

    return int(m.group()) if m else 0


def to_decimal(value):

    value = clean_text(value)

    m = re.search(r"\d+(\.\d+)?", value)

    if m:

        return float(m.group())

    return None