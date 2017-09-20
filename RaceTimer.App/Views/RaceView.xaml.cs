using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using RaceTimer.Business;
using RaceTimer.Data;

namespace RaceTimer.App.Views
{
    /// <summary>
    /// Interaction logic for RaceView.xaml
    /// </summary>
    public partial class RaceView : UserControl
    {
        //   private static Timer _updateTimer = new Timer(UpdateTimer, null, 100, 100);
        // private static event EventHandler OnTick;
        private DispatcherTimer _timer;
        private TimeSpan _duration;
        private DateTime _startTime;
        private DateTime _raceTime;
        private readonly RaceRepository _raceRepository;
        private readonly RfidManager _rfidManager;
        private readonly ReportManager _reportManager;
        private Race _race;

        public RaceView()
        {
            InitializeComponent();

            var mainWindow = (MainWindow)Application.Current.MainWindow;
            _rfidManager = mainWindow.RfidManager;
            _reportManager = mainWindow.ReportManager;


            this.DataContext = _rfidManager;
            cbRaces.DataContext = _reportManager;
            cbRaces.ItemsSource = _reportManager.Races;

            //  btnSetTimer.IsEnabled = _rfidManager.Connected;

            btnStartTimer.IsEnabled = false;
            btnStopTimer.IsEnabled = false;

            //  ttbTimer.IsStarted = false;
            _raceRepository = new RaceRepository();

            this.Show.Text = _raceTime.ToString("HH:mm:ss:ff");

            dgSplits.ItemsSource = _rfidManager.AthleteSplits;

            //   btnStartTimer.IsEnabled = RfidManager.Connected;
        }

        private void SetTimer()
        {
            _timer = new DispatcherTimer(new TimeSpan(0, 0, 0, 0, 1), DispatcherPriority.Normal, TimerOnTick, this.Dispatcher);
            _timer.Stop();
            _raceTime = DateTime.Now.Add(-_startTime.TimeOfDay);
        }

        private void TimerOnTick(object sender, object o)
        {
            _duration = _duration.Add(_timer.Interval);
            _raceTime = DateTime.Now.Add(-_startTime.TimeOfDay);

            this.Show.Text = _raceTime.ToString("HH:mm:ss:ff"); //+ " - " + Convert.ToDateTime(Duration.ToString()).ToString("HH:mm:ss:fff");// DateTime.Now.ToString("HH:mm:ss:ff") + _dateTime;
            // this.Show.Text = String.Format("{0:D2}:{1:D2}", Duration.Seconds, Duration.Milliseconds);

            // Duration.DataContext = Duration;
        }


        private void btnNewRace_Click(object sender, RoutedEventArgs e)
        {
            if (_rfidManager.IsReading)
            {
                return;
            }
            _raceTime = DateTime.MinValue;
            this.Show.Text = _raceTime.ToString("HH:mm:ss:ff");
            
            _race = new Race
            {
                Name = rbRaceName.Text,
                StartDateTime = DateTime.MinValue,
            };

            _reportManager.Races.Insert(0, _race);

            _rfidManager.ClearSplits();

            btnStartTimer.IsEnabled = true;
            btnStopTimer.IsEnabled = false;
            

        }

        private void btnStartTimer_Click(object sender, RoutedEventArgs e)
        {
            _startTime = DateTime.Now;
            SetTimer();

            _race.StartDateTime = _startTime;
            _race.StartTime = _startTime.ToString("hh.mm.ss.ff");

            _raceRepository.Add(_race);
            _raceRepository.Save();
            _rfidManager.Start(_race);
            _timer.Start();
            
            btnStartTimer.IsEnabled = false;
            btnStopTimer.IsEnabled = true;

        }

        private void btnStopTimer_Click(object sender, RoutedEventArgs e)
        {
            // ttbTimer.IsStarted = false;
            _rfidManager.Stop();
            _timer.Stop();
            _race.FinishDateTime = DateTime.Now;
            _raceRepository.Edit(_race, _race.Id);
            _raceRepository.Save();

            btnStopTimer.IsEnabled = false;

        }

        //private void btnResetTimer_Click(object sender, RoutedEventArgs e)
        //{
        //    _dateTime = DateTime.Now;


        //    //_timer.Stop();
        //    //_timer.Interval = new TimeSpan(0, 0, 0, 0, 1);
        //    //_timer.Start();



        //    //  Duration = new TimeSpan();
        //    //   ttbTimer.Reset();
        //}

    }
}
