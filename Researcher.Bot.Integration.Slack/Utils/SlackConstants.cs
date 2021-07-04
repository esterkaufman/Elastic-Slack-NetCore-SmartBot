namespace Researcher.Bot.Integration.Slack.Utils
{
    public static class SlackConstants
    {
        public const string SlackApiTypes_Event = "event_callback";
        public const string SlackApiTypes_Shortcut = "shortcut";
    }
    public class HttpContextKeys
    {
        public const string ChallengeKey = "slackevents:challenge";
        public const string ApiMetadataKey = "slackevents:metadata";
        public const string EventBodyKey = "slackevents:event";
        public const string EventTypeKey = "slackevents:eventtype";
        public const string InteractivePayloadKey = "slackinteractive:payload";
        public const string InteractiveSlashCommandsKey = "slackinteractive:slashcommands";


    }
    public static class SlackEventKeys
    {
        public const string TokensRevoked = "tokens_revoked";
        public const string AppMention = "app_mention";
        public const string ImMessage = "message";
        public const string AppHome = "app_home_opened";
    }
    public class InteractionTypes
    {
        public const string ViewSubmission = "view_submission";
        public const string BlockActions = "block_actions";
        public const string MessageAction = "message_action";
    }
    public static class ViewsTypes
    {

        public const string Home = "home";
        public const string Modal = "modal";
    }
}