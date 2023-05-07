using Rocket.Unturned.Player;

namespace SBAdvancedTeleportation.Models
{
    public class TpaRequest
    {
        public UnturnedPlayer sender { get; set; }
        public UnturnedPlayer target { get; set; }
    }
}
