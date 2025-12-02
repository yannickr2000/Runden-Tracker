public class CSVRecord
{
    public int Platz { get; set; }
    public int PlatzAK { get; set; }
    public string Startnummer { get; set; }
    public string Name { get; set; }
    public string Vorname { get; set; }
    public string Geschlecht { get; set; }
    public string Geburtsdatum { get; set; }
    public string Verein { get; set; }
    public string Strecke { get; set; }
    public string Klasse { get; set; }
    public string UciCode { get; set; }
    public TimeSpan Zeit { get; set; }
    public List<double> Rundenzeiten { get; set; }

    // "", "Ü40", "Ü50" – wird in RankRecords() gesetzt
    public string AltersgruppeHobby { get; set; }
}
