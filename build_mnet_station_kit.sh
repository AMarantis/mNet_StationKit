#!/usr/bin/env bash
set -euo pipefail

repo_root="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
kit_src="${repo_root}/station_kit"
dist_root="${repo_root}/dist"
kit_name="mNetStationKit"
out_dir="${dist_root}/${kit_name}"

rm -rf "${out_dir}"
mkdir -p "${out_dir}"

echo "Building station kit at: ${out_dir}"

# Copy kit scripts/config (exclude payload + logs)
rsync -a --delete \
  --exclude 'payload/' \
  --exclude 'logs/' \
  "${kit_src}/" "${out_dir}/"

mkdir -p "${out_dir}/payload"
mkdir -p "${out_dir}/deps/root" "${out_dir}/deps/iisexpress" "${out_dir}/deps/installers"

cat > "${out_dir}/deps/README.txt" <<'EOF'
Place offline dependencies here.

Required:
- deps\root\bin\root.exe  (portable ROOT)

One of:
- deps\iisexpress\iisexpress.exe  (portable IIS Express)
- OR have IIS Express installed on the station at: C:\Program Files\IIS Express\iisexpress.exe

Optional:
- deps\installers\ (offline installers like VC++ redist, IIS Express MSI/EXE)
EOF

cat > "${out_dir}/deps/root/README.txt" <<'EOF'
Put a portable ROOT for Windows here.
Expected path: deps\root\bin\root.exe
EOF

cat > "${out_dir}/deps/iisexpress/README.txt" <<'EOF'
Put IIS Express here if you want fully offline run.
Expected path: deps\iisexpress\iisexpress.exe

If you do not provide it, the kit will try: C:\Program Files\IIS Express\iisexpress.exe
EOF

copy_payload() {
  local src="$1"
  local dst="$2"
  echo "  payload: ${dst}"
  rsync -a --delete \
    --exclude '.DS_Store' \
    --exclude '.vs/' \
    --exclude 'obj/' \
    --exclude '*.ncb' \
    --exclude '*.opt' \
    --exclude '*.plg' \
    --exclude '*.APS' \
    --exclude 'Debug.rar' \
    --exclude '*.data' \
    --exclude '*.showerdata' \
    "${src}/" "${dst}/"
}

copy_payload "${repo_root}/DAQ mNet/single_stationOnline_Monitoring" "${out_dir}/payload/single_stationOnline_Monitoring"
copy_payload "${repo_root}/DAQ mNet/single_station_DAQ_calibration" "${out_dir}/payload/single_station_DAQ_calibration"
copy_payload "${repo_root}/DAQ mNet/single_station_DAQ_showers" "${out_dir}/payload/single_station_DAQ_showers"

echo "Done."
echo "Next:"
echo "  - Copy '${out_dir}' to the USB stick root."
echo "  - Add portable ROOT at '${out_dir}/deps/root' (must contain bin/root.exe)."
echo "  - Provide IIS Express at '${out_dir}/deps/iisexpress/iisexpress.exe' or install it on the station."
