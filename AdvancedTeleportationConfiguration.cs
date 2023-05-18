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
        public void LoadDefaults()
        {
            SqlConnectionString = "server=156.236.84.65;uid=u4483_1AutG8ISAR;pwd=j+XeDdISx8F5eIuZUHY3@Ew=;database=s4483_TestDB";
            RichLeftDelimiter = "-=";
            RichRightDelimiter = "=-";
            DefaultGroupAutoAccept = true;
            TeleportDelay = 5;
            TeleportCooldown = 10;
        }
    }
}
