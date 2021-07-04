
using Researcher.Bot.Integration.Slack.Models.Interactive;

namespace Researcher.Bot.Integration.Slack.Models.Events
{
    public class HandlerResponse
    {
        public string Response { get; set; }
        public bool? IsModal { get; set; }
        public IBlock[] Blocks{ get; set; }
        public Attachment[] Attachments { get; set; }

        public HandlerResponse(string response, IBlock[] blocks = null, Attachment[] attachments=null,bool? isModal=null)
        {
            Response = response;
            Blocks = blocks;
            Attachments = attachments;
            IsModal = isModal;
        }
    }
}
