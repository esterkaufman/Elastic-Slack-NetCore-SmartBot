using System;
using Researcher.Bot.Integration.Slack.Interfaces;

namespace Researcher.Bot.Implementations.Slack
{
    public class SlackSecuredStore : ISlackSecuredStore
    {
        private readonly string SecretKey = Environment.GetEnvironmentVariable("SLACKAPI_SECRETKEY");
        private readonly string SlackToken = Environment.GetEnvironmentVariable("SLACKAPI_BOTUSER_OAUTHACCESSTOKEN");

        public string GetSecretKey()
        {
            return SecretKey;
        }

        public string GetToken()
        {
            return SlackToken;
        }
    }
}