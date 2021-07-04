namespace Researcher.Bot.Integration.Slack.Models.Interactive
{
    public class UnknownInteractiveMessage : InteractionPayloadBody
    {
        public string RawJson { get; set; }
    }
}