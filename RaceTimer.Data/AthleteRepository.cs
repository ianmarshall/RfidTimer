using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations.Schema;
using System.Runtime.CompilerServices;
using System.Text;

namespace RaceTimer.Data
{
    public class AthleteRepository : BaseRepository<RaceTimerContext, Athlete>
    {

    }

    public class Athlete : INotifyPropertyChanged
    {
        private int _bib;
        private string _tagId;
        public Athlete()
        {
            Races = new List<Race>();
            Tags = new List<Split>();
        }

        public int Id { get; set; }
        // public int Bib { get; set; }
        public int Bib
        {
            get { return _bib; }
            set
            {
                if (_bib != value)
                {
                    _bib = value;
                    OnPropertyChanged("Bib");
                }
            }
        }

        public string TagId

        {
            get { return _tagId; }
            set
            {
                if (_tagId
                    != value)
                {
                    _tagId = value;
                    OnPropertyChanged("TagId");
                }
            }
        }

        public string FirstName { get; set; }
        public string LastName { get; set; }
        [NotMapped]
        public DateTime Dob { get; set; }
        public string Club { get; set; }

        public virtual ICollection<Race> Races { get; set; }
        public virtual ICollection<Split> Tags { get; set; }
        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        }
    }
}
