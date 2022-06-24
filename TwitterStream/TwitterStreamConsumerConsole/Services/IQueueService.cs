using System.Threading.Tasks;
using Models.Twitter;

namespace TwitterStreamConsumerConsole.Services
{
    public interface IQueueService
    {
        Task QueueTweet(TweetObject tweet);
    }
}
