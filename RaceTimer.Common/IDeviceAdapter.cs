using System;
using RaceTimer.Data;

namespace RaceTimer.Common
{
    public interface IDeviceAdapter
    {
        void Setup(ReaderProfile readerProfile);
        void BeginReading();
        bool CloseConnection();
        bool OpenConnection();
        void StopReading();
        event EventHandler<EventArgs> OnRecordTag;
    }
}