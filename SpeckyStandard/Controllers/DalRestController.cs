using Newtonsoft.Json;
using SpeckyStandard.DI;
using System.Net;

namespace SpeckyStandard.Controllers
{
    public class DalRestController
    {
        public T Get<T>(string url)
        {
            var webClient = SpeckContainer.Instance.GetInstance<WebClient>();
            var json = webClient.DownloadString(url);
            return JsonConvert.DeserializeObject<T>(json);
        }
    }
}
