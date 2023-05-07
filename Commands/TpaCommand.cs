using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Rocket.API;
using Rocket.Core.Logging;
using Rocket.Unturned.Chat;
using Rocket.Unturned.Player;
using SBAdvancedTeleportation.Managers;
using SBAdvancedTeleportation.Models;

namespace SBAdvancedTeleportation.Commands
{
    public class TpaCommand : IRocketCommand
    {
        public AllowedCaller AllowedCaller => AllowedCaller.Player;

        public string Name => "tpa";

        public string Help => "Sends/Accepts/Cancels/Denies a teleportation request to a player";

        public string Syntax => "<accept/cancel/deny/list/whitelist/blacklist> (player)";

        public List<string> Aliases => new List<string>();

        public List<string> Permissions => new List<string>()
        {
            "teleportation"
        };

        public void Execute(IRocketPlayer caller, string[] command)
        {
            if (caller is not UnturnedPlayer player) return;
            if (command.Length < 1)
            {
                UnturnedChat.Say(player.CSteamID, $"Incorrect syntax: /tpa {Syntax}", color: UnityEngine.Color.red);
                return;
            }
            var type = command[0];
            Logger.Log($"type: {type}");
            switch (type)
            {
                case "a":
                case "accept":
                    AcceptRequest(player);
                    break;
                case "c":
                case "cancel":
                    CancelAllRequests(player);
                    break;
                case "d":
                case "deny":
                    DenyRequest(player);
                    break;
                case "list":
                    ListRequests(player);
                    break;
                case "w":
                case "whitelist":
                    WhitelistPlayer(player, command);
                    break;
                case "b":
                case "blacklist":
                    BlacklistPlayer(player, command);
                    break;
                default:
                    SendRequest(player, command);
                    break;
            }
        }

        private void BlacklistPlayer(UnturnedPlayer player, string[] command)
        {
            if (command.Length < 2)
                UnturnedChat.Say(player.CSteamID, $"Incorrect syntax: /tpa {Syntax}", color: UnityEngine.Color.red);
            else
            {
                var target = UnturnedPlayer.FromName(command[0].ToLower());
                if (target == null)
                {
                    UnturnedChat.Say(player.CSteamID, AdvancedTeleportationPlugin.TranslateRich("PLAYER_NOT_FOUND", command[1]), true);
                }
                else
                {

                    var preferences = DatabaseManager.GetPlayerPreferences(player.CSteamID);
                    preferences.BlacklistedPlayerIDs.Add(target.CSteamID.m_SteamID);
                    DatabaseManager.StorePlayerPreferences(preferences);
                    UnturnedChat.Say(player.CSteamID, AdvancedTeleportationPlugin.TranslateRich("PLAYER_BLACKLISTED", target.DisplayName));
                }   
            }
        }

        private void WhitelistPlayer(UnturnedPlayer player, string[] command)
        {
            if (command.Length < 2)
                UnturnedChat.Say(player.CSteamID, $"Incorrect syntax: /tpa {Syntax}", color: UnityEngine.Color.red);
            else
            {
                var target = UnturnedPlayer.FromName(command[0].ToLower());
                if (target == null)
                {
                    UnturnedChat.Say(player.CSteamID, AdvancedTeleportationPlugin.TranslateRich("PLAYER_NOT_FOUND", command[1]), true);
                }
                else
                {

                    var preferences = DatabaseManager.GetPlayerPreferences(player.CSteamID);
                    preferences.WhitelistedPlayerIDs.Add(target.CSteamID.m_SteamID);
                    DatabaseManager.StorePlayerPreferences(preferences);
                    UnturnedChat.Say(player.CSteamID, AdvancedTeleportationPlugin.TranslateRich("PLAYER_WHITELISTED", target.DisplayName));
                }
            }
        }

        private void CancelAllRequests(UnturnedPlayer player)
        {
            AdvancedTeleportationPlugin.TpaComponent.CancelAllRequests(player);
        }

        private void AcceptRequest(UnturnedPlayer player)
        {
            AdvancedTeleportationPlugin.TpaComponent.AcceptRequest(player);
        }

        private void DenyRequest(UnturnedPlayer player)
        {
            AdvancedTeleportationPlugin.TpaComponent.DenyRequest(player);
        }

        private void ListRequests(UnturnedPlayer player)
        {
            var requests = AdvancedTeleportationPlugin.TpaComponent.GetRequests(player);
            if (requests == null)
            {
                UnturnedChat.Say(player.CSteamID, AdvancedTeleportationPlugin.TranslateRich("REQUESTS_NOT_FOUND"), true);
            }
            else
            {
                UnturnedChat.Say(player.CSteamID, AdvancedTeleportationPlugin.TranslateRich("REQUESTS_FOUND", requests.Count), true);
                int i = 0;
                foreach (TpaRequest request in requests)
                {
                    UnturnedChat.Say(player.CSteamID, $"{i++}: {request.sender.DisplayName}");
                }
            }
        }

        private void SendRequest(UnturnedPlayer sender, string[] command)
        {
            var target = UnturnedPlayer.FromName(command[0].ToLower());
            if (target == null)
            {
                UnturnedChat.Say(sender.CSteamID, AdvancedTeleportationPlugin.TranslateRich("PLAYER_NOT_FOUND", command[0]), true);
            }
            else
            {
                AdvancedTeleportationPlugin.TpaComponent.SendRequest(sender, target);
            }
        }
    }
}
