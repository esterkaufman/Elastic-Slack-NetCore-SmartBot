using Newtonsoft.Json;

namespace Researcher.Bot.Integration.ElasticSearch.Models
{
    public abstract class EsBaseResponse {
        public string ToJson()
        {            
            return JsonConvert.SerializeObject(this, Formatting.Indented);
        }
    }
}
