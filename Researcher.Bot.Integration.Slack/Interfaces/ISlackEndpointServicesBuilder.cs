namespace Researcher.Bot.Integration.Slack.Interfaces
{
    public interface ISlackEndpointServicesBuilder
    {
        public ISlackEndpointServicesBuilder AddBotResponseSender<T>() where T : class, IBotResponseSender;
        public ISlackEndpointServicesBuilder AddAppMentionHandler<T>() where T : class, IAppMentionHandler;
        public ISlackEndpointServicesBuilder AddSlashCommandsHandler<T>() where T : class, ISlashCommandsHandler;
        public ISlackEndpointServicesBuilder AddInteractivesHandler<T>() where T : class, IInteractivesHandler;
        public ISlackEndpointServicesBuilder AddAppHomeHandler<T>() where T : class, IAppHomeHandler;
    }
}
