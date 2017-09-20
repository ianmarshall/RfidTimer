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

namespace RaceTimer.Business
{
    public class RfidManager : INotifyPropertyChanged
    {
        //lock object for synchronization;

        private IEnumerable<ReaderProfile> _readerProfiles;
        private readonly object _syncLock = new object();
        private bool _connected;
        private bool _isReading;
        private readonly Dictionary<ReaderModel, IDeviceAdapter> _deviceStrategies =
            new Dictionary<ReaderModel, IDeviceAdapter>();
        private Race _race;

        private readonly SplitRepository _splitRepository = new SplitRepository();
        private readonly AthleteRepository _athleteRepository = new AthleteRepository();
     
        private readonly AthleteManager _athleteManager;
     

        public ObservableCollection<AthleteSplit> AthleteSplits;

        public RfidManager(AthleteManager athleteManager)
        {
            _athleteManager = athleteManager;
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
            split.SplitTime = split.DateTimeOfDay.Subtract(_race.StartDateTime.TimeOfDay)
                .ToString("HH:mm:ss:ff");
            split.RaceId = _race.Id;
            split.SplitDeviceId = split.SplitDeviceId;
            split.SplitName = split.SplitName;
            string atheleteName = String.Empty;

            Athlete athelete = _athleteManager.Athletes.FirstOrDefault(x => x.TagId == split.Epc);
            if (athelete != null)
            {
                atheleteName = string.IsNullOrEmpty(athelete.FirstName)  ? ""  : athelete.FirstName;

                //atheleteName  = atheleteName + string.IsNullOrEmpty(athelete.LastName) ? "" : athelete.LastName) : "";
            }
            else
            {
                athelete = new Athlete
                {
                    TagId = split.Epc,
                    TagAssignDateTime = DateTime.Now
                };
                _athleteRepository.Add(athelete);
           //     _athleteRepository.Save();
            }

            split.Athlete = athelete;
            split.AthleteId = athelete.Id;
            split.AthleteName = atheleteName;
            var splitCount = AthleteSplits.Count(x => x.Epc == split.Epc && x.SplitDeviceId == split.SplitDeviceId) + 1;
            split.SplitLapCount = splitCount;

            AthleteSplits.Insert(0, new AthleteSplit
            {
                AtheleteName = atheleteName,
                Epc = split.Epc,
                Time = split.DateTimeOfDay,
                SplitTime = split.DateTimeOfDay.Subtract(_race.StartDateTime.TimeOfDay).ToString("HH:mm:ss:ff"),
                Rssi = split.Rssi,
                SplitName = split.SplitName,
                SplitDeviceId = split.SplitDeviceId,
                SplitLapCount = splitCount,
                Bib = athelete.Bib
            });
            _splitRepository.Add(split);
            _splitRepository.Save();
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}