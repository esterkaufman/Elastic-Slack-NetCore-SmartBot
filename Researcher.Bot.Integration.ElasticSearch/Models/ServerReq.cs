using System;
using System.Collections.Generic;
using System.Text;

namespace Researcher.Bot.Integration.ElasticSearch.Models
{
    internal class ServerReq : EsBaseModel
    {
        public Event Event { get; set; }

        public Params Params { get; set; }

        public string ContentLength { get; set; }

        public Protocol Protocol { get; set; }
        public Client Client { get; set; }

    }

    public class Client
    {
        public string System { get; set; }
    }
    public class Event
    {
        public string OldUid { get; set; }
        public string EmailToken { get; set; }
        public string Uid { get; set; }
        public string Uuid { get; set; }
    }
    public class Stats
    {
        public Hades Hades { get; set; }
        public Total Total { get; set; }
    }
    public class Hades
    {
        public string Calls { get; set; }
    }
    public class Total
    {
        public string Time { get; set; }
    }


    public class Params
    {
        public string EmailToken { get; set; }

        public string Email { get; set; }
        public object Uid { get; set; }
    }
    public class Protocol
    {
        public string Method { get; set; }
    }


}
