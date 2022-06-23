using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using TwitterStream.Repositories;

[assembly: FunctionsStartup(typeof(TwitterStream.Startup))]
namespace TwitterStream
{
    public class Startup : FunctionsStartup
    {
        public override void Configure(IFunctionsHostBuilder builder)
        {
            builder.Services.AddOptions<Configuration>()
                .Configure<IConfiguration>((settings, configuration) =>
                {
                    configuration.GetSection("Configs").Bind(settings);
                });

            // Setup scoped repositories.
            // Repositories must be scoped so we can create a new SqlConnection per function execution.
            builder.Services.AddScoped<ITweetRepository, TweetRepository>();
        }
    }
}
