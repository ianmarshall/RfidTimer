using System;
using System.Collections.Generic;

namespace RaceTimer.Business.ViewModel
{
    public class AthleteSplit : IEquatable<AthleteSplit>
    {
        private int _timeOut = 30;

        public int Bib { get; set; }
        public string AtheleteName { get; set; }
        public int AthleteId { get; set; }
        public string SplitName { get; set; }
        public int SplitDeviceId { get; set; }
        public int SplitLapCount { get; set; }
        public string Epc { get; set; }
        public DateTime Time { get; set; }
        public string RaceTime { get; set; }
        public string SplitTime { get; set; }
        public string Rssi { get; set; }

        public bool Equals(AthleteSplit other)
        {
            return other.Epc == Epc && IsDuplicateRead(other);
        }

        private bool IsDuplicateRead(AthleteSplit other)
        {
            TimeSpan duration;

            if (this.Time > other.Time)
            {
                duration = Time - other.Time;
                return duration.Seconds < _timeOut;
            }
            else if (Time < other.Time)
            {
                duration = other.Time - Time;
                return duration.Seconds < _timeOut;
            }
            else
            {
                return true;
            }
        }
    }
}
