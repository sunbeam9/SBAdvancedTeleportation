using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Rocket.Core.Utils;
using Rocket.Unturned.Player;
using SBAdvancedTeleportation.Models;
using SDG.Unturned;
using Steamworks;
using UnityEngine;
using Logger = Rocket.Core.Logging.Logger;

namespace SBAdvancedTeleportation.Managers
{
    public class TpaManager
    {
        public TpaManager()
        {
            Requests = new List<TpaRequest>();
        }

        public List<TpaRequest> Requests { get; set; }
        public Dictionary<CSteamID, Coroutine> PvpTimers { get; set; }

        public void OnPlayerDeath(UnturnedPlayer player, EDeathCause cause, ELimb limb, Steamworks.CSteamID murderer)
        {
            Requests.RemoveAll((request) =>
            {
                var shouldRemove = (request.Target.CSteamID == player.CSteamID || request.Sender.CSteamID == player.CSteamID) && request.Coroutine != null;
                if (shouldRemove)
                {
                    AdvancedTeleportationPlugin.Instance.StopCoroutine(request.Coroutine);
                    AdvancedTeleportationPlugin.Say(request.Sender.CSteamID, AdvancedTeleportationPlugin.TranslateRich("REQUEST_CANCELED"));
                    AdvancedTeleportationPlugin.Say(request.Target.CSteamID, AdvancedTeleportationPlugin.TranslateRich("REQUEST_CANCELED"));
                }
                return shouldRemove;
            });
        }

        private IEnumerator TeleportEnumerator(float delaySeconds, TpaRequest request)
        {
            WaitForSeconds delay = new WaitForSeconds(delaySeconds);
            yield return delay;
            TeleportPlayerToPlayerWithMessage(request.Sender, request.Target);
            Logger.Log("Removing Request");
            Requests.Remove(request);
            Logger.Log("Request Removed");
        }

        public async Task SendRequest(UnturnedPlayer sender, UnturnedPlayer target)
        {
            var type = await DatabaseManager.GetListType(target.CSteamID, sender.CSteamID);
            if (type == Enums.EListType.BLACKLIST)
            {
                AdvancedTeleportationPlugin.Say(sender.CSteamID, AdvancedTeleportationPlugin.TranslateRich("REQUEST_BLACKLISTED"));
                return;
            }
            Requests.Add(new TpaRequest()
            {
                Sender = sender,
                Target = target,
            });
            AdvancedTeleportationPlugin.Say(sender.CSteamID, AdvancedTeleportationPlugin.TranslateRich("REQUEST_SENT", target.DisplayName));
            AdvancedTeleportationPlugin.Say(target.CSteamID, AdvancedTeleportationPlugin.TranslateRich("REQUEST_RECIEVED", sender.DisplayName));
            var isMemberOfSameGroup = sender.SteamPlayer().isMemberOfSameGroupAs(target.SteamPlayer());
            if (type == Enums.EListType.WHITELIST || isMemberOfSameGroup)
            {
                TaskDispatcher.QueueOnMainThread(() =>
                {
                    AcceptRequest(target);
                });
            };
        }

        public void AcceptRequest(UnturnedPlayer player)
        {
            var request = Requests.LastOrDefault((request) => request.Target.CSteamID == player.CSteamID && request.Coroutine == null);
            if (request == null)
                AdvancedTeleportationPlugin.Say(player.CSteamID, AdvancedTeleportationPlugin.TranslateRich("REQUESTS_NONE"));
            else
            {
                var sender = request.Sender;
                var delay = AdvancedTeleportationPlugin.Instance.Configuration.Instance.TeleportDelay;
                AdvancedTeleportationPlugin.Say(player.CSteamID, AdvancedTeleportationPlugin.TranslateRich("REQUEST_ACCEPTED_TARGET", sender.DisplayName));
                AdvancedTeleportationPlugin.Say(sender.CSteamID, AdvancedTeleportationPlugin.TranslateRich("REQUEST_ACCEPTED_SENDER", player.DisplayName));
                if (delay <= 0)
                {
                    TeleportPlayerToPlayerWithMessage(sender, request.Target);
                    Requests.Remove(request);
                }
                else
                {
                    Logger.Log("Making Enumerator");
                    var Enumerator = TeleportEnumerator(delay, request);
                    Logger.Log("Starting Coroutine and setting request");
                    request.Coroutine = AdvancedTeleportationPlugin.Instance.StartCoroutine(Enumerator);
                    Logger.Log("Done");
                }
            }
        }

        public bool TeleportPlayerToPlayer(UnturnedPlayer sender, UnturnedPlayer target)
        {
            if (target.IsInVehicle)
                return VehicleManager.ServerForcePassengerIntoVehicle(sender.Player, target.CurrentVehicle);
            else
                return sender.Player.teleportToPlayer(target.Player);
        }

        public void TeleportPlayerToPlayerWithMessage(UnturnedPlayer sender, UnturnedPlayer target)
        {
            var successful = TeleportPlayerToPlayer(sender, target);
            if (successful)
            {
                AdvancedTeleportationPlugin.Say(sender.CSteamID, AdvancedTeleportationPlugin.TranslateRich("PLAYER_TELEPORTED_SENDER", target.DisplayName));
                AdvancedTeleportationPlugin.Say(target.CSteamID, AdvancedTeleportationPlugin.TranslateRich("PLAYER_TELEPORTED_TARGET", sender.DisplayName));
            }
            else
            {
                AdvancedTeleportationPlugin.Say(sender.CSteamID, AdvancedTeleportationPlugin.TranslateRich("PLAYER_TELEPORTATION_FAILED_SENDER", target.DisplayName));
                AdvancedTeleportationPlugin.Say(target.CSteamID, AdvancedTeleportationPlugin.TranslateRich("PLAYER_TELEPORTATION_FAILED_TARGET", sender.DisplayName));
            }
        }

        public void DenyRequest(UnturnedPlayer player)
        {
            var request = Requests.LastOrDefault((request) => request.Target.CSteamID == player.CSteamID);
            if (request == null)
                AdvancedTeleportationPlugin.Say(player.CSteamID, AdvancedTeleportationPlugin.TranslateRich("REQUESTS_NONE"), true);
            else
            {
                Requests.Remove(request);
                AdvancedTeleportationPlugin.Say(player.CSteamID, AdvancedTeleportationPlugin.TranslateRich("REQUEST_DENIED_TARGET", request.Sender.DisplayName));
                AdvancedTeleportationPlugin.Say(request.Target.CSteamID, AdvancedTeleportationPlugin.TranslateRich("REQUEST_DENIED_SENDER", player.DisplayName));
            }
        }

        public void CancelAllRequests(UnturnedPlayer player)
        {
            Requests.RemoveAll((request) =>
            {
                var match = request.Sender.CSteamID == player.CSteamID;
                if (match && request.Coroutine != null)
                {
                    AdvancedTeleportationPlugin.Instance.StopCoroutine(request.Coroutine);
                    AdvancedTeleportationPlugin.Say(request.Target.CSteamID, AdvancedTeleportationPlugin.TranslateRich("REQUEST_CANCELED"));
                }
                return match;
            });
            AdvancedTeleportationPlugin.Say(player.CSteamID, AdvancedTeleportationPlugin.TranslateRich("REQUESTS_CANCELED"));
        }

        public List<TpaRequest> GetRequests(UnturnedPlayer player)
        {
            return Requests.Where((request) => request.Target.CSteamID == player.CSteamID).ToList();
        }

        public void OnPlayerDamageRequested(ref DamagePlayerParameters parameters, ref bool shouldAllow)
        {
            throw new NotImplementedException();
        }
    }
}
