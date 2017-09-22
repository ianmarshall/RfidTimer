using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using RaceTimer.Business;
using RaceTimer.Data;
using NLog;

namespace RaceTimer.App.Views
{
    /// <summary>
    /// Interaction logic for ReadersView.xaml
    /// </summary>
    public partial class ReadersView : UserControl
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();

        private RfidManager _rfidManager;
        private readonly ObservableCollection<ReaderProfile> _readers;

        private readonly ReaderProfileRepository _readerProfileRepository = new ReaderProfileRepository();

        public ReadersView()
        {
            InitializeComponent();
            var mainWindow = (MainWindow)Application.Current.MainWindow;
            _rfidManager = mainWindow.RfidManager;

            this.DataContext = _rfidManager;
            _readers = new ObservableCollection<ReaderProfile>(_readerProfileRepository.GetAll());

            readersControl.ItemsSource = _readers;

          
        }

        private void btnAddReader_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            var readerProfile = new ReaderProfile();
            readerProfile.PowerDbm = 30;

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



            //foreach (var item in readersControl.Items)
            //{
            //    var container =
            //        readersControl.ItemContainerGenerator.ContainerFromItem(item) as FrameworkElement;

            //    Button btn = container.FindName("btnEnable") as Button;

            //    //UserInformation user = container.DataContext as UserInformation;

            //    //bool isMale = true;
            //    //if (user.sex == isMale && checkbox.IsChecked.Value == true)
            //    //{
            //    //    container.Visibility = System.Windows.Visibility.Visible;
            //    //}
            //}


            //for (int i = 0; i < readers.Items.Count; i++)
            //{

            //    GroupBox gb = (GroupBox)readers.ItemContainerGenerator.ContainerFromItem(i);
            //    Button btn = gb.ContentTemplate.FindName("btnEnable", gb) as Button;

            //    if (btn != null) btn.IsEnabled = true;

            //    //if (!btn.IsEnabled)
            //    //{
            //    //    //do stuff

            //    //}
            //}

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

            currentReader.Status = _rfidManager.Test(reader) ? "Successfull connection" : "Error";
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





        private void btnEnable_Click(object sender, RoutedEventArgs e)
        {
            ReaderProfile reader = (ReaderProfile)((Button)sender).Tag;

            bool connected = _rfidManager.EnableReader(reader);

            Button button = (Button)sender;
            button.IsEnabled = !connected;
        }

        private void cbReadingMode_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ReaderProfile reader = (ReaderProfile)((ComboBox)sender).Tag;

            if (reader.ReadingMode == ReadingMode.Desktop)
            {
                reader.PowerDbm = 5;
                reader.InventorySearchMode = InventorySearchMode.Session2DualTarget;
            }

            if (reader.ReadingMode == ReadingMode.Start || reader.ReadingMode == ReadingMode.Finish)
            {
                reader.PowerDbm = 30;
                reader.InventorySearchMode = InventorySearchMode.Session2DualTarget;
            }

            if (reader.ReadingMode == ReadingMode.Custom)
            {
                reader.PowerDbm = 30;
                reader.InventorySearchMode = InventorySearchMode.Session1SingleTarget;
            }
        }
    }
}

