using Researcher.Bot.Integration.ElasticSearch.Models;
using Researcher.Bot.Tests.Helpers;
using System;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Researcher.Bot.Implementations.Slack;
using Researcher.Bot.Integration.Slack.Models.Events;
using Xunit;
using Xunit.Abstractions;

namespace Researcher.Bot.Integration.ElasticSearch.Tests
{
    public class SlackApiTests : Setup
    {
        public static readonly JsonSerializerSettings JsonSerializerSettings = new JsonSerializerSettings
        {
            ContractResolver = new CamelCasePropertyNamesContractResolver(),
            NullValueHandling = NullValueHandling.Ignore
        };

        public SlackApiTests(ITestOutputHelper helper) : base(helper)
        {
        }
        [Fact]
        public async void AppHomeHandler_Handle()
        {
            var payload = await new AppHomeHandler().Handle(null, new AppHomeEventBody { User = "ester" });
            var res = JsonConvert.SerializeObject(payload, JsonSerializerSettings);

            Assert.True(res != null);
        }

        [Fact]
        public async void ResearcherBotRequestHandler_HandleCommand()
        {
            var payload = await new ResearcherBotRequestHandler( ElasticSearchHandler,Logger).HandleCommand(new SlashCommandPayloadBody
            {
                Text = "au1+20a0a2099f3046b0bc619ac05573b7ba",
                Command = "/logdog"
            });
            var res = JsonConvert.SerializeObject(payload, JsonSerializerSettings);

            Assert.True(res != null);
        } 
        [Fact]
        public async void ResearcherBotRequestHandler_Handle()
        {
            var payload = await new ResearcherBotRequestHandler( ElasticSearchHandler,Logger).Handle(null,new AppMentionEventBody()
            {
                Text = "*Get stats eu1 AccountsService*",
            });
            var res = JsonConvert.SerializeObject(payload, JsonSerializerSettings);

            Assert.True(res != null);
        }


    }
}
