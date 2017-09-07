using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows.Data;
using RaceTimer.Business.ViewModel;
using RaceTimer.Common;
using RaceTimer.Data;
using RaceTimer.Device.IntegratedReaderR2000;

namespace RaceTimer.Business
{
    public class RfidManager
    {
        //lock object for synchronization;
        private static object _syncLock = new object();
        private IList<IDeviceAdapter> _deviceAdapters;
        private DateTime _raceTime;
        private DateTime _startTime;

        public ObservableCollection<AthleteSplit> AthleteSplits;

        public RfidManager()
        {
            AthleteSplits = new ObservableCollection<AthleteSplit>();
            //Enable the cross acces to this collection elsewhere
            BindingOperations.EnableCollectionSynchronization(AthleteSplits, _syncLock);
        }

        public void SetUp(DateTime raceTime)
        {
            _raceTime = raceTime;
            
            _deviceAdapters = new List<IDeviceAdapter>();

            _deviceAdapters.Add(new IntegratedReaderR2000Adapter());
            
        

            AthleteSplits.Add(new AthleteSplit
            {
                TagId = 1,
            });

            foreach (var device in _deviceAdapters)
            {
                device.OnRecordTag += onRecordTag;
                device.OpenConnection();
            }
            
        }

        public void Start()
        {
            _startTime = DateTime.Now;
            foreach (var device in _deviceAdapters)
            {
                device.BeginReading();
            }
        }

        public void Stop()
        {
            foreach (var device in _deviceAdapters)
            {
                device.StopReading();
            }
        }

        private void onRecordTag(object sender, EventArgs e)
        {
            var tag = (Tag) e;

            Task.Factory.StartNew(() =>
            {
                lock (_syncLock)
                {
                   
                    AthleteSplits.Add(new AthleteSplit
                    {

                        Epc= tag.TagId,
                        Time = tag.Time.ToUniversalTime(),
                        SplitTime = tag.Time.Subtract(_startTime.TimeOfDay).ToString("HH:mm:ss:ff"),
                        Rssi = tag.Rssi
                    });
                }
            });
        }
    }
}