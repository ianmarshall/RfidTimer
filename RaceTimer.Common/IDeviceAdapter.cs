using System;
using RaceTimer.Data;

namespace RaceTimer.Common
{
    public interface IDeviceAdapter
    {
        void Setup(ReaderProfile readerProfile);
        bool BeginReading();
        bool CloseConnection();
        bool OpenConnection();
        bool StopReading();
        event EventHandler<EventArgs> OnRecordTag;
    }
}