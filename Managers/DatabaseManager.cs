using System;
using System.Data.SqlClient;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Dapper;
using Rocket.Core.Logging;
using SBAdvancedTeleportation.Enums;
using SBAdvancedTeleportation.Models;
using Steamworks;

namespace SBAdvancedTeleportation.Managers
{
    public class DatabaseManager
    {
        public static string TableName = "SBAdvancedTeleportation";

        public static async Task InitializeDatabase()
        {
            try
            {
                using (var connection = new SqlConnection(AdvancedTeleportationPlugin.Instance.Configuration.Instance.SqlConnectionString))
                {
                    var query = $"CREATE TABLE IF NOT EXISTS {TableName};";
                    await connection.QueryAsync(query);
                }
            }
            catch (Exception e)
            {
                Logger.LogError($"[SBAdvancedTeleportation] [ERROR] DatabaseManager_InitializeDatabase: {e.Message}");
                Logger.LogError($"[SBAdvancedTeleportation] [ERROR] Details: {e}");
            }
        }

        public static async Task<EListType> GetListType(CSteamID player, CSteamID target)
        {
            try
            {
                EListType type;
                using (var connection = new SqlConnection(AdvancedTeleportationPlugin.Instance.Configuration.Instance.SqlConnectionString))
                {
                    var query = $"SELECT * FROM {TableName} WHERE PlayerID = @playerID;";
                    var result = await connection.QueryAsync<DatabaseEntry>(query, new { PlayerID = player.m_SteamID, TargetID = target.m_SteamID });
                    type = result
                        .DefaultIfEmpty(new DatabaseEntry()
                        {
                            PlayerID = player.m_SteamID,
                            TargetID = target.m_SteamID,
                            ListType = EListType.NONE,
                        })
                        .First()
                        .ListType;
                }
                return type;
            }
            catch (Exception e)
            {
                Logger.LogError($"[SBAdvancedTeleportation] [ERROR] DatabaseManager_GetListType: {e.Message}");
                Logger.LogError($"[SBAdvancedTeleportation] [ERROR] Details: {e}");
                return EListType.NONE;
            }
        }

        public static async Task UpsertListType(CSteamID player, CSteamID target, EListType listType)
        {
            try
            {
                using (var connection = new SqlConnection(AdvancedTeleportationPlugin.Instance.Configuration.Instance.SqlConnectionString))
                {
                    var query = $"INSERT INTO {TableName} (PlayerID, TargetID, ListType) VALUES (@PlayerID, @TargetID, @ListType) ON DUPLICATE KEY UPDATE ListType=@ListType;";
                    await connection.QueryAsync<DatabaseEntry>(query, new { PlayerID = player.m_SteamID, TargetID = target.m_SteamID, ListType = listType });
                }
            }
            catch (Exception e)
            {
                Logger.LogError($"[SBAdvancedTeleportation] [ERROR] DatabaseManager_UpsertListType: {e.Message}");
                Logger.LogError($"[SBAdvancedTeleportation] [ERROR] Details: {e}");
            }
        }
    }
}
