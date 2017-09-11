using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RaceTimer.Common;
using RaceTimer.Data;

namespace RfidTimer.Device.ChaFonFourChannelR2000
{
    public class ChaFonFourChannelR2000Adapter : IDeviceAdapter
    {
        private ReaderProfile _readerProfile;
        

        public void Setup(ReaderProfile readerProfile)
        {
            _readerProfile = readerProfile;
        }

        public bool BeginReading()
        {
            throw new NotImplementedException();
        }

        public bool CloseConnection()
        {
            return false;
        }

        public bool OpenConnection()
        {
            return false;
        }

        public bool StopReading()
        {
            throw new NotImplementedException();
        }

        public event EventHandler<EventArgs> OnRecordTag;
    }
}
