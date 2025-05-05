#!/usr/bin/env python3
"""
generate_climate_db.py ― Load Kaggle London weather CSV (1979–2021)
───────────────────────────────────────────────────────────────────
* Reads the CSV with columns:
    date, cloud_cover, sunshine, global_radiation,
    max_temp, mean_temp, min_temp, precipitation, pressure, snow_depth
* Filters to date >= 2010-01-01
* Creates a ClimateRecord table in EnviroMonitorApp/Data/enviro.db3
* Upserts each row as TEXT/REAL values
* Prints debug/info logs
"""

import os
import sys
import csv
import sqlite3
from datetime import datetime

# ───────────────────── CONFIG ──────────────────────────────
DB_PATH   = os.path.join("EnviroMonitorApp", "Resources", "Raw", "enviro.db3")
DATE_CUTOFF = datetime(2010, 1, 1).date()

def main(csv_path):
    if not os.path.isfile(csv_path):
        print(f"[ERROR] CSV file not found: {csv_path}")
        sys.exit(1)
    if not os.path.isdir(os.path.dirname(DB_PATH)):
        print(f"[ERROR] DB directory does not exist: {os.path.dirname(DB_PATH)}")
        sys.exit(1)

    print(f"[DEBUG] Opening database at {DB_PATH}")
    conn = sqlite3.connect(DB_PATH)
    cur  = conn.cursor()

    print("[DEBUG] Creating ClimateRecord table if needed")
    cur.execute("""
    CREATE TABLE IF NOT EXISTS ClimateRecord (
        Date              TEXT PRIMARY KEY,
        CloudCover        REAL,
        Sunshine          REAL,
        GlobalRadiation   REAL,
        MaxTemp           REAL,
        MeanTemp          REAL,
        MinTemp           REAL,
        Precipitation     REAL,
        Pressure          REAL,
        SnowDepth         REAL
    )""")
    conn.commit()

    inserted = 0
    skipped  = 0

    print(f"[DEBUG] Reading CSV and filtering from {DATE_CUTOFF.isoformat()}")
    with open(csv_path, newline="", encoding="utf-8") as f:
        reader = csv.DictReader(f, delimiter=',')
        for row in reader:
            try:
                row_date = datetime.fromisoformat(row["date"]).date()
            except Exception as e:
                print(f"[WARN] Skipping invalid date '{row.get('date')}': {e}")
                skipped += 1
                continue

            if row_date < DATE_CUTOFF:
                skipped += 1
                continue

            # parse floats, allow blank→None
            def p(name):
                val = row.get(name, "").strip()
                return float(val) if val not in ("", None) else None

            vals = (
                row_date.isoformat(),
                p("cloud_cover"),
                p("sunshine"),
                p("global_radiation"),
                p("max_temp"),
                p("mean_temp"),
                p("min_temp"),
                p("precipitation"),
                p("pressure"),
                p("snow_depth")
            )

            cur.execute("""
            INSERT INTO ClimateRecord
              (Date, CloudCover, Sunshine, GlobalRadiation, MaxTemp,
               MeanTemp, MinTemp, Precipitation, Pressure, SnowDepth)
            VALUES (?, ?, ?, ?, ?, ?, ?, ?, ?, ?)
            ON CONFLICT(Date) DO UPDATE SET
              CloudCover      = excluded.CloudCover,
              Sunshine        = excluded.Sunshine,
              GlobalRadiation = excluded.GlobalRadiation,
              MaxTemp         = excluded.MaxTemp,
              MeanTemp        = excluded.MeanTemp,
              MinTemp         = excluded.MinTemp,
              Precipitation   = excluded.Precipitation,
              Pressure        = excluded.Pressure,
              SnowDepth       = excluded.SnowDepth
            """, vals)
            inserted += 1

    conn.commit()
    conn.close()

    print(f"[INFO] Finished. Inserted/updated {inserted} rows, skipped {skipped} rows.")

if __name__ == "__main__":
    if len(sys.argv) != 2:
        print("Usage: python generate_climate_db.py path/to/london_weather.csv")
        sys.exit(1)
    main(sys.argv[1])
