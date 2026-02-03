# mNetStationKit (μNet Station DAQ + Monitoring)

Run the existing μNet station software with **minimal manual Windows setup**:

- No Windows IIS configuration
- No Visual Studio / CMake / Python required on the station
- Monitoring runs locally on: `http://localhost:8080/`

This kit is designed for **Windows 11 Home** school laptops and “USB-first” deployments.

## Greek user guide (recommended)

- Step-by-step (non-technical): `SETUP_GR.md`

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

## Quick start (on the station PC)

1) Copy this folder to the USB root as:

- `E:\mNetStationKit\` (USB drive letter may differ)

2) Install dependencies (one-time):

Option A (manual):

- Install VC++ redist (x64)
- Install IIS Express (so `C:\Program Files\IIS Express\iisexpress.exe` exists)
- Download ROOT ZIP and extract so this exists:
  - `mNetStationKit\deps\root\bin\root.exe`

Option B (semi-automatic):

- Edit `config/station.json` and set `downloads.*` URLs
- Run as Admin: `scripts/Install-Dependencies.cmd`

3) Run one-time setup (Admin):

- Right click `scripts/Setup-Admin.cmd` → **Run as administrator**

4) (Optional) Start DAQ:

- `scripts/Start-DAQ-Calibration.cmd`
- `scripts/Start-DAQ-Showers.cmd`
- Stop DAQ (switch modes): `scripts/Stop-DAQ.cmd`

5) Start monitoring:

- `scripts/Start-Monitoring.cmd`
- Open: `http://localhost:8080/`

## Data paths (important)

The existing monitoring code expects `D:\Save_Pulses_*`.

- If the station already has a real `D:` (e.g. “Virtual_D”), the kit uses it.
- Otherwise it creates a virtual `D:` mapping to a small local cache:
  - `C:\mNetSpool`

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
  - Ensure `deps/root/bin/root.exe` exists (watch out for an extra top folder inside the ZIP)
- IIS Express not found:
  - Ensure `C:\Program Files\IIS Express\iisexpress.exe` exists, or provide `deps/iisexpress/iisexpress.exe`
- Port 8080 busy:
  - Edit `config/station.json` → `monitoringPort`

## Security / privacy

- Do not commit credentials or passwords into this repo.
