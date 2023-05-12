using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Rocket.Unturned.Chat;
using Rocket.Unturned.Events;
using Rocket.Unturned.Player;
using SBAdvancedTeleportation.Models;
using SDG.Unturned;
using UnityEngine;
using Logger = Rocket.Core.Logging.Logger;

namespace SBAdvancedTeleportation.Managers
{
    public class TpaManager
    {
        public List<TpaRequest> Requests { get; set; }

        public void Instantiate()
        {
            Requests = new List<TpaRequest>();
            UnturnedPlayerEvents.OnPlayerDeath += OnPlayerDeath;
        }

        private void OnPlayerDeath(UnturnedPlayer player, EDeathCause cause, ELimb limb, Steamworks.CSteamID murderer)
        {
            Requests.RemoveAll((request) =>
            {
                var shouldRemove = (request.Target.CSteamID == player.CSteamID || request.Sender.CSteamID == player.CSteamID) && request.Coroutine != null;
                if (shouldRemove)
                {
                    AdvancedTeleportationPlugin.Instance.StopCoroutine(request.Coroutine);
                    UnturnedChat.Say(request.Sender.CSteamID, AdvancedTeleportationPlugin.TranslateRich("REQUEST_CANCELED"), true);
                    UnturnedChat.Say(request.Target.CSteamID, AdvancedTeleportationPlugin.TranslateRich("REQUEST_CANCELED"), true);
                }
                return shouldRemove;
            });
        }

        public void Destroy()
        {
            UnturnedPlayerEvents.OnPlayerDeath -= OnPlayerDeath;
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

        public void SendRequest(UnturnedPlayer sender, UnturnedPlayer target)
        {
            var preferences = DatabaseManager.GetPlayerPreferences(target.CSteamID);
            if (preferences.IsBlacklisted(sender.CSteamID))
            {
                UnturnedChat.Say(sender.CSteamID, AdvancedTeleportationPlugin.TranslateRich("REQUEST_BLACKLISTED"), true);
                return;
            }
            Requests.Add(new TpaRequest()
            {
                Sender = sender,
                Target = target,
            });
            UnturnedChat.Say(sender.CSteamID, AdvancedTeleportationPlugin.TranslateRich("REQUEST_SENT", target.DisplayName), true);
            UnturnedChat.Say(target.CSteamID, AdvancedTeleportationPlugin.TranslateRich("REQUEST_RECIEVED", sender.DisplayName), true);
            var isMemberOfSameGroup = sender.SteamPlayer().isMemberOfSameGroupAs(target.SteamPlayer());
            if (preferences.IsWhitelisted(sender.CSteamID) || (isMemberOfSameGroup && preferences.ShouldAutoAcceptGroup)) AcceptRequest(target);
        }

        public void AcceptRequest(UnturnedPlayer player)
        {
            var request = Requests.LastOrDefault((request) => request.Target.CSteamID == player.CSteamID && request.Coroutine == null);
            if (request == null)
                UnturnedChat.Say(player.CSteamID, AdvancedTeleportationPlugin.TranslateRich("REQUESTS_NONE"), true);
            else
            {
                var sender = request.Sender;
                var delay = AdvancedTeleportationPlugin.Instance.Configuration.Instance.TeleportDelay;
                UnturnedChat.Say(player.CSteamID, AdvancedTeleportationPlugin.TranslateRich("REQUEST_ACCEPTED_TARGET", sender.DisplayName), true);
                UnturnedChat.Say(sender.CSteamID, AdvancedTeleportationPlugin.TranslateRich("REQUEST_ACCEPTED_SENDER", player.DisplayName), true);
                if (delay <= 0)
                {
                    TeleportPlayerToPlayerWithMessage(sender, request.Target);
                    Requests.Remove(request);
                }
                else
                {
                    UnturnedChat.Say(player.CSteamID, AdvancedTeleportationPlugin.TranslateRich("REQUEST_ACCEPTED_TIMER_TARGET", delay), true);
                    UnturnedChat.Say(sender.CSteamID, AdvancedTeleportationPlugin.TranslateRich("REQUEST_ACCEPTED_TIMER_SENDER", delay), true);
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
                UnturnedChat.Say(sender.CSteamID, AdvancedTeleportationPlugin.TranslateRich("PLAYER_TELEPORTED_SENDER", target.DisplayName), true);
                UnturnedChat.Say(target.CSteamID, AdvancedTeleportationPlugin.TranslateRich("PLAYER_TELEPORTED_TARGET", sender.DisplayName), true);
            }
            else
            {
                UnturnedChat.Say(sender.CSteamID, AdvancedTeleportationPlugin.TranslateRich("PLAYER_TELEPORTATION_FAILED_SENDER", target.DisplayName), true);
                UnturnedChat.Say(target.CSteamID, AdvancedTeleportationPlugin.TranslateRich("PLAYER_TELEPORTATION_FAILED_TARGET", sender.DisplayName), true);
            }
        }

        public void DenyRequest(UnturnedPlayer player)
        {
            var request = Requests.LastOrDefault((request) => request.Target.CSteamID == player.CSteamID);
            if (request == null)
                UnturnedChat.Say(player.CSteamID, AdvancedTeleportationPlugin.TranslateRich("REQUESTS_NONE"), true);
            else
            {
                Requests.Remove(request);
                UnturnedChat.Say(player.CSteamID, AdvancedTeleportationPlugin.TranslateRich("REQUEST_DENIED_TARGET", request.Sender.DisplayName), true);
                UnturnedChat.Say(request.Target.CSteamID, AdvancedTeleportationPlugin.TranslateRich("REQUEST_DENIED_SENDER", player.DisplayName), true);
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
                    UnturnedChat.Say(request.Target.CSteamID, AdvancedTeleportationPlugin.TranslateRich("REQUEST_CANCELED"), true);
                }
                return match;
            });
            UnturnedChat.Say(player.CSteamID, AdvancedTeleportationPlugin.TranslateRich("REQUESTS_CANCELED"), true);
        }

        public List<TpaRequest> GetRequests(UnturnedPlayer player)
        {
            return Requests.Where((request) => request.Target.CSteamID == player.CSteamID).ToList();
        }
    }
}
