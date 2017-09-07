using System;
using System.Collections.Generic;
using System.Diagnostics;
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
using System.Windows.Threading;
using RaceTimer.Business;
using RaceTimer.Data;


namespace RaceTimer.App
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

    

        public MainWindow()
        {
            InitializeComponent();
          
            _rfidManager = new RfidManager();


        }


     

        DispatcherTimer dt = new DispatcherTimer();
        Stopwatch sw = new Stopwatch();
        string _currentTime = string.Empty;

        private RfidManager _rfidManager;



        //void dt_Tick(object sender, EventArgs e)
        //{
        //    _rfidManager.SetUp();

        //    if (sw.IsRunning)
        //    {
        //        TimeSpan ts = sw.Elapsed;
        //        _currentTime = String.Format("{0:00}:{1:00}:{2:00}",
        //            ts.Minutes, ts.Seconds, ts.Milliseconds / 10);
        //        clocktxtblock.Text = _currentTime;
        //    }
        //}


        private void StartBtn_Click(object sender, RoutedEventArgs e)
        {
            sw.Start();
            dt.Start();
            _rfidManager.Start();
        }

        private void button_Click(object sender, RoutedEventArgs e)
        {
           
           

            Class1 cl = new Class1();
            cl.add();
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
           

        }
    }
}
