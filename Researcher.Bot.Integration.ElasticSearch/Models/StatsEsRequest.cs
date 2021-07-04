using System;
using System.Collections.Generic;
using System.Text;

namespace Researcher.Bot.Integration.ElasticSearch.Models
{
    public class StatsEsRequest
    {
        public string Type { get; set; } = "serverReq";
        public string From { get; set; } = "now-1h";
        public string To { get; set; } = "now";
        public string Dc { get; set; }
        public string ServiceName { get; set; }
    }
}
