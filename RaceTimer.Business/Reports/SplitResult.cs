using RaceTimer.Data;
using System;
using System.Collections.Generic;


namespace RaceTimer.Business.Reports
{
    public class SplitResult
    {
        public int Bib { get; set; }
        public int LapNumber { get; set; }
        public int OveralPosition { get; set; }
        public int GendorPosition { get; set; }
        public int AgeCategoryPosition { get; set; }
        public string AthleteName { get; set; }
        public string AgeCategory { get; set; }
        public string SplitTime { get; set; }
        public string RaceTime { get; set; }
        public string TimeOfDay { get; set; }
        public string Epc { get; set; }
        public int Rssi { get; set; }
        public string SplitName { get; set; }
        public string RaceName { get; set; }
        public string AverLap { get; set; }
        public string EstFinish { get; set; }
    }
}