using Rocket.API;

namespace SBAdvancedTeleportation
{
    public class AdvancedTeleportationConfiguration : IRocketPluginConfiguration
    {
        public string SqlConnectionString { get; set; }
        public string RichLeftDelimiter { get; set; }
        public string RichRightDelimiter { get; set; }
        public bool DefaultGroupAutoAccept { get; set; }
        public int TeleportDelay { get; set; }
        public void LoadDefaults()
        {
            SqlConnectionString = "";
            RichLeftDelimiter = "-=";
            RichRightDelimiter = "=-";
            DefaultGroupAutoAccept = true;
            TeleportDelay = 5;
        }
    }
}
