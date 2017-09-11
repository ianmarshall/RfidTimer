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
using RaceTimer.App.Views;
using RaceTimer.Business;
using RaceTimer.Data;


namespace RaceTimer.App
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public RfidManager RfidManager { get; set; }

        public MainWindow()
        {
            RfidManager = new RfidManager();
            InitializeComponent();
        }


        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
           

        }
    }
}
