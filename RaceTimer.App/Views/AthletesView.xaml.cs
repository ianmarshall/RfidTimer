﻿using System;
using System.Windows;
using System.Windows.Controls;
using RaceTimer.Business;
using RaceTimer.Data;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace RaceTimer.App.Views
{
    /// <summary>
    /// Interaction logic for AthletesView.xaml
    /// </summary>
    public partial class AthletesView : UserControl
    {
        private RfidManager _rfidManager;
        private AthleteManager _athleteManager;
        private AthleteRepository _athleteRepository;
        private bool _autoAssign;


        public AthletesView()
        {
            InitializeComponent();
            var mainWindow = (MainWindow)Application.Current.MainWindow;
            _rfidManager = mainWindow.RfidManager;
            _athleteManager = mainWindow.AthleteManager;
            btnStopAssign.IsEnabled = false;

            this.DataContext = _athleteManager;

            btnAssign.DataContext = _rfidManager;

            _athleteRepository = new AthleteRepository();


            _athleteRepository.Save();


         dgAthletes.ItemsSource = _athleteManager.Athletes;
            //    _athleteManager.
        }

        private void dgAthletes_RowEditEnding(object sender, DataGridRowEditEndingEventArgs e)
        {
            if (e.EditAction == DataGridEditAction.Commit)
            {
                // Athlete athlete = (Athlete)dgAthletes.SelectedItem;

                Athlete athlete = e.Row.DataContext as Athlete;
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

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            AssignTags assignTags = new AssignTags();
            assignTags.Show();
        }

        private void btnAssign_Click(object sender, RoutedEventArgs e)
        {
            btnStopAssign.IsEnabled = true;
            _athleteManager.Message = "";
            _rfidManager.StartAssigning();
        }

        private void cbAutoAssign_Click(object sender, RoutedEventArgs e)
        {
            if (cbAutoAssign.IsChecked.HasValue && cbAutoAssign.IsChecked.Value)
            {
                _athleteManager.AutoAssign = true;
                _athleteManager.NextBib = _athleteRepository.GetMaxBib() + 1;
            }
            else
            {
                _athleteManager.AutoAssign = false;
            }
        }

        private void btnStopAssign_Click(object sender, RoutedEventArgs e)
        {
            _rfidManager.Stop();
            btnStopAssign.IsEnabled = false;
        }


        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            foreach (var ath in _athleteManager.Athletes)
            {
                _athleteRepository.Edit(ath, ath.Id);
                _athleteRepository.Save();
            }
        }

        private void dgAthletes_AutoGeneratedColumns(object sender, EventArgs e)
        {
            var grid = (DataGrid)sender;
            foreach (var item in grid.Columns)
            {
                if (item.Header.ToString() == "Edit")
                {
                    item.DisplayIndex = grid.Columns.Count - 1;
                    continue;
                }

                if (item.Header.ToString() == "Delete")
                {
                    item.DisplayIndex = grid.Columns.Count - 1;
                    continue;
                }
            }
        }

        private void btnEdit_Click(object sender, RoutedEventArgs e)
        {

            Athlete athlete = ((FrameworkElement)sender).DataContext as Athlete;
            if (athlete != null)
            {
                var editForm = new AthleteEdit(athlete, _athleteManager, _athleteRepository);

                editForm.Show();
            }

        }

        private void btnDelete_Click(object sender, RoutedEventArgs e)
        {
            Athlete athelete = ((FrameworkElement)sender).DataContext as Athlete;
            if (MessageBox.Show("Are you sure", "Question", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes)
            {
                if (athelete != null)
                {
                    _athleteRepository.DeleteAthlete(athelete);
                    _athleteRepository.Save();

                    _athleteManager.Athletes.Remove(athelete);

                   
                }
            }
        }

        private void btnAdd_Click(object sender, RoutedEventArgs e)
        {
            var athlete = new Athlete();

            var editForm = new AthleteEdit(athlete, _athleteManager, _athleteRepository);

            editForm.Show();
        }

        private void btnExport_Click(object sender, RoutedEventArgs e)
        {
            _athleteManager.ExportAthletes();
        }

        private void btnImport_Click(object sender, RoutedEventArgs e)
        {
            Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();

            // Set filter for file extension and default file extension 
            dlg.DefaultExt = ".csv";
            dlg.Filter = "CSV Files (*.csv)|*.csv";


            // Display OpenFileDialog by calling ShowDialog method 
            Nullable<bool> result = dlg.ShowDialog();

            // Get the selected file name and display in a TextBox 
            if (result == true)
            {
                // Open document 
                string filename = dlg.FileName;

                _athleteManager.ImportAthletes(filename);

            }
        }

        private void btnCheckDups_Click(object sender, RoutedEventArgs e)
        {
            var duplicateItems = _athleteManager.Athletes.GroupBy(item => item).ToDictionary(x => x.Key, x => x.Count());
            MessageBox.Show(duplicateItems.Count() + " DUPLICATES");
            dgDups.ItemsSource = _athleteManager.Athletes.GroupBy(item => item).ToDictionary(x => x.Key, x => x).Keys.Select(x => x);
        }
    }
}
