using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
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
        private ObservableCollection<ReaderProfile> _readers = new ObservableCollection<ReaderProfile>();

        private readonly ReaderProfileRepository _readerProfileRepository = new ReaderProfileRepository();
       
        public ReadersView()
        {
            InitializeComponent();
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
            RfidManager.SetUp(_readers);
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

            if (RfidManager.Test(reader))
            {
                currentReader.Status = "Successfull connection";
            }
            else
            {
                currentReader.Status = "Error";
            }

        }
    }
}
