using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using Researcher.Bot.Integration.Slack.Models.Exceptions;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Researcher.Bot.Integration.Slack.Utils;

namespace Researcher.Bot.Integration.Slack.Extensions
{
    public static class DebugExtensions
    {
        public static Task WriteErrorInResponse(this HttpResponse response, Exception e) 
        {
            var res = new
            {
                response_type = "ephemeral",
                text = $"I've got an error,:worried: when trying to parse your request. adding some details: \nError=Internal Error\nMessage={e.Message}"
            };

            if (e is SlackApiException @slackEx)
            {
                res = new
                {
                    response_type = "ephemeral",
                    text =
                        $"I've got an error,:worried: when trying to parse your request. adding some details: \nError={@slackEx.Error}\nMessage={@slackEx.ResponseContent}"
                };
            }
            response.ContentType = "application/json";
            return response.WriteAsync(JsonConvert.SerializeObject(res));
        }

        public static Task WriteSuccessInResponse(this HttpResponse response)
        {
            response.StatusCode = 200;
            response.ContentType = "application/json";
            return response.WriteAsync(JsonConvert.SerializeObject(new
            {
                response_type = "ephemeral",
                text = $"I've got your request.{Emojis.GetRandomWorkingEmoji()} working on it..."
            }));
        }

    }
}
