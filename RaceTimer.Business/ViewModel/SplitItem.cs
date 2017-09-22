using System;
using System.Collections.Generic;

namespace RaceTimer.Business.ViewModel
{
    public class AthleteSplit : IEquatable<AthleteSplit>
    {
        private int _timeOut = 5;

        public int Bib { get; set; }
        public string AtheleteName { get; set; }
        public int AthleteId { get; set; }
        public string SplitName { get; set; }
        public int SplitDeviceId { get; set; }
        public int SplitLapCount { get; set; }
        public string Epc { get; set; }
        public int TagId { get; set; }
        public DateTime Time { get; set; }
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

    public class AthleteSplitComparer : IEqualityComparer<AthleteSplit>
    {
        public bool Equals(AthleteSplit x, AthleteSplit y)
        {
            return x.TagId == y.TagId && IsDuplicateRead(x, y);
        }

        public int GetHashCode(AthleteSplit obj)
        {
            return obj.TagId.GetHashCode();
        }

        private bool IsDuplicateRead(AthleteSplit x, AthleteSplit y)
        {
            TimeSpan duration;

            if (x.Time > y.Time)
            {
                duration = x.Time - y.Time;
                return duration.Seconds < 5;
            }
            else if (x.Time < y.Time)
            {
                duration = y.Time - x.Time;
                return duration.Seconds < 5;
            }
            else
            {
                return true;
            }
        }
    }
}
