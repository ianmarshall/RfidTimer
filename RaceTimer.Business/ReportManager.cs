using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using CsvHelper;
using RaceTimer.Business.ViewModel;
using RaceTimer.Data;

namespace RaceTimer.Business
{
    public class ReportManager : INotifyPropertyChanged
    {
        private RaceRepository _raceRepository;
        private SplitRepository _splitRepository;
        private AthleteRepository _athleteRepository;

        public ObservableCollection<Race> Races;
        public ObservableCollection<Split> Splits;
        private Race _currentRace;


        public ReportManager()
        {
            _raceRepository = new RaceRepository();
            _splitRepository = new SplitRepository();
            _athleteRepository = new AthleteRepository();
            Races = new ObservableCollection<Race>(_raceRepository.GetAll());

        }


        public void LoadSplits(Race race)
        {
            _currentRace = race;

            Splits = new ObservableCollection<Split>(_splitRepository.FindBy(x => x.RaceId == race.Id));
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }


        public void LoadResults(Race race)
        {
            IQueryable<IGrouping<int, Split>> athleteSplits =
                _splitRepository.FindBy(x => x.RaceId == race.Id).GroupBy(x => x.AthleteId);

            foreach (IGrouping<int, Split> athleteSplit in athleteSplits)
            {
                var group = athleteSplit.ToList().OrderBy(x => x.SplitLapCount);

                //var result = new Result();
                //result.Bib = athleteSplit.FirstOrDefault().Athlete.FirstName;


                //foreach (var split in group)
                //{
                    
                //}
            }

        }

        public void ExportResults()
        {
            //https://tech.io/playgrounds/5197/writing-csv-files-using-c

            using (StreamWriter sw = new StreamWriter(
                $@"C:\Temp\results_{_currentRace.Name}_{_currentRace.StartDateTime:yyyy-dd-M--HH-mm-ss}.csv"))
            {
                using (var csvWriter = new CsvWriter(sw))
                {
                    csvWriter.Configuration.HasHeaderRecord = true;
                    csvWriter.Configuration.AutoMap<SplitClassMap>();
                    
                    csvWriter.WriteRecords(Splits);
                    sw.Flush();
                }
            }
        }
    }
}