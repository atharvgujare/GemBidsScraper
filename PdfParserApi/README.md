# GeM PDF Parser API - Setup (portable, any laptop)

## 1. Install
```bash
python -m venv venv
venv\Scripts\activate        # Windows
# source venv/bin/activate   # macOS/Linux

pip install -r requirements.txt
playwright install chromium
```

No paths need editing. `config.py` resolves everything relative to
this folder and auto-creates `config.json` + a local `pdfFolder/` on
first run, wherever the project is copied to.

## 2. Run
```bash
python app.py
```
Visit `http://localhost:8000` - you'll see the active config there too.

## 3. Configure (edit `config.json`, then restart)
```json
{
  "pdf_folder": "...auto-filled...",
  "ministry": "Ministry of Defence",
  "pages_per_run": 50,
  "download_workers": 64,
  "parse_workers": 8,
  "verbose": false,
  "schedule_times": ["09:00", "21:00"],
  "schedule_enabled": true
}
```
- `schedule_times`: 24-hour "HH:MM", local machine time. Default runs
  the full scrape twice a day. Add/remove entries for more/fewer runs.
- `pages_per_run` / `ministry`: what the *scheduled* runs use (manual
  API calls can still pass their own `pages`/`ministry` per request).
- `verbose: true` restores full old-style per-page/per-PDF logging, for
  debugging. Leave `false` for the quiet single-line progress output.
- `download_workers` / `parse_workers`: raise `download_workers` if you
  have fast internet and want more concurrent downloads; leave
  `parse_workers` at the CPU core count (default) - going higher just
  causes context-switch overhead, it won't parse faster.

You can also update the schedule without touching the file:
`POST /schedule {"times": ["08:00", "14:00", "20:00"]}` (still needs an
app restart to take effect).

## 4. Important - avoid double-scraping
If the C# `ScraperBackgroundService.cs` already schedules its own
scrape, turn ONE of the two schedulers off (`"schedule_enabled": false`
here, or disable the C# background service) so GeM isn't hit twice as
often as intended.

## Files
- `config.py`      - all settings, portable paths
- `pipeline.py`     - the actual scrape+download+parse engine (shared)
- `scheduler.py`    - runs `pipeline.py` automatically at configured times
- `app.py`          - FastAPI endpoints, thin wrapper around pipeline.py
- `gem_scraper.py`  - Playwright bid-list scraper
- `online_parser.py`, `parser.py`, `extractor.py`, `layout_builder.py`,
  `json_builder.py`, `cleaner.py` - PDF download/parse chain (unchanged
  logic from before, just quieter output)
