using System;
using System.Collections.Generic;
using System.Text;

namespace Researcher.Bot.Integration.ElasticSearch.Models
{
    public class StatsEsResponse: EsBaseResponse
    {
        public string Dc{get;set;}
        public string Time{get;set;}
        public long RequestsCount{get;set;}
        public IEnumerable<string> ErrorCodeCounts{get;set;}
        public IEnumerable<string> Endpoints { get; internal set; }
        public IEnumerable<string> TargetMethods { get; internal set; }
        public IEnumerable<string> HadesCalls { get; internal set; }
    }
}
