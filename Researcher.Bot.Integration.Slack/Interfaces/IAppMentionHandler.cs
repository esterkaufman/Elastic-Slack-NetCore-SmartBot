using System;
using Researcher.Bot.Integration.Slack.Models.Events;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Researcher.Bot.Integration.Slack.Interfaces
{
    public interface IAppMentionHandler
    {
        Task<HandlerResponse> Handle(SlackApiMetaData eventMetadata, AppMentionEventBody slackEvent, Action<string> postToBot = null);
        string BuildDescription() => "";
    }
}
