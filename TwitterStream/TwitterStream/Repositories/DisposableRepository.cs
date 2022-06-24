using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Options;
using System;
using System.Threading.Tasks;

namespace TwitterStream.Repositories
{
    /// <summary>
    /// A base class for repositories. It takes care of setting up and tearing down database connections.
    /// </summary>
    public class DisposableRepository : IDisposable
    {
        protected readonly Configuration _configuration;
        protected readonly SqlConnection _connection;

        public DisposableRepository(IOptions<Configuration> configuration)
        {
            _configuration = configuration.Value;
            
            // Arguably, this logic could move to a connection factory.
            if (string.IsNullOrWhiteSpace(_configuration.DatabaseConnectionString))
            {
                throw new ArgumentException("Configuration does not provide DatabaseConnectionString", "configuration");
            }
            _connection = new SqlConnection(_configuration.DatabaseConnectionString);
            _connection.Open();
        }

        /// <summary>
        /// Close our database connection so we don't leak connections from the pool.
        /// </summary>
        public void Dispose()
        {
            if (_connection != null)
            {
                _connection.Close();
            }
        }

        /// <summary>
        /// Generic method to insert a record and return the ID of the inserted record.
        /// </summary>
        /// <param name="cmd">SqlCommand to execute.</param>
        /// <returns>The ID of the inserted record or null on error.</returns>
        protected async Task<int?> Insert(SqlCommand cmd)
        {
            try
            {
                var result = await cmd.ExecuteScalarAsync();
                if (result == null || result == DBNull.Value)
                {
                    return null;
                }
                return (int)result;
            }
            catch (Exception)
            {
                return null;
            }
        }
    }
}
