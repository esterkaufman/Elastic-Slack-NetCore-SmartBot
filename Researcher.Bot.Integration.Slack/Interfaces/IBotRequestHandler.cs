using Researcher.Bot.Integration.Slack.Models.Events;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Researcher.Bot.Integration.Slack.Interfaces
{
    public interface IBotRequestHandler: IAppMentionHandler, ISlashCommandsHandler,IInteractivesHandler
    {
    }
}
