$ErrorActionPreference = "Stop"

$procs = Get-Process iisexpress -ErrorAction SilentlyContinue
if (-not $procs) {
  Write-Host "No IIS Express process found."
  exit 0
}

$procs | ForEach-Object {
  Write-Host "Stopping iisexpress pid=$($_.Id)"
  try { Stop-Process -Id $_.Id -Force } catch {}
}

Write-Host "Stopped."

