using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace RaceTimer.Data
{
    public class Split : EventArgs
    {
        public int Id { get; set; }
        public string AthleteName { get; set; }
        public int Bib { get; set; }
        public string Epc { get; set; }
        public DateTime DateTimeOfDay { get; set; }
        public string TimeOfDay { get; set; }
        public double SplitTime { get; set; }
        public long RaceTime { get; set; }
        public string RaceTime2 { get; set;}
        public int Rssi { get; set; }
        public string Antenna { get; set; }
        public string SplitName { get; set; }
        public int SplitDeviceId { get; set; }
        public InventorySearchMode InventorySearchMode { get; set; }
        public int RaceId { get; set; }
        public int SplitLapCount { get; set; }

        public int? AthleteId { get; set; }
        public virtual Athlete Athlete { get; set; }
        [ForeignKey("RaceId")]
        public virtual Race Race { get; set; }


        public override string ToString()
        {
            return string.Format("Epc: {0}, DateTime: {1}, SplitTime: {2}, RaceTime: {3}, SplitName: {4}, RaceId: {5}, AthleteId: {6} Bib: {7}", Epc, DateTimeOfDay, SplitTime, RaceTime2, SplitName, RaceId, AthleteId, Bib);
        }
    }


    public class SplitComparer : IEqualityComparer<Split>
    {
        public bool Equals(Split x, Split y)
        {
            return x.RaceId == y.RaceId && x.Epc == y.Epc && IsDuplicateRead(x, y);
        }

        public int GetHashCode(Split obj)
        {
            return obj.Epc.GetHashCode();
        }

        private bool IsDuplicateRead(Split x, Split y)
        {
            TimeSpan duration;

            if (x.DateTimeOfDay > y.DateTimeOfDay)
            {

                duration = x.DateTimeOfDay - y.DateTimeOfDay;
                return duration.Seconds < 30;
            }
            else if (x.DateTimeOfDay < y.DateTimeOfDay)
            {
                 duration = y.DateTimeOfDay - x.DateTimeOfDay;
                return duration.Seconds < 30;
            }
            else
            {
                return true;
            }
        }

       
    }

   
}