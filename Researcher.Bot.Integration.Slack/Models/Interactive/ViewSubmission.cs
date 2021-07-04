using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Text;

namespace Researcher.Bot.Integration.Slack.Models.Interactive
{
    public class ViewSubmission : InteractionPayloadBody
    {

        public string ViewId { get; set; }
        public JObject ViewState { get; set; }
    }
}
