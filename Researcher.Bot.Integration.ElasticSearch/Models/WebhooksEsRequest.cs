using System;
using System.Collections.Generic;
using System.Text;

namespace Researcher.Bot.Integration.ElasticSearch.Models
{
    public class WebhooksEsRequest
    {
        public string Type { get; set; } = "webhooks";
        public string From { get; set; } = "now-1h";
        public string To { get; set; } = "now";
        public string Index { get; set; }
        public string ServiceName { get; set; }
        public string SiteId { get; set; }
        public string CallbackUrl { get; set; }
    }
}
