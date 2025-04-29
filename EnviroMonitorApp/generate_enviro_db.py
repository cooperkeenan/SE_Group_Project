#!/usr/bin/env python3
"""
generate_enviro_db.py

Fetch historical air‐quality data from OpenAQ and populate your
EnviroMonitorApp/Data/enviro.db3 SQLite database.

Usage:
    python generate_enviro_db.py <start_date> <end_date>
Example:
    python generate_enviro_db.py 2025-04-06 2025-04-15
"""

import os
import sys
import sqlite3
import requests
from datetime import datetime

# ───── region stub ────────────────────────────────────────────────
ISO        = "GB"
COORDS     = "51.5074,-0.1278"
PARAM_IDS  = "2,1,7,9"   # pm25, pm10, so2, no2
LOC_LIMIT  = 10
MEAS_LIMIT = 100

def iso_datetime(date_str, at_end=False):
    """Convert YYYY-MM-DD → YYYY-MM-DDT00:00:00Z (or 23:59:59Z if at_end)."""
    dt = datetime.fromisoformat(date_str)
    if at_end:
        dt = dt.replace(hour=23, minute=59, second=59)
    return dt.strftime("%Y-%m-%dT%H:%M:%SZ")

def fetch_locations():
    url = "https://api.openaq.org/v3/locations"
    resp = requests.get(url, params={
        "iso": ISO,
        "coordinates": COORDS,
        "radius": 25000,
        "parameter_id": PARAM_IDS,
        "limit": LOC_LIMIT,
    })
    resp.raise_for_status()
    return resp.json().get("results", [])

def fetch_sensor_measurements(sensor_id, frm, to):
    out = []
    page = 1
    while True:
        resp = requests.get(
            f"https://api.openaq.org/v3/sensors/{sensor_id}/measurements",
            params={
                "datetime_from": frm,
                "datetime_to":   to,
                "limit":         MEAS_LIMIT,
                "page":          page
            })
        resp.raise_for_status()
        js = resp.json()
        results = js.get("results", [])
        if not results:
            break
        out.extend(results)

        found = js.get("meta", {}).get("found", 0)
        # meta.found can be ">100", so strip
        if isinstance(found, str) and found.startswith(">"):
            found = int(found[1:])
        else:
            found = int(found)
        if page * MEAS_LIMIT >= found:
            break
        page += 1

    return out

def main(start_date, end_date):
    # build full ISO datetimes
    dt_from = iso_datetime(start_date, at_end=False)
    dt_to   = iso_datetime(end_date,   at_end=True)

    # path to your SQLite file
    db_path = os.path.join("EnviroMonitorApp", "Data", "enviro.db3")
    if not os.path.exists(db_path):
        print(f"ERROR: DB not found at {db_path}", file=sys.stderr)
        sys.exit(1)

    # open sqlite, ensure table exists
    conn = sqlite3.connect(db_path)
    conn.execute("""
    CREATE TABLE IF NOT EXISTS AirQualityRecord (
      Timestamp TEXT PRIMARY KEY,
      NO2       REAL,
      SO2       REAL,
      PM25      REAL,
      PM10      REAL
    );
    """)
    conn.commit()

    # fetch all sensors for our region
    print("Fetching locations…", file=sys.stderr)
    locs = fetch_locations()

    # collect into a dict by UTC timestamp
    records = {}  # ts_str → {NO2:…,SO2:…,PM25:…,PM10:…}
    for loc in locs:
        for sensor in loc.get("sensors", []):
            sid = sensor.get("id")
            if sid is None: continue
            print(f"  Sensor {sid}: fetching measurements…", file=sys.stderr)
            meas = fetch_sensor_measurements(sid, dt_from, dt_to)
            for m in meas:
                # drill into period.datetimeFrom.utc
                period = m.get("period", {})
                df = period.get("datetimeFrom", {})
                utc_ts = df.get("utc")
                if not utc_ts: 
                    continue
                param = m.get("parameter", {}).get("name", "").lower()
                value = m.get("value")
                rec = records.setdefault(utc_ts, {"NO2":None,"SO2":None,"PM25":None,"PM10":None})
                if param == "no2":  rec["NO2"]  = value
                if param == "so2":  rec["SO2"]  = value
                if param == "pm25": rec["PM25"] = value
                if param == "pm10": rec["PM10"] = value

    # write into table
    print(f"Writing {len(records)} records to SQLite…", file=sys.stderr)
    cur = conn.cursor()
    for ts, vals in sorted(records.items()):
        cur.execute("""
          INSERT OR REPLACE INTO AirQualityRecord
            (Timestamp, NO2, SO2, PM25, PM10)
          VALUES (?,?,?,?,?);
        """, (ts, vals["NO2"], vals["SO2"], vals["PM25"], vals["PM10"]))
    conn.commit()
    conn.close()
    print("Done.", file=sys.stderr)

if __name__=="__main__":
    if len(sys.argv) != 3:
        print("Usage:", sys.argv[0], "YYYY-MM-DD YYYY-MM-DD", file=sys.stderr)
        sys.exit(1)
    main(sys.argv[1], sys.argv[2])
