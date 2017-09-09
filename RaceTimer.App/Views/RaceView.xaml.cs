using System;
using System.Windows;
using System.Windows.Controls;
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


        private readonly DispatcherTimer _timer;
        private TimeSpan _duration;

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


            dgSplits.ItemsSource = RfidManager.AthleteSplits;

        

        }

        private void TimerOnTick(object sender, object o)
        {
            _duration = _duration.Add(_timer.Interval);
         


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

            RfidManager.Start();
            _dateTime = DateTime.Now;
            _timer.Start();
            //  ttbTimer.IsStarted = true;
        }

        private void btnStopTimer_Click(object sender, RoutedEventArgs e)
        {
            // ttbTimer.IsStarted = false;
            RfidManager.Stop();
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



     
    }
}
