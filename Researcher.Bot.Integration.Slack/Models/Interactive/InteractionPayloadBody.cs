using System;
using System.Collections.Generic;
using System.Text;

namespace Researcher.Bot.Integration.Slack.Models.Interactive
{
    public class InteractionPayloadBody
    {

        public string Type { get; set; }
        public string Trigger_Id { get; set; }
        public Channel Channel { get; set; }
        public Team Team { get; set; }
        public User User { get; set; }
    }
}
