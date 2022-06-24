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
            // Setup a rest client for the Twitter API.
            _restClient = new RestClient(twitterBaseApiUrl);
            _restClient.AddDefaultHeader("Authorization", $"Bearer {twitterBearerToken}");
            _queueService = queueService;
        }

        public async Task ConsumeTweets()
        {
            // TODO: Inject a logger in and log instead of writing to the console.
            Console.WriteLine("About to start consuming tweets. Kill the program to stop.");

            // For simplicity, this while (true) will keep the console app streaming tweets in the face of errors.
            // In practice, I am temtped to remove it and run the console app in a docker container / pod.
            // Configured to start a new container / pod on failure.
            // If it fails too often, make it more robust.
            while (true)
            {
                try
                {
                    // In theory, we could take in a CancellationToken and allow callers to stop tweet consumption.
                    var cancellationToken = new CancellationToken();

                    // Start streaming tweets.
                    var response = _restClient.StreamJsonAsync<TwitterStreamResponse>(twitterStreamEndpoint, cancellationToken);
                    await foreach (var tweet in response.WithCancellation(cancellationToken))
                    {
                        // Add the tweet to our queue.
                        await _queueService.QueueTweet(tweet.Data);
                    }
                }
                catch (Exception ex)
                {
                    // TODO: Again, inject a logger and stop using the console.
                    Console.WriteLine("Exception while streaming tweets");
                    Console.WriteLine(ex.ToString());
                }
            }
        }
    }
}
