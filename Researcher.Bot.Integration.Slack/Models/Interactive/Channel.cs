using Newtonsoft.Json;

namespace Researcher.Bot.Integration.Slack.Models.Interactive
{
    public class Channel
    {
        [JsonProperty("id")]
        public string Id { get; set; }
        public string Name { get; set; }
    }
}