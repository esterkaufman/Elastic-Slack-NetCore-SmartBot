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
    public class InteractivesMiddleware
    {
        private readonly ILogger<InteractivesMiddleware> _logger;
        private readonly RequestDelegate _next;

        private readonly IEnumerable<IInteractivesHandler> _responseHandlers;
        private readonly IBotResponseSender _responseSender;

        public InteractivesMiddleware(RequestDelegate next, ILogger<InteractivesMiddleware> logger,
            IEnumerable<IInteractivesHandler> responseHandler, IBotResponseSender responseSender)
        {
            _next = next;
            _logger = logger;
            _responseHandlers = responseHandler;
            _responseSender = responseSender;
        }

        public async Task Invoke(HttpContext context)
        {
            var interaction = (InteractionPayloadBody)context.Items[HttpContextKeys.InteractivePayloadKey];
            var handler = _responseHandlers.FirstOrDefault();

            if (handler == null)
            {
                _logger.LogError("No handler registered for IInteractivesHandler");
            }
            else
            {
                _logger.LogInformation($"Handling using {handler.GetType()}");
                try
                {
                    var response = await handler.HandleInteraction(interaction);
                    var responseForEvent = response.Response;

                    if (!string.IsNullOrEmpty(responseForEvent))
                    {
                        Response res;

                        if ((response.IsModal.HasValue && response.IsModal.Value) ||
                            (interaction.Channel?.Id == null && interaction.Trigger_Id != null))
                        {
                            res = await _responseSender.SendBotResponseInModal(interaction.Trigger_Id,
                                interaction.User.Id, responseForEvent,
                                response.Blocks);
                        }
                        else
                        {
                            res = await _responseSender.SendBotResponse(interaction.Channel.Id,
                                responseForEvent,
                                response.Blocks, response.Attachments);
                        }

                        if (!res.Ok)
                        {
                            _logger.LogError($"Cannot response the interaction \n{res.Error}");
                        }
                    }
                    else
                    {
                        _logger.LogError($"Got empty response for the interaction");
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
            return ctx.Items.ContainsKey(HttpContextKeys.InteractivePayloadKey);
        }
    }
}