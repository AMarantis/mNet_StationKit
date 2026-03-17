# Troubleshooting / Known Bugs (mNetStationKit)

Το αρχείο αυτό κρατάει γνωστά προβλήματα, διάγνωση και τι έχει ήδη διορθωθεί.

---

## Commit 1 που έγινε ήδη (hash `3f29c25`)

Τίτλος commit: `Monitoring: robust parsing + runtime logs (no physics change)`

### Τι αλλάξαμε

- Στο `payload/single_stationOnline_Monitoring/OnlineMonitoring.aspx.cs` προστέθηκε robust parsing (safe checks πριν από fixed-width parsing).
- Τα malformed/partial records δεν ρίχνουν πλέον άμεσα το UI· γίνονται skip/recover και συνεχίζει η ροή.
- Προστέθηκαν runtime logs για monitoring:
  - `X:\mNetStationKit\logs\monitoring_errors.log`
  - `X:\mNetStationKit\logs\monitoring_warnings.log`
- Προστέθηκε ασφαλέστερη διαχείριση stream position/alignment για resync σε έγκυρο boundary event.

### Τι **δεν** αλλάξαμε

- Δεν άλλαξε ο DAQ κώδικας (`VCDSO.exe` calibration/showers).
- Δεν άλλαξε το format που γράφεται στα `*.data` / `*.showerdata`.
- Δεν άλλαξε η “φυσική” (αλγόριθμοι υπολογισμού/plots/reconstruction).

---

## Bug: “Index and length must refer to a location within the string”

### Συμπτώματα

- Στο `http://localhost:8080/OnlineMonitoring` εμφανίζεται ASP.NET server error.
- Exception: `System.ArgumentOutOfRangeException` με `Parameter name: length`.
- Stack trace συνήθως δείχνει `Read_events()` στο `OnlineMonitoring.aspx.cs`.

### Ρίζα προβλήματος

Το monitoring διαβάζει live αρχείο `*.showerdata` την ώρα που το DAQ γράφει. Αν διαβαστεί μισογραμμένη/κατεστραμμένη γραμμή, το fixed-width parsing μπορεί να αποτύχει.

### Σχέση με χώρο στον `C:`

- Δεν είναι η βασική αιτία του συγκεκριμένου exception.
- Όμως πολύ χαμηλός χώρος (`~MB`) αυξάνει γενικά την αστάθεια. Στόχος: τουλάχιστον `1–2 GB` ελεύθερα.

---

## Τι ελέγχουμε όταν συμβεί

- Επιβεβαίωση ότι ο DAQ συνεχίζει:
  - `<kit>\mNetSpool\Save_Pulses_Showers_Phase2\*.showerdata`
- Logs monitoring:
  - `X:\mNetStationKit\logs\monitoring_errors.log`
  - `X:\mNetStationKit\logs\monitoring_warnings.log`
- Logs watchdog:
  - `X:\mNetStationKit\logs\restart_daq_watchdog.log`
- Συλλογή πλήρους πακέτου:
  - `scripts/Collect-Logs.cmd`
  - Παράγει `X:\mNetStationKit\logs\mnet_logs_*.zip`

---

## Άμεση αποκατάσταση στον σταθμό

- `scripts/Stop-Monitoring.cmd`
- `scripts/Stop-DAQ.cmd`
- `scripts/Start-Monitoring.cmd`
- `scripts/Start-DAQ-Showers.cmd`

Αν υπάρχει malformed record στο τρέχον αρχείο ώρας, η επανεκκίνηση συνήθως επαναφέρει σωστή ευθυγράμμιση.

---

## Commit 2 (calibration summary file)

### Τι υλοποιήθηκε

- Στο STOP του calibration παράγεται summary αρχείο:
  - `X:\mNetStationKit\payload\single_stationOnline_Monitoring\App_Data\last_calibration_summary.txt`
- Το summary περιέχει αυτόματα:
  - `offset1`, `offset2`, `offset3`
  - `peak1`, `peak2`, `peak3`
  - `station`, `calibration_folder`, `generated_at_utc`
- Το summary κρατιέται ως reference αρχείο για τον operator.
- Τα `Telescope Parameters` στο `Online Monitoring` δεν πρέπει να αλλάζουν αυτόματα από το summary.

### Κανόνας invalid τιμών

- Τιμές τύπου `9999` / `-9999` (και γενικά `abs(value) >= 9000`) θεωρούνται invalid και αγνοούνται από τον υπολογισμό means.
- Αν το summary δεν έχει έγκυρες τιμές, δεν σπάει το UI.

---

## Commit 3 (calibration start latency + reconstruction regression)

### Τι αλλάξαμε

- Στο calibration `START` δεν γίνεται πλέον blocking read/plot generation μέσα στο ίδιο request.
- Ο calibration reader ευθυγραμμίζεται πλέον στο τελευταίο πλήρες event του τρέχοντος `*.data` file, αντί να μπορεί να γυρίσει σχεδόν στην αρχή του αρχείου όταν υπάρχει partial tail.
- Το πρώτο calibration polling γίνεται γρήγορα μετά το `START`, ώστε το run να ξεκινά άμεσα από πλευράς UI.
- Το `last_calibration_summary.txt` δεν ξαναγράφει αυτόματα timing offsets στο `Online Monitoring`, ώστε το showers reconstruction να μην αλλάζει behaviour σε σχέση με τον παλιό κώδικα.

---

## SmartScreen block σε `VCDSO.exe` / scripts

### Συμπτώματα

- Μήνυμα Windows: “Τα Windows προστάτευσαν τον υπολογιστή σας”.
- Σε script launch μπορεί να εμφανιστεί `Start-Process ... Η λειτουργία ακυρώθηκε από το χρήστη`.

### Τι κάνουμε

- Στο prompt πάτα: **Περισσότερες πληροφορίες** → **Εκτέλεση ούτως ή άλλως**.
- Αν επιμένει, τρέξε PowerShell ως Admin και κάνε:
  - `Get-ChildItem "C:\mNetStationKit" -Recurse -File | Unblock-File`
