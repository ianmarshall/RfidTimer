using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Data;
using RaceTimer.Business.ViewModel;
using RaceTimer.Common;
using RaceTimer.Data;
using RaceTimer.Device.IntegratedReaderR2000;
using RfidTimer.Device.ChaFonFourChannelR2000;

namespace RaceTimer.Business
{
    public static class RfidManager
    {
        //lock object for synchronization;
        private static IEnumerable<ReaderProfile> _readerProfiles;
        private static object _syncLock = new object();
        private static IList<IDeviceAdapter> _deviceAdapters;
        private static DateTime _raceTime;
        private static DateTime _startTime;
        private static Dictionary<ReaderModel, IDeviceAdapter> _deviceStrategies =
            new Dictionary<ReaderModel, IDeviceAdapter>();

     
        public static ObservableCollection<AthleteSplit> AthleteSplits;
        
        static RfidManager()
        {
            _deviceStrategies.Add(ReaderModel.ChaFonIntegratedR2000, new IntegratedReaderR2000Adapter());
            _deviceStrategies.Add(ReaderModel.ChaFonFourChannelR2000, new ChaFonFourChannelR2000Adapter());
           

            AthleteSplits = new ObservableCollection<AthleteSplit>();
            //Enable the cross acces to this collection elsewhere
            BindingOperations.EnableCollectionSynchronization(AthleteSplits, _syncLock);
        }

        public static void SetUp(IEnumerable<ReaderProfile> readerProfiles)
        {
            _readerProfiles = readerProfiles;
            //_deviceStrategies.Add(ReaderModel.ChaFonIntegratedR2000, new IntegratedReaderR2000Adapter());
            //_deviceStrategies.Add(ReaderModel.ChaFonFourChannelR2000, new ChaFonFourChannelR2000Adapter());


            //AthleteSplits = new ObservableCollection<AthleteSplit>();
            ////Enable the cross acces to this collection elsewhere
            //BindingOperations.EnableCollectionSynchronization(AthleteSplits, _syncLock);
         //   _raceTime = raceTime;

            foreach (var readerProfile in _readerProfiles)
            {
                var device = _deviceStrategies[readerProfile.Model];
                device.Setup(readerProfile);

                device.OnRecordTag += onRecordTag;
                device.OpenConnection();
            }

            AthleteSplits.Add(new AthleteSplit
            {
                TagId = 1,
            });

         

        }

        public static void Start()
        {
            _startTime = DateTime.Now;
            foreach (var readerProfile in _readerProfiles)
            {
                IDeviceAdapter device = _deviceStrategies[readerProfile.Model];
                device.BeginReading();
            }
        }

        public static void Stop()
        {
            foreach (var readerProfile in _readerProfiles)
            {
                IDeviceAdapter device = _deviceStrategies[readerProfile.Model];
                device.StopReading();
            }
        }

        public static bool Test(ReaderProfile readerProfile)
        {
            IDeviceAdapter reader = _deviceStrategies[readerProfile.Model];
            reader.Setup(readerProfile);

            return reader.OpenConnection() && reader.CloseConnection();
        }

        private static void onRecordTag(object sender, EventArgs e)
        {
            var tag = (Tag) e;

            Task.Factory.StartNew(() =>
            {
                lock (_syncLock)
                {
                   
                    AthleteSplits.Insert(0, new AthleteSplit
                    {

                        Epc= tag.TagId,
                        Time = tag.Time.ToUniversalTime(),
                        SplitTime = tag.Time.Subtract(_startTime.TimeOfDay).ToString("HH:mm:ss:ff"),
                        Rssi = tag.Rssi,
                        SplitName = tag.SplitName
                    });
                }
            });
        }
    }
}