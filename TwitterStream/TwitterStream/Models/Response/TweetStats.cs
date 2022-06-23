using System.Collections.Generic;

namespace TwitterStream.Models.Response
{
    public class TweetStats
    {
        public int TweetsCollected { get; set; }
        public Dictionary<string, int> TopHashtags { get; set; }
    }
}
