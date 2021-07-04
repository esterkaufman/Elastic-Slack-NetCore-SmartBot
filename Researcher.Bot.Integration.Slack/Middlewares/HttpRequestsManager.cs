using System;
using System.IO;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Researcher.Bot.Integration.Slack.Extensions;
using Researcher.Bot.Integration.Slack.Interfaces;
using Researcher.Bot.Integration.Slack.Models.Events;
using Researcher.Bot.Integration.Slack.Models.Interactive;
using Researcher.Bot.Integration.Slack.Utils;

namespace Researcher.Bot.Integration.Slack.Middlewares
{
    public class HttpRequestsManager
    {
        private readonly RequestDelegate _next;
        private readonly ISlackSecuredStore _tokenStore;
        private readonly ILogger<HttpRequestsManager> _logger;

        public HttpRequestsManager(RequestDelegate next, ISlackSecuredStore tokenStore,
            ILogger<HttpRequestsManager> logger)
        {
            _next = next;
            _tokenStore = tokenStore;
            _logger = logger;
        }

        public async Task Invoke(HttpContext context)
        {
            try
            {
                if (context.Request.Path.Value.ToLower().Contains("ping"))
                {
                    context.Response.StatusCode = 200;
                    context.Response.ContentType = "application/json";
                    await context.Response.WriteAsync(JsonConvert.SerializeObject(new {Ok = "Server is up"}));
                    return;
                }

                context.Request.EnableBuffering();
                using (var reader =
                    new StreamReader(context.Request.Body, Encoding.UTF8, false, leaveOpen: true))
                {
                    var body = await reader.ReadToEndAsync();
                    
                    VerifyRequest(context.Request, body);
                    if (body.StartsWith("{"))
                    {
                        var jObject = JObject.Parse(body);

                        _logger.LogInformation($"Received events API payload: {jObject}");
                        if (jObject.ContainsKey("challenge"))
                        {
                            context.Items.Add(HttpContextKeys.ChallengeKey, jObject["challenge"]);
                        }
                        else
                        {
                            var metadata = JsonConvert.DeserializeObject<SlackApiMetaData>(body);
                            BaseEventBody slackEventBody = null;

                            if (jObject["event"] is JObject @event)
                            {
                                slackEventBody = ToEventType(@event, body);
                            }
                            else if (GetEventType(jObject) == SlackEventKeys.AppHome)
                            {
                                slackEventBody = ToEventType(jObject, body);
                            }

                            if (slackEventBody != null && slackEventBody.Bot_Profile == null)
                            {
                                context.Items.Add(HttpContextKeys.ApiMetadataKey, metadata);
                                context.Items.Add(HttpContextKeys.EventBodyKey, slackEventBody);
                                context.Items.Add(HttpContextKeys.EventTypeKey, slackEventBody.Type);
                            }
                        }
                    }
                    // https://api.slack.com/interactivity/handling#payloads                    
                    else if (body.StartsWith("payload="))
                    {
                        var payloadJsonUrlEncoded = body.Remove(0, 8);
                        var decodedJson = WebUtility.UrlDecode(payloadJsonUrlEncoded);
                        _logger.LogInformation($"Received interactivity payload: {decodedJson}");
                        var payload = JObject.Parse(decodedJson);
                        var interactivePayloadTyped = ToInteractiveType(payload, body);
                        
                        context.Items.Add(HttpContextKeys.InteractivePayloadKey, interactivePayloadTyped);
                    }
                    // https://api.slack.com/interactivity/slash-commands#creating_commands                    
                    else if (body.Contains("&command="))
                    {
                        _logger.LogInformation($"Received slash-commands payload: {WebUtility.UrlDecode(body)}");
                        var slashCommandPayload = new SlashCommandPayloadBody(body);

                        context.Items.Add(HttpContextKeys.InteractiveSlashCommandsKey, slashCommandPayload);
                    }

                    context.Request.Body.Position = 0;
                }
                _next(context);

                await context.Response.WriteSuccessInResponse();
            }
            catch (Exception e)
            {
                _logger.LogError(e, $"Error in HttpRequestsmanager: {e.Message}");
                await context.Response.WriteErrorInResponse(e);
            }
        }

        private void VerifyRequest(HttpRequest request, string body)
        {
            var signingSecret = _tokenStore.GetSecretKey();
            var verifier = new RequestVerifier(signingSecret);
            var verified = verifier.Verify(request.Headers[RequestVerifier.SignatureHeaderName],
                long.Parse(request.Headers[RequestVerifier.TimestampHeaderName]), body, out string sig);

            if (!verified)
            {
                throw new Exception(
                    $"Auth Error: invalid request, verifing the requests faild,params:{request.Headers[RequestVerifier.SignatureHeaderName]},{request.Headers[RequestVerifier.TimestampHeaderName]},{sig}");
            }
        }

        private static BaseEventBody ToEventType(JObject eventJson, string raw)
        {
            var eventType = GetEventType(eventJson);

            switch (eventType)
            {
                case SlackEventKeys.AppMention:
                case SlackEventKeys.ImMessage:
                    return eventJson.ToObject<AppMentionEventBody>();
                case SlackEventKeys.AppHome:
                    return eventJson.ToObject<AppHomeEventBody>();
                default:
                    UnknownEventBody unknownSlackEvent = eventJson.ToObject<UnknownEventBody>();
                    unknownSlackEvent.RawJson = raw;
                    return unknownSlackEvent;
            }
        }

        private static InteractionPayloadBody ToInteractiveType(JObject payloadJson, string raw)
        {
            var eventType = GetEventType(payloadJson);

            switch (eventType)
            {
                case InteractionTypes.ViewSubmission:
                    var viewSubmission = payloadJson.ToObject<ViewSubmission>();

                    var view = payloadJson["view"] as JObject;
                    var viewState = view["state"] as JObject;
                    
                    viewSubmission.ViewId = view.Value<string>("id");
                    viewSubmission.ViewState = viewState;
                    return viewSubmission;
                case InteractionTypes.BlockActions:
                    return payloadJson.ToObject<BlockActionInteraction>();
                case InteractionTypes.MessageAction:
                    return payloadJson.ToObject<MessageActionInteraction>();
                default:
                    var unknownSlackEvent = payloadJson.ToObject<UnknownInteractiveMessage>();
                    unknownSlackEvent.RawJson = raw;
                    return unknownSlackEvent;
            }
        }

        public static string GetEventType(JObject eventJson)
        {
            if (eventJson != null)
            {
                return eventJson["type"].Value<string>();
            }

            return "unknown";
        }
    }
}