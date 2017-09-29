using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;

namespace RaceTimer.Data
{
    public class RaceRepository : BaseRepository<Race>
    {
        
    }

    public class Race : IDataErrorInfo
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

        string IDataErrorInfo.Error
        {
            get
            {
                return null; 
            }
        }

        string IDataErrorInfo.this[string columnName]
        {
            get
            {
                switch (columnName)
                {
                    case "Name":
                        if (string.IsNullOrEmpty(this.Name))
                            return "Race name must be given";
                        break;
                }

                return string.Empty;
            }
        }
    }
}
