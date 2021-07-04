using System.Threading.Tasks;

namespace Researcher.Bot.Integration.Slack.Interfaces
{
    public interface ISlackSecuredStore
    {
        string GetToken();
        string GetSecretKey();
    }
}
