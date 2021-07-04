using System;
using System.Linq;
using Microsoft.Extensions.Logging;
using Researcher.Bot.Integration.Slack.Interfaces;
using Researcher.Bot.Integration.Slack.Models.Responses;
using Researcher.Bot.Integration.Slack.Views;
using System.Threading.Tasks;
using Researcher.Bot.Integration.Slack.Models.Interactive;
using Researcher.Bot.Integration.Slack.Utils;

namespace Researcher.Bot.Implementations.Slack
{
    public class ResearcherBotResponseSender : IBotResponseSender
    {
        private readonly ISlackHttpClientBuilder _slackHttpBuilder;
        private readonly ISlackSecuredStore _tokenStore;
        private readonly ILogger<ResearcherBotResponseSender> _logger;
        private ISlackHttpClient _slackClient { get; set; }

        public ResearcherBotResponseSender(ISlackHttpClientBuilder slackHttpBuilder, ISlackSecuredStore tokenStore, ILogger<ResearcherBotResponseSender> logger)
        {
            _slackHttpBuilder = slackHttpBuilder;
            _tokenStore = tokenStore;
            _logger = logger;
            var token = _tokenStore.GetToken();
            _slackClient = _slackHttpBuilder.Build(token);
        }

        public Task<ViewPublishResponse> SendAppHomeViewResponse(ViewPublishRequest request)
        {
            return _slackClient.ViewPublish(request);
        }

        public Task<ViewPublishResponse> SendBotResponseInModal(string triggerId, string userId,string title, IBlock[] blocks)
        {
            if (blocks == null || !blocks.Any())
            {
                blocks = new IBlock[] { new SectionBlock() { text = new Text { type = TextTypes.Markdown, text = title } }};
            }

            return _slackClient.ViewPublish(new ViewPublishRequest(userId, triggerId)
            {
                View = new View()
                {
                    Title = new Text {  text = title[..Math.Min(title.Length, 24)] },
                    Blocks = blocks,
                    Type = ViewsTypes.Modal,
                    Submit = new Text
                    {
                        text = "Submit"
                    },
                    Close = new Text
                    {
                        text = "Cancel"
                    },
                    
                }
            });
        }

        public Task<ChatPostMessageResponse> SendBotResponse(string channel, string response, IBlock[] blocks = null, Attachment[] attachments = null)
        {

            return _slackClient.ChatPostMessage(channel, response, blocks, attachments);
        }
        public Task<ChatPostMessageResponse> SendBotEphemeralResponse(string user, string channel, string response, IBlock[] blocks = null, Attachment[] attachments = null)
        {

            return _slackClient.ChatPostEphemeral(user, channel, response, blocks, attachments);
        }
    }
}
