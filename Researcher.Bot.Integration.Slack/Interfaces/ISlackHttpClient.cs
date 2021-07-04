using Researcher.Bot.Integration.Slack.Models.Responses;
using Researcher.Bot.Integration.Slack.Views;
using System.Threading.Tasks;
using Researcher.Bot.Integration.Slack.Models.Interactive;

namespace Researcher.Bot.Integration.Slack.Interfaces
{
    public interface ISlackHttpClient
    {
        // https://api.slack.com/methods/chat.postMessage
        Task<ChatPostMessageResponse> ChatPostMessage(string channel, string text,  IBlock[] blocks, Attachment[] attachments);

        // https://api.slack.com/methods/chat.postEphemeral
        Task<ChatPostMessageResponse> ChatPostEphemeral(string user, string channel, string text, IBlock[] blocks, Attachment[] attachments);

        // https://api.slack.com/methods/views.publish
        Task<ViewPublishResponse> ViewPublish(ViewPublishRequest view);
    }
}
