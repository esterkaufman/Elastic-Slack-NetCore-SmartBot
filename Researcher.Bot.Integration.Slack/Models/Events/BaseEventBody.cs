using Newtonsoft.Json.Linq;
using Researcher.Bot.Integration.Slack.Models.Interactive;

namespace Researcher.Bot.Integration.Slack.Models.Events
{
    public class BaseEventBody
    {
        public string Type { get; set; }
        public JObject Bot_Profile { get; set; }
        public Block[] Blocks { get; set; }
    }

    public class UnknownEventBody : BaseEventBody
    {
        public string RawJson { get; set; }
    }
}