using Microsoft.Extensions.Logging;
using Models.Twitter;
using Moq;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Threading.Tasks;
using TwitterStream.Repositories;
using Xunit;

namespace TwitterStream.Tests
{
    public class TweetQueueListenerTests
    {
        [Fact]
        public async Task Run_ValidTweet_Persisted()
        {
            // Arrange.
            var tweetRepositoryMock = new Mock<ITweetRepository>();
            var loggerMock = new Mock<ILogger>();
            var queueListener = new TweetQueueListener(tweetRepositoryMock.Object);

            // Act.
            var tweet = new TweetObject
            {
                Id = "12345",
                Text = "Test tweet",
                Entities = new TweetEntities
                {
                    Hashtags = new List<TweetHashtag>
                    {
                        new TweetHashtag
                        {
                            Tag = "Azure"
                        }
                    }
                }
            };
            await queueListener.Run(JsonConvert.SerializeObject(tweet), loggerMock.Object);

            // Assert.
            tweetRepositoryMock.Verify(m => m.AddTweet(It.IsAny<TweetObject>()), Times.Once);
        }


        [Fact]
        public async Task Run_NullTweet_DoesNotPersist()
        {
            // Arrange.
            var tweetRepositoryMock = new Mock<ITweetRepository>();
            var loggerMock = new Mock<ILogger>();
            var queueListener = new TweetQueueListener(tweetRepositoryMock.Object);

            // Act.
            await queueListener.Run(null, loggerMock.Object);

            // Assert.
            tweetRepositoryMock.Verify(m => m.AddTweet(It.IsAny<TweetObject>()), Times.Never);
        }

        [Theory]
        [InlineData(null, "Test tweet")]
        [InlineData("", "Test tweet")]
        [InlineData("12345", null)]
        [InlineData("12345", "")]
        public async Task Run_InvalidTweet_DoesNotPersist(string twitterId, string text)
        {
            // Arrange.
            var tweetRepositoryMock = new Mock<ITweetRepository>();
            var loggerMock = new Mock<ILogger>();
            var queueListener = new TweetQueueListener(tweetRepositoryMock.Object);

            // Act.
            var tweet = new TweetObject
            {
                Id = twitterId,
                Text = text,
                Entities = null
            };
            await queueListener.Run(JsonConvert.SerializeObject(tweet), loggerMock.Object);

            // Assert.
            tweetRepositoryMock.Verify(m => m.AddTweet(It.IsAny<TweetObject>()), Times.Never);
        }

        [Theory]
        [InlineData("")]
        [InlineData((string) null)]
        public async Task Run_InvalidTweetHashtag_DoesNotPersist(string hashtagValue)
        {
            // Arrange.
            var tweetRepositoryMock = new Mock<ITweetRepository>();
            var loggerMock = new Mock<ILogger>();
            var queueListener = new TweetQueueListener(tweetRepositoryMock.Object);

            // Act.
            var tweet = new TweetObject
            {
                Id = "12345",
                Text = "Test tweet",
                Entities = new TweetEntities
                {
                    Hashtags = new List<TweetHashtag>
                    {
                        new TweetHashtag
                        {
                            Tag = hashtagValue
                        }
                    }
                }
            };
            await queueListener.Run(JsonConvert.SerializeObject(tweet), loggerMock.Object);

            // Assert.
            tweetRepositoryMock.Verify(m => m.AddTweet(It.IsAny<TweetObject>()), Times.Never);
        }
    }
}