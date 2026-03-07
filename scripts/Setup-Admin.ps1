. "$PSScriptRoot/_mnet_common.ps1"

$cfg = Get-MNetConfig
$kitRoot = Get-MNetKitRoot
$driveRoot = Get-MNetDriveRootFromPath -Path $kitRoot

Write-Host "Kit path: $kitRoot"
if ($driveRoot) { Write-Host "Kit drive: $driveRoot" }

if (-not (Test-Path $kitRoot)) { throw "Kit folder not found at '$kitRoot'." }

$spool = $cfg.spoolPath
Ensure-Directory -Path $spool

# StationKit runs in spool-only mode (no virtual drives).
Ensure-SubstDrive -DriveLetter $cfg.virtualDataDriveLetter -TargetPath $spool

$calibrationDir = Join-Path $spool "Save_Pulses_Calibration_Phase2"
$showersDir = Join-Path $spool "Save_Pulses_Showers_Phase2"
$showersRecDir = Join-Path $spool "Save_Pulses_Showers_Rec_Phase2"

# Create expected data folders
Ensure-Directory -Path $calibrationDir
Ensure-Directory -Path $showersDir
Ensure-Directory -Path $showersRecDir

# Deploy DAQ runtimes into the spool data folders (so the DAQ writes data where monitoring expects it).
$calibProject = Get-PayloadOrRepoPath -Config $cfg -RepoRelativePath "DAQ mNet/single_station_DAQ_calibration" -PayloadSubPath "single_station_DAQ_calibration"
$showersProject = Get-PayloadOrRepoPath -Config $cfg -RepoRelativePath "DAQ mNet/single_station_DAQ_showers" -PayloadSubPath "single_station_DAQ_showers"

$calibDebug = Join-Path $calibProject "Debug"
$showersDebug = Join-Path $showersProject "Debug"

if (-not (Test-Path $calibDebug)) { throw "Missing calibration Debug folder at '$calibDebug' (expected prebuilt binaries)." }
if (-not (Test-Path $showersDebug)) { throw "Missing showers Debug folder at '$showersDebug' (expected prebuilt binaries)." }

$excludeFiles = @("*.pch","*.pdb","*.idb","*.ilk","*.bsc","*.log","*.tlog","*.obj","*.sbr","*.res","*.recipe","*.FileListAbsolute.txt","*.showerdata","*.data")
$excludeDirs = @("VCDSO.tlog",".vs")

Write-Host "Deploying Calibration DAQ runtime to $calibrationDir ..."
Copy-DirRobocopy -Source $calibDebug -Destination $calibrationDir -ExcludeFiles $excludeFiles -ExcludeDirs $excludeDirs

Write-Host "Deploying Showers DAQ runtime to $showersDir ..."
Copy-DirRobocopy -Source $showersDebug -Destination $showersDir -ExcludeFiles $excludeFiles -ExcludeDirs $excludeDirs

# Validate monitoring site exists
$monitoringSite = Get-PayloadOrRepoPath -Config $cfg -RepoRelativePath "DAQ mNet/single_stationOnline_Monitoring" -PayloadSubPath "single_stationOnline_Monitoring"
if (-not (Test-Path $monitoringSite)) { throw "Missing monitoring site folder at '$monitoringSite'." }

# Ensure IIS Express is present (try to install from deps/installers if missing)
try {
  $null = Get-IisExpressExe -Config $cfg
} catch {
  $installerScript = Join-Path $PSScriptRoot "Install-Dependencies.ps1"
  if (Test-Path $installerScript) {
    Write-Host "IIS Express not found; attempting offline install from deps/installers..."
    & $installerScript
    $null = Get-IisExpressExe -Config $cfg
  } else {
    throw
  }
}

# Ensure ROOT is present
$null = Get-RootExe -Config $cfg

try {
  $shortcutScript = Join-Path $PSScriptRoot "Create-Desktop-Shortcuts.ps1"
  if (Test-Path $shortcutScript) {
    Write-Host "Creating desktop shortcuts..."
    & $shortcutScript
  }
} catch {
  Write-Host "WARNING: Could not create desktop shortcuts automatically."
  Write-Host "You can run scripts\\Create-Desktop-Shortcuts.cmd manually."
}

Write-Host "Setup complete."
Write-Host "Next: run Start-Monitoring.cmd (it will open the browser automatically)."
