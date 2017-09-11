using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using RaceTimer.Business;
using RaceTimer.Data;


namespace RaceTimer.App.Views
{
    /// <summary>
    /// Interaction logic for ReadersView.xaml
    /// </summary>
    public partial class ReadersView : UserControl
    {
        private RfidManager _rfidManager;
        private ObservableCollection<ReaderProfile> _readers = new ObservableCollection<ReaderProfile>();

        private readonly ReaderProfileRepository _readerProfileRepository = new ReaderProfileRepository();
       
        public ReadersView()
        {
            InitializeComponent();
            var mainWindow = (MainWindow)Application.Current.MainWindow;

            _rfidManager = mainWindow.RfidManager;

            this.DataContext = _rfidManager;
            _readers = new ObservableCollection<ReaderProfile>(_readerProfileRepository.GetAll());

            readers.ItemsSource = _readers;
        }

        private void btnAddReader_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            var readerProfile = new ReaderProfile();
            _readerProfileRepository.Add(readerProfile);
            _readerProfileRepository.Save();
            _readers.Add(readerProfile);
        }

        private void btnEnableAllReaders_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            _rfidManager.SetUp(_readers);
            if (_rfidManager.Connected)
            {
                btnEnableAllReaders.IsEnabled = false;
                btnAddReader.IsEnabled = false;
                btnDisableAllReaders.IsEnabled = true;
            }

        }

        private void Button_Click_Remove(object sender, System.Windows.RoutedEventArgs e)
        {

            ReaderProfile reader = (ReaderProfile)((Button)sender).Tag;
            _readerProfileRepository.Delete(reader);
            _readerProfileRepository.Save();
            _readers.Remove(reader);

        }

        private void Button_Click_Save(object sender, System.Windows.RoutedEventArgs e)
        {

            ReaderProfile reader = (ReaderProfile)((Button)sender).Tag;
            _readerProfileRepository.Edit(reader, reader.Id);
            _readerProfileRepository.Save();
        }

        private void Button_Click_Test(object sender, System.Windows.RoutedEventArgs e)
        {
            ReaderProfile reader = (ReaderProfile)((Button)sender).Tag;
            ReaderProfile currentReader = _readers.First(x => x.Id == reader.Id);

            if (_rfidManager.Test(reader))
            {
                currentReader.Status = "Successfull connection";
            }
            else
            {
                currentReader.Status = "Error";
            }

        }

        private void btnDisableAllReaders_Click(object sender, RoutedEventArgs e)
        {
            if (_rfidManager.IsReading == false && _rfidManager.CloseAll())
            {
                btnDisableAllReaders.IsEnabled = false;
                btnEnableAllReaders.IsEnabled = true;
                btnAddReader.IsEnabled = true;
            }
        }
    }
}
