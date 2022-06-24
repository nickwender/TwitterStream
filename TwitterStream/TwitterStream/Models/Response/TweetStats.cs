using System.Collections.Generic;

namespace TwitterStream.Models.Response
{
    /// <summary>
    /// Model returned by the /stats REST endpoint.
    /// </summary>
    public class TweetStats
    {
        /// <summary>
        /// Number of tweets we have persisted.
        /// </summary>
        public int TweetsCollected { get; set; }

        /// <summary>
        /// Most used hashtags. The key is the hashtag, the value is the number of tweets it has been used with.
        /// </summary>
        public Dictionary<string, int> TopHashtags { get; set; }
    }
}
