using Researcher.Bot.Integration.ElasticSearch.Models;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Researcher.Bot.Integration.ElasticSearch.Interfaces
{
    public interface IElasticSearchHandler
    {
        Task<StatsEsResponse> GetStatistics(StatsEsRequest request);
        Task<BlamerEsResponse> GetNocBlamer(string callID, string dc = null);
        Task<LogDogEsResponse> GetLogDog( string callID, string dc = null);

    }
}
