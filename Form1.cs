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

        //private System.Windows.Forms.ComboBox raceNameComboBox;                  // CS8618 fix

        public Form1()
        {
            InitializeComponent();

            raceNameComboBox.Items.AddRange(new object[]
            {
             "CrossimBad",
             "Wintercross",
             "Gravelride",
             "Test"
            });
            raceNameComboBox.SelectedIndex = -1;


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
            OpenFileDialog ofd = new OpenFileDialog
            {
                Filter = "CSV files (*.csv)|*.csv|All files (*.*)|*.*",
                Title = "Select CSV file"
            };

            if (ofd.ShowDialog() == DialogResult.OK)
            {
                string filePath = ofd.FileName;
                entries = ReadCsvFile(filePath);

                if (entries == null)
                {
                    starterListLabel.Text = "Fehler: Doppelte Startnummern";
                    return;
                }

                if (entries.Count > 1)
                {
                    string fileName = Path.GetFileNameWithoutExtension(filePath);
                    string rennenNummer = ExtractRaceNumber(fileName);   // z.B. "6"
                    resultFilePath = GetResultFilePath(rennenNummer);

                    saveCSV();
                    starterListLabel.Text = "Teilnehmer geladen";
                    starterListBtn.BackColor = Color.LightGreen;
                    //starterListBtn.Enabled = false;
                    startRaceBtn.Enabled = true;
                }
            }
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
            return $"Ergebnisse_{veranstaltung}_Rennen{rennenNummer}_{date}.csv";
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

        public static string FormatSecondsToMMSS(int totalSeconds)
        {
            bool isNegative = totalSeconds < 0;
            totalSeconds = Math.Abs(totalSeconds);

            int minutes = totalSeconds / 60;
            int seconds = totalSeconds % 60;

            string formatted = $"{minutes:D2}:{seconds:D2}";
            return isNegative ? "-" + formatted : formatted;
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

            roundResultLabel.Text = $"Eingetragen - {person.Startnummer} - {Math.Round((DateTime.Now - start.AddSeconds(toAdd)).TotalSeconds, 1)}s";
            roundResultLabel.ForeColor = Color.Green;

            roundEntries.Add(new RoundEntry()
            {
                Startnummer = person.Startnummer,
                Time = DateTime.Now - start.AddSeconds(toAdd)
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

        // File.WriteAllLines(printablePath, lines, Encoding.UTF8);
        //}
        // Sucht im EXE-Ordner (\Vorlagen\) nach einer Logo-Datei (png/jpg/jpeg)
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

        // Fügt das Logo oben links ein (Position & Größe leicht anpassbar)
        
        private void InsertLogo(ExcelWorksheet ws, string logoPath, string uniqueNameSuffix = "")
        {
            if (string.IsNullOrEmpty(logoPath) || !File.Exists(logoPath)) return;

            string picName = "Logo" + (string.IsNullOrEmpty(uniqueNameSuffix) ? "" : "_" + uniqueNameSuffix);
            var pic = ws.Drawings.AddPicture(picName, new FileInfo(logoPath));

            // Links oben platzieren, klein
            // (rowIndex, rowOffsetPx, colIndex, colOffsetPx)
            pic.SetPosition(0, 2, 0, 2); // A1-Ecke, bisschen Abstand
            pic.SetSize(120);            // Größe ca. wie im Screenshot
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
        // ======= NEU: Excel-Ausgabe mit einem Blatt pro Klasse =======
        private static string FormatLapList(IList<double> lapsSec)
        {
            if (lapsSec == null) return "";
            var parts = new List<string>();
            foreach (var v in lapsSec)
            {
                double d = v;
                // Ungültige Werte abfangen
                if (double.IsNaN(d) || double.IsInfinity(d) || d < 0 || d > TimeSpan.MaxValue.TotalSeconds)
                    d = 0;

                parts.Add(TimeSpan.FromSeconds(d).ToString(@"mm\:ss")); // nur EIN Backslash vor dem Doppelpunkt
            }
            return string.Join(", ", parts);
        }

        private void SaveToExcelPerClass(List<CSVRecord> records, string filePath)
        {
            // Zieldatei
            string excelPath = Path.Combine(
                Path.GetDirectoryName(filePath),
                Path.GetFileNameWithoutExtension(filePath) + "_Druck.xlsx"
            );

            // Veranstaltung & Rennnummer für die Überschrift
            string veranstaltung = raceNameComboBox?.SelectedItem?.ToString() ?? "CrossImBad";
            string rennen = "?";
            var m = Regex.Match(Path.GetFileNameWithoutExtension(filePath), @"Rennen(\d+)");
            if (m.Success) rennen = m.Groups[1].Value;

            string[] header = { "Platz", "PlatzAK", "Startnummer", "Name", "Vorname", "Geschlecht", "Verein", "Klasse", "Zeit", "Rundenzeiten" };
            string logoPath = FindLogoPath();

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

                // Logo links oben
                InsertLogo(wsAll, logoPath, "Gesamt");

                // Kopfzeile ab Zeile 5
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
                if (wsAll.Dimension != null) wsAll.Cells[wsAll.Dimension.Address].AutoFitColumns();
                wsAll.PrinterSettings.Orientation = eOrientation.Landscape;
                wsAll.PrinterSettings.FitToPage = true;
                wsAll.PrinterSettings.FitToWidth = 1;
                wsAll.PrinterSettings.FitToHeight = 0;

                // ===== Pro Klasse ein eigenes Blatt ======================================================
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
                        ws.Cells[row, 10].Value = FormatLapList(r?.Rundenzeiten);  // <--- wichtig: ws, nicht wsAll!
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

                    if (ws.Dimension != null) ws.Cells[ws.Dimension.Address].AutoFitColumns();
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

            //Zahlen als int speichern
            table.Columns.Add("Platz", typeof(int));
            table.Columns.Add("PlatzAK", typeof(int));
            table.Columns.Add("Startnummer", typeof(int));

            //Rest bleibt String
            table.Columns.Add("Name", typeof(string));
            table.Columns.Add("Vorname", typeof(string));
            table.Columns.Add("Verein", typeof(string));
            table.Columns.Add("Geschlecht", typeof(string));
            table.Columns.Add("Klasse", typeof(string));
            table.Columns.Add("Zeit", typeof(string));
            table.Columns.Add("Rundenzeiten", typeof(string));

            // 👉 Hilfsspalten für Sortierung
            //table.Columns.Add("AK", typeof(int));          // numerisch aus Klasse
            table.Columns.Add("GeschlechtSort", typeof(int)); // 0 = m, 1 = w
            table.Columns.Add("AK", typeof(int));             // numerische Altersklasse
            foreach (var r in records)
            {
                int startnr;
                int.TryParse(r.Startnummer, out startnr);
                // Altersklasse extrahieren
                int ak = ExtractAgeClass(r.Klasse);

                // Geschlecht sortierbar machen (m vor w)
                int gSort = 2;
                if (!string.IsNullOrEmpty(r.Geschlecht))
                {
                    if (r.Geschlecht.ToLower().StartsWith("m")) gSort = 0;
                    else if (r.Geschlecht.ToLower().StartsWith("w")) gSort = 1;
                }

                // Rundenzeiten schön als mm,ss formatieren
                string rundenText = "[" + string.Join(", ",
                    r.Rundenzeiten.Select(sec => TimeSpan.FromSeconds(sec).ToString(@"mm\,ss"))
                ) + "]";

                table.Rows.Add(
                    r.Platz,
                    r.PlatzAK,
                    startnr,
                    r.Name,
                    r.Vorname,
                    r.Verein,
                    r.Geschlecht, 
                    r.Klasse,
                    r.Zeit.ToString(@"hh\:mm\:ss") + $" ({r.Rundenzeiten.Count} Runden)",
                     rundenText, //$"[{string.Join(", ", r.Rundenzeiten)}]",
                    ak, gSort
                );
            }

            //DataView mit Sortierung nach Platz
            var view = table.DefaultView;
            view.Sort = "Klasse ASC, Geschlecht ASC, PlatzAK ASC";      // immer automatisch aufsteigend
            resultGrid.DataSource = view;
            //Hilfsspalten ausblenden (nur für Sortierung da)
            resultGrid.Columns["AK"].Visible = false;
            resultGrid.Columns["GeschlechtSort"].Visible = false;
        }

        private void SaveToCsvFile(List<CSVRecord> records, string header, string filePath)
        {
            var lines = new List<string> { header };

            foreach (var record in records)
            {
                string line = $"{record.Platz};{record.PlatzAK};{record.Startnummer};{record.Name};{record.Vorname};{record.Geschlecht};{record.Geburtsdatum};{record.Verein};{record.Strecke};{record.Klasse};{record.Zeit.ToString(@"hh\:mm\:ss")} ({record.Rundenzeiten.Count} Runden);[{string.Join(", ", record.Rundenzeiten)}]";
                lines.Add(line);
            }

            File.WriteAllLines(filePath, lines, new UTF8Encoding(encoderShouldEmitUTF8Identifier: true));
        }

        private void RankRecords(List<CSVRecord> records)
        {
            //Liste direkt sortieren
            var ranked = records
                // 1. nach Altersklasse (z.B. "U11", "U13", …)
                .OrderBy(r => ExtractAgeClass(r.Klasse))
                .OrderBy(r => r.Geschlecht.ToLower() == "männlich" || r.Geschlecht.ToLower() == "m" ? 0 : 1)
                .ThenByDescending(r => r.Rundenzeiten.Count)
                .ThenBy(r => r.Zeit.TotalSeconds)
                .ToList();

            //Platz setzen
            for (int i = 0; i < ranked.Count; i++)
                ranked[i].Platz = i + 1;

            // PlatzAK getrennt nach Klasse UND Geschlecht / PlatzAK berechnen
            foreach (var group in ranked.GroupBy(r => new { r.Klasse, r.Geschlecht }))
            {
                int akPlatz = 1;
                foreach (var r in group.OrderBy(x => x.Platz))
                {
                    r.PlatzAK = akPlatz++;
                }
            }

            //Ursprüngliche Liste überschreiben
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
    }
}
