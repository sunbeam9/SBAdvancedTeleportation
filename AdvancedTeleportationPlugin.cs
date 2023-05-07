using Rocket.API.Collections;
using Rocket.Core.Logging;
using Rocket.Core.Plugins;
using Rocket.Unturned;
using SBAdvancedTeleportation.Components;
using SBAdvancedTeleportation.Managers;

namespace SBAdvancedTeleportation
{
    public class AdvancedTeleportationPlugin : RocketPlugin<AdvancedTeleportationConfiguration>
    {
        private static readonly string LoadUnloadMessage = @"
 ____  ____  __    ____  ____  _____  ____  ____   __   ____  ____  _____  _  _ 
(_  _)( ___)(  )  ( ___)(  _ \(  _  )(  _ \(_  _) /__\ (_  _)(_  _)(  _  )( \( )
  )(   )__)  )(__  )__)  )___/ )(_)(  )   /  )(  /(__)\  )(   _)(_  )(_)(  )  ( 
 (__) (____)(____)(____)(__)  (_____)(_)\_) (__)(__)(__)(__) (____)(_____)(_)\_)
 ____  _  _    ___  __  __  _  _    ____  ____    __    __  __                  
(  _ \( \/ )  / __)(  )(  )( \( )  (  _ \( ___)  /__\  (  \/  )                 
 ) _ < \  /   \__ \ )(__)(  )  (    ) _ < )__)  /(__)\  )    (                  
(____/ (__)   (___/(______)(_)\_)  (____/(____)(__)(__)(_/\/\_)                 
";
        public static AdvancedTeleportationPlugin Instance { get; set; }
        public static TpaComponent TpaComponent { get; set; }

        public override TranslationList DefaultTranslations => new TranslationList()
        {
            {"PLAYER_NOT_FOUND", "-=color=yellow=-Couldn't find a player named {0}.-=/color=-" },
            {"REQUEST_SENT", "-=color=yellow=-Succesfully sent a tpa request to {0}.-=/color=-" },
            {"REQUEST_RECIEVED", "-=color=yellow=-You recieved a tpa request from {0}.-=/color=-" },
            {"REQUESTS_NOT_FOUND", "-=color=yellow=-You have no tpa requests.-=/color=-" },
            {"REQUESTS_FOUND", "-=color=yellow=-You have {0} tpa requests.-=/color=-" },
            {"REQUEST_BLACKLISTED", "-=color=red=-Your tpa request was ignored.-=/color=-" },
            {"REQUEST_ACCEPTED_SENDER", "-=color=green=-{0} accepted your tpa request." },
            {"REQUEST_ACCEPTED_TARGET", "-=color=green=-Accepted {0}'s tpa request." },
            {"REQUEST_ACCEPTED_TIMER_SENDER", "You will be teleported in {0} seconds" },
            {"REQUEST_ACCEPTED_TIMER_TARGET", "They will be teleported in {0} seconds" },
            {"REQUESTS_CANCELED", "Canceled all outgoing tpa requests." },
            {"REQUESTS_NONE", "You have no tpa requests." },
            {"REQUEST_ALREADY_SENT", "You have already sent a tpa request to {0}." },
            {"REQUEST_DENIED_TARGET", "You denied {0}'s tpa request." },
            {"REQUEST_DENIED_SENDER", "{0} denied your tpa request." },
            {"PLAYER_BLACKLISTED", "Successfully blacklisted {0}." },
            {"PLAYER_WHITELISTED", "Successfully whitelisted {0}." },
        };

        public static string TranslateRich(string translationKey, params object[] placeholder)
        {
            return Instance.Translate(translationKey, placeholder).Replace(Instance.Configuration.Instance.RichLeftDelimiter, "<").Replace(Instance.Configuration.Instance.RichRightDelimiter, ">");
        }

        protected override void Load()
        {
            Logger.Log(LoadUnloadMessage);
            Instance = this;
            TpaComponent = gameObject.AddComponent<TpaComponent>();
        }

        protected override void Unload()
        {
            Logger.Log(LoadUnloadMessage);
        }
    }
}
