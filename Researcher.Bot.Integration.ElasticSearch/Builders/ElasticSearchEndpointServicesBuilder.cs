using Microsoft.Extensions.DependencyInjection;
using Researcher.Bot.Integration.ElasticSearch.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace Researcher.Bot.Integration.ElasticSearch.Builders
{
    public class ElasticSearchEndpointServicesBuilder : IElasticSearchEndpointServicesBuilder
    {
        private readonly IServiceCollection _services;

        public ElasticSearchEndpointServicesBuilder(IServiceCollection services)
        {
            _services = services;
        }

        public IElasticSearchEndpointServicesBuilder AddElasticSearchHandler<T>() where T : class, IElasticSearchHandler
        {
            _services.AddSingleton<IElasticSearchHandler, T>();
            return this;
        }
    }
}
