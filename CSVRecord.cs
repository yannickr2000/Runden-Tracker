using System;
using System.Collections.Generic;

namespace Rundenzeiten
{
    public class CSVRecord
    {
        public int Platz  { get; set; }
        public int PlatzAK { get; set; }
        public string Startnummer { get; set; }
        public string Name { get; set; }
        public string Vorname { get; set; }
        public string Geschlecht { get; set; }
        public string Geburtsdatum { get; set; }

        public string Verein { get; set; }
        public string Strecke { get; set; }
        public string Klasse { get; set; }
        public TimeSpan Zeit { get; set; }
        public List<double> Rundenzeiten { get; set; }
        //public int PlatzAKHobby { get; set; }          // 2. AK-Platz für Hobby (geschlechtsunabhängig)
        public string AltersgruppeHobby { get; set; }  // "Normal", "Ü40", "Ü50"

    }
}
