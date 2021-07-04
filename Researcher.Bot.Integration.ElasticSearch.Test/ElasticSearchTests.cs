using Researcher.Bot.Integration.ElasticSearch.Models;
using Researcher.Bot.Tests.Helpers;
using System;
using System.IO;
using System.Linq;
using Xunit;
using Xunit.Abstractions;

namespace Researcher.Bot.Integration.ElasticSearch.Tests
{
    public class ElasticSearchTests : Setup
    {
        public ElasticSearchTests(ITestOutputHelper helper) : base(helper)
        {
        }
        [Fact]        
        public async void SearchLogDog_WithCallID()
        {
            var res = await EsSearchService.GetLogDog( "20a0a2099f3046b0bc619ac05573b7ba","au1");
            Assert.True(res != null);
        }

        [Fact]        
        public async void SearchNocBlamer_WithCallID_NoDc()
        {
            var res = await EsSearchService.GetNocBlamer("9781ca8c59c541389d0d991a24a1c42c");
            Assert.True(res != null);
        } 
        
        [Fact]        
        public async void SearchNocBlamer_WithCallID_WithDc()
        {
            var res = await EsSearchService.GetNocBlamer("9781ca8c59c541389d0d991a24a1c42c", "us1d");
            Assert.True(res != null);
        }

        [Fact]
        public async void GetStatisticsForService()
        {
            var res = await EsSearchService.GetStatisticsForService(new StatsEsRequest
            {
                Dc = "eu1",
                ServiceName = "AccountsService"
            });
            Assert.True(res != null);
        }

        [Fact]
        public async void GetWebhooksComparer_Sent_To_SentToDispatcher_Results_ToCsv()
        {
            var res = await EsSearchService.GetWebhooksComparerSentToSentToDispatcherEvents(new WebhooksEsRequest
            {
                Index = "eu1:*app-logs-2021.06.*",
                ServiceName = "NotificationsService",
                SiteId= "354826790407",
                From= "2021-06-29T03:30:08.955Z",
                To = "2021-06-30T07:20:27.075Z",
                CallbackUrl= "https://api.iqos.com/notification/v1/api/v1/gigya/publish"
            });
            
            await File.WriteAllLinesAsync("results.csv",res.Select(e=>$"{e.Uid},{e.Webhooks.EventType},{e.Webhooks.AccountType},{e.Webhooks.EventID}").ToArray());
            Assert.True(res != null);
        }
    }
}
