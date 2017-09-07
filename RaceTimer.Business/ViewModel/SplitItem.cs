using System;

namespace RaceTimer.Business.ViewModel
{
    public class AthleteSplit
    {
        public string AtheleteName { get; set; }
        public int AthleteId { get; set;  }
        public string SplitName { get; set; }
        public string Epc { get; set; }
        public int TagId { get; set; }
        public DateTime Time { get; set; }
        public string SplitTime { get; set; }
        public string Rssi { get; set; }
    }
}
