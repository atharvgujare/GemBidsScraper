"""
Centralized, portable configuration.

Nothing here is hardcoded to a specific machine or username. All paths
are resolved relative to this file's location, so the project runs the
same after being copied/zipped to any other laptop.

Settings can be overridden three ways (in order of priority):
  1. Environment variables (GEM_*)      - best for servers/CI
  2. config.json (auto-created here)     - best for a laptop, hand-editable
  3. Built-in defaults below
"""
import json
import os
from pathlib import Path

BASE_DIR = Path(__file__).resolve().parent
CONFIG_PATH = BASE_DIR / "config.json"

DEFAULTS = {
    "pdf_folder": str(BASE_DIR / "pdfFolder"),
    "ministry": "Ministry of Defence",
    "pages_per_run": 50,
    "download_workers": 64,
    "parse_workers": os.cpu_count() or 4,
    "verbose": True,
    # 24-hour "HH:MM" times, local machine time. Add/remove entries for
    # more/fewer runs per day. Edit this list (or use POST /schedule)
    # and restart the app to change the schedule.
    "schedule_times": ["09:00", "21:00"],
    "schedule_enabled": True,
}


def _load_json():
    if CONFIG_PATH.exists():
        try:
            with open(CONFIG_PATH, "r", encoding="utf-8") as f:
                return json.load(f)
        except Exception:
            return {}
    return {}


def _save_json(cfg):
    with open(CONFIG_PATH, "w", encoding="utf-8") as f:
        json.dump(cfg, f, indent=2)


def get_config():
    cfg = DEFAULTS.copy()
    cfg.update(_load_json())

    env_map = {
        "GEM_PDF_FOLDER": "pdf_folder",
        "GEM_MINISTRY": "ministry",
        "GEM_PAGES_PER_RUN": "pages_per_run",
        "GEM_DOWNLOAD_WORKERS": "download_workers",
        "GEM_PARSE_WORKERS": "parse_workers",
        "GEM_VERBOSE": "verbose",
        "GEM_SCHEDULE_ENABLED": "schedule_enabled",
    }

    for env_key, cfg_key in env_map.items():
        if env_key in os.environ:
            val = os.environ[env_key]

            if cfg_key in ("pages_per_run", "download_workers", "parse_workers"):
                val = int(val)

            elif cfg_key in ("verbose", "schedule_enabled"):
                val = val.lower() in ("1", "true", "yes")

            cfg[cfg_key] = val

    # Resolve PDF folder relative to this Python project
    # when a relative path is configured.
    pdf_folder = Path(cfg["pdf_folder"])

    if not pdf_folder.is_absolute():
        pdf_folder = BASE_DIR / pdf_folder

    cfg["pdf_folder"] = str(pdf_folder.resolve())

    # Make sure the folder exists.
    pdf_folder.mkdir(parents=True, exist_ok=True)

    if not CONFIG_PATH.exists():
        _save_json(cfg)

    return cfg


def update_schedule_times(times):
    """Persist new schedule times (list of 'HH:MM' strings) to config.json."""
    cfg = DEFAULTS.copy()
    cfg.update(_load_json())
    cfg["schedule_times"] = times
    _save_json(cfg)


def update_config(**kwargs):
    """Persist arbitrary config overrides (e.g. pages_per_run, ministry)."""
    cfg = DEFAULTS.copy()
    cfg.update(_load_json())
    cfg.update(kwargs)
    _save_json(cfg)