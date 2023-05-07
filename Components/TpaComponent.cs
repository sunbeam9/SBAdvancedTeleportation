using Rocket.Unturned.Chat;
using Rocket.Unturned.Player;
using SBAdvancedTeleportation.Managers;
using SBAdvancedTeleportation.Models;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace SBAdvancedTeleportation.Components
{
    public class TpaComponent : MonoBehaviour
    {
        private List<TpaRequest> Requests { get; set; }
        private IEnumerator TeleportCoroutine { get; set; }

        public void Start()
        {
            Requests = new List<TpaRequest>();
        }

        public void OnDestroy()
        {
        }

        private IEnumerator TeleportEnumerator(float delaySeconds, TpaRequest request)
        {
            WaitForSeconds delay = new WaitForSeconds(delaySeconds);
            while (true)
            {
                yield return delay;
                var sender = request.sender;
                sender.Player.teleportToPlayer(request.target.Player);
                UnturnedChat.Say(sender.CSteamID, AdvancedTeleportationPlugin.TranslateRich("PLAYER_TELEPORTED", sender.DisplayName), true);
            }
        }

        public void SendRequest(UnturnedPlayer sender, UnturnedPlayer target)
        {
            if (Requests.Any((request) => request.sender.CSteamID == sender.CSteamID && request.target.CSteamID == target.CSteamID))
            {
                UnturnedChat.Say(sender.CSteamID, AdvancedTeleportationPlugin.TranslateRich("REQUEST_ALREADY_SENT", target.DisplayName), true);
                return;
            }
            var preferences = DatabaseManager.GetPlayerPreferences(target.CSteamID);
            if (preferences.IsBlacklisted(sender.CSteamID))
            {
                UnturnedChat.Say(sender.CSteamID, AdvancedTeleportationPlugin.TranslateRich("REQUEST_BLACKLISTED"), true);
                return;
            }
            Requests.Add(new TpaRequest()
            {
                sender = sender,
                target = target,
            });
            UnturnedChat.Say(sender.CSteamID, AdvancedTeleportationPlugin.TranslateRich("REQUEST_SENT", target.DisplayName), true);
            UnturnedChat.Say(target.CSteamID, AdvancedTeleportationPlugin.TranslateRich("REQUEST_RECIEVED", sender.DisplayName), true);
            var isMemberOfSameGroup = sender.SteamPlayer().isMemberOfSameGroupAs(target.SteamPlayer());
            if (preferences.IsWhitelisted(sender.CSteamID) || (isMemberOfSameGroup && preferences.ShouldAutoAcceptGroup)) AcceptRequest(target);
        }

        public void AcceptRequest(UnturnedPlayer player)
        {
            var request = Requests.LastOrDefault((request) => request.target.CSteamID == player.CSteamID);
            if (request == null)
                UnturnedChat.Say(player.CSteamID, AdvancedTeleportationPlugin.TranslateRich("REQUESTS_NONE"), true);
            else
            {
                var sender = request.sender;
                var delay = AdvancedTeleportationPlugin.Instance.Configuration.Instance.TeleportDelay;
                UnturnedChat.Say(player.CSteamID, AdvancedTeleportationPlugin.TranslateRich("REQUEST_ACCEPTED_TARGET", sender.DisplayName), true);
                UnturnedChat.Say(sender.CSteamID, AdvancedTeleportationPlugin.TranslateRich("REQUEST_ACCEPTED_SENDER", player.DisplayName), true);
                if (delay <= 0)
                {
                    sender.Player.teleportToPlayer(player.Player);
                    UnturnedChat.Say(sender.CSteamID, AdvancedTeleportationPlugin.TranslateRich("PLAYER_TELEPORTED", sender.DisplayName), true);
                }
                else
                {
                    UnturnedChat.Say(player.CSteamID, AdvancedTeleportationPlugin.TranslateRich("REQUEST_ACCEPTED_TIMER_TARGET", delay), true);
                    UnturnedChat.Say(sender.CSteamID, AdvancedTeleportationPlugin.TranslateRich("REQUEST_ACCEPTED_TIMER_SENDER", delay), true);
                    TeleportCoroutine = TeleportEnumerator(delay, request);
                    StartCoroutine(TeleportCoroutine);
                }
            }
        }

        public void DenyRequest(UnturnedPlayer player)
        {
            var request = Requests.LastOrDefault((request) => request.target.CSteamID == player.CSteamID);
            if (request == null)
                UnturnedChat.Say(player.CSteamID, AdvancedTeleportationPlugin.TranslateRich("REQUESTS_NONE"), true);
            else
            {
                Requests.Remove(request);
                UnturnedChat.Say(player.CSteamID, AdvancedTeleportationPlugin.TranslateRich("REQUEST_DENIED_TARGET", request.sender.CSteamID), true);
                UnturnedChat.Say(request.target.CSteamID, AdvancedTeleportationPlugin.TranslateRich("REQUEST_DENIED_SENDER", player.CSteamID), true);
            }
        }

        public void CancelAllRequests(UnturnedPlayer player)
        {
            Requests.RemoveAll((request) => request.sender.CSteamID == player.CSteamID);
            UnturnedChat.Say(player.CSteamID, AdvancedTeleportationPlugin.TranslateRich("REQUESTS_CANCELED"), true);
        }

        public List<TpaRequest> GetRequests(UnturnedPlayer player)
        {
            return Requests.Where((request) => request.target.CSteamID == player.CSteamID).ToList();
        }
    }
}
