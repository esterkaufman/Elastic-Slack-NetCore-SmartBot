using System;
using System.Collections.Generic;
using System.Text;

namespace Researcher.Bot.Integration.ElasticSearch.Interfaces
{
    public interface IElasticSearchEndpointServicesBuilder
    {
        public IElasticSearchEndpointServicesBuilder AddElasticSearchHandler<T>() where T : class, IElasticSearchHandler;
    }
}
