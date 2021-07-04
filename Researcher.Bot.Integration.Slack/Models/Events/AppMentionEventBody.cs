namespace Researcher.Bot.Integration.Slack.Models.Events
{
    public class AppMentionEventBody : BaseEventBody
    {
        public string Text { get; set; }
        public string User { get; set; }
        public string Channel { get; set; }
        public string Ts { get; set; }
        public string Event_Ts { get; set; }
    }
}