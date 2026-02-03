. "$PSScriptRoot/_mnet_common.ps1"

$cfg = Get-MNetConfig
$kitRoot = Get-MNetKitRoot
$driveRoot = Get-MNetDriveRootFromPath -Path $kitRoot

Write-Host "Kit path: $kitRoot"
if ($driveRoot) { Write-Host "Kit drive: $driveRoot" }

if (-not (Test-Path $kitRoot)) { throw "Kit folder not found at '$kitRoot'." }

$spool = $cfg.spoolPath
Ensure-Directory -Path $spool

# Create virtual data drive (D:) pointing to C:\mNetSpool
$driveLetter = $cfg.virtualDataDriveLetter
Ensure-SubstDrive -DriveLetter $driveLetter -TargetPath $spool

# Create expected data folders
Ensure-Directory -Path "$driveLetter`:\Save_Pulses_Calibration_Phase2"
Ensure-Directory -Path "$driveLetter`:\Save_Pulses_Showers_Phase2"
Ensure-Directory -Path "$driveLetter`:\Save_Pulses_Showers_Rec_Phase2"

# Deploy DAQ runtimes into the D: data folders (so the DAQ writes data where monitoring expects it).
$calibProject = Get-PayloadOrRepoPath -Config $cfg -RepoRelativePath "DAQ mNet/single_station_DAQ_calibration" -PayloadSubPath "single_station_DAQ_calibration"
$showersProject = Get-PayloadOrRepoPath -Config $cfg -RepoRelativePath "DAQ mNet/single_station_DAQ_showers" -PayloadSubPath "single_station_DAQ_showers"

$calibDebug = Join-Path $calibProject "Debug"
$showersDebug = Join-Path $showersProject "Debug"

if (-not (Test-Path $calibDebug)) { throw "Missing calibration Debug folder at '$calibDebug' (expected prebuilt binaries)." }
if (-not (Test-Path $showersDebug)) { throw "Missing showers Debug folder at '$showersDebug' (expected prebuilt binaries)." }

$excludeFiles = @("*.pch","*.pdb","*.idb","*.ilk","*.bsc","*.log","*.tlog","*.obj","*.sbr","*.res","*.recipe","*.FileListAbsolute.txt","*.showerdata","*.data")
$excludeDirs = @("VCDSO.tlog",".vs")

Write-Host "Deploying Calibration DAQ runtime to $driveLetter`:\Save_Pulses_Calibration_Phase2 ..."
Copy-DirRobocopy -Source $calibDebug -Destination "$driveLetter`:\Save_Pulses_Calibration_Phase2" -ExcludeFiles $excludeFiles -ExcludeDirs $excludeDirs

Write-Host "Deploying Showers DAQ runtime to $driveLetter`:\Save_Pulses_Showers_Phase2 ..."
Copy-DirRobocopy -Source $showersDebug -Destination "$driveLetter`:\Save_Pulses_Showers_Phase2" -ExcludeFiles $excludeFiles -ExcludeDirs $excludeDirs

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

Write-Host "Setup complete."
Write-Host "Next: run Start-Monitoring.cmd and open http://localhost:$($cfg.monitoringPort)/"
