# Rebuild (compile) του DAQ μετά από αλλαγές κώδικα (στον δικό σου υπολογιστή)

Αυτός ο οδηγός είναι **μόνο** για το PC σου (dev machine), όπου έχεις ήδη εγκατεστημένο Visual Studio IDE και όλα τα components που χρειάζονται. Δεν υποθέτει τίποτα για USB / drive letters / virtual `D:` κτλ (αυτά αφορούν τους υπολογιστές των σταθμών και καλύπτονται στο `SETUP_GR.md`).

## Τι “χτίζουμε”

Στο repo/kit υπάρχουν **δύο** DAQ projects (C++ / Visual Studio) που παράγουν `VCDSO.exe`:

- Calibration DAQ:
  - `payload\single_station_DAQ_calibration\VCDSO.sln`
  - output: `payload\single_station_DAQ_calibration\Debug\VCDSO.exe`
- Showers DAQ:
  - `payload\single_station_DAQ_showers\VCDSO.sln`
  - output: `payload\single_station_DAQ_showers\Debug\VCDSO.exe`

Σημαντικό:
- Τα scripts του kit περιμένουν **Debug/Win32** builds (ώστε να υπάρχει `Debug\VCDSO.exe`).

## Προαπαιτούμενα (στο PC που κάνει build)

- Windows + εγκατεστημένο **Visual Studio IDE**
- Workload/components (συνήθως χρειάζονται):
  - **Desktop development with C++**
  - Windows 10/11 SDK
  - (πολύ πιθανό) **MFC for C++** (τα projects είναι MFC)

Πρακτική σύσταση:
- Κάνε build σε **τοπικό δίσκο** (π.χ. `C:\...`) και όχι σε network drive / removable drive.

## 1) Κάνε αλλαγές στον κώδικα

Άλλαξε τα αρχεία που σε ενδιαφέρουν:

- Calibration: `payload\single_station_DAQ_calibration\` (π.χ. `VCDSO.cpp`, `.h`, thresholds files, κτλ)
- Showers: `payload\single_station_DAQ_showers\`

## 2) Άνοιξε το `.sln` και κάνε build (Visual Studio IDE)

Calibration:

- Άνοιξε: `payload\single_station_DAQ_calibration\VCDSO.sln`
- Βάλε `Debug` και `Win32` **μέσα από το Visual Studio**:
  - Από τα dropdowns στην πάνω μπάρα:
    - `Solution Configurations` → `Debug`
    - `Solution Platforms` → `Win32`
  - Αν δεν τα βλέπεις:
    - `Build -> Configuration Manager...`
    - `Active solution configuration` → `Debug`
    - `Active solution platform` → `Win32` (ή φτιάξε το από `<New...>` αν λείπει)
- Πάτα: `Build -> Build Solution`
- Έλεγξε ότι βγήκε: `payload\single_station_DAQ_calibration\Debug\VCDSO.exe`

Showers:

- Άνοιξε: `payload\single_station_DAQ_showers\VCDSO.sln`
- Βάλε `Debug` και `Win32` **μέσα από το Visual Studio**:
  - Από τα dropdowns στην πάνω μπάρα:
    - `Solution Configurations` → `Debug`
    - `Solution Platforms` → `Win32`
  - Αν δεν τα βλέπεις:
    - `Build -> Configuration Manager...`
    - `Active solution configuration` → `Debug`
    - `Active solution platform` → `Win32` (ή φτιάξε το από `<New...>` αν λείπει)
- Πάτα: `Build -> Build Solution`
- Έλεγξε ότι βγήκε: `payload\single_station_DAQ_showers\Debug\VCDSO.exe`

## 3) Αν θέλεις να φτιάξεις νέο “πακέτο” (zip) για αποστολή/εγκατάσταση σε σταθμό

Αφού έχεις κάνει build και τα outputs υπάρχουν στα `...\Debug\...`:

- Κάνε commit/push τις αλλαγές σου στο GitHub repo.
- Στον δικό σου υπολογιστή (στο root του repo) τρέξε: `build_mnet_station_kit.cmd`
- Θα παραχθεί νέο zip διανομής: `dist\mNetStationKit.zip`

Ο σταθμός/χρήστης μετά ακολουθεί τον οδηγό εγκατάστασης/λειτουργίας: `SETUP_GR.md`.
