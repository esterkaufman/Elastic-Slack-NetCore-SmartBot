using Newtonsoft.Json;

namespace Researcher.Bot.Integration.Slack.Views
{
    public class ViewPublishRequest
    {
        public ViewPublishRequest(string userId=null, string triggerId=null)
        {
            Trigger_id = triggerId;
            User_Id = userId;
        }

        [JsonProperty("trigger_id")]
        public string Trigger_id { get; }

        [JsonProperty("user_id")]
        public string User_Id { get; }
        
        public View View { get; set; }
    }
}