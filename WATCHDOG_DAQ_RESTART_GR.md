# DAQ Watchdog (αυτόματο restart του VCDSO ανά ~1 ώρα)

Αυτό το έγγραφο περιγράφει **γιατί** υπάρχει το watchdog, **πώς** δουλεύει, τι **δικλίδες ασφαλείας** έχουμε βάλει και ποιοι είναι οι **κίνδυνοι/περιορισμοί**.

## Πρόβλημα που λύνει (γιατί το κάνουμε)

Έχει παρατηρηθεί ότι, όταν το DAQ (`VCDSO.exe`) τρέχει για αρκετή ώρα και ο Hantek “παίρνει σήμα”, μετά από ~1 ώρα μπορεί να εμφανιστεί latency/“μπούκωμα”. Ένα πρακτικό mitigation είναι να γίνεται **restart του `VCDSO.exe` περίπου ανά ώρα**, όσο τρέχει DAQ.

Στόχοι:

- Να είναι **αυτόματο** (να μη βασίζεται στη μνήμη του χρήστη).
- Να είναι **δεμένο με το lifecycle** του DAQ: ξεκινά μαζί με το DAQ και σταματά μαζί του.
- Να έχει **ελάχιστο workload** και να μην πειράζει το Monitoring UI (IIS Express/Web).

## Πού βρίσκεται / ποια scripts το χρησιμοποιούν

- Watchdog script: `scripts/Restart-DAQ-Watchdog.ps1`
- Ξεκινά αυτόματα από:
  - `scripts/Start-DAQ-Calibration.ps1`
  - `scripts/Start-DAQ-Showers.ps1`
- Σταματά από:
  - `scripts/Stop-DAQ.ps1`

## Τι κάνει (ακριβής συμπεριφορά)

Όταν τρέξεις `Start-DAQ-Calibration` ή `Start-DAQ-Showers`:

1) Ξεκινάει το αντίστοιχο `VCDSO.exe`.
2) Ξεκινάει **ένα δεύτερο PowerShell process “hidden”** (watchdog) που:
   - περιμένει “περίπου” μία ώρα (τυχαία μέσα σε εύρος),
   - ελέγχει αν υπάρχει πρόσφατη ροή δεδομένων,
   - και τότε κάνει restart το `VCDSO.exe` (stop/start).

Το watchdog **δεν** κάνει restart το Monitoring UI και **δεν** αγγίζει IIS Express.

## Πώς αποφασίζει αν “τρέχει DAQ και ο Hantek παίρνει σήμα”

Το DAQ γράφει δεδομένα σε αρχεία:

- Calibration: `*.data`
- Showers: `*.showerdata`

Στον κώδικα του DAQ, το filename περιέχει την **ώρα** (`..._<Hour>.data` / `..._<Hour>.showerdata`), άρα:

- νέο αρχείο εμφανίζεται όταν αλλάξει ώρα **και** έρθει το πρώτο trigger μετά την αλλαγή,
- μέσα στην ίδια ώρα γίνεται **append** στο ίδιο αρχείο (άρα αλλάζει συνεχώς `LastWriteTime`/μέγεθος).

Το watchdog χρησιμοποιεί ως βασικό κριτήριο “υπάρχει σήμα/ροή” το:

- “έχει γραφτεί πρόσφατα κάποιο `*.data`/`*.showerdata` στον φάκελο δεδομένων;”

Αυτό είναι πιο αξιόπιστο από το “βγήκε νέο hourly file”, γιατί το hourly file μπορεί να καθυστερήσει αν δεν υπάρχουν triggers.

## Πότε κάνει restart (κριτήρια)

Με default ρυθμίσεις:

- Στο startup επιλέγει τυχαία επόμενο restart σε **55–65 λεπτά**.
- Όταν έρθει η ώρα του restart:
  - βρίσκει το πιο πρόσφατο `*.data` ή `*.showerdata` στον φάκελο δεδομένων,
  - αν το πιο πρόσφατο write είναι μέσα στα τελευταία **10 λεπτά**, κάνει restart,
  - αλλιώς θεωρεί ότι “δεν υπάρχει σήμα/ροή” και **αναβάλλει** το restart (δοκιμάζει ξανά σε 5’).

Το “περίπου” (random 55–65’) είναι σκόπιμο: αρκεί επιχειρησιακά και αποφεύγει να “κουμπώνει” πάντα στην αλλαγή της ώρας.

## Πώς κάνει το restart

Το restart γίνεται “hard”:

- `Stop-Process -Force` στο `VCDSO.exe`
- μικρή αναμονή ~2s
- `Start-Process` με:
  - `-FilePath` το ίδιο `VCDSO.exe`
  - `-WorkingDirectory` το ίδιο data directory

## Δικλίδες ασφαλείας (για να μη γίνει χάος)

1) **Μόνο ένα watchdog ενεργό**
   - Χρησιμοποιείται mutex: `Local\\mNetStationKit_RestartDAQ_Watchdog`.
   - Αν ξεκινήσει δεύτερο watchdog (π.χ. ο χρήστης πατήσει λάθος scripts), θα περιμένει μέχρι `MutexWaitSeconds` (default 60s). Αν δεν ελευθερωθεί, τερματίζει.

2) **Δέσιμο με συγκεκριμένο executable path**
   - Το watchdog ΔΕΝ αρκείται να δει “τρέχει κάποιο VCDSO.exe”.
   - Ελέγχει ότι το `VCDSO.exe` που τρέχει έχει **ακριβώς το ίδιο** `ExecutablePath` με αυτό που του δόθηκε από το `Start-DAQ-*`.
   - Έτσι αποφεύγουμε να σκοτώσει “άλλο” VCDSO από άλλη εγκατάσταση/φάκελο.

3) **“Μην κάνεις restart όταν δεν υπάρχει ροή δεδομένων”**
   - Αν δεν γράφονται πρόσφατα `*.data`/`*.showerdata`, το watchdog αναβάλλει.

4) **Best-effort cleanup**
   - Τα `Start-DAQ-*` προσπαθούν να σκοτώσουν προηγούμενο watchdog (διαβάζοντας PID από `logs/restart_daq_watchdog.pid`) πριν ξεκινήσουν νέο.
   - Το `Stop-DAQ` σκοτώνει και το DAQ και τον watchdog.

5) **Logging / state**
   - Για να μπορείς να δεις “τι έκανε και πότε”, το watchdog γράφει:
     - `logs/restart_daq_watchdog.log`
     - `logs/restart_daq_watchdog_state.json`
     - `logs/restart_daq_watchdog.pid`

## Τι μπορεί να πάει στραβά (κίνδυνοι/περιορισμοί)

- **Χάνεις λίγα δευτερόλεπτα δεδομένων** σε κάθε restart (όσο πέφτει/ανεβαίνει το DAQ). Αυτό είναι αναμενόμενο.
- Αν ο χρήστης ξεκινήσει “Showers” ενώ τρέχει “Calibration”:
  - τα start scripts σταματούν το υπάρχον `VCDSO.exe` και ξεκινούν το νέο,
  - το watchdog θα ακολουθήσει το νέο mode (mutex + PID cleanup),
  - αλλά υπάρχει πάντα ένα μικρό παράθυρο λίγων δευτερολέπτων όπου γίνεται switch.
- Αν δεν υπάρχει σήμα/trigger για ώρα, το watchdog θα αναβάλλει restart (επειδή δεν βλέπει πρόσφατη εγγραφή σε αρχείο).
- Αν υπάρχουν δικαιώματα/antivirus που μπλοκάρουν `Stop-Process` ή την εκτέλεση scripts, μπορεί να μην δουλέψει (δες log).

## Πώς ελέγχω ότι δουλεύει

- Μετά το `Start-DAQ-*`, έλεγξε ότι υπάρχουν τα αρχεία:
  - `logs/restart_daq_watchdog.pid`
  - `logs/restart_daq_watchdog_state.json`
- Άνοιξε το:
  - `logs/restart_daq_watchdog.log`
  και δες μηνύματα τύπου “Watchdog started … Next restart …”.

## Ρυθμίσεις (advanced)

Τα βασικά knobs (defaults):

- `MinRestartMinutes` / `MaxRestartMinutes`: 55–65
- `RecentWriteMinutes`: 10 (πόσο πρόσφατη πρέπει να είναι η εγγραφή για να θεωρήσουμε ότι υπάρχει ροή)
- `PollSeconds`: 60 (κάθε πόσα δευτερόλεπτα κάνει checks)
- `MutexWaitSeconds`: 60 (πόσο περιμένει να “κλείσει” προηγούμενος watchdog σε αλλαγή mode)

Αν αλλάξουν αυτά, πρέπει να είμαστε προσεκτικοί να μη γίνουν υπερβολικά επιθετικά (π.χ. πολύ μικρό `RecentWriteMinutes` μπορεί να κάνει το watchdog να “μην πιάνει” περιόδους χαμηλού rate).

