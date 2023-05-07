using System;
using System.Collections.Generic;
using System.Linq;
using Steamworks;

namespace SBAdvancedTeleportation.Models
{
    [Serializable]
    public class PlayerPreferences
    {
        public ulong PlayerID { get; set; }
        public List<ulong> WhitelistedPlayerIDs { get; set; }
        public List<ulong> BlacklistedPlayerIDs { get; set; }
        public bool ShouldAutoAcceptGroup { get; set; }

        public CSteamID GetPlayerID()
        {
            return (CSteamID)PlayerID;
        }

        public bool IsWhitelisted(CSteamID steamID)
        {
            return WhitelistedPlayerIDs.Any((id) => id == steamID.m_SteamID);
        }

        public bool IsBlacklisted(CSteamID steamID)
        {
            return BlacklistedPlayerIDs.Any((id) => id == steamID.m_SteamID);
        }
    }
}
