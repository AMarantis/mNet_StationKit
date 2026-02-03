. "$PSScriptRoot/_mnet_common.ps1"

$cfg = Get-MNetConfig
$kitRoot = Get-MNetKitRoot

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

Write-Host "Kit: $kitRoot"

# IIS Express (optional if already installed)
try {
  $iis = Get-IisExpressExe -Config $cfg
  Write-Host "IIS Express found: $iis"
} catch {
  $msi = Find-Installer -Pattern "*iisexpress*.msi"
  $exe = Find-Installer -Pattern "*iisexpress*.exe"
  if ($msi) {
    Write-Host "Installing IIS Express MSI: $($msi.FullName)"
    Install-Msi -Path $msi.FullName
  } elseif ($exe) {
    Write-Host "Installing IIS Express EXE: $($exe.FullName)"
    Install-ExeQuiet -Path $exe.FullName
  } else {
    $dl = Get-Downloads
    if ($dl -and $dl.iisExpressInstallerUrl) {
      $tmp = Join-Path $env:TEMP ("iisexpress_" + [guid]::NewGuid().ToString())
      Ensure-Directory -Path $tmp
      $out = Join-Path $tmp (Split-Path -Leaf $dl.iisExpressInstallerUrl)
      Download-File -Url $dl.iisExpressInstallerUrl -OutFile $out
      if ($out.ToLower().EndsWith(".msi")) {
        Install-Msi -Path $out
      } elseif ($out.ToLower().EndsWith(".zip")) {
        $dest = Join-Path $kitRoot "deps/iisexpress"
        Extract-ZipTo -ZipPath $out -DestDir $dest
      } else {
        Install-ExeQuiet -Path $out
      }
    } else {
      Write-Host "No IIS Express installer found under deps/installers, and no downloads.iisExpressInstallerUrl configured. Skipping."
    }
  }

  # Re-check
  $iis = Get-IisExpressExe -Config $cfg
  Write-Host "IIS Express now available: $iis"
}

# VC++ redist (optional)
$vc = Find-Installer -Pattern "vc_redist*.exe"
if ($vc) {
  Write-Host "Installing VC++ redist: $($vc.FullName)"
  Install-ExeQuiet -Path $vc.FullName -Args @("/install", "/quiet", "/norestart")
} else {
  $dl = Get-Downloads
  if ($dl -and $dl.vcRedistInstallerUrl) {
    $tmp = Join-Path $env:TEMP ("vcredist_" + [guid]::NewGuid().ToString())
    Ensure-Directory -Path $tmp
    $out = Join-Path $tmp (Split-Path -Leaf $dl.vcRedistInstallerUrl)
    Download-File -Url $dl.vcRedistInstallerUrl -OutFile $out
    Install-ExeQuiet -Path $out -Args @("/install", "/quiet", "/norestart")
  } else {
    Write-Host "No vc_redist installer found under deps/installers, and no downloads.vcRedistInstallerUrl configured. Skipping."
  }
}

# ROOT (portable zip -> deps/root)
try {
  $null = Get-RootExe -Config $cfg
  Write-Host "ROOT found."
} catch {
  $dl = Get-Downloads
  if ($dl -and $dl.rootZipUrl) {
    $tmp = Join-Path $env:TEMP ("root_" + [guid]::NewGuid().ToString())
    Ensure-Directory -Path $tmp
    $out = Join-Path $tmp (Split-Path -Leaf $dl.rootZipUrl)
    Download-File -Url $dl.rootZipUrl -OutFile $out
    if (-not $out.ToLower().EndsWith(".zip")) {
      throw "downloads.rootZipUrl must point to a .zip for portable ROOT."
    }
    $dest = Join-Path $kitRoot "deps/root"
    Extract-ZipTo -ZipPath $out -DestDir $dest
    $null = Get-RootExe -Config $cfg
    Write-Host "ROOT now available: $($cfg.rootRelativeExePath)"
  } else {
    Write-Host "ROOT not found and no downloads.rootZipUrl configured. Skipping."
  }
}

Write-Host "Done."
