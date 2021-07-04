using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Researcher.Bot.Integration.Slack.Models.Events;
using Researcher.Bot.Integration.Slack.Models.Interactive;

namespace Researcher.Bot.Integration.Slack.Interfaces
{
    public interface IInteractivesHandler
    {
        Task<HandlerResponse> HandleInteraction(InteractionPayloadBody interaction);

    }
}
