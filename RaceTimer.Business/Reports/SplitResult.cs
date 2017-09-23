using System;
using System.Collections.Generic;


namespace RaceTimer.Business.Reports
{
    public class SplitResult
    {
        public int LapNumber { get; set; }
        public int OveralPosition { get; set; }
        public string AthleteName { get; set; }
        public int Bib { get; set; }
        public string SplitTime { get; set; }
        public string RaceTime { get; set; }
        public string TimeOfDay { get; set; }
        public string Epc { get; set; }
        public string Rssi { get; set; }
        public string SplitName { get; set; }
        public string RaceName { get; set; }
    }
}