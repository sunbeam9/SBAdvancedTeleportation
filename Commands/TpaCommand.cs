using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Rocket.API;
using Rocket.Core.Logging;
using Rocket.Core.Utils;
using Rocket.Unturned.Chat;
using Rocket.Unturned.Player;
using SBAdvancedTeleportation.Enums;
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

        public string Syntax => "<accept/cancel/deny/list/whitelist/blacklist/reset> (player)";

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
                case "b":
                case "blacklist":
                case "r":
                case "reset":
                    Task.Run(async () =>
                    {
                        await WhitelistBlacklistResetPlayer(player, command);
                    });
                    break;
                default:
                    Task.Run(async () =>
                    {
                        await SendRequest(player, command);
                    });
                    break;
            }
        }

        private async Task WhitelistBlacklistResetPlayer(UnturnedPlayer player, string[] command)
        {
            if (command.Length < 2)
            {
                AdvancedTeleportationPlugin.Say(player.CSteamID, $"<color=red>Incorrect syntax: /tpa {command[0]} <player></color=red>", true);
                return;
            }
            var target = UnturnedPlayer.FromName(command[1]);
            if (target == null)
            {
                AdvancedTeleportationPlugin.Say(player.CSteamID, AdvancedTeleportationPlugin.TranslateRich("PLAYER_NOT_FOUND", command[1]), true);
                return;
            }
            EListType listType;
            switch (command[0])
            {
                case "w":
                case "whitelist":
                    listType = EListType.WHITELIST;
                    break;
                case "b":
                case "blacklist":
                    listType = EListType.BLACKLIST;
                    break;
                case "r":
                case "reset":
                default:
                    listType = EListType.NONE;
                    break;
            }
            await DatabaseManager.UpsertListType(player.CSteamID, target.CSteamID, listType);
            AdvancedTeleportationPlugin.Say(player.CSteamID, AdvancedTeleportationPlugin.TranslateRich("PLAYER_LIST_UPDATED", listType.ToString().ToLower(), target.DisplayName), true);
        }

        private void CancelAllRequests(UnturnedPlayer player)
        {
            AdvancedTeleportationPlugin.Instance.TpaManager.CancelAllRequests(player);
        }

        private void AcceptRequest(UnturnedPlayer player)
        {
            AdvancedTeleportationPlugin.Instance.TpaManager.AcceptRequest(player);
        }

        private void DenyRequest(UnturnedPlayer player)
        {
            AdvancedTeleportationPlugin.Instance.TpaManager.DenyRequest(player);
        }

        private void ListRequests(UnturnedPlayer player)
        {
            var requests = AdvancedTeleportationPlugin.Instance.TpaManager.GetRequests(player);
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

        private async Task SendRequest(UnturnedPlayer sender, string[] command)
        {
            if (AdvancedTeleportationPlugin.Instance.HasCooldown(sender.CSteamID, out TimeSpan timeLeft))
                AdvancedTeleportationPlugin.Say(sender.CSteamID, AdvancedTeleportationPlugin.TranslateRich("COMMAND_COOLDOWN", (int)timeLeft.TotalSeconds), true);
            else
            {
                var target = UnturnedPlayer.FromName(command[0].ToLower());
                if (target == null)
                {
                    AdvancedTeleportationPlugin.Say(sender.CSteamID, AdvancedTeleportationPlugin.TranslateRich("PLAYER_NOT_FOUND", command[0]), true);
                }
                else
                {
                    if (AdvancedTeleportationPlugin.Instance.TpaManager.Requests.Any((request) => request.Sender.CSteamID == sender.CSteamID && request.Target.CSteamID == target.CSteamID))
                        AdvancedTeleportationPlugin.Say(sender.CSteamID, AdvancedTeleportationPlugin.TranslateRich("REQUEST_ALREADY_SENT", target.DisplayName), true);
                    else
                    {
                        AdvancedTeleportationPlugin.Instance.RegisterCooldown(sender.CSteamID);
                        await AdvancedTeleportationPlugin.Instance.TpaManager.SendRequest(sender, target);
                    }
                }
            }
        }
    }
}
