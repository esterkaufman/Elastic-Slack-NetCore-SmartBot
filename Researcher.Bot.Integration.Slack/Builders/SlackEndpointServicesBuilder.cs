using Microsoft.Extensions.DependencyInjection;
using Researcher.Bot.Integration.Slack.Interfaces;

namespace Researcher.Bot.Integration.Slack.Builders
{
    public class SlackEndpointServicesBuilder : ISlackEndpointServicesBuilder
    {
        private readonly IServiceCollection _services;

        public SlackEndpointServicesBuilder(IServiceCollection services)
        {
            _services = services;
        }

        public ISlackEndpointServicesBuilder AddAppMentionHandler<T>() where T : class, IAppMentionHandler
        {
            _services.AddSingleton<IAppMentionHandler, T>();
            return this;
        }
        public ISlackEndpointServicesBuilder AddSlashCommandsHandler<T>() where T : class, ISlashCommandsHandler
        {
            _services.AddSingleton<ISlashCommandsHandler, T>();
            return this;
        }

        public ISlackEndpointServicesBuilder AddInteractivesHandler<T>() where T : class, IInteractivesHandler
        {
            _services.AddSingleton<IInteractivesHandler, T>();
            return this;
        }

        public ISlackEndpointServicesBuilder AddAppHomeHandler<T>() where T : class, IAppHomeHandler
        {
            _services.AddSingleton<IAppHomeHandler, T>();
            return this;
        }

        public ISlackEndpointServicesBuilder AddBotResponseSender<T>() where T : class, IBotResponseSender
        {
            _services.AddSingleton<IBotResponseSender, T>();
            return this;
        }

    }
}
