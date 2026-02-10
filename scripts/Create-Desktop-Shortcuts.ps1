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

$launchers = @(
  @{ Name = "mNetStationKit - Start Monitoring.cmd"; KitScript = "Start-Monitoring.cmd" },
  @{ Name = "mNetStationKit - Stop Monitoring.cmd"; KitScript = "Stop-Monitoring.cmd" },
  @{ Name = "mNetStationKit - Start DAQ Calibration.cmd"; KitScript = "Start-DAQ-Calibration.cmd" },
  @{ Name = "mNetStationKit - Start DAQ Showers.cmd"; KitScript = "Start-DAQ-Showers.cmd" },
  @{ Name = "mNetStationKit - Stop DAQ.cmd"; KitScript = "Stop-DAQ.cmd" }
)

Write-Host "Desktop: $desktop"
Write-Host "Creating shortcuts for kit folder: $kitFolderName"

foreach ($l in $launchers) {
  $outPath = Join-Path $desktop $l.Name
  $content = New-LauncherCmdContent -KitFolder $kitFolderName -KitScriptName $l.KitScript
  [System.IO.File]::WriteAllText($outPath, $content, [System.Text.Encoding]::ASCII)
  Write-Host "Created: $outPath"
}

Write-Host "Done."

