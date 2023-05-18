using SBAdvancedTeleportation.Enums;

namespace SBAdvancedTeleportation.Models
{
    public class DatabaseEntry
    {
        public ulong PlayerID { get; set; }
        public ulong TargetID { get; set; }
        public EListType ListType { get; set; }
    }
}
