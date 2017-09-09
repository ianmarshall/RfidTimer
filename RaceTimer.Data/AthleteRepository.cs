using System;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace RaceTimer.Data
{
    public class AthleteRepository : BaseRepository<RaceTimerContext,Athlete>
    {
       
    }

    public class Athlete
    {
        public int Id { get; set; }
        public int Bib { get; set; }
        public string TagId { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        [NotMapped]
        public DateTime Dob { get; set; }
        public string Club { get; set; }
    }
}
