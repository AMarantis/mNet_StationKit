# Rebuild / Compile guide για αλλαγές στο mNetStationKit

Αυτός ο οδηγός περιγράφει τη σωστή σειρά όταν αλλάζεις κώδικα και πρέπει να φτιάξεις νέο `mNetStationKit.zip`.

Σημαντικό: το `build_mnet_station_kit.cmd` φτιάχνει το zip από το **committed HEAD** (`git archive HEAD`). Άρα οι αλλαγές πρέπει πρώτα να γίνουν commit και μετά να χτιστεί το zip.

## 1) Έλεγχος branch και αλλαγών

Άνοιξε PowerShell:

```powershell
cd D:\CERNBox\Private\PhD_Leo\mNet_052026\mNet_StationKit
git status
git branch --show-current
git log --oneline -3
```

Δούλεψε στο branch που θέλεις να παραδώσεις, συνήθως `main`.

## 2) Αν άλλαξες ASP.NET / WebForms κώδικα

Παραδείγματα:

- `payload\single_stationOnline_Monitoring\calibration.aspx.cs`
- `payload\single_stationOnline_Monitoring\OnlineMonitoring.aspx.cs`
- `.aspx`, `.designer.cs`, web app files

Κάνε compile το WebApplication2:

```powershell
cd D:\CERNBox\Private\PhD_Leo\mNet_052026\mNet_StationKit\payload\single_stationOnline_Monitoring

$msbuild = "C:\Program Files\Microsoft Visual Studio\2022\Community\MSBuild\Current\Bin\MSBuild.exe"

& $msbuild ".\WebApplication2.sln" `
  /t:Rebuild `
  /p:Configuration=Release `
  /p:Platform="Any CPU" `
  /m `
  /v:minimal

if ($LASTEXITCODE -ne 0) {
    throw "MSBuild failed with exit code $LASTEXITCODE"
}
```

Έλεγξε ότι ενημερώθηκαν τα binaries:

```powershell
Get-Item ".\bin\WebApplication2.dll", ".\bin\WebApplication2.pdb" |
  Select-Object FullName, Length, LastWriteTime
```

Κάνε commit το source change μαζί με τα rebuilt binaries:

```powershell
cd D:\CERNBox\Private\PhD_Leo\mNet_052026\mNet_StationKit
git status
git add payload\single_stationOnline_Monitoring\calibration.aspx.cs
git add payload\single_stationOnline_Monitoring\bin\WebApplication2.dll
git add payload\single_stationOnline_Monitoring\bin\WebApplication2.pdb
git commit -m "Fix calibration reader reset during live polling"
```

Προσαρμόζεις το `git add` και το commit message ανάλογα με τα αρχεία που άλλαξες.

## 3) Αν άλλαξες DAQ C++ κώδικα

Παραδείγματα:

- `payload\single_station_DAQ_calibration\...`
- `payload\single_station_DAQ_showers\...`

Χτίζεις τα DAQ projects σε `Debug | Win32`, όπως περιγράφεται στο `REBUILD_GR.md`.

Calibration DAQ:

```powershell
cd D:\CERNBox\Private\PhD_Leo\mNet_052026\mNet_StationKit

$msbuild = "C:\Program Files\Microsoft Visual Studio\2022\Community\MSBuild\Current\Bin\MSBuild.exe"

& $msbuild ".\payload\single_station_DAQ_calibration\VCDSO.sln" `
  /t:Rebuild `
  /p:Configuration=Debug `
  /p:Platform=Win32 `
  /m
```

Showers DAQ:

```powershell
& $msbuild ".\payload\single_station_DAQ_showers\VCDSO.sln" `
  /t:Rebuild `
  /p:Configuration=Debug `
  /p:Platform=Win32 `
  /m
```

Έλεγξε ότι υπάρχουν:

```powershell
Get-Item ".\payload\single_station_DAQ_calibration\Debug\VCDSO.exe"
Get-Item ".\payload\single_station_DAQ_showers\Debug\VCDSO.exe"
```

Μετά κάνε commit τα αντίστοιχα source/output αρχεία που χρειάζεται να μπουν στο kit.

## 4) Build του mNetStationKit.zip

Αφού έχεις κάνει commit όλες τις αλλαγές που πρέπει να μπουν στο zip:

```powershell
cd D:\CERNBox\Private\PhD_Leo\mNet_052026\mNet_StationKit
.\build_mnet_station_kit.cmd
```

Θα δημιουργηθεί:

```text
D:\CERNBox\Private\PhD_Leo\mNet_052026\mNet_StationKit\mNetStationKit.zip
```

Κάνε commit το zip:

```powershell
git add mNetStationKit.zip
git commit -m "Update kit package"
```

## 5) Έλεγχος ότι το zip έχει τα σωστά αρχεία

Για WebForms αλλαγές:

```powershell
Add-Type -AssemblyName System.IO.Compression.FileSystem
$zip = [IO.Compression.ZipFile]::OpenRead("D:\CERNBox\Private\PhD_Leo\mNet_052026\mNet_StationKit\mNetStationKit.zip")
$zip.Entries |
  Where-Object {
    $_.FullName -eq "payload/single_stationOnline_Monitoring/bin/WebApplication2.dll" -or
    $_.FullName -eq "payload/single_stationOnline_Monitoring/bin/WebApplication2.pdb" -or
    $_.FullName -eq "payload/single_stationOnline_Monitoring/calibration.aspx.cs"
  } |
  Select-Object FullName, Length
$zip.Dispose()
```

Για DAQ αλλαγές, έλεγξε αντίστοιχα τα `Debug\VCDSO.exe` paths μέσα στο zip.

## 6) Push

```powershell
git status
git log --oneline -5
git push origin main
```

Αν δουλεύεις σε άλλο branch:

```powershell
git push -u origin <branch-name>
```

## 7) Εγκατάσταση / δοκιμή στον σταθμό

Στον σταθμό:

1. Σταμάτα monitoring και DAQ.
2. Κλείσε τυχόν `iisexpress.exe`.
3. Κάνε καθαρό extract του νέου `mNetStationKit.zip`.
4. Τρέξε `scripts\Setup-Admin.cmd` ως Administrator.
5. Ξεκίνα ξανά monitoring/DAQ και κάνε test.

Για να σταματήσεις IIS Express:

```powershell
Get-Process iisexpress -ErrorAction SilentlyContinue | Stop-Process -Force
```

