. "$PSScriptRoot/_mnet_common.ps1"

Write-Host "Stopping DAQ (VCDSO.exe) if running..."

$procs = Get-Process -Name "VCDSO" -ErrorAction SilentlyContinue
if (-not $procs) { Write-Host "No VCDSO process found." }

$procs | ForEach-Object {
  try {
    Write-Host "Stopping VCDSO (PID $($_.Id))..."
    Stop-Process -Id $_.Id -Force
  } catch {
    Write-Host "Failed to stop PID $($_.Id): $($_.Exception.Message)"
  }
}

try {
  $kitRoot = Get-MNetKitRoot
  $pidPath = Join-Path (Join-Path $kitRoot "logs") "restart_daq_watchdog.pid"
  if (Test-Path $pidPath) {
    $pid = (Get-Content $pidPath -Raw).Trim()
    if ($pid -match "^[0-9]+$") {
      Write-Host "Stopping DAQ watchdog (PID $pid)..."
      Stop-Process -Id ([int]$pid) -Force -ErrorAction SilentlyContinue
    }
    Remove-Item -Path $pidPath -Force -ErrorAction SilentlyContinue
  }
} catch {}

Write-Host "Done."
