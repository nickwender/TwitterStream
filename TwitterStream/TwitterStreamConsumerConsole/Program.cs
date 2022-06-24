using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Azure.Storage.Queues;
using TwitterStreamConsumerConsole.Services;
using System;

namespace TwitterStreamConsumerConsole
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var appSettings = LoadAppSettings();
            if (string.IsNullOrWhiteSpace(appSettings.AzureStorageQueueConnectionString))
            {
                Console.WriteLine("No Azure Storage Queue connection string present in appsettings.json");
                return;
            }

            if (string.IsNullOrWhiteSpace(appSettings.TwitterBearerToken))
            {
                Console.WriteLine("No Twitter API bearer token present in appsettings.json");
                return;
            }

            var queueService = CreateQueueService(appSettings.AzureStorageQueueConnectionString);
            var tweetConsumer = new TwitterStreamConsumer(appSettings.TwitterBearerToken, queueService);
            await tweetConsumer.ConsumeTweets();
        }

        private static AppSettings LoadAppSettings()
        {
            var builder = new ConfigurationBuilder().AddJsonFile("appsettings.json", true, true);
            var configuration = builder.Build();
            var appSettings = new AppSettings
            {
                TwitterBearerToken = configuration["AppSettings:TwitterApiBearerToken"],
                AzureStorageQueueConnectionString = configuration["AppSettings:AzureStorageQueueConnectionString"]
            };
            return appSettings;
        }

        private static IQueueService CreateQueueService(string connectionString)
        {
            var queueClient = new QueueClient(connectionString, "tweets");
            var queueService = new QueueService(queueClient);
            return queueService;
        }
    }
}
