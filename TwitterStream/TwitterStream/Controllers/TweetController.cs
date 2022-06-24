using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using TwitterStream.Repositories;
using System.Net.Http;
using Newtonsoft.Json.Serialization;

namespace TwitterStream.Controllers
{
    /// <summary>
    /// TweetsController has an HTTP trigger that provides statistics about tweets consumed.
    /// </summary>
    public class TweetController 
    {
        private readonly ITweetRepository _tweetRepository;
        private readonly JsonSerializerSettings _jsonSerializerSettings;

        public TweetController(ITweetRepository tweetRepository)
        {
            _tweetRepository = tweetRepository;
            _jsonSerializerSettings = new JsonSerializerSettings
            {
                ContractResolver = new CamelCasePropertyNamesContractResolver()
            };
        }

        /// <summary>
        /// HTTP trigger Azure Function that returns statistics about tweets consumed.
        /// </summary>
        /// <param name="req"></param>
        /// <param name="log"></param>
        /// <returns></returns>
        [FunctionName("Stats")]
        public async Task<IActionResult> Stats(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = null)] HttpRequestMessage req,
            ILogger log)
        {
            var stats = await _tweetRepository.GetStats();

            return new OkObjectResult(JsonConvert.SerializeObject(stats, _jsonSerializerSettings));
        }
    }
}
