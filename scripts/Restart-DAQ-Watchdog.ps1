$ErrorActionPreference = "Stop"

param(
  [Parameter(Mandatory = $true)][string]$ExePath,
  [Parameter(Mandatory = $true)][string]$WorkingDirectory,
  [Parameter(Mandatory = $true)][string]$DataDir,
  [Parameter(Mandatory = $true)][ValidateSet(".data", ".showerdata")][string]$FileExtension,
  [string]$Mode = "",
  [int]$MinRestartMinutes = 55,
  [int]$MaxRestartMinutes = 65,
  [int]$RecentWriteMinutes = 10,
  [int]$PollSeconds = 60,
  [int]$MutexWaitSeconds = 60
)

. "$PSScriptRoot/_mnet_common.ps1"

function New-RandomRestartDelayMinutes {
  param([int]$Min, [int]$Max)
  if ($Max -lt $Min) { $t = $Min; $Min = $Max; $Max = $t }
  if ($Max -eq $Min) { return $Min }
  return (Get-Random -Minimum $Min -Maximum ($Max + 1))
}

function Get-LatestDataFileInfo {
  param([string]$Dir, [string]$Ext)
  try {
    return Get-ChildItem -Path $Dir -File -Filter ("*" + $Ext) -ErrorAction SilentlyContinue |
      Sort-Object -Property LastWriteTimeUtc -Descending |
      Select-Object -First 1
  } catch {
    return $null
  }
}

function Get-VcdsoProcessIdByPath {
  param([string]$ExpectedExePath)
  $expected = $ExpectedExePath.Trim().ToLowerInvariant()
  try {
    $procs = Get-CimInstance Win32_Process -Filter "Name='VCDSO.exe'" -ErrorAction SilentlyContinue
    foreach ($p in $procs) {
      if (-not $p.ExecutablePath) { continue }
      if ($p.ExecutablePath.Trim().ToLowerInvariant() -eq $expected) {
        return [int]$p.ProcessId
      }
    }
  } catch {}
  return $null
}

function Write-LogLine {
  param([string]$Message)
  $ts = (Get-Date).ToString("yyyy-MM-dd HH:mm:ss")
  $line = "[$ts] $Message"
  Write-Host $line
  try { Add-Content -Path $script:LogPath -Value $line -Encoding UTF8 } catch {}
}

$kitRoot = Get-MNetKitRoot
$logsDir = Join-Path $kitRoot "logs"
Ensure-Directory -Path $logsDir

$script:LogPath = Join-Path $logsDir "restart_daq_watchdog.log"
$pidPath = Join-Path $logsDir "restart_daq_watchdog.pid"
$statePath = Join-Path $logsDir "restart_daq_watchdog_state.json"

$mutexName = "Local\\mNetStationKit_RestartDAQ_Watchdog"
$mutex = $null
$hasMutex = $false

try {
  $mutex = New-Object System.Threading.Mutex($false, $mutexName)
  $waitMs = [Math]::Max(0, $MutexWaitSeconds) * 1000
  $hasMutex = $mutex.WaitOne($waitMs)
  if (-not $hasMutex) {
    Write-LogLine "Another watchdog instance is still running (mutex '$mutexName' busy after ${MutexWaitSeconds}s). Exiting."
    exit 0
  }

  Set-Content -Path $pidPath -Value ([string]$PID) -Encoding ASCII
  $startUtc = (Get-Date).ToUniversalTime()
  $lastRestartUtc = $startUtc
  $delayMin = New-RandomRestartDelayMinutes -Min $MinRestartMinutes -Max $MaxRestartMinutes
  $nextRestartUtc = $lastRestartUtc.AddMinutes($delayMin)

  $state = [ordered]@{
    startedUtc      = $startUtc.ToString("o")
    lastRestartUtc  = $lastRestartUtc.ToString("o")
    nextRestartUtc  = $nextRestartUtc.ToString("o")
    mode            = $Mode
    exePath         = $ExePath
    dataDir         = $DataDir
    fileExtension   = $FileExtension
    minRestartMin   = $MinRestartMinutes
    maxRestartMin   = $MaxRestartMinutes
    recentWriteMin  = $RecentWriteMinutes
    pollSeconds     = $PollSeconds
    mutexWaitSec    = $MutexWaitSeconds
    pid             = [int]$PID
  }
  ($state | ConvertTo-Json -Depth 5) | Set-Content -Path $statePath -Encoding UTF8

  Write-LogLine "Watchdog started. Mode='$Mode' Exe='$ExePath' DataDir='$DataDir' Ext='$FileExtension'. Next restart in ~${delayMin}min."

  while ($true) {
    Start-Sleep -Seconds $PollSeconds

    $procId = Get-VcdsoProcessIdByPath -ExpectedExePath $ExePath
    if (-not $procId) {
      Write-LogLine "VCDSO.exe is not running (or path differs). Exiting watchdog."
      break
    }

    $nowUtc = (Get-Date).ToUniversalTime()
    if ($nowUtc -lt $nextRestartUtc) { continue }

    $latest = Get-LatestDataFileInfo -Dir $DataDir -Ext $FileExtension
    if (-not $latest) {
      Write-LogLine "No '$FileExtension' files found under '$DataDir'. Postponing restart."
      $nextRestartUtc = $nowUtc.AddMinutes(5)
      continue
    }

    $ageMin = ($nowUtc - $latest.LastWriteTimeUtc).TotalMinutes
    if ($ageMin -gt $RecentWriteMinutes) {
      Write-LogLine ("Latest file '{0}' last wrote {1:n1} min ago (> {2} min). Assuming no signal; postponing restart." -f $latest.Name, $ageMin, $RecentWriteMinutes)
      $nextRestartUtc = $nowUtc.AddMinutes(5)
      continue
    }

    Write-LogLine ("Restarting VCDSO.exe (PID {0}). Latest write: {1:n1} min ago in '{2}'." -f $procId, $ageMin, $latest.Name)

    try {
      Stop-Process -Id $procId -Force -ErrorAction Stop
    } catch {
      Write-LogLine "Failed to stop VCDSO (PID $procId): $($_.Exception.Message). Will retry later."
      $nextRestartUtc = $nowUtc.AddMinutes(2)
      continue
    }

    Start-Sleep -Seconds 2

    try {
      Start-Process -FilePath $ExePath -WorkingDirectory $WorkingDirectory | Out-Null
    } catch {
      Write-LogLine "Failed to start VCDSO after restart: $($_.Exception.Message). Will retry later."
      $nextRestartUtc = $nowUtc.AddMinutes(2)
      continue
    }

    $lastRestartUtc = (Get-Date).ToUniversalTime()
    $delayMin = New-RandomRestartDelayMinutes -Min $MinRestartMinutes -Max $MaxRestartMinutes
    $nextRestartUtc = $lastRestartUtc.AddMinutes($delayMin)

    $state.lastRestartUtc = $lastRestartUtc.ToString("o")
    $state.nextRestartUtc = $nextRestartUtc.ToString("o")
    ($state | ConvertTo-Json -Depth 5) | Set-Content -Path $statePath -Encoding UTF8

    Write-LogLine "Restart complete. Next restart in ~${delayMin}min."
  }
} finally {
  try { Remove-Item -Path $pidPath -Force -ErrorAction SilentlyContinue } catch {}
  try {
    if ($mutex) {
      if ($hasMutex) { $mutex.ReleaseMutex() | Out-Null }
      $mutex.Dispose()
    }
  } catch {}
}

