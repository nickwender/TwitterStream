using Microsoft.AspNetCore.Mvc;
using Moq;
using Shouldly;
using System.Collections.Generic;
using TwitterStream.Controllers;
using TwitterStream.Models.Response;
using TwitterStream.Repositories;
using Xunit;

namespace TwitterStream.Tests
{

    public class TweetControllerTests
    {
        [Fact]
        public void Stats_ReturnsTweetStats()
        {
            // Arrange.
            var tweetRepositoryMock = new Mock<ITweetRepository>();
            tweetRepositoryMock.Setup(s => s.GetStats())
                .ReturnsAsync(new TweetStats
                {
                    TweetsCollected = 100,
                    TopHashtags = new Dictionary<string, int>
                    {
                        { "Azure", 20 },
                        { "AWS", 10 },
                        { "serverless", 5 },
                        { "docker", 5 },
                    }
                });
            var controller = new TweetController(tweetRepositoryMock.Object);

            // Act.
            var response = controller.Stats(null, null).Result;

            // Assert.
            response.ShouldNotBeNull();

            var jsonResult = response as JsonResult;
            jsonResult.ShouldNotBeNull();
            jsonResult.Value.ShouldBeOfType<TweetStats>();

            var stats = jsonResult.Value as TweetStats;
            stats.ShouldNotBeNull();
            stats.TweetsCollected.ShouldBe(100);
            stats.TopHashtags.Count.ShouldBe(4);
            stats.TopHashtags.ShouldContainKeyAndValue("Azure", 20);
            stats.TopHashtags.ShouldContainKeyAndValue("AWS", 10);
            stats.TopHashtags.ShouldContainKeyAndValue("serverless", 5);
            stats.TopHashtags.ShouldContainKeyAndValue("docker", 5);
        }
    }
}