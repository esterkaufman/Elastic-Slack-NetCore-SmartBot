using Microsoft.Extensions.Logging;
using Researcher.Bot.Integration.Slack.Interfaces;
using System;
using System.Net.Http;
using System.Net.Http.Headers;

namespace Researcher.Bot.Integration.Slack.Services
{
    public class SlackHttpClientBuilder : ISlackHttpClientBuilder
    {
        private readonly ILoggerFactory _loggerFactory;
        private readonly IHttpClientFactory _factory;

        public SlackHttpClientBuilder(ILoggerFactory loggerFactory, IHttpClientFactory factory)
        {
            _loggerFactory = loggerFactory;
            _factory = factory;
        }

        public ISlackHttpClient Build(string token)
        {
            var c = _factory.CreateClient();
            ConfigureHttpClient(c, token);
            return new SlackHttpClient(c, _loggerFactory.CreateLogger<ISlackHttpClient>());
        }

        public static void ConfigureHttpClient(HttpClient c, string token)
        {
            ConfigureHttpClient(c);
            c.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }

        public static void ConfigureHttpClient(HttpClient c)
        {
            c.BaseAddress = new Uri("https://slack.com/api/");
            c.Timeout = TimeSpan.FromSeconds(15);
        }
    }
}
