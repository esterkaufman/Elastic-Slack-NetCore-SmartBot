using System;
using System.Collections.Generic;
using System.Text;

namespace Researcher.Bot.Integration.ElasticSearch.Models
{
    public class BlamerEsResponse : EsBaseResponse
    {
        public string Dc { get; set; }
        public string ServiceName { get; set; }
        public string Endpoint { get; set; }
        public string ExceptionType { get; set; }
        public string ExceptionMessage { get; set; }
        public string[] CallsStack { get; set; }
        public string OneWordMessage { get; set; }
        public string Time { get; set; }
    }
}
