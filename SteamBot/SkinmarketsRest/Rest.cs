using System;
using System.Net;
using System.Runtime.Remoting.Messaging;
using System.ServiceModel;
using System.ServiceModel.Activation;
using System.ServiceModel.Web;
using ExampleBot.SkinmarketsHandler;
using SteamBot;
using SteamBot.SkinmarketsHandler;
using SteamTrade.TradeOffer;

namespace ExampleBot
{
    public static class Routing
    {        
        public const string SendRequestRoute = "/sendRequest/{weapon}/{user}/{token}"; //tested
        public const string SendOfferRoute = "/sendOffer/{classId}/{instanceId}/{user}/{token}"; //tested
        public const string GetRequestStatusRoute = "/status/{requestId}"; //tested
        public const string GetBotInvRoute = "/inventory/"; //tested
        public const string GetBotDisplayName = "/name/";
        public const string Route = "/whatsup";
        public const string HealthRoute = "/";
        public const string HealthRoute2 = "";
        public const string CheckRequestRoute = "/request/{requestId}";
        public const string CheckRequestsRoute = "/requests";
        public const string GetOffersRoute = "/offers";
    }


    [ServiceContract(Name = "RESTDemoServices")]
    public interface IRESTDemoServices
    {
        [OperationContract]
        [WebGet(UriTemplate = Routing.SendRequestRoute, BodyStyle = WebMessageBodyStyle.Bare)]
        ulong SendRequest(string weapon, string user, string token);

        [OperationContract]
        [WebGet(UriTemplate = Routing.SendOfferRoute, BodyStyle = WebMessageBodyStyle.Bare)]
        ulong SendOffer(string classId, string instanceId, string user, string token);
        
        [OperationContract]
        [WebGet(UriTemplate = Routing.GetRequestStatusRoute, BodyStyle = WebMessageBodyStyle.Bare)]
        string GetRequestStatus(string requestId);

        [OperationContract]
        [WebGet(UriTemplate = Routing.CheckRequestRoute, BodyStyle = WebMessageBodyStyle.Bare)]
        string CheckRequestStatus(string requestId);

        [OperationContract]
        [WebGet(UriTemplate = Routing.GetBotInvRoute, BodyStyle = WebMessageBodyStyle.Bare)]
        string GetBotInventory();

        [OperationContract]
        [WebGet(UriTemplate = Routing.Route, BodyStyle = WebMessageBodyStyle.Bare)]
        string GetActualStatus();

        [OperationContract] 
        [WebGet(UriTemplate = Routing.HealthRoute, BodyStyle = WebMessageBodyStyle.Bare)]
        HttpStatusCode HealthCheck();

        [OperationContract] 
        [WebGet(UriTemplate = Routing.HealthRoute2, BodyStyle = WebMessageBodyStyle.Bare)]
        HttpStatusCode HealthCheck2();

        [OperationContract]
        [WebGet(UriTemplate = Routing.GetBotDisplayName, BodyStyle = WebMessageBodyStyle.Bare)]
        string GetDisplayName();
        
        [OperationContract]
        [WebGet(UriTemplate = Routing.CheckRequestsRoute, BodyStyle = WebMessageBodyStyle.Bare)]
        HttpStatusCode CheckOffers();

        [OperationContract]
        [WebGet(UriTemplate = Routing.GetOffersRoute, BodyStyle = WebMessageBodyStyle.Bare)]
        string getAllOffers();
        
        

        void AddBot(Bot bot);

    }


    [ServiceBehavior(
        InstanceContextMode = InstanceContextMode.Single,
        ConcurrencyMode = ConcurrencyMode.Single,
        IncludeExceptionDetailInFaults = true
    )]
    [AspNetCompatibilityRequirements(RequirementsMode = AspNetCompatibilityRequirementsMode.Allowed)]
    public class RestDemoServices:IRESTDemoServices
    {
        private SkinMarketsUserHandler botOfferResource;
        //private TradeOfferUserHandler simple;
        public RestClient _restClient;
        private RequestQueue queue = new RequestQueue();
        private long request;
//        private Log mainLog;
        private Bot bot;

        private TradeOffer tradeOffers;
        
        public RestDemoServices()
        {
            _restClient = new RestClient();
            Service.GetBackendUrl();
        }

        public RestDemoServices(UserHandler bot)
        {
            _restClient = new RestClient();
            Service.GetBackendUrl();
            botOfferResource = (SkinMarketsUserHandler) bot;
            //simple = (TradeOfferUserHandler) bot;
            botOfferResource.setDemoService(this);
        }

        public ulong SendRequest(string weapon, string user, string token)
        {
            ulong reqId = botOfferResource.RequestWeaponFromUser(weapon, user, token);
            request = queue.addRequest(reqId);
//            queue.ChangeStatus(request,success ? "Offer successfull" : "Failed to send offer");
            return reqId;
        }

        public ulong SendOffer(string classId, string instanceId, string user, string token)
        {
            
            ulong reqId = botOfferResource.SendOfferToUser(classId,instanceId, user, token);
            request = queue.addRequest(reqId);
//            queue.ChangeStatus(request,success ? "Offer successfull" : "Failed to send offer");

            return reqId;
        }

        public string GetActualStatus()
        {
            Console.WriteLine("Status abgefragt" + bot.SteamClient.SteamID);
            return "Ist verfügbar";
        }

        public HttpStatusCode HealthCheck()
        {
//            mainLog.Success("Healthcheck");
            return HttpStatusCode.OK;
        }

        public HttpStatusCode HealthCheck2()
        {
            return HealthCheck();
        }

        public string GetDisplayName()
        {
            return bot.DisplayName;
        }

        public string CheckRequestStatus(string requestId)
        {
            return GetRequestStatus(requestId);
        }
        
        public string GetRequestStatus(string requestId)
        {
            /*
            var id = Convert.ToUInt64(requestId);
            if (id > 0)
            {
                var offer = tradeOffers.GetTradeOffer(id);
                if (offer != null && offer.Offer != null)
                {
                    tradeOffers.AddPendingTradeOfferToList(id);
                    var state = offer.Offer.State;
                    return state.ToString();
                }
            }
                //queue.GetStatus(id);
                */
            return "Not found";
            
        }

        public string GetBotInventory()
        {               
            
            var inv = botOfferResource.GetBotInventory(bot.SteamClient.SteamID.ToString());
            if (inv == null)
            {
                return "Bot isn't up or has no inventory.";
            }
            foreach (var item in inv.items)
            {
                // if you need info about the item, such as name, etc, use GetItemDescription
                var description = inv.getDescription(item.Key);
                var name = description.name;
                break;
            }
            return inv.ToString(); // TODO: transform inventory to proper json output 
            
        }
        
        public void AddBot(Bot bot)
        {
            this.bot = bot;
        }

        public void addToReqQueue(ulong id)
        {
            queue.addRequest(id);
        }

        public void SetReqStatus(TradeOffer tradeOffer, string success)
        {
            
            queue.ChangeStatus(tradeOffer.TradeOfferId,success);
            _restClient.SendRequestUpdate(tradeOffer, success);
        }

        public HttpStatusCode CheckOffers()
        {
            if (checkAllOffers())
            {
                return HttpStatusCode.OK;
            }
            return HttpStatusCode.NoContent;

        }


        public string getAllOffers()
        {
            /*
            var ids = "";
            var first = true;
            tradeOffers = bot.TradeOffers;
            if (tradeOffers != null)
            {
                var offers = tradeOffers.GetTradeOffers();
                foreach (var offer in offers)
                {
                    if (first)
                    {
                        ids = offer.Id + ":" + offer.State;
                        first = false;
                    }
                    else
                    {
                        ids = ids + " , " + offer.Id + ":" + offer.State;
                        
                    }
                }
                return ids;
            }
            */
            return "No Offers";
            
        }

        public bool checkAllOffers()
        {
            /*
            tradeOffers = bot.TradeOffers;
            if (tradeOffers != null)
            {
                var offers = tradeOffers.GetTradeOffers();
                foreach (var offer in offers)
                {
                        //mainLog.Success("Change status for offer " + offer.Id + "!");
                        tradeOffers.AddPendingTradeOfferToList(offer.Id);
                }

                return true;
            }
            else
            {
//                mainLog.Warn("No tradeOffers!");
                return false;
            }
            */
            return false;
        }

        public void RedoTrade(TradeOffer tradeOffer)
        {
  //          mainLog.Warn("Should Redo trade.");
            //var tradeId = botOfferResource.RedoTrade(tradeOffer.Id);
            //queue.addRequest(tradeId);
        }
    }
}