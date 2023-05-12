using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Steamworks;

namespace SBAdvancedTeleportation.Models
{
    public class MockDatabase
    {
        public static List<KeyValuePair<CSteamID, CSteamID>> SBAdvancedTeleportation_Whitelist { get; set; }
        public static List<KeyValuePair<CSteamID, CSteamID>> SBAdvancedTeleportation_Blacklist { get; set; }

        public static void Initalize()
        {
            SBAdvancedTeleportation_Whitelist = new List<KeyValuePair<CSteamID, CSteamID>>();
            SBAdvancedTeleportation_Blacklist = new List<KeyValuePair<CSteamID, CSteamID>>();
        }
    }
}
