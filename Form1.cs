using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;

using OfficeOpenXml;
using OfficeOpenXml.Style;
using OfficeOpenXml.Table;

namespace Rundenzeiten
{
    public partial class Form1 : Form
    {
        private DateTime start = DateTime.MinValue;
        private DateTime end;
        private System.Windows.Forms.Timer countdownTimer;

        private List<PersonEntry> entries = new List<PersonEntry>();
        private List<RoundEntry> roundEntries = new List<RoundEntry>();
        private string resultFilePath = string.Empty;
        // Wellenstarts
        private DateTime raceStart = DateTime.MinValue;
        private readonly Dictionary<string, DateTime> classStartTimes =
           new Dictionary<string, DateTime>(StringComparer.OrdinalIgnoreCase);
        // Mehrfachstart (gleichzeitig, ohne Versatz)
        private Button startSelectedClassesBtn;

        // Helfer
        private bool RaceRunning => startRaceBtn.Text == "Rennen beenden";
        //private bool IsClassStarted(string k) => !string.IsNullOrWhiteSpace(k) && classStartTimes.ContainsKey(k);
        private static string Norm(string s) => (s ?? string.Empty).Trim();

        public Form1()
        {
            InitializeComponent();

            classMultiList.DrawMode = DrawMode.OwnerDrawFixed;
            classMultiList.DrawItem += classMultiList_DrawItem;
            classMultiList.ItemCheck += classMultiList_ItemCheck;   // blockt Umschalten bei gestartetenclassMultiList.ItemCheck += classMultiList_ItemCheck;   // blockt Umschalten bei gestarteten
            classMultiList.MouseDown += classMultiList_MouseDown;


            this.startClassBtn.Click += new System.EventHandler(this.startClassBtn_Click);

            // Veranstaltung ändern -> UI umschalten
            raceNameComboBox.SelectedIndexChanged += raceNameComboBox_SelectedIndexChanged;

            // Veranstaltungsliste
            raceNameComboBox.Items.AddRange(new object[]
            {
             "CrossImBad",  // <-- Schreibweise vereinheitlicht
             "Wintercross",
             "Gravelride",
             "Test"
            });
            raceNameComboBox.SelectedIndex = -1;

            // Ereignisse verknüpfen
            classMultiList.SelectedIndexChanged += (s, e) => UpdateClassStartUIState();
            //startSelectedClassesBtn.Click += startSelectedClassesBtn_Click;

            countdownTimer = new System.Windows.Forms.Timer();
            countdownTimer.Interval = 500;
            countdownTimer.Tick += CountdownTimer_Tick;
            countdownTimer.Start();

            // Startnummerfeld & Button sperren bis Rennstart
            startNumberInput.Enabled = false;
            enterRoundBtn.Enabled = false;
        }

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
            //confirmDurationBtn.Enabled = false;
            //raceDuration.Enabled = false;
            remainingTime.Text = FormatSecondsToMMSS(Convert.ToInt32(raceDuration.Value * 60));

            confirmDurationBtn.BackColor = Color.LightGreen;
        }

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
            entries = ReadCsvFile(filePath);
            if (entries == null)
            {
                starterListLabel.Text = "Fehler: Doppelte Startnummern";
                return;
            }

            if (entries.Count < 1)
            {
                starterListLabel.Text = "Keine Teilnehmer gefunden";
                return;
            }

            // Ergebnis-Dateipfad ableiten
            string fileName = Path.GetFileNameWithoutExtension(filePath);
            string rennenNummer = ExtractRaceNumber(fileName);
            resultFilePath = GetResultFilePath(rennenNummer);

            // ------ Klassenliste befüllen (nur CrossImBad nutzt Klassenstarts) ------
            if (IsCrossImBadSelected())
            {
                // Klassen-Namen normalisieren, Duplikate case-insensitiv entfernen, sinnvoll sortieren
                var classes = entries
                    .Select(e => Norm(e.Klasse))
                    .Where(k => !string.IsNullOrWhiteSpace(k))
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .OrderBy(k => ExtractAgeClass(k))
                    .ThenBy(k => k, StringComparer.OrdinalIgnoreCase)
                    .ToList();

                // Mehrfachliste füllen
                classMultiList.Items.Clear();
                foreach (var k in classes)
                    classMultiList.Items.Add(k, false);

                classMultiList.Enabled = classes.Count > 0;

                // Bereits vorher gestartete Klassen sofort „sperren“ (grau + nicht anklickbar)
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
                // Bei anderen Veranstaltungen ist die Klassenliste aus
                classMultiList.Items.Clear();
                classMultiList.Enabled = false;
            }
            // ------------------------------------------------------------------------

            // CSV/Excel sofort einmal erzeugen & UI-Status aktualisieren
            saveCSV();
            starterListLabel.Text = "Teilnehmer geladen";
            starterListBtn.BackColor = Color.LightGreen;
            startRaceBtn.Enabled = true;

            UpdateClassStartUIState();
            UpdateClassStatusLabel();
        }


        private string ExtractRaceNumber(string fileName)
        {
            // akzeptiert "Rennen6", "Rennen_6", "rennen 6"
            var m = Regex.Match(fileName, @"[Rr]ennen[_ ]*(\d+)");
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

            // Ordner ".\Ergebnisse" neben der EXE
            string resultsDir = Path.Combine(Application.StartupPath, "Ergebnisse");
            Directory.CreateDirectory(resultsDir); // sicherstellen, dass er existiert

            string fileName = $"Ergebnisse_{veranstaltung}_Rennen{rennenNummer}_{date}.csv";
            return Path.Combine(resultsDir, fileName);
        }

        private List<PersonEntry> ReadCsvFile(string path)
        {
            List<PersonEntry> result = new List<PersonEntry>();

            using (var reader = new StreamReader(path))
            {
                bool isFirstLine = true;

                while (!reader.EndOfStream)
                {
                    var line = reader.ReadLine();

                    if (isFirstLine)
                    {
                        isFirstLine = false; // skip header
                        continue;
                    }

                    var fields = line.Split(';');

                    if (fields.Length >= 8)
                    {
                        if (result.Any(x => x.Startnummer == fields[0]))
                            return null;

                        result.Add(new PersonEntry
                        {
                            Startnummer = fields[0],
                            Name = fields[1],
                            Vorname = fields[2],
                            Geschlecht = fields[3],
                            Verein = fields[4],
                            Geburtsdatum = fields[5],
                            Strecke = fields[6],
                            Klasse = fields[7]
                        });
                    }
                }
            }

            return result;
        }
        private bool IsCrossImBadSelected()
        {
            return string.Equals(raceNameComboBox.SelectedItem?.ToString(), "CrossImBad", StringComparison.OrdinalIgnoreCase);
        }
        private void startClassBtn_Click(object sender, EventArgs e)
        {
            // still arbeiten, keine Popups
            if (!IsCrossImBadSelected()) return;
            if (!RaceRunning) return;
            if (classMultiList == null || classMultiList.Items.Count == 0) return;

            // Angekreuzte Klassen einsammeln (normalisiert, doppelte vermeiden)
            var toStart = classMultiList.CheckedItems
                .Cast<object>()
                .Select(it => Norm(it?.ToString()))                     // Norm = (s??"").Trim()
                .Where(k => !string.IsNullOrWhiteSpace(k))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();

            if (toStart.Count == 0)
                return;

            // Nur Klassen starten, die NOCH NICHT gestartet sind
            var fresh = toStart.Where(k => !classStartTimes.ContainsKey(k)).ToList();
            if (fresh.Count == 0)
                return;

            var t0 = DateTime.Now;

            // Startzeit nur für neue Klassen setzen (niemals überschreiben)
            foreach (var k in fresh)
                classStartTimes[k] = t0;

            // UI: frisch gestartete in der Liste sperren & Häkchen entfernen
            for (int i = 0; i < classMultiList.Items.Count; i++)
            {
                var name = Norm(classMultiList.Items[i]?.ToString());
                if (fresh.Contains(name, StringComparer.OrdinalIgnoreCase))
                {
                    classMultiList.SetItemCheckState(i, CheckState.Indeterminate); // „ausgegraut“
                    classMultiList.SetItemChecked(i, false);                       // kein Haken mehr
                }
            }

            // neu zeichnen & Status aktualisieren
            classMultiList.Invalidate();
            UpdateClassStatusLabel();
            UpdateClassStartUIState();
        }


        //gestartete Klassen blockieren
        private void classMultiList_MouseDown(object sender, MouseEventArgs e)
        {
            int index = classMultiList.IndexFromPoint(e.Location);
            if (index < 0) return;

            string klasse = Norm(classMultiList.Items[index]?.ToString());
            if (classStartTimes.ContainsKey(klasse))
            {
                // Klick ignorieren (kein Umschalten)
                return;
            }
        }
        private void classMultiList_ItemCheck(object sender, ItemCheckEventArgs e)
        {
            // Name der Klasse holen
            string name = Norm(classMultiList.Items[e.Index]?.ToString());

            // Wenn schon gestartet → gewünschte neue Zustandsänderung ignorieren
            if (classStartTimes.ContainsKey(name))
            {
                e.NewValue = e.CurrentValue; // verhindert das Umschalten (Maus & Tastatur)
            }
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

        public static string FormatSecondsToMMSS(int totalSeconds)
        {
            bool isNegative = totalSeconds < 0;
            totalSeconds = Math.Abs(totalSeconds);

            int minutes = totalSeconds / 60;
            int seconds = totalSeconds % 60;

            string formatted = $"{minutes:D2}:{seconds:D2}";
            return isNegative ? "-" + formatted : formatted;
        }
        private void raceNameComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            UpdateClassStartUIState();
        }

        private void UpdateClassStartUIState()
        {
            bool canUse = IsCrossImBadSelected() && entries != null && entries.Count > 0;
            bool anyFreshChecked = false;
            if (classMultiList != null)
            {
                classMultiList.Enabled = canUse;
                var checkedItems = classMultiList.CheckedItems.Cast<string>().ToList();
                anyFreshChecked = checkedItems.Any(k => !classStartTimes.ContainsKey(k));
            }
        }
        private void UpdateClassStatusLabel()
        {
            if (classStatusLabel == null) return;

            bool ready = IsCrossImBadSelected() && entries != null && entries.Count > 0
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
                if (started.Count > 0)
                    //sb.AppendLine(); // nur eine Zeile Abstand, optional: kannst du löschen wenn du gar keine willst
                sb.AppendLine("⏱️ offen:");
                foreach (var o in open)
                    sb.AppendLine("• " + o);
            }

            if (started.Count == 0 && open.Count > 0 && RaceRunning)
                sb.AppendLine("(wähle Klassen und drücke „Klasse starten“)");

            classStatusLabel.Text = sb.ToString().TrimEnd();
        }

        private void startRaceBtn_Click(object sender, EventArgs e)
        {
            // wurde eine Veranstaltung ausgewählt?
            if (raceNameComboBox.SelectedIndex == -1)
            {
                MessageBox.Show("Bitte zuerst eine Veranstaltung auswählen!",
                                "Hinweis",
                                MessageBoxButtons.OK,
                                MessageBoxIcon.Warning);
                return; // Rennen nicht starten
            }

            if (startRaceBtn.Text == "Rennen beenden")
            {
                saveCSV();
                Environment.Exit(0);
            }

            start = DateTime.Now;
            raceStart = start;

            if (IsCrossImBadSelected())
            {
                startClassBtn.Enabled = true; // jetzt dürfen Klassen manuell gestartet werden
            }
            else
            {
                startClassBtn.Enabled = false;
                classStartTimes.Clear();

            }

            end = start.AddMinutes(Convert.ToInt32(raceDuration.Value));

            startRaceBtn.BackColor = Color.Red;
            startRaceBtn.Text = "Rennen beenden";
            startNumberInput.Enabled = true;
            enterRoundBtn.Enabled = true;

            //Sperren erst jetzt:
            raceNameComboBox.Enabled = false;
            starterListBtn.Enabled = false;
            raceDuration.Enabled = false;
            confirmDurationBtn.Enabled = false;
            UpdateClassStartUIState();

        }

        private void enterRoundBtn_Click(object sender, EventArgs e)
        {
            int startNumber = 0;

            try
            {
                startNumber = Convert.ToInt32(startNumberInput.Text);
            }
            catch (Exception)
            { }

            enterRound(startNumber);
        }

        private void enterRound(int startnumber)
        {
            startNumberInput.Text = "";
            if (startnumber < 0 && roundEntries.Count > 0)
            {
                roundResultLabel.Text = "Letzten Eintrag gelöscht";
                roundResultLabel.ForeColor = Color.Green;
                roundEntries.RemoveAt(roundEntries.Count - 1);
                saveCSV();
                return;
            }


            PersonEntry person = entries.Where(x => x.Startnummer == startnumber.ToString()).FirstOrDefault();

            if (person == null)
            {
                roundResultLabel.Text = "Startnummer ungültig!";
                roundResultLabel.ForeColor = Color.Red;
                return;
            }


            List<RoundEntry> personEntries = roundEntries.Where(x => x.Startnummer == person.Startnummer).ToList();
            double toAdd = personEntries.Select(x => x.Time.TotalSeconds).Sum();

            TimeSpan lapTime;

            if (IsCrossImBadSelected())
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

            roundEntries.Add(new RoundEntry()
            {
                Startnummer = person.Startnummer,
                Time = lapTime
            });

            saveCSV();

        }

        private void saveCSV()
        {
            const string Header = "Platz;PlatzAK;Startnummer;Name;Vorname;Geschlecht;Geburtsdatum;Verein;Strecke;Klasse;Zeit;Rundenzeiten";
            List<CSVRecord> records = BuildRecords();
            RankRecords(records);

            // 1) CSV
            SaveToCsvFile(records, Header, resultFilePath);

            // 2) Excel mit "Gesamt" + pro-Klasse-Blättern
            SaveToExcelPerClass(records, resultFilePath);

            // 3) Anzeige
            DisplayInGrid(records);
        }

        private string FindLogoPath()
        {
            string baseDir = Path.Combine(Application.StartupPath, "Vorlagen");
            if (!Directory.Exists(baseDir)) return null;

            string[] candidates = new[]
            {
        "Logo.png", "logo.png", "Logo.jpg", "logo.jpg", "Logo.jpeg", "logo.jpeg"
    };

            foreach (var name in candidates)
            {
                string p = Path.Combine(baseDir, name);
                if (File.Exists(p)) return p;
            }
            // sonst erstes Bild im Ordner nehmen
            var anyImg = Directory.EnumerateFiles(baseDir)
                                  .FirstOrDefault(f => f.EndsWith(".png", StringComparison.OrdinalIgnoreCase)
                                                    || f.EndsWith(".jpg", StringComparison.OrdinalIgnoreCase)
                                                    || f.EndsWith(".jpeg", StringComparison.OrdinalIgnoreCase));
            return anyImg;
        }

        private void InsertLogo(ExcelWorksheet ws, string logoPath, string uniqueNameSuffix = "")
        {
            if (string.IsNullOrEmpty(logoPath) || !File.Exists(logoPath)) return;

            string picName = "Logo" + (string.IsNullOrEmpty(uniqueNameSuffix) ? "" : "_" + uniqueNameSuffix);
            var pic = ws.Drawings.AddPicture(picName, new FileInfo(logoPath));

            // Links oben platzieren, klein
            // (rowIndex, rowOffsetPx, colIndex, colOffsetPx)
            pic.SetPosition(0, 2, 0, 2); // A1-Ecke, bisschen Abstand
            pic.SetSize(35);            // Größe ca. wie im Screenshot
        }

        private static string ToHhMmSs(object zeit)
        {
            if (zeit == null) return "";
            if (zeit is TimeSpan ts) return ts.ToString(@"hh\:mm\:ss");
            return TimeSpan.TryParse(zeit.ToString(), out var t) ? t.ToString(@"hh\:mm\:ss") : zeit.ToString();
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
            // Zieldatei
            string excelPath = Path.Combine(
                Path.GetDirectoryName(filePath)!,
                Path.GetFileNameWithoutExtension(filePath) + "_Druck.xlsx"
            );

            // Zielordner sicher anlegen
            Directory.CreateDirectory(Path.GetDirectoryName(excelPath)!);

            // Veranstaltung & Rennnummer für die Überschrift
            string veranstaltung = raceNameComboBox?.SelectedItem?.ToString() ?? "CrossImBad";
            string rennen = "?";
            var m = Regex.Match(Path.GetFileNameWithoutExtension(filePath), @"Rennen(\d+)");
            if (m.Success) rennen = m.Groups[1].Value;

            string[] header = { "Platz", "PlatzAK", "Startnummer", "Name", "Vorname", "Geschlecht", "Verein", "Klasse", "Zeit", "Rundenzeiten" };
            string logoPath = FindLogoPath();

            // Master-Spaltenbreiten der Gesamtliste, werden später befüllt und auf Klassenblätter angewandt
            double[] masterWidths = null;

            using (var package = new ExcelPackage())
            {
                // ===== Blatt: Gesamt =====================================================================
                var wsAll = package.Workbook.Worksheets.Add("Gesamt");

                // Kopfbereich (Logo/Titel) – so kollidiert nichts
                wsAll.Row(1).Height = 36;  // Logo-Zeile
                wsAll.Row(2).Height = 28;  // Titel-Zeile
                wsAll.Row(3).Height = 6;   // Leer
                wsAll.Row(4).Height = 6;   // Leer

                // Titel mittig
                wsAll.Cells["A2:J2"].Merge = true;
                wsAll.Cells["A2"].Value = $"{veranstaltung} – Ergebnisliste (Rennen {rennen})";
                wsAll.Cells["A2"].Style.Font.Bold = true;
                wsAll.Cells["A2"].Style.Font.Size = 20;
                wsAll.Cells["A2"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

                // Logo links oben (klein)
                InsertLogo(wsAll, logoPath, "Gesamt");

                // Kopfzeile ab Zeile 5
                int headerRowAll = 5;
                for (int c = 0; c < header.Length; c++)
                {
                    var cell = wsAll.Cells[headerRowAll, c + 1];
                    cell.Value = header[c];
                    cell.Style.Font.Bold = true;
                    cell.Style.Font.Color.SetColor(Color.Black);  // Schrift schwarz
                    cell.Style.Fill.PatternType = ExcelFillStyle.Solid;
                    cell.Style.Fill.BackgroundColor.SetColor(Color.LightGray);
                    cell.Style.Border.BorderAround(ExcelBorderStyle.Thin);
                }

                // Daten ab Zeile 6
                int rowAll = headerRowAll + 1;
                foreach (var r in records)
                {
                    wsAll.Cells[rowAll, 1].Value = r?.Platz;
                    wsAll.Cells[rowAll, 2].Value = r?.PlatzAK;
                    wsAll.Cells[rowAll, 3].Value = r?.Startnummer;
                    wsAll.Cells[rowAll, 4].Value = r?.Name ?? "";
                    wsAll.Cells[rowAll, 5].Value = r?.Vorname ?? "";
                    wsAll.Cells[rowAll, 6].Value = r?.Geschlecht ?? "";
                    wsAll.Cells[rowAll, 7].Value = r?.Verein ?? "";
                    wsAll.Cells[rowAll, 8].Value = r?.Klasse ?? "";
                    wsAll.Cells[rowAll, 9].Value = ToHhMmSs(r?.Zeit);
                    wsAll.Cells[rowAll, 10].Value = FormatLapList(r?.Rundenzeiten);
                    rowAll++;
                }

                // Als Excel-Tabelle formatieren
                if (rowAll > headerRowAll + 1)
                {
                    var rangeAll = wsAll.Cells[headerRowAll, 1, rowAll - 1, header.Length];
                    var tblAll = wsAll.Tables.Add(rangeAll, "tbl_Gesamt");
                    tblAll.TableStyle = TableStyles.Medium2;
                }

                wsAll.View.FreezePanes(headerRowAll + 1, 1); // Kopf fixieren

                // DRUCK-geeignete Breiten + Masterbreiten erfassen
                if (wsAll.Dimension != null)
                {
                    // Grund-AutoFit mit Grenzen
                    wsAll.Cells[wsAll.Dimension.Address].AutoFitColumns(8, 28);

                    // Exakte Wunschbreiten
                    wsAll.Column(1).Width = 8;   // Platz
                    wsAll.Column(2).Width = 10;   // PlatzAK
                    wsAll.Column(3).Width = 14;  // Startnummer
                    wsAll.Column(4).Width = 16;  // Name
                    wsAll.Column(5).Width = 14;  // Vorname
                    wsAll.Column(6).Width = 10;  // Geschlecht
                    wsAll.Column(7).Width = 30;  // Verein
                    wsAll.Column(8).Width = 8;  // Klasse
                    wsAll.Column(9).Width = 10;  // Zeit
                    wsAll.Column(10).Width = 32;  // Rundenzeiten

                    // Lange Texte umbrechen
                    wsAll.Cells[headerRowAll + 1, 7, rowAll - 1, 7].Style.WrapText = true;  // Verein
                    wsAll.Cells[headerRowAll + 1, 10, rowAll - 1, 10].Style.WrapText = true;  // Rundenzeiten

                    wsAll.Row(headerRowAll).Height = 20;

                    // <- Masterbreiten sichern, damit alle Klassenblätter identisch sind
                    masterWidths = new double[header.Length];
                    for (int c = 1; c <= header.Length; c++)
                        masterWidths[c - 1] = wsAll.Column(c).Width;
                }

                wsAll.PrinterSettings.Orientation = eOrientation.Landscape;
                wsAll.PrinterSettings.FitToPage = true;
                wsAll.PrinterSettings.FitToWidth = 1;
                wsAll.PrinterSettings.FitToHeight = 0;

                // ====================== Pro Klasse ein eigenes Blatt =====================================
                var groups = records.GroupBy(r => r?.Klasse ?? "Unbekannt")
                                    .OrderBy(g => g.Key);

                int linkRow = 2; // Links auf "Gesamt" (Spalte L)
                foreach (var g in groups)
                {
                    string rawName = g.Key;
                    string sheetName = SanitizeName(rawName);
                    string uniqueName = sheetName;
                    int suffix = 2;
                    while (package.Workbook.Worksheets.Any(ws => ws.Name.Equals(uniqueName, StringComparison.OrdinalIgnoreCase)))
                        uniqueName = SanitizeName($"{sheetName}_{suffix++}");

                    var ws = package.Workbook.Worksheets.Add(uniqueName);

                    // Kopfbereich wie oben
                    ws.Row(1).Height = 36;
                    ws.Row(2).Height = 28;
                    ws.Row(3).Height = 6;
                    ws.Row(4).Height = 6;

                    ws.Cells["A2:J2"].Merge = true;
                    ws.Cells["A2"].Value = $"{veranstaltung} – Ergebnisliste – {rawName} (Rennen {rennen})";
                    ws.Cells["A2"].Style.Font.Bold = true;
                    ws.Cells["A2"].Style.Font.Size = 20;
                    ws.Cells["A2"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

                    // Logo pro Klassenblatt
                    InsertLogo(ws, logoPath, uniqueName);

                    // Kopfzeile ab Zeile 5
                    int headerRow = 5;
                    for (int c = 0; c < header.Length; c++)
                    {
                        var cell = ws.Cells[headerRow, c + 1];
                        cell.Value = header[c];
                        cell.Style.Font.Bold = true;
                        cell.Style.Font.Color.SetColor(Color.Black);  // Schrift schwarz
                        cell.Style.Fill.PatternType = ExcelFillStyle.Solid;
                        cell.Style.Fill.BackgroundColor.SetColor(Color.LightGray);
                        cell.Style.Border.BorderAround(ExcelBorderStyle.Thin);
                    }

                    // Daten der Klasse (ab Zeile 6)
                    var data = g.OrderBy(r => r?.Platz ?? int.MaxValue).ToList();
                    int row = headerRow + 1;
                    foreach (var r in data)
                    {
                        ws.Cells[row, 1].Value = r?.Platz;
                        ws.Cells[row, 2].Value = r?.PlatzAK;
                        ws.Cells[row, 3].Value = r?.Startnummer;
                        ws.Cells[row, 4].Value = r?.Name ?? "";
                        ws.Cells[row, 5].Value = r?.Vorname ?? "";
                        ws.Cells[row, 6].Value = r?.Geschlecht ?? "";
                        ws.Cells[row, 7].Value = r?.Verein ?? "";
                        ws.Cells[row, 8].Value = r?.Klasse ?? "";
                        ws.Cells[row, 9].Value = ToHhMmSs(r?.Zeit);
                        ws.Cells[row, 10].Value = FormatLapList(r?.Rundenzeiten);
                        row++;
                    }

                    // Tabelle anwenden
                    if (row > headerRow + 1)
                    {
                        var range = ws.Cells[headerRow, 1, row - 1, header.Length];
                        var tableName = "tbl_" + Regex.Replace(uniqueName, @"\W", "");
                        var tbl = ws.Tables.Add(range, tableName);
                        tbl.TableStyle = TableStyles.Medium2;
                    }

                    // Breiten identisch zur Gesamtliste übernehmen
                    if (ws.Dimension != null)
                    {
                        // optionales AutoFit zuerst
                        ws.Cells[ws.Dimension.Address].AutoFitColumns(8, 28);

                        if (masterWidths != null)
                        {
                            for (int c = 1; c <= header.Length; c++)
                                ws.Column(c).Width = masterWidths[c - 1];
                        }

                        // gleiche Text-Optionen für Verein & Rundenzeiten
                        ws.Cells[headerRow + 1, 7, row - 1, 7].Style.WrapText = true;
                        ws.Cells[headerRow + 1, 10, row - 1, 10].Style.WrapText = true;

                        ws.Row(headerRow).Height = 20;
                    }

                    ws.View.FreezePanes(headerRow + 1, 1);
                    ws.PrinterSettings.Orientation = eOrientation.Landscape;
                    ws.PrinterSettings.FitToPage = true;
                    ws.PrinterSettings.FitToWidth = 1;
                    ws.PrinterSettings.FitToHeight = 0;

                    // Link von "Gesamt" auf das Klassenblatt (Spalte L)
                    wsAll.Cells[linkRow, 12].Hyperlink = new ExcelHyperLink($"'{uniqueName}'!A1", rawName);
                    wsAll.Cells[linkRow, 12].Value = rawName;
                    linkRow++;
                }

                package.SaveAs(new FileInfo(excelPath));
            }
        }

        private void DisplayInGrid(List<CSVRecord> records)
        {
            var table = new DataTable();

            // Zahlen
            table.Columns.Add("Platz", typeof(int));
            table.Columns.Add("PlatzAK", typeof(int));
            table.Columns.Add("Startnummer", typeof(int));

            // Texte
            table.Columns.Add("Name", typeof(string));
            table.Columns.Add("Vorname", typeof(string));
            table.Columns.Add("Verein", typeof(string));
            table.Columns.Add("Geschlecht", typeof(string));
            table.Columns.Add("Klasse", typeof(string));
            table.Columns.Add("Zeit", typeof(string));
            table.Columns.Add("Rundenzeiten", typeof(string));

            // Hilfsspalten für Sortierung
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

                // Rundenzeiten "mm:ss"
                string rundenText = "[" + string.Join(", ",
                    (r.Rundenzeiten ?? new List<double>())
                        .Select(sec => TimeSpan.FromSeconds(double.IsFinite(sec) && sec >= 0 ? sec : 0).ToString(@"mm\:ss"))
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
                    gSort,       // <-- Reihenfolge korrigiert (erst GeschlechtSort)
                    ak           // <-- dann AK
                );
            }

            var view = table.DefaultView;
            // Sortiert: Klasse ↑, m vor w, PlatzAK ↑
            view.Sort = "Klasse ASC, GeschlechtSort ASC, PlatzAK ASC";

            resultGrid.DataSource = view;

            // Hilfsspalten ausblenden
            if (resultGrid.Columns.Contains("AK")) resultGrid.Columns["AK"].Visible = false;
            if (resultGrid.Columns.Contains("GeschlechtSort")) resultGrid.Columns["GeschlechtSort"].Visible = false;
        }

        private void SaveToCsvFile(List<CSVRecord> records, string header, string filePath)
        {
            var lines = new List<string> { header };
            foreach (var record in records)
            {
                var laps = record.Rundenzeiten ?? new List<double>();
                string lapsRaw = "[" + string.Join(", ", laps.Select(x => x.ToString(System.Globalization.CultureInfo.InvariantCulture))) + "]";
                string line =
                    $"{record.Platz};{record.PlatzAK};{record.Startnummer};{record.Name};{record.Vorname};" +
                    $"{record.Geschlecht};{record.Geburtsdatum};{record.Verein};{record.Strecke};{record.Klasse};" +
                    $"{record.Zeit:hh\\:mm\\:ss} ({laps.Count} Runden);{lapsRaw}";
                lines.Add(line);
            }

            Directory.CreateDirectory(Path.GetDirectoryName(filePath)!);
            File.WriteAllLines(filePath, lines, new UTF8Encoding(true)); // UTF-8 mit BOM
        }


        private void RankRecords(List<CSVRecord> records)
        {
            // Geschlecht zu Rang 0/1 m/w (null-sicher)
            int GenderKey(string g)
            {
                if (string.IsNullOrWhiteSpace(g)) return 2;
                g = g.Trim().ToLowerInvariant();
                if (g.StartsWith("m")) return 0; // männlich/m
                if (g.StartsWith("w")) return 1; // weiblich/w
                return 2;
            }

            var ranked = records
                .OrderBy(r => ExtractAgeClass(r.Klasse))               // Klasse (U11, U13, …)
                .ThenBy(r => GenderKey(r.Geschlecht))                  // m vor w
                .ThenByDescending(r => r.Rundenzeiten?.Count ?? 0)     // mehr Runden zuerst
                .ThenBy(r => r.Zeit.TotalSeconds)                      // schnellere Zeit zuerst
                .ToList();

            // Platz (gesamt)
            for (int i = 0; i < ranked.Count; i++)
                ranked[i].Platz = i + 1;

            // PlatzAK je Klasse UND Geschlecht
            foreach (var group in ranked.GroupBy(r => new { r.Klasse, Key = GenderKey(r.Geschlecht) }))
            {
                int akPlatz = 1;
                foreach (var r in group.OrderBy(x => x.Platz))
                    r.PlatzAK = akPlatz++;
            }

            records.Clear();
            records.AddRange(ranked);
        }

        private int ExtractAgeClass(string klasse)
        {
            if (string.IsNullOrWhiteSpace(klasse))
                return int.MaxValue; // falls leer, nach hinten

            klasse = klasse.Trim().ToUpper();

            // Klammerzusätze entfernen → "U13 (m/w)" → "U13"
            int p1 = klasse.IndexOf('(');
            if (p1 >= 0)
            {
                int p2 = klasse.IndexOf(')', p1);
                if (p2 > p1)
                    klasse = klasse.Remove(p1, p2 - p1 + 1).Trim();
            }

            // "U11", "U13", … → Zahl extrahieren
            if (klasse.StartsWith("U"))
            {
                string num = new string(klasse.Skip(1).TakeWhile(char.IsDigit).ToArray());
                if (int.TryParse(num, out int age))
                    return age;
            }

            // keine U-Klasse → nach hinten sortieren
            return int.MaxValue;
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
                    Zeit = TimeSpan.FromSeconds(rundenzeiten.Sum())
                });
            }

            return result;
        }

        private void remainingTime_Click(object sender, EventArgs e)
        {

        }

        private void label4_Click(object sender, EventArgs e)
        {

        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {

        }

        private void Form1_Load(object sender, EventArgs e)
        {

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
                catch (Exception)
                { }

                enterRound(startNumber);
            }
        }

        private void label6_Click(object sender, EventArgs e)
        {

        }

        private void pictureBox1_Click(object sender, EventArgs e)
        {

        }

        private void classStatusLabel_Click(object sender, EventArgs e)
        {

        }
    }
}
