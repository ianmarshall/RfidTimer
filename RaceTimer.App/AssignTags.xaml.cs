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
using RaceTimer.Business;
using RaceTimer.Data;

namespace RaceTimer.App
{
    /// <summary>
    /// Interaction logic for AssignTags.xaml
    /// </summary>
    public partial class AssignTags : Window
    {
        private RfidManager _rfidManager;
        private AthleteRepository _athleteRepository;
        private int _nextBib;

        public AssignTags()
        {
            InitializeComponent();
            var mainWindow = (MainWindow)Application.Current.MainWindow;
            _rfidManager = mainWindow.RfidManager;
            _athleteRepository = new AthleteRepository();
        }

        private void btnAssign_Click(object sender, RoutedEventArgs e)
        {
            int maxBib = _athleteRepository.GetAll().Max(x => x.Bib);
                
            //    OrderBy(x=>x.Bib).FirstOrDefault(x => string.IsNullOrEmpty(x.TagId));


        }
    }
}
