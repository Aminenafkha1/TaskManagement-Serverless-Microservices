using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Logging;
using System.Net;
using TaskManagement.Models;

namespace TaskManagement.Shared.Services
{
    public interface IPersistenceService
    {
        Task<T> CreateAsync<T>(T item, string containerId, string? partitionKey = null) where T : class;
        Task<T?> GetAsync<T>(string id, string containerId, string? partitionKey = null) where T : class;
        Task<IEnumerable<T>> GetAllAsync<T>(string containerId, string? queryString = null) where T : class;
        Task<T> UpdateAsync<T>(T item, string containerId, string id, string? partitionKey = null) where T : class;
        Task DeleteAsync(string id, string containerId, string? partitionKey = null);
    }

    public class CosmosPersistenceService : IPersistenceService, IDisposable
    {
        private readonly CosmosClient _cosmosClient;
        private readonly Database _database;
        private readonly ILogger<CosmosPersistenceService> _logger;
        private readonly string _databaseName;

        public CosmosPersistenceService(string connectionString, string databaseName, ILogger<CosmosPersistenceService> logger)
        {
            // Parse connection string to extract endpoint and key
            var parts = connectionString.Split(';');
            string? endpointUri = null;
            string? primaryKey = null;
            
            foreach (var part in parts)
            {
                if (part.StartsWith("AccountEndpoint="))
                {
                    endpointUri = part.Substring("AccountEndpoint=".Length);
                }
                else if (part.StartsWith("AccountKey="))
                {
                    primaryKey = part.Substring("AccountKey=".Length);
                }
            }
            
            if (string.IsNullOrEmpty(endpointUri) || string.IsNullOrEmpty(primaryKey))
            {
                throw new ArgumentException("Invalid Cosmos DB connection string. Must contain AccountEndpoint and AccountKey.");
            }
            
            // Create CosmosClient without custom serialization options - let it use defaults
            _cosmosClient = new CosmosClient(endpointUri, primaryKey);
            _databaseName = databaseName;
            _logger = logger;
            _database = _cosmosClient.GetDatabase(databaseName);
        }

        public async Task<T> CreateAsync<T>(T item, string containerId, string? partitionKey = null) where T : class
        {
            try
            {
                var container = _database.GetContainer(containerId);
                
                // If partition key is not provided, try to get the id property value
                if (partitionKey == null)
                {
                    var idProperty = typeof(T).GetProperty("Id");
                    if (idProperty == null)
                    {
                        throw new InvalidOperationException($"Type {typeof(T).Name} must have an Id property or specify partitionKey parameter");
                    }
                    
                    var id = idProperty.GetValue(item)?.ToString();
                    if (string.IsNullOrEmpty(id))
                    {
                        throw new InvalidOperationException($"Id property cannot be null or empty for type {typeof(T).Name}");
                    }
                    partitionKey = id;
                }
                
                var pk = new PartitionKey(partitionKey);
                var response = await container.CreateItemAsync(item, pk);
                _logger.LogInformation("Created item in container {ContainerId} with status {StatusCode}", containerId, response.StatusCode);
                return response.Resource;
            }
            catch (CosmosException ex)
            {
                _logger.LogError(ex, "Failed to create item in container {ContainerId}. Error: {Message}", containerId, ex.Message);
                throw;
            }
        }

        public async Task<T?> GetAsync<T>(string id, string containerId, string? partitionKey = null) where T : class
        {
            try
            {
                var container = _database.GetContainer(containerId);
                var pk = partitionKey != null ? new PartitionKey(partitionKey) : new PartitionKey(id);
                var response = await container.ReadItemAsync<T>(id, pk);
                return response.Resource;
            }
            catch (CosmosException ex) when (ex.StatusCode == HttpStatusCode.NotFound)
            {
                _logger.LogWarning("Item with id {Id} not found in container {ContainerId}", id, containerId);
                return null;
            }
            catch (CosmosException ex)
            {
                _logger.LogError(ex, "Failed to get item {Id} from container {ContainerId}", id, containerId);
                throw;
            }
        }

        public async Task<IEnumerable<T>> GetAllAsync<T>(string containerId, string? queryString = null) where T : class
        {
            try
            {
                var container = _database.GetContainer(containerId);
                var query = queryString ?? "SELECT * FROM c";
                var queryDefinition = new QueryDefinition(query);
                
                var results = new List<T>();
                using var feedIterator = container.GetItemQueryIterator<T>(queryDefinition);
                
                while (feedIterator.HasMoreResults)
                {
                    var response = await feedIterator.ReadNextAsync();
                    results.AddRange(response);
                }

                _logger.LogInformation("Retrieved {Count} items from container {ContainerId}", results.Count, containerId);
                return results;
            }
            catch (CosmosException ex)
            {
                _logger.LogError(ex, "Failed to get items from container {ContainerId}", containerId);
                throw;
            }
        }

        public async Task<T> UpdateAsync<T>(T item, string containerId, string id, string? partitionKey = null) where T : class
        {
            try
            {
                var container = _database.GetContainer(containerId);
                
                // If partition key is not provided, try to get the id property value
                if (partitionKey == null)
                {
                    var idProperty = typeof(T).GetProperty("Id");
                    if (idProperty != null)
                    {
                        partitionKey = idProperty.GetValue(item)?.ToString() ?? id;
                    }
                    else
                    {
                        partitionKey = id;
                    }
                }
                
                var pk = new PartitionKey(partitionKey);
                var response = await container.UpsertItemAsync(item, pk);
                _logger.LogInformation("Updated item {Id} in container {ContainerId} with status {StatusCode}", id, containerId, response.StatusCode);
                return response.Resource;
            }
            catch (CosmosException ex)
            {
                _logger.LogError(ex, "Failed to update item {Id} in container {ContainerId}", id, containerId);
                throw;
            }
        }

        public async Task DeleteAsync(string id, string containerId, string? partitionKey = null)
        {
            try
            {
                var container = _database.GetContainer(containerId);
                var pk = partitionKey != null ? new PartitionKey(partitionKey) : new PartitionKey(id);
                await container.DeleteItemAsync<object>(id, pk);
                _logger.LogInformation("Deleted item {Id} from container {ContainerId}", id, containerId);
            }
            catch (CosmosException ex) when (ex.StatusCode == HttpStatusCode.NotFound)
            {
                _logger.LogWarning("Item with id {Id} not found for deletion in container {ContainerId}", id, containerId);
            }
            catch (CosmosException ex)
            {
                _logger.LogError(ex, "Failed to delete item {Id} from container {ContainerId}", id, containerId);
                throw;
            }
        }

        public void Dispose()
        {
            _cosmosClient?.Dispose();
        }
    }

    // Configuration class for Cosmos DB settings
    public class CosmosDbSettings
    {
        public string ConnectionString { get; set; } = string.Empty;
        public string DatabaseName { get; set; } = string.Empty;
        public string UsersContainerName { get; set; } = "users";
        public string TasksContainerName { get; set; } = "tasks";
    }
}
