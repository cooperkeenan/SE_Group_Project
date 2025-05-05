#!/usr/bin/env python3
"""
generate_full_enviro_db.py ― London daily‐average air & water ETL
───────────────────────────────────────────────────────────────────
* Pulls hourly air‐quality from OpenAQ, daily‐averages → AirQualityRecord
* Pulls sub‐daily water readings from UK Hydrology, daily‐averages → WaterQualityRecord
* Persists both tables into Resources/Raw/enviro.db3 (overwriting)
* Exports CSVs to EnviroMonitorApp/Data/enviro.csv & enviro_water.csv
"""

import os
import sys
import time
import sqlite3
import csv
from datetime import datetime, timedelta
from collections import defaultdict
import requests

# ────────────────────── PATHS ───────────────────────────────
BASE_DIR       = os.path.dirname(__file__)
RAW_DIR        = os.path.join(BASE_DIR, "EnviroMonitorApp", "Resources", "Raw")
DB_FILENAME    = "enviro.db3"
DB_PATH        = os.path.join(RAW_DIR, DB_FILENAME)
CSV_AIR_PATH   = os.path.join(BASE_DIR, "EnviroMonitorApp", "Data", "enviro.csv")
CSV_WATER_PATH = os.path.join(BASE_DIR, "EnviroMonitorApp", "Data", "enviro_water.csv")

# ────────────────────── AIR CONFIG ───────────────────────────
API_BASE     = "https://api.openaq.org/v3"
COORDS       = "51.5074,-0.1278"
RADIUS_M     = 25_000
PARAM_IDS    = {5: "NO2", 7: "SO2", 2: "PM25", 1: "PM10"}
PAGE_SIZE    = 500
SLEEP_PAGE   = 1
MAX_RETRIES  = 5
BACKOFF_BASE = 2

# ─────────────────── WATER CONFIG ────────────────────────────
READINGS_URL = "https://environment.data.gov.uk/hydrology/data/readings.json"
MEASURE_MAP  = {
    "Nitrate":         "https://environment.data.gov.uk/hydrology/id/measures/E05962A-nitrate-i-subdaily-mgL",
    "PH":              "https://environment.data.gov.uk/hydrology/id/measures/E05962A-ph-i-subdaily",
    "DissolvedOxygen": "https://environment.data.gov.uk/hydrology/id/measures/E05962A-do-i-subdaily-mgL",
    "Temperature":     "https://environment.data.gov.uk/hydrology/id/measures/E05962A-temp-i-subdaily-C",
}

# ────────────────── HTTP SESSIONS ────────────────────────────
# Air (requires OPENAQ_KEY)
air_sess = requests.Session()
if "OPENAQ_KEY" not in os.environ:
    sys.exit("ERROR: set OPENAQ_KEY in environment")
air_sess.headers["X-API-Key"] = os.environ["OPENAQ_KEY"]

# ───────────────────── UTILITIES ─────────────────────────────
def retry_get(session, url, params):
    for i in range(1, MAX_RETRIES+1):
        try:
            r = session.get(url, params=params, timeout=30)
        except Exception as e:
            wait = BACKOFF_BASE**i
            print(f"[WARN] Air GET error {e}, retry in {wait}s")
            time.sleep(wait)
            continue
        if r.status_code in (429,500):
            wait = BACKOFF_BASE**i
            print(f"[WARN] Air {r.status_code}, retry in {wait}s")
            time.sleep(wait)
            continue
        r.raise_for_status()
        return r.json()
    raise RuntimeError("Air: max retries exceeded")

def retry_get_water(url, params):
    for i in range(1, MAX_RETRIES+1):
        try:
            r = requests.get(url, params=params, timeout=30)
        except Exception as e:
            wait = BACKOFF_BASE**i
            print(f"[WARN] Water GET error {e}, retry in {wait}s")
            time.sleep(wait)
            continue
        if r.status_code in (429,500):
            wait = BACKOFF_BASE**i
            print(f"[WARN] Water {r.status_code}, retry in {wait}s")
            time.sleep(wait)
            continue
        if r.status_code != 200:
            print(f"[ERROR] Water {r.status_code}: {r.text}")
            return None
        return r.json()
    print("[ERROR] Water: max retries exceeded")
    return None

# ────────────────── AIR ETL ─────────────────────────────────
def gather_air(start_iso, end_iso):
    # discover sensors
    sensors = defaultdict(list)
    page = 1
    while True:
        j = retry_get(air_sess, f"{API_BASE}/locations", {
            "coordinates": COORDS,
            "radius": RADIUS_M,
            "parameters_id": ",".join(map(str, PARAM_IDS)),
            "limit": PAGE_SIZE,
            "page": page
        })
        if not j or not j.get("results"): break
        for loc in j["results"]:
            for s in loc.get("sensors",[]):
                pid, sid = s["parameter"]["id"], s["id"]
                if pid in PARAM_IDS: sensors[pid].append(sid)
        found = int(str(j["meta"]["found"]).lstrip(">"))
        if page*PAGE_SIZE>=found: break
        page+=1
        time.sleep(SLEEP_PAGE)

    # fetch hours & pivot
    hourly = defaultdict(lambda: {c:[] for c in PARAM_IDS.values()})
    for pid,sids in sensors.items():
        col = PARAM_IDS[pid]
        for sid in sids:
            page=1
            while True:
                j = retry_get(air_sess, f"{API_BASE}/sensors/{sid}/hours", {
                    "date_from": start_iso,
                    "date_to":   end_iso,
                    "limit":     PAGE_SIZE,
                    "page":      page
                })
                if not j or not j.get("results"): break
                for hr in j["results"]:
                    utc = hr["period"]["datetimeFrom"]["utc"]
                    dt = datetime.fromisoformat(utc.replace("Z","+00:00"))
                    h  = dt.replace(minute=0,second=0,microsecond=0)
                    v  = hr.get("value")
                    if v is not None: hourly[h][col].append(v)
                found = int(str(j["meta"]["found"]).lstrip(">"))
                if page*PAGE_SIZE>=found: break
                page+=1
                time.sleep(SLEEP_PAGE)

    # daily avg
    daily = {}
    start_dt = datetime.fromisoformat(start_iso.replace("Z","+00:00")).date()
    end_dt   = datetime.fromisoformat(end_iso.replace("Z","+00:00")).date()
    for h,cols in hourly.items():
        d = h.date()
        if start_dt<=d<=end_dt:
            daily.setdefault(d,{}).update({c:sum(v)/len(v) if v else None for c,v in cols.items()})
    return daily

# ────────────────── WATER ETL ────────────────────────────────
def gather_water(start_iso,end_iso):
    start_d = datetime.fromisoformat(start_iso.replace("Z","+00:00")).date()
    end_d   = datetime.fromisoformat(end_iso.replace("Z","+00:00")).date()
    daily   = defaultdict(lambda:{m:[] for m in MEASURE_MAP})
    for m,url in MEASURE_MAP.items():
        j = retry_get_water(READINGS_URL, {"measure":url,"since":start_iso})
        if not j or "items" not in j: continue
        for it in j["items"]:
            dt = None
            for key in ("dateTime","date"):
                if key in it:
                    try: dt=datetime.fromisoformat(it[key]); break
                    except: pass
            if not dt: continue
            d=dt.date()
            if start_d<=d<=end_d and (v:=it.get("value")) is not None:
                daily[d][m].append(v)

    # avg
    out={}
    for d,cols in daily.items():
        out[d]={m:(sum(v)/len(v) if v else None) for m,v in cols.items()}
    return out

# ────────────────── PERSIST & EXPORT ─────────────────────────
def persist(daily_air,daily_water):
    # ensure raw folder exists
    os.makedirs(RAW_DIR,exist_ok=True)
    # remove old DB
    if os.path.exists(DB_PATH): os.remove(DB_PATH)
    conn=sqlite3.connect(DB_PATH);cur=conn.cursor()

    # air table
    cur.execute("""
      CREATE TABLE AirQualityRecord(
        Timestamp TEXT PRIMARY KEY, NO2 REAL, SO2 REAL, PM25 REAL, PM10 REAL
      )""")
    for d,cols in sorted(daily_air.items()):
        ts=f"{d.isoformat()}T00:00:00Z"
        cur.execute("""
          INSERT INTO AirQualityRecord VALUES(?,?,?,?,?)
        """,[ts,cols["NO2"],cols["SO2"],cols["PM25"],cols["PM10"]])
    # water table
    cur.execute("""
      CREATE TABLE WaterQualityRecord(
        Timestamp TEXT PRIMARY KEY,
        Nitrate REAL, PH REAL, DissolvedOxygen REAL, Temperature REAL
      )""")
    for d,cols in sorted(daily_water.items()):
        ts=f"{d.isoformat()}T00:00:00Z"
        cur.execute("""
          INSERT INTO WaterQualityRecord VALUES(?,?,?,?,?)
        """,[ts,cols.get("Nitrate"),cols.get("PH"),
             cols.get("DissolvedOxygen"),cols.get("Temperature")])
    conn.commit();conn.close()

    # CSV outputs
    with open(CSV_AIR_PATH,"w",newline="") as f:
        w=csv.writer(f); w.writerow(["Date","NO2","SO2","PM25","PM10"])
        for d,cols in sorted(daily_air.items()):
            w.writerow([d.isoformat(),cols["NO2"],cols["SO2"],cols["PM25"],cols["PM10"]])

    with open(CSV_WATER_PATH,"w",newline="") as f:
        w=csv.writer(f); w.writerow(["Date","Nitrate","PH","DissolvedOxygen","Temperature"])
        for d,cols in sorted(daily_water.items()):
            w.writerow([d.isoformat(),
                        cols.get("Nitrate"),cols.get("PH"),
                        cols.get("DissolvedOxygen"),cols.get("Temperature")])

# ────────────────────────── MAIN ─────────────────────────────
def main(s,e):
    s_iso,f_iso=f"{s}T00:00:00Z",f"{e}T23:59:59Z"
    print(f"[INFO] Air range {s_iso}→{f_iso}"); aw=gather_air(s_iso,f_iso)
    print(f"[INFO] Water range {s_iso}→{f_iso}"); ww=gather_water(s_iso,f_iso)
    print(f"[INFO] Persisting DB → {DB_PATH}"); persist(aw,ww)
    print("[INFO] Done.")

if __name__=="__main__":
    if len(sys.argv)!=3:
        print("Usage: generate_full_enviro_db.py YYYY-MM-DD YYYY-MM-DD"); sys.exit(1)
    main(sys.argv[1],sys.argv[2])
