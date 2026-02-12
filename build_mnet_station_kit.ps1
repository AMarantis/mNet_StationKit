$ErrorActionPreference = "Stop"

function Ensure-Directory {
  param([Parameter(Mandatory = $true)][string]$Path)
  if (-not (Test-Path $Path)) {
    New-Item -ItemType Directory -Path $Path | Out-Null
  }
}

function Remove-IfExists {
  param([Parameter(Mandatory = $true)][string]$Path)
  if (Test-Path $Path) {
    Remove-Item -Force -Recurse -Path $Path -ErrorAction SilentlyContinue
  }
}

function Assert-Command {
  param([Parameter(Mandatory = $true)][string]$Name)
  $cmd = Get-Command $Name -ErrorAction SilentlyContinue
  if (-not $cmd) {
    throw "Missing required command: $Name"
  }
}

$repoRoot = Split-Path -Parent $PSCommandPath
$zipPath = Join-Path $repoRoot "mNetStationKit.zip"
$zipPrefix = "mNetStationKit/"

Write-Host "Repo: $repoRoot"

Assert-Command -Name "git"

if (-not (Test-Path (Join-Path $repoRoot ".git"))) {
  throw "This folder does not look like a git checkout (missing .git). Clone the repo with git, then rerun."
}

Remove-IfExists -Path $zipPath

Write-Host "Creating archive: $zipPath"
& git -C $repoRoot archive --worktree-attributes --format=zip --prefix=$zipPrefix --output $zipPath HEAD

Write-Host ""
Write-Host "Done."
Write-Host "Next:"
Write-Host "  - Copy '$zipPath' to the station USB stick root."
Write-Host "  - Extract it so you get a folder like X:\\mNetStationKit\\ (where X: is the USB drive letter)."
Write-Host "  - On the station PC: run scripts\\Install-Dependencies.cmd (Admin), then scripts\\Setup-Admin.cmd (Admin)."
