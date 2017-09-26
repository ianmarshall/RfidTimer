using RaceTimer.Business;
using RaceTimer.Data;
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
using System.Windows.Shapes;

namespace RaceTimer.App
{
    /// <summary>
    /// Interaction logic for AthleteEdit.xaml
    /// </summary>
    public partial class AthleteEdit : Window
    {
     

        private AthleteManager _athleteManager;
        private AthleteRepository _athleteRepository;

        public Athlete _athlete { get; set; }


        public AthleteEdit(Athlete athlete, AthleteManager athleteManager, AthleteRepository athleteRepository)
        {
            _athlete = athlete;
            _athleteManager = athleteManager;
            _athleteRepository = athleteRepository;
            InitializeComponent();
            grdEdit.DataContext = _athlete;
        }

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            var isNew = _athlete.Id == 0;
         
            if(isNew)
            {
                _athleteRepository.Add(_athlete);
            }
            else
            {
                _athleteRepository.Edit(_athlete, _athlete.Id);
            }

            _athleteRepository.Save();

            if (isNew)
            {
                _athleteManager.Athletes.Add(_athlete);
            }
            else
            {
                var index = _athleteManager.Athletes.IndexOf(_athlete);//get its location in list
                _athleteManager.Athletes[index] = _athlete;
            }

            this.Close();
        }


        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
         //   _athleteManager.ReloadAtheletes();
        }
    }
}
