using RaceTimer.Data;
using System.Data.Entity;

namespace RaceTimer.Data
{
    public class SplitRepository : BaseRepository<Split>
    {
        public void AddSplit(Split split)
        {
            if(split.Athlete != null && split.Athlete.Id > 0)
            {
                Context.Athletes.Attach(split.Athlete);
            }

            Add(split);
        }
    }
}
