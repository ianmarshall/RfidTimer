using System;
using System.Collections.ObjectModel;

namespace RaceTimer.App.Views
{
   
    public class SplitsViewModel 
    {
        public ObservableCollection<Split> Splits;

        private string _startTime = "na";

        public string StartTime
        {
            get { return _startTime; }
            set
            {
                _startTime = value;
            }
        }


        public void StartRace()
        {
            _startTime = DateTime.UtcNow.ToString();
        }
    }

    public class Split
    {
        public string AtheleteName { get; set; }
        public int AthleteId { get; set; }
        public string SplitName { get; set; }
        public int TagId { get; set; }
        public string Time { get; set; }
    }
}
