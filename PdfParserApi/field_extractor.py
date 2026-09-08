import re


def extract_special_fields(data):

    result = {}

    # --------------------------
    # Bid Number
    # --------------------------

    text = data.get("Bid Number", "")

    if text:

        match = re.search(
            r"GEM/\d{4}/B/\d+",
            text
        )

        if match:
            result["Bid Number"] = match.group()

        match = re.search(
            r"Dated[: ]*([0-9]{2}-[0-9]{2}-[0-9]{4})",
            text
        )

        if match:
            result["Bid Date"] = match.group(1)

    # --------------------------
    # Bid End Date
    # --------------------------

    text = data.get("Bid End Date/Time", "")

    if text:

        match = re.search(
            r"\d{2}-\d{2}-\d{4}\s+\d{2}:\d{2}:\d{2}",
            text
        )

        if match:
            result["Bid End Date/Time"] = match.group()

    # --------------------------
    # Bid Opening Date
    # --------------------------

    text = data.get("Bid Opening Date/Time", "")

    if text:

        match = re.search(
            r"\d{2}-\d{2}-\d{4}\s+\d{2}:\d{2}:\d{2}",
            text
        )

        if match:
            result["Bid Opening Date/Time"] = match.group()

    # --------------------------
    # EMD Amount
    # --------------------------

    text = data.get("EMD Amount", "")

    if text:

        match = re.search(
            r"[0-9,.]+",
            text
        )

        if match:
            result["EMD Amount"] = match.group()

    return result