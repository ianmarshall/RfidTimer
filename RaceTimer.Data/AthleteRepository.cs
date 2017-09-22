using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;

namespace RaceTimer.Data
{
    public class AthleteRepository : BaseRepository<RaceTimerContext, Athlete>
    {
        public int GetMaxBib()
        {
            if (_entities.Athletes.Any())
            {
                return _entities.Athletes.Max(x => x.Bib);
            }
            return 0;
        }
    }

    public sealed class Athlete : INotifyPropertyChanged, IEquatable<Athlete>
    {
        private int _bib;
        private string _tagId;
        public Athlete()
        {
            Races = new List<Race>();
            Tags = new List<Split>();
        }

        public int Id { get; set; }
        
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
                    TagAssignDateTime = DateTime.Now;
                    OnPropertyChanged("TagId");
                }
            }
        }

        public DateTime ? TagAssignDateTime { get; set; }

        public string FirstName { get; set; }
        public string LastName { get; set; }
        public DateTime ? Dob { get; set; }
        public string Club { get; set; }

        public ICollection<Race> Races { get; set; }
        public ICollection<Split> Tags { get; set; }
        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public bool Equals(Athlete other)
        {
            return TagId == other.TagId;
        }
    }

    public class AthleteComparer : IEqualityComparer<Athlete>
    {
        public bool Equals(Athlete x, Athlete y)
        {
            return x.TagId == y.TagId;
        }

        public int GetHashCode(Athlete obj)
        {
            return obj.TagId.GetHashCode();
        }
    }
}
