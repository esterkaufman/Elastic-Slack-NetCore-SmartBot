using Researcher.Bot.Integration.Slack.Models.Events;
using Researcher.Bot.Integration.Slack.Views;
using System.Threading.Tasks;

namespace Researcher.Bot.Integration.Slack.Interfaces
{
    public interface IAppHomeHandler
    {
        Task<ViewPublishRequest> Handle(SlackApiMetaData eventMetadata, AppHomeEventBody payload);
    }
}
