using System;

namespace Researcher.Bot.Integration.Slack.Models.Exceptions
{
    public class SlackApiException : Exception
    {
        public SlackApiException(string error, string responseContent) : base(error)
        {
            Error = error;
            ResponseContent = responseContent;
        }

        public string ResponseContent { get; }

        public string Error { get; }
    }
}