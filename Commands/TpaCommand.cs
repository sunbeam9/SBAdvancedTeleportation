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
using SDG.Unturned;

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
            if (command.Length < 3)
                UnturnedChat.Say(player.CSteamID, $"Incorrect syntax: /tpa blacklist <add/remove> <player>", color: UnityEngine.Color.red);
            else
            {
                var target = UnturnedPlayer.FromName(command[2].ToLower());
                if (target == null)
                {
                    UnturnedChat.Say(player.CSteamID, AdvancedTeleportationPlugin.TranslateRich("PLAYER_NOT_FOUND", command[1]), true);
                }
                else
                {
                    var type = command[1];
                    switch(type)
                    {
                        case "a":
                        case "add":
                            break;
                        case "r":
                        case "d":
                        case "delete":
                        case "remove":
                            break;
                    }
                }
            }
        }

        private void WhitelistPlayer(UnturnedPlayer player, string[] command)
        {
            if (command.Length < 3)
                UnturnedChat.Say(player.CSteamID, $"Incorrect syntax: /tpa whitelist <add/remove> <player>", color: UnityEngine.Color.red);
            else
            {
                var target = UnturnedPlayer.FromName(command[2].ToLower());
                if (target == null)
                {
                    UnturnedChat.Say(player.CSteamID, AdvancedTeleportationPlugin.TranslateRich("PLAYER_NOT_FOUND", command[1]), true);
                }
                else
                {
                }
            }
        }

        private void CancelAllRequests(UnturnedPlayer player)
        {
            AdvancedTeleportationPlugin.Instance.TpaComponent.CancelAllRequests(player);
        }

        private void AcceptRequest(UnturnedPlayer player)
        {
            AdvancedTeleportationPlugin.Instance.TpaComponent.AcceptRequest(player);
        }

        private void DenyRequest(UnturnedPlayer player)
        {
            AdvancedTeleportationPlugin.Instance.TpaComponent.DenyRequest(player);
        }

        private void ListRequests(UnturnedPlayer player)
        {
            var requests = AdvancedTeleportationPlugin.Instance.TpaComponent.GetRequests(player);
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
                    UnturnedChat.Say(player.CSteamID, $"{i++}: {request.Sender.DisplayName}");
                }
            }
        }

        private void SendRequest(UnturnedPlayer sender, string[] command)
        {
            if (AdvancedTeleportationPlugin.Instance.HasCooldown(sender.CSteamID, out TimeSpan timeLeft))
                UnturnedChat.Say(sender.CSteamID, AdvancedTeleportationPlugin.TranslateRich("COMMAND_COOLDOWN", (int)timeLeft.TotalSeconds), true);
            else
            {
                var target = UnturnedPlayer.FromName(command[0].ToLower());
                if (target == null)
                {
                    UnturnedChat.Say(sender.CSteamID, AdvancedTeleportationPlugin.TranslateRich("PLAYER_NOT_FOUND", command[0]), true);
                }
                else
                {
                    if (AdvancedTeleportationPlugin.Instance.TpaComponent.Requests.Any((request) => request.Sender.CSteamID == sender.CSteamID && request.Target.CSteamID == target.CSteamID))
                        UnturnedChat.Say(sender.CSteamID, AdvancedTeleportationPlugin.TranslateRich("REQUEST_ALREADY_SENT", target.DisplayName), true);
                    else
                    {
                        AdvancedTeleportationPlugin.Instance.RegisterCooldown(sender.CSteamID);
                        AdvancedTeleportationPlugin.Instance.TpaComponent.SendRequest(sender, target);
                    }
                }
            }
        }
    }
}
