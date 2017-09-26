using System.Windows;
using System.Windows.Controls;
using RaceTimer.Business;
using RaceTimer.Data;
using System.Windows.Forms;
using System.ComponentModel;
using System.Collections.Generic;
using RaceTimer.Business.Reports;

namespace RaceTimer.App.Views
{
    /// <summary>
    /// Interaction logic for ReportView.xaml
    /// </summary>
    public partial class ReportView : System.Windows.Controls.UserControl
    {
        private readonly RaceRepository _raceRepository;
        private readonly RfidManager _rfidManager;
        private readonly ReportManager _reportManager;
        private Timer timer;
        private readonly BackgroundWorker worker = new BackgroundWorker();



        public ReportView()
        {
            InitializeComponent();

            var mainWindow = (MainWindow)System.Windows.Application.Current.MainWindow;
            _rfidManager = mainWindow.RfidManager;
            _reportManager = mainWindow.ReportManager;

            this.DataContext = _reportManager;
            cbRaces.DataContext = _reportManager;
            cbRaces.ItemsSource = _reportManager.Races;
            //dgSplits.DataContext = _reportManager;
            //dgSplits.ItemsSource = _reportManager.Splits;
            //    dgResults.ItemsSource = _reportManager.SplitResults;
            dgResults.DataContext = _reportManager;
            worker.DoWork += worker_DoWork;
            worker.RunWorkerCompleted += worker_RunWorkerCompleted;

        }

        private void worker_DoWork(object sender, DoWorkEventArgs e)
        {
            Race race = (Race)e.Argument;

            List<SplitResult> results = _reportManager.LoadResults(race);

            e.Result = results;
        }

        private void worker_RunWorkerCompleted(object sender,
                                               RunWorkerCompletedEventArgs e)
        {
            dgResults.ItemsSource = (List<SplitResult>)e.Result;
        }


        private void cbRaces_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cbRaces.SelectedItem != null)
            {
                Race race = (Race)cbRaces.SelectedItem;
                //  _reportManager.LoadSplits(race);
                dgResults.ItemsSource = _reportManager.LoadResults(race);

                //  dgSplits.ItemsSource = _reportManager.Splits;
            }
        }

        private void btnExport_Click(object sender, RoutedEventArgs e)
        {
            _reportManager.ExportResults();
        }


        private void StartReadDelay()
        {
            int delayMiliSeconds = 10 * 1000;

            timer = new Timer();
            timer.Interval = delayMiliSeconds;
            timer.Tick += (s, e) =>
            {
                // logger.Info("Started reading at {0} from IntegratedReaderR2000 after a {1} delay", DateTime.Now, _readerProfile.StartReadDelay);
                //_aTimer.Enabled = true;
                //timer.Stop();
                if (_rfidManager.CurrentRace != null)
                {
                    worker.RunWorkerAsync(_rfidManager.CurrentRace);
                }
            };

            timer.Start();
            if (_rfidManager.CurrentRace != null)
            {
                //  dgResults.ItemsSource = _reportManager.LoadResults(_rfidManager.CurrentRace);
                worker.RunWorkerAsync(_rfidManager.CurrentRace);
            }
        }

        private void btnAutoRefresh_Click(object sender, RoutedEventArgs e)
        {
            StartReadDelay();
        }

        private void cbAutoRefresh_Checked(object sender, RoutedEventArgs e)
        {
            if (cbAutoRefresh.IsChecked.HasValue && cbAutoRefresh.IsChecked.Value)
            {
                StartReadDelay();
            }
        }

        private void cbAutoRefresh_Unchecked(object sender, RoutedEventArgs e)
        {
            timer.Stop();
        }
    }
}
