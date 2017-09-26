using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using RaceTimer.Business.ViewModel;
using RaceTimer.Data;
using System.IO;
using CsvHelper;
using RaceTimer.Business.Reports;
using NLog;
using System.Collections.Generic;

namespace RaceTimer.Business
{
    public class AthleteManager : INotifyPropertyChanged
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();
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
                    else if (Athletes.Any(x => x.Bib == NextBib) == false)
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

                Application.Current.Dispatcher.Invoke((Action)(() =>
                {

                    Athletes.Insert(0, nextAthlete);
                }));

                _athleteRepository.Add(nextAthlete);
                _athleteRepository.Save();


                Message =
                    $"Tag {split.Epc} assigned to {nextAthlete.Bib} with no athlete name";
            }
        }

        public void ExportAthletes()
        {

            try
            {
                var athleteList = new List<AthleteCsv>();

                foreach (var ath in Athletes)
                {
                    athleteList.Add(
                        new AthleteCsv
                        {
                            Bib = ath.Bib,
                            FirstName = ath.FirstName,
                            LastName = ath.LastName,
                            Gendor = ath.Gendor,
                            AgeCategory = ath.AgeCategory,
                            TagId = ath.TagId
                        }
                        );
                }


                using (StreamWriter sw = new StreamWriter(
                    $@"C:\Temp\athletes_{DateTime.Now:yyyy-dd-M--HH-mm-ss}.csv"))
                {
                    using (var csvWriter = new CsvWriter(sw))
                    {
                        csvWriter.Configuration.HasHeaderRecord = true;
                        //csvWriter.Configuration.AutoMap<SplitClassMap>();
                        csvWriter.WriteHeader<AthleteCsv>();

                        csvWriter.WriteRecords(athleteList);
                        sw.Flush();
                    }
                }
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Error exporting athletes csv " + ex.Message);
            }
        }

        public void ImportAthletes(string filePath)
        {
            try
            {
                using (TextReader fileReader = File.OpenText(filePath))
                {
                    var csv = new CsvReader(fileReader);
                    //csv.Configuration.HasHeaderRecord = false;

                    var records = csv.GetRecords<AthleteCsv>().ToList();

                    foreach (var ath in records)
                    {
                        var athlete = new Athlete
                        {
                            Bib = ath.Bib,
                            FirstName = ath.FirstName,
                            LastName = ath.LastName,
                            Gendor = ath.Gendor,
                            AgeCategory = ath.AgeCategory,
                            TagId = ath.TagId
                        };

                        _athleteRepository.Add(athlete);
                        _athleteRepository.Save();
                        Athletes.Add(athlete);
                    }
                }
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Error importing athletes csv " + ex.Message);
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
