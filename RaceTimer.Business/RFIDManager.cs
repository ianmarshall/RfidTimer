using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using RaceTimer.Business.ViewModel;
using RaceTimer.Common;
using RaceTimer.Data;
using RaceTimer.Device.IntegratedReaderR2000;
using RfidTimer.Device.ChaFonFourChannelR2000;

namespace RaceTimer.Business
{
    public class RfidManager : INotifyPropertyChanged
    {
        //lock object for synchronization;

        private IEnumerable<ReaderProfile> _readerProfiles;
        private readonly object _syncLock = new object();
        private bool _connected;
        private bool _isReading;
        private int _nextBib;
        private readonly Dictionary<ReaderModel, IDeviceAdapter> _deviceStrategies =
            new Dictionary<ReaderModel, IDeviceAdapter>();
        private Race _race;

        private readonly SplitRepository _splitRepository = new SplitRepository();
        private readonly AthleteRepository _athleteRepository = new AthleteRepository();
        private ObservableCollection<Athlete> _athletes;

        public ObservableCollection<AthleteSplit> AthleteSplits;

        public RfidManager()
        {
            _deviceStrategies.Add(ReaderModel.ChaFonIntegratedR2000, new IntegratedReaderR2000Adapter());
            _deviceStrategies.Add(ReaderModel.ChaFonFourChannelR2000, new ChaFonFourChannelR2000Adapter());

            AthleteSplits = new ObservableCollection<AthleteSplit>();
            //Enable the cross acces to this collection elsewhere
            BindingOperations.EnableCollectionSynchronization(AthleteSplits, _syncLock);
        }

        public bool Connected
        {
            get { return _connected; }
            set
            {
                if (_connected != value)
                {
                    _connected = value;
                    OnPropertyChanged("Connected");
                }
            }
        }

        public bool IsReading
        {
            get { return _isReading; }
            set
            {
                if (_isReading != value)
                {
                    _isReading = value;
                    OnPropertyChanged("IsReading");
                }
            }
        }

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

        public void SetUp(IEnumerable<ReaderProfile> readerProfiles)
        {
            _readerProfiles = readerProfiles;

            foreach (var readerProfile in _readerProfiles)
            {
                var device = _deviceStrategies[readerProfile.Model];
                device.Setup(readerProfile);

                device.OnRecordTag += OnRecordTag;
                Connected = device.OpenConnection();
            }
        }

        public void ClearSplits()
        {
            AthleteSplits.Clear();
        }

        public void Start(Race race)
        {
            _race = race;

            foreach (var readerProfile in _readerProfiles)
            {
                IDeviceAdapter device = _deviceStrategies[readerProfile.Model];
                IsReading = device.BeginReading();
            }
        }

        public void StartAssigning(ObservableCollection<Athlete> athletes)
        {
            _athletes = athletes;
            foreach (var readerProfile in _readerProfiles)
            {
                IDeviceAdapter device = _deviceStrategies[readerProfile.Model];
                IsReading = device.BeginReading();
            }
        }

        public void Stop()
        {
            foreach (var readerProfile in _readerProfiles)
            {
                IDeviceAdapter device = _deviceStrategies[readerProfile.Model];
                IsReading = !device.StopReading();
            }
        }

        public bool CloseAll()
        {
            foreach (var readerProfile in _readerProfiles)
            {
                IDeviceAdapter device = _deviceStrategies[readerProfile.Model];
                Connected = !device.CloseConnection();
            }
            return !Connected;
        }

        public bool Test(ReaderProfile readerProfile)
        {
            IDeviceAdapter reader = _deviceStrategies[readerProfile.Model];
            reader.Setup(readerProfile);

            return reader.OpenConnection() && reader.CloseConnection();
        }

        private void OnRecordTag(object sender, EventArgs e)
        {
            Split split = (Split)e;

            Task.Factory.StartNew(() =>
            {
                lock (_syncLock)
                {
                    if (_readerProfiles.All(x => x.ReadingMode == ReadingMode.Desktop))
                    {
                        AssignTag(split);
                        return;
                    }

                    RecordSplit(split);
                }
            });
        }

        private void RecordSplit(Split split)
        {
            split.SplitTime = split.DateTimeOfDay.Subtract(_race.StartDateTime.TimeOfDay)
                .ToString("HH:mm:ss:ff");
            split.RaceId = _race.Id;

            AthleteSplits.Insert(0, new AthleteSplit
            {
                AtheleteName = "",
                Epc = split.Epc,
                Time = split.DateTimeOfDay,
                SplitTime = split.DateTimeOfDay.Subtract(_race.StartDateTime.TimeOfDay).ToString("HH:mm:ss:ff"),
                Rssi = split.Rssi,
                SplitName = split.SplitName,
            });
            _splitRepository.Add(split);
            _splitRepository.Save();
        }

        private void AssignTag(Split split)
        {
            Athlete nextAthlete;


            var all = _athletes;
                //_athleteRepository.GetAll();

            if (all.Any())
            {
                if (all.Any(x => x.TagId == split.Epc))
                {
                    //prevent duplicates
                    return;
                }

                int maxBib = all.Max(x => x.Bib);

                NextBib = ++maxBib;

                nextAthlete = all.SingleOrDefault(x => x.Bib == NextBib && string.IsNullOrEmpty(x.TagId));
                if (nextAthlete != null)
                {
                    nextAthlete.TagId = split.Epc;
                    _athleteRepository.Edit(nextAthlete, nextAthlete.Id);
                    _athleteRepository.Save();
                }
                else
                {
                    nextAthlete = all.FirstOrDefault(x => x.Bib == 0);
                    if (nextAthlete != null)
                    {
                        nextAthlete.Bib = NextBib;
                        nextAthlete.TagId = split.Epc;
                        _athleteRepository.Edit(nextAthlete, nextAthlete.Id);
                        _athleteRepository.Save();
                    }
                    else
                    {
                        nextAthlete = new Athlete
                        {
                            Bib = NextBib,
                            TagId = split.Epc
                        };
                        _athleteRepository.Add(nextAthlete);
                        _athleteRepository.Save();

                        Application.Current.Dispatcher.Invoke((Action) (() =>
                        {
                            _athletes.Insert(0, nextAthlete);
                        }));
                    }
                }
            }
            else
            {
                nextAthlete = new Athlete
                {
                    Bib = 1,
                    TagId = split.Epc
                };
                _athleteRepository.Add(nextAthlete);
                _athleteRepository.Save();
                Application.Current.Dispatcher.Invoke((Action)(() =>
                {
                    _athletes.Insert(0, nextAthlete);
                }));
            }
        

       

            return;
        }

        

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}