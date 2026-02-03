. "$PSScriptRoot/_mnet_common.ps1"

$cfg = Get-MNetConfig
$kitRoot = Get-MNetKitRoot

$spool = $cfg.spoolPath
Ensure-Directory -Path $spool
Ensure-SubstDrive -DriveLetter $cfg.virtualDataDriveLetter -TargetPath $spool

$rootExe = Get-RootExe -Config $cfg
$iisExpress = Get-IisExpressExe -Config $cfg

$sitePath = Get-PayloadOrRepoPath -Config $cfg -RepoRelativePath "DAQ mNet/single_stationOnline_Monitoring" -PayloadSubPath "single_stationOnline_Monitoring"

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

Write-Host "Started. Open: http://localhost:$port/"
