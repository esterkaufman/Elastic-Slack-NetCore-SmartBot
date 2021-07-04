using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Nest;
using Researcher.Bot.Integration.ElasticSearch.Builders;
using Researcher.Bot.Integration.ElasticSearch.Interfaces;
using Researcher.Bot.Integration.ElasticSearch.Models;
using Researcher.Bot.Integration.ElasticSearch.Services;
using System;
using System.Collections.Generic;
using System.Text;

namespace Researcher.Bot.Integration.ElasticSearch.Extensions
{
    public static class ServiceCollectionExtensions
    {

        private static IElasticSearchEndpointServicesBuilder AddElasticSearchApiEvents(this IServiceCollection services)
        {
            return new ElasticSearchEndpointServicesBuilder(services);
        }

        public static IElasticSearchEndpointServicesBuilder AddElasticSearchIntegration(this IServiceCollection services, IElasticSearchConfig esConfig)
        {
            services.AddSingleton(esConfig);
            services.AddSingleton(ElasticSearchServiceBuilder.Build(esConfig));
            services.AddSingleton<IElasticSearchService, ElasticSearchService>();
            return services.AddElasticSearchApiEvents();
        }

    }
}
