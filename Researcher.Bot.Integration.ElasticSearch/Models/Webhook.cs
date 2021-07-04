using System;
using System.Collections.Generic;
using System.Text;

namespace Researcher.Bot.Integration.ElasticSearch.Models
{
    public class Webhook : EsBaseModel
    {
        public Webhooks Webhooks { get; set; }
        public string Uid { get; set; }

    }

    public class Webhooks
    {
        public string AccountType;
        public string EventType;
        public string EventID;
    }

}
