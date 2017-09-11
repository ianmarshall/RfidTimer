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
        public DbSet<Split> Tags { get; set; }
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

    public enum ReadingMode
    {
        Start,
        Finish,
        Desktop
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
        public ReadingMode ReadingMode { get; set; }
        [NotMapped]
        public IList<ReaderModel> Models { get; } = Enum.GetValues(typeof(ReaderModel)).Cast<ReaderModel>().ToList();
        [NotMapped]
        public IList<ReadingMode> ReadingModes { get; } = Enum.GetValues(typeof(ReadingMode)).Cast<ReadingMode>().ToList();
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
}
