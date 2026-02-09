#!/usr/bin/env bash
set -euo pipefail

repo_root="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
dist_root="${repo_root}/dist"
kit_name="mNetStationKit"
out_dir="${dist_root}/${kit_name}"

rm -rf "${out_dir}"
mkdir -p "${out_dir}"

echo "Packaging kit at: ${out_dir}"

# Copy the repo contents into dist/, honoring .gitignore to avoid runtime output and large deps.
# Notes:
# - This repo is already structured as the kit root (config/, scripts/, payload/, deps/).
# - We exclude .git and dist itself to avoid recursion.
rsync -a --delete \
  --filter=':- .gitignore' \
  --exclude '.git/' \
  --exclude 'dist/' \
  --exclude '.gitattributes' \
  --exclude '.gitignore' \
  --exclude 'build_mnet_station_kit.sh' \
  --exclude 'build_mnet_station_kit.ps1' \
  --exclude 'build_mnet_station_kit.cmd' \
  "${repo_root}/" "${out_dir}/"

echo "Done."
echo "Next:"
echo "  - Copy '${out_dir}' to the USB stick root."
echo "  - Add portable ROOT at '${out_dir}/deps/root' (must contain bin/root.exe)."
echo "  - Provide IIS Express at '${out_dir}/deps/iisexpress/iisexpress.exe' or install it on the station."
