using System;
using System.Collections.ObjectModel;
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
        private AthleteRepository _athleteRepository;

        public ObservableCollection<Athlete> Athletes;

        public AthletesView()
        {
            InitializeComponent();
            var mainWindow = (MainWindow)Application.Current.MainWindow;
            _rfidManager = mainWindow.RfidManager;
            this.DataContext = _rfidManager;

            _athleteRepository = new AthleteRepository();

            _athleteRepository.Add(new Athlete
            {
                FirstName = "Ian",
                LastName = "Marshall",
                Dob = new DateTime(1979, 3, 8),
                Club = "Hunts AC"
            });

            _athleteRepository.Save();

            Athletes = new ObservableCollection<Athlete>(_athleteRepository.GetAll());

            dgAthletes.ItemsSource = Athletes;
        }

        private void dgAthletes_RowEditEnding(object sender, DataGridRowEditEndingEventArgs e)
        {
            Athlete athlete = (Athlete) e.Row.Item;
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

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            AssignTags assignTags = new AssignTags();
            assignTags.Show();
        }

        private void btnAssign_Click(object sender, RoutedEventArgs e)
        {
            _rfidManager.StartAssigning(Athletes);
        }
    }
}
