#!/usr/bin/env bash
set -euo pipefail

repo_root="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
kit_name="mNetStationKit"
zip_path="${repo_root}/${kit_name}.zip"

echo "Repo: ${repo_root}"
echo "Creating archive: ${zip_path}"

rm -f "${zip_path}"

command -v git >/dev/null 2>&1 || { echo "Missing required command: git" >&2; exit 1; }

git -C "${repo_root}" archive --worktree-attributes --format=zip --prefix="${kit_name}/" --output "${zip_path}" HEAD

echo "Done."
echo "Next:"
echo "  - Copy '${zip_path}' to the station USB stick root."
echo "  - Extract it so you get a folder like X:\\${kit_name}\\ (where X: is the USB drive letter)."
