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
    Remove-Item -Force -Recurse -Path $Path
  }
}

function Assert-Command {
  param([Parameter(Mandatory = $true)][string]$Name)
  $cmd = Get-Command $Name -ErrorAction SilentlyContinue
  if (-not $cmd) {
    throw "Missing required command: $Name"
  }
}

function Expand-ZipToDirectory {
  param(
    [Parameter(Mandatory = $true)][string]$ZipPath,
    [Parameter(Mandatory = $true)][string]$DestinationPath
  )

  Ensure-Directory -Path $DestinationPath

  # Prefer tar.exe on Windows (more reliable than Expand-Archive in some environments).
  $tar = Get-Command "tar.exe" -ErrorAction SilentlyContinue
  if ($tar) {
    & $tar.Source -xf $ZipPath -C $DestinationPath
    return
  }

  Expand-Archive -Path $ZipPath -DestinationPath $DestinationPath
}

$repoRoot = Split-Path -Parent $PSCommandPath
$distRoot = Join-Path $repoRoot "dist"
$outDir = Join-Path $distRoot "mNetStationKit"
$zipPath = Join-Path $distRoot "mNetStationKit.zip"
$tmpExtractDir = Join-Path $distRoot ("_extract_" + [guid]::NewGuid().ToString())

Write-Host "Repo: $repoRoot"

Assert-Command -Name "git"

if (-not (Test-Path (Join-Path $repoRoot ".git"))) {
  throw "This folder does not look like a git checkout (missing .git). Clone the repo with git, then rerun."
}

Ensure-Directory -Path $distRoot
Remove-IfExists -Path $tmpExtractDir
Remove-IfExists -Path $zipPath

Write-Host "Creating archive: $zipPath"
& git -C $repoRoot archive --format=zip --output $zipPath HEAD

try {
  # Expand-Archive -Force can fail when the destination exists or is in a partial state.
  # Extract to a fresh temp folder first, then move it into place.
  Write-Host "Extracting to: $tmpExtractDir"
  Expand-ZipToDirectory -ZipPath $zipPath -DestinationPath $tmpExtractDir

  # Remove repo-only helper files from the packaged kit (not needed on the station USB).
  @(
    ".gitignore",
    ".gitattributes",
    "build_mnet_station_kit.sh",
    "build_mnet_station_kit.ps1",
    "build_mnet_station_kit.cmd"
  ) | ForEach-Object {
    $p = Join-Path $tmpExtractDir $_
    if (Test-Path $p) { Remove-Item -Force -Path $p }
  }

  Remove-IfExists -Path $outDir
  Move-Item -Path $tmpExtractDir -Destination $outDir
} finally {
  # Best-effort cleanup if something failed before Move-Item.
  if (Test-Path $tmpExtractDir) {
    Remove-Item -Force -Recurse -Path $tmpExtractDir -ErrorAction SilentlyContinue
  }
}

Write-Host ""
Write-Host "Done."
Write-Host "Next:"
Write-Host "  - Copy '$outDir' to the USB stick root (e.g. E:\\mNetStationKit\\...)."
Write-Host "  - On the station PC: run scripts\\Install-Dependencies.cmd (Admin), then scripts\\Setup-Admin.cmd (Admin)."
