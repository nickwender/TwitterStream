using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Options;
using Models.Twitter;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TwitterStream.Models.Database;
using TwitterStream.Models.Response;

namespace TwitterStream.Repositories
{
    /// <summary>
    /// Repository to persist tweets.
    /// </summary>
    public class TweetRepository : DisposableRepository, ITweetRepository
    {
        public TweetRepository(IOptions<Configuration> configuration) : base(configuration)
        {
        }

        /// <summary>
        /// Try to persist a tweet to the database.
        /// </summary>
        /// <param name="tweet">The tweet to persist.</param>
        /// <returns>True if the tweet was persisted.</returns>
        public async Task<bool> AddTweet(TweetObject tweet)
        {
            // TODO: Use Unit of Work pattern to make rollbacks on error easier.
            // TODO: As is, this could orphan a tweet <-> hashtag association.
            // TODO: Use an ORM (Dapper, EF, ServiceStack.ORMLite, etc) instead of SqlCommand.
            // TODO: Inject a logger into the repository and log when inserts fail.

            // Insert the tweet (text and Twitter's ID).
            var tweetId = await InsertTweet(tweet);
            if (!tweetId.HasValue)
            {
                return false;
            }

            // Check if the tweet has hashtag to insert.
            if (tweet.Entities?.Hashtags?.Any() ?? false)
            {
                // Insert each hashtag of the tweet.
                var hashtagIds = await InsertHashtags(tweet.Entities.Hashtags);
                if (hashtagIds.Any(id => !id.HasValue))
                {
                    return false;
                }

                // Associated the hashtags with the tweet.
                foreach (var hashtagId in hashtagIds)
                {
                    var tweetHashtagIds = await InsertTweetHashtag(tweetId.Value, hashtagId.Value);
                    if (!tweetHashtagIds.HasValue)
                    {
                        return false;
                    }
                }
            }

            return true;
        }

        /// <summary>
        /// Get stats about the tweets.
        /// </summary>
        /// <returns></returns>
        public async Task<TweetStats> GetStats()
        {
            var stats = new TweetStats()
            {
                TweetsCollected = 0,
                TopHashtags = new Dictionary<string, int>()
            };

            try
            {
                // An unfortunate limit of this implementation is that the structure of the stats returned is tightly
                // coupled with the queries below. It would be reasonable to have methods to get tweets and their
                // associated hashtags and build up the stats in a service layer. That would take the business logic
                // out of the queries.

                // Get the top 10 most-used hashtags.
                var query = "SELECT TOP(10) COUNT(th.TweetId) AS HashtagCount, h.Hashtag AS Hashtag"
                          + " FROM Hashtags h WITH (NOLOCK)"
                          + " JOIN TweetHashtags th WITH (NOLOCK)"
                          + " ON th.HashtagId = h.Id"
                          + " GROUP BY h.Hashtag"
                          + " ORDER BY COUNT(th.TweetId) DESC";
                using SqlCommand top10Cmd = new SqlCommand(query, _connection);
                {
                    using (var reader = await top10Cmd.ExecuteReaderAsync())
                    {
                        if (reader.HasRows)
                        {
                            while (await reader.ReadAsync())
                            {
                                var hashtag = Convert.ToString(reader["Hashtag"]);
                                var count = Convert.ToInt32(reader["HashtagCount"]);
                                stats.TopHashtags.Add(hashtag, count);
                            }
                        }
                    }
                }

                // Get the number of tweets we have on hand.
                var tweetCountQuery = "SELECT COUNT(1) AS TweetCount FROM Tweets WITH (NOLOCK)";
                using SqlCommand tweetCountCmd = new SqlCommand(tweetCountQuery, _connection);
                {
                    using (var reader = await tweetCountCmd.ExecuteReaderAsync())
                    {
                        if (reader.HasRows)
                        {
                            while (await reader.ReadAsync())
                            {
                                var tweetCount = Convert.ToInt32(reader["TweetCount"]);
                                stats.TweetsCollected = tweetCount;
                            }
                        }
                    }
                }
            }
            catch (Exception)
            {
                // TODO: Inject a logger into the repository and log this exception.
            }

            return stats;
        }

        /// <summary>
        /// Insert the tweet's text and Twitter's ID.
        /// </summary>
        /// <param name="tweet">The tweet to insert.</param>
        /// <returns>The ID of the record inserted or null if an exception occured.</returns>
        private async Task<int?> InsertTweet(TweetObject tweet)
        {
            var query = "INSERT INTO Tweets (TwitterTweetId, Text) OUTPUT INSERTED.ID VALUES(@twitterId, @text)";
            using SqlCommand cmd = new SqlCommand(query, _connection);
            {
                cmd.Parameters.AddWithValue("@twitterId", tweet.Id);
                cmd.Parameters.AddWithValue("@text", tweet.Text);

                return await Insert(cmd);
            }
        }

        /// <summary>
        /// Insert hashtags of a tweet without duplicating existing hashtag records.
        /// </summary>
        /// <param name="hashtags"></param>
        /// <returns>List of hashtag IDs used by the tweet.</returns>
        private async Task<IEnumerable<int?>> InsertHashtags(IEnumerable<TweetHashtag> hashtags)
        {
            var hashtagIds = new List<int?>();

            foreach (var hashtag in hashtags)
            {
                var existingHashtag = await GetHashtag(hashtag.Tag);

                if (existingHashtag != null)
                {
                    hashtagIds.Add(existingHashtag.Id);
                }
                else
                {
                    hashtagIds.Add(await InsertHashtag(hashtag.Tag));
                }
            }

            return hashtagIds;
        }

        /// <summary>
        /// Insert a hashtag.
        /// </summary>
        /// <param name="hashtag">The hashtag to insert.</param>
        /// <returns>The ID of the new record.</returns>
        private async Task<int?> InsertHashtag(string hashtag)
        {
            var query = "INSERT INTO Hashtags (Hashtag) OUTPUT INSERTED.ID VALUES(@hashtag)";
            using SqlCommand cmd = new SqlCommand(query, _connection);
            {
                cmd.Parameters.AddWithValue("@hashtag", hashtag);
                return await Insert(cmd);
            }
        }

        /// <summary>
        /// Retrieve a hashtag entity by hashtag text.
        /// </summary>
        /// <param name="hashtag">Text of the hashtag to find</param>
        /// <returns>The hashtag and our ID for it.</returns>
        private async Task<HashtagEntity> GetHashtag(string hashtag)
        {
            HashtagEntity hashtagEntity = null;

            var selectQuery = "SELECT Id, Hashtag FROM Hashtags WHERE Hashtag = @hashtag";
            using SqlCommand selectCmd = new SqlCommand(selectQuery, _connection);
            {
                selectCmd.Parameters.AddWithValue("@hashtag", hashtag);
                using (var reader = await selectCmd.ExecuteReaderAsync())
                {
                    if (reader.HasRows)
                    {
                        await reader.ReadAsync();

                        try
                        {
                            hashtagEntity = new HashtagEntity
                            {
                                Id = Convert.ToInt32(reader["Id"]),
                                Hashtag = Convert.ToString(reader["Hashtag"])
                            };
                        }
                        catch (Exception)
                        {
                            // TODO: Inject a logger into the repository and log this exception.
                            return null;
                        }
                    }
                }
            }

            return hashtagEntity;
        }

        /// <summary>
        /// Associate a tweet with a hashtag.
        /// </summary>
        /// <param name="tweetId"></param>
        /// <param name="hashtagId"></param>
        /// <returns></returns>
        private async Task<int?> InsertTweetHashtag(int tweetId, int hashtagId)
        {
            var query = "INSERT INTO TweetHashtags (TweetId, HashtagId) OUTPUT INSERTED.ID VALUES(@tweetId, @hashtagId)";
            using SqlCommand cmd = new SqlCommand(query, _connection);
            {
                cmd.Parameters.AddWithValue("@tweetId", tweetId);
                cmd.Parameters.AddWithValue("@hashtagId", hashtagId);

                return await Insert(cmd);
            }
        }
    }
}
