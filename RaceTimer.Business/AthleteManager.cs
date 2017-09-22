using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using RaceTimer.Business.ViewModel;
using RaceTimer.Data;

namespace RaceTimer.Business
{
    public class AthleteManager : INotifyPropertyChanged
    {
        private readonly AthleteRepository _athleteRepository = new AthleteRepository();

        private int _nextBib;
        private string _message;

        public AthleteManager()
        {
            Athletes = new ObservableCollection<Athlete>(_athleteRepository.GetAll());
            _nextBib = 1;

        }

        public ObservableCollection<Athlete> Athletes;

        public ObservableCollection<AthleteSplit> AthleteSplits;

        public bool AutoAssign;

        public int NextBib
        {
            get { return _nextBib; }
            set
            {
                if (_nextBib != value)
                {
                    _nextBib = value;
                    OnPropertyChanged("NextBib");
                }
            }
        }

        public string Message
        {
            get { return _message; }
            set
            {
                if (_message != value)
                {
                   _message = value;
                    OnPropertyChanged("Message");
                }
            }
        }

        public void OnAssignTag(object sender, EventArgs e)
        {
            Split split = (Split)e;
            AssignTag(split);
        }


        private void AssignTag(Split split)
        {
            Athlete nextAthlete;
         
            //_athleteRepository.GetAll();

            if (Athletes.Any())
            {
                if (Athletes.Any(x => x.TagId == split.Epc && x.Bib > 0))
                {
                    var athelete = Athletes.FirstOrDefault(x => x.TagId == split.Epc);
                    Message =
                        $"Tag {split.Epc} already assigned to {athelete.Bib} {athelete.FirstName} {athelete.LastName}";
                    return;
                }


                if (AutoAssign)
                {
                    int maxBib = _athleteRepository.GetMaxBib();

                    NextBib = ++maxBib;
                }

                nextAthlete = Athletes.SingleOrDefault(x => x.Bib == NextBib && string.IsNullOrEmpty(x.TagId));
                if (nextAthlete != null)
                {
                    nextAthlete.TagId = split.Epc;
                    _athleteRepository.Edit(nextAthlete, nextAthlete.Id);
                    _athleteRepository.Save();
                }
                else
                {
                    nextAthlete = Athletes.FirstOrDefault(x => x.Bib == 0);
                    if (nextAthlete != null)
                    {
                        nextAthlete.Bib = NextBib;
                        nextAthlete.TagId = split.Epc;
                        _athleteRepository.Edit(nextAthlete, nextAthlete.Id);
                        _athleteRepository.Save();
                        Message =
                       $"Tag {split.Epc} assigned to { nextAthlete.Bib}";
                    }
                    else if (Athletes.Any(x=>x.Bib == NextBib) == false)
                    {
                        nextAthlete = new Athlete
                        {
                            Bib = NextBib,
                            TagId = split.Epc
                        };
                        _athleteRepository.Add(nextAthlete);
                        _athleteRepository.Save();

                        Application.Current.Dispatcher.Invoke((Action)(() =>
                        {
                            Athletes.Insert(0, nextAthlete);
                        }));
                        Message =
                       $"Tag {split.Epc} assigned to { nextAthlete.Bib}";
                    }
                   
                    return;
                }
            }
            else
            {
                if (Athletes.Any(x => x.Bib == NextBib))
                {
                    var athelete = Athletes.FirstOrDefault(x => x.Bib == NextBib);
                    Message =
                        $"Tag {split.Epc} already assigned to {athelete.Bib} {athelete.FirstName} {athelete.LastName}";
                    return;
                }

                nextAthlete = new Athlete
                {
                    Bib = NextBib,
                    TagId = split.Epc
                };
                _athleteRepository.Add(nextAthlete);
                _athleteRepository.Save();
                Application.Current.Dispatcher.Invoke((Action)(() =>
                {
                    Athletes.Insert(0, nextAthlete);
                }));

                Message =
                    $"Tag {split.Epc} assigned to {nextAthlete.Bib} with no athlete name";
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
