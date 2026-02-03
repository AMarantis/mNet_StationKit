. "$PSScriptRoot/_mnet_common.ps1"

$cfg = Get-MNetConfig

$spool = $cfg.spoolPath
Ensure-Directory -Path $spool
Ensure-SubstDrive -DriveLetter $cfg.virtualDataDriveLetter -TargetPath $spool

$outDir = "$($cfg.virtualDataDriveLetter):\Save_Pulses_Showers_Phase2"
Ensure-Directory -Path $outDir

$exe = Join-Path $outDir "VCDSO.exe"
if (-not (Test-Path $exe)) {
  throw "Showers DAQ executable not found at '$exe'. Run Setup-Admin.cmd to deploy the DAQ runtime first."
}

Get-Process -Name "VCDSO" -ErrorAction SilentlyContinue | ForEach-Object {
  try {
    Write-Host "Stopping existing VCDSO process (PID $($_.Id))..."
    Stop-Process -Id $_.Id -Force
  } catch {}
}

Write-Host "Starting Showers DAQ..."
Start-Process -FilePath $exe -WorkingDirectory $outDir
Write-Host "Started."
