using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;
using RaceTimer.Business;

namespace RaceTimer.App.Views
{
    /// <summary>
    /// Interaction logic for RaceView.xaml
    /// </summary>
    public partial class RaceView : UserControl
    {
        //   private static Timer _updateTimer = new Timer(UpdateTimer, null, 100, 100);
        private static event EventHandler OnTick;


        private DispatcherTimer _timer;
        private TimeSpan Duration;
        private RfidManager _rfidManager;
        private DateTime _dateTime;
        private DateTime _raceTime;

        public RaceView()
        {
            InitializeComponent();
            //  ttbTimer.IsStarted = false;
            _dateTime = DateTime.Now;
            _timer = new DispatcherTimer(new TimeSpan(0, 0, 0, 0, 1), DispatcherPriority.Normal, TimerOnTick, this.Dispatcher);
            _timer.Stop();
            _raceTime = DateTime.Now.Add(-_dateTime.TimeOfDay);

            this.Show.Text = _raceTime.ToString("HH:mm:ss:ff");

            _rfidManager = new RfidManager();
          

            _rfidManager.SetUp(_raceTime);

            dgSplits.ItemsSource = _rfidManager.AthleteSplits;

        }

        private void TimerOnTick(object sender, object o)
        {
            Duration = Duration.Add(_timer.Interval);
          //  dgSplits.ItemsSource = _rfidManager.AthleteSplits;
           

            _raceTime = DateTime.Now.Add(-_dateTime.TimeOfDay);

            this.Show.Text = _raceTime.ToString("HH:mm:ss:ff"); //+ " - " + Convert.ToDateTime(Duration.ToString()).ToString("HH:mm:ss:fff");// DateTime.Now.ToString("HH:mm:ss:ff") + _dateTime;
            // this.Show.Text = String.Format("{0:D2}:{1:D2}", Duration.Seconds, Duration.Milliseconds);

            // Duration.DataContext = Duration;
        }


        private void btnSetTimer_Click(object sender, RoutedEventArgs e)
        {
            //  ttbTimer.TimeSpan = TimeSpan.FromMinutes(5);

            dgSplits.Items.Refresh();
        }

        private void btnStartTimer_Click(object sender, RoutedEventArgs e)
        {
            _rfidManager.Start();
            _dateTime = DateTime.Now;
            _timer.Start();
            //  ttbTimer.IsStarted = true;
        }

        private void btnStopTimer_Click(object sender, RoutedEventArgs e)
        {
            // ttbTimer.IsStarted = false;
            _rfidManager.Stop();
            _timer.Stop();

        }

        private void btnResetTimer_Click(object sender, RoutedEventArgs e)
        {
            _dateTime = DateTime.Now;


            //_timer.Stop();
            //_timer.Interval = new TimeSpan(0, 0, 0, 0, 1);
            //_timer.Start();



            //  Duration = new TimeSpan();
            //   ttbTimer.Reset();
        }



        //private static void UpdateTimer(object state)
        //{
        //    OnTick?.Invoke(null, EventArgs.Empty);
        //}





        //void dt_Tick(object sender, EventArgs e)
        //{
        //    _rfidManager.SetUp();

        //    if (sw.IsRunning)
        //    {
        //        TimeSpan ts = sw.Elapsed;
        //        _currentTime = String.Format("{0:00}:{1:00}:{2:00}",
        //            ts.Minutes, ts.Seconds, ts.Milliseconds / 10);
        //        clocktxtblock.Text = _currentTime;
        //    }
        //}


        //private void StartBtn_Click(object sender, RoutedEventArgs e)
        //{
        //    sw.Start();
        //    dt.Start();
        //    _rfidManager.Start();
        //}

    }
}
