namespace Researcher.Bot.Integration.Slack.Interfaces
{
    public interface ISlackHttpClientBuilder
    {
        ISlackHttpClient Build(string token);
    }
}
