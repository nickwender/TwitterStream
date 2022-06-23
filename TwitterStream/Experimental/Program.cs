using RestSharp;
using System;
using System.Threading.Tasks;
using System.Threading;
using System.Text.Json;
using Models.Twitter;

namespace Experimental
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var twitterBaseApiUrl = "https://api.twitter.com/2";
            var twitterStreamEndpoint = "tweets/sample/stream?tweet.fields=entities";
            var twitterBearerToken = "";

            var cancellationToken = new CancellationToken();

            var client = new RestClient(twitterBaseApiUrl);
            client.AddDefaultHeader("Authorization", $"Bearer {twitterBearerToken}");

            try
            {
                var response = client.StreamJsonAsync<TwitterStreamResponse>(twitterStreamEndpoint, cancellationToken);

                await foreach (var tweet in response.WithCancellation(cancellationToken))
                {
                    Console.WriteLine(JsonSerializer.Serialize(tweet));
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
