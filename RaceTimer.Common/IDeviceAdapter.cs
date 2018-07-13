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
        bool UpdateSettings();
        event EventHandler<EventArgs> OnRecordTag;
        event EventHandler<EventArgs> OnAssignTag;
        event EventHandler<EventArgs> OnReportTags;
    }
}