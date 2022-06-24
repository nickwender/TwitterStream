using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using Models.Twitter;
using Newtonsoft.Json;
using System;
using System.Linq;
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

        /// <summary>
        /// An Azure Function triggered when messages are added to an Azure Storage Queue.
        /// </summary>
        /// <param name="tweetJson">JSON serialized tweet object</param>
        /// <param name="log">A logger</param>
        /// <returns></returns>
        [FunctionName("TweetQueueListener")]
        public async Task Run(
            [QueueTrigger("tweets", Connection = "AzureQueueStorageConnectionString")] string tweetJson,
            ILogger log)
        {
            try
            {
                var tweet = JsonConvert.DeserializeObject<TweetObject>(tweetJson);

                if (!IsValid(tweet, log))
                {
                    return;
                }

                var saved = await _tweetRepository.AddTweet(tweet);
                if (!saved)
                {
                    var message = $"Failed to add tweet to database tweet: {tweetJson}";
                    log.LogError(message);
                }
            }
            catch (Exception e)
            {
                log.LogError(e, $"Unexpected error processing {tweetJson}");
                throw; // This ensures the message gets put on the poison queue.
            }
        }

        /// <summary>
        /// Validate that the tweet has all the fields we want to persist.
        /// </summary>
        /// <param name="tweet">The deserialized tweet object</param>
        /// <param name="log">Our logger</param>
        /// <returns>True if we can persist the tweet</returns>
        private bool IsValid(TweetObject tweet, ILogger log)
        {
            if (tweet == null)
            {
                log.LogInformation("Invalid tweet, is null.");
                return false;
            }

            // All tweets must have a Twitter ID for us to store.
            if (string.IsNullOrWhiteSpace(tweet.Id))
            {
                log.LogInformation("Invalid tweet, no Twitter ID.");
                return false;
            }

            // All tweets must have text for us to store.
            if (string.IsNullOrWhiteSpace(tweet.Text))
            {
                log.LogInformation("Invalid tweet, no text.");
                return false;
            }

            // Hashtags are optional, but a null hashtag itself cannot be null / whitespace.
            if (tweet?.Entities?.Hashtags?.Any(h => string.IsNullOrWhiteSpace(h.Tag)) ?? false)
            {
                log.LogInformation("Invalid tweet, has invalid hashtag.");
                return false;
            }

            return true;
        }
    }
}
