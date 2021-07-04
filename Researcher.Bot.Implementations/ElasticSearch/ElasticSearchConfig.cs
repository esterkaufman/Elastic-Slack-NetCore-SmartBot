using Microsoft.Extensions.Configuration;
using Researcher.Bot.Integration.ElasticSearch.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace Researcher.Bot.Implementations.ElasticSearch
{
    public class ElasticSearchConfig : IElasticSearchConfig
    {

        public ElasticSearchConfig(IConfiguration configuration)
        {
            if (configuration != null)
            {
                Uri = configuration["elasticsearch:url"];
                DefaultIndex = configuration["elasticsearch:index"];
            }
        }
        public ElasticSearchConfig(string uri)
        {
            Uri = uri;
        }

        public string Uri { get; set; }
        public string[] AllIndices { get; set; } =  {
            "us1d:us1d-*",
            "eu1:eu1-*",
            "cn1e:cn1e-*",
            "ru1e:ru1e-*",
            "au1:au1-*",
            "il0:il1-*",
            "eu2a:eu2a-*",
        };
        public string DefaultIndex { get; set; }
        public int MaxDocuments { get; set; } = 1000;
        public int MaxCallIDsSize { get; set; } = 300;
    }
}
