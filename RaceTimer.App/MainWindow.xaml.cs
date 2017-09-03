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
using System.Windows.Navigation;
using System.Windows.Shapes;
using RaceTimer.Data;
using RaceTimer.Device.IntegratedReaderR2000;

namespace RaceTimer.App
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        private Adapter adapter;

        public MainWindow()
        {
            InitializeComponent();

            Adapter adapter = new Adapter();
        }

        private void button_Click(object sender, RoutedEventArgs e)
        {
            adapter = new Adapter();
            if(adapter.OpenConnection())
            {
                adapter.BeginReading();    
            }

            Class1 cl = new Class1();
            cl.add();
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            adapter.CloseConnection();
        }
    }
}
