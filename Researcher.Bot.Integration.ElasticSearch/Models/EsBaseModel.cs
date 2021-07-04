using System;
using System.Collections.Generic;
using System.Text;

namespace Researcher.Bot.Integration.ElasticSearch.Models
{
    public class EsBaseModel
    {
        public string SiteId { get; set; }

        public string Priority { get; set; }
        public string Host { get; set; }

        public string Dc { get; set; }
        public string Type { get; set; }

        public DateTime Timestamp { get; set; }

        public string CallID { get; set; }

        public Target Target { get; set; }
        public EsException Exception { get; set; }

        public Server Server { get; set; }
        public string Endpoint { get; set; }
        public string Message { get; set; }
        public string ErrCode { get; set; }
        public string ErrMessage { get; set; }
        public string PspanID { get; set; }
        public string SpanID { get; set; }
        public Stats Stats { get; set; }
    }

    public class EsException
    {
        public string Message { get; set; }
        public string Type { get; set; }
        public string OneWordMessage { get; set; }
    }

    public class Target
    {
        public string Method { get; set; }

        public string Service { get; set; }
    }

    public class Server
    {
        public string System { get; set; }
    }

}
