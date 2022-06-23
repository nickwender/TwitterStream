using RestSharp;
using System;
using System.Threading.Tasks;
using System.Threading;
using System.Text.Json;
using Models.Twitter;
using Microsoft.Extensions.Configuration;
using Azure.Storage.Queues;
using System.Text;

namespace Experimental
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var twitterBaseApiUrl = "https://api.twitter.com/2";
            var twitterStreamEndpoint = "tweets/sample/stream?tweet.fields=entities";

            var builder = new ConfigurationBuilder().AddJsonFile("appsettings.json", true, true);
            var configuration = builder.Build();

            var twitterBearerToken = configuration["AppSettings:TwitterApiBearerToken"];

            var azureStorageQueueConnectionString = configuration["AppSettings:AzureStorageQueueConnectionString"];
            var queueClient = new QueueClient(azureStorageQueueConnectionString, "tweets");
            var queueService = new QueueService(queueClient);

            var cancellationToken = new CancellationToken();

            var client = new RestClient(twitterBaseApiUrl);
            client.AddDefaultHeader("Authorization", $"Bearer {twitterBearerToken}");

            try
            {
                var response = client.StreamJsonAsync<TwitterStreamResponse>(twitterStreamEndpoint, cancellationToken);

                await foreach (var tweet in response.WithCancellation(cancellationToken))
                {
                    await queueService.QueueTweet(tweet.Data);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Exception while streaming tweets");
                Console.WriteLine(ex.ToString());
            }
        }
    }

    public class QueueService
    {
        private readonly QueueClient _queueClient;

        public QueueService(QueueClient queueClient)
        {
            _queueClient = queueClient;
        }

        public async Task QueueTweet(TweetObject tweet)
        {
            var serializedTweet = JsonSerializer.Serialize(tweet);
            Console.WriteLine($"Sending tweet to queue: {serializedTweet}");

            // Azure storage queues can take arbitrary string messages.
            // However, queue triggered Azure Function need the message to be base64 encoded.
            var base64EncodedTweet = Convert.ToBase64String(Encoding.UTF8.GetBytes(serializedTweet));
            await _queueClient.SendMessageAsync(base64EncodedTweet);
        }
    }
}
