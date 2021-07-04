namespace Researcher.Bot.Integration.Slack.Models.Events
{
    public class SlackApiMetaData
    {
        public string Token { get; set; }
        public string Team_Id { get; set; }
        public string Type { get; set; }
        public string[] AuthedUsers { get; set; }
        public string Event_Id { get; set; }

    }
}
