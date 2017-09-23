﻿using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;
using CsvHelper;
using RaceTimer.Data;
using RaceTimer.Business.Reports;
using System.Linq;
using System.Collections.Generic;

namespace RaceTimer.Business
{
    public class ReportManager : INotifyPropertyChanged
    {
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
            SplitResults = new List<SplitResult>();
            _currentRace = race;

            var athletes = _athleteRepository.GetAll(); //FindBy(x => x.Races.Any(y => y.Id == race.Id));
            var splits = _splitRepository.FindBy(x => x.RaceId == race.Id && x.AthleteId > 0);

            foreach (var athlete in athletes)
            {
                foreach (var split in splits.Where(x => x.AthleteId == athlete.Id))
                {
                    var splitposition = splits.Where(x => x.SplitLapCount == split.SplitLapCount).OrderBy(x=>x.DateTimeOfDay).ToList().FindIndex(x => x.AthleteId == athlete.Id) + 1;

                    var splitResult = new SplitResult
                    {
                        OveralPosition = splitposition,
                        AthleteName = split.AthleteName,
                        Bib = split.Bib,
                        SplitTime = split.SplitTime,
                        TimeOfDay = split.TimeOfDay,
                        SplitName = split.SplitName,
                        RaceTime = split.RaceTime,
                        LapNumber = split.SplitLapCount,
                        Epc = split.Epc,
                        RaceName = split.Race.Name,
                        Rssi = split.Rssi
                    };

                    SplitResults.Add(splitResult);
                }
            }
            SplitResults = SplitResults.OrderByDescending(x => x.LapNumber).ThenBy(x=>x.OveralPosition).ToList();

            return SplitResults;
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
    }
}