$ErrorActionPreference = "Stop"

. "$PSScriptRoot/_mnet_common.ps1"

$cfg = Get-MNetConfig
$kitFolderName = $cfg.kitFolderName
if (-not $kitFolderName -or $kitFolderName.Trim() -eq "") { $kitFolderName = "mNetStationKit" }

function Get-DesktopPath {
  try {
    $p = [Environment]::GetFolderPath("Desktop")
    if ($p -and (Test-Path $p)) { return $p }
  } catch {}
  return (Join-Path $env:USERPROFILE "Desktop")
}

function New-DesktopShortcut {
  param(
    [Parameter(Mandatory = $true)][string]$ShortcutPath,
    [Parameter(Mandatory = $true)][string]$TargetPath,
    [string]$WorkingDirectory = "",
    [string]$Arguments = "",
    [string]$IconPath = "",
    [string]$Description = ""
  )

  $wsh = New-Object -ComObject WScript.Shell
  $sc = $wsh.CreateShortcut($ShortcutPath)
  $sc.TargetPath = $TargetPath
  if ($WorkingDirectory -and $WorkingDirectory.Trim() -ne "") { $sc.WorkingDirectory = $WorkingDirectory }
  if ($Arguments -and $Arguments.Trim() -ne "") { $sc.Arguments = $Arguments }
  if ($IconPath -and $IconPath.Trim() -ne "" -and (Test-Path $IconPath)) { $sc.IconLocation = $IconPath }
  if ($Description -and $Description.Trim() -ne "") { $sc.Description = $Description }
  $sc.Save()
}

function New-LauncherCmdContent {
  param(
    [Parameter(Mandatory = $true)][string]$KitFolder,
    [Parameter(Mandatory = $true)][string]$KitScriptName
  )

  $scriptRel = "scripts\{0}" -f $KitScriptName

  $lines = @(
    "@echo off",
    "setlocal EnableExtensions",
    "",
    "set ""KIT_FOLDER=$KitFolder""",
    "set ""SCRIPT_REL=$scriptRel""",
    "",
    "for %%D in (A B C D E F G H I J K L M N O P Q R S T U V W X Y Z) do (",
    "  if exist ""%%D:\%KIT_FOLDER%\%SCRIPT_REL%"" (",
    "    call ""%%D:\%KIT_FOLDER%\%SCRIPT_REL%""",
    "    exit /b %ERRORLEVEL%",
    "  )",
    ")",
    "",
    "echo Could not find %KIT_FOLDER% on any drive letter.",
    "echo Expected the kit folder at drive root, e.g. E:\%KIT_FOLDER%\",
    "echo Make sure the USB stick is inserted and the folder name matches.",
    "pause",
    "exit /b 1",
    ""
  )

  return ($lines -join "`r`n")
}

$desktop = Get-DesktopPath
Ensure-Directory -Path $desktop

$kitRoot = Get-MNetKitRoot
$iconsSourceDir = Join-Path $kitRoot "icons"

$localBase = Join-Path $env:LOCALAPPDATA $kitFolderName
$launcherDir = Join-Path $localBase "launchers"
$localIconsDir = Join-Path $localBase "icons"
Ensure-Directory -Path $launcherDir
Ensure-Directory -Path $localIconsDir

$launchers = @(
  @{ Name = "Start Monitoring"; KitScript = "Start-Monitoring.cmd"; Icon = "Start_Monitoring.ico" },
  @{ Name = "Stop Monitoring"; KitScript = "Stop-Monitoring.cmd"; Icon = "Stop_Monitoring.ico" },
  @{ Name = "Start Calibration"; KitScript = "Start-DAQ-Calibration.cmd"; Icon = "Start_Calibration.ico" },
  @{ Name = "Start Showers"; KitScript = "Start-DAQ-Showers.cmd"; Icon = "Start_Showers.ico" },
  @{ Name = "Stop DAQ"; KitScript = "Stop-DAQ.cmd"; Icon = "Stop_DAQ.ico" }
)

Write-Host "Desktop: $desktop"
Write-Host "Creating shortcuts for kit folder: $kitFolderName"
Write-Host "Launcher dir: $launcherDir"

# Clean up previous desktop launchers (old naming scheme).
$oldDesktopCmdNames = @(
  "mNetStationKit - Start Monitoring.cmd",
  "mNetStationKit - Stop Monitoring.cmd",
  "mNetStationKit - Start DAQ Calibration.cmd",
  "mNetStationKit - Start DAQ Showers.cmd",
  "mNetStationKit - Stop DAQ.cmd"
)
foreach ($n in $oldDesktopCmdNames) {
  $p = Join-Path $desktop $n
  if (Test-Path $p) {
    try { Remove-Item -Force $p } catch {}
  }
}

foreach ($l in $launchers) {
  $cmdPath = Join-Path $launcherDir ("{0}.cmd" -f $l.Name)
  $lnkPath = Join-Path $desktop ("{0}.lnk" -f $l.Name)

  $content = New-LauncherCmdContent -KitFolder $kitFolderName -KitScriptName $l.KitScript
  [System.IO.File]::WriteAllText($cmdPath, $content, [System.Text.Encoding]::ASCII)

  $iconPath = ""
  if ($l.Icon -and $l.Icon.Trim() -ne "") {
    $srcIcon = Join-Path $iconsSourceDir $l.Icon
    $dstIcon = Join-Path $localIconsDir $l.Icon
    if (Test-Path $srcIcon) {
      try { Copy-Item -Force $srcIcon $dstIcon } catch {}
      $iconPath = $dstIcon
    }
  }

  New-DesktopShortcut `
    -ShortcutPath $lnkPath `
    -TargetPath $cmdPath `
    -WorkingDirectory $launcherDir `
    -IconPath $iconPath `
    -Description ("mNetStationKit: " + $l.KitScript)

  Write-Host ("Created: {0}" -f $lnkPath)
}

Write-Host "Done."

