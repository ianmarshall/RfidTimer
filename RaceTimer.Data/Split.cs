using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace RaceTimer.Data
{
    public class Split : EventArgs
    {
        public int Id { get; set; }
        public string Epc { get; set; }
        public DateTime DateTimeOfDay { get; set; }
        public string TimeOfDay { get; set; }
        public string SplitTime { get; set; }
        public string Rssi { get; set; }
        public string SplitName { get; set; }
        public int RaceId { get; set; }
        public virtual Athlete Athlete { get; set; }
        [ForeignKey("RaceId")]
        public virtual Race Race { get; set; }
    }
}