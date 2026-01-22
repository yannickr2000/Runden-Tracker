using OfficeOpenXml;
using OfficeOpenXml.Style;
using OfficeOpenXml.Table;
using Rundenzeiten;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Reflection.PortableExecutable;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.Button;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.ExplorerBar;



namespace Rundenzeiten
{
    public partial class Form1 : Form
    {
        private DateTime start = DateTime.MinValue;
        private DateTime end;
        private System.Windows.Forms.Timer countdownTimer;

        private readonly List<PersonEntry> entries = new();
        private readonly List<RoundEntry> roundEntries = new();
        private string resultFilePath = string.Empty;

        // Wellenstarts
        private DateTime raceStart = DateTime.MinValue;
        private readonly Dictionary<string, DateTime> classStartTimes =
            new(StringComparer.OrdinalIgnoreCase);

        // Helfer
        private bool RaceRunning => startRaceBtn.Text == "Rennen beenden";
        private static string Norm(string s) => (s ?? string.Empty).Trim();

        // LIVETICKER – SERVER UPDATE
        private static readonly HttpClient httpClient = new HttpClient();


        // URL zu deinem PHP-Liveticker-Skript
        //private const string LiveTickerUrl =
        //    "https://events.remse-radsport.de/Cross_im_Bad_2025/update_liveticker.php";

        // Muss exakt zum $secret in update_liveticker.php passen!
        private const string LiveTickerToken = "MEIN_SUPER_GEHEIMER_TOKEN_123";

        // HTML-Helper
        private static string HtmlEncode(string s) => WebUtility.HtmlEncode(s ?? string.Empty);

        // Klassenstarts per Checkbox (egal welche Veranstaltung)
        private bool UseClassStarts => chkKlassenstarts != null && chkKlassenstarts.Checked;
        private bool classStartOptionLocked = false;
        private bool classStartOptionValue = false;

        public Form1()
        {
            InitializeComponent();

            // CheckedListBox für Klassenstarts konfigurieren
            classMultiList.DrawMode = DrawMode.OwnerDrawFixed;
            classMultiList.DrawItem += classMultiList_DrawItem;
            classMultiList.ItemCheck += classMultiList_ItemCheck;
            classMultiList.MouseDown += classMultiList_MouseDown;

            // Veranstaltung ändern -> UI umschalten
            raceNameComboBox.SelectedIndexChanged += raceNameComboBox_SelectedIndexChanged;

            // Veranstaltungsliste
            raceNameComboBox.Items.AddRange(new object[]
            {
                "CrossImBad",
                "Wintercross",
                "Bergzeitfahren",
                "Gravelride",
                "Einzelzeitfahren"
            });
            raceNameComboBox.SelectedIndex = -1;

            chkKlassenstarts.CheckedChanged += (s, e) =>
            {
                // Wenn gesperrt -> Änderung sofort zurückdrehen
                if (classStartOptionLocked)
                {
                    chkKlassenstarts.CheckedChanged -= null; // (macht nichts, nur damit klar ist: kein Rekursions-Loop)
                    chkKlassenstarts.Checked = classStartOptionValue;
                    return;
                }

                // normal: merken & UI updaten
                classStartOptionValue = chkKlassenstarts.Checked;

                // Wenn ausgeschaltet wird: Klassenstarts zurücksetzen
                if (!chkKlassenstarts.Checked)
                {
                    classStartTimes.Clear();

                    if (classMultiList != null)
                    {
                        for (int i = 0; i < classMultiList.Items.Count; i++)
                            classMultiList.SetItemChecked(i, false);

                        classMultiList.Invalidate();
                    }
                }

                UpdateClassStartUIState();
            };




            // bei Änderung der Auswahl UI aktualisieren
            classMultiList.SelectedIndexChanged += (s, e) => UpdateClassStartUIState();

            // Countdown-Timer
            countdownTimer = new System.Windows.Forms.Timer
            {
                Interval = 500
            };
            countdownTimer.Tick += CountdownTimer_Tick;
            countdownTimer.Start();

            // Rennen-Start-Button erstmal sperren
            startRaceBtn.Enabled = false;

            // CheckedListBox für Klassenstarts konfigurieren
            classMultiList.DrawMode = DrawMode.OwnerDrawFixed;
            classMultiList.DrawItem += classMultiList_DrawItem;
            classMultiList.ItemCheck += classMultiList_ItemCheck;
            classMultiList.MouseDown += classMultiList_MouseDown;

            // Startnummerfeld & Button sperren bis Rennstart
            startNumberInput.Enabled = false;
            enterRoundBtn.Enabled = false;
        }

        // ===========================================================
        //  TIMER / RENDAUER
        // ===========================================================
        private void CountdownTimer_Tick(object sender, EventArgs e)
        {
            if (start == DateTime.MinValue) return;

            TimeSpan remaining = end - DateTime.Now;

            if (remaining.TotalSeconds < 0)
            {
                remainingTime.ForeColor = Color.DarkRed;
            }

            remainingTime.Text = FormatSecondsToMMSS(Convert.ToInt32(remaining.TotalSeconds));
        }

        private void confirmDurationBtn_Click(object sender, EventArgs e)
        {
            starterListBtn.Enabled = true;
            remainingTime.Text = FormatSecondsToMMSS(Convert.ToInt32(raceDuration.Value * 60));
            confirmDurationBtn.BackColor = Color.LightGreen;
        }

        public static string FormatSecondsToMMSS(int totalSeconds)
        {
            bool isNegative = totalSeconds < 0;
            totalSeconds = Math.Abs(totalSeconds);

            int minutes = totalSeconds / 60;
            int seconds = totalSeconds % 60;

            string formatted = $"{minutes:D2}:{seconds:D2}";
            return isNegative ? "-" + formatted : formatted;
        }

        // ===========================================================
        //  STARTLISTE LADEN
        // ===========================================================
        private void starterListBtn_Click(object sender, EventArgs e)
        {
            var ofd = new OpenFileDialog
            {
                Filter = "CSV files (*.csv)|*.csv|All files (*.*)|*.*",
                Title = "Select CSV file"
            };

            if (ofd.ShowDialog() != DialogResult.OK)
                return;

            string filePath = ofd.FileName;

            // Starter einlesen
            entries.Clear();

            try
            {
                entries.AddRange(ReadCsvFile(filePath));
            }
            catch (InvalidOperationException ex)
            {
                MessageBox.Show(ex.Message, "Starterliste ungültig", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                starterListLabel.Text = "Starterliste nicht geladen";
                return;
            }
            catch (IOException ex)
            {
                MessageBox.Show("Datei kann nicht gelesen werden. (Falls Excel offen ist: schließen oder speichern)\n\n" + ex.Message,
                    "Datei gesperrt", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                starterListLabel.Text = "Starterliste nicht geladen";
                return;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Fehler beim Einlesen der Starterliste:\n\n" + ex.Message,
                    "Fehler", MessageBoxButtons.OK, MessageBoxIcon.Error);
                starterListLabel.Text = "Starterliste nicht geladen";
                return;
            }

            if (entries.Count < 1)
            {
                starterListLabel.Text = "Keine Teilnehmer gefunden";
                return;
            }

            // Checkbox festsetzen ab jetzt
            classStartOptionValue = chkKlassenstarts.Checked;
            classStartOptionLocked = true;

            chkKlassenstarts.Enabled = false;     // optisch grau
            chkKlassenstarts.AutoCheck = false;   // selbst wenn enabled irgendwo wieder true wird: Klick ändert nichts


            // Ergebnis-Dateipfad ableiten
            string fileName = Path.GetFileNameWithoutExtension(filePath);
            string rennenNummer = ExtractRaceNumber(fileName);
            resultFilePath = GetResultFilePath(rennenNummer);

            // ------ Klassenliste befüllen (nur wenn Checkbox aktiv) ------
            if (UseClassStarts)
            {
                var classes = entries
                    .Select(e2 => Norm(e2.Klasse))
                    .Where(k => !string.IsNullOrWhiteSpace(k))
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .OrderBy(k => ExtractAgeClass(k))
                    .ThenBy(k => k, StringComparer.OrdinalIgnoreCase)
                    .ToList();

                classMultiList.Items.Clear();
                foreach (var k in classes)
                    classMultiList.Items.Add(k, false);

                classMultiList.Enabled = classes.Count > 0;

                // bereits gestartete Klassen "sperren"
                for (int i = 0; i < classMultiList.Items.Count; i++)
                {
                    string name = Norm(classMultiList.Items[i]?.ToString());
                    if (classStartTimes.ContainsKey(name))
                    {
                        classMultiList.SetItemCheckState(i, CheckState.Indeterminate);
                        classMultiList.SetItemChecked(i, false);
                    }
                }

                classMultiList.Invalidate();
            }
            else
            {
                classMultiList.Items.Clear();
                classMultiList.Enabled = false;
            }


            // CSV/Excel sofort einmal erzeugen & UI-Status aktualisieren
            saveCSV();
            starterListLabel.Text = "Teilnehmer geladen";
            starterListBtn.BackColor = Color.LightGreen;
            //startRaceBtn.Enabled = true;

            UpdateClassStartUIState();
            this.Text = $"UseClassStarts={UseClassStarts} entries={entries.Count}";

            entries.Clear();
            entries.AddRange(ReadCsvFile(filePath));
        }

        private List<PersonEntry> ReadCsvFile(string path)
        {
            var result = new List<PersonEntry>();
            var seenStartnummern = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            // Wichtig: damit es auch geht, wenn Excel die Datei offen hat
            using var fs = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            using var reader = CreateSmartReader(fs);

            // ---- 1) Header lesen (erste nicht-leere Zeile) ----
            string headerLine = null;
            while (!reader.EndOfStream)
            {
                var l = reader.ReadLine();
                if (!string.IsNullOrWhiteSpace(l))
                {
                    headerLine = l;
                    break;
                }
            }

            if (string.IsNullOrWhiteSpace(headerLine))
                return result;

            char delimiter = DetectDelimiter(headerLine);

            var headerFields = SplitCsvLine(headerLine, delimiter);
            var colIndex = BuildHeaderIndex(headerFields);

            bool useFallback = !HasRequiredColumns(colIndex);

            // ---- 2) Daten lesen ----
            while (!reader.EndOfStream)
            {
                var line = reader.ReadLine();
                if (string.IsNullOrWhiteSpace(line))
                    continue;

                var fields = SplitCsvLine(line, delimiter);

                // Zeilen die komplett leer sind (z.B. ",,,,,,," oder ";;;;;;;") ignorieren
                if (fields.All(f => string.IsNullOrWhiteSpace(f)))
                    continue;

                string startnummer = useFallback
                    ? GetSafe(fields, 0)
                    : GetByKey(fields, colIndex, "startnummer");

                startnummer = (startnummer ?? "").Trim();

                // Startnummer leer oder 0 => ignorieren
                if (string.IsNullOrWhiteSpace(startnummer) || startnummer == "0")
                    continue;

                // NEU: Wenn NACH der Startnummer alles leer ist -> ignorieren
                bool allOtherEmpty;
                if (useFallback)
                {
                    allOtherEmpty = fields.Length <= 1 || fields.Skip(1).All(f => string.IsNullOrWhiteSpace(f));
                }
                else
                {
                    var otherValues = new[]
                    {
                    GetByKey(fields, colIndex, "name", true),
                    GetByKey(fields, colIndex, "vorname", true),
                    GetByKey(fields, colIndex, "geschlecht", true),
                    GetByKey(fields, colIndex, "verein", true),
                    GetByKey(fields, colIndex, "geburtsdatum", true),
                    GetByKey(fields, colIndex, "strecke", true),
                    GetByKey(fields, colIndex, "klasse", true),
                    GetByKey(fields, colIndex, "ucicode", true)
                };

                    allOtherEmpty = otherValues.All(v => string.IsNullOrWhiteSpace(v));
                }

                if (allOtherEmpty)
                    continue;

                // echte doppelte Startnummer => Starterliste ungültig
                if (!seenStartnummern.Add(startnummer))
                    throw new InvalidOperationException(
                        $"Doppelte Startnummer gefunden: '{startnummer}'. Starterliste wird nicht geladen.");

                var entry = new PersonEntry
                {
                    Startnummer = startnummer,
                    Name = useFallback ? GetSafe(fields, 1) : GetByKey(fields, colIndex, "name"),
                    Vorname = useFallback ? GetSafe(fields, 2) : GetByKey(fields, colIndex, "vorname"),
                    Geschlecht = useFallback ? GetSafe(fields, 3) : GetByKey(fields, colIndex, "geschlecht"),
                    Verein = useFallback ? GetSafe(fields, 4) : GetByKey(fields, colIndex, "verein"),
                    Geburtsdatum = useFallback ? GetSafe(fields, 5) : GetByKey(fields, colIndex, "geburtsdatum"),
                    Strecke = useFallback ? GetSafe(fields, 6) : GetByKey(fields, colIndex, "strecke"),
                    Klasse = useFallback ? GetSafe(fields, 7) : GetByKey(fields, colIndex, "klasse"),
                    UciCode = useFallback
                                    ? (fields.Length >= 9 ? GetSafe(fields, 8) : "")
                                    : GetByKey(fields, colIndex, "ucicode", optional: true)
                };

                result.Add(entry);
            }

            return result;
        }

        // -------------------- Encoding (Umlaute) --------------------

        private StreamReader CreateSmartReader(FileStream fs)
        {
            // 1) Erst mal mit UTF-8 (BOM wird automatisch erkannt)
            fs.Position = 0;
            using var testReader = new StreamReader(
                fs,
                new UTF8Encoding(encoderShouldEmitUTF8Identifier: false, throwOnInvalidBytes: false),
                detectEncodingFromByteOrderMarks: true,
                bufferSize: 4096,
                leaveOpen: true
            );

            char[] buffer = new char[4096];
            int read = testReader.Read(buffer, 0, buffer.Length);
            string sample = new string(buffer, 0, read);

            // viele � => sehr wahrscheinlich nicht UTF-8
            int repl = sample.Count(c => c == '\uFFFD'); // "�"
            bool looksBad = repl >= 2;

            fs.Position = 0;

            if (!looksBad)
            {
                return new StreamReader(fs, new UTF8Encoding(false, false), detectEncodingFromByteOrderMarks: true);
            }

            // 2) Fallback: Windows-1252 (Excel/ANSI typisch)
            return new StreamReader(fs, Encoding.GetEncoding(1252), detectEncodingFromByteOrderMarks: true);
        }

        // -------------------- CSV / Header helpers --------------------

        private static char DetectDelimiter(string line)
        {
            int semis = line.Count(c => c == ';');
            int commas = line.Count(c => c == ',');
            return (commas > semis) ? ',' : ';';
        }

        private static Dictionary<string, int> BuildHeaderIndex(string[] headerFields)
        {
            var map = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);

            for (int i = 0; i < headerFields.Length; i++)
            {
                var key = NormHeader(headerFields[i]);
                if (string.IsNullOrWhiteSpace(key)) continue;
                if (!map.ContainsKey(key))
                    map[key] = i;
            }

            // Synonyme erlauben
            AddAlias(map, "startnummer", "bib", "startno", "startnumber");
            AddAlias(map, "name", "nachname", "lastname");
            AddAlias(map, "vorname", "firstname");
            AddAlias(map, "geschlecht", "sex", "gender");
            AddAlias(map, "verein", "club", "team");
            AddAlias(map, "geburtsdatum", "dob", "birthdate", "geburtstag");
            AddAlias(map, "strecke", "rennen", "race", "course");
            AddAlias(map, "klasse", "ak", "category");
            AddAlias(map, "ucicode", "uci", "uci-code", "uci_code");

            return map;
        }

        private static void AddAlias(Dictionary<string, int> map, string canonical, params string[] aliases)
        {
            canonical = NormHeader(canonical);
            if (!map.TryGetValue(canonical, out int idx)) return;

            foreach (var a in aliases.Select(NormHeader))
                if (!map.ContainsKey(a)) map[a] = idx;
        }

        private static bool HasRequiredColumns(Dictionary<string, int> colIndex)
        {
            return colIndex.ContainsKey("startnummer")
                && colIndex.ContainsKey("name")
                && colIndex.ContainsKey("vorname")
                && colIndex.ContainsKey("geschlecht")
                && colIndex.ContainsKey("verein")
                && colIndex.ContainsKey("geburtsdatum")
                && colIndex.ContainsKey("strecke")
                && colIndex.ContainsKey("klasse");
        }

        private static string NormHeader(string s)
        {
            s = (s ?? "").Trim();

            // vereinheitlichen
            s = s.Replace(" ", "")
                 .Replace("-", "")
                 .Replace("_", "");

            return s.ToLowerInvariant();
        }

        private static string GetByKey(string[] fields, Dictionary<string, int> idx, string key, bool optional = false)
        {
            key = NormHeader(key);
            if (!idx.TryGetValue(key, out int i))
                return optional ? "" : "";

            return GetSafe(fields, i);
        }

        private static string GetSafe(string[] fields, int i)
        {
            if (fields == null) return "";
            if (i < 0 || i >= fields.Length) return "";
            return (fields[i] ?? "").Trim();
        }

        /// <summary>
        /// CSV split mit Quotes-Unterstützung (für Namen/Vereine mit Komma/; etc.)
        /// </summary>
        private static string[] SplitCsvLine(string line, char delimiter)
        {
            var res = new List<string>();
            var sb = new StringBuilder();
            bool inQuotes = false;

            for (int i = 0; i < line.Length; i++)
            {
                char c = line[i];

                if (c == '"')
                {
                    // "" innerhalb Quotes => ein "
                    if (inQuotes && i + 1 < line.Length && line[i + 1] == '"')
                    {
                        sb.Append('"');
                        i++;
                    }
                    else
                    {
                        inQuotes = !inQuotes;
                    }
                    continue;
                }

                if (c == delimiter && !inQuotes)
                {
                    res.Add(sb.ToString());
                    sb.Clear();
                    continue;
                }

                sb.Append(c);
            }

            res.Add(sb.ToString());
            return res.ToArray();
        }

        //==========================================================

        private string ExtractRaceNumber(string fileName)
        {
            // akzeptiert "Rennen6", "Rennen_6", "rennen 6" UND "Lauf6", "Lauf_6", "lauf 6"
            var m = Regex.Match(fileName, @"(?:[Rr]ennen|[Ll]auf)[_ ]*(\d+)");
            if (m.Success) return m.Groups[1].Value;

            // Fallback: Zahl am Ende des Namens
            m = Regex.Match(fileName, @"(\d+)$");
            if (m.Success) return m.Groups[1].Value;

            return "1";
        }

        private string GetResultFilePath(string rennenNummer)
        {
            string date = DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss");
            string veranstaltung = raceNameComboBox.SelectedItem?.ToString() ?? "CrossImBad";

            // Cross im Bad => Rennen, Wintercross => Lauf
            string bezeichner = IsCrossImBadSelected() ? "Rennen" : "Lauf";

            // Ordner ".\Ergebnisse" neben der EXE
            string resultsDir = Path.Combine(Application.StartupPath, "Ergebnisse");
            Directory.CreateDirectory(resultsDir);

            string fileName = $"Ergebnisse_{veranstaltung}_{bezeichner}{rennenNummer}_{date}.csv";
            return Path.Combine(resultsDir, fileName);
        }


        private bool IsCrossImBadSelected()
        {
            return string.Equals(
                raceNameComboBox.SelectedItem?.ToString(),
                "CrossImBad",
                StringComparison.OrdinalIgnoreCase);
        }

        // ===========================================================
        //  KLASSENSTARTS
        // ===========================================================
        private void startClassBtn_Click(object sender, EventArgs e)
        {
            // Sicherheit: Klassen erst starten, wenn das Rennen läuft
            if (!RaceRunning) return;

            // aktuell angehakten Klassen mit JETZT starten
            StartSelectedClasses(DateTime.Now);
        }

        // startet alle aktuell angehakten, noch nicht gestarteten Klassen
        private void StartSelectedClasses(DateTime startTime)
        {
            if (!UseClassStarts) return;
            if (classMultiList == null || classMultiList.Items.Count == 0) return;

            var toStart = classMultiList.CheckedItems
                .Cast<object>()
                .Select(it => Norm(it?.ToString()))
                .Where(k => !string.IsNullOrWhiteSpace(k))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();

            if (toStart.Count == 0)
                return;

            // nur Klassen, die noch keine Startzeit haben
            var fresh = toStart.Where(k => !classStartTimes.ContainsKey(k)).ToList();
            if (fresh.Count == 0)
                return;

            foreach (var k in fresh)
                classStartTimes[k] = startTime;

            // UI aktualisieren: gestartete Klassen grau & gesperrt
            for (int i = 0; i < classMultiList.Items.Count; i++)
            {
                var name = Norm(classMultiList.Items[i]?.ToString());
                if (fresh.Contains(name, StringComparer.OrdinalIgnoreCase))
                {
                    classMultiList.SetItemCheckState(i, CheckState.Indeterminate);
                    classMultiList.SetItemChecked(i, false);
                }
            }

            classMultiList.Invalidate();
            UpdateClassStatusLabel();
            UpdateClassStartUIState();
        }

        // gestartete Klassen blockieren
        private void classMultiList_MouseDown(object sender, MouseEventArgs e)
        {
            int index = classMultiList.IndexFromPoint(e.Location);
            if (index < 0) return;

            string klasse = Norm(classMultiList.Items[index]?.ToString());
            if (classStartTimes.ContainsKey(klasse))
            {
                // Klick wird über ItemCheck wieder zurückgesetzt
                return;
            }
        }

        private void classMultiList_ItemCheck(object sender, ItemCheckEventArgs e)
        {
            string name = Norm(classMultiList.Items[e.Index]?.ToString());

            if (classStartTimes.ContainsKey(name))
            {
                e.NewValue = e.CurrentValue; // verhindert Umschalten
            }

            // nach jeder Änderung den Button-Status prüfen
            BeginInvoke(new Action(UpdateClassStartUIState));
        }

        private void classMultiList_DrawItem(object sender, DrawItemEventArgs e)
        {
            if (e.Index < 0) return;

            string name = Norm(classMultiList.Items[e.Index]?.ToString());
            bool started = classStartTimes.ContainsKey(name);

            e.DrawBackground();

            using (var brush = new SolidBrush(started ? Color.Gray : SystemColors.ControlText))
            {
                e.Graphics.DrawString(name, e.Font, brush, e.Bounds);
            }

            e.DrawFocusRectangle();
        }

        private void raceNameComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            UpdateClassStartUIState();
        }

        private void UpdateClassStartUIState()
        {
            bool hasEntries = entries.Count > 0;

            // Klassenstarts sind nur aktiv, wenn Checkbox an + Starterliste geladen
            bool canUseClasses = UseClassStarts && hasEntries;

            if (classMultiList != null)
                classMultiList.Enabled = canUseClasses;

            // (optional) wenn du ein Panel/GroupBox hast, besser alles zusammen sperren:
            // grpKlassenstarts.Enabled = canUseClasses;

            bool anyFreshChecked = false;

            if (canUseClasses && classMultiList != null)
            {
                foreach (var item in classMultiList.CheckedItems.Cast<object>())
                {
                    string k = Norm(item?.ToString());
                    if (!classStartTimes.ContainsKey(k))
                    {
                        anyFreshChecked = true;
                        break;
                    }
                }
            }

            // Button "Klassen starten"
            startClassBtn.Enabled = canUseClasses && RaceRunning && anyFreshChecked;

            // Button "Rennen starten"
            if (RaceRunning)
            {
                startRaceBtn.Enabled = true;
            }
            else
            {
                if (UseClassStarts)
                {
                    // Wenn Klassenstarts aktiv: erst starten, wenn mind. 1 Klasse angehakt ist
                    startRaceBtn.Enabled = hasEntries && anyFreshChecked;
                }
                else
                {
                    // Ohne Klassenstarts reicht Starterliste
                    startRaceBtn.Enabled = hasEntries;
                }
            }
        }


        private void UpdateClassStatusLabel()
        {
            if (classStatusLabel == null) return;

            bool ready = IsCrossImBadSelected() && entries.Count > 0
                         && classMultiList != null && classMultiList.Items.Count > 0;

            if (!ready)
            {
                classStatusLabel.Text = "";
                return;
            }

            var allClasses = classMultiList.Items.Cast<object>()
                .Select(o => o?.ToString())
                .Where(s => !string.IsNullOrWhiteSpace(s))
                .Distinct()
                .OrderBy(k => ExtractAgeClass(k))
                .ThenBy(k => k)
                .ToList();

            var started = new List<string>();
            var open = new List<string>();

            foreach (var k in allClasses)
            {
                if (classStartTimes.TryGetValue(k, out var t))
                    started.Add($"{k} ({t:HH:mm:ss})");
                else
                    open.Add(k);
            }

            var sb = new StringBuilder();

            if (started.Count > 0)
            {
                sb.AppendLine("✅ gestartet:");
                foreach (var s in started)
                    sb.AppendLine("• " + s);
            }

            if (open.Count > 0)
            {
                sb.AppendLine("⏱️ offen:");
                foreach (var o in open)
                    sb.AppendLine("• " + o);
            }

            if (started.Count == 0 && open.Count > 0 && RaceRunning)
                sb.AppendLine("(wähle Klassen und drücke „Klasse starten“)");

            classStatusLabel.Text = sb.ToString().TrimEnd();
        }

        // ===========================================================
        //  RENNSTART / RUNDEN ERFASSEN
        // ===========================================================
        private void startRaceBtn_Click(object sender, EventArgs e)
        {
            if (raceNameComboBox.SelectedIndex == -1)
            {
                MessageBox.Show("Bitte zuerst eine Veranstaltung auswählen!",
                                "Hinweis",
                                MessageBoxButtons.OK,
                                MessageBoxIcon.Warning);
                return;
            }

            if (startRaceBtn.Text == "Rennen beenden")
            {
                saveCSV();                   // letztes Speichern
                SendTickerToServer("");      // Live-Ticker leeren
                Environment.Exit(0);
            }

            // Neues Rennen starten
            start = DateTime.Now;
            raceStart = start;
            end = start.AddMinutes(Convert.ToInt32(raceDuration.Value));
            // alle Klassen-Startzeiten zurücksetzen (neues Rennen)
            classStartTimes.Clear();

            startRaceBtn.BackColor = Color.Red;
            startRaceBtn.Text = "Rennen beenden";
            startNumberInput.Enabled = true;
            enterRoundBtn.Enabled = true;

            raceNameComboBox.Enabled = false;
            starterListBtn.Enabled = false;
            raceDuration.Enabled = false;
            confirmDurationBtn.Enabled = false;
            classStartOptionValue = chkKlassenstarts.Checked;
            classStartOptionLocked = true;
            chkKlassenstarts.Enabled = false;
            chkKlassenstarts.AutoCheck = false;

            if (UseClassStarts)
            {
                StartSelectedClasses(start);
            }
            // direkt zum Rennstart einmal Live-Tabelle erzeugen
            saveCSV();

            UpdateClassStartUIState();


        }

        private void enterRoundBtn_Click(object sender, EventArgs e)
        {
            int startNumber = 0;

            try
            {
                startNumber = Convert.ToInt32(startNumberInput.Text);
            }
            catch
            {
                // ignorieren
            }

            enterRound(startNumber);
        }

        private void startNumberInput_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyData == Keys.Enter)
            {
                int startNumber = 0;

                try
                {
                    startNumber = Convert.ToInt32(startNumberInput.Text);
                }
                catch { }

                enterRound(startNumber);
            }
        }

        private void enterRound(int startnumber)
        {
            startNumberInput.Text = "";

            // negative Startnummer: letzten Eintrag löschen
            if (startnumber < 0 && roundEntries.Count > 0)
            {
                roundResultLabel.Text = "Letzten Eintrag gelöscht";
                roundResultLabel.ForeColor = Color.Green;
                roundEntries.RemoveAt(roundEntries.Count - 1);
                saveCSV();
                return;
            }

            var person = entries.FirstOrDefault(x => x.Startnummer == startnumber.ToString());

            if (person == null)
            {
                roundResultLabel.Text = "Startnummer ungültig!";
                roundResultLabel.ForeColor = Color.Red;
                return;
            }

            var personEntries = roundEntries
                .Where(x => x.Startnummer == person.Startnummer)
                .ToList();

            double toAdd = personEntries.Select(x => x.Time.TotalSeconds).Sum();
            TimeSpan lapTime;

            if (UseClassStarts)
            {
                var key = Norm(person.Klasse);
                if (!classStartTimes.TryGetValue(key, out DateTime classStart))
                {
                    roundResultLabel.Text = $"Klasse \"{person.Klasse}\" noch nicht gestartet!";
                    roundResultLabel.ForeColor = Color.Red;
                    return;
                }

                lapTime = DateTime.Now - classStart - TimeSpan.FromSeconds(toAdd);
            }
            else
            {
                lapTime = DateTime.Now - start - TimeSpan.FromSeconds(toAdd);
            }

            roundResultLabel.Text = $"Eingetragen - {person.Startnummer} - {Math.Round(lapTime.TotalSeconds, 1)}s";
            roundResultLabel.ForeColor = Color.Green;

            roundEntries.Add(new RoundEntry
            {
                Startnummer = person.Startnummer,
                Time = lapTime
            });

            saveCSV();
        }

        // ===========================================================
        //  Live-Ticker
        // ===========================================================
        private bool LiveTickerEnabled => chkLiveTickerAktiv != null && chkLiveTickerAktiv.Checked;

        private void SendTickerToServer(string text)
        {
            // Nur senden, wenn Checkbox aktiv ist
            if (!LiveTickerEnabled)
                return;

            try
            {
                string url = GetLiveTickerUrl();
                if (string.IsNullOrEmpty(url))
                    return; // für diese Veranstaltung kein Liveticker definiert

                var values = new Dictionary<string, string>
                {
                    ["token"] = LiveTickerToken,
                    ["text"] = text
                };

                using (var content = new FormUrlEncodedContent(values))
                {
                    var response = httpClient.PostAsync(url, content).Result;
                }
            }
            catch
            {
                // Fehler ignorieren
            }
        }

        private void SaveLiveTicker(List<CSVRecord> records)
        {
            try
            {
                // Wenn Liveticker nicht aktiv ist → gar nichts machen
                if (!LiveTickerEnabled)
                    return;
                // Wenn kein Rennen läuft → Hinweis anzeigen
                if (!RaceRunning)
                {
                    SendTickerToServer("<p><strong>Rennen noch nicht gestartet.</strong></p>");
                    return;
                }

                // Wenn Rennen läuft, aber keine Daten vorhanden
                if (records == null || records.Count == 0)
                {
                    SendTickerToServer("<p>Keine Teilnehmer vorhanden.</p>");
                    return;
                }

                // Restzeit berechnen
                TimeSpan remaining = end - DateTime.Now;
                if (remaining < TimeSpan.Zero)
                    remaining = TimeSpan.Zero;

                // WICHTIG: richtiger Format-String (nur ein Backslash)
                string remainingText = remaining.ToString(@"hh\:mm\:ss");

                // Nach Klasse gruppieren (für jede Klasse eigene Tabelle)
                var groups = records
                    .GroupBy(r => r.Klasse ?? "")
                    .OrderBy(g => ExtractAgeClass(g.Key))   // U9, U11, U13 ...
                    .ThenBy(g => g.Key);

                var sb = new StringBuilder();

                // Restzeit anzeigen
                sb.AppendLine($"<p><strong>Restzeit: {remainingText}</strong></p>");

                foreach (var g in groups)
                {
                    string klasse = string.IsNullOrWhiteSpace(g.Key) ? "ohne Klasse" : g.Key;

                    // Überschrift je Klasse
                    sb.AppendLine($"<h3>{HtmlEncode(klasse)}</h3>");

                    sb.AppendLine("<table style=\"width:100%;border-collapse:collapse;\">");
                    sb.AppendLine("<thead><tr>");
                    sb.AppendLine("<th style='border:1px solid #ccc;padding:4px;'>Platz</th>");
                    sb.AppendLine("<th style='border:1px solid #ccc;padding:4px;'>Startnr.</th>");
                    sb.AppendLine("<th style='border:1px solid #ccc;padding:4px;'>Name</th>");
                    sb.AppendLine("<th style='border:1px solid #ccc;padding:4px;'>Verein</th>");
                    sb.AppendLine("<th style='border:1px solid #ccc;padding:4px;'>Zeit</th>");
                    sb.AppendLine("<th style='border:1px solid #ccc;padding:4px;'>Runden</th>");
                    sb.AppendLine("</tr></thead><tbody>");

                    // Sortierung in der Klasse:
                    // 1. Runden absteigend
                    // 2. Zeit aufsteigend
                    // 3. Startnummer
                    var ordered = g
                        .OrderByDescending(r => r.Rundenzeiten?.Count ?? 0)
                        .ThenBy(r => r.Zeit.TotalSeconds)
                        .ThenBy(r => int.TryParse(r.Startnummer, out var sn) ? sn : int.MaxValue);

                    int place = 1;
                    foreach (var r in ordered)
                    {
                        int rounds = r.Rundenzeiten?.Count ?? 0;
                        string timeText = rounds > 0
                            // WICHTIG: auch hier richtiger Format-String
                            ? r.Zeit.ToString(@"hh\:mm\:ss")
                            : "";

                        sb.AppendLine("<tr>");
                        sb.AppendLine($"<td style='border:1px solid #ccc;padding:4px;text-align:center;'>{place}</td>");
                        sb.AppendLine($"<td style='border:1px solid #ccc;padding:4px;text-align:center;'>{HtmlEncode(r.Startnummer)}</td>");
                        sb.AppendLine($"<td style='border:1px solid #ccc;padding:4px;'>{HtmlEncode(r.Vorname)} {HtmlEncode(r.Name)}</td>");
                        sb.AppendLine($"<td style='border:1px solid #ccc;padding:4px;'>{HtmlEncode(r.Verein)}</td>");
                        sb.AppendLine($"<td style='border:1px solid #ccc;padding:4px;text-align:center;'>{HtmlEncode(timeText)}</td>");
                        sb.AppendLine($"<td style='border:1px solid #ccc;padding:4px;text-align:center;'>{rounds}</td>");
                        sb.AppendLine("</tr>");

                        place++;
                    }

                    sb.AppendLine("</tbody></table>");
                }

                // das fertige HTML an den Server schicken
                SendTickerToServer(sb.ToString());
            }
            catch (Exception ex)
            {
                // Zum Debuggen kannst du dir den Fehler auch anzeigen lassen:
                // SendTickerToServer("<p>Fehler im Liveticker: " + HtmlEncode(ex.Message) + "</p>");
                // aktuell: still schweigen
            }
        }

        private string GetLiveTickerUrl()
        {
            string veranstaltung = raceNameComboBox?.SelectedItem?.ToString() ?? string.Empty;

            switch (veranstaltung)
            {
                case "CrossImBad":
                    return "https://events.remse-radsport.de/Cross_im_Bad_2025/update_liveticker.php";

                case "Wintercross":
                    return "https://events.remse-radsport.de/Remser_Wintercross/update_liveticker.php";

                case "Gravelride":
                    return "";

                case "Bergzeitfahren":
                    return "";

                default:
                    // für diese Veranstaltung kein Liveticker
                    return null;
            }
        }

        // ===========================================================
        //  CSV + EXCEL
        // ===========================================================
        private void saveCSV()
        {
            if (string.IsNullOrWhiteSpace(resultFilePath))
                return;

            const string Header =
                "Platz;PlatzAK;Startnummer;Name;Vorname;Geschlecht;Geburtsdatum;Verein;Strecke;Klasse;Ucicode;Zeit;Rundenzeiten";

            List<CSVRecord> records = BuildRecords();
            RankRecords(records);

            // 1) CSV
            SaveToCsvFile(records, Header, resultFilePath);

            // 2) Excel mit "Gesamt" + pro-Klasse-Blättern
            SaveToExcelPerClass(records, resultFilePath);

            // 3) Anzeige
            DisplayInGrid(records);

            // 4) Live-Ticker aktualisieren
            SaveLiveTicker(records);
        }
        private (string Label, string Nummer) GetRunLabelAndNumberFromPath(string filePath)
        {
            // Standard: Cross im Bad = Rennen, Wintercross = Lauf
            string label = IsCrossImBadSelected() ? "Rennen" : "Lauf";

            string name = Path.GetFileNameWithoutExtension(filePath) ?? "";

            // Wir akzeptieren beides, falls mal gemischt gespeichert wurde:
            // ...Rennen1 / Rennen_1 / Lauf1 / Lauf_1 etc.
            var m = Regex.Match(name, @"(?:Rennen|Lauf)\s*_?\s*(\d+)", RegexOptions.IgnoreCase);
            if (m.Success)
                return (label, m.Groups[1].Value);

            // Fallback: nur Zahl irgendwo?
            var m2 = Regex.Match(name, @"(\d+)");
            if (m2.Success)
                return (label, m2.Groups[1].Value);

            return (label, "?");
        }

        private List<CSVRecord> BuildRecords()
        {
            var result = new List<CSVRecord>();

            foreach (var person in entries)
            {
                var personEntries = roundEntries.Where(x => x.Startnummer == person.Startnummer).ToList();
                var rundenzeiten = personEntries.Select(e => Math.Round(e.Time.TotalSeconds, 1)).ToList();

                result.Add(new CSVRecord
                {
                    Geschlecht = person.Geschlecht,
                    Klasse = person.Klasse,
                    Name = person.Name,
                    Platz = 0,
                    PlatzAK = 0,
                    Geburtsdatum = person.Geburtsdatum,
                    Rundenzeiten = rundenzeiten,
                    Startnummer = person.Startnummer,
                    Strecke = person.Strecke,
                    Verein = person.Verein,
                    Vorname = person.Vorname,
                    Zeit = TimeSpan.FromSeconds(rundenzeiten.Sum()),
                    UciCode = person.UciCode,
                    AltersgruppeHobby = string.Empty
                });
            }

            return result;
        }

        private void SaveToCsvFile(List<CSVRecord> records, string header, string filePath)
        {
            var lines = new List<string> { header };

            foreach (var record in records)
            {
                var laps = record.Rundenzeiten ?? new List<double>();
                string lapsRaw = "[" + string.Join(", ",
                    laps.Select(x => x.ToString(System.Globalization.CultureInfo.InvariantCulture))) + "]";

                string line =
                    $"{record.Platz};{record.PlatzAK};{record.Startnummer};{record.Name};{record.Vorname};" +
                    $"{record.Geschlecht};{record.Geburtsdatum};{record.Verein};{record.Strecke};{record.Klasse};" +
                    $"{record.UciCode};" +
                    $"{record.Zeit:hh\\:mm\\:ss} ({laps.Count} Runden);{lapsRaw}";

                lines.Add(line);
            }

            Directory.CreateDirectory(Path.GetDirectoryName(filePath)!);
            File.WriteAllLines(filePath, lines, new UTF8Encoding(true)); // UTF-8 mit BOM
        }

        // -----------------------------------------------------------
        //  Excel (gleich wie vorher, nur übernommen)
        // -----------------------------------------------------------
        private string FindLogoPath()
        {
            string baseDir = Path.Combine(Application.StartupPath, "Vorlagen");
            if (!Directory.Exists(baseDir)) return null;

            string[] candidates =
            {
                "Logo.png", "logo.png", "Logo.jpg", "logo.jpg", "Logo.jpeg", "logo.jpeg"
            };

            foreach (var name in candidates)
            {
                string p = Path.Combine(baseDir, name);
                if (File.Exists(p)) return p;
            }

            var anyImg = Directory.EnumerateFiles(baseDir)
                .FirstOrDefault(f =>
                    f.EndsWith(".png", StringComparison.OrdinalIgnoreCase) ||
                    f.EndsWith(".jpg", StringComparison.OrdinalIgnoreCase) ||
                    f.EndsWith(".jpeg", StringComparison.OrdinalIgnoreCase));

            return anyImg;
        }

        private void InsertLogo(ExcelWorksheet ws, string logoPath, string uniqueNameSuffix = "")
        {
            if (string.IsNullOrEmpty(logoPath) || !File.Exists(logoPath)) return;

            string picName = "Logo" + (string.IsNullOrEmpty(uniqueNameSuffix) ? "" : "_" + uniqueNameSuffix);
            var pic = ws.Drawings.AddPicture(picName, new FileInfo(logoPath));

            pic.SetPosition(0, 2, 0, 2);
            pic.SetSize(35);
        }

        private static string ToHhMmSs(object zeit)
        {
            if (zeit == null) return "";
            if (zeit is TimeSpan ts) return ts.ToString(@"hh\:mm\:ss");
            return TimeSpan.TryParse(zeit.ToString(), out var t)
                ? t.ToString(@"hh\:mm\:ss")
                : zeit.ToString();
        }

        private static string SanitizeName(string s, int maxLen = 31)
        {
            if (string.IsNullOrWhiteSpace(s)) s = "Unbekannt";
            s = Regex.Replace(s, @"[:\\\/\?\*\[\]]", "_");
            s = s.Trim('\'');
            if (s.Length > maxLen) s = s.Substring(0, maxLen);
            return string.IsNullOrWhiteSpace(s) ? "Klasse" : s;
        }

        private static string FormatLapList(IList<double> lapsSec)
        {
            if (lapsSec == null || lapsSec.Count == 0) return "";
            return string.Join(", ",
                lapsSec.Select(v =>
                {
                    double d = (double.IsFinite(v) && v >= 0 && v <= TimeSpan.MaxValue.TotalSeconds) ? v : 0;
                    return TimeSpan.FromSeconds(d).ToString(@"mm\:ss");
                })
            );
        }

        private void SaveToExcelPerClass(List<CSVRecord> records, string filePath)
        {
            // ===== Lokale Helper: Zeilenfärbung nach Regeln (ganze Zeile) =====
            void AddRowShading(ExcelWorksheet ws, int dataStartRow, int dataEndRow, int colAkHobby, int colGeschlecht, int lastCol)
            {
                if (ws == null || dataEndRow < dataStartRow) return;

                var fullRowAddr = new ExcelAddress(dataStartRow, 1, dataEndRow, lastCol);

                string cellAK = $"${GetColumnLetter(colAkHobby)}{dataStartRow}";
                string cellG = $"${GetColumnLetter(colGeschlecht)}{dataStartRow}";

                // 1) Frauen (w)
                var cfFemale = ws.ConditionalFormatting.AddExpression(fullRowAddr);
                cfFemale.Formula = $"LOWER(LEFT({cellG},1))=\"w\"";
                cfFemale.Style.Fill.PatternType = ExcelFillStyle.Solid;
                cfFemale.Style.Fill.BackgroundColor.Color = Color.FromArgb(255, 229, 236); // Rosa
                cfFemale.Priority = 1;

                // 2) Ü50
                var cfU50 = ws.ConditionalFormatting.AddExpression(fullRowAddr);
                cfU50.Formula = $"{cellAK}=\"Ü50\"";
                cfU50.Style.Fill.PatternType = ExcelFillStyle.Solid;
                cfU50.Style.Fill.BackgroundColor.Color = Color.FromArgb(200, 200, 200); // dunkleres Grau
                cfU50.Priority = 2;

                // 3) Ü40
                var cfU40 = ws.ConditionalFormatting.AddExpression(fullRowAddr);
                cfU40.Formula = $"{cellAK}=\"Ü40\"";
                cfU40.Style.Fill.PatternType = ExcelFillStyle.Solid;
                cfU40.Style.Fill.BackgroundColor.Color = Color.FromArgb(221, 235, 247); // Hellblau
                cfU40.Priority = 3;

                // 4) Rest → weiß
                var cfRest = ws.ConditionalFormatting.AddExpression(fullRowAddr);
                cfRest.Formula = $"AND({cellAK}<>\"Ü40\", {cellAK}<>\"Ü50\", LOWER(LEFT({cellG},1))<>\"w\")";
                cfRest.Style.Fill.PatternType = ExcelFillStyle.Solid;
                cfRest.Style.Fill.BackgroundColor.Color = Color.White;
                cfRest.Priority = 10;
            }

            // Spaltenindex -> Excel-Buchstabe
            string GetColumnLetter(int col)
            {
                var result = "";
                while (col > 0)
                {
                    int mod = (col - 1) % 26;
                    result = (char)('A' + mod) + result;
                    col = (col - 1) / 26;
                }
                return result;
            }

            // ===== Zieldatei =====
            string excelPath = Path.Combine(
                Path.GetDirectoryName(filePath)!,
                Path.GetFileNameWithoutExtension(filePath) + "_Druck.xlsx"
            );
            Directory.CreateDirectory(Path.GetDirectoryName(excelPath)!);


            // Veranstaltung & Lauf/Rennen Nummer
            string veranstaltung = raceNameComboBox?.SelectedItem?.ToString() ?? "CrossImBad";
            var (runLabel, runNummer) = GetRunLabelAndNumberFromPath(filePath);


            // ===== Header inkl. "UCI-Code" =====
            string[] header =
            {
                "Platz",
                "PlatzAK",
                "AK (Hobby)",
                "Startnummer",
                "Name",
                "Vorname",
                "Geschlecht",
                "Verein",
                "UCI-Code",
                "Klasse",
                "Zeit",
                "Rundenzeiten"
            };

            const int COL_AkHobby = 3;
            const int COL_Geschlecht = 7;
            int LAST_COL = header.Length;

            string logoPath = FindLogoPath();
            double[] masterWidths = null;

            using var package = new ExcelPackage();

            // ===== Blatt: Gesamt =====
            var wsAll = package.Workbook.Worksheets.Add("Gesamt");

            wsAll.Row(1).Height = 36;
            wsAll.Row(2).Height = 28;
            wsAll.Row(3).Height = 6;
            wsAll.Row(4).Height = 6;

            wsAll.Cells[2, 1, 2, header.Length].Merge = true;
            wsAll.Cells[2, 1].Value = $"{veranstaltung} – Ergebnisliste ({runLabel} {runNummer})";
            wsAll.Cells[2, 1].Style.Font.Bold = true;
            wsAll.Cells[2, 1].Style.Font.Size = 20;
            wsAll.Cells[2, 1].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

            InsertLogo(wsAll, logoPath, "Gesamt");

            int headerRowAll = 5;
            for (int c = 0; c < header.Length; c++)
            {
                var cell = wsAll.Cells[headerRowAll, c + 1];
                cell.Value = header[c];
                cell.Style.Font.Bold = true;
                cell.Style.Font.Color.SetColor(Color.Black);
                cell.Style.Fill.PatternType = ExcelFillStyle.Solid;
                cell.Style.Fill.BackgroundColor.SetColor(Color.LightGray);
                cell.Style.Border.BorderAround(ExcelBorderStyle.Thin);
            }

            int rowAll = headerRowAll + 1;
            foreach (var r in records)
            {
                bool isHobby = r.Klasse?.IndexOf("hobby", StringComparison.OrdinalIgnoreCase) >= 0;
                string rawAkHobby = r?.AltersgruppeHobby ?? "";

                int? platzAkFromText = null;
                string akGroupLabel = rawAkHobby;

                if (!string.IsNullOrWhiteSpace(rawAkHobby))
                {
                    var parts = rawAkHobby.Split(new[] { ' ' }, 2, StringSplitOptions.RemoveEmptyEntries);
                    if (parts.Length > 0 && int.TryParse(parts[0], out int parsedPlatz))
                    {
                        platzAkFromText = parsedPlatz;
                        akGroupLabel = parts.Length > 1 ? parts[1] : "";
                    }
                }

                object platzAkCell;
                if (!IsCrossImBadSelected() || !isHobby)
                {
                    platzAkCell = (r?.PlatzAK > 0 ? r.PlatzAK : (int?)null);
                }
                else
                {
                    if (r?.PlatzAK > 0)
                        platzAkCell = r.PlatzAK;
                    else if (platzAkFromText.HasValue)
                        platzAkCell = platzAkFromText.Value;
                    else
                        platzAkCell = null;
                }

                wsAll.Cells[rowAll, 1].Value = r?.Platz;
                wsAll.Cells[rowAll, 2].Value = platzAkCell;
                wsAll.Cells[rowAll, 3].Value = akGroupLabel;
                wsAll.Cells[rowAll, 4].Value = r?.Startnummer;
                wsAll.Cells[rowAll, 5].Value = r?.Name ?? "";
                wsAll.Cells[rowAll, 6].Value = r?.Vorname ?? "";
                wsAll.Cells[rowAll, 7].Value = r?.Geschlecht ?? "";
                wsAll.Cells[rowAll, 8].Value = r?.Verein ?? "";
                wsAll.Cells[rowAll, 9].Value = r?.UciCode ?? "";
                wsAll.Cells[rowAll, 10].Value = r?.Klasse ?? "";
                wsAll.Cells[rowAll, 11].Value = ToHhMmSs(r?.Zeit);
                wsAll.Cells[rowAll, 12].Value = FormatLapList(r?.Rundenzeiten);
                rowAll++;
            }

            if (rowAll > headerRowAll + 1)
            {
                var rangeAll = wsAll.Cells[headerRowAll, 1, rowAll - 1, header.Length];
                var tblAll = wsAll.Tables.Add(rangeAll, "tbl_Gesamt");
                tblAll.TableStyle = TableStyles.Medium2;
            }

            wsAll.View.FreezePanes(headerRowAll + 1, 1);

            if (wsAll.Dimension != null)
            {
                wsAll.Cells[wsAll.Dimension.Address].AutoFitColumns(8, 28);

                wsAll.Column(1).Width = 8;
                wsAll.Column(2).Width = 10;
                wsAll.Column(3).Width = 10;
                wsAll.Column(4).Width = 14;
                wsAll.Column(5).Width = 16;
                wsAll.Column(6).Width = 14;
                wsAll.Column(7).Width = 10;
                wsAll.Column(8).Width = 30;
                wsAll.Column(9).Width = 16;
                wsAll.Column(10).Width = 12;
                wsAll.Column(11).Width = 10;
                wsAll.Column(12).Width = 34;

                if (rowAll > headerRowAll + 1)
                {
                    wsAll.Cells[headerRowAll + 1, 8, rowAll - 1, 8].Style.WrapText = true;
                    wsAll.Cells[headerRowAll + 1, 12, rowAll - 1, 12].Style.WrapText = true;
                }

                wsAll.Row(headerRowAll).Height = 20;

                masterWidths = new double[header.Length];
                for (int c = 1; c <= header.Length; c++)
                    masterWidths[c - 1] = wsAll.Column(c).Width;
            }

            wsAll.PrinterSettings.Orientation = eOrientation.Landscape;
            wsAll.PrinterSettings.FitToPage = true;
            wsAll.PrinterSettings.FitToWidth = 1;
            wsAll.PrinterSettings.FitToHeight = 0;

            var groups = records
                .GroupBy(r => r?.Klasse ?? "Unbekannt")
                .OrderBy(g => g.Key);

            int linkRow = 2;

            int dataStartAll = headerRowAll + 1;
            int dataEndAll = rowAll - 1;
            AddRowShading(wsAll, dataStartAll, dataEndAll, COL_AkHobby, COL_Geschlecht, LAST_COL);

            foreach (var g in groups)
            {
                string rawName = g.Key;
                string sheetName = SanitizeName(rawName);
                string uniqueName = sheetName;
                int suffix = 2;

                while (package.Workbook.Worksheets.Any(ws => ws.Name.Equals(uniqueName, StringComparison.OrdinalIgnoreCase)))
                    uniqueName = SanitizeName($"{sheetName}_{suffix++}");

                var ws = package.Workbook.Worksheets.Add(uniqueName);

                ws.Row(1).Height = 36;
                ws.Row(2).Height = 28;
                ws.Row(3).Height = 6;
                ws.Row(4).Height = 6;

                ws.Cells[2, 1, 2, header.Length].Merge = true;
                ws.Cells[2, 1].Value = $"{veranstaltung} – Ergebnisliste – {rawName} ({runLabel} {runNummer})";
                ws.Cells[2, 1].Style.Font.Bold = true;
                ws.Cells[2, 1].Style.Font.Size = 20;
                ws.Cells[2, 1].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

                InsertLogo(ws, logoPath, uniqueName);

                int headerRow = 5;
                for (int c = 0; c < header.Length; c++)
                {
                    var cell = ws.Cells[headerRow, c + 1];
                    cell.Value = header[c];
                    cell.Style.Font.Bold = true;
                    cell.Style.Font.Color.SetColor(Color.Black);
                    cell.Style.Fill.PatternType = ExcelFillStyle.Solid;
                    cell.Style.Fill.BackgroundColor.SetColor(Color.LightGray);
                    cell.Style.Border.BorderAround(ExcelBorderStyle.Thin);
                }

                var data = g.OrderBy(r => r?.Platz ?? int.MaxValue).ToList();
                int row = headerRow + 1;
                foreach (var r in data)
                {
                    bool isHobby = r.Klasse?.IndexOf("hobby", StringComparison.OrdinalIgnoreCase) >= 0;
                    string rawAkHobby = r?.AltersgruppeHobby ?? "";

                    int? platzAkFromText = null;
                    string akGroupLabel = rawAkHobby;

                    if (!string.IsNullOrWhiteSpace(rawAkHobby))
                    {
                        var parts = rawAkHobby.Split(new[] { ' ' }, 2, StringSplitOptions.RemoveEmptyEntries);
                        if (parts.Length > 0 && int.TryParse(parts[0], out int parsedPlatz))
                        {
                            platzAkFromText = parsedPlatz;
                            akGroupLabel = parts.Length > 1 ? parts[1] : "";
                        }
                    }

                    object platzAkCell;
                    if (!IsCrossImBadSelected() || !isHobby)
                    {
                        platzAkCell = (r?.PlatzAK > 0 ? r.PlatzAK : (int?)null);
                    }
                    else
                    {
                        if (r?.PlatzAK > 0)
                            platzAkCell = r.PlatzAK;
                        else if (platzAkFromText.HasValue)
                            platzAkCell = platzAkFromText.Value;
                        else
                            platzAkCell = null;
                    }

                    ws.Cells[row, 1].Value = r?.Platz;
                    ws.Cells[row, 2].Value = platzAkCell;
                    ws.Cells[row, 3].Value = akGroupLabel;
                    ws.Cells[row, 4].Value = r?.Startnummer;
                    ws.Cells[row, 5].Value = r?.Name ?? "";
                    ws.Cells[row, 6].Value = r?.Vorname ?? "";
                    ws.Cells[row, 7].Value = r?.Geschlecht ?? "";
                    ws.Cells[row, 8].Value = r?.Verein ?? "";
                    ws.Cells[row, 9].Value = r?.UciCode ?? "";
                    ws.Cells[row, 10].Value = r?.Klasse ?? "";
                    ws.Cells[row, 11].Value = ToHhMmSs(r?.Zeit);
                    ws.Cells[row, 12].Value = FormatLapList(r?.Rundenzeiten);
                    row++;
                }

                if (row > headerRow + 1)
                {
                    var range = ws.Cells[headerRow, 1, row - 1, header.Length];
                    var tableName = "tbl_" + Regex.Replace(uniqueName, @"\W", "");
                    var tbl = ws.Tables.Add(range, tableName);
                    tbl.TableStyle = TableStyles.Medium2;
                }

                if (ws.Dimension != null)
                {
                    ws.Cells[ws.Dimension.Address].AutoFitColumns(8, 28);

                    if (masterWidths != null)
                    {
                        for (int c = 1; c <= header.Length; c++)
                            ws.Column(c).Width = masterWidths[c - 1];
                    }

                    if (row > headerRow + 1)
                    {
                        ws.Cells[headerRow + 1, 8, row - 1, 8].Style.WrapText = true;
                        ws.Cells[headerRow + 1, 12, row - 1, 12].Style.WrapText = true;
                    }

                    ws.Row(headerRow).Height = 20;
                }

                ws.View.FreezePanes(headerRow + 1, 1);
                ws.PrinterSettings.Orientation = eOrientation.Landscape;
                ws.PrinterSettings.FitToPage = true;
                ws.PrinterSettings.FitToWidth = 1;
                ws.PrinterSettings.FitToHeight = 0;

                int dataStart = headerRow + 1;
                int dataEnd = row - 1;
                AddRowShading(ws, dataStart, dataEnd, COL_AkHobby, COL_Geschlecht, LAST_COL);

                wsAll.Cells[linkRow, header.Length].Hyperlink =
                    new ExcelHyperLink($"'{uniqueName}'!A1", rawName);
                wsAll.Cells[linkRow, header.Length].Value = rawName;
                linkRow++;
            }

            package.SaveAs(new FileInfo(excelPath));
        }

        // ===========================================================
        //  ALTER / HOBBY-BUCKET / RANGFOLGE
        // ===========================================================
        private bool TryParseGeburtsdatum(string s, out DateTime geb)
        {
            geb = DateTime.MinValue;
            if (string.IsNullOrWhiteSpace(s)) return false;

            if (DateTime.TryParseExact(
                s.Trim(),
                new[] { "dd.MM.yyyy", "d.M.yyyy", "yyyy-MM-dd", "dd.MM.yy", "d.M.yy" },
                System.Globalization.CultureInfo.GetCultureInfo("de-DE"),
                System.Globalization.DateTimeStyles.None,
                out var dt))
            {
                geb = dt;
                return true;
            }

            return DateTime.TryParse(
                s,
                System.Globalization.CultureInfo.GetCultureInfo("de-DE"),
                System.Globalization.DateTimeStyles.None,
                out geb);
        }

        private int? GetAgeOnEvent(string geburtsdatum)
        {
            if (!TryParseGeburtsdatum(geburtsdatum, out var geb)) return null;
            var eventDate = (raceStart != DateTime.MinValue ? raceStart.Date : DateTime.Today);
            int age = eventDate.Year - geb.Year;
            if (eventDate < geb.AddYears(age)) age--;
            return Math.Max(age, 0);
        }

        private string GetHobbyBucket(int? age)
        {
            if (age == null) return "";
            if (age >= 50) return "Ü50";
            if (age >= 40) return "Ü40";
            return "";
        }

        private void RankRecords(List<CSVRecord> records)
        {
            int GenderKey(string g)
            {
                if (string.IsNullOrWhiteSpace(g)) return 2;
                g = g.Trim().ToLowerInvariant();
                if (g.StartsWith("m")) return 0;
                if (g.StartsWith("w")) return 1;
                return 2;
            }

            // 1) Sortierung für "sportlich": zuerst Altersklasse (U9/U11/...), dann Leistung
            //    (Runden desc, Zeit asc, Startnummer)
            var ordered = records
                .OrderBy(r => ExtractAgeClass(r.Klasse))
                .ThenByDescending(r => r.Rundenzeiten?.Count ?? 0)
                .ThenBy(r => r.Zeit.TotalSeconds)
                .ThenBy(r => int.TryParse(r.Startnummer, out var sn) ? sn : int.MaxValue)
                .ToList();

            // 2) PLATZ = innerhalb der Altersklasse (z.B. U9 gemischt m/w)
            foreach (var grp in ordered.GroupBy(r => ExtractAgeClass(r.Klasse)))
            {
                int place = 1;
                foreach (var r in grp)
                    r.Platz = place++;
            }

            // 3) PLATZAK = innerhalb Altersklasse + Geschlecht (U9 m, U9 w, ...)
            foreach (var grp in ordered.GroupBy(r => new { AK = ExtractAgeClass(r.Klasse), G = GenderKey(r.Geschlecht) }))
            {
                int akPlace = 1;
                foreach (var r in grp)
                    r.PlatzAK = akPlace++;
            }

            // 4) Hobby-Sonderregel (dein bisheriger Block) – bleibt, überschreibt ggf. PlatzAK wie gewollt
            if (IsCrossImBadSelected())
            {
                foreach (var r in ordered.Where(x =>
                             x.Klasse?.IndexOf("hobby", StringComparison.OrdinalIgnoreCase) >= 0))
                {
                    var age = GetAgeOnEvent(r.Geburtsdatum);
                    r.AltersgruppeHobby = GetHobbyBucket(age); // "", "Ü40", "Ü50"
                }

                var hobbyBuckets = ordered.Where(x =>
                    x.Klasse?.IndexOf("hobby", StringComparison.OrdinalIgnoreCase) >= 0 &&
                    !string.IsNullOrEmpty(x.AltersgruppeHobby));

                foreach (var grp in hobbyBuckets.GroupBy(x => x.AltersgruppeHobby))
                {
                    int place = 1;
                    foreach (var r in grp
                                 .OrderByDescending(x => x.Rundenzeiten?.Count ?? 0)
                                 .ThenBy(x => x.Zeit.TotalSeconds))
                    {
                        r.PlatzAK = place++;
                    }
                }

                foreach (var r in ordered.Where(x =>
                             x.Klasse?.IndexOf("hobby", StringComparison.OrdinalIgnoreCase) >= 0 &&
                             string.IsNullOrEmpty(x.AltersgruppeHobby)))
                {
                    r.PlatzAK = 0;
                }
            }

            records.Clear();
            records.AddRange(ordered);
        }

        private int ExtractAgeClass(string klasse)
        {
            if (string.IsNullOrWhiteSpace(klasse))
                return int.MaxValue;

            klasse = klasse.Trim().ToUpper();

            int p1 = klasse.IndexOf('(');
            if (p1 >= 0)
            {
                int p2 = klasse.IndexOf(')', p1);
                if (p2 > p1)
                    klasse = klasse.Remove(p1, p2 - p1 + 1).Trim();
            }

            if (klasse.StartsWith("U"))
            {
                string num = new string(klasse.Skip(1).TakeWhile(char.IsDigit).ToArray());
                if (int.TryParse(num, out int age))
                    return age;
            }

            return int.MaxValue;
        }

        // ===========================================================
        //  GRID-AUSGABE
        // ===========================================================
        private void DisplayInGrid(List<CSVRecord> records)
        {
            var table = new DataTable();

            table.Columns.Add("Platz", typeof(int));
            table.Columns.Add("PlatzAK", typeof(int));
            table.Columns.Add("Startnummer", typeof(int));

            table.Columns.Add("Name", typeof(string));
            table.Columns.Add("Vorname", typeof(string));
            table.Columns.Add("Verein", typeof(string));
            table.Columns.Add("Geschlecht", typeof(string));
            table.Columns.Add("Klasse", typeof(string));
            table.Columns.Add("Zeit", typeof(string));
            table.Columns.Add("Rundenzeiten", typeof(string));

            table.Columns.Add("GeschlechtSort", typeof(int)); // 0=m, 1=w, 2=sonst
            table.Columns.Add("AK", typeof(int));             // numerisch aus Klasse

            int GenderKey(string g)
            {
                if (string.IsNullOrWhiteSpace(g)) return 2;
                g = g.Trim().ToLowerInvariant();
                if (g.StartsWith("m")) return 0;
                if (g.StartsWith("w")) return 1;
                return 2;
            }

            foreach (var r in records)
            {
                int.TryParse(r.Startnummer, out var startnr);
                int ak = ExtractAgeClass(r.Klasse);
                int gSort = GenderKey(r.Geschlecht);

                string rundenText = "[" + string.Join(", ",
                    (r.Rundenzeiten ?? new List<double>())
                        .Select(sec =>
                            TimeSpan.FromSeconds(double.IsFinite(sec) && sec >= 0 ? sec : 0)
                                .ToString(@"mm\:ss"))
                ) + "]";

                table.Rows.Add(
                    r.Platz,
                    r.PlatzAK,
                    startnr,
                    r.Name ?? "",
                    r.Vorname ?? "",
                    r.Verein ?? "",
                    r.Geschlecht ?? "",
                    r.Klasse ?? "",
                    r.Zeit.ToString(@"hh\:mm\:ss") + $" ({r.Rundenzeiten?.Count ?? 0} Runden)",
                    rundenText,
                    gSort,
                    ak
                );
            }

            var view = table.DefaultView;
            view.Sort = "Klasse ASC, GeschlechtSort ASC, PlatzAK ASC";

            resultGrid.DataSource = view;

            if (resultGrid.Columns.Contains("AK")) resultGrid.Columns["AK"].Visible = false;
            if (resultGrid.Columns.Contains("GeschlechtSort")) resultGrid.Columns["GeschlechtSort"].Visible = false;
        }

        // ===========================================================
        //  LEERE EVENT-HANDLER (vom Designer)
        // ===========================================================
        private void remainingTime_Click(object sender, EventArgs e) { }
        private void label4_Click(object sender, EventArgs e) { }
        private void Form1_Load(object sender, EventArgs e) { }
        private void label6_Click(object sender, EventArgs e) { }
        private void pictureBox1_Click(object sender, EventArgs e) { }

        private void classStatusLabel_Click(object sender, EventArgs e) { }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {

        }

        private void raceNameComboBox_SelectedIndexChanged_1(object sender, EventArgs e)
        {

        }

        private void chkKlassenstarts_CheckedChanged(object sender, EventArgs e)
        {

        }
    }
}
