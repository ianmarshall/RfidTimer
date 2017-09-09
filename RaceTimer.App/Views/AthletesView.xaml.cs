using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
using RaceTimer.Business.ViewModel;
using RaceTimer.Data;

namespace RaceTimer.App.Views
{
    
    /// <summary>
    /// Interaction logic for AthletesView.xaml
    /// </summary>
    public partial class AthletesView : UserControl
    {
        private AthleteRepository _athleteRepository;

        public static ObservableCollection<Athlete> Athletes;

        public AthletesView()
        {
            InitializeComponent();
            _athleteRepository = new AthleteRepository();

            _athleteRepository.Add(new Athlete
            {
                FirstName = "Ian",
                LastName = "Marshall",
                Dob = new DateTime(1979,3, 8),
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
    }
}
