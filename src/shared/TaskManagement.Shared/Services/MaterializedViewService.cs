using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Logging;
using TaskManagement.Models;
using System;

namespace TaskManagement.Shared.Services
{
    /// <summary>
    /// Service for managing and querying Azure Cosmos DB native materialized views
    /// </summary>
    public class MaterializedViewService : IMaterializedViewService
    {
        private readonly IPersistenceService _persistenceService;
        private readonly ILogger<MaterializedViewService> _logger;
        private readonly string _connectionString;
        private readonly string _databaseName;

        public MaterializedViewService(ILogger<MaterializedViewService> logger)
        {
            _logger = logger;
            
            // Get connection string and database name from environment variables
            _connectionString = Environment.GetEnvironmentVariable("CosmosConnectionString") 
                ?? throw new InvalidOperationException("CosmosConnectionString environment variable is required");
            _databaseName = Environment.GetEnvironmentVariable("CosmosDbDatabaseName") ?? "TaskManagementDB";
            
            // Create persistence service for materialized view operations
            _persistenceService = new CosmosPersistenceService(_connectionString, _databaseName, 
                logger as ILogger<CosmosPersistenceService> ?? 
                new LoggerAdapter<CosmosPersistenceService>(logger));
        }

        /// <summary>
        /// Gets tasks with denormalized user information for a specific user
        /// </summary>
        public async Task<IEnumerable<TaskWithUserInfoView>> GetTasksWithUserInfoAsync(string userId)
        {
            try
            {
                var query = $@"
                    SELECT * FROM c 
                    WHERE c.assignedToUserId = '{userId}' OR c.createdByUserId = '{userId}'
                    ORDER BY c.createdAt DESC";

                var tasks = await _persistenceService.GetAllAsync<TaskWithUserInfoView>("vw_tasks_with_users", query);
                _logger.LogInformation("Retrieved {Count} tasks with user info for user {UserId}", tasks.Count(), userId);
                return tasks;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get tasks with user info for user {UserId}", userId);
                return new List<TaskWithUserInfoView>();
            }
        }

        /// <summary>
        /// Gets all tasks with denormalized user information
        /// </summary>
        public async Task<IEnumerable<TaskWithUserInfoView>> GetAllTasksWithUserInfoAsync()
        {
            try
            {
                var query = "SELECT * FROM c ORDER BY c.createdAt DESC";
                var tasks = await _persistenceService.GetAllAsync<TaskWithUserInfoView>("vw_tasks_with_users", query);
                _logger.LogInformation("Retrieved {Count} total tasks with user info", tasks.Count());
                return tasks;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get all tasks with user info");
                return new List<TaskWithUserInfoView>();
            }
        }

        /// <summary>
        /// Gets user activity and statistics for a specific user
        /// </summary>
        public async Task<UserActivityView?> GetUserActivityAsync(string userId)
        {
            try
            {
                var userActivity = await _persistenceService.GetAsync<UserActivityView>(userId, "vw_user_activity", userId);
                if (userActivity != null)
                {
                    _logger.LogInformation("Retrieved user activity for user {UserId}", userId);
                }
                else
                {
                    _logger.LogWarning("No user activity found for user {UserId}", userId);
                }
                return userActivity;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get user activity for user {UserId}", userId);
                return null;
            }
        }

        /// <summary>
        /// Gets user activity for all users
        /// </summary>
        public async Task<IEnumerable<UserActivityView>> GetAllUserActivityAsync()
        {
            try
            {
                var query = "SELECT * FROM c ORDER BY c.totalTasksAssigned DESC";
                var userActivities = await _persistenceService.GetAllAsync<UserActivityView>("vw_user_activity", query);
                _logger.LogInformation("Retrieved user activity for {Count} users", userActivities.Count());
                return userActivities;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get all user activities");
                return new List<UserActivityView>();
            }
        }

        /// <summary>
        /// Gets system-wide dashboard metrics and statistics
        /// </summary>
        public async Task<DashboardView?> GetDashboardViewAsync()
        {
            try
            {
                var dashboard = await _persistenceService.GetAsync<DashboardView>("dashboard", "vw_dashboard", "dashboard");
                if (dashboard != null)
                {
                    _logger.LogInformation("Retrieved dashboard view with {TotalTasks} total tasks", dashboard.totalTasks);
                }
                else
                {
                    _logger.LogWarning("No dashboard view found");
                }
                return dashboard;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get dashboard view");
                return null;
            }
        }

        /// <summary>
        /// Gets tasks with user info filtered by status
        /// </summary>
        public async Task<IEnumerable<TaskWithUserInfoView>> GetTasksWithUserInfoByStatusAsync(Models.TaskStatus status)
        {
            try
            {
                var query = $"SELECT * FROM c WHERE c.status = {(int)status} ORDER BY c.createdAt DESC";
                var tasks = await _persistenceService.GetAllAsync<TaskWithUserInfoView>("vw_tasks_with_users", query);
                _logger.LogInformation("Retrieved {Count} tasks with status {Status}", tasks.Count(), status);
                return tasks;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get tasks with status {Status}", status);
                return new List<TaskWithUserInfoView>();
            }
        }

        /// <summary>
        /// Gets overdue tasks with user information
        /// </summary>
        public async Task<IEnumerable<TaskWithUserInfoView>> GetOverdueTasksWithUserInfoAsync()
        {
            try
            {
                var currentDate = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ss.fffZ");
                var query = $@"
                    SELECT * FROM c 
                    WHERE c.dueDate < '{currentDate}' 
                    AND c.status != {(int)Models.TaskStatus.Done}
                    ORDER BY c.dueDate ASC";

                var overdueTasks = await _persistenceService.GetAllAsync<TaskWithUserInfoView>("vw_tasks_with_users", query);
                _logger.LogInformation("Retrieved {Count} overdue tasks", overdueTasks.Count());
                return overdueTasks;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get overdue tasks");
                return new List<TaskWithUserInfoView>();
            }
        }

        /// <summary>
        /// Gets top performing users based on task completion
        /// </summary>
        public async Task<IEnumerable<UserActivityView>> GetTopPerformingUsersAsync(int topCount = 10)
        {
            try
            {
                var query = $@"
                    SELECT TOP {topCount} * FROM c 
                    WHERE c.totalTasksAssigned > 0
                    ORDER BY c.tasksCompleted DESC, c.averageCompletionDays ASC";

                var topUsers = await _persistenceService.GetAllAsync<UserActivityView>("vw_user_activity", query);
                _logger.LogInformation("Retrieved top {Count} performing users", topUsers.Count());
                return topUsers;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get top performing users");
                return new List<UserActivityView>();
            }
        }

        /// <summary>
        /// Initializes the materialized view containers if they don't exist
        /// </summary>
        public async Task InitializeMaterializedViewsAsync()
        {
            try
            {
                _logger.LogInformation("Initializing materialized view containers");

                using var cosmosClient = new CosmosClient(_connectionString);
                var database = cosmosClient.GetDatabase(_databaseName);

                // Define the materialized view containers
                var containers = new[]
                {
                    new { Name = "vw_tasks_with_users", PartitionKey = "/id" },
                    new { Name = "vw_user_activity", PartitionKey = "/id" },
                    new { Name = "vw_dashboard", PartitionKey = "/id" },
                    new { Name = "leases", PartitionKey = "/id" },
                    new { Name = "leases-users", PartitionKey = "/id" }
                };

                foreach (var containerInfo in containers)
                {
                    try
                    {
                        var containerResponse = await database.CreateContainerIfNotExistsAsync(
                            containerInfo.Name, 
                            containerInfo.PartitionKey,
                            400); // 400 RU/s

                        if (containerResponse.StatusCode == System.Net.HttpStatusCode.Created)
                        {
                            _logger.LogInformation("Created materialized view container: {ContainerName}", containerInfo.Name);
                        }
                        else
                        {
                            _logger.LogInformation("Materialized view container already exists: {ContainerName}", containerInfo.Name);
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Failed to create container {ContainerName}", containerInfo.Name);
                    }
                }

                _logger.LogInformation("Materialized view containers initialization completed");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to initialize materialized view containers");
                throw;
            }
        }
    }

    /// <summary>
    /// Logger adapter to bridge different logger types
    /// </summary>
    internal class LoggerAdapter<T> : ILogger<T>
    {
        private readonly ILogger _logger;

        public LoggerAdapter(ILogger logger)
        {
            _logger = logger;
        }

        public IDisposable? BeginScope<TState>(TState state) where TState : notnull
        {
            return _logger.BeginScope(state);
        }

        public bool IsEnabled(LogLevel logLevel)
        {
            return _logger.IsEnabled(logLevel);
        }

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
        {
            _logger.Log(logLevel, eventId, state, exception, formatter);
        }
    }
}
