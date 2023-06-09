﻿using System;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using MySql.Data.MySqlClient;
using Rocket.Core.Logging;
using SBAdvancedTeleportation.Enums;
using SBAdvancedTeleportation.Models;
using Steamworks;

namespace SBAdvancedTeleportation.Managers
{
    public class DatabaseManager
    {
        public static string TableName = "SBAdvancedTeleportation";

        public static async Task<bool> InitializeDatabase()
        {
            try
            {
                using (var connection = new MySqlConnection(AdvancedTeleportationPlugin.Instance.Configuration.Instance.SqlConnectionString))
                {
                    var query = $"CREATE TABLE IF NOT EXISTS {TableName} (PlayerID BIGINT(17) UNSIGNED NOT NULL, TargetID BIGINT(17) UNSIGNED NOT NULL, ListType INT(1), PRIMARY KEY (PlayerID, TargetID));";
                    await connection.QueryAsync(query);
                }
                return true;
            }
            catch (Exception e)
            {
                Logger.LogError($"[SBAdvancedTeleportation] [ERROR] DatabaseManager_InitializeDatabase: {e.Message}");
                Logger.LogError($"[SBAdvancedTeleportation] [ERROR] Details: {e}");
                return false;
            }
        }

        public static async Task<EListType> GetListType(CSteamID player, CSteamID target)
        {
            try
            {
                EListType type;
                using (var connection = new MySqlConnection(AdvancedTeleportationPlugin.Instance.Configuration.Instance.SqlConnectionString))
                {
                    var query = $"SELECT * FROM {TableName} WHERE PlayerID = @PlayerID AND TargetID = @TargetID;";
                    var result = await connection.QueryAsync<DatabaseEntry>(query, new { PlayerID = player.m_SteamID, TargetID = target.m_SteamID });
                    type = result
                        .DefaultIfEmpty(new DatabaseEntry()
                        {
                            PlayerID = player.m_SteamID,
                            TargetID = target.m_SteamID,
                            ListType = EListType.RESET,
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
                return EListType.RESET;
            }
        }

        public static async Task UpsertListType(CSteamID player, CSteamID target, EListType listType)
        {
            try
            {
                using (var connection = new MySqlConnection(AdvancedTeleportationPlugin.Instance.Configuration.Instance.SqlConnectionString))
                {
                    string query;
                    if (listType == EListType.RESET)
                        query = $"DELETE FROM {TableName} WHERE PlayerID = @PlayerID AND TargetID = @TargetID;";
                    else
                        query = $"INSERT INTO {TableName} (PlayerID, TargetID, ListType) VALUES (@PlayerID, @TargetID, @ListType) ON DUPLICATE KEY UPDATE ListType=@ListType;";
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
