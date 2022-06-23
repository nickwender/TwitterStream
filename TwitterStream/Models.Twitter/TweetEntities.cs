using System.Collections.Generic;

namespace Models.Twitter
{
    /// <summary>
    /// Based on https://developer.twitter.com/en/docs/twitter-api/data-dictionary/object-model/tweet
    /// </summary>
    public class TweetEntities
    {
        public IEnumerable<TweetHashtag> Hashtags { get; set; }
    }
}
