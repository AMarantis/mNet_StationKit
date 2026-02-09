$ErrorActionPreference = "Stop"

function Get-MNetKitRoot {
  # station_kit/scripts -> station_kit
  return (Resolve-Path (Join-Path $PSScriptRoot "..")).Path
}

function Find-VsDevCmdBat {
  $pf86 = ${env:ProgramFiles(x86)}
  if (-not $pf86 -or $pf86.Trim() -eq "") { $pf86 = $env:ProgramFiles }
  if (-not $pf86 -or $pf86.Trim() -eq "") { return $null }

  $roots = @(
    (Join-Path $pf86 "Microsoft Visual Studio\\2022\\BuildTools"),
    (Join-Path $pf86 "Microsoft Visual Studio\\2022\\Community"),
    (Join-Path $pf86 "Microsoft Visual Studio\\2022\\Professional"),
    (Join-Path $pf86 "Microsoft Visual Studio\\2022\\Enterprise"),
    (Join-Path $pf86 "Microsoft Visual Studio\\2019\\BuildTools"),
    (Join-Path $pf86 "Microsoft Visual Studio\\2019\\Community"),
    (Join-Path $pf86 "Microsoft Visual Studio\\2019\\Professional"),
    (Join-Path $pf86 "Microsoft Visual Studio\\2019\\Enterprise")
  ) | Where-Object { $_ -and (Test-Path $_) }

  foreach ($r in $roots) {
    $cand = Join-Path $r "Common7\\Tools\\VsDevCmd.bat"
    if (Test-Path $cand) { return $cand }
  }
  return $null
}

function Import-VsDevCmdEnv {
  param([string]$VsDevCmdBat = $null)

  if (-not $VsDevCmdBat) { $VsDevCmdBat = Find-VsDevCmdBat }
  if (-not $VsDevCmdBat) { return $false }

  # Load the VS "Developer Command Prompt" environment into this PowerShell process.
  # This is needed for ROOT/Cling to find MSVC headers (e.g. <new>) on Windows.
  $cmd = "`"$VsDevCmdBat`" -no_logo -arch=x64 -host_arch=x64 >nul && set"
  $lines = cmd.exe /c $cmd

  foreach ($line in $lines) {
    if (-not $line) { continue }
    $m = [regex]::Match($line, "^(?<k>[^=]+)=(?<v>.*)$")
    if (-not $m.Success) { continue }
    $k = $m.Groups["k"].Value
    $v = $m.Groups["v"].Value
    try { Set-Item -Path ("Env:" + $k) -Value $v } catch {}
  }

  return $true
}

function Test-CppToolchainAvailable {
  try {
    $null = Get-Command cl.exe -ErrorAction Stop
    return $true
  } catch {
    return $false
  }
}

function Get-MNetKitRootDriveLetter {
  $kitRoot = Get-MNetKitRoot
  $drive = Split-Path -Path $kitRoot -Qualifier
  if (-not $drive) { return $null }
  return $drive.TrimEnd("\").TrimEnd(":")
}

function Get-MNetConfig {
  $kitRoot = Get-MNetKitRoot
  $cfgPath = Join-Path $kitRoot "config/station.json"
  if (-not (Test-Path $cfgPath)) {
    throw "Missing config file: $cfgPath"
  }
  return (Get-Content $cfgPath -Raw | ConvertFrom-Json)
}

function Get-MNetDriveRootFromPath {
  param([Parameter(Mandatory=$true)][string]$Path)
  $qualifier = Split-Path -Path $Path -Qualifier
  if (-not $qualifier) { return $null }
  if ($qualifier.EndsWith("\")) { return $qualifier }
  return ($qualifier + "\")
}

function Get-MNetUsbDriveLetter {
  param([Parameter(Mandatory=$true)][object]$Config)

  # Prefer volume label match (works even if drive letter changes)
  try {
    $vol = Get-Volume | Where-Object { $_.FileSystemLabel -eq $Config.usbVolumeLabel } | Select-Object -First 1
    if ($null -ne $vol -and $null -ne $vol.DriveLetter -and $vol.DriveLetter -ne "") {
      return $vol.DriveLetter
    }
  } catch {
    # ignore and fall back
  }

  # Fallback: E: if present
  if (Test-Path "E:\") { return "E" }

  throw "Could not find the USB drive. Either set the USB volume label to '$($Config.usbVolumeLabel)' or ensure it mounts as E:."
}

function Get-MNetUsbRoot {
  param([Parameter(Mandatory=$true)][object]$Config)
  $letter = Get-MNetUsbDriveLetter -Config $Config
  return "$letter`:\"
}

function Get-MNetKitPathOnUsb {
  param([Parameter(Mandatory=$true)][object]$Config)
  $usbRoot = Get-MNetUsbRoot -Config $Config
  return (Join-Path $usbRoot $Config.kitFolderName)
}

function Get-PayloadPath {
  param([Parameter(Mandatory=$true)][object]$Config)
  $kitRoot = Get-MNetKitRoot
  $payload = Join-Path $kitRoot $Config.payloadRelativePath
  if (Test-Path $payload) { return $payload }
  throw "Payload folder not found at '$payload'."
}

function Get-PayloadOrRepoPath {
  param(
    [Parameter(Mandatory=$true)][object]$Config,
    [Parameter(Mandatory=$true)][string]$RepoRelativePath,
    [Parameter(Mandatory=$true)][string]$PayloadSubPath
  )

  $kitRoot = Get-MNetKitRoot
  $payloadCandidate = Join-Path (Join-Path $kitRoot $Config.payloadRelativePath) $PayloadSubPath
  if (Test-Path $payloadCandidate) { return $payloadCandidate }

  # Fallback: if the kit is still inside the full repo (not a dist kit),
  # look up one level: <repo_root>\DAQ mNet\...
  $repoRoot = Resolve-Path (Join-Path $kitRoot "..")
  $repoCandidate = Join-Path $repoRoot $RepoRelativePath
  if (Test-Path $repoCandidate) { return $repoCandidate }

  throw "Could not locate '$PayloadSubPath'. Expected either '$payloadCandidate' or '$repoCandidate'."
}

function Ensure-Directory {
  param([Parameter(Mandatory=$true)][string]$Path)
  if (-not (Test-Path $Path)) {
    New-Item -ItemType Directory -Path $Path | Out-Null
  }
}

function Ensure-SubstDrive {
  param(
    [Parameter(Mandatory=$true)][string]$DriveLetter,
    [Parameter(Mandatory=$true)][string]$TargetPath
  )

  Ensure-Directory -Path $TargetPath

  $drive = "$DriveLetter`:\"
  $existing = (subst | Select-String -Pattern ("^" + [regex]::Escape($drive)))
  if ($existing) {
    return
  }

  # If the drive exists as a real drive (e.g. a pre-created Virtual_D (D:) volume),
  # don't fail. We'll just use it and skip subst.
  if (Test-Path $drive) {
    Write-Host "Drive $drive already exists; skipping subst and using the existing drive."
    return
  }

  cmd.exe /c "subst $DriveLetter`: `"$TargetPath`"" | Out-Null
}

function Get-IisExpressExe {
  param([Parameter(Mandatory=$true)][object]$Config)
  $kitRoot = Get-MNetKitRoot
  $candidate = Join-Path $kitRoot $Config.iisExpressRelativeExePath
  if (Test-Path $candidate) { return $candidate }

  $default = "C:\Program Files\IIS Express\iisexpress.exe"
  if (Test-Path $default) { return $default }

  throw "Could not find iisexpress.exe. Put it at '$candidate' (relative to the kit root) or install IIS Express so it exists at '$default'."
}

function Get-RootExe {
  param([Parameter(Mandatory=$true)][object]$Config)
  $kitRoot = Get-MNetKitRoot
  $rootExe = Join-Path $kitRoot $Config.rootRelativeExePath

  if (Test-Path $rootExe) { return $rootExe }

  # Fallback: allow a system-installed ROOT (when the user downloads the ROOT .exe installer).
  try {
    if ($env:ROOTSYS) {
      $fromEnv = Join-Path $env:ROOTSYS "bin\\root.exe"
      if (Test-Path $fromEnv) { return $fromEnv }
    }
  } catch {}

  $candidates = @()

  # Common ROOT install folders
  foreach ($base in @("C:\\", "${env:ProgramFiles}\\", "${env:ProgramFiles(x86)}\\")) {
    if (-not $base) { continue }
    try {
      $dirs = Get-ChildItem -Path $base -Directory -Filter "root*" -ErrorAction SilentlyContinue
      foreach ($d in $dirs) {
        $cand = Join-Path $d.FullName "bin\\root.exe"
        if (Test-Path $cand) { $candidates += $cand }
      }
    } catch {}
  }

  # Try known default folder patterns (root_vX.YY.ZZ...)
  try {
    $dirs = Get-ChildItem -Path "C:\\" -Directory -Filter "root_v*" -ErrorAction SilentlyContinue
    foreach ($d in $dirs) {
      $cand = Join-Path $d.FullName "bin\\root.exe"
      if (Test-Path $cand) { $candidates += $cand }
    }
  } catch {}

  if ($candidates.Count -gt 0) {
    return $candidates | Select-Object -First 1
  }

  throw "ROOT not found. Expected portable ROOT at '$rootExe' OR a system install with root.exe under e.g. C:\\root_v*\\bin\\root.exe. Recommended: put portable ROOT at deps\\root\\bin\\root.exe."
}

function Copy-DirRobocopy {
  param(
    [Parameter(Mandatory=$true)][string]$Source,
    [Parameter(Mandatory=$true)][string]$Destination,
    [string[]]$ExcludeFiles = @(),
    [string[]]$ExcludeDirs = @()
  )

  if (-not (Test-Path $Source)) {
    throw "Source path not found: $Source"
  }
  Ensure-Directory -Path $Destination

  $args = @(
    "`"$Source`"",
    "`"$Destination`"",
    "/E",
    "/R:1",
    "/W:1",
    "/NFL",
    "/NDL",
    "/NP"
  )

  if ($ExcludeFiles.Count -gt 0) {
    $args += "/XF"
    $args += $ExcludeFiles
  }
  if ($ExcludeDirs.Count -gt 0) {
    $args += "/XD"
    $args += $ExcludeDirs
  }

  $p = Start-Process -FilePath "robocopy.exe" -ArgumentList $args -Wait -PassThru -NoNewWindow
  # robocopy uses bitmask exit codes; 0-7 are "success" variants
  if ($p.ExitCode -gt 7) {
    throw "robocopy failed with exit code $($p.ExitCode)"
  }
}
