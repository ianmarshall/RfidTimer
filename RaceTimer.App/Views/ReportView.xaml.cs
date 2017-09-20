using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
using RaceTimer.Business;
using RaceTimer.Data;

namespace RaceTimer.App.Views
{
    /// <summary>
    /// Interaction logic for ReportView.xaml
    /// </summary>
    public partial class ReportView : UserControl
    {
        private readonly RaceRepository _raceRepository;
        private readonly RfidManager _rfidManager;
        private readonly ReportManager _reportManager;

        public ReportView()
        {
            InitializeComponent();

            var mainWindow = (MainWindow)Application.Current.MainWindow;
            _rfidManager = mainWindow.RfidManager;
            _reportManager = mainWindow.ReportManager;


            this.DataContext = _reportManager;
            cbRaces.DataContext = _reportManager;
            cbRaces.ItemsSource = _reportManager.Races;
            dgSplits.DataContext = _reportManager;
            dgSplits.ItemsSource = _reportManager.Splits;
        }

        private void cbRaces_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cbRaces.SelectedItem != null)
            {
                Race race = (Race)cbRaces.SelectedItem;
                  _reportManager.LoadSplits(race);
                //_reportManager.LoadResults(race);

                dgSplits.ItemsSource = _reportManager.Splits;
            }
        }

        private void btnExport_Click(object sender, RoutedEventArgs e)
        {
            _reportManager.ExportResults();
        }
    }
}
