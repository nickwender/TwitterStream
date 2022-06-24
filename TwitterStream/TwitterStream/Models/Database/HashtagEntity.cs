namespace TwitterStream.Models.Database
{
    /// <summary>
    /// Model we retrieve from the database for a tweet hashtag.
    /// </summary>
    public class HashtagEntity 
    {
        public int Id { get; set; }
        public string Hashtag { get; set; }
    }
}
