using Researcher.Bot.Integration.ElasticSearch.Models;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace Researcher.Bot.Integration.ElasticSearch.Interfaces
{
    public interface IElasticSearchService
    {
        Task<StatsEsResponse> GetStatisticsForService(StatsEsRequest request);
        Task<BlamerEsResponse> GetNocBlamer(string callID, string dc = null);
        Task<LogDogEsResponse> GetLogDog(string callID, string dc = null);
        Task<List<Webhook>> GetWebhooksComparerSentToSentToDispatcherEvents(WebhooksEsRequest webhooksEsRequest);
    }
}
