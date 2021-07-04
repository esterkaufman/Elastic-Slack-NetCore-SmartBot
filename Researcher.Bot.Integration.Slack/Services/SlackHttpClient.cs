using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Researcher.Bot.Integration.Slack.Interfaces;
using Researcher.Bot.Integration.Slack.Models.Exceptions;
using Researcher.Bot.Integration.Slack.Models.Responses;
using Researcher.Bot.Integration.Slack.Views;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using Researcher.Bot.Integration.Slack.Models.Interactive;
using Researcher.Bot.Integration.Slack.Utils;

namespace Researcher.Bot.Integration.Slack.Services
{
    public class SlackHttpClient : ISlackHttpClient
    {
        private readonly HttpClient _client;
        private readonly ILogger<ISlackHttpClient> _logger;

        public SlackHttpClient(HttpClient client, ILogger<ISlackHttpClient> logger)
        {
            _client = client;
            _logger = logger;
        }

        public async Task<ViewPublishResponse> ViewPublish(ViewPublishRequest view)
        {
            return await _client.PostJson<ViewPublishResponse>(view, view.View.Type == ViewsTypes.Modal? "views.open" : "views.publish", s => _logger.LogInformation(s));
        }


        public async Task<ChatPostMessageResponse> ChatPostMessage(string channel, string text, IBlock[] blocks, Attachment[] attachments)
        {
            var parameters = new List<KeyValuePair<string, string>>
            {
                new KeyValuePair<string, string>("channel", channel),
                new KeyValuePair<string, string>("text", text),
            };

            if (blocks != null && blocks.Any())
                parameters.Add(new KeyValuePair<string, string>("blocks",
                    JsonConvert.SerializeObject(blocks, HttpClientExtensions.JsonSerializerSettings)));

            if (attachments != null && attachments.Any())
                parameters.Add(new KeyValuePair<string, string>("attachments",
                    JsonConvert.SerializeObject(attachments, HttpClientExtensions.JsonSerializerSettings)));
            return await _client.PostParametersAsForm<ChatPostMessageResponse>(parameters, "chat.postMessage", s => _logger.LogInformation(s));
        }

        public async Task<ChatPostMessageResponse> ChatPostEphemeral(string user, string channel, string text, IBlock[] blocks, Attachment[] attachments)
        {
            var parameters = new List<KeyValuePair<string, string>>
            {
                new KeyValuePair<string, string>("channel", channel),
                new KeyValuePair<string, string>("text", text),
                new KeyValuePair<string, string>("user", user),
            };

            if (blocks != null && blocks.Any())
                parameters.Add(new KeyValuePair<string, string>("blocks",
                    JsonConvert.SerializeObject(blocks, HttpClientExtensions.JsonSerializerSettings)));

            if (attachments != null && attachments.Any())
                parameters.Add(new KeyValuePair<string, string>("attachments",
                    JsonConvert.SerializeObject(attachments, HttpClientExtensions.JsonSerializerSettings)));
            return await _client.PostParametersAsForm<ChatPostMessageResponse>(parameters, "chat.postEphemeral", s => _logger.LogInformation(s));
        }
    }
    internal static class HttpClientExtensions
    {
        public static readonly JsonSerializerSettings JsonSerializerSettings = new JsonSerializerSettings
        {
            ContractResolver = new CamelCasePropertyNamesContractResolver(),
            NullValueHandling = NullValueHandling.Ignore
        };

        public static async Task<T> PostJson<T>(this HttpClient httpClient, object payload, string api, Action<string> logger = null) where T : Response
        {
            var serializedObject = JsonConvert.SerializeObject(payload, JsonSerializerSettings);
            var httpContent = new StringContent(serializedObject, Encoding.UTF8, "application/json");
            var request = new HttpRequestMessage(HttpMethod.Post, api)
            {
                Content = httpContent
            };

            var response = await httpClient.SendAsync(request);
            var responseContent = await response.Content.ReadAsStringAsync();

            logger?.Invoke($"{serializedObject}");
            logger?.Invoke($"{response.StatusCode} - {responseContent}");

            if (!response.IsSuccessStatusCode)
            {
                logger?.Invoke(serializedObject);
                logger?.Invoke(responseContent);
            }

            response.EnsureSuccessStatusCode();

            var resObj = JsonConvert.DeserializeObject<T>(responseContent, JsonSerializerSettings);

            if (!resObj.Ok)
            {
                logger?.Invoke(serializedObject);
                logger?.Invoke(resObj.Error);
                throw new SlackApiException(error: $"{resObj.Error}", responseContent: $"{serializedObject}\n\n{responseContent}");
            }

            return resObj;
        }

        public static async Task<T> PostParametersAsForm<T>(this HttpClient httpClient, IEnumerable<KeyValuePair<string, string>> parameters, string api, Action<string> logger = null) where T : Response
        {
            var request = new HttpRequestMessage(HttpMethod.Post, api);
            var requestContent = "";
            
            if (parameters != null && parameters.Any())
            {
                var formUrlEncodedContent = new FormUrlEncodedContent(parameters);
                requestContent = await formUrlEncodedContent.ReadAsStringAsync();
                var httpContent = new StringContent(requestContent, Encoding.UTF8, "application/x-www-form-urlencoded");
                httpContent.Headers.ContentType.CharSet = string.Empty;
                request.Content = httpContent;
            }

            var response = await httpClient.SendAsync(request);
            var responseContent = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                logger?.Invoke($"{response.StatusCode} \n {responseContent}");
            }

            response.EnsureSuccessStatusCode();

            var resObj = JsonConvert.DeserializeObject<T>(responseContent, JsonSerializerSettings);

            if (!resObj.Ok)
                throw new SlackApiException(error: $"{resObj.Error}", responseContent: $"{requestContent}\n\n{responseContent}");

            return resObj;
        }
    }
}
