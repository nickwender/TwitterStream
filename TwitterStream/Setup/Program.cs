using Azure.Storage.Queues;
using FluentMigrator.Runner;
using Microsoft.Extensions.DependencyInjection;
using Setup.Migrators;
using System;

namespace Setup
{
    class Program
    {
        static void Main(string[] args)
        {
            // The database named in the connection string must exist!
            // This setup project only creates the tables.
            var connectionString = "";

            // Run the database migrations.
            var serviceProvider = CreateServices(connectionString);
            using (var scope = serviceProvider.CreateScope())
            {
                UpdateDatabase(scope.ServiceProvider);
            }

            // Create Azure storage queues.
            CreateAzureStorageQueues();
        }

        private static IServiceProvider CreateServices(string connectionString)
        {
            return new ServiceCollection()
                .AddFluentMigratorCore()
                .ConfigureRunner(r => r
                    .AddSqlServer()
                    .WithGlobalConnectionString(connectionString)
                    .ScanIn(typeof(InitialMigration).Assembly).For.Migrations())
                .AddLogging(lb => lb.AddFluentMigratorConsole())
                .BuildServiceProvider(false);
        }

        private static void UpdateDatabase(IServiceProvider serviceProvider)
        {
            var runner = serviceProvider.GetRequiredService<IMigrationRunner>();
            runner.MigrateUp();
        }

        private static void CreateAzureStorageQueues()
        {
            var azureStorageConnectionString = "";
            var queueClient = new QueueClient(azureStorageConnectionString, "tweets");
            queueClient.CreateIfNotExists();
        }
    }
}
