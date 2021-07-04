using System;
using System.Collections.Generic;
using System.Text;

namespace Researcher.Bot.Integration.ElasticSearch.Interfaces
{
    public interface IElasticSearchConfig
    {
        string Uri { get; set; }
        string DefaultIndex { get; set; }
        string[] AllIndices { get; set; }
        int MaxDocuments { get; set; }
        int MaxCallIDsSize { get; set; }
    }
}
