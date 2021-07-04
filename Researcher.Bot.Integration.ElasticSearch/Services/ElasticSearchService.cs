using Microsoft.Extensions.Logging;
using Nest;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Researcher.Bot.Integration.ElasticSearch.Interfaces;
using Researcher.Bot.Integration.ElasticSearch.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Researcher.Bot.Integration.ElasticSearch.Services
{
    public class ElasticSearchService : IElasticSearchService
    {
        public IElasticSearchConfig _esConfig { get; set; }
        public Indices EsAllIndices { get; set; }
        public ILogger<IElasticSearchService> _logger { get; set; }
        public IElasticClient _elasticClient { get; set; }
        public ElasticSearchService(IElasticClient elasticClient, IElasticSearchConfig esConfig,
            ILogger<ElasticSearchService> logger)
        {
            _logger = logger;
            _esConfig = esConfig;
            _elasticClient = elasticClient;
            EsAllIndices = _esConfig.AllIndices;
        }

        #region Interface Implementaition Methods 
        public async Task<LogDogEsResponse> GetLogDog(string callID, string dc = null)
        {
            _logger.LogInformation($"GetLogDog - {dc ?? "All DCs"},{callID}");
            var response = await SearchLogDogAsync(callID, dc);

            ThrowIfResponseIsInValid(response);

            var first = response.Documents.Last();
            var gateway = response.Documents.First(d => d.Type == "request");

            return new LogDogEsResponse
            {
                Dc = first.Dc,
                CallId = first.CallID,
                Time = gateway.Timestamp.AddMilliseconds(-double.Parse(gateway.Stats.Total.Time)).ToUniversalTime().ToString("o"),
                EndTime = response.Documents.Max(d => d.Timestamp).ToUniversalTime().ToString("o"),
                Duration = $"{gateway.Stats.Total.Time}ms",
                Endpoint = gateway?.Endpoint,
                ErrorCode = gateway.ErrCode,
                ErrorMessage = gateway.ErrMessage,
                ServiceNames = string.Join(",", response.Documents.Select(d => d.Server.System).Distinct()),
                Logs = response.Total,
                Errors = response.Documents.Where(d => !d.Message.StartsWith("Failed creating Flume performance Counters")
                    && !d.Message.StartsWith("Rejecting message on bus ServerNotifications")).Select(d =>
                {

                    if (d.Type == "serverReq" && d.Exception != null)
                    {
                        return $"{d.Server.System}.{d.Target.Method} : {d.Exception.Message} {d.Exception.Type}";
                    }
                    if (new[] { "ERROR", "CRITICAL" }.Contains(d.Priority))
                    {
                        return $"{d.Server.System} : {d.Message.Take(d.Message.IndexOf("\n"))}; {d.Exception?.Message}";
                    }
                    return "";
                }).Where(name => name != "")
            };
        }

        public async Task<BlamerEsResponse> GetNocBlamer(string callID, string dc = null)
        {
            _logger.LogInformation($"GetNocBlamer - {dc ?? "All DCs"},{callID}");

            var response = await SearchNocBlamerAsync(callID, dc);

            ThrowIfResponseIsInValid(response);
            var (success, sortedCalls, endpoint) = SortCalls(response.Documents);

            if (success && sortedCalls != null && sortedCalls.Any())
            {
                var finalErrorRequest = sortedCalls.Last();

                return new BlamerEsResponse
                {
                    Dc = finalErrorRequest.Dc,
                    ServiceName = finalErrorRequest.Server.System,
                    Endpoint = endpoint,
                    ExceptionMessage = finalErrorRequest.Exception.Message,
                    ExceptionType = finalErrorRequest.Exception.Type,
                    OneWordMessage = finalErrorRequest.Exception.OneWordMessage,
                    CallsStack = sortedCalls.Select(GetStoryLikeCall).ToArray(),
                    Time = finalErrorRequest.Timestamp.ToUniversalTime().ToString("u")
                };
            }
            return null;
        }

        public async Task<StatsEsResponse> GetStatisticsForService(StatsEsRequest request)
        {
            var dc = !string.IsNullOrEmpty(request.Dc) ? request.Dc : _esConfig.DefaultIndex;

            _logger.LogInformation($"GetNocBlamer - {dc},{request.ServiceName},{request.From},{request.To}");

            var response = await SearchAsync(dc, request);

            ThrowIfResponseIsInValid(response);
            return new StatsEsResponse
            {
                Dc = request.Dc,
                Time = $"From {request.From} until {request.To}",
                RequestsCount = response.Total,
                ErrorCodeCounts = response.Aggregations.Terms("ErrCodes").Buckets
                    .Select(grp => $"Error Code {grp.Key} occurred {grp.DocCount} times.").AsEnumerable(),
                Endpoints = response.Aggregations.Terms("Endpoints").Buckets
                    .Select(grp => $"Endpoint {grp.Key} occurred {grp.DocCount} times.").AsEnumerable(),
                TargetMethods = response.Aggregations.Terms("TargetMethods").Buckets
                    .Select(grp => $"Target Method {grp.Key} occurred {grp.DocCount} times.").AsEnumerable(),
                HadesCalls = response.Aggregations.Terms("StatsHadesCalls").Buckets
                    .Select(grp => $"Stats.Hades.Calls number {grp.Key} occurred {grp.DocCount} times.").AsEnumerable()
            };
        }

        public async Task<List<Webhook>> GetWebhooksComparerSentToSentToDispatcherEvents(WebhooksEsRequest webhooksEsRequest)
        {
            // Start searching for Sent eventts
            var response = await SearchWebhooksAsync(webhooksEsRequest, "Sent");
            ThrowIfResponseIsInValid(response);

            var documents = new List<Webhook>();

            while (response.Documents.Any())
            {
                documents.AddRange(response.Documents);
                response = await ScrollWebhooksAsync(response.ScrollId);
            }

            // Save the  Sent events in a dictionary, for comparing in next steps
            var sentEventsMapping = SaveSentEventsInCache(documents);

            // Start searching for SentToDispatcher events
            response = await SearchWebhooksAsync(webhooksEsRequest, "SentToDispatcher");
            ThrowIfResponseIsInValid(response);

            // Add to result list the SentToDispatcher events that not exists in dictionary
            var unSentEventList = new List<Webhook>();
            while (response.Documents.Any())
            {
                unSentEventList.AddRange(response.Documents.Where(doc => !sentEventsMapping.Contains(doc.Webhooks.EventID)));
                response = await ScrollWebhooksAsync(response.ScrollId);
            }
            return unSentEventList;

        }
        #endregion

        #region Elastic Searching Methods
        private Task<ISearchResponse<ServerReq>> SearchAsync(string dc, StatsEsRequest request)
        {
            return _elasticClient
                .SearchAsync<ServerReq>(s => s
                    .Index(GetIndexFromDc(dc))
                    .Size(0)
                    .RequestConfiguration(r => r.RequestTimeout(TimeSpan.FromMinutes(5)))
                    .Query(q =>
                        q.Match(m => m.Field(x => x.Type).Query(request.Type)) &&
                        q.Match(m => m.Field(x => x.Server.System).Query(request.ServiceName)) &&
                        +q.DateRange(dr => dr
                            .Field(x => x.Timestamp)
                            .GreaterThanOrEquals(request.From)
                            .LessThanOrEquals(request.To)))
                    .Aggregations(a => a
                        .Terms("ErrCodes", x => x.Field(x => x.ErrCode))
                        .Terms("Endpoints", x => x.Field(x => x.Endpoint))
                        .Terms("TargetMethods", x => x.Field(x => x.Target.Method))
                        .Terms("StatsHadesCalls", x => x.Field(x => x.Stats.Hades.Calls))
                    ));
        }

        private Task<ISearchResponse<EsBaseModel>> SearchNocBlamerAsync(string callID, string dc = null)
        {
            return _elasticClient
                .SearchAsync<EsBaseModel>(s => s
                    .Index(dc == null ? EsAllIndices : GetIndexFromDc(dc))
                    .Size(_esConfig.MaxCallIDsSize)
                    .RequestConfiguration(r => r.RequestTimeout(TimeSpan.FromMinutes(5)))
                    .Query(q =>
                        q.Match(m => m.Field("callID").Query(callID)) &&
                        !q.Match(m => m.Field("@type").Query("log")) &&
                        !q.Match(m => m.Field("@type").Query("providerCall"))
                    ));
        }

        private Task<ISearchResponse<EsBaseModel>> SearchLogDogAsync(string callID, string dc = null)
        {
            return _elasticClient
                .SearchAsync<EsBaseModel>(s => s
                    .Index(dc == null ? EsAllIndices : GetIndexFromDc(dc))
                    .Size(_esConfig.MaxCallIDsSize)
                    .RequestConfiguration(r => r.RequestTimeout(TimeSpan.FromMinutes(5)))
                    .Query(q =>
                        q.Match(m => m.Field("callID").Query(callID))
                    ));
        }
        private Task<ISearchResponse<Webhook>> SearchWebhooksAsync(WebhooksEsRequest request, string eventStatus)
        {
            return _elasticClient
                .SearchAsync<Webhook>(s => s
                    .Index(request.Index)
                    .Size(9000)
                    .RequestConfiguration(r => r.RequestTimeout(TimeSpan.FromMinutes(5)))
                    .Query(q =>
                        q.Match(m => m.Field(x => x.Type).Query(request.Type)) &&
                        q.Match(m => m.Field(x => x.Server.System).Query(request.ServiceName)) &&
                        q.Match(m => m.Field("siteID").Query(request.SiteId)) &&
                        q.Match(m => m.Field("webhooks.callbackUrl").Query(request.CallbackUrl)) &&
                        q.Match(m => m.Field("webhooks.eventStatus").Query(eventStatus)) &&
                        +q.DateRange(dr => dr
                            .Field(x => x.Timestamp)
                            .GreaterThan(request.From)
                            .LessThan(request.To)))
                    .Scroll("10s"));
        }
        #endregion

        #region Private Methods

        private void ThrowIfResponseIsInValid(ISearchResponse<object> response)
        {
            if (!response.IsValid || response.ServerError?.Error != null || response.OriginalException != null)
            {
                throw new Exception(response.DebugInformation, response.OriginalException);
            }
            if (response.Total <= 0)
            {
                throw new Exception($"No data were found in logs, for the given parameter!");
            }
        }

        private Task<ISearchResponse<Webhook>> ScrollWebhooksAsync(string scrollId)
        {
            return _elasticClient.ScrollAsync<Webhook>("10s", scrollId);
        }

        private string GetIndexFromDc(string dc)
        {
            return dc.ToLower().Contains("il") ? "il0:il1-*" : dc.ToLower().Equals("us1") ? "us1d:us1d-*" : $"{dc}:{dc}-*";
        }
        private (bool success, IList<EsBaseModel> callsStack, string endpoint) SortCalls(IReadOnlyCollection<EsBaseModel> documents)
        {
            var errorCalls = documents.Where(d => int.TryParse(d.ErrCode, out var errorCode) && errorCode >= 500000).ToList();
            var gatewayCall = errorCalls.FirstOrDefault(q => q.Type == "request");

            if (gatewayCall == null)
            {
                return (false, null, "");
            }

            // Get the first clientReq the gator called
            var callsStack = new List<EsBaseModel> { gatewayCall };
            var head = errorCalls.First(q => q.Type == "clientReq" && q.PspanID == gatewayCall.SpanID);

            do
            {
                callsStack.Add(head);
                // Each clientReq has a following serverReq
                var serverReq = errorCalls.FirstOrDefault(q => q.Type == "serverReq" && q.PspanID == head.PspanID);

                if (serverReq != null)
                {
                    callsStack.Add(serverReq);
                }

                // Move head to the next clientReq, by matching the next clientReq.PspanID to current clientReq.spanID
                head = errorCalls.FirstOrDefault(q => q.Type == "clientReq" && q.PspanID == head.SpanID);
            } while (head != null);

            return (true, callsStack, gatewayCall.Endpoint);
        }
        private string GetStoryLikeCall(EsBaseModel call)
        {
            var isClient = call.Type == "clientReq";
            //9:48:16.539 Gator calls to ApplicationService.Create, client time: 30075.151ms
            //9:48:16.541 ApplicationService received a call for method Create, time: 30073.662ms
            return $"{call.Timestamp.ToUniversalTime().TimeOfDay:g} {call.Server?.System} {(isClient ? "calls to" : "received a call")} {(isClient ? call.Target?.Service : "")}{(call.Target?.Method != null ? $"{(isClient ? "." : " for method ")}{call.Target?.Method}" : "")}, *{(isClient ? "client " : "")}time: {call.Stats?.Total.Time}ms*";
        }

        private HashSet<string> SaveSentEventsInCache(List<Webhook> documents)
        {
            var sentEventsMapping = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            foreach (var evt in documents.Where(evt => !sentEventsMapping.Contains(evt.Webhooks.EventID)))
            {
                sentEventsMapping.Add(evt.Webhooks.EventID);
            }

            return sentEventsMapping;
        }

        #endregion

        private Task<ISearchResponse<dynamic>> SearchRawQueryAsync(string query)
        {
            var kibanaQr = "srv.system:UserManagement AND @type:serverReq";
            var parts = kibanaQr.Split(" ");
            var obj = new JObject();
            obj["query"] = new JObject();
            obj["query"]["match"] = new JObject();
            obj["size"] = 0;

            parts.Where(p => p.Contains(":")).Select(p => p.Split(":")).All(p =>
            {
                obj["query"]["match"][p[0]] = p[1];
                return true;
            });
            query = "{     match\": { \"@type\" : \"serverReq\",\"srv.system\" : \"UserManagement\"}        }";


            var dynamicRes = _elasticClient.SearchAsync<dynamic>(s => s
                .RequestConfiguration(r => r.RequestTimeout(TimeSpan.FromMinutes(5)))
                .AllIndices()
                .MatchAll()
                .IgnoreUnavailable()
                .Size(0)
                .Query(q =>
                    //q.QueryString(d => d.Query(query))
                    q.Match(c => c.Field("some_field").Query("query"))
                )
                .Aggregations(a => a
                    .Terms("ErrCodes", x => x.Field("errCode"))
                    .Terms("Endpoints", x => x.Field("endpoint"))
                    .Terms("TargetMethods", x => x.Field("target.method"))
                    .Terms("StatsHadesCalls", x => x.Field("stats.hades.calls"))
                ));
            return dynamicRes;
        }
    }
}