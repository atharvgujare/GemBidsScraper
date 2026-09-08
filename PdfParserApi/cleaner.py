import re


def clean(text):
    if text is None:
        return ""

    text = str(text)

    text = re.sub(r"\(cid:\d+\)", " ", text)
    text = re.sub(r"[\u0900-\u097F]+", " ", text)
    text = re.sub(r"[\x00-\x1F\x7F]", " ", text)
    text = re.sub(r"[\\/]{2,}", " ", text)
    text = re.sub(r"^[^A-Za-z0-9]+", "", text)

    # keep newlines
    text = re.sub(r"[ \t]+", " ", text)

    # remove blank lines
    text = re.sub(r"\n+", "\n", text)

    return text.strip()