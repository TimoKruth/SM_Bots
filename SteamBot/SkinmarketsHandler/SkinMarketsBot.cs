using SteamBot;
using SteamKit2.GC.Dota.Internal;

namespace ExampleBot.SkinmarketsHandler
{
    public class SkinMarketsBot : Bot
    {
        public SkinMarketsBot(Configuration.BotInfo config, string apiKey, UserHandlerCreator handlerCreator, bool debug = false, bool process = false) : base(config, apiKey, handlerCreator, debug, process)
        {
            
        }
    }
}