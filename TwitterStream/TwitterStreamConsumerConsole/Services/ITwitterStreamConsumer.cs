using System.Threading.Tasks;

namespace TwitterStreamConsumerConsole.Services
{
    public interface ITwitterStreamConsumer
    {
        /// <summary>
        /// Endlessly consumes tweets.
        /// </summary>
        /// <returns></returns>
        Task ConsumeTweets();
    }
}
