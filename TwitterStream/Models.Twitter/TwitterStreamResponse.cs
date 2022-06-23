namespace Models.Twitter
{
    /// <summary>
    /// Based on https://developer.twitter.com/en/docs/twitter-api/tweets/volume-streams/quick-start/sampled-stream
    /// </summary>
    public class TwitterStreamResponse
    {
        public TweetObject Data { get; set; }
    }
}
