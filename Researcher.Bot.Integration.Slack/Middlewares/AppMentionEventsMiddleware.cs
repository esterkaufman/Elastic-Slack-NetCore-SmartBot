using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Researcher.Bot.Integration.Slack.Interfaces;
using Researcher.Bot.Integration.Slack.Models.Events;
using Researcher.Bot.Integration.Slack.Models.Exceptions;
using Researcher.Bot.Integration.Slack.Models.Interactive;
using Researcher.Bot.Integration.Slack.Models.Responses;
using Researcher.Bot.Integration.Slack.Utils;

namespace Researcher.Bot.Integration.Slack.Middlewares
{
    public class AppMentionEventsMiddleware
    {
        private readonly ILogger<AppMentionEventsMiddleware> _logger;
        private readonly RequestDelegate _next;

        private readonly IEnumerable<IAppMentionHandler> _responseHandlers;
        private readonly IBotResponseSender _responseSender;

        public AppMentionEventsMiddleware(RequestDelegate next, ILogger<AppMentionEventsMiddleware> logger,
            IEnumerable<IAppMentionHandler> responseHandler, IBotResponseSender responseSender)
        {
            _next = next;
            _logger = logger;
            _responseHandlers = responseHandler;
            _responseSender = responseSender;
        }

        public async Task Invoke(HttpContext context)
        {
            var metadata = (SlackApiMetaData)context.Items[HttpContextKeys.ApiMetadataKey];
            var appMentionEvent = (AppMentionEventBody)context.Items[HttpContextKeys.EventBodyKey];
            var handler = _responseHandlers.FirstOrDefault();

            if (handler == null)
            {
                _logger.LogError("No handler registered for IAppMentionHandler");
            }
            else
            {
                _logger.LogInformation($"Handling using {handler.GetType()}");
                try
                {

                    await SendBotEphemeralResponse(appMentionEvent, $"I've got your request.{Emojis.GetRandomWorkingEmoji()} working on it...");

                    var response = await handler.Handle(metadata, appMentionEvent, async (msg) =>
                    {
                        await SendBotEphemeralResponse(appMentionEvent, msg);
                    });
                    var responseForEvent = response.Response;

                    if (!string.IsNullOrEmpty(responseForEvent))
                    {
                        var res = 
                            await _responseSender.SendBotResponse(appMentionEvent.Channel, responseForEvent, response.Blocks, response.Attachments);

                        if (!res.Ok)
                        {
                            _logger.LogError($"Cannot response the app mention event \n{res.Error}");
                        }
                    }
                    else
                    {
                        _logger.LogError($"Got empty response for the app mention event");
                    }
                }
                catch (SlackApiException e)
                {
                    _logger.LogError(e, $"Error: {e.Error}\nResponseContent: {e.ResponseContent}");
                }
                catch (Exception e)
                {
                    _logger.LogError(e, e.Message);
                }
            }

        }

        private async Task SendBotEphemeralResponse(AppMentionEventBody appMentionEvent, string msg)
        {
            await _responseSender.SendBotEphemeralResponse(appMentionEvent.User, appMentionEvent.Channel, msg);
        }

        public static bool ShouldRun(HttpContext ctx)
        {
            return ctx.Items.ContainsKey(HttpContextKeys.EventTypeKey) &&
                   new[] { SlackEventKeys.AppMention, SlackEventKeys.ImMessage }
                       .Contains(ctx.Items[HttpContextKeys.EventTypeKey].ToString());
        }
    }
}