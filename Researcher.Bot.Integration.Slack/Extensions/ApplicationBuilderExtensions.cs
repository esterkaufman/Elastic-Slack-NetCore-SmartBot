
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Researcher.Bot.Integration.Slack.Middlewares;

namespace Researcher.Bot.Integration.Slack.Extensions
{
    public static class ApplicationBuilderExtensions
    {
        public static IApplicationBuilder UseSlackApi(this IApplicationBuilder app, string path = "/slack")
        {
            app.MapWhen(c => IsSlackRequest(c, path), a =>
            {
                a.UseMiddleware<HttpRequestsManager>();
                a.MapWhen(ChallengeEventsMiddleware.ShouldRun, b => b.UseMiddleware<ChallengeEventsMiddleware>());
                a.MapWhen(AppMentionEventsMiddleware.ShouldRun, b => b.UseMiddleware<AppMentionEventsMiddleware>());
                a.MapWhen(AppHomeEventsMiddleware.ShouldRun, b => b.UseMiddleware<AppHomeEventsMiddleware>());
                a.MapWhen(SlashCommandsMiddleware.ShouldRun, b => b.UseMiddleware<SlashCommandsMiddleware>());
                a.MapWhen(InteractivesMiddleware.ShouldRun, b => b.UseMiddleware<InteractivesMiddleware>());
            });

            return app;
        }

        private static bool IsSlackRequest(HttpContext ctx, string path)
        {
            return ctx.Request.Path.Value.Contains(path) && ctx.Request.Method == "POST";
        }
    }
}