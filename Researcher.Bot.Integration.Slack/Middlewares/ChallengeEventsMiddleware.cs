using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Researcher.Bot.Integration.Slack.Utils;
using System.Threading.Tasks;

namespace Researcher.Bot.Integration.Slack.Middlewares
{
    public class ChallengeEventsMiddleware
    {
        private readonly ILogger<ChallengeEventsMiddleware> _logger;
        public ChallengeEventsMiddleware(RequestDelegate next, ILogger<ChallengeEventsMiddleware> logger)
        {
            _logger = logger;
        }

        public async Task Invoke(HttpContext context)
        {
            var challenge = context.Items[HttpContextKeys.ChallengeKey];

            _logger.LogInformation($"Handling challenge request. Challenge: {challenge}");
            context.Response.StatusCode = 200;
            context.Response.ContentType = "application/json";
            await context.Response.WriteAsync(JsonConvert.SerializeObject(new { challenge }));
        }

        public static bool ShouldRun(HttpContext ctx)
        {
            return ctx.Items.ContainsKey(HttpContextKeys.ChallengeKey);
        }
    }
}