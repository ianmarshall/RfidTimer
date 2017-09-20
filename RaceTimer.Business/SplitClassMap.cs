using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CsvHelper.Configuration;
using RaceTimer.Data;

namespace RaceTimer.Business
{
   public class SplitClassMap : CsvClassMap<Split>
    {
        public SplitClassMap()
        {

            Map(m => m.Id).Name("Id");
            Map(m => m.AthleteName).Name("Athlete");
        }
    }
}
