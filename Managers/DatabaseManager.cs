using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;
using System.Threading.Tasks;
using Dapper;
using Rocket.Core.Logging;
using SBAdvancedTeleportation.Models;
using Steamworks;

namespace SBAdvancedTeleportation.Managers
{
    public class DatabaseManager
    {
        public static string TableName = "SBAdvancedTeleportation";
        public static void StorePlayerPreferences(PlayerPreferences playerPreferences)
        {
            try
            {
                var result = new List<PlayerPreferences>();
                using (var connection = new SqlConnection(AdvancedTeleportationPlugin.Instance.Configuration.Instance.SqlConnectionString))
                {
                    var query = $"select * from `{TableName}`;";
                    result = connection.Query<PlayerPreferences>(query).AsList();
                }
            }
            catch (Exception e)
            {
                Logger.LogError($"[SBAdvancedTeleportation] [ERROR] DatabaseManager_StorePlayerPreferences: {e.Message}");
                Logger.LogError($"[SBAdvancedTeleportation] [ERROR] Details: {e}");
            }
        }

        public static List<PlayerPreferences> GetPlayerPreferences(CSteamID cSteamID)
        {
            try
            {
                var result = new List<PlayerPreferences>();
                using (var connection = new SqlConnection(AdvancedTeleportationPlugin.Instance.Configuration.Instance.SqlConnectionString))
                {
                    var query = $"SELECT * FROM {TableName} WHERE PlayerID = @playerID;";
                    result = connection.Query<PlayerPreferences>(query, new { playerID = cSteamID.m_SteamID}).AsList();
                }
                return result;
            }
            catch (Exception e)
            {
                Logger.LogError($"[SBAdvancedTeleportation] [ERROR] DatabaseManager_StorePlayerPreferences: {e.Message}");
                Logger.LogError($"[SBAdvancedTeleportation] [ERROR] Details: {e}");
                return new List<PlayerPreferences>();
            }
        }
    }
}
