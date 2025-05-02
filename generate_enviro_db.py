#!/usr/bin/env python3
"""
generate_enviro_db.py ― London daily‐average air‐quality ETL using OpenAQ v3
──────────────────────────────────────────────────────────────────────────────
* Discovers all nearby sensors for NO2, SO2, PM2.5, PM10
* Pulls hourly values for a user‐supplied date range
* Sleeps on 429 / 500 with exponential backoff
* Averages each day’s readings (ignoring missing)
* Writes/updates records in enviro.db3 (Timestamp at midnight UTC)
* Exports the same daily averages to enviro.csv
"""
import os
import sys
import time
import sqlite3
import csv
from datetime import datetime, timedelta
from collections import defaultdict
import requests

# ─────────────────────── CONFIG ─────────────────────────────
API_BASE     = "https://api.openaq.org/v3"
COORDS       = "51.5074,-0.1278"       # central London
RADIUS_M     = 25_000                  # 25 km
PARAM_IDS    = {5: "NO2", 7: "SO2", 2: "PM25", 1: "PM10"}
PAGE_SIZE    = 500                     # /hours limit
SLEEP_PAGE   = 1                       # seconds between pages
MAX_RETRIES  = 5                       # for 429 & 500
BACKOFF_BASE = 2                       # exponential backoff base

DB_PATH      = os.path.join("EnviroMonitorApp","Data","enviro.db3")
CSV_PATH     = os.path.join("EnviroMonitorApp","Data","enviro.csv")

# ───────────────────── HTTP SESSION ─────────────────────────
session = requests.Session()
if "OPENAQ_KEY" not in os.environ:
    sys.exit("ERROR: set OPENAQ_KEY in your environment first.")
session.headers["X-API-Key"] = os.environ["OPENAQ_KEY"]

def get_json_or_none_on_404(url, params):
    """GET → JSON, retry on 429/500, return None on 404."""
    for attempt in range(1, MAX_RETRIES+1):
        r = session.get(url, params=params, timeout=30)
        if r.status_code == 404:
            return None
        if r.status_code in (429, 500):
            # backoff
            if r.headers.get("Retry-After"):
                wait = int(r.headers["Retry-After"])
            else:
                wait = BACKOFF_BASE ** attempt
            print(f"[WARN] {r.status_code} on {url} (try {attempt}/{MAX_RETRIES}) → sleep {wait}s")
            time.sleep(wait)
            continue
        r.raise_for_status()
        return r.json()
    # if still failing:
    r.raise_for_status()

# ────────────────── 1️⃣ DISCOVER SENSORS ──────────────────────
def discover_sensors():
    sensors = defaultdict(list)
    page = 1
    while True:
        j = get_json_or_none_on_404(
            f"{API_BASE}/locations",
            {
                "coordinates":   COORDS,
                "radius":        RADIUS_M,
                "parameters_id": ",".join(map(str, PARAM_IDS)),
                "limit":         PAGE_SIZE,
                "page":          page
            }
        )
        if not j or not j.get("results"):
            break
        for loc in j["results"]:
            for s in loc.get("sensors", []):
                pid = s["parameter"]["id"]
                sid = s["id"]
                if pid in PARAM_IDS:
                    sensors[pid].append(sid)
        found = int(str(j["meta"]["found"]).lstrip(">"))
        if page * PAGE_SIZE >= found:
            break
        page += 1
        time.sleep(SLEEP_PAGE)
    if not sensors:
        raise RuntimeError("No sensors found – check coords/radius/params")
    return sensors

# ────────────────── 2️⃣ FETCH HOURLY ─────────────────────────
def fetch_hours(sensor_id, date_from, date_to):
    page = 1
    while True:
        j = get_json_or_none_on_404(
            f"{API_BASE}/sensors/{sensor_id}/hours",
            {"date_from": date_from, "date_to": date_to, "limit": PAGE_SIZE, "page": page}
        )
        if j is None:
            # sensor retired or missing
            return
        results = j.get("results", [])
        if not results:
            return
        yield from results
        found = int(str(j["meta"]["found"]).lstrip(">"))
        if page * PAGE_SIZE >= found:
            return
        page += 1
        time.sleep(SLEEP_PAGE)

# ────────────────── 3️⃣ GATHER + DAILY AVG ──────────────────
def gather_daily_averages(start_iso, end_iso):
    sensors_by_param = discover_sensors()
    print(f"[INFO] discovered {sum(len(v) for v in sensors_by_param.values())} sensors")

    # pivot hourly readings
    hourly = defaultdict(lambda: {c: [] for c in PARAM_IDS.values()})
    for pid, sids in sensors_by_param.items():
        col = PARAM_IDS[pid]
        print(f"[INFO] fetching {col} from {len(sids)} sensors")
        for sid in sids:
            for hr in fetch_hours(sid, start_iso, end_iso):
                utc = hr["period"]["datetimeFrom"]["utc"]
                dt  = datetime.fromisoformat(utc.replace("Z","+00:00"))
                hour = dt.replace(minute=0, second=0, microsecond=0)
                val = hr.get("value")
                if val is not None:
                    hourly[hour][col].append(val)

    # aggregate daily
    daily_acc = defaultdict(lambda: {c: [] for c in PARAM_IDS.values()})
    for hour_ts, cols in hourly.items():
        day = hour_ts.date()
        for c, lst in cols.items():
            daily_acc[day][c].extend(lst)

    # compute means
    daily = {}
    for day, cols in daily_acc.items():
        daily[day] = {
            c: (sum(vals)/len(vals) if vals else None)
            for c, vals in cols.items()
        }
    return daily

# ────────────────── 4️⃣ PERSIST & EXPORT ──────────────────────
def persist_and_export(daily):
    # SQLite
    conn = sqlite3.connect(DB_PATH)
    cur  = conn.cursor()
    cur.execute("""
      CREATE TABLE IF NOT EXISTS AirQualityRecord(
        Timestamp TEXT PRIMARY KEY,
        NO2 REAL, SO2 REAL, PM25 REAL, PM10 REAL
      )
    """)
    for day, cols in sorted(daily.items()):
        ts = datetime.combine(day, datetime.min.time()) \
                     .strftime("%Y-%m-%dT%H:%M:00Z")
        cur.execute("""
          INSERT INTO AirQualityRecord(Timestamp,NO2,SO2,PM25,PM10)
          VALUES(?,?,?,?,?)
          ON CONFLICT(Timestamp) DO UPDATE SET
            NO2=excluded.NO2, SO2=excluded.SO2,
            PM25=excluded.PM25, PM10=excluded.PM10
        """, (
          ts, cols["NO2"], cols["SO2"], cols["PM25"], cols["PM10"]
        ))
    conn.commit()
    conn.close()

    # CSV
    with open(CSV_PATH, "w", newline="") as f:
        w = csv.writer(f)
        w.writerow(["Date","NO2","SO2","PM25","PM10"])
        for day, cols in sorted(daily.items()):
            w.writerow([
                day.isoformat(),
                cols["NO2"], cols["SO2"],
                cols["PM25"], cols["PM10"]
            ])

# ─────────────────────────── CLI ─────────────────────────────
def main(sdate, edate):
    start_iso = f"{sdate}T00:00:00Z"
    end_iso   = f"{edate}T23:59:59Z"
    print(f"[INFO] Aggregating daily means {start_iso} → {end_iso}")
    daily = gather_daily_averages(start_iso, end_iso)
    print(f"[INFO] Got {len(daily)} days; persisting…")
    persist_and_export(daily)
    print("[INFO] Done.")

if __name__ == "__main__":
    if len(sys.argv) != 3:
        sys.exit("Usage: generate_enviro_db.py YYYY-MM-DD YYYY-MM-DD")
    main(sys.argv[1], sys.argv[2])
