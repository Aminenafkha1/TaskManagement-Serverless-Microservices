using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using TaskManagement.Models;
using TaskManagement.Shared.Services;
using TaskStatus = TaskManagement.Models.TaskStatus;

namespace TaskManagement.MaterializedViewProcessor
{
    /// <summary>
    /// Azure Function that maintains materialized views using Cosmos DB Change Feed
    /// This automatically updates denormalized views when source data changes
    /// </summary>
    public class MaterializedViewProcessor
    {
        private readonly ILogger<MaterializedViewProcessor> _logger;
        private readonly IPersistenceService _persistenceService;

        public MaterializedViewProcessor(ILogger<MaterializedViewProcessor> logger, IPersistenceService persistenceService)
        {
            _logger = logger;
            _persistenceService = persistenceService;
        }

        /// <summary>
        /// Processes changes from the tasks container and updates materialized views
        /// </summary>
        [Function("ProcessTaskChanges")]
        public async Task ProcessTaskChanges([CosmosDBTrigger(
            databaseName: "TaskManagementDB",
            containerName: "tasks",
            Connection = "CosmosConnectionString",
            LeaseContainerName = "leases",
            CreateLeaseContainerIfNotExists = true)] IReadOnlyList<TaskItem> input)
        {
            if (input != null && input.Count > 0)
            {
                _logger.LogInformation("Processing {Count} task changes for materialized views", input.Count);

                foreach (var task in input)
                {
                    try
                    {
                        await UpdateTaskWithUserInfoView(task);
                        await UpdateUserActivityView(task);
                        await UpdateDashboardView();
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Failed to update materialized views for task {TaskId}", task.Id);
                    }
                }

                _logger.LogInformation("Completed processing task changes for materialized views");
            }
        }

        /// <summary>
        /// Processes changes from the users container and updates materialized views
        /// </summary>
        [Function("ProcessUserChanges")]
        public async Task ProcessUserChanges([CosmosDBTrigger(
            databaseName: "TaskManagementDB",
            containerName: "users",
            Connection = "CosmosConnectionString",
            LeaseContainerName = "leases-users",
            CreateLeaseContainerIfNotExists = true)] IReadOnlyList<User> input)
        {
            if (input != null && input.Count > 0)
            {
                _logger.LogInformation("Processing {Count} user changes for materialized views", input.Count);

                foreach (var user in input)
                {
                    try
                    {
                        await UpdateUserActivityViewFromUserChange(user);
                        await UpdateTasksWithUserInfoFromUserChange(user);
                        await UpdateDashboardView();
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Failed to update materialized views for user {UserId}", user.Id);
                    }
                }

                _logger.LogInformation("Completed processing user changes for materialized views");
            }
        }

        /// <summary>
        /// Manual trigger to rebuild all materialized views
        /// </summary>
        [Function("RebuildMaterializedViews")]
        public async Task<string> RebuildMaterializedViews([HttpTrigger(AuthorizationLevel.Admin, "post", Route = "admin/rebuild-views")] HttpRequestData req)
        {
            try
            {
                _logger.LogInformation("Starting materialized views rebuild");

                await RebuildTasksWithUserInfoView();
                await RebuildUserActivityView();
                await RebuildDashboardView();

                var message = "Materialized views rebuild completed successfully";
                _logger.LogInformation(message);

                var response = req.CreateResponse(System.Net.HttpStatusCode.OK);
                await response.WriteStringAsync(message);
                return message;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to rebuild materialized views");
                
                var errorMessage = $"Failed to rebuild materialized views: {ex.Message}";
                var response = req.CreateResponse(System.Net.HttpStatusCode.InternalServerError);
                await response.WriteStringAsync(errorMessage);
                return errorMessage;
            }
        }

        private async Task UpdateTaskWithUserInfoView(TaskItem task)
        {
            try
            {
                // Get user information
                var assignedUser = await _persistenceService.GetAsync<User>(task.AssignedToUserId.ToString(), "users", task.AssignedToUserId.ToString());
                var createdByUser = await _persistenceService.GetAsync<User>(task.CreatedByUserId.ToString(), "users", task.CreatedByUserId.ToString());

                var view = new TaskWithUserInfoView
                {
                    id = task.Id.ToString(),
                    title = task.Title,
                    description = task.Description,
                    status = task.Status,
                    priority = task.Priority,
                    createdAt = task.CreatedAt,
                    updatedAt = task.UpdatedAt,
                    dueDate = task.DueDate,
                    assignedToUserId = task.AssignedToUserId.ToString(),
                    assignedToUserName = assignedUser != null ? $"{assignedUser.FirstName} {assignedUser.LastName}".Trim() : "Unknown User",
                    assignedToUserEmail = assignedUser?.Email ?? "unknown@email.com",
                    createdByUserId = task.CreatedByUserId.ToString(),
                    createdByUserName = createdByUser != null ? $"{createdByUser.FirstName} {createdByUser.LastName}".Trim() : "Unknown User",
                    createdByUserEmail = createdByUser?.Email ?? "unknown@email.com",
                    lastUpdated = DateTime.UtcNow
                };

                // Upsert to materialized view container
                await _persistenceService.CreateAsync(view, "vw_tasks_with_users");
                
                _logger.LogInformation("Updated TaskWithUserInfo view for task {TaskId}", task.Id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to update TaskWithUserInfo view for task {TaskId}", task.Id);
                throw;
            }
        }

        private async Task UpdateUserActivityView(TaskItem task)
        {
            try
            {
                // Update activity for assigned user
                await UpdateUserActivityForUser(task.AssignedToUserId.ToString());

                // Update activity for creator if different
                if (task.CreatedByUserId != task.AssignedToUserId)
                {
                    await UpdateUserActivityForUser(task.CreatedByUserId.ToString());
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to update UserActivity view for task {TaskId}", task.Id);
                throw;
            }
        }

        private async Task UpdateUserActivityForUser(string userId)
        {
            try
            {
                // Get user details
                var user = await _persistenceService.GetAsync<User>(userId, "users", userId);
                if (user == null)
                {
                    _logger.LogWarning("User {UserId} not found for activity update", userId);
                    return;
                }

                // Get all tasks for this user
                var userTasks = await _persistenceService.GetAllAsync<TaskItem>("tasks", 
                    $"SELECT * FROM c WHERE c.AssignedToUserId = '{userId}' OR c.CreatedByUserId = '{userId}'");

                // Calculate statistics
                var totalAssigned = userTasks.Count(t => t.AssignedToUserId.ToString() == userId);
                var completed = userTasks.Count(t => t.AssignedToUserId.ToString() == userId && t.Status == TaskStatus.Done);
                var inProgress = userTasks.Count(t => t.AssignedToUserId.ToString() == userId && t.Status == TaskStatus.InProgress);
                var pending = userTasks.Count(t => t.AssignedToUserId.ToString() == userId && t.Status == TaskStatus.Todo);
                var overdue = userTasks.Count(t => t.AssignedToUserId.ToString() == userId && 
                    t.DueDate.HasValue && t.DueDate < DateTime.UtcNow && t.Status != TaskStatus.Done);

                var view = new UserActivityView
                {
                    id = userId,
                    userId = userId,
                    userName = $"{user.FirstName} {user.LastName}".Trim(),
                    email = user.Email,
                    role = user.Role,
                    totalTasksAssigned = totalAssigned,
                    tasksCompleted = completed,
                    tasksInProgress = inProgress,
                    tasksPending = pending,
                    tasksOverdue = overdue,
                    lastActivity = DateTime.UtcNow,
                    recentTaskIds = userTasks.Take(10).Select(t => t.Id.ToString()).ToList(),
                    lastUpdated = DateTime.UtcNow
                };

                // Calculate average completion days for completed tasks
                var completedTasks = userTasks.Where(t => t.Status == TaskStatus.Done && t.UpdatedAt != null);
                if (completedTasks.Any())
                {
                    view.averageCompletionDays = completedTasks.Average(t => (t.UpdatedAt - t.CreatedAt).TotalDays);
                }

                await _persistenceService.CreateAsync(view, "vw_user_activity");
                
                _logger.LogInformation("Updated UserActivity view for user {UserId}", userId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to update UserActivity view for user {UserId}", userId);
                throw;
            }
        }

        private async Task UpdateUserActivityViewFromUserChange(User user)
        {
            try
            {
                // When user details change, update the user activity view
                await UpdateUserActivityForUser(user.Id.ToString());
                
                _logger.LogInformation("Updated UserActivity view from user change for user {UserId}", user.Id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to update UserActivity view from user change for user {UserId}", user.Id);
                throw;
            }
        }

        private async Task UpdateTasksWithUserInfoFromUserChange(User user)
        {
            try
            {
                // When user details change, update all task views that reference this user
                var tasks = await _persistenceService.GetAllAsync<TaskItem>("tasks", 
                    $"SELECT * FROM c WHERE c.AssignedToUserId = '{user.Id}' OR c.CreatedByUserId = '{user.Id}'");

                foreach (var task in tasks)
                {
                    await UpdateTaskWithUserInfoView(task);
                }
                
                _logger.LogInformation("Updated TasksWithUserInfo views from user change for user {UserId}", user.Id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to update TasksWithUserInfo views from user change for user {UserId}", user.Id);
                throw;
            }
        }

        private async Task UpdateDashboardView()
        {
            try
            {
                // Get system-wide statistics
                var allTasks = await _persistenceService.GetAllAsync<TaskItem>("tasks");
                var allUsers = await _persistenceService.GetAllAsync<User>("users");

                var totalTasks = allTasks.Count();
                var completed = allTasks.Count(t => t.Status == TaskStatus.Done);
                var inProgress = allTasks.Count(t => t.Status == TaskStatus.InProgress);
                var pending = allTasks.Count(t => t.Status == TaskStatus.Todo);
                var overdue = allTasks.Count(t => t.DueDate.HasValue && t.DueDate < DateTime.UtcNow && t.Status != TaskStatus.Done);

                // Get active users (users with tasks in last 30 days)
                var thirtyDaysAgo = DateTime.UtcNow.AddDays(-30);
                var activeUsers = allTasks.Where(t => t.CreatedAt > thirtyDaysAgo)
                    .Select(t => t.AssignedToUserId).Distinct().Count();

                var view = new DashboardView
                {
                    id = "dashboard",
                    totalTasks = totalTasks,
                    tasksCompleted = completed,
                    tasksInProgress = inProgress,
                    tasksPending = pending,
                    tasksOverdue = overdue,
                    totalUsers = allUsers.Count(),
                    activeUsers = activeUsers,
                    lastUpdated = DateTime.UtcNow
                };

                // Generate daily metrics for last 30 days
                for (int i = 29; i >= 0; i--)
                {
                    var date = DateTime.UtcNow.Date.AddDays(-i);
                    var dayTasks = allTasks.Where(t => t.CreatedAt.Date == date);
                    var dayCompleted = allTasks.Where(t => t.UpdatedAt.Date == date && t.Status == TaskStatus.Done);
                    var dayActiveUsers = dayTasks.Select(t => t.AssignedToUserId).Distinct().Count();

                    view.dailyMetrics.Add(new DailyMetric
                    {
                        date = date,
                        tasksCreated = dayTasks.Count(),
                        tasksCompleted = dayCompleted.Count(),
                        activeUsers = dayActiveUsers
                    });
                }

                await _persistenceService.CreateAsync(view, "vw_dashboard");
                
                _logger.LogInformation("Updated Dashboard view");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to update Dashboard view");
                throw;
            }
        }

        private async Task RebuildTasksWithUserInfoView()
        {
            var tasks = await _persistenceService.GetAllAsync<TaskItem>("tasks");
            foreach (var task in tasks)
            {
                await UpdateTaskWithUserInfoView(task);
            }
        }

        private async Task RebuildUserActivityView()
        {
            var users = await _persistenceService.GetAllAsync<User>("users");
            foreach (var user in users)
            {
                await UpdateUserActivityForUser(user.Id.ToString());
            }
        }

        private async Task RebuildDashboardView()
        {
            await UpdateDashboardView();
        }
    }
}
