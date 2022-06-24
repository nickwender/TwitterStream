using System;
using System.Threading.Tasks;
using System.Text.Json;
using Models.Twitter;
using Azure.Storage.Queues;
using System.Text;

namespace TwitterStreamConsumerConsole.Services
{
    public class QueueService : IQueueService
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
