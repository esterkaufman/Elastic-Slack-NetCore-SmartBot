using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;

namespace Researcher.Bot.Integration.Slack.Models.Events
{
    public class SlashCommandPayloadBody
    {
        public string Token { get; set; }
        public string TeamId { get; set; }
        public string TeamDomain { get; set; }
        public string ChannelId { get; set; }
        public string ChannelName { get; set; }
        public string UserId { get; set; }
        public string UserName { get; set; }
        public string Command { get; set; }
        public string Text { get; set; }
        public string ResponseUrl { get; set; }
        public string TriggerId { get; set; }

        public SlashCommandPayloadBody()
        {
        }
        /// <summary>
        /// Builds a SlashCommandPaylod from the rawBody of a SlashCommand Post request
        /// </summary>
        /// <param name="rawBody"></param>
        public SlashCommandPayloadBody(string rawBody)
        {
            var dictionary = ParseBody(rawBody);

            Token = dictionary["token"];
            TeamId = dictionary["team_id"];
            TeamDomain = dictionary["team_domain"];
            ChannelId = dictionary["channel_id"];
            ChannelName = dictionary["channel_name"];
            UserId = dictionary["user_id"];
            UserName = dictionary["user_name"];
            Command = dictionary["command"];
            Text = dictionary["text"];
            ResponseUrl = dictionary["response_url"];
            TriggerId = dictionary["trigger_id"];
        }

        /// <summary>
        /// Parses the body of a slack slash command into a dictionary
        /// </summary>
        /// <param name="body"></param>
        /// <returns></returns>
        private IDictionary<string, string> ParseBody(string body)
        {
            return body.Split('&').Select(x => x.Split('=')).ToDictionary(x => x[0], x => HttpUtility.UrlDecode(x[1]));
        }
    }
}
