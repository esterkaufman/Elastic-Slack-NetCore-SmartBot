using Newtonsoft.Json;

namespace Researcher.Bot.Integration.Slack.Models.Interactive
{
    public class Team
    {
        [JsonProperty("id")]
        public string Id { get; set; }
    }
}