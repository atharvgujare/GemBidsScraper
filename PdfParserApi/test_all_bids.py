import json
import requests

BASE_URL = "https://bidplus.gem.gov.in"
SEARCH_PAGE = f"{BASE_URL}/all-bids"
ALL_BIDS_URL = f"{BASE_URL}/all-bids-data"

session = requests.Session()

session.headers.update({
    "User-Agent": (
        "Mozilla/5.0 (Windows NT 10.0; Win64; x64) "
        "AppleWebKit/537.36 (KHTML, like Gecko) "
        "Chrome/152.0.0.0 Safari/537.36"
    ),
    "Accept": "application/json, text/javascript, */*; q=0.01",
    "X-Requested-With": "XMLHttpRequest",
    "Referer": SEARCH_PAGE,
    "Origin": BASE_URL
})

# 1. Open All Bids page
page_response = session.get(SEARCH_PAGE, timeout=30)
page_response.raise_for_status()

print("All Bids page:", page_response.status_code)
print("Cookies:", session.cookies.get_dict())

# 2. Get CSRF token from session cookie
csrf_token = session.cookies.get("csrf_gem_cookie")

if not csrf_token:
    raise RuntimeError("CSRF token not found in cookies")

# 3. Exact payload copied from your cURL
payload = {
    "param": {
        "searchBid": "",
        "searchType": "fullText"
    },
    "filter": {
        "bidStatusType": "ongoing_bids",
        "byType": "all",
        "highBidValue": "",
        "byEndDate": {
            "from": "",
            "to": ""
        },
        "sort": "Bid-End-Date-Oldest"
    }
}

# 4. Send POST request
form_data = {
    "payload": json.dumps(payload, separators=(",", ":")),
    "csrf_bd_gem_nk": csrf_token
}

response = session.post(
    ALL_BIDS_URL,
    data=form_data,
    timeout=60
)

print("API Status:", response.status_code)
print("Content-Type:", response.headers.get("Content-Type"))
print("Response Length:", len(response.content))

response.raise_for_status()

# 5. Print response
try:
    result = response.json()

    print("\nResponse JSON:")
    print(json.dumps(result, indent=2))

except ValueError:
    print("\nResponse is not JSON:")
    print(response.text[:2000])