using Microsoft.Extensions.DependencyInjection;

using Researcher.Bot.Integration.Slack.Builders;
using Researcher.Bot.Integration.Slack.Interfaces;
using Researcher.Bot.Integration.Slack.Services;

namespace Researcher.Bot.Integration.Slack.Extensions
{
    public static class ServiceCollectionExtensions
    {
        private static IServiceCollection AddSlackHttpClientBuilder(this IServiceCollection services)
        {
            services.AddHttpClient();
            services.AddSingleton<ISlackHttpClientBuilder, SlackHttpClientBuilder>();
            return services;
        }

        private static ISlackEndpointServicesBuilder AddSlackApiEvents<T>(this IServiceCollection services) where T : class, ISlackSecuredStore
        {
            services.AddSingleton<ISlackSecuredStore, T>();
            return new SlackEndpointServicesBuilder(services);
        }

        public static ISlackEndpointServicesBuilder AddSlackIntegration<T>(this IServiceCollection services) where T : class, ISlackSecuredStore
        {
            services.AddSlackHttpClientBuilder();
            return services.AddSlackApiEvents<T>();

        }
    }
}
