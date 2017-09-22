using System;
using System.Windows;
using System.Windows.Controls;
using RaceTimer.Business;
using RaceTimer.Data;

namespace RaceTimer.App.Views
{
    /// <summary>
    /// Interaction logic for AthletesView.xaml
    /// </summary>
    public partial class AthletesView : UserControl
    {
        private RfidManager _rfidManager;
        private AthleteManager _athleteManager;
        private AthleteRepository _athleteRepository;
        private bool _autoAssign;


        public AthletesView()
        {
            InitializeComponent();
            var mainWindow = (MainWindow)Application.Current.MainWindow;
            _rfidManager = mainWindow.RfidManager;
            _athleteManager = mainWindow.AthleteManager;
            btnStopAssign.IsEnabled = false;

            this.DataContext = _athleteManager;

            btnAssign.DataContext = _rfidManager;

            _athleteRepository = new AthleteRepository();

            //_athleteRepository.Add(new Athlete
            //{
            //    FirstName = "Ian",
            //    LastName = "Marshall",
            //    Dob = new DateTime(1979, 3, 8),
            //    Club = "Hunts AC"
            //});

            _athleteRepository.Save();

            //      Athletes = new ObservableCollection<Athlete>(_athleteRepository.GetAll());

            dgAthletes.ItemsSource = _athleteManager.Athletes;
            //    _athleteManager.
        }

        private void dgAthletes_RowEditEnding(object sender, DataGridRowEditEndingEventArgs e)
        {
            if (e.EditAction == DataGridEditAction.Commit)
            {
                // Athlete athlete = (Athlete)dgAthletes.SelectedItem;

                Athlete athlete = e.Row.DataContext as Athlete;
                if (athlete.Id > 0)
                {
                    _athleteRepository.Edit(athlete, athlete.Id);
                }
                else
                {
                    _athleteRepository.Add(athlete);
                }
                _athleteRepository.Save();
            }

        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            AssignTags assignTags = new AssignTags();
            assignTags.Show();
        }

        private void btnAssign_Click(object sender, RoutedEventArgs e)
        {
            btnStopAssign.IsEnabled = true;
            _athleteManager.Message = "";
            _rfidManager.StartAssigning();
        }

        private void cbAutoAssign_Click(object sender, RoutedEventArgs e)
        {
            if (cbAutoAssign.IsChecked.HasValue && cbAutoAssign.IsChecked.Value)
            {
                _athleteManager.AutoAssign = true;
                _athleteManager.NextBib = _athleteRepository.GetMaxBib() + 1;
            }
            else
            {
                _athleteManager.AutoAssign = false;
            }
        }

        private void btnStopAssign_Click(object sender, RoutedEventArgs e)
        {
            _rfidManager.Stop();
            btnStopAssign.IsEnabled = false;
        }


        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            foreach (var ath in _athleteManager.Athletes)
            {
                _athleteRepository.Edit(ath, ath.Id);
                _athleteRepository.Save();
            }
        }
    }
}
