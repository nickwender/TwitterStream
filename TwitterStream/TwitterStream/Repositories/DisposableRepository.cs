using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Options;
using System;
using System.Threading.Tasks;

namespace TwitterStream.Repositories
{
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

        public void Dispose()
        {
            if (_connection != null)
            {
                _connection.Close();
            }
        }

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
