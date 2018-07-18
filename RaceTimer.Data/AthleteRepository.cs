using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
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
            if (Context.Athletes.Any())
            {
                return Context.Athletes.Max(x => x.Bib);
            }
            return 0;
        }


        public void DeleteAthlete(Athlete athlete)
        {
            Context.Athletes.Attach(athlete);
            Context.Athletes.Remove(athlete);
        }

    }

    public sealed class Athlete : INotifyPropertyChanged, IEquatable<Athlete>
    {
        private int _bib;
        private string _tagId;
        public Athlete()
        {
            Races = new List<Race>();
            Splits = new List<Split>();
        }

        [Key]
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


        private string _firstName { get; set; }
        public string FirstName
        {
            get { return _firstName; }
            set
            {
                if (_firstName
                    != value)
                {
                    _firstName = value;
                    OnPropertyChanged("FirstName");
                }
            }
        }


        private string _lastName { get; set; }
        public string LastName
        {
            get { return _lastName; }
            set
            {
                if (_lastName
                    != value)
                {
                    _lastName = value;
                    OnPropertyChanged("LastName");
                }
            }
        }

        public DateTime ? Dob { get; set; }

        private Gendor _gendor { get; set; }
        public Gendor Gendor
        {
            get { return _gendor; }
            set
            {
                if (_gendor
                    != value)
                {
                    _gendor = value;
                    OnPropertyChanged("Gendor");
                }
            }
        }

        [NotMapped]
        public IList<Gendor> Gendors { get; } = Enum.GetValues(typeof(Gendor)).Cast<Gendor>().ToList();


        private string _ageCategory { get; set; }
        public string AgeCategory
        {
            get { return _ageCategory; }
            set
            {
                if (_ageCategory
                    != value)
                {
                    _ageCategory = value;
                    OnPropertyChanged("AgeCategory");
                }
            }
        }

        [NotMapped]
        public IList<AgeCategory> AgeCategories { get; } = Enum.GetValues(typeof(AgeCategory)).Cast<AgeCategory>().ToList();


        public string Club { get; set; }

        public ICollection<Race> Races { get; set; }
        public ICollection<Split> Splits { get; set; }
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


    public enum Gendor
    {
        M,
        F
    }

    public enum AgeCategory
    {
        SM,
        SF,
        VM,
        VF
    }
}
