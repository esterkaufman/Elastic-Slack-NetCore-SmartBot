using System;
using System.Collections.Generic;
using System.Text;

namespace Researcher.Bot.Integration.ElasticSearch.Models
{
    public class LogDogEsResponse : EsBaseResponse
    {
        public string ErrorCode { get; set; }
        public string ErrorMessage { get; set; }
        public string Duration { get; set; }
        public string Time { get; set; }
        public string CallId { get; set; }
        public string Dc { get; set; }
        public string Endpoint { get; set; }
        public string ServiceNames { get; set; }
        public long Logs { get; set; }
        public IEnumerable<string> Errors { get; set; }
        public string EndTime { get; set; }
    }
}
