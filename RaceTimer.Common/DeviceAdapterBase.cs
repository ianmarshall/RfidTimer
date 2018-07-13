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
      //  public static ConcurrentDictionary<string, List<Split>> RecentTags = new ConcurrentDictionary<string, List<Split>>();

        public static ConcurrentDictionary<string, Split> RecentTags = new ConcurrentDictionary<string, Split>();

        public static ConcurrentDictionary<string, List<Split>> TagsInView = new ConcurrentDictionary<string, List<Split>>();

        protected static readonly Object RecentTagsLock = new object();

        protected static readonly Object NewTagsLock = new object();


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
