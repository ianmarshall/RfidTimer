using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;
using CsvHelper;
using RaceTimer.Data;
using RaceTimer.Business.Reports;
using System.Linq;
using System.Collections.Generic;
using System;
using NLog;

namespace RaceTimer.Business
{
    public class ReportManager : INotifyPropertyChanged
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();
        private RaceRepository _raceRepository;
        private SplitRepository _splitRepository;
        private AthleteRepository _athleteRepository;

        public ObservableCollection<Race> Races;
        public List<SplitResult> SplitResults;

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
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public List<SplitResult> LoadResults(Race race)
        {
            _currentRace = race;
            SplitResults = new List<SplitResult>();

            try
            {

                var athletes = _athleteRepository.GetAll(); //FindBy(x => x.Races.Any(y => y.Id == race.Id));
                var splits = _splitRepository.FindBy(x => x.RaceId == race.Id && x.AthleteId > 0);

                foreach (var athlete in athletes)
                {
                    foreach (var split in splits.Where(x => x.AthleteId == athlete.Id && x.RaceId == race.Id))
                    {
                        var splitPosition = splits.Where(x => x.SplitLapCount == split.SplitLapCount).OrderBy(x => x.DateTimeOfDay).ThenBy(x => x.RaceTime).ToList().FindIndex(x => x.AthleteId == athlete.Id) + 1;
                        var gendorPosition = splits.Where(x => x.SplitLapCount == split.SplitLapCount && x.Athlete.Gendor == athlete.Gendor).OrderBy(x => x.DateTimeOfDay).ThenBy(x => x.RaceTime).ToList().FindIndex(x => x.AthleteId == athlete.Id) + 1;
                        var ageCatPosition = splits.Where(x => x.SplitLapCount == split.SplitLapCount && x.Athlete.AgeCategory == athlete.AgeCategory).OrderBy(x => x.DateTimeOfDay).ThenBy(x => x.RaceTime).ToList().FindIndex(x => x.AthleteId == athlete.Id) + 1;

                        List<double> splitTimes = new List<double>();

                        foreach (var s in splits.Where(x => x.AthleteId == athlete.Id && x.RaceId == race.Id && x.SplitLapCount <= split.SplitLapCount))
                        {
                            splitTimes.Add(s.SplitTime);
                        }

                        double doubleAverageTicks = splitTimes.Any() ? splitTimes.Average(timeSpan => timeSpan) : 0;
                        TimeSpan avgLap = TimeSpan.FromMilliseconds(doubleAverageTicks);
                       

                        var splitResult = new SplitResult
                        {
                            OveralPosition = splitPosition,
                            GendorPosition = gendorPosition,
                            AgeCategoryPosition = ageCatPosition,
                            AthleteName = split.AthleteName,
                            AgeCategory = split.Athlete.AgeCategory,
                            Bib = split.Bib,
                            SplitTime = TimeSpan.FromMilliseconds(split.SplitTime).ToString("hh':'mm':'ss':'ff"),
                            TimeOfDay = split.TimeOfDay,
                            SplitName = split.SplitName,
                            RaceTime = TimeSpan.FromTicks(split.RaceTime).ToString("hh':'mm':'ss':'ff"),
                         //   RaceTime2 = split.RaceTime2,
                            LapNumber = split.SplitLapCount,
                            Epc = split.Epc,
                            RaceName = split.Race.Name,
                            Rssi = split.Rssi,
                            AverLap = avgLap.ToString("hh':'mm':'ss':'ff"),
                            EstFinish = TimeSpan.FromSeconds(avgLap.Seconds * 25).ToString("hh':'mm':'ss':'ff"),
                        };

                        SplitResults.Add(splitResult);
                    }
                }
                SplitResults = SplitResults.OrderByDescending(x => x.LapNumber).ThenBy(x => x.OveralPosition).ToList();

            }
            catch (Exception ex)
            {
                logger.Error(ex, "Error loading results" + ex.Message);
            }

            return SplitResults;
        }

        public void ExportResults()
        {
            //https://tech.io/playgrounds/5197/writing-csv-files-using-c

            try
            {

                using (StreamWriter sw = new StreamWriter(
                    $@"C:\Temp\results_{_currentRace.Name}_{_currentRace.StartDateTime:yyyy-dd-M--HH-mm-ss}.csv"))
                {
                    using (var csvWriter = new CsvWriter(sw))
                    {
                        csvWriter.Configuration.HasHeaderRecord = true;
                        //csvWriter.Configuration.AutoMap<SplitClassMap>();
                        csvWriter.WriteHeader<SplitResult>();
                        // csvWriter.WriteField("Race Start" + _currentRace.StartDateTime);
                        //foreach (var item in Splits)
                        //{

                        //    csvWriter.WriteField(item.AthleteName);
                        //    csvWriter.WriteField(item.DateTimeOfDay);
                        //    csvWriter.WriteField(item.Epc);
                        //    csvWriter.NextRecord();
                        //}

                        csvWriter.WriteRecords(SplitResults);
                        sw.Flush();
                    }
                }
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Error exporting csv " + ex.Message);
            }
        }
    }
}