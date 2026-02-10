. "$PSScriptRoot/_mnet_common.ps1"

$cfg = Get-MNetConfig
$kitRoot = Get-MNetKitRoot

$spool = $cfg.spoolPath
Ensure-Directory -Path $spool
Ensure-SubstDrive -DriveLetter $cfg.virtualDataDriveLetter -TargetPath $spool

$rootExe = Get-RootExe -Config $cfg
$iisExpress = Get-IisExpressExe -Config $cfg

$sitePath = Get-PayloadOrRepoPath -Config $cfg -RepoRelativePath "DAQ mNet/single_stationOnline_Monitoring" -PayloadSubPath "single_stationOnline_Monitoring"

# ROOT macros are compiled by Cling and typically need MSVC headers/SDK on Windows.
# Try to import the Visual Studio Developer Command Prompt environment so root.exe can find <new>, etc.
$cppOk = Test-CppToolchainAvailable
if (-not $cppOk) {
  $loaded = Import-VsDevCmdEnv
  if ($loaded) { $cppOk = Test-CppToolchainAvailable }
}
if (-not $cppOk) {
  Write-Host "WARNING: C++ Build Tools / Windows SDK not detected (cl.exe not found)."
  Write-Host "Plots may NOT update because ROOT cannot compile macros (e.g. fatal error: '<new>' file not found)."
  Write-Host "Fix: Install 'Visual Studio Build Tools 2022' and select 'Desktop development with C++', then rerun Start-Monitoring."
}

# IIS Express runs as the current user; child cmd processes inherit env vars.
$env:MNET_ROOT_EXE = $rootExe

$port = [int]$cfg.monitoringPort

# Stop existing IIS Express (best effort)
Get-Process iisexpress -ErrorAction SilentlyContinue | ForEach-Object {
  try { Stop-Process -Id $_.Id -Force } catch {}
}

Write-Host "Starting IIS Express..."
Write-Host "Site: $sitePath"
Write-Host "Port: $port"
Write-Host "ROOT: $rootExe"

Start-Process -FilePath $iisExpress -ArgumentList @("/path:`"$sitePath`"", "/port:$port", "/systray:false") -WorkingDirectory $sitePath

Write-Host "Started. Opening: http://localhost:$port/"

# Open the default browser at the monitoring URL (best effort).
Start-Sleep -Seconds 1
try {
  Start-Process "http://localhost:$port/"
} catch {
  Write-Host "Could not open browser automatically. Open manually: http://localhost:$port/"
}
