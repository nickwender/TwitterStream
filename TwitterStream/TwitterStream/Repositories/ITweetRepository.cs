using Models.Twitter;
using System.Threading.Tasks;
using TwitterStream.Models.Response;

namespace TwitterStream.Repositories
{
    public interface ITweetRepository
    {
        Task<bool> AddTweet(TweetObject tweet);
        Task<TweetStats> GetStats();
    }
}
