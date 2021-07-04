using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Researcher.Bot.Integration.Slack.Interfaces;
using Researcher.Bot.Integration.Slack.Models.Events;
using Researcher.Bot.Integration.Slack.Models.Exceptions;
using Researcher.Bot.Integration.Slack.Models.Responses;
using Researcher.Bot.Integration.Slack.Utils;
using Researcher.Bot.Integration.Slack.Views;

namespace Researcher.Bot.Integration.Slack.Middlewares
{
    internal class AppHomeEventsMiddleware
    {
        private readonly ILogger<AppHomeEventsMiddleware> _logger;
        private readonly RequestDelegate _next;
        private readonly IEnumerable<IAppHomeHandler> _responseHandlers;
        private readonly IBotResponseSender _responseSender;

        public AppHomeEventsMiddleware(RequestDelegate next, ILogger<AppHomeEventsMiddleware> logger,
            IEnumerable<IAppHomeHandler> responseHandlers, IBotResponseSender responseSender)
        {
            _next = next;
            _logger = logger;
            _responseSender = responseSender;
            _responseHandlers = responseHandlers;
        }

        public async Task Invoke(HttpContext context)
        {
            var metadata = (SlackApiMetaData)context.Items[HttpContextKeys.ApiMetadataKey];
            var appHomeEvent = (AppHomeEventBody)context.Items[HttpContextKeys.EventBodyKey];
            var handler = _responseHandlers.FirstOrDefault();
            if (handler == null)
            {
                _logger.LogError("No handler registered for IAppHomeHandler");
            }
            else
            {
                _logger.LogInformation($"Handling using {handler.GetType()}");
                try
                {
                    _logger.LogInformation($"Handling using {handler.GetType()}");

                    var viewPublishRequest = await handler.Handle(metadata, appHomeEvent);
                    var res = await _responseSender.SendAppHomeViewResponse(viewPublishRequest);

                    _logger.LogInformation("View publish done");
                    if (!res.Ok)
                    {
                        _logger.LogError($"Cannot response the app home opened event \n{res.Error}");
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

        public static bool ShouldRun(HttpContext ctx)
        {
            return ctx.Items.ContainsKey(HttpContextKeys.EventTypeKey) &&
                   ctx.Items[HttpContextKeys.EventTypeKey].ToString() == SlackEventKeys.AppHome;
        }
    }
}