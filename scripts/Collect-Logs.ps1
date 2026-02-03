. "$PSScriptRoot/_mnet_common.ps1"

$cfg = Get-MNetConfig
$kitRoot = Get-MNetKitRoot

$outDir = Join-Path $kitRoot "logs"
Ensure-Directory -Path $outDir

$ts = Get-Date -Format "yyyyMMdd_HHmmss"
$zip = Join-Path $outDir "mnet_logs_$ts.zip"
$tmp = Join-Path $outDir "tmp_$ts"
Ensure-Directory -Path $tmp

try {
  $info = Join-Path $tmp "system.txt"
  @(
    "Date: $(Get-Date)",
    "User: $(whoami)",
    "OS: $([System.Environment]::OSVersion.VersionString)",
    "PowerShell: $($PSVersionTable.PSVersion)",
    "Drives:",
    (Get-PSDrive -PSProvider FileSystem | Format-Table -AutoSize | Out-String),
    "subst:",
    ((cmd.exe /c subst) | Out-String)
  ) | Set-Content -Path $info -Encoding UTF8

  $siteLog = Join-Path $tmp "iisexpress_processes.txt"
  (Get-Process iisexpress -ErrorAction SilentlyContinue | Format-Table -AutoSize | Out-String) | Set-Content -Path $siteLog -Encoding UTF8

  # Copy recent spool data sizes (do not copy data itself by default)
  $spool = $cfg.spoolPath
  $spoolInfo = Join-Path $tmp "spool_tree.txt"
  if (Test-Path $spool) {
    (Get-ChildItem -Path $spool -Recurse -ErrorAction SilentlyContinue | Select-Object FullName,Length,LastWriteTime | Format-Table -AutoSize | Out-String) |
      Set-Content -Path $spoolInfo -Encoding UTF8
  }

  Compress-Archive -Path (Join-Path $tmp "*") -DestinationPath $zip -Force
  Write-Host "Wrote: $zip"
} finally {
  Remove-Item -Recurse -Force $tmp -ErrorAction SilentlyContinue
}
