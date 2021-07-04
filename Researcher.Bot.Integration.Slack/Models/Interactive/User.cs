using Newtonsoft.Json;

namespace Researcher.Bot.Integration.Slack.Models.Interactive
{
    public class User
    {
        //[JsonProperty("user_id")]
        [JsonProperty("id")]
        public string Id { get; set; }
    }
}