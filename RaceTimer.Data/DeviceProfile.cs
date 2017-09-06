using System.Data.Entity;

namespace RaceTimer.Data
{
    public class RaceTimerContext : DbContext
    {
        public RaceTimerContext() : base("name=RaceTimerDatabase")
        {
        }
        public DbSet<ReaderProfile> ReaderProfiles { get; set; }
        public DbSet<Tag> Tags { get; set; }
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

    public class ReaderProfile
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public ConnectionType ConnectionType { get; set; }
        public InventorySearchMode InventorySearchMode { get; set; }
    }

    public class Tag
    {
        public int Id { get; set; }
        public string TagId { get; set; }
    }
}
