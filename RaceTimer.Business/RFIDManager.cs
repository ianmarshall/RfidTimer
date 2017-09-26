using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows.Data;
using RaceTimer.Business.ViewModel;
using RaceTimer.Common;
using RaceTimer.Data;
using RaceTimer.Device.IntegratedReaderR2000;
using RfidTimer.Device.ChaFonFourChannelR2000;
using RaceTimer.Device.UhfReader18;
using NLog;

namespace RaceTimer.Business
{
    public class RfidManager : INotifyPropertyChanged
    {
        //lock object for synchronization;
        private static Logger logger = LogManager.GetCurrentClassLogger();
        private IEnumerable<ReaderProfile> _readerProfiles;
        private readonly object _syncLock = new object();
        private bool _connected;
        private bool _isReading;
        private readonly Dictionary<ReaderModel, IDeviceAdapter> _deviceStrategies =
            new Dictionary<ReaderModel, IDeviceAdapter>();
        public Race CurrentRace;

        private readonly SplitRepository _splitRepository = new SplitRepository();
        private readonly ReaderProfileRepository _readerProfileRepository = new ReaderProfileRepository();
        private readonly AthleteRepository _athleteRepository = new AthleteRepository();

        private readonly AthleteManager _athleteManager;

        private Dictionary<string, Athlete> _athleteDictionary;


        public ObservableCollection<AthleteSplit> AthleteSplits;

        public RfidManager(AthleteManager athleteManager)
        {
            _athleteManager = athleteManager;
            _deviceStrategies.Add(ReaderModel.ChaFonIntegratedR2000, new IntegratedReaderR2000Adapter());
            _deviceStrategies.Add(ReaderModel.ChaFonFourChannelR2000, new ChaFonFourChannelR2000Adapter());
            _deviceStrategies.Add(ReaderModel.UhfReader18Adapter, new UhfReader18Adapter());


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

        public void SetUp(IEnumerable<ReaderProfile> readerProfiles)
        {
            _readerProfiles = readerProfiles;

            foreach (var readerProfile in _readerProfiles)
            {
                var device = _deviceStrategies[readerProfile.Model];
                device.Setup(readerProfile);

                device.OnRecordTag += OnRecordTag;
                device.OnAssignTag += _athleteManager.OnAssignTag;
                Connected = device.OpenConnection();

                if (Connected)
                {
                    _readerProfileRepository.Edit(readerProfile, readerProfile.Id);
                    _readerProfileRepository.Save();
                }
            }
        }

        public bool EnableReader(ReaderProfile readerProfile)
        {
            var device = _deviceStrategies[readerProfile.Model];

            device.Setup(readerProfile);

            device.OnRecordTag += OnRecordTag;
            device.OnAssignTag += _athleteManager.OnAssignTag;
            return device.OpenConnection();
        }

        public void SetNewRace()
        {
            _athleteDictionary = _athleteRepository.GetAll().ToList().Distinct(new AthleteComparer()).ToDictionary(x => x.TagId, x => x);
            AthleteSplits.Clear();
        }

        public void Start(Race race)
        {
            CurrentRace = race;

            logger.Info("Started race {0} {1} at {2}", CurrentRace.Id, CurrentRace.Name, CurrentRace.StartDateTime);

            foreach (var readerProfile in _readerProfiles)
            {
                IDeviceAdapter device = _deviceStrategies[readerProfile.Model];
                IsReading = device.BeginReading();
            }
        }

        public void StartAssigning()
        {

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

            logger.Info("Finished race {0} {1} at {2}", CurrentRace.Id, CurrentRace.Name, CurrentRace.FinishDateTime);
        }

        public bool CloseAll()
        {
            if (_readerProfiles == null)
            {
                return false;
            }

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
                    RecordSplit(split);
                }
            });
        }

        private void RecordSplit(Split split)
        {
            DateTime prevSplit = GetpreviousSplit(split.Epc);

            split.RaceTime = split.DateTimeOfDay.Subtract(CurrentRace.StartDateTime.TimeOfDay).Ticks;
            split.RaceTime2 = split.DateTimeOfDay.Subtract(CurrentRace.StartDateTime.TimeOfDay).ToString("HH:mm:ss:ff");

            split.SplitTime = split.DateTimeOfDay.Subtract(prevSplit.TimeOfDay).TimeOfDay.TotalMilliseconds;

            split.RaceId = CurrentRace.Id;
            split.SplitDeviceId = split.SplitDeviceId;
            split.SplitName = split.SplitName;

            var splitCount = AthleteSplits.Count(x => x.Epc == split.Epc && x.SplitDeviceId == split.SplitDeviceId) + 1;
            split.SplitLapCount = splitCount;



            var atheleteSplit = new AthleteSplit
            {
                Epc = split.Epc,
                Time = split.DateTimeOfDay,
                RaceTime = TimeSpan.FromTicks(split.RaceTime).ToString("hh':'mm':'ss':'ff"),
                SplitTime = TimeSpan.FromMilliseconds(split.SplitTime).ToString("hh':'mm':'ss':'ff"),
                Rssi = split.Rssi,
                SplitName = split.SplitName,
                SplitDeviceId = split.SplitDeviceId,
                SplitLapCount = splitCount
            };


            if (_athleteDictionary != null && _athleteDictionary.ContainsKey(split.Epc))
            {
                Athlete athlete = _athleteDictionary[split.Epc];
                split.Athlete = athlete;
                split.AthleteId = athlete.Id;
                split.AthleteName = string.Format("{0} {1}", athlete.FirstName, athlete.LastName);
                split.Bib = athlete.Bib;
                atheleteSplit.AthleteId = athlete.Id;
                atheleteSplit.AtheleteName = split.AthleteName;
                atheleteSplit.Bib = athlete.Bib;
            }

            if (split.InventorySearchMode == InventorySearchMode.Session1SingleTarget && AthleteSplits.Contains(atheleteSplit))
            {
                logger.Info("Suppress tag split: {0}", split.ToString());
                return;
            }

            logger.Info("Read tag split: {0}", split.ToString());

            AthleteSplits.Insert(0, atheleteSplit);

            _splitRepository.AddSplit(split);
            _splitRepository.Save();
        }

        private DateTime GetpreviousSplit(string epc)
        {
            var split = AthleteSplits.OrderByDescending(x => x.Time).FirstOrDefault(x => x.Epc == epc);

            if (split != null)
            {

                return split.Time;
            }

            return CurrentRace.StartDateTime;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}