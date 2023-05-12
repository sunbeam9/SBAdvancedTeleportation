using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Xml;
using Dapper;
using Rocket.Core.Logging;
using SBAdvancedTeleportation.Models;
using Steamworks;

namespace SBAdvancedTeleportation.Managers
{
    public class DatabaseManager
    {
        public static string TableName = "SBAdvancedTeleportation";
        public static Dictionary<CSteamID, PlayerPreferences> preferences = new Dictionary<CSteamID, PlayerPreferences>();

        /*
        public static void StorePlayerPreferences(PlayerPreferences playerPreferences)
        {
            try
            {
                using (var connection = new SqlConnection(AdvancedTeleportationPlugin.Instance.Configuration.Instance.SqlConnectionString))
                {
                    var query = $"select * from `{TableName}`;";
                    //result = connection.Query<PlayerPreferences>(query).AsList().FirstOrDefault();
                }
            }
            catch (Exception e)
            {
                Logger.LogError($"[SBAdvancedTeleportation] [ERROR] DatabaseManager_StorePlayerPreferences: {e.Message}");
                Logger.LogError($"[SBAdvancedTeleportation] [ERROR] Details: {e}");
            }
        }

        public static PlayerPreferences GetPlayerPreferences(CSteamID cSteamID)
        {
            try
            {
                PlayerPreferences result;
                using (var connection = new SqlConnection(AdvancedTeleportationPlugin.Instance.Configuration.Instance.SqlConnectionString))
                {
                    var query = $"SELECT * FROM {TableName} WHERE PlayerID = @playerID;";
                    result = connection.Query<PlayerPreferences>(query, new { playerID = cSteamID.m_SteamID}).FirstOrDefault();
                }
                return result;
            }
            catch (Exception e)
            {
                Logger.LogError($"[SBAdvancedTeleportation] [ERROR] DatabaseManager_StorePlayerPreferences: {e.Message}");
                Logger.LogError($"[SBAdvancedTeleportation] [ERROR] Details: {e}");
                return null;
            }
        }
        */

        public static bool AddWhitelist(CSteamID player, CSteamID target)
        {
            if (MockDatabase.SBAdvancedTeleportation_Whitelist.Any((kvp) => kvp.Key == player && kvp.Value == target))
                return false;
            else
            {
                MockDatabase.SBAdvancedTeleportation_Whitelist.Add();
            }
        }
    }
}
