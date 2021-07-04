using System;
using System.Collections.Generic;
using System.Text;

namespace Researcher.Bot.Integration.Slack.Models.Interactive
{
    public class BlockActionInteraction : InteractionPayloadBody
    {

        public IEnumerable<ActionsBlock> Actions { get; set; }

    }
}
