using Rocket.API;

namespace SBAdvancedTeleportation
{
    public class AdvancedTeleportationConfiguration : IRocketPluginConfiguration
    {
        public string SqlConnectionString { get; set; }
        public string RichLeftDelimiter { get; set; }
        public string RichRightDelimiter { get; set; }
        public string MessageIcon { get; set; }
        public bool DefaultGroupAutoAccept { get; set; }
        public int TeleportDelay { get; set; }
        public int TeleportCooldown { get; set; }
        public bool PvpTimerEnabled { get; set; }
        public int PvpTimer { get; set; }
        public void LoadDefaults()
        {
            SqlConnectionString = "server=156.236.84.65;port=3306;uid=u4483_l9JIF5RkLG;pwd=wD.WvLoGqGnzIXRCMKq=tXrp;database=s4483_TESTDB";
            RichLeftDelimiter = "-=";
            RichRightDelimiter = "=-";
            DefaultGroupAutoAccept = true;
            TeleportDelay = 5;
            TeleportCooldown = 10;
            PvpTimerEnabled = true;
            PvpTimer = 5;
        }
    }
}
