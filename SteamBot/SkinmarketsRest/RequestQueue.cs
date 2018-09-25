using System;
using System.Collections.Generic;

namespace ExampleBot
{
    public class RequestQueue
    {
        private Dictionary<long,string> dict = new Dictionary<long, string>();
//        private int requestCounter = 0;
        public long addRequest(ulong id)
        {
            var reqid = Convert.ToInt64(id);
            return addRequest(reqid);
        }

        public long addRequest(long id)
        {
            dict.Add(id, "Request created");
            return id;
        }

        public long addRequest(string id)
        {
            var reqid = Convert.ToInt64(id);
            return addRequest(reqid);
        }

        public void ChangeStatus(ulong id, string status)
        {
            var reqid = Convert.ToInt64(id);
            ChangeStatus(reqid,status);
        }

        public void ChangeStatus(long id, string status)
        {
            dict[id] = status;
        }

        public void ChangeStatus(string id, string status)
        {
            var lid = Convert.ToInt64(id);
            ChangeStatus(lid, status);
        }

        public string GetStatus(long id)
        {
            return dict[id];
        }

    }
}