using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RaceTimer.Data
{
    public class SettingsRepository : BaseRepository<Settings>
    {

    }

    public class Settings
    {
        public int Id { get; set; }

        public bool TestMode { get; set; }

        public StartReadDelay StartReadDelay { get; set; }

        public int ReadSuppressionTime { get; set;}

        public int MaxReadUpdateTime { get; set; }

        public int MinNewReadTime { get; set; }

        public IEnumerable<int> Seconds { get; } = Enumerable.Range(1, 360);

        public virtual ICollection<ReaderProfile> ReaderProfiles { get; set; }
    }
}
