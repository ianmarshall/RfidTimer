using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RaceTimer.Data
{
   public class TagRepo
    {

        public void Add(Tag tag)
        {
            using (var db = new RaceTimerContext())
            {

                db.Tags.Add(tag);
                db.SaveChanges();
            }
        }
    }
}
