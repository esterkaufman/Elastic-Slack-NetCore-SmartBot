using Microsoft.Extensions.Logging;
using Nest;
using Researcher.Bot.Integration.ElasticSearch.Interfaces;
using Researcher.Bot.Integration.ElasticSearch.Models;
using System;
using System.Collections.Generic;
using System.Text;
using Elasticsearch.Net;

namespace Researcher.Bot.Integration.ElasticSearch.Services
{
    public static class ElasticSearchServiceBuilder
    {
        public static IElasticClient Build(IElasticSearchConfig esConfig)
        {
            var settings = CreateConnectionSettings(esConfig);

            AddDefaultMappings(settings);

            var client = new ElasticClient(settings);
            
            //CreateIndex(client, index);
            return client;
        }
        private static ConnectionSettings CreateConnectionSettings(IElasticSearchConfig config)
        {
            return new ConnectionSettings(new Uri(config.Uri))
                .DefaultIndex(config.DefaultIndex);
        }

        private static void AddDefaultMappings(ConnectionSettings settings)
        {
            settings
                .DefaultMappingFor<EsBaseModel>(m => m
                    .PropertyName(request => request.Dc, "@datacenter")
                    .PropertyName(request => request.Timestamp, "@timestamp")
                    .PropertyName(request => request.Message, "@message")
                    .PropertyName(request => request.CallID, "callID")
                    .PropertyName(request => request.Type, "@type")
                    .PropertyName(request => request.Target, "target")
                    .PropertyName(request => request.Host, "@source_host")
                    .PropertyName(request => request.Priority, "priority")
                    .PropertyName(request => request.SiteId, "siteID")
                    .PropertyName(request => request.Endpoint, "endpoint")
                    .PropertyName(request => request.ErrCode, "errCode")
                    .PropertyName(request => request.ErrMessage, "errMessage")
                    .PropertyName(request => request.SpanID, "spanID")
                    .PropertyName(request => request.PspanID, "pspanID")
                    .PropertyName(request => request.Server, "srv")
                    .PropertyName(request => request.Exception, "ex")
                    .PropertyName(request => request.Stats, "stats"))
               .DefaultMappingFor<Webhook>(m => m
                    .PropertyName(serverReq => serverReq.Uid, "uid")
                    .PropertyName(serverReq => serverReq.Webhooks, "webhooks"))
                .DefaultMappingFor<ServerReq>(m => m
                    .PropertyName(serverReq => serverReq.ContentLength, "ContentLength")
                    .PropertyName(serverReq => serverReq.Protocol, "protocol")
                    .PropertyName(serverReq => serverReq.Client, "cln"))
                .DefaultMappingFor<Webhooks>(m => m
                    .PropertyName(cln => cln.AccountType, "accountType")
                    .PropertyName(cln => cln.EventID, "eventID")
                    .PropertyName(cln => cln.EventType, "eventType"))
                .DefaultMappingFor<Server>(m => m
                    .PropertyName(srv => srv.System, "system"))
                .DefaultMappingFor<Client>(m => m
                    .PropertyName(cln => cln.System, "system"))
               .DefaultMappingFor<EsException>(m => m
                    .PropertyName(ex => ex.Message, "message")
                    .PropertyName(ex => ex.Type, "type")
                    .PropertyName(ex => ex.OneWordMessage, "oneWordMessage"))
               .DefaultMappingFor<Stats>(m => m
                    .PropertyName(stats => stats.Hades, "hades")
                    .PropertyName(stats => stats.Total, "total"))
               .DefaultMappingFor<Hades>(m => m
                    .PropertyName(hades => hades.Calls, "calls"))
               .DefaultMappingFor<Total>(m => m
                    .PropertyName(total => total.Time, "time"))
                .DefaultMappingFor<Protocol>(m => m
                    .PropertyName(protocol => protocol.Method, "Method"))
                .DefaultMappingFor<Target>(m => m
                    .PropertyName(target => target.Method, "method")
                    .PropertyName(target => target.Service, "service"))
                .DefaultMappingFor<Params>(m => m
                    .PropertyName(parames => parames.EmailToken, "emailToken")
                    .PropertyName(parames => parames.Email, "email")
                    .PropertyName(parames => parames.Uid, "uid"));
        }


        private static void CreateIndex(IElasticClient client, string indexName)
        {

            var createIndexResponse = client.Indices.Create(indexName,
                index => index
                .Map<EsBaseModel>(x => x.AutoMap())
                .Map<ServerReq>(x => x.AutoMap())
                .Map<Server>(x => x.AutoMap())
                .Map<Client>(x => x.AutoMap())
                .Map<EsException>(x => x.AutoMap())
                .Map<Stats>(x => x.AutoMap())
                .Map<Hades>(x => x.AutoMap())
                .Map<Total>(x => x.AutoMap())
                .Map<Protocol>(x => x.AutoMap())
                .Map<Target>(x => x.AutoMap())
                .Map<Params>(x => x.AutoMap())
            );
        }

    }
}
