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
    public class TweetRepository : DisposableRepository, ITweetRepository
    {
        public TweetRepository(IOptions<Configuration> configuration) : base(configuration)
        {
        }

        public async Task<bool> AddTweet(TweetObject tweet)
        {
            // TODO: Use Unit of Work pattern to make rollbacks on error easier.
            // TODO: Use an ORM like Dapper instead of SqlCommand.

            var tweetId = await InsertTweet(tweet);

            if (!tweetId.HasValue)
            {
                return false;
            }

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

        public async Task<TweetStats> GetStats()
        {
            var stats = new TweetStats()
            {
                TweetsCollected = 0,
                TopHashtags = new Dictionary<string, int>()
            };

            try
            {
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
                // TODO: add logger.
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

        private async Task<int?> InsertHashtag(string hashtag)
        {
            var query = "INSERT INTO Hashtags (Hashtag) OUTPUT INSERTED.ID VALUES(@hashtag)";
            using SqlCommand cmd = new SqlCommand(query, _connection);
            {
                cmd.Parameters.AddWithValue("@hashtag", hashtag);
                return await Insert(cmd);
            }
        }

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
                            // TODO: add logger.
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
