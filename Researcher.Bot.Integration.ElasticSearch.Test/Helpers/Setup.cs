using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NReco.Logging.File;
using Researcher.Bot.Implementations.ElasticSearch;
using Researcher.Bot.Implementations.Slack;
using Researcher.Bot.Integration.ElasticSearch.Interfaces;
using Researcher.Bot.Integration.ElasticSearch.Services;
using Xunit.Abstractions;

namespace Researcher.Bot.Tests.Helpers
{
    public class Setup
    {
        protected readonly IElasticSearchService EsSearchService;
        protected readonly IElasticSearchHandler ElasticSearchHandler;
        protected readonly ILogger<ResearcherBotRequestHandler> Logger;


        public Setup(ITestOutputHelper helper)
        {
            var esConfig = new ElasticSearchConfig("http://kibana:9200/");
            var services = new ServiceCollection();

            services.AddLogging(loggingBuilder => {
            });

            services.AddSingleton<IElasticSearchConfig>(esConfig);
            services.AddSingleton<IElasticSearchHandler, ResearcherBotEsHandler>();
            services.AddSingleton(ElasticSearchServiceBuilder.Build(esConfig));
            services.AddSingleton<IElasticSearchService, ElasticSearchService>();
            services.AddSingleton<ILogger<IElasticSearchService>>(new XUnitLogger<IElasticSearchService>(helper));

            var provider = services.BuildServiceProvider();
            EsSearchService = provider.GetService<IElasticSearchService>();
            ElasticSearchHandler = provider.GetService<IElasticSearchHandler>();
            Logger = provider.GetService<ILogger<ResearcherBotRequestHandler>>();
        }
    }
}