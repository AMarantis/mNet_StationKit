. "$PSScriptRoot/_mnet_common.ps1"

Write-Host "Stopping DAQ (VCDSO.exe) if running..."

$procs = Get-Process -Name "VCDSO" -ErrorAction SilentlyContinue
if (-not $procs) {
  Write-Host "No VCDSO process found."
  exit 0
}

$procs | ForEach-Object {
  try {
    Write-Host "Stopping VCDSO (PID $($_.Id))..."
    Stop-Process -Id $_.Id -Force
  } catch {
    Write-Host "Failed to stop PID $($_.Id): $($_.Exception.Message)"
  }
}

Write-Host "Done."

