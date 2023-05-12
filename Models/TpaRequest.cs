using Rocket.Unturned.Player;
using UnityEngine;

namespace SBAdvancedTeleportation.Models
{
    public class TpaRequest
    {
        public UnturnedPlayer Sender { get; set; }
        public UnturnedPlayer Target { get; set; }
        public Coroutine Coroutine { get; set; }
    }
}
