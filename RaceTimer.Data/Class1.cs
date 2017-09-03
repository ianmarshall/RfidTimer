using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RaceTimer.Data
{
    public class Class1
    {
        public void add()
        {
            using (var db = new RaceTimerContext())
            {
               
                db.ReaderProfiles.Add(new ReaderProfile { Name = "TEST1" });
                db.SaveChanges();

              
                  
            }
        }
    }
}
