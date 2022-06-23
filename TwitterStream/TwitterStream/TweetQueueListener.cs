using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using Models.Twitter;
using Newtonsoft.Json;
using System;
using System.Threading.Tasks;
using TwitterStream.Repositories;

namespace TwitterStream
{
    /// <summary>
    /// Azure storage queue listener triggered Azure function. The function handles stores tweets to our tweet database.
    /// </summary>
    public class TweetQueueListener
    {
        private readonly ITweetRepository _tweetRepository;

        public TweetQueueListener(ITweetRepository tweetRepository)
        {
            _tweetRepository = tweetRepository;
        }

        [FunctionName("TweetQueueListener")]
        public async Task Run(
            [QueueTrigger("tweets", Connection = "AzureQueueStorageConnectionString")] string tweetJson,
            ILogger log)
        {
            try
            {
                var tweet = JsonConvert.DeserializeObject<TweetObject>(tweetJson);

                var sucess = await _tweetRepository.AddTweet(tweet);

                if (!sucess)
                {
                    var message = $"Failed to add tweet to database tweet: {tweetJson}";
                    log.LogError(message);
                }
            }
            catch (Exception e)
            {
                log.LogError(e, $"Unexpected error processing {tweetJson}");
            }
        }
    }
}
