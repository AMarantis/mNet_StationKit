# Οδηγίες χρήσης mNetStationKit (Windows 11 – για μη τεχνικούς χρήστες)

Στόχος: να τρέξει ο σταθμός μNet **όπως τρέχει σήμερα**, αλλά χωρίς τα χειροκίνητα βήματα:
- CMake / Python / Visual Studio
- ρυθμίσεις Windows IIS / publish σε `C:\inetpub\wwwroot`

Το kit τρέχει το monitoring με **IIS Express** και χρησιμοποιεί **ROOT** για τα διαγράμματα.

Monitoring URL: `http://localhost:8080/`

---

## Τι θα χρειαστείς

- Laptop σταθμού με Windows 11
- Το USB stick (ιδανικά 64GB) μόνιμα κουμπωμένο
- Δικαιώματα Administrator (θα πατήσεις “Yes” σε UAC)
- Σύνδεση Internet (μόνο για να κατέβουν 2–3 installers + ROOT)
- Συνδεδεμένο Hantek (σε θύρα USB του laptop)

---

## 1) Βάλε το kit στο USB

1. Κατέβασε από το GitHub το folder `mNetStationKit` (ZIP download είναι ΟΚ).
2. Κάνε extract **στο root** του USB, ώστε να υπάρχει:

- `E:\mNetStationKit\`
- `E:\mNetStationKit\scripts\`
- `E:\mNetStationKit\payload\`
- `E:\mNetStationKit\deps\`

Σημείωση: Αν το USB δεν είναι `E:`, δεν πειράζει. Τα scripts τρέχουν από το folder που κάνεις click.

---

## 2) Αυτόματη εγκατάσταση dependencies (μία φορά – προτείνεται)

Αυτό είναι το βήμα που αντικαθιστά τα “χειροκίνητα downloads / εγκαταστάσεις”.

1) Πήγαινε: `E:\mNetStationKit\scripts\`
2) Δεξί κλικ `Install-Dependencies.cmd` → **Run as administrator**

Το script ελέγχει και (αν λείπουν) κατεβάζει/εγκαθιστά:
- **Microsoft Visual C++ Redistributable (x64)**
- **IIS Express**
- **ROOT** (είτε portable `.zip` είτε installer `.exe`, ανάλογα με το URL στο `config\station.json`)
- **Visual Studio Build Tools (C++ toolchain + Windows SDK)** (απαραίτητο για να παράγονται τα plots από το ROOT σε Windows)

Σημείωση για ROOT: αν κατεβαίνει ως installer `.exe`, μπορεί να ανοίξει παράθυρο εγκατάστασης.  
Απλά ακολούθησε τα default βήματα (Next/Install). Μετά το script θα προσπαθήσει να βρει το `root.exe`.

### 2.1 Αν θες “offline” (χωρίς internet)

Μπορείς να βάλεις installers/zip μέσα στο:

- `E:\mNetStationKit\deps\installers\`

και μετά να τρέξεις πάλι `Install-Dependencies.cmd`.  
(Εναλλακτικά, βάλε `allowOnlineDownloads=false` στο `config\station.json`.)

### 2.2 Αν προτιμάς “χειροκίνητα” (fallback)

Αν δεν θες να τρέξεις το `Install-Dependencies.cmd`, τότε πρέπει να ισχύουν:

- IIS Express installed:
  - `C:\Program Files\IIS Express\iisexpress.exe`
- VC++ redist installed (x64)
- ROOT διαθέσιμο με έναν από τους δύο τρόπους:
  - Portable: `E:\mNetStationKit\deps\root\bin\root.exe`
  - Ή system install: κάπου σε `C:\root_v*\bin\root.exe`

---

## 3) One‑time setup του σταθμού (Admin)

Πήγαινε στο:

- `E:\mNetStationKit\scripts\`

και κάνε:

- δεξί κλικ `Setup-Admin.cmd` → **Run as administrator**

Αυτό κάνει:

- Φτιάχνει local cache: `C:\mNetSpool` (περίπου 1–2GB χώρος είναι αρκετός)
- Εξασφαλίζει ότι υπάρχει `D:` για τα data paths:
  - Αν υπάρχει ήδη πραγματικό `D:` (π.χ. “Virtual_D”), το χρησιμοποιεί.
  - Αλλιώς δημιουργεί “virtual D:” με `subst` που δείχνει στο `C:\mNetSpool`.
- Φτιάχνει τους φακέλους δεδομένων:
  - `D:\Save_Pulses_Calibration_Phase2\`
  - `D:\Save_Pulses_Showers_Phase2\`
  - `D:\Save_Pulses_Showers_Rec_Phase2\`
- Αντιγράφει τα προ‑compiled DAQ αρχεία μέσα στα `D:\Save_Pulses_*` (για να γράφει εκεί που περιμένει ο κώδικας)

---

## 4) Πριν πάρεις δεδομένα: βάλε σωστά τις συντεταγμένες

Άνοιξε το αρχείο:

- `E:\mNetStationKit\payload\single_stationOnline_Monitoring\ProgramFiles\positions.txt`

και σιγουρέψου ότι έχει τις σωστές συντεταγμένες για τους 3 ανιχνευτές του σταθμού.

---

## 5) Calibration (λήψη + plots)

### 5.1 Ξεκίνα το calibration DAQ

Από `E:\mNetStationKit\scripts\`:

- τρέξε `Start-DAQ-Calibration.cmd`

Έλεγχος ότι “γράφει”:

- Μπες στο `D:\Save_Pulses_Calibration_Phase2\`
- Πρέπει να εμφανίζονται αρχεία τύπου:
  - `detectorid_year_month_day_hour.data`

### 5.2 Ξεκίνα το Monitoring UI

Από `E:\mNetStationKit\scripts\`:

- τρέξε `Start-Monitoring.cmd`

Άνοιξε browser:

- `http://localhost:8080/`

### 5.3 Στο UI (Calibration)

1. Επίλεξε **σταθμό 4** (είναι ο τοπικός).
2. Πήγαινε tab **Calibration**.
3. Sub‑tab **Detector Response** → πάτα **START**.
4. Sub‑tab **Synchronization** → πάτα **START**.

Αποτελέσματα που θα δεις:

- Δημιουργείται φάκελος κάτω από:
  - `E:\mNetStationKit\payload\single_stationOnline_Monitoring\App_Data\calibration_key_detectorid_year_month_day_hour_minutes_seconds`
- Από τα plots (και τα αρχεία εικόνας που παράγονται) παίρνεις:
  - **MIP mean peak 1, 2, 3** (μέσα ύψη παλμού για κάθε ανιχνευτή)
  - **Timing Offset 1, 2** (offset timings)

---

## 6) Showers (λήψη + online monitoring)

### 6.1 Ξεκίνα το showers DAQ

Από `E:\mNetStationKit\scripts\`:

- τρέξε `Start-DAQ-Showers.cmd`

Έλεγχος ότι “γράφει”:

- Μπες στο `D:\Save_Pulses_Showers_Phase2\`
- Πρέπει να εμφανίζονται αρχεία τύπου:
  - `detectorid_year_month_day_hour.showerdata`

### 6.2 Στο UI (Online Monitoring)

1. Άνοιξε `http://localhost:8080/` (αν δεν είναι ήδη ανοιχτό).
2. Επίλεξε **σταθμό 4**.
3. Πήγαινε tab **Online Monitoring**.
4. Sub‑tab **Telescope Parameters**:
   - Βάλε τα **Timing Offset 1 & 2**
   - Βάλε τα **MIP mean peak 1–3** (από το calibration)
   - Πάτα **START**

---

## Σημαντικές παρατηρήσεις / κοινά προβλήματα

### A) USB + Virtual D: μετά από reboot

- Το “virtual D:” από `subst` μπορεί να χαθεί μετά από restart.
- Αν συμβεί:
  - τρέξε ξανά `Start-Monitoring.cmd` ή/και `Setup-Admin.cmd` (ως Admin).

### B) Μην τρέχει calibration και showers μαζί

- Δεν πρέπει να τρέχουν ταυτόχρονα.
- Αν ξεκινήσεις το ένα ενώ τρέχει το άλλο, κλείσε το άλλο πρώτα.
- Για γρήγορο κλείσιμο: τρέξε `scripts/Stop-DAQ.cmd`.

### C) Decimal separator (πολύ σημαντικό)

Στα Windows με Ελληνικά regional settings, η υποδιαστολή είναι συχνά `,` και δημιουργεί πρόβλημα.

Ρύθμιση:

1. Άνοιξε **Control Panel** → **Region**.
2. Tab **Formats** → **Additional settings…**
3. Βρες **Decimal symbol** και βάλ’ το `.` (τελεία).

### D) Hantek latency / restart κάθε ~1 ώρα

Αν μετά από ώρα αρχίζει latency:

- Κλείσε και ξανάνοιξε το DAQ:
  - `scripts/Stop-DAQ.cmd`
  - μετά `Start-DAQ-Calibration.cmd` ή `Start-DAQ-Showers.cmd`

### E) Error για `bin\roslyn\csc.exe` (monitoring δεν ανοίγει)

Αν στο `http://localhost:8080/` δεις μήνυμα τύπου “Could not find … `bin\roslyn\csc.exe`”, τότε:

1) Έλεγξε ότι υπάρχει ο φάκελος:
   - `E:\mNetStationKit\payload\single_stationOnline_Monitoring\bin\roslyn\`
   και μέσα υπάρχουν `csc.exe`, `VBCSCompiler.exe`, `vbc.exe`.
2) Αν “εξαφανίστηκαν”, συνήθως τα έβγαλε το Windows Security/Defender:
   - Windows Security → Virus & threat protection → Protection history → Restore/Allow
   - (προαιρετικά) πρόσθεσε Exclusion για:
     - `E:\mNetStationKit\payload\single_stationOnline_Monitoring\bin\roslyn`

---

## Troubleshooting / Logs

Αν κάτι δεν δουλεύει και θες να το στείλεις για διάγνωση:

- Τρέξε `scripts/Collect-Logs.cmd`
- Θα φτιάξει ZIP μέσα στο `logs/` στο kit
