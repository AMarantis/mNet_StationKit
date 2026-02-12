# Οδηγίες χρήσης mNetStationKit (Windows 11 – για μη τεχνικούς χρήστες)

Στόχος: να εγκαταστήσεις και να εκκινήσεις τον σταθμό μNet (DAQ + Monitoring) **από USB**, ώστε να πάρεις δεδομένα:

- **Calibration** (για τα Timing Offsets / MIP peaks)
- και μετά **Showers** (online monitoring)

Το kit τρέχει το monitoring με **IIS Express** και χρησιμοποιεί **ROOT** για τα διαγράμματα.

Monitoring URL: `http://localhost:8080/`

Σημείωση για paths (πολύ σημαντικό):

- Το γράμμα του USB stick **διαφέρει** από υπολογιστή σε υπολογιστή (π.χ. `D:`, `E:`, `F:`…).
- Στον οδηγό γράφουμε το USB σαν **`X:`**.
  - Π.χ. `X:\mNetStationKit\scripts\` σημαίνει “στο USB σου, μέσα στον φάκελο `mNetStationKit`, στο `scripts`”.

---

## Τι θα χρειαστείς

- Laptop σταθμού με Windows 11
- Το USB stick (ιδανικά 64GB) μόνιμα κουμπωμένο
- Δικαιώματα Administrator (θα πατήσεις “Yes” σε UAC)
- Σύνδεση Internet (μόνο για να κατέβουν 2–3 installers + ROOT)
- Συνδεδεμένο Hantek (σε θύρα USB του laptop)
- Visual Studio Build Tools 2022 με C++ workload (Desktop development with C++)

---

## 1) Βάλε το kit στο USB

1. Κατέβασε από το GitHub το folder `mNetStationKit` (ZIP download είναι ΟΚ).
2. Κάνε extract **στο root** του USB, ώστε να υπάρχει:

- `X:\mNetStationKit\config\`
- `X:\mNetStationKit\scripts\`
- `X:\mNetStationKit\payload\`
- `X:\mNetStationKit\deps\`

Σημείωση:

- Δεν θέλουμε να καταλήξεις σε **διπλό folder**, π.χ. `X:\mNetStationKit\mNetStationKit\...`
  - Αν συμβεί, είτε:
    - κάνε extract ξανά στο **root** του USB, ή
    - μετέφερε τον “εσωτερικό” φάκελο `mNetStationKit` ένα επίπεδο πάνω.
- Σε κάθε περίπτωση, στο τέλος πρέπει να έχεις **ακριβώς** έναν φάκελο `X:\mNetStationKit\` που μέσα του έχει τους επιμέρους φακέλους `config/`, `deps/`, `payload/`, `scripts/`.

---

## 2) Εγκατάσταση Visual Studio Build Tools 2022 (μία φορά – απαραίτητο)

Αυτό είναι απαραίτητο για να δουλεύουν τα plots στο UI (το ROOT/Cling χρειάζεται MSVC headers/Windows SDK).

1) Κατέβασε τον installer `vs_BuildTools.exe` από το επίσημο link:

- `https://aka.ms/vs/17/release/vs_BuildTools.exe`

2) Τρέξε το `vs_BuildTools.exe` (αν σε ρωτήσει UAC πάτα **Yes**).
2) Στο tab Workloads:
   - επίλεξε ΜΟΝΟ: Desktop development with C++
3) Στο tab Installation locations:
   - βάλε Download cache σε fixed δίσκο (στον `C:`), π.χ. `C:\VSCache`
   - (προαιρετικό) βγάλε το “Keep download cache after the installation” για να μη σου τρώει χώρο
4) Πάτα Install και περίμενε να ολοκληρωθεί

---

## 3) Install dependencies (μία φορά – προτείνεται)

Αυτό είναι το βήμα που αντικαθιστά τα “χειροκίνητα downloads / εγκαταστάσεις”.

1) Πήγαινε: `X:\mNetStationKit\scripts\`
2) Δεξί κλικ `Install-Dependencies.cmd` → **Run as administrator**

Το script ελέγχει και (αν λείπουν) κατεβάζει/εγκαθιστά:
- **Microsoft Visual C++ Redistributable (x64)**
- **IIS Express**
- **ROOT** (είτε portable `.zip` είτε installer `.exe`, ανάλογα με το URL στο `config\station.json`)
- (έλεγχο) ότι υπάρχει C++ toolchain / Windows SDK ώστε να παράγονται τα ROOT plots σε Windows

Σημείωση για ROOT: αν κατεβαίνει ως installer `.exe`, μπορεί να ανοίξει παράθυρο εγκατάστασης.  
Απλά ακολούθησε τα default βήματα (Next/Install). Μετά το script θα προσπαθήσει να βρει το `root.exe`.

### 3.1 Αν θες “offline” (χωρίς internet)

Μπορείς να βάλεις installers/zip μέσα στο:

- `X:\mNetStationKit\deps\installers\`

και μετά να τρέξεις πάλι `Install-Dependencies.cmd`.  
(Εναλλακτικά, βάλε `allowOnlineDownloads=false` στο `config\station.json`.)

### 3.2 Αν προτιμάς “χειροκίνητα” (fallback)

Αν δεν θες να τρέξεις το `Install-Dependencies.cmd`, τότε πρέπει να ισχύουν:

- IIS Express installed:
  - `C:\Program Files\IIS Express\iisexpress.exe`
- VC++ redist installed (x64)
- ROOT διαθέσιμο με έναν από τους δύο τρόπους:
  - Portable: `X:\mNetStationKit\deps\root\bin\root.exe`
  - Ή system install: κάπου σε `C:\root_v*\bin\root.exe`

---

## 4) One‑time setup του σταθμού (Admin)

Πήγαινε στο:

- `X:\mNetStationKit\scripts\`

και κάνε:

- δεξί κλικ `Setup-Admin.cmd` → **Run as administrator**

Αυτό κάνει:

- Αν **δεν** υπάρχει πραγματικό `D:`, φτιάχνει spool folder **στο USB** (μέσα στο kit): `X:\mNetStationKit\mNetSpool\`
- Εξασφαλίζει ότι υπάρχει `D:` για τα data paths:
  - Στον οδηγό, το `D:` είναι το “data drive” που περιμένει ο DAQ κώδικας (μπορεί να είναι πραγματικός δίσκος ή virtual drive μέσω `subst`).
  - Αν υπάρχει ήδη πραγματικό `D:` (π.χ. “Virtual_D”), το χρησιμοποιεί.
  - Αλλιώς δημιουργεί “virtual D:” με `subst` που δείχνει στο USB spool folder (π.χ. `X:\mNetStationKit\mNetSpool\`).
- Φτιάχνει τους φακέλους δεδομένων:
  - `D:\Save_Pulses_Calibration_Phase2\`
  - `D:\Save_Pulses_Showers_Phase2\`
  - `D:\Save_Pulses_Showers_Rec_Phase2\`
- Αντιγράφει τα προ‑compiled DAQ αρχεία μέσα στα `D:\Save_Pulses_*` (για να γράφει εκεί που περιμένει ο κώδικας)

### 4.1 (Προαιρετικό αλλά προτείνεται) Φτιάξε “κουμπιά” στο Desktop

Από `X:\mNetStationKit\scripts\`:

- τρέξε `Create-Desktop-Shortcuts.cmd`

Θα φτιάξει στο Desktop shortcuts που δουλεύουν ακόμα κι αν το USB αλλάξει drive letter (D:, E:, F:, κτλ).

---

## 5) Πριν πάρεις δεδομένα: βάλε σωστά τις συντεταγμένες

Άνοιξε το αρχείο:

- `X:\mNetStationKit\payload\single_stationOnline_Monitoring\ProgramFiles\positions.txt`

και σιγουρέψου ότι έχει τις σωστές συντεταγμένες για τους 3 ανιχνευτές του σταθμού.

---

## 6) Calibration (λήψη + plots)

### 6.1 Ξεκίνα το calibration DAQ

Από `X:\mNetStationKit\scripts\`:

- τρέξε `Start-DAQ-Calibration.cmd`

Εναλλακτικά (αν έχεις φτιάξει shortcuts στο Desktop από το 4.1):

- πάτα `mNetStationKit - Start DAQ Calibration`

Έλεγχος ότι “γράφει”:

- Μπες στο `D:\Save_Pulses_Calibration_Phase2\`
- Πρέπει να εμφανίζονται αρχεία τύπου:
  - `detectorid_year_month_day_hour.data`

### 6.2 Ξεκίνα το Monitoring UI

Από `X:\mNetStationKit\scripts\`:

- τρέξε `Start-Monitoring.cmd`

Εναλλακτικά (αν έχεις φτιάξει shortcuts στο Desktop από το 4.1):

- πάτα `mNetStationKit - Start Monitoring`

Το script ανοίγει αυτόματα τον default browser στο:

- `http://localhost:8080/`

Σημείωση:
- Μερικές φορές στην **πρώτη** εκκίνηση μπορεί να ανοίξει ο browser αλλά η σελίδα να “φορτώνει” χωρίς να δείχνει UI.
  - Περίμενε ~10–30 δευτερόλεπτα και πάτα `Reload` (`Ctrl+R`).

### 6.3 Στο UI (Calibration)

1. Επίλεξε **σταθμό 4** (είναι ο τοπικός).
2. Πήγαινε tab **Calibration**.
3. Sub‑tab **Detector Response** → πάτα **START**.
   - Περίμενε να εμφανιστεί pop‑up:
     - `Response: Acquisition starting.`
   - Πάτα **OK**.
4. Sub‑tab **Synchronization** → πάτα **START**.
   - Περίμενε να εμφανιστεί pop‑up:
     - `Synchronization: Acquisition starting.`
   - Πάτα **OK**.

Αποτελέσματα που θα δεις:

- Δημιουργείται φάκελος κάτω από:
  - `X:\mNetStationKit\payload\single_stationOnline_Monitoring\App_Data\calibration_key_detectorid_year_month_day_hour_minutes_seconds`
- Από τα plots (και τα αρχεία εικόνας που παράγονται) παίρνεις:
  - **MIP mean peak 1, 2, 3** (μέσα ύψη παλμού για κάθε ανιχνευτή)
  - **Timing Offset 1, 2** (offset timings)

### 6.4 Πού αποθηκεύονται τα αρχεία του Calibration (paths)

Raw outputs (ανά session) γράφονται εδώ:

- `X:\mNetStationKit\payload\single_stationOnline_Monitoring\App_Data\`
  - `calibration_<SessionId>_<Station>_<YYYY_M_D_H_m_s>\`

Στον παραπάνω φάκελο θα δεις συνήθως:

- `outroot.jpg` (Detector Response)
- `outroot2.jpg` (Synchronization)
- `info_response.txt`, `info_timing.txt`
- `timing.txt`
- `test1.txt`, `test2.txt`, `test3.txt`
- `pulse1.txt`, `pulse2.txt`, `pulse3.txt`

Οι εικόνες που “σερβίρει” το site (δηλαδή αυτές που βλέπει ο browser) είναι εδώ:

- `X:\mNetStationKit\payload\single_stationOnline_Monitoring\images\`
  - `outroot_<SessionId>.jpg`
  - `outroot2_<SessionId>.jpg`

### 6.5 Πριν πας σε Showers: σταμάτα το Calibration

Πριν ξεκινήσεις showers, **πρέπει** να έχει σταματήσει το calibration (δεν πρέπει να τρέχουν μαζί).

Σημείωση (σημαντικό για αργότερα):

- Κράτα κάπου (π.χ. σε σημείωση / screenshot) τις τιμές:
  - **Timing Offset 1 & 2**
  - **MIP mean peak 1–3**
- Θα τις χρειαστείς στο **Online Monitoring → Telescope Parameters** στο βήμα **7.2**.
- Αν αλλάξεις tab / κάνεις refresh, είναι πιθανό τα plots του Calibration να “χαθούν” από το UI.
  - Μπορείς να τα ξαναβρείς από τις εικόνες που παράγονται:
    - `X:\mNetStationKit\payload\single_stationOnline_Monitoring\images\outroot_<SessionId>.jpg` (όπου `X:` είναι το USB drive, π.χ. `D:`)
    - `X:\mNetStationKit\payload\single_stationOnline_Monitoring\images\outroot2_<SessionId>.jpg` (όπου `X:` είναι το USB drive, π.χ. `D:`)

1) Στο UI (Calibration):
- Πάτα **STOP** στο sub‑tab **Detector Response** (αν τρέχει).
- Πάτα **STOP** στο sub‑tab **Synchronization** (αν τρέχει).

2) Σταμάτα το DAQ process:
- Τρέξε `scripts/Stop-DAQ.cmd`
  - Εναλλακτικά (αν έχεις shortcuts): πάτα `mNetStationKit - Stop DAQ`

Σημείωση:
- Δεν χρειάζεται να κλείσεις τον browser. Απλά μετά πήγαινε tab **Online Monitoring** για το showers.

---

## 7) Showers (λήψη + online monitoring)

### 7.1 Ξεκίνα το showers DAQ

Από `X:\mNetStationKit\scripts\`:

- τρέξε `Start-DAQ-Showers.cmd`

Εναλλακτικά (αν έχεις φτιάξει shortcuts στο Desktop από το 4.1):

- πάτα `mNetStationKit - Start DAQ Showers`

Έλεγχος ότι “γράφει”:

- Μπες στο `D:\Save_Pulses_Showers_Phase2\`
- Πρέπει να εμφανίζονται αρχεία τύπου:
  - `detectorid_year_month_day_hour.showerdata`

### 7.2 Στο UI (Online Monitoring)

1. Άνοιξε `http://localhost:8080/` (αν δεν είναι ήδη ανοιχτό).
2. Επίλεξε **σταθμό 4**.
3. Πήγαινε tab **Online Monitoring**.
4. Sub‑tab **Telescope Parameters**:
   - Βάλε τα **Timing Offset 1 & 2**
   - Βάλε τα **MIP mean peak 1–3** (από το calibration)
   - Πάτα **START**

### 7.3 Πού αποθηκεύονται τα αρχεία του Showers Monitoring (paths)

Raw outputs (ανά session) γράφονται εδώ:

- `X:\mNetStationKit\payload\single_stationOnline_Monitoring\App_Data\`
  - `shower_<SessionId>_<Station>_<YYYY_M_D_H_m_s>\`

Στον παραπάνω φάκελο θα δεις συνήθως:

- `info_shower.txt` (οι παράμετροι που έβαλες + start time + station)
- `events.txt`
- `pulses1.jpg`
- `plots.jpg`

Οι εικόνες που “σερβίρει” το site (δηλαδή αυτές που βλέπει ο browser) είναι εδώ:

- `X:\mNetStationKit\payload\single_stationOnline_Monitoring\images\`
  - `pulses_<SessionId>.jpg`
  - `plots_<SessionId>.jpg`

Reconstructed events μπορεί να γράφονται και στο:

- `D:\Save_Pulses_Showers_Rec_Phase2\`
  - `events_<Station>_<YYYY>_<M>_<D>_<H>*`

### 7.4 Πώς σταματάς τα showers και πώς “κλείνεις” το UI

Όταν τελειώσεις το showers run:

1) Στο UI:
- Στο tab **Online Monitoring** → sub‑tab **Telescope Parameters**:
  - Πάτα **STOP**
  - Περίμενε να εμφανιστεί pop‑up:
    - `Acquisition stopped. To resume, press start.`
  - Πάτα **OK**

Σημείωση:
- Το **STOP** στο UI σταματάει το *online monitoring acquisition/plots* (δηλαδή το διάβασμα/ενημέρωση στο site).
- Για να σταματήσει και η **λήψη δεδομένων**, πρέπει να σταματήσεις και το DAQ (`VCDSO.exe`), όπως στο επόμενο βήμα.

2) Σταμάτα το DAQ:
- Τρέξε `scripts/Stop-DAQ.cmd`
  - Εναλλακτικά (αν έχεις shortcuts): πάτα `mNetStationKit - Stop DAQ`

3) Κλείσε το Monitoring UI (IIS Express):
- Τρέξε `scripts/Stop-Monitoring.cmd`
  - Εναλλακτικά (αν έχεις shortcuts): πάτα `mNetStationKit - Stop Monitoring`

4) Κλείσε τον browser (προαιρετικό):
- Κλείσε απλά το tab/παράθυρο.

---

## Σημαντικές παρατηρήσεις / κοινά προβλήματα

### A) USB + Virtual D: μετά από reboot

- Το “virtual D:” από `subst` μπορεί να χαθεί μετά από restart.
- Αν συμβεί:
  - απλά πάτα `Start-Monitoring.cmd` ή `Start-DAQ-Calibration.cmd` ή `Start-DAQ-Showers.cmd` (τα scripts ξαναφτιάχνουν το D: όταν λείπει).
  - έλεγξε ότι υπάρχουν οι φάκελοι:
    - `D:\Save_Pulses_Calibration_Phase2\`
    - `D:\Save_Pulses_Showers_Phase2\`
    - `D:\Save_Pulses_Showers_Rec_Phase2\`
  - αν το D: δεν εμφανιστεί/δεν φτιαχτούν οι φάκελοι, τρέξε `Setup-Admin.cmd` ως Admin και ξαναδοκίμασε.

### B) Μην τρέχει calibration και showers μαζί

- Δεν πρέπει να τρέχουν ταυτόχρονα.
- Αν ξεκινήσεις το ένα ενώ τρέχει το άλλο, κλείσε το άλλο πρώτα.
- Για γρήγορο κλείσιμο: τρέξε `scripts/Stop-DAQ.cmd`.
  - Εναλλακτικά (αν έχεις shortcuts): πάτα `mNetStationKit - Stop DAQ`.

### C) Decimal separator (πολύ σημαντικό)

Στα Windows με Ελληνικά regional settings, η υποδιαστολή είναι συχνά `,` και δημιουργεί πρόβλημα.

Ρύθμιση:

1. Άνοιξε **Control Panel** → **Region**.
2. Tab **Formats** → **Additional settings…**
3. Βρες **Decimal symbol** και βάλ’ το `.` (τελεία).

### D) Hantek latency / restart κάθε ~1 ώρα

Το kit έχει watchdog που κάνει **αυτόματο restart** του DAQ (`VCDSO.exe`) περίπου κάθε ~1 ώρα (όσο γράφονται δεδομένα), ώστε να αποφεύγεται το “μπούκωμα”/latency.

Σημειώσεις:
- Αυτό δουλεύει **μόνο** αν ξεκινάς το DAQ με τα scripts/shortcuts του kit (`Start-DAQ-Calibration` / `Start-DAQ-Showers`). Αν τρέξεις `VCDSO.exe` “με το χέρι”, δεν ξεκινά watchdog.
- Για λεπτομέρειες/λογική/ασφάλειες δες: `WATCHDOG_DAQ_RESTART_GR.md`.

Αν παρ’ όλα αυτά δεις latency και θες να ελέγξεις τι έγινε:
- Άνοιξε το log: `logs\restart_daq_watchdog.log`
- (fallback) κάνε manual restart:
  - `scripts/Stop-DAQ.cmd`
  - μετά `Start-DAQ-Calibration.cmd` ή `Start-DAQ-Showers.cmd`

### E) Error για `bin\roslyn\csc.exe` (monitoring δεν ανοίγει)

Αν στο `http://localhost:8080/` δεις μήνυμα τύπου “Could not find … `bin\roslyn\csc.exe`”, τότε:

1) Έλεγξε ότι υπάρχει ο φάκελος:
   - `X:\mNetStationKit\payload\single_stationOnline_Monitoring\bin\roslyn\`
   και μέσα υπάρχουν `csc.exe`, `VBCSCompiler.exe`, `vbc.exe`.
2) Αν “εξαφανίστηκαν”, συνήθως τα έβγαλε το Windows Security/Defender:
   - Windows Security → Virus & threat protection → Protection history → Restore/Allow
   - (προαιρετικά) πρόσθεσε Exclusion για:
     - `X:\mNetStationKit\payload\single_stationOnline_Monitoring\bin\roslyn`

---

## Troubleshooting / Logs

Αν κάτι δεν δουλεύει και θες να το στείλεις για διάγνωση:

- Τρέξε `scripts/Collect-Logs.cmd`
- Θα φτιάξει ZIP μέσα στο `logs/` στο kit

Χρήσιμες default τοποθεσίες για IIS Express logs (συνήθως):

- `C:\Users\<USERNAME>\Documents\IISExpress\Logs\`
- `C:\Users\<USERNAME>\Documents\IISExpress\config\`
