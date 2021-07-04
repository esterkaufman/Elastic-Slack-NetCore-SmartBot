using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Researcher.Bot.Integration.ElasticSearch.Interfaces;
using Researcher.Bot.Integration.ElasticSearch.Models;

namespace Researcher.Bot.Implementations.ElasticSearch
{
    public class ResearcherBotEsHandler : IElasticSearchHandler
    {
        private ILogger<ResearcherBotEsHandler> _logger;
        private readonly IElasticSearchService _searchService;

        public ResearcherBotEsHandler(IElasticSearchService searchService, ILogger<ResearcherBotEsHandler> logger)
        {
            _searchService = searchService;
            _logger = logger;
        }

        public Task<StatsEsResponse> GetStatistics(StatsEsRequest request)
        {
            return _searchService.GetStatisticsForService(request);
        }

        public Task<BlamerEsResponse> GetNocBlamer(string callID, string dc = null)
        {
            return _searchService.GetNocBlamer(callID,dc);
        }
        public Task<LogDogEsResponse> GetLogDog( string callID, string dc = null)
        {
            return _searchService.GetLogDog( callID, dc);
        }
    }
}