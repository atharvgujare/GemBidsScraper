import threading
import time

import requests

from config import get_config

_cfg = get_config()
_session = None
_session_lock = threading.Lock()

ADVANCE_SEARCH_URL = "https://bidplus.gem.gov.in/advance-search"


def _get_session():
    """
    Lazily build one shared requests.Session with a connection pool sized
    to match the configured download concurrency. requests.Session (and
    the urllib3 pool manager underneath it) is thread-safe, and reusing
    it avoids a fresh TCP/TLS handshake per PDF - normally the single
    biggest cost when hitting hundreds of URLs.
    """
    global _session
    if _session is not None:
        return _session

    with _session_lock:
        if _session is None:
            pool_size = max(64, _cfg.get("download_workers", 64))
            session = requests.Session()
            adapter = requests.adapters.HTTPAdapter(
                pool_connections=pool_size,
                pool_maxsize=pool_size,
                max_retries=2,
            )
            session.mount("http://", adapter)
            session.mount("https://", adapter)
            session.headers.update({"User-Agent": "Mozilla/5.0"})
            _session = session

    return _session


def _is_pdf_response(response):
    content = response.content or b""

    if len(content) == 0:
        return False

    if content[:4] == b"%PDF":
        return True

    content_type = response.headers.get("Content-Type", "")

    return (
        "application/pdf" in content_type.lower()
        and len(content) > 128
    )


def download_pdf(url, timeout=120, attempts=3):
    """
    Pure I/O: fetch raw PDF bytes for a URL. Safe to call concurrently
    from many threads.
    """
    session = _get_session()

    last_error = None

    headers = {
        "Accept": "application/pdf,*/*",
        "Referer": ADVANCE_SEARCH_URL,
    }

    for attempt in range(1, attempts + 1):
        try:
            response = session.get(
                url,
                headers=headers,
                timeout=timeout,
                allow_redirects=True,
            )

            response.raise_for_status()

            if _is_pdf_response(response):
                return response.content

            content_type = response.headers.get(
                "Content-Type",
                "(missing)"
            )

            last_error = (
                "Downloaded content is not a valid PDF "
                f"(status={response.status_code}, "
                f"content_type={content_type}, "
                f"bytes={len(response.content or b'')})"
            )

        except requests.RequestException as ex:
            last_error = str(ex)

        if attempt < attempts:
            time.sleep(2 * attempt)

    raise RuntimeError(
        f"PDF download failed after {attempts} attempts: {last_error}"
    )
