namespace Researcher.Bot.Integration.Slack.Models.Events
{
    public class AppHomeEventBody : BaseEventBody
    {
        public string Tab { get; set; }
        public string User { get; set; }
        public string Channel { get; set; }
        public string Event_Ts { get; set; }
    }
}