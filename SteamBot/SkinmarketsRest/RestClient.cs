using System;
using System.IO;
using System.Net;
using SteamTrade.TradeOffer;

namespace ExampleBot
{
    public class RestClient
    {
        public RestClient()
        {
            URL = Service.GetBackendUrl();
            SECRET = Service.GetSecret();
        }        
        
        private string URL = "https://skinmarkets.com/api/";
        private string SECRET;
        private string BOTNAME;
        private string TRADERID;


        public string RestCall(string botname, string address, string secret)
        {
            if (address.Equals(""))
            {
                // TODO
            }
            var url = GetUrl();

            if (botname == null)
            {
                BOTNAME = botname;
            }

            String traderId = GetTraderId(BOTNAME);
            HttpWebRequest request = (HttpWebRequest) WebRequest.Create(url);
            request.KeepAlive = false;
            request.ProtocolVersion = HttpVersion.Version10;
            request.Method = "POST";

            request.ContentType = "application/json; charset=UTF-8";
            request.Accept = "application/json";

            GetSecret(secret);

            try
            {
                using (var streamWriter = new StreamWriter(request.GetRequestStream()))
                {
                    var json = "{\"traderId\":\"" + traderId + "\"," +
                               "\"address\"" + ":\"" + address + "\"," +
                               "\"secret\"" + ":\"" + SECRET + "\"}";
                    streamWriter.Write(json);
                    streamWriter.Flush();
                    streamWriter.Close();
                }
            }
            catch (WebException webEx)
            {
                return "Connection refused: " + webEx.Message;
            }

            try
            {
                var httpResponse = (HttpWebResponse) request.GetResponse();
                using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
                {
                    var result = streamReader.ReadToEnd();
                    return result;
                }
            }
            catch (Exception e)
            {
                return e.Message;
            }
        }

        private string GetUrl()
        {
            if (URL == null)
            {
                URL = "https://skinmarkets.com/api/";
            }
            var url = URL + "trade/register";
            return url;
        }

        private void GetSecret(string secret)
        {
            if (SECRET == null && secret == null)
            {
                SECRET = Service.GetSecret();
            }
            else
            {
                SECRET = secret;
            }
        }

        public string SendRequestUpdate(TradeOffer tradeOffer, String status)
        {
            
            if (URL == null)
            {
                URL = "https://skinmarkets.com/api/";
            }
            var id = tradeOffer.TradeOfferId;
            var url = URL + "trade/reqStatus";
            HttpWebRequest request = (HttpWebRequest) WebRequest.Create(url);
            request.KeepAlive = false;
            request.ProtocolVersion = HttpVersion.Version10;
            request.Method = "POST";

            request.ContentType = "application/json; charset=UTF-8";
            request.Accept = "application/json";
            if (SECRET == null) SECRET = Service.GetSecret();
            
            try
            {

            using (var streamWriter = new StreamWriter(request.GetRequestStream()))
            {
                var json = "{\"transferId\":\"" + id + "\"," +
                           "\"transferStatus\"" + ":\"" + status + "\"," +
                           "\"secret\"" + ":\"" + SECRET + "\"," +
                           "\"steambotId\"" + ":\"" + TRADERID + "\"}";
                streamWriter.Write(json);
                streamWriter.Flush();
                streamWriter.Close();
            }
            try
            {
                var httpResponse = (HttpWebResponse) request.GetResponse();
                using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
                {
                    var result = streamReader.ReadToEnd();
                    return result;
                }
                }
                catch (Exception e)
                {
                    return e.Message;
                }
            }
            catch (WebException e)
            {
                return e.Message;
            }
        }

        private String GetTraderId(string name)
        {
            String traderId = (name+SECRET)
                                  .GetHashCode().ToString();
            TRADERID = traderId;
            return TRADERID;
        }
        
        public string RestEndCall(string address,string secret)
        {
            if (URL == null)
            {
                URL = "https://skinmarkets.com/api/";
            }
            var url = URL + "trade/unregister";

            String traderId = GetTraderId(BOTNAME);
            HttpWebRequest request = (HttpWebRequest) WebRequest.Create(URL);
            request.KeepAlive = false;
            request.ProtocolVersion = HttpVersion.Version10;
            request.Method = "POST";

            request.ContentType = "application/json; charset=UTF-8";
            request.Accept = "application/json";

            if (SECRET == null && secret == null)
            {
                SECRET = Service.GetSecret();
            }
            else
            {
                SECRET = secret;
            }

            using (var streamWriter = new StreamWriter(request.GetRequestStream()))
            {
                var json = "{\"traderId\":\"" + traderId + "\"," +
                           "\"address\"" + ":\"" + address + "\"," +
                           "\"secret\"" + ":\"" + SECRET + "\"}";
                streamWriter.Write(json);
                streamWriter.Flush();
                streamWriter.Close();
            }
            try
            {
                //var httpResponse = (HttpWebResponse) request.GetResponse();
                //using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
                {
                    //var result = streamReader.ReadToEnd();
                    //return result;
                    return "";
                }
            }
            catch (Exception e)
            {
                return e.Message;
            }

        }


    }
}