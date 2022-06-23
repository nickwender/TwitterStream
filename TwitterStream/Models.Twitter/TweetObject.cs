namespace Models.Twitter
{
    /// <summary>
    /// Based on https://developer.twitter.com/en/docs/twitter-api/data-dictionary/object-model/tweet
    /// </summary>
    public class TweetObject
    {
        public string Id { get; set; }
        public string Text { get; set; }
        public TweetEntities Entities { get; set; }
    }
}
