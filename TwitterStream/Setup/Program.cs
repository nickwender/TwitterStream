using Azure.Storage.Queues;
using FluentMigrator.Runner;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Setup.Migrators;
using System;

namespace Setup
{
    class Program
    {
        static void Main(string[] args)
        {
            var configuration = LoadAppSettings();

            // Run the database migrations.
            // The database named in the connection string must exist!
            // This setup project only creates the tables.
            var serviceProvider = CreateServices(configuration["AppSettings:DatabaseConnectionString"]);
            using (var scope = serviceProvider.CreateScope())
            {
                UpdateDatabase(scope.ServiceProvider);
            }

            // The storage account in the connection string must exist.
            // If using the Azure Storage Emulator, it must be running.
            // Create Azure storage queues.
            CreateAzureStorageQueues(configuration["AppSettings:AzureStorageQueueConnectionString"]);
        }
        private static IConfigurationRoot LoadAppSettings()
        {
            var builder = new ConfigurationBuilder().AddJsonFile("appsettings.json", true, true);
            var configuration = builder.Build();
            return configuration;
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

        private static void CreateAzureStorageQueues(string connectionString)
        {
            var queueClient = new QueueClient(connectionString, "tweets");
            queueClient.CreateIfNotExists();
        }
    }
}
