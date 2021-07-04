using Researcher.Bot.Integration.Slack.Models.Responses;
using Researcher.Bot.Integration.Slack.Views;
using System.Threading.Tasks;
using Researcher.Bot.Integration.Slack.Models.Interactive;

namespace Researcher.Bot.Integration.Slack.Interfaces
{
    public interface IBotResponseSender
    {
        Task<ChatPostMessageResponse> SendBotResponse(string channel, string response, IBlock[] blocks = null, Attachment[] attachments = null);

        Task<ChatPostMessageResponse> SendBotEphemeralResponse(string user, string channel, string response, IBlock[] blocks=null, Attachment[] attachments = null);
        Task<ViewPublishResponse> SendAppHomeViewResponse(ViewPublishRequest request);
        Task<ViewPublishResponse> SendBotResponseInModal(string triggerId, string userId, string title, IBlock[] blocks);
    }
}
