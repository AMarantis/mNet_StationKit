# mNetStationKit (μNet Station DAQ + Monitoring)

Run the existing μNet station software with **minimal manual Windows setup**:

- No Windows IIS configuration
- No full Visual Studio IDE / CMake / Python required on the station (but Visual Studio Build Tools are required)
- Monitoring runs locally on: `http://localhost:8080/`

This kit is designed for **Windows 11 Home** school laptops and “USB-first” deployments.

## Greek user guide (recommended)

- Step-by-step (non-technical, detailed): `SETUP_GR.md`

## What this kit does

- Starts the monitoring website using **IIS Express** (not Windows IIS).
- Runs the existing ROOT macros for plots (via `root.exe -b -q ...`).
- Deploys the prebuilt DAQ executables into the expected data folders (`D:\Save_Pulses_*`) so the DAQ behaves “as it does today”.
- Avoids USB write latency by allowing a small local cache on `C:` when needed.

## Folder structure

- `config/` – station configuration (`station.json`)
- `deps/` – dependencies (ROOT / IIS Express / installers)
- `payload/` – station software (DAQ + monitoring site)
- `scripts/` – one-click commands (setup/start/stop/logs)

## Requirements (install once on the station)

Minimum:

1) **ROOT for Windows (x64)** (required for plots)
2) **IIS Express** (required for the monitoring website without IIS)
3) **Microsoft Visual C++ Redistributable (x64)** (often required by ROOT/DAQ DLLs)
4) **Visual Studio Build Tools 2022** (select **Desktop development with C++**) (required by ROOT/Cling to compile macros on Windows)

## Quick start (on the station PC)

1) Copy this folder to the USB root as:

- `E:\mNetStationKit\` (USB drive letter may differ)

2) Install dependencies (one-time):

Required (manual, one-time):

- Install **Visual Studio Build Tools 2022** with **Desktop development with C++**.
  - Tip: set installer "Download cache" to a fixed disk path like `C:\VSCache`.

Recommended (automatic, after VS Build Tools):

- Run as Admin: `scripts/Install-Dependencies.cmd`

This checks and installs (if missing):
- VC++ redist (x64)
- IIS Express
- ROOT (portable ZIP or installer EXE depending on `config/station.json`)

Fallback (manual):

- Install VC++ redist (x64)
- Install IIS Express (so `C:\Program Files\IIS Express\iisexpress.exe` exists)
- Download ROOT ZIP and extract so this exists:
  - `mNetStationKit\deps\root\bin\root.exe`

You can also edit `config/station.json` → `downloads.*` URLs to match your environment.

3) Run one-time setup (Admin):

- Right click `scripts/Setup-Admin.cmd` → **Run as administrator**

4) (Optional) Start DAQ:

- `scripts/Start-DAQ-Calibration.cmd`
- `scripts/Start-DAQ-Showers.cmd`
- Stop DAQ (switch modes): `scripts/Stop-DAQ.cmd`

5) Start monitoring:

- `scripts/Start-Monitoring.cmd`
- It opens the default browser at: `http://localhost:8080/`

## Data paths (important)

The existing monitoring code expects `D:\Save_Pulses_*`.

- If the station already has a real `D:` (e.g. “Virtual_D”), the kit uses it.
- Otherwise it creates a virtual `D:` mapping to the configured spool folder:
  - By default: `<kit>\mNetSpool` (i.e. alongside the kit folder on the USB)

Expected folders:

- `D:\Save_Pulses_Calibration_Phase2\`
- `D:\Save_Pulses_Showers_Phase2\`
- `D:\Save_Pulses_Showers_Rec_Phase2\`

## Troubleshooting

- Collect diagnostics:
  - Run `scripts/Collect-Logs.cmd`
  - Send the ZIP from `logs/`
- USB drive letter changed:
  - Easiest: rename the USB volume label to match `config/station.json` → `usbVolumeLabel` (default: `mNetStation`)
  - Or ensure it mounts as `E:`
- `root.exe` not found:
  - Preferred: ensure `deps/root/bin/root.exe` exists (watch out for an extra top folder inside the ZIP)
  - Alternative: install ROOT system-wide and ensure it provides `...\bin\root.exe` (the kit will try to auto-detect common locations)
- IIS Express not found:
  - Ensure `C:\Program Files\IIS Express\iisexpress.exe` exists, or provide `deps/iisexpress/iisexpress.exe`
- Browser shows error about `bin\\roslyn\\csc.exe` missing:
  - Confirm `payload/single_stationOnline_Monitoring/bin/roslyn/csc.exe` exists
  - If it keeps disappearing, check Windows Security/Defender quarantine and allow/exclude that folder
- Port 8080 busy:
  - Edit `config/station.json` → `monitoringPort`

## Security / privacy

- Do not commit credentials or passwords into this repo.
