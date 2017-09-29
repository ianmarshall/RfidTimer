using RaceTimer.Data;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace RaceTimer.Common
{




   public class DeviceAdapterBase
    {
        public static ConcurrentDictionary<string, Split> RecentTags = new ConcurrentDictionary<string, Split>();
        //protected void StartReadDelay()
        //{
        //    if (_readerProfile.StartReadDelay == Data.StartReadDelay.None)
        //    {
        //        _aTimer.Enabled = true;
        //        return;
        //    }

        //    int delayMiliSeconds = (int)_readerProfile.StartReadDelay * 1000;

        //    Timer timer = new Timer();
        //    timer.Interval = delayMiliSeconds;
        //    timer.Tick += (s, e) =>
        //    {
        //        _aTimer.Enabled = true;
        //        timer.Stop();
        //    };
        //    timer.Start();
        //}
    }
}
