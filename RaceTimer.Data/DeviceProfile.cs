using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity;
using System.Linq;
using System.Runtime.CompilerServices;

namespace RaceTimer.Data
{
    public class RaceTimerContext : DbContext
    {
        public RaceTimerContext() : base("name=RaceTimerDatabase")
        {
        }
        public DbSet<ReaderProfile> ReaderProfiles { get; set; }
        public DbSet<Tag> Tags { get; set; }
        public DbSet<Athlete> Athletes { get; set; }
    }


    public enum ConnectionType
    {
        Serial = 0,
        Eathernet = 1
    }

    public enum InventorySearchMode
    {
        Session1SingleTarget = 1,
        Session2DualTarget = 2,
        Session3DualTargetWithSuppression = 3,
    }

    public enum ReaderModel
    {
        ChaFonIntegratedR2000,
        ChaFonFourChannelR2000,
        MotorolaFourChannelFx7400
    }

  

    public class ReaderProfile : INotifyPropertyChanged
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        public string Name { get; set; }
        public ReaderModel Model { get; set; }
        [NotMapped]
        public IList<ReaderModel> Models { get; } = Enum.GetValues(typeof(ReaderModel)).Cast<ReaderModel>().ToList();
        public ConnectionType ConnectionType { get; set; }
        public InventorySearchMode InventorySearchMode { get; set; }

        private string _Status;

        [NotMapped]
        public string Status
        {
            get { return _Status; }
            set
            {
                if (_Status != value)
                {
                    _Status = value;
                    OnPropertyChanged("Status");
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public class Tag : EventArgs
    {
        public int Id { get; set; }
        public string TagId { get; set; }
        public DateTime Time { get; set; }
        public string Rssi { get; set; }
        public string SplitName { get; set; }
    }
}
