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
        public DbSet<Settings> Settings { get; set; }
        public DbSet<ReaderProfile> ReaderProfiles { get; set; }
        public DbSet<Split> Splits { get; set; }
        public DbSet<Athlete> Athletes { get; set; }


        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {

            modelBuilder.Entity<Split>()
                        .HasOptional<Athlete>(x => x.Athlete);

            //modelBuilder.Entity<Athlete>()
            //            .HasMany<Split>(w => w.Splits)
            //            .WithOptional(d => d.s);
        }

    }
    public enum ConnectionType
    {
        Serial = 0,
        Ethernet = 1
    }

    public enum InventorySearchMode
    {
        Session0Continues = 0,
        Session1SingleTarget = 1,
        Session2DualTarget = 2,
        Session3DualTargetWithSuppression = 3,
        Auto = 4,
    }

    public enum ReadingMode
    {
        Start,
        Finish,
        Desktop,
        Custom
    }

    public enum ReaderModel
    {
        ChaFonFourChannelR2000,
        ChaFonIntegratedR2000,
        ChaFonUsbDesktop,
        MotorolaFourChannelFx7400
    }

    public enum ComPort
    {
        Auto = 0,
        Com1 = 1,
        Com2 = 2,
        Com3 = 3,
        Com4 = 4,
        Com5 = 5,
        Com6 = 6,
        Com7 = 7,
        Com8 = 8,
        Com9 = 9,
        Com10 = 10,
        Com11 = 11,
        Com12 = 12,
    }


    public enum StartReadDelay
    {
        None = 0,
        T10s = 10,
        T20s = 20,
        T30s = 30,
        T60s = 60,
        T90s = 90,
    }

    public class ReaderProfile : INotifyPropertyChanged, System.ComponentModel.IDataErrorInfo
    {
        public ReaderProfile()
        {
            PowerDbm = 30;
            IpAddress = "192.168.0.250";
            GatingTime = 4;
        }

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
       

        [NotMapped]
        public IList<InventorySearchMode> InventorySearchModes { get; } = Enum.GetValues(typeof(InventorySearchMode)).Cast<InventorySearchMode>().ToList();

        public ComPort ComPort { get; set; }

        public string IpAddress { get; set; }

        [NotMapped]
        public IList<ComPort> ComPorts { get; } = Enum.GetValues(typeof(ComPort)).Cast<ComPort>().ToList();

        [NotMapped]
        public IList<ConnectionType> ConnectionTypes { get; } = Enum.GetValues(typeof(ConnectionType)).Cast<ConnectionType>().ToList();

        public StartReadDelay StartReadDelay { get; set; }
        [NotMapped]
        public IList<StartReadDelay> StartReadDelays { get; } = Enum.GetValues(typeof(StartReadDelay)).Cast<StartReadDelay>().ToList();

        public int PowerDbm { get; set; }
     

        public int GatingTime { get; set; }

        [NotMapped]
        public IEnumerable<int> Powers { get; } = Enumerable.Range(1, 30);

        private string _status;

        [NotMapped]
        public string Status
        {
            get { return _status; }
            set
            {
                if (_status != value)
                {
                    _status = value;
                    OnPropertyChanged("Status");
                }
            }
        }

       // public int SettingsId { get; set; }

        //[ForeignKey("SettingsId")]
        //public virtual Settings Settings { get; set; }

        string IDataErrorInfo.Error
        {
            get
            {
                return null;
            }
        }

        string IDataErrorInfo.this[string columnName]
        {
            get
            {
                switch (columnName)
                {
                    case "Name":
                        if (string.IsNullOrEmpty(Name))
                            return "Name must be set";
                        break;
                }

                return string.Empty;
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
