# Troubleshooting / Known Bugs (mNetStationKit)

Το αρχείο αυτό κρατάει **γνωστά προβλήματα** που έχουν εμφανιστεί σε σταθμούς, μαζί με **πιθανές αιτίες** και **προτεινόμενες λύσεις**.

---

## Bug: “Server Error in '/' Application” στο Online Monitoring μετά από πολλές ώρες

### Συμπτώματα

- Στο browser (π.χ. `http://localhost:8080/OnlineMonitoring`) εμφανίζεται “Server Error in '/' Application”.
- Το μήνυμα περιλαμβάνει:
  - `System.ArgumentOutOfRangeException: Index and length must refer to a location within the string. Parameter name: length`
- Stack trace δείχνει συνήθως:
  - `OnlineMonitoring.aspx.cs` μέσα σε `Read_events()`
  - σε γραμμές τύπου `line.Substring(...)` (fixed-width parsing).

### Πιθανή αιτία (most likely)

Ο κώδικας του monitoring διαβάζει το `*.showerdata` αρχείο **την ίδια στιγμή** που το DAQ (`VCDSO.exe`) το γράφει. Αν “πιάσει”:

- κενή γραμμή,
- ή μερικώς γραμμένη γραμμή (partial write),
- ή κακή ευθυγράμμιση record (π.χ. μετά από restart/kill του DAQ),

τότε το `Substring` σε fixed θέσεις αποτυγχάνει και ρίχνει exception → το UI σκάει.

### Σχετίζεται με πολύ λίγο free space στον C:;

- **Δεν είναι η άμεση αιτία** του συγκεκριμένου `Substring` exception.
- Όμως, αν ο `C:` έχει μείνει με **ελάχιστο χώρο** (π.χ. ~12MB), μπορεί να προκαλέσει δευτερογενή προβλήματα (temp files, caching, αστάθεια υπηρεσιών/διεργασιών).
- Για σταθερή λειτουργία, κρατάμε πρακτικά στόχο: **>= 1–2 GB ελεύθερα** στον `C:` (ή περισσότερο).

---

## Άμεση αποκατάσταση (στον σταθμό)

Για να επανέλθει το monitoring όταν εμφανιστεί το “Server Error”:

- 1) Ελευθέρωσε χώρο στον `C:` (στόχος: >= 1–2 GB free).
- 2) Κλείσε/επανεκκίνησε τη ροή:
  - `scripts/Stop-Monitoring.cmd`
  - `scripts/Stop-DAQ.cmd`
  - `scripts/Start-Monitoring.cmd`
  - `scripts/Start-DAQ-Showers.cmd`

Σημείωση:
- Αν υπάρχει “κομμένο”/corrupt record στο τρέχον αρχείο της ώρας, η επανεκκίνηση συνήθως μετακινεί την ανάγνωση σε νέο/σωστό σημείο.

---

## Γρήγορη διάγνωση (τι να μαζέψεις)

- 1) Έλεγξε ότι γράφονται δεδομένα:
  - `D:\Save_Pulses_Showers_Phase2\*.showerdata`
  - `D:\Save_Pulses_Showers_Rec_Phase2\*` (αν γράφει reconstructed events)
- 2) Έλεγξε τα kit logs:
  - `X:\mNetStationKit\logs\restart_daq_watchdog.log`
- 3) Πάρε πακέτο logs:
  - τρέξε `scripts/Collect-Logs.cmd`
  - θα γράψει zip στο `X:\mNetStationKit\logs\mnet_logs_*.zip`

---

## Προτεινόμενη μόνιμη λύση (fix στον κώδικα του monitoring)

Στόχος: να μην “ρίχνει” όλο το UI όταν συναντήσει malformed/partial γραμμή σε live mode.

### Option A (προτεινόμενο): “Robust parsing” με checks

Στη `Read_events()` (στο `payload/single_stationOnline_Monitoring/OnlineMonitoring.aspx.cs`) όπου γίνεται:

- `line.Substring(0, 5)`
- `line.Substring(6, 5)`
- `line.Substring(12, 5)`
- `line.Substring(18)`

να μπει πριν:

- `if (string.IsNullOrWhiteSpace(line) || line.Length < 19) { Set_File_Position(); return evts_read; }`

και όπου αλλού υπάρχει fixed-width parsing.

Αυτό κάνει το UI “self-healing”: αν πιάσει partial write, περιμένει το επόμενο tick.

### Option B: “Skip malformed event” (χωρίς Set_File_Position)

Αν βρεθεί malformed γραμμή, αντί να γίνεται throw:

- να γίνεται `continue` (skip) ή να μετακινεί το stream σε ασφαλές boundary.

Θέλει προσοχή γιατί μπορεί να “χαθεί” συγχρονισμός του record format.

### Option C: “ReadAll mode only” workaround

Σαν workaround, αν σε κάποιες περιπτώσεις το live mode είναι εύθραυστο, να επιτρέπεται λειτουργία που διαβάζει μόνο πλήρως κλεισμένα αρχεία (π.χ. προηγούμενης ώρας).

Μειονέκτημα: χάνει “real-time” αίσθηση.

---

## Watchdog interaction (πιθανό σενάριο)

Αν το watchdog κάνει restart το `VCDSO.exe` ακριβώς τη στιγμή που γράφει, μπορεί να αφήσει partial record.

Mitigations:

- Μικρή καθυστέρηση πριν restart (υπάρχει ήδη μικρό sleep).
- Έλεγχος “recent write” υπάρχει ήδη· ωστόσο δεν εγγυάται ότι δεν θα γίνει restart σε write boundary.
- Ο πιο σωστός τρόπος είναι το monitoring να είναι robust (Option A), ώστε να αντέχει partial writes.

