using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Rundenzeiten
{
    public partial class Form1 : Form
    {
        private DateTime start = DateTime.MinValue;
        private DateTime end;
        private System.Windows.Forms.Timer countdownTimer;
        private List<PersonEntry> entries;
        private List<RoundEntry> roundEntries = new List<RoundEntry>();
        private string resultFilePath = "Ergebnisse_" + DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss") + ".csv";

        public Form1()
        {
            InitializeComponent();

            countdownTimer = new System.Windows.Forms.Timer();
            countdownTimer.Interval = 500;
            countdownTimer.Tick += CountdownTimer_Tick;
            countdownTimer.Start();
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
            confirmDurationBtn.Enabled = false;
            raceDuration.Enabled = false;
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
                    saveCSV();
                    starterListLabel.Text = "Teilnehmer geladen";
                    starterListBtn.BackColor = Color.LightGreen;
                    starterListBtn.Enabled = false;
                    startRaceBtn.Enabled = true;
                }
            }
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
            SaveToCsvFile(records, Header, resultFilePath);
            DisplayInGrid(records);
        }


        private void DisplayInGrid(List<CSVRecord> records)
        {
            var table = new DataTable();

            table.Columns.AddRange(new[]
            {
                new DataColumn("Platz"),
                new DataColumn("PlatzAK"),
                new DataColumn("Startnummer"),
                new DataColumn("Name"),
                new DataColumn("Vorname"),
                new DataColumn("Verein"),
                new DataColumn("Klasse"),
                new DataColumn("Zeit"),
                new DataColumn("Rundenzeiten")
            });

            foreach (var r in records)
            {
                table.Rows.Add(
                    r.Platz,
                    r.PlatzAK,
                    r.Startnummer,
                    r.Name,
                    r.Vorname,
                    r.Verein,
                    r.Klasse,
                    r.Zeit.ToString(@"hh\:mm\:ss") + $" ({r.Rundenzeiten.Count} Runden)",
                    $"[{string.Join(", ", r.Rundenzeiten)}]"
                );
            }

            resultGrid.DataSource = table;
        }

        private void SaveToCsvFile(List<CSVRecord> records, string header, string filePath)
        {
            var lines = new List<string> { header };

            foreach (var record in records)
            {
                string line = $"{record.Platz};{record.PlatzAK};{record.Startnummer};{record.Name};{record.Vorname};{record.Geschlecht};{record.Geburtsdatum};{record.Verein};{record.Strecke};{record.Klasse};{record.Zeit.ToString(@"hh\:mm\:ss")} ({record.Rundenzeiten.Count} Runden);[{string.Join(", ", record.Rundenzeiten)}]";
                lines.Add(line);
            }

            File.WriteAllLines(filePath, lines);
        }

        private void RankRecords(List<CSVRecord> records)
        {
            // Global Ranking
            records = records
                .OrderByDescending(r => r.Rundenzeiten.Count)
                .ThenBy(r => r.Zeit.TotalSeconds)
                .ToList();

            for (int i = 0; i < records.Count; i++)
                records[i].Platz = i + 1;

            // Class-wise (PlatzAK)
            var ranked = records
                .GroupBy(r => r.Klasse)
                .SelectMany(group =>
                    group.OrderBy(r => r.Platz)
                         .Select((r, index) =>
                         {
                             r.PlatzAK = index + 1;
                             return r;
                         }))
                .ToList();

            records.Clear();
            records.AddRange(ranked);
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
    }
}
