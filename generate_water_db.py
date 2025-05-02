#!/usr/bin/env python3
"""
generate_water_db.py — London daily-average water quality ETL using UK Hydrology API
─────────────────────────────
* Pulls sub-daily water readings from UK Hydrology for Nitrate, pH,
  Dissolved Oxygen, and Temperature.
* Aggregates daily averages between given dates (inclusive).
* Persists results into EnviroMonitorApp/Data/enviro.db3
  in a new WaterQualityRecord table.
* Exports daily averages to EnviroMonitorApp/Data/enviro_water.csv.
"""

import os
import sys
import time
import sqlite3
import csv
from datetime import datetime, date
from collections import defaultdict
import requests

# ────────────────────── CONFIG ────────────────────────────
READINGS_URL = "https://environment.data.gov.uk/hydrology/data/readings.json"
DB_PATH      = os.path.join("EnviroMonitorApp", "Data", "enviro.db3")
CSV_PATH     = os.path.join("EnviroMonitorApp", "Data", "enviro_water.csv")
MAX_RETRIES  = 5
BACKOFF_BASE = 2

MEASURE_MAP = {
    "Nitrate":         "https://environment.data.gov.uk/hydrology/id/measures/E05962A-nitrate-i-subdaily-mgL",
    "PH":              "https://environment.data.gov.uk/hydrology/id/measures/E05962A-ph-i-subdaily",
    "DissolvedOxygen": "https://environment.data.gov.uk/hydrology/id/measures/E05962A-do-i-subdaily-mgL",
    "Temperature":     "https://environment.data.gov.uk/hydrology/id/measures/E05962A-temp-i-subdaily-C",
}

def get_json_with_retry(url, params):
    """GET JSON with retries on 429/500."""
    for attempt in range(1, MAX_RETRIES + 1):
        try:
            resp = requests.get(url, params=params, timeout=30)
        except requests.RequestException as e:
            print(f"[WARN] Request error {e} (try {attempt}/{MAX_RETRIES})")
            time.sleep(BACKOFF_BASE ** attempt)
            continue
        if resp.status_code in (429, 500):
            wait = BACKOFF_BASE ** attempt
            print(f"[WARN] {resp.status_code} on {url} → retry in {wait}s")
            time.sleep(wait)
            continue
        if resp.status_code != 200:
            print(f"[ERROR] {resp.status_code} on {url}: {resp.text}")
            return None
        return resp.json()
    print("[ERROR] Max retries reached")
    return None

def gather_daily_averages(start_iso, end_iso):
    """Fetch sub-daily readings and compute daily averages."""
    # derive date boundaries (inclusive)
    start_date = datetime.fromisoformat(start_iso.replace("Z", "+00:00")).date()
    end_date   = datetime.fromisoformat(end_iso.replace("Z", "+00:00")).date()

    daily_acc = defaultdict(lambda: {k: [] for k in MEASURE_MAP})
    for measure, url in MEASURE_MAP.items():
        print(f"[INFO] fetching {measure} since {start_date}")
        data = get_json_with_retry(READINGS_URL, {"measure": url, "since": start_iso})
        if not data or "items" not in data:
            print(f"[WARN] no data for {measure}")
            continue
        items = data["items"]
        print(f"[INFO] got {len(items)} records for {measure}")
        for item in items:
            dt_str = item.get("dateTime") or item.get("date")
            if not dt_str:
                continue
            try:
                # timestamp may be dateTime (with time) or date only
                dt = datetime.fromisoformat(dt_str)
            except ValueError:
                continue
            day = dt.date()
            if day < start_date or day > end_date:
                continue
            val = item.get("value")
            if val is not None:
                daily_acc[day][measure].append(val)

    # compute daily means
    daily = {}
    for day, measures in daily_acc.items():
        daily[day] = {m: (sum(vals) / len(vals) if vals else None) for m, vals in measures.items()}
    return daily

def persist_and_export(daily):
    """Persist to SQLite and export to CSV."""
    conn = sqlite3.connect(DB_PATH)
    cur = conn.cursor()
    cur.execute("""
        CREATE TABLE IF NOT EXISTS WaterQualityRecord (
            Timestamp TEXT PRIMARY KEY,
            Nitrate REAL,
            PH REAL,
            DissolvedOxygen REAL,
            Temperature REAL
        )
    """
    )
    for day, measures in sorted(daily.items()):
        ts = datetime.combine(day, datetime.min.time()).strftime("%Y-%m-%dT%H:%M:00Z")
        cur.execute("""
            INSERT INTO WaterQualityRecord
              (Timestamp, Nitrate, PH, DissolvedOxygen, Temperature)
            VALUES (?, ?, ?, ?, ?)
            ON CONFLICT(Timestamp) DO UPDATE SET
              Nitrate=excluded.Nitrate,
              PH=excluded.PH,
              DissolvedOxygen=excluded.DissolvedOxygen,
              Temperature=excluded.Temperature
        """, (
            ts,
            measures.get("Nitrate"),
            measures.get("PH"),
            measures.get("DissolvedOxygen"),
            measures.get("Temperature")
        ))
    conn.commit()
    conn.close()

    with open(CSV_PATH, "w", newline="") as f:
        w = csv.writer(f)
        w.writerow(["Date", "Nitrate", "PH", "DissolvedOxygen", "Temperature"])
        for day, measures in sorted(daily.items()):
            w.writerow([
                day.isoformat(),
                measures.get("Nitrate"),
                measures.get("PH"),
                measures.get("DissolvedOxygen"),
                measures.get("Temperature")
            ])

def main(sdate, edate):
    start_iso = f"{sdate}T00:00:00Z"
    end_iso   = f"{edate}T23:59:59Z"
    print(f"[INFO] Aggregating WATER daily means {start_iso} → {end_iso}")
    daily = gather_daily_averages(start_iso, end_iso)
    print(f"[INFO] Got {len(daily)} days; persisting…")
    persist_and_export(daily)
    print("[INFO] Done.")

if __name__ == "__main__":
    if len(sys.argv) != 3:
        sys.exit("Usage: generate_water_db.py YYYY-MM-DD YYYY-MM-DD")
    main(sys.argv[1], sys.argv[2])
