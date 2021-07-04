using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Researcher.Bot.Implementations.ElasticSearch;
using Researcher.Bot.Implementations.Slack;
using Researcher.Bot.Integration.ElasticSearch.Extensions;
using Researcher.Bot.Integration.Slack.Extensions;

namespace Researcher.Bot.Api
{
    public class Startup
    {
        private const string API_PATH = "/researcher/api/slack";

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddLogging(loggingBuilder => {
                var loggingSection = Configuration.GetSection("Logging");
                loggingBuilder.AddFile(loggingSection);
            });

            services.AddElasticSearchIntegration(new ElasticSearchConfig(Configuration))
                .AddElasticSearchHandler<ResearcherBotEsHandler>();

            services.AddSlackIntegration<SlackSecuredStore>()
                .AddAppMentionHandler<ResearcherBotRequestHandler>()
                .AddSlashCommandsHandler<ResearcherBotRequestHandler>()
                .AddInteractivesHandler<ResearcherBotRequestHandler>()
                .AddAppHomeHandler<AppHomeHandler>()
                .AddBotResponseSender<ResearcherBotResponseSender>();

            services.AddControllers();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseSlackApi(API_PATH);
        }
    }
}
