using System;
using System.Net;
using System.Net.Sockets;

namespace ExampleBot
{
    public static class Service
    {        
        public static string GetBackendUrl()
        {
            var envVar = "BACKEND_URL";
            var url = GetEnvVar(envVar);
            if (url != null && url.Contains("trade"))
            {
                return url.Substring(0,url.IndexOf("trade"));
            }
            //if(mainLog !=null)mainLog.Debug("Backend URL: " + URL);
            if (url == null) url = "https://skinmarkets.com/api/";
            //if (url == null) url = "http://localhost:8080/api/";
            return url;
        }

        public static string GetEnvVar(string envVar)
        {
            var url = Environment.GetEnvironmentVariable(envVar,
                EnvironmentVariableTarget.Process); // Funktioniert mit der EnvVar aus de Dockerfile
            if (url == null) url = Environment.GetEnvironmentVariable(envVar, EnvironmentVariableTarget.Machine);
            if (url == null) url = Environment.GetEnvironmentVariable(envVar, EnvironmentVariableTarget.User);
            return url;
        }

        public static string GetLocalIpAddress()
        {
            var host = Dns.GetHostEntry(Dns.GetHostName());
            foreach (var ip in host.AddressList)
            {
                if (ip.AddressFamily == AddressFamily.InterNetwork)
                {
                    return ip.ToString();
                }
            }
            throw new Exception("Local IP Address not found!");
        }
        
        public static string GetSecret()
        {
            return Environment.GetEnvironmentVariable("SECRET", EnvironmentVariableTarget.Process);
        }


    }
}