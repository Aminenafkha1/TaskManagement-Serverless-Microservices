using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace TaskManagement.Shared.Services
{
    public class DatabaseInitializer
    {
        private readonly CosmosClient _cosmosClient;
        private readonly ILogger _logger;

        public DatabaseInitializer(CosmosClient cosmosClient, ILogger<DatabaseInitializer> logger)
        {
            _cosmosClient = cosmosClient;
            _logger = logger;
        }

        public async Task InitializeAsync(string databaseName, params string[] containerNames)
        {
            try
            {
                _logger.LogInformation("Initializing Cosmos DB - Database: {DatabaseName}", databaseName);

                // Create database if it doesn't exist
                var databaseResponse = await _cosmosClient.CreateDatabaseIfNotExistsAsync(
                    databaseName,
                    throughput: 400); // Shared throughput

                _logger.LogInformation("Database {DatabaseName} ready", databaseName);

                // Create containers if they don't exist
                foreach (var containerName in containerNames)
                {
                    var containerProperties = new ContainerProperties
                    {
                        Id = containerName,
                        PartitionKeyPath = GetPartitionKeyPath(containerName)
                    };

                    var containerResponse = await databaseResponse.Database.CreateContainerIfNotExistsAsync(
                        containerProperties,
                        throughput: null); // Use database-level throughput

                    _logger.LogInformation("Container {ContainerName} ready in database {DatabaseName}", 
                        containerName, databaseName);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to initialize database {DatabaseName}", databaseName);
                throw;
            }
        }

        private string GetPartitionKeyPath(string containerName)
        {
            return containerName switch
            {
                "users" => "/email",      // Partition by email for users
                "tasks" => "/userId",     // Partition by userId for tasks
                _ => "/id"                // Default partition key
            };
        }
    }
}
