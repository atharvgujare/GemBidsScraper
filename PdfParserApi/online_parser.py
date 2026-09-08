import threading

import requests

from config import get_config

_cfg = get_config()
_session = None
_session_lock = threading.Lock()


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


def download_pdf(url, timeout=120):
    """
    Pure I/O: fetch raw PDF bytes for a URL. Safe to call concurrently
    from many threads.
    """
    session = _get_session()
    r = session.get(url, timeout=timeout)
    r.raise_for_status()
    return r.content