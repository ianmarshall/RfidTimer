using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;

namespace RaceTimer.Data
{
    public class RaceRepository : BaseRepository<RaceTimerContext,Race>
    {
        
    }

    public class Race
    {
        public Race()
        {
            Athletes = new List<Athlete>();
        }

        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        public string Name { get; set; }
        public DateTime StartDateTime { get; set; }
        public DateTime ? FinishDateTime { get; set; }
        public string StartTime { get; set; }
        public virtual ICollection<Athlete> Athletes { get; set; }
    }
}
