"""
Runs the scrape+parse pipeline automatically, at whatever times you
define in config.json under "schedule_times" (24-hour "HH:MM" strings,
local machine time), e.g.:

    "schedule_times": ["09:00", "21:00"]

That default runs it twice a day. Add/remove times for more or fewer
runs. Edit config.json (or POST /schedule) and restart the app for
schedule changes to take effect - no code changes needed.

NOTE: if the C# side (ScraperBackgroundService.cs) also has its own
schedule, make sure only ONE of the two triggers the scrape, or you'll
hit GeM twice as often as intended and risk rate limiting / duplicate
work.
"""
import asyncio

from apscheduler.schedulers.background import BackgroundScheduler
from apscheduler.triggers.cron import CronTrigger

from config import get_config
from pipeline import run_scrape_pipeline

_scheduler = BackgroundScheduler()


def _run_scheduled_job():
    cfg = get_config()
    print(f"[Scheduler] Starting scheduled run ({cfg['ministry']}, {cfg['pages_per_run']} pages)...")
    try:
        asyncio.run(run_scrape_pipeline(cfg["pages_per_run"], cfg["ministry"]))
    except Exception as ex:
        print(f"[Scheduler] Scheduled run failed: {ex}")


def start_scheduler():
    cfg = get_config()
    if not cfg.get("schedule_enabled", True):
        print("[Scheduler] Disabled (schedule_enabled: false in config.json).")
        return

    times = cfg.get("schedule_times", [])
    if not times:
        print("[Scheduler] No schedule_times configured - nothing scheduled.")
        return

    for time_str in times:
        try:
            hour, minute = map(int, time_str.strip().split(":"))
        except ValueError:
            print(f"[Scheduler] Skipping invalid schedule time: '{time_str}' (expected 'HH:MM')")
            continue

        _scheduler.add_job(
            _run_scheduled_job,
            CronTrigger(hour=hour, minute=minute),
            id=f"scrape_{time_str}",
            replace_existing=True,
        )
        print(f"[Scheduler] Daily run scheduled at {time_str}")

    _scheduler.start()


def stop_scheduler():
    _scheduler.shutdown(wait=False)