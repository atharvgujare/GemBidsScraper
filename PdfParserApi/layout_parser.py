import re
from cleaner import clean


# --------------------------------------------
# Clean and normalize keys
# --------------------------------------------
def normalize_key(key):

    key = clean(key)

    # Remove leading symbols
    key = re.sub(r'^[^A-Za-z0-9]+', '', key)

    # Remove duplicate spaces
    key = re.sub(r'\s+', ' ', key)

    return key.strip()


# --------------------------------------------
# Clean values
# --------------------------------------------
def normalize_value(value):

    value = clean(value)

    value = re.sub(r'\s+', ' ', value)

    return value.strip()


# --------------------------------------------
# Skip useless keys
# --------------------------------------------
def is_valid_key(key):

    if not key:
        return False

    # Very short keys
    if len(key) < 2:
        return False

    # Only numbers
    if key.isdigit():
        return False

    # Too long -> probably paragraph
    if len(key) > 120:
        return False

    return True


# --------------------------------------------
# Add field safely
# --------------------------------------------
def add(result, key, value):

    key = normalize_key(key)
    value = normalize_value(value)

    if not is_valid_key(key):
        return

    if not value:
        return

    if key not in result:

        result[key] = value


# --------------------------------------------
# Main parser
# --------------------------------------------
def parse_layout(extracted):

    result = {}

    # =====================================
    # TABLES FIRST
    # =====================================

    for table in extracted["Tables"]:

        if not table:
            continue

        for row in table:

            if not row:
                continue

            cells = []

            for cell in row:

                cell = normalize_value(cell)

                if cell:
                    cells.append(cell)

            if len(cells) < 2:
                continue

            # Read every pair
            # Key Value Key Value Key Value

            i = 0

            while i + 1 < len(cells):

                key = cells[i]
                value = cells[i + 1]

                add(result, key, value)

                i += 2

    # =====================================
    # RAW TEXT
    # =====================================

    text = extracted["RawText"]

    for line in text.split("\n"):

        line = normalize_value(line)

        if ":" not in line:
            continue

        key, value = line.split(":", 1)

        add(result, key, value)

    # =====================================
    # REMOVE DUPLICATE VALUES
    # =====================================

    cleaned = {}

    seen = set()

    for key, value in result.items():

        token = (key.lower(), value.lower())

        if token in seen:
            continue

        seen.add(token)

        cleaned[key] = value

    return cleaned