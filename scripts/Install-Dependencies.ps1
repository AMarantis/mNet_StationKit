. "$PSScriptRoot/_mnet_common.ps1"

$cfg = Get-MNetConfig
$kitRoot = Get-MNetKitRoot

$DEFAULT_VCREDIST_URL = "https://aka.ms/vs/17/release/vc_redist.x64.exe"

function Get-Downloads {
  if ($null -eq $cfg.downloads) { return $null }
  return $cfg.downloads
}

function Find-Installer {
  param([Parameter(Mandatory=$true)][string]$Pattern)
  $dir = Join-Path $kitRoot "deps/installers"
  if (-not (Test-Path $dir)) { return $null }
  return (Get-ChildItem -Path $dir -File -ErrorAction SilentlyContinue | Where-Object { $_.Name -like $Pattern } | Select-Object -First 1)
}

function Download-File {
  param(
    [Parameter(Mandatory=$true)][string]$Url,
    [Parameter(Mandatory=$true)][string]$OutFile
  )

  if (-not $cfg.allowOnlineDownloads) {
    throw "Online downloads are disabled by config (allowOnlineDownloads=false)."
  }
  if (-not $Url) {
    throw "Missing download URL."
  }
  if (Test-Path $OutFile) {
    Write-Host "Already downloaded: $OutFile"
    return
  }
  Write-Host "Downloading: $Url"
  Invoke-WebRequest -Uri $Url -OutFile $OutFile -UseBasicParsing
}

function Install-Msi {
  param([Parameter(Mandatory=$true)][string]$Path)
  Start-Process msiexec.exe -ArgumentList @("/i", "`"$Path`"", "/qn", "/norestart") -Wait
}

function Install-ExeQuiet {
  param(
    [Parameter(Mandatory=$true)][string]$Path,
    [string[]]$Args
  )
  if (-not $Args -or $Args.Count -eq 0) {
    $Args = @("/quiet", "/norestart")
  }
  Start-Process -FilePath $Path -ArgumentList $Args -Wait
}

function Extract-ZipTo {
  param(
    [Parameter(Mandatory=$true)][string]$ZipPath,
    [Parameter(Mandatory=$true)][string]$DestDir
  )
  Ensure-Directory -Path $DestDir
  Expand-Archive -Path $ZipPath -DestinationPath $DestDir -Force
}

function Get-DownloadCachePath {
  param([Parameter(Mandatory=$true)][string]$Url)
  $dir = Join-Path $kitRoot "deps/installers"
  Ensure-Directory -Path $dir
  $leaf = Split-Path -Leaf $Url
  if (-not $leaf -or $leaf.Trim() -eq "") {
    $leaf = "download_" + [guid]::NewGuid().ToString()
  }
  return (Join-Path $dir $leaf)
}

function Test-VcRedistInstalled {
  $keys = @(
    "HKLM:\SOFTWARE\Microsoft\VisualStudio\14.0\VC\Runtimes\x64",
    "HKLM:\SOFTWARE\WOW6432Node\Microsoft\VisualStudio\14.0\VC\Runtimes\x64"
  )
  foreach ($k in $keys) {
    try {
      $p = Get-ItemProperty -Path $k -ErrorAction Stop
      if ($p.Installed -eq 1) { return $true }
    } catch {
      # ignore
    }
  }
  return $false
}

function Find-VsBuildToolsInstaller {
  $dir = Join-Path $kitRoot "deps/installers"
  if (-not (Test-Path $dir)) { return $null }

  return (Get-ChildItem -Path $dir -File -ErrorAction SilentlyContinue |
    Where-Object { $_.Name -match "(?i)vs.*buildtools.*\\.exe$" } |
    Sort-Object -Property LastWriteTime -Descending |
    Select-Object -First 1)
}

function Ensure-VsBuildToolsCpp {
  if (Test-CppToolchainAvailable) {
    Write-Host "C++ toolchain already available (cl.exe found)."
    return
  }

  # If VS is installed but the env isn't loaded, try to load it.
  $vsDev = Find-VsDevCmdBat
  if ($vsDev) {
    Write-Host "Visual Studio installation detected; loading Developer environment..."
    $null = Import-VsDevCmdEnv -VsDevCmdBat $vsDev
    if (Test-CppToolchainAvailable) {
      Write-Host "C++ toolchain now available (after loading VsDevCmd)."
      return
    }
  }

  Write-Host "C++ Build Tools not found; installing Visual Studio Build Tools (C++ workload)..."

  $installer = Find-VsBuildToolsInstaller
  if (-not $installer) {
    $dl = Get-Downloads
    $url = $null
    if ($dl -and $dl.vsBuildToolsUrl) { $url = $dl.vsBuildToolsUrl }
    if (-not $url) {
      throw "Missing downloads.vsBuildToolsUrl in config/station.json (needed to auto-install C++ Build Tools)."
    }
    $out = Get-DownloadCachePath -Url $url
    Download-File -Url $url -OutFile $out
    $installer = Get-Item $out
  }

  # Install minimal C++ build tools. This can take several GB and several minutes.
  $args = @(
    "--quiet",
    "--wait",
    "--norestart",
    "--nocache",
    "--add", "Microsoft.VisualStudio.Workload.VCTools",
    "--includeRecommended"
  )

  Write-Host "Running Build Tools installer (this may take a while)..."
  Start-Process -FilePath $installer.FullName -ArgumentList $args -Wait

  # Best effort: load env and verify cl.exe.
  $null = Import-VsDevCmdEnv
  if (-not (Test-CppToolchainAvailable)) {
    throw "Build Tools install finished, but cl.exe is still not available. Try rebooting, then rerun Install-Dependencies."
  }

  Write-Host "C++ toolchain installed and available (cl.exe found)."
}

function Ensure-RootFromZip {
  param([Parameter(Mandatory=$true)][string]$ZipPath)

  $tmp = Join-Path $env:TEMP ("root_extract_" + [guid]::NewGuid().ToString())
  Ensure-Directory -Path $tmp
  Expand-Archive -Path $ZipPath -DestinationPath $tmp -Force

  $cand = Get-ChildItem -Path $tmp -Recurse -File -Filter "root.exe" -ErrorAction SilentlyContinue |
    Where-Object { $_.FullName.ToLower().EndsWith("\bin\root.exe") } |
    Select-Object -First 1

  if (-not $cand) {
    throw "Could not locate bin\\root.exe inside the downloaded ROOT zip."
  }

  $rootBin = Split-Path -Path $cand.FullName -Parent
  $rootDir = Split-Path -Path $rootBin -Parent

  $dest = Join-Path $kitRoot "deps/root"
  Ensure-Directory -Path $dest

  # Clean destination except README.txt
  Get-ChildItem -Path $dest -Force -ErrorAction SilentlyContinue |
    Where-Object { $_.Name -ne "README.txt" } |
    ForEach-Object { Remove-Item -Path $_.FullName -Recurse -Force -ErrorAction SilentlyContinue }

  Copy-Item -Path (Join-Path $rootDir "*") -Destination $dest -Recurse -Force
}

function Install-RootFromExe {
  param([Parameter(Mandatory=$true)][string]$ExePath)

  Write-Host "Running ROOT installer: $ExePath"
  Write-Host "If a GUI opens, accept defaults; we'll try to auto-detect root.exe afterwards."

  # Best-effort silent first (depends on installer type).
  try {
    Install-ExeQuiet -Path $ExePath -Args @("/S")
  } catch {
    try {
      Install-ExeQuiet -Path $ExePath -Args @("/quiet", "/norestart")
    } catch {
      Start-Process -FilePath $ExePath -Wait
    }
  }
}

function Ensure-IisExpressPortableFromZip {
  param([Parameter(Mandatory=$true)][string]$ZipPath)

  $dest = Join-Path $kitRoot "deps/iisexpress"
  Ensure-Directory -Path $dest

  Expand-Archive -Path $ZipPath -DestinationPath $dest -Force

  $expected = Join-Path $dest "iisexpress.exe"
  if (Test-Path $expected) { return }

  $found = Get-ChildItem -Path $dest -Recurse -File -Filter "iisexpress.exe" -ErrorAction SilentlyContinue | Select-Object -First 1
  if ($found) {
    Copy-Item -Path $found.FullName -Destination $expected -Force
    return
  }

  throw "Downloaded IIS Express zip did not contain iisexpress.exe."
}

Write-Host "Kit: $kitRoot"

# IIS Express (optional if already installed)
try {
  $iis = Get-IisExpressExe -Config $cfg
  Write-Host "IIS Express found: $iis"
} catch {
  $msi = Find-Installer -Pattern "*iisexpress*.msi"
  $exe = Find-Installer -Pattern "*iisexpress*.exe"
  $zip = Find-Installer -Pattern "*iisexpress*.zip"
  if ($msi) {
    Write-Host "Installing IIS Express MSI: $($msi.FullName)"
    Install-Msi -Path $msi.FullName
  } elseif ($exe) {
    Write-Host "Installing IIS Express EXE: $($exe.FullName)"
    Install-ExeQuiet -Path $exe.FullName
  } elseif ($zip) {
    Write-Host "Extracting IIS Express ZIP: $($zip.FullName)"
    Ensure-IisExpressPortableFromZip -ZipPath $zip.FullName
  } else {
    $dl = Get-Downloads
    if ($dl -and $dl.iisExpressInstallerUrl) {
      $out = Get-DownloadCachePath -Url $dl.iisExpressInstallerUrl
      Download-File -Url $dl.iisExpressInstallerUrl -OutFile $out
      if ($out.ToLower().EndsWith(".msi")) { Install-Msi -Path $out }
      elseif ($out.ToLower().EndsWith(".zip")) { Ensure-IisExpressPortableFromZip -ZipPath $out }
      else { Install-ExeQuiet -Path $out }
    } else {
      Write-Host "IIS Express not found."
      Write-Host "Provide one of:"
      Write-Host "  - Install IIS Express (so C:\\Program Files\\IIS Express\\iisexpress.exe exists)"
      Write-Host "  - Put an installer/zip under deps\\installers (name containing 'iisexpress')"
      Write-Host "  - Set config/station.json -> downloads.iisExpressInstallerUrl"
    }
  }

  # Re-check
  $iis = Get-IisExpressExe -Config $cfg
  Write-Host "IIS Express now available: $iis"
}

# VC++ redist (optional)
if (Test-VcRedistInstalled) {
  Write-Host "VC++ redist already installed."
} else {
  $vc = Find-Installer -Pattern "vc_redist*.exe"
  if ($vc) {
    Write-Host "Installing VC++ redist: $($vc.FullName)"
    Install-ExeQuiet -Path $vc.FullName -Args @("/install", "/quiet", "/norestart")
  } else {
    $dl = Get-Downloads
    $url = $null
    if ($dl -and $dl.vcRedistInstallerUrl) { $url = $dl.vcRedistInstallerUrl }
    if (-not $url) { $url = $DEFAULT_VCREDIST_URL }

    $out = Get-DownloadCachePath -Url $url
    Download-File -Url $url -OutFile $out
    Write-Host "Installing VC++ redist: $out"
    Install-ExeQuiet -Path $out -Args @("/install", "/quiet", "/norestart")
  }
}

# C++ Build Tools / Windows SDK (needed by ROOT/Cling to compile macros on Windows)
Ensure-VsBuildToolsCpp

# ROOT (portable zip -> deps/root)
try {
  $null = Get-RootExe -Config $cfg
  Write-Host "ROOT found."
} catch {
  $dl = Get-Downloads
  if ($dl -and $dl.rootZipUrl) {
    $out = Get-DownloadCachePath -Url $dl.rootZipUrl
    Download-File -Url $dl.rootZipUrl -OutFile $out
    if ($out.ToLower().EndsWith(".zip")) {
      Ensure-RootFromZip -ZipPath $out
    } elseif ($out.ToLower().EndsWith(".exe")) {
      Install-RootFromExe -ExePath $out
    } else {
      throw "downloads.rootZipUrl must point to a .zip (portable) or .exe (installer)."
    }
    $null = Get-RootExe -Config $cfg
    Write-Host "ROOT now available: $($cfg.rootRelativeExePath)"
  } else {
    Write-Host "ROOT not found."
    Write-Host "Provide one of:"
    Write-Host "  - Put portable ROOT at deps\\root\\bin\\root.exe"
    Write-Host "  - Set config/station.json -> downloads.rootZipUrl (a .zip or .exe) and rerun Install-Dependencies"
  }
}

Write-Host "Done."
