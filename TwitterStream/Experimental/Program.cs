using RestSharp;
using System;
using System.Threading.Tasks;
using System.Threading;
using System.Collections.Generic;
using System.Text.Json;

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

    public class TwitterStreamResponse
    {
        public TweetObject Data { get; set; }
    }


    public class TweetObject
    {
        public string Id { get; set; }
        public string Text { get; set; }
        public TweetEntity Entities { get; set; }

    }

    public class TweetEntity
    {
        public IEnumerable<TweetHashtag> Hashtags { get; set; }
    }

    public class TweetHashtag
    {
        public int Start { get; set; }
        public int End { get; set; }
        public string Tag { get; set; }
    }
}
