# Kalavryta station – Cursor/Codex handoff

## Goal

Get the existing μNet station software running with **minimal manual Windows setup**, by using an **offline/portable “station kit”** and **IIS Express** (not full IIS). This avoids slow/fragile remote GUI sysadmin work.

## Constraints we discovered

- The school network (ΠΣΔ) blocks some domains (e.g. Tailscale download/login endpoints), so **we do not rely on Tailscale/VPN/SSH** for automation.
- The station is **Windows 11 Home**.
- The station uses a **USB flash drive** (usually `E:`) and has small internal disk; we allow a small local cache on `C:`.
- Data paths used by the existing monitoring code expect `D:\Save_Pulses_*` style folders.

## What we built in this repo

### 1) A station kit folder

- Source kit: `station_kit/README.md:1`
- Build script (run on Mac): `station_kit/build_mnet_station_kit.sh:1`
- Built output folder (copy this to USB / publish to GitHub): `dist/mNetStationKit`

`dist/mNetStationKit` contains:

- `scripts/` – one-click commands for install/start/logs
- `payload/` – copies of the existing DAQ + monitoring project folders
- `deps/` – placeholder for dependencies (ROOT / IIS Express / installers)
- `config/station.json` – runtime config

### 2) Made ROOT macros runnable without hard-coded `C:\root_v...`

The monitoring site runs ROOT macros via `.cmd` scripts. We changed them to use:

- environment variable: `MNET_ROOT_EXE`

Files modified in repo:

- `DAQ mNet/single_stationOnline_Monitoring/ProgramFiles/script.cmd:1`
- `DAQ mNet/single_stationOnline_Monitoring/ProgramFiles/script_calib_start.cmd:1`
- `DAQ mNet/single_stationOnline_Monitoring/ProgramFiles/script_shower_start.cmd:1`
- `DAQ mNet/single_stationOnline_Monitoring/ProgramFiles/script_sync_start.cmd:1`

`Start-Monitoring` sets `MNET_ROOT_EXE` automatically.

### 3) “Step 1–5” are automated

The goal was to eliminate manual work for the first setup steps (dependencies, IIS, build/copy, permissions).

The kit automates those via:

- `dist/mNetStationKit/scripts/Setup-Admin.cmd` / `Setup-Admin.ps1`
- `dist/mNetStationKit/scripts/Install-Dependencies.cmd` / `Install-Dependencies.ps1`

## How the kit is supposed to work (high level)

- Monitoring runs on **IIS Express** → `http://localhost:8080/`
- DAQ writes to `D:\Save_Pulses_*`
  - If `D:` already exists (some stations have “Virtual_D (D:)”), we use it.
  - Otherwise we create a virtual `D:` with: `subst D: C:\mNetSpool` (small local cache).
- `Setup-Admin` deploys prebuilt DAQ runtime files from the `Debug/` folders into:
  - `D:\Save_Pulses_Calibration_Phase2\`
  - `D:\Save_Pulses_Showers_Phase2\`

## Required software (what to install on the station)

Minimum for MVP:

- **ROOT (Windows)** (needed for plots)
- **IIS Express** (needed for monitoring without full IIS)
- **Microsoft Visual C++ Redistributable (x64)** (often required for ROOT + DAQ DLLs)

We intentionally do **not** require Visual Studio / CMake / Python / IIS Windows Features.

## What to do on the Kalavryta laptop (pilot procedure)

### A) Get the kit onto the station USB

Option 1 (recommended): from GitHub download a ZIP of `dist/mNetStationKit` and extract to the USB as:

- `E:\mNetStationKit\...` (drive letter may differ)

### B) Install dependencies

1) Edit config:

- `mNetStationKit\config\station.json`

You can either:

- Place dependencies manually under `mNetStationKit\deps\...`, or
- Set URLs in `downloads.*` and let the script download them.

2) Run as Administrator:

- `mNetStationKit\scripts\Install-Dependencies.cmd`

Notes:

- ROOT: the portable install must end up containing: `mNetStationKit\deps\root\bin\root.exe`
- If the downloaded ZIP extracts into an extra top folder, move files so the path matches.

### C) One-time station setup (admin)

Run as Administrator:

- `mNetStationKit\scripts\Setup-Admin.cmd`

This will:

- Ensure `C:\mNetSpool` exists (local cache)
- Ensure `D:\` exists (existing real D: or `subst` mapping)
- Create the expected folders under `D:\Save_Pulses_*`
- Copy DAQ runtime files from `payload/.../Debug/` into the `D:\Save_Pulses_*` folders

### D) Start DAQ (optional)

- `mNetStationKit\scripts\Start-DAQ-Calibration.cmd`
- `mNetStationKit\scripts\Start-DAQ-Showers.cmd`

### E) Start monitoring

- `mNetStationKit\scripts\Start-Monitoring.cmd`

Open:

```text
http://localhost:8080/
```

### F) Collect logs on failure

If anything fails:

- `mNetStationKit\scripts\Collect-Logs.cmd`

This writes a zip under:

- `mNetStationKit\logs\`

## What the agent should focus on next

1) Validate the pilot flow works end-to-end on Kalavryta:
   - Dependencies install
   - `Setup-Admin` creates/uses `D:\Save_Pulses_*`
   - DAQ runs from `D:` and produces files
   - Monitoring runs on IIS Express and ROOT plots execute successfully
2) If something breaks, iterate on:
   - dependency download/extract logic (ROOT zip layout, IIS Express installer type)
   - paths/permissions (especially if the station has an existing “Virtual_D” setup)
3) Only after the MVP works, consider scheduled tasks (auto-start, periodic flushing).
