#!/usr/bin/env python3
"""
generate_april_weather_min.py ― Historical daily‐min weather for April
──────────────────────────────────────────────────────────────────────
* Uses OpenWeatherMap’s onecall/timemachine to fetch hourly data for each day.
* Computes the minimum Temperature, Humidity, and WindSpeed for each April 2025 day.
* Writes results to EnviroMonitorApp/Data/april_weather_min.csv.
* Emits debug logs for request status and per‐day minima.
"""

import os
import sys
import time
import csv
import requests
from datetime import datetime, timezone, timedelta

# ───────────────────── CONFIG ────────────────────────────
API_URL     = "https://api.openweathermap.org/data/2.5/onecall/timemachine"
LATITUDE    = 51.5074
LONGITUDE   = -0.1278
API_KEY_ENV = "OPENWEATHERMAP_KEY"
DATA_FOLDER = os.path.join("EnviroMonitorApp", "Data")
CSV_PATH    = os.path.join(DATA_FOLDER, "april_weather_min.csv")

def main():
    print("[DEBUG] Starting April historical‐min ETL")

    # 1) API key
    api_key = os.environ.get(API_KEY_ENV)
    if not api_key:
        print(f"[ERROR] {API_KEY_ENV} not set")
        sys.exit(1)
    print(f"[DEBUG] Using API key from {API_KEY_ENV}")

    # 2) Prepare date range: April 1–30, 2025
    start = datetime(2025, 4, 1, tzinfo=timezone.utc)
    end   = datetime(2025, 4, 30, tzinfo=timezone.utc)

    # 3) Collect per‐day minima
    results = []
    day_count = (end - start).days + 1
    for i in range(day_count):
        day = start + timedelta(days=i)
        # use midday timestamp to retrieve full-day hours
        dt_midday = day.replace(hour=12, minute=0, second=0)
        ts = int(dt_midday.timestamp())
        params = {
            "lat": LATITUDE,
            "lon": LONGITUDE,
            "dt":  ts,
            "appid": api_key,
            "units": "metric"
        }
        print(f"[DEBUG] Fetching data for {day.date()} (ts={ts})")
        try:
            resp = requests.get(API_URL, params=params, timeout=30)
        except requests.RequestException as e:
            print(f"[ERROR] HTTP error for {day.date()}: {e}")
            continue

        print(f"[DEBUG] HTTP status for {day.date()}: {resp.status_code}")
        if resp.status_code != 200:
            print(f"[ERROR] Bad response {resp.status_code} for {day.date()}: {resp.text}")
            continue

        payload = resp.json()
        hourly = payload.get("hourly", [])
        if not hourly:
            print(f"[WARN] No hourly data for {day.date()}")
            continue

        temps = [h.get("temp") for h in hourly if h.get("temp") is not None]
        hums  = [h.get("humidity") for h in hourly if h.get("humidity") is not None]
        winds = [h.get("wind_speed") for h in hourly if h.get("wind_speed") is not None]

        if not temps or not hums or not winds:
            print(f"[WARN] Incomplete data for {day.date()}")
            continue

        min_temp    = min(temps)
        min_humidity= min(hums)
        min_wind    = min(winds)
        print(f"[DEBUG] {day.date()} → min_temp={min_temp:.1f}, min_humidity={min_humidity:.0f}, min_wind={min_wind:.1f}")

        results.append({
            "date": day.date().isoformat(),
            "min_temp": round(min_temp, 1),
            "min_humidity": round(min_humidity),
            "min_wind": round(min_wind, 1)
        })

        # avoid hammering the API
        time.sleep(1)

    # 4) Write CSV
    os.makedirs(DATA_FOLDER, exist_ok=True)
    print(f"[DEBUG] Writing {len(results)} rows to {CSV_PATH}")
    with open(CSV_PATH, "w", newline="") as f:
        writer = csv.writer(f)
        writer.writerow(["Date","MinTemperature","MinHumidity","MinWindSpeed"])
        for r in results:
            writer.writerow([r["date"], r["min_temp"], r["min_humidity"], r["min_wind"]])

    print("[INFO] Completed April minima ETL")

if __name__ == "__main__":
    main()
