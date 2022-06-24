using RestSharp;
using System;
using System.Threading.Tasks;
using System.Threading;
using Models.Twitter;

namespace TwitterStreamConsumerConsole.Services
{
    public class TwitterStreamConsumer : ITwitterStreamConsumer
    {
        private const string twitterBaseApiUrl = "https://api.twitter.com/2";
        private const string twitterStreamEndpoint = "tweets/sample/stream?tweet.fields=entities";
        private readonly RestClient _restClient;
        private readonly IQueueService _queueService;

        public TwitterStreamConsumer(string twitterBearerToken, IQueueService queueService)
        {
            _restClient = new RestClient(twitterBaseApiUrl);
            _restClient.AddDefaultHeader("Authorization", $"Bearer {twitterBearerToken}");
            _queueService = queueService;
        }

        public async Task ConsumeTweets()
        {
            Console.WriteLine("About to start consuming tweets. Kill the program to stop.");
            while (true)
            {
                try
                {
                    var cancellationToken = new CancellationToken();
                    var response = _restClient.StreamJsonAsync<TwitterStreamResponse>(twitterStreamEndpoint, cancellationToken);

                    await foreach (var tweet in response.WithCancellation(cancellationToken))
                    {
                        await _queueService.QueueTweet(tweet.Data);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Exception while streaming tweets");
                    Console.WriteLine(ex.ToString());
                }
            }
        }
    }
}
