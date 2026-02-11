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

try {
  $kitRoot = Get-MNetKitRoot
  $logsDir = Join-Path $kitRoot "logs"
  $pidPath = Join-Path $logsDir "restart_daq_watchdog.pid"
  if (Test-Path $pidPath) {
    $oldPid = (Get-Content $pidPath -Raw).Trim()
    if ($oldPid -match "^[0-9]+$") {
      Stop-Process -Id ([int]$oldPid) -Force -ErrorAction SilentlyContinue
    }
    Remove-Item -Path $pidPath -Force -ErrorAction SilentlyContinue
  }
} catch {}

try {
  $watchdog = Join-Path $PSScriptRoot "Restart-DAQ-Watchdog.ps1"
  if (Test-Path $watchdog) {
    Write-Host "Starting DAQ watchdog (hourly restart, best-effort)..."
    Start-Process -FilePath "powershell.exe" -WindowStyle Hidden -ArgumentList @(
      "-NoProfile",
      "-ExecutionPolicy", "Bypass",
      "-File", $watchdog,
      "-ExePath", $exe,
      "-WorkingDirectory", $outDir,
      "-DataDir", $outDir,
      "-FileExtension", ".showerdata",
      "-Mode", "Showers"
    ) | Out-Null
  }
} catch {
  Write-Host "WARNING: Could not start DAQ watchdog: $($_.Exception.Message)"
}
