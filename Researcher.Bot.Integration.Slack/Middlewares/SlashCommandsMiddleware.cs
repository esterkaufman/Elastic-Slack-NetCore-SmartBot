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

namespace Researcher.Bot.Integration.Slack.Middlewares
{
    public class SlashCommandsMiddleware
    {
        private readonly ILogger<SlashCommandsMiddleware> _logger;
        private readonly RequestDelegate _next;

        private readonly IEnumerable<ISlashCommandsHandler> _responseHandlers;
        private readonly IBotResponseSender _responseSender;

        public SlashCommandsMiddleware(RequestDelegate next, ILogger<SlashCommandsMiddleware> logger,
            IEnumerable<ISlashCommandsHandler> responseHandler, IBotResponseSender responseSender)
        {
            _next = next;
            _logger = logger;
            _responseHandlers = responseHandler;
            _responseSender = responseSender;
        }

        public async Task Invoke(HttpContext context)
        {
            var slashCommand = (SlashCommandPayloadBody)context.Items[HttpContextKeys.InteractiveSlashCommandsKey];
            var handler = _responseHandlers.FirstOrDefault();

            if (handler == null)
            {
                _logger.LogError("No handler registered for ISlashCommandsHandler");
            }
            else
            {
                _logger.LogInformation($"Handling using {handler.GetType()}");
                try
                {
                    await SendBotEphemeralResponse(slashCommand, $"I've got your request.{Emojis.GetRandomWorkingEmoji()} working on it...");
                    
                    var response = await handler.HandleCommand(slashCommand, async (msg) =>
                    {
                        await SendBotEphemeralResponse(slashCommand, msg);
                    });
                    var responseForEvent = response.Response;

                    if (!string.IsNullOrEmpty(responseForEvent))
                    {
                        var res = response.IsModal.HasValue && response.IsModal.Value ? 
                            (Response) await _responseSender.SendBotResponseInModal(slashCommand.TriggerId,slashCommand.UserId, responseForEvent, response.Blocks):
                            (Response) await _responseSender.SendBotResponse(slashCommand.ChannelId, responseForEvent, response.Blocks, response.Attachments);

                        if (!res.Ok)
                        {
                            _logger.LogError($"Cannot response the slash command \n{res.Error}");
                        }
                    }
                    else
                    {
                        _logger.LogError($"Got empty response for the slash command");
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

        private async Task SendBotEphemeralResponse(SlashCommandPayloadBody slashCommand, string msg)
        {
            await _responseSender.SendBotEphemeralResponse(slashCommand.UserId, slashCommand.ChannelId, msg);
        }

        public static bool ShouldRun(HttpContext ctx)
        {
            return ctx.Items.ContainsKey(HttpContextKeys.InteractiveSlashCommandsKey);
        }
    }
}