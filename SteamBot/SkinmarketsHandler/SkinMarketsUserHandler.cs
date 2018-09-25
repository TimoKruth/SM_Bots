using System;
using System.Collections;
using System.Collections.Generic;
using System.ServiceModel.Web;
using ExampleBot;
using SteamKit2;
using SteamTrade;
using SteamTrade.TradeOffer;

namespace SteamBot.SkinmarketsHandler
{
    public class SkinMarketsUserHandler : TradeOfferUserHandler
    {
        private ulong steamid;
        private ulong weaponid;
        private ulong classId;
        private ulong instanceId;
        private SteamID steamId;
        private Dictionary<ulong,DoneTrade> trades = new Dictionary<ulong, DoneTrade>();
        private GenericInventory genInv;
        private IEnumerable<long> contextid = new long[2];
        private readonly GenericInventory OtherSteamInventory;

        
        private static bool _shouldSendActivationNote = true;
        private static int _sencActivationRefreshRate = 60000; // Send Call every Minute
        private static int _checkOfferRate= 600000; // Send Call every Minute
        private static bool _shouldCheckOffers = true;
        private static RestDemoServices DemoServices;
        private static WebServiceHost _serviceHost;
        
        

        public SkinMarketsUserHandler(Bot bot, SteamID sid) : base(bot, sid)
        {
            genInv = new GenericInventory(Bot.SteamWeb);
            OtherSteamInventory = new GenericInventory(SteamWeb);
        }


        private class DoneTrade
        {
            private TradeOffer trade;
            private string userToken;

            public DoneTrade(TradeOffer trade, string userToken)
            {
                this.trade = trade;
                this.userToken = userToken;
            }

            public TradeOffer GetTrade()
            {
                return trade;
            }
            public string GetUserToken()
            {
                return userToken;
            }
        }

        public ulong SendOfferToUser(string classId, string instanceId, string userId, string userToken)
        {
            ConvertIds(classId, instanceId);
            ConvertUserId(userId);
            loadCSGOInventory();
            TradeOffer trade = CreateTradeOffer(steamId, this.classId, this.instanceId);
            var tradeId = SendOffer(trade, userToken);
            var doneTrade = new DoneTrade(trade,userToken);
            trades.Add(tradeId,doneTrade);
            return tradeId;
        }

        private void ConvertUserId(string userId)
        {
            steamid = Convert.ToUInt64(userId);
            steamId = new SteamID(steamid);
        }

        private void ConvertIds(string classId, string instanceId)
        {
            this.classId = Convert.ToUInt64(classId);
            this.instanceId = Convert.ToUInt64(instanceId);
        }

        private void ConvertWeaponId(string weaponId)
        {
            weaponid = Convert.ToUInt64(weaponId);
        }

        public ulong RequestWeaponFromUser(string weaponId,string userId,string userToken)
        {
            GetTradeOffers();
            ConvertWeaponId(weaponId);
            ConvertUserId(userId);
            OtherSteamInventory.load(730, contextid, steamid);
            Log.Success("IDs converted.");
            TradeOffer trade = CreateGetItemTrade(steamId, weaponid);
            var tradeID = SendOffer(trade, userToken);
            Log.Info("Trade ID: " + tradeID);
            return tradeID;
        }

        private void GetTradeOffers()
        {
            // TODO:
            //if(TradeOffers == null) TradeOffers = Bot.TradeOffers;
        }

        private ulong SendOffer(TradeOffer tradeOffer, string otherToken)
        {
            Log.Success("Trying to send with token..");
            try
            {
                Log.Success("Trying to send with token...");
                    // TradeOfferSteamException will be thrown when sending fails
                    string tradeOfferIdWithToken;
                    tradeOffer.SendWithToken(out tradeOfferIdWithToken, otherToken);
                    Log.Success("Trade offer sent: Offer ID " + tradeOfferIdWithToken);
                    return Convert.ToUInt64(tradeOfferIdWithToken);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }

            return 0;
        }

        public TradeOffer CreateTradeOffer(SteamID OtherSID, ulong classId, ulong instanceId)
        {
            var tradeOffer = Bot.NewTradeOffer(OtherSID);

            var item = GetItemFromInventory(classId, genInv);
            tradeOffer.Items.AddMyItem(730, 2, Convert.ToInt64(item.assetid) );
            return tradeOffer;
        }

        private void loadCSGOInventory()
        {
            Bot.GetInventory();
            genInv.load(730, contextid, Bot.SteamClient.SteamID);
        }

        public TradeOffer CreateGetItemTrade(SteamID OtherSID, ulong itemId)
        {
            // EXAMPLE: working with inventories
            var tradeOffer = Bot.NewTradeOffer(OtherSID);

            if (tradeOffer != null)
            {
                Log.Success("Offer Status: " + tradeOffer.OfferState);
            }
            // second parameter is optional and tells the bot to only fetch the CSGO inventory (730)
            Log.Success("Offer Status " + tradeOffer.OfferState);
            var item = GetItemFromInventory(itemId, OtherSteamInventory);
            Log.Success("Item: " + item);
            try
            {
                Log.Success("Add item: " + item.assetid);
                tradeOffer.Items.AddTheirItem(730,2, Convert.ToInt64(item.assetid));
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
            Log.Success("Added item with id: " + item.assetid);
            return tradeOffer;
        }

        private GenericInventory.Item GetItemFromInventory(ulong classId, GenericInventory genericInventory)
        {
            foreach (var item in genericInventory.items)
            {
                var description = genericInventory.getDescription(item.Key);
                Log.Info("This item is: {0}.", description.name);
                if (description.classid.Equals(classId.ToString()))
                {
                    return item.Value;
                }
            }
            return null;
        }

        public GenericInventory GetBotInventory(string requestId)
        {
            var steamID = new SteamID(Convert.ToUInt64(requestId));
            if (Bot.SteamClient.SteamID == steamID)
            {
                return genInv;
            }
            return null;
        }

        public override void OnTradeOfferUpdated(TradeOffer tradeOffer)
        {
            var tradeOfferId = tradeOffer.TradeOfferId;
            var myItems = tradeOffer.Items.GetMyItems();
            var userItems = tradeOffer.Items.GetTheirItems();
            switch (tradeOffer.OfferState)
            {
                case TradeOfferState.TradeOfferStateAccepted:
                    Log.Success("Trade Complete.");
                    Log.Info("Trade offer #{0} accepted. Items given: {1}, Items received: {2}", tradeOfferId, myItems.Count, userItems.Count);
                    DemoServices.SetReqStatus(tradeOffer,"Success");
                    break;
                case TradeOfferState.TradeOfferStateCanceled:
                    Log.Warn("Trade offer #{0} has been canceled by bot.", tradeOfferId);
                    DemoServices.SetReqStatus(tradeOffer,"Canceled");
                    break;
                case TradeOfferState.TradeOfferStateDeclined:
                    Log.Warn("Trade offer #{0} has been declined.", tradeOfferId);
                    DemoServices.SetReqStatus(tradeOffer,"Declined");
                    break;
                case TradeOfferState.TradeOfferStateInvalid:
                    Log.Warn("Trade offer #{0} is invalid, with state: {1}.", tradeOfferId, tradeOffer.OfferState);
                    break;
                    
            }
        }

        public void setDemoService(RestDemoServices DemoService)
        {
            DemoServices = DemoService;
        }
    }
}