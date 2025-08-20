using Microsoft.Extensions.Logging;
using TaskManagement.Models;
using TaskManagement.Models.Events;
using TaskManagement.Models.ReadModels;
using TaskManagement.Shared.Services;
using Models = TaskManagement.Models;

namespace TaskManagement.Shared.Services
{
    /// <summary>
    /// Interface for projection services that handle event-driven updates to read models
    /// </summary>
    public interface IProjectionService
    {
        Task HandleTaskCreatedAsync(TaskCreatedEvent taskEvent);
        Task HandleTaskUpdatedAsync(TaskUpdatedEvent taskEvent);
        Task HandleTaskCompletedAsync(TaskCompletedEvent taskEvent);
        Task HandleUserCreatedAsync(UserCreatedEvent userEvent);
        Task HandleUserUpdatedAsync(UserUpdatedEvent userEvent);
        Task HandleUserDeletedAsync(UserDeletedEvent userEvent);
        Task RebuildProjectionsAsync();
    }

    /// <summary>
    /// Service responsible for maintaining denormalized read models based on domain events
    /// This implements the projection side of CQRS pattern
    /// </summary>
    public class ProjectionService : IProjectionService
    {
        private readonly IPersistenceService _persistenceService;
        private readonly ILogger<ProjectionService> _logger;
        private const string TASK_WITH_USER_INFO_CONTAINER = "task-with-user-info";
        private const string USER_ACTIVITY_CONTAINER = "user-activity";
        private const string DASHBOARD_METRICS_CONTAINER = "dashboard-metrics";

        public ProjectionService(IPersistenceService persistenceService, ILogger<ProjectionService> logger)
        {
            _persistenceService = persistenceService;
            _logger = logger;
        }

        public async Task HandleTaskCreatedAsync(TaskCreatedEvent taskEvent)
        {
            try
            {
                _logger.LogInformation("Processing TaskCreatedEvent for task {TaskId}", taskEvent.TaskId);

                // Update TaskWithUserInfo read model
                await UpdateTaskWithUserInfoReadModel(taskEvent.TaskId.ToString(), taskEvent);

                // Update UserActivity read model for assigned user
                await UpdateUserActivityForTaskEvent(taskEvent.AssignedToUserId.ToString(), "TaskCreated", taskEvent.TaskId.ToString(), taskEvent.Title);

                // Update UserActivity read model for creator (if different)
                if (taskEvent.CreatedByUserId != taskEvent.AssignedToUserId)
                {
                    await UpdateUserActivityForTaskEvent(taskEvent.CreatedByUserId.ToString(), "TaskCreatedByUser", taskEvent.TaskId.ToString(), taskEvent.Title);
                }

                // Update dashboard metrics
                await UpdateDashboardMetrics();

                _logger.LogInformation("Successfully processed TaskCreatedEvent for task {TaskId}", taskEvent.TaskId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to process TaskCreatedEvent for task {TaskId}", taskEvent.TaskId);
                throw;
            }
        }

        public async Task HandleTaskUpdatedAsync(TaskUpdatedEvent taskEvent)
        {
            try
            {
                _logger.LogInformation("Processing TaskUpdatedEvent for task {TaskId}", taskEvent.TaskId);

                // Update TaskWithUserInfo read model
                await UpdateTaskWithUserInfoReadModel(taskEvent.TaskId.ToString(), taskEvent);

                // Update UserActivity read model for both old and new assigned users if they differ
                var newAssignedUserId = taskEvent.NewAssignedToUserId?.ToString();
                var oldAssignedUserId = taskEvent.OldAssignedToUserId?.ToString();
                
                if (!string.IsNullOrEmpty(newAssignedUserId))
                {
                    await UpdateUserActivityForTaskEvent(newAssignedUserId, "TaskUpdated", taskEvent.TaskId.ToString(), taskEvent.Title, taskEvent.OldStatus, taskEvent.NewStatus);
                }
                
                if (!string.IsNullOrEmpty(oldAssignedUserId) && oldAssignedUserId != newAssignedUserId)
                {
                    await UpdateUserActivityForTaskEvent(oldAssignedUserId, "TaskUnassigned", taskEvent.TaskId.ToString(), taskEvent.Title, taskEvent.OldStatus, taskEvent.NewStatus);
                }

                // Update dashboard metrics if status changed
                if (taskEvent.OldStatus != taskEvent.NewStatus)
                {
                    await UpdateDashboardMetrics();
                }

                _logger.LogInformation("Successfully processed TaskUpdatedEvent for task {TaskId}", taskEvent.TaskId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to process TaskUpdatedEvent for task {TaskId}", taskEvent.TaskId);
                throw;
            }
        }

        public async Task HandleTaskCompletedAsync(TaskCompletedEvent taskEvent)
        {
            try
            {
                _logger.LogInformation("Processing TaskCompletedEvent for task {TaskId}", taskEvent.TaskId);

                // Update TaskWithUserInfo read model
                await UpdateTaskWithUserInfoReadModel(taskEvent.TaskId.ToString(), taskEvent);

                // Update UserActivity read model with completion
                await UpdateUserActivityForTaskEvent(taskEvent.CompletedByUserId.ToString(), "TaskCompleted", taskEvent.TaskId.ToString(), taskEvent.Title);

                // Update dashboard metrics
                await UpdateDashboardMetrics();

                _logger.LogInformation("Successfully processed TaskCompletedEvent for task {TaskId}", taskEvent.TaskId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to process TaskCompletedEvent for task {TaskId}", taskEvent.TaskId);
                throw;
            }
        }

        public async Task HandleUserCreatedAsync(UserCreatedEvent userEvent)
        {
            try
            {
                _logger.LogInformation("Processing UserCreatedEvent for user {UserId}", userEvent.UserId);

                // Create initial UserActivity read model
                var userActivity = new UserActivityReadModel
                {
                    Id = Guid.NewGuid().ToString(),
                    UserId = userEvent.UserId,
                    UserName = $"{userEvent.FirstName} {userEvent.LastName}".Trim(),
                    Email = userEvent.Email,
                    Role = userEvent.Role,
                    LastActivity = userEvent.Timestamp,
                    LastProjectionUpdate = DateTime.UtcNow,
                    ProjectionVersion = "1.0"
                };

                await _persistenceService.CreateAsync(userActivity, USER_ACTIVITY_CONTAINER);

                // Update any existing TaskWithUserInfo read models
                var fullName = $"{userEvent.FirstName} {userEvent.LastName}".Trim();
                await UpdateTaskReadModelsForUser(userEvent.UserId, fullName, userEvent.Email);

                // Update dashboard metrics
                await UpdateDashboardMetrics();

                _logger.LogInformation("Successfully processed UserCreatedEvent for user {UserId}", userEvent.UserId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to process UserCreatedEvent for user {UserId}", userEvent.UserId);
                throw;
            }
        }

        public async Task HandleUserUpdatedAsync(UserUpdatedEvent userEvent)
        {
            try
            {
                _logger.LogInformation("Processing UserUpdatedEvent for user {UserId}", userEvent.UserId);

                // Update UserActivity read model
                var userActivity = await _persistenceService.GetAsync<UserActivityReadModel>(userEvent.UserId, USER_ACTIVITY_CONTAINER, userEvent.UserId);
                if (userActivity != null)
                {
                    userActivity.UserName = $"{userEvent.FirstName} {userEvent.LastName}".Trim();
                    userActivity.Email = userEvent.Email;
                    userActivity.Role = userEvent.Role;
                    userActivity.LastActivity = userEvent.Timestamp;
                    userActivity.LastProjectionUpdate = DateTime.UtcNow;

                    await _persistenceService.UpdateAsync(userActivity, USER_ACTIVITY_CONTAINER, userActivity.Id);
                }

                // Update TaskWithUserInfo read models
                var fullName = $"{userEvent.FirstName} {userEvent.LastName}".Trim();
                await UpdateTaskReadModelsForUser(userEvent.UserId, fullName, userEvent.Email);

                _logger.LogInformation("Successfully processed UserUpdatedEvent for user {UserId}", userEvent.UserId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to process UserUpdatedEvent for user {UserId}", userEvent.UserId);
                throw;
            }
        }

        public async Task HandleUserDeletedAsync(UserDeletedEvent userEvent)
        {
            try
            {
                _logger.LogInformation("Processing UserDeletedEvent for user {UserId}", userEvent.UserId);

                // Mark user as inactive in UserActivity read model
                var userActivity = await _persistenceService.GetAsync<UserActivityReadModel>(userEvent.UserId, USER_ACTIVITY_CONTAINER, userEvent.UserId);
                if (userActivity != null)
                {
                    await _persistenceService.DeleteAsync(userActivity.Id, USER_ACTIVITY_CONTAINER, userActivity.PartitionKey);
                }

                // Update TaskWithUserInfo read models to mark user as inactive/deleted
                await UpdateTaskReadModelsForDeletedUser(userEvent.UserId);

                // Update dashboard metrics
                await UpdateDashboardMetrics();

                _logger.LogInformation("Successfully processed UserDeletedEvent for user {UserId}", userEvent.UserId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to process UserDeletedEvent for user {UserId}", userEvent.UserId);
                throw;
            }
        }

        public async Task RebuildProjectionsAsync()
        {
            try
            {
                _logger.LogInformation("Starting projection rebuild");

                // This would typically involve:
                // 1. Reading all tasks and users from their respective containers
                // 2. Rebuilding all read models from scratch
                // 3. This is expensive but ensures consistency

                await Task.Run(() => _logger.LogWarning("Projection rebuild not yet implemented - this would be a full system resync"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to rebuild projections");
                throw;
            }
        }

        private async Task UpdateTaskWithUserInfoReadModel(string taskId, object taskEvent)
        {
            // This method would need to fetch the full task details and user details
            // to create/update the denormalized read model
            _logger.LogInformation("Updating TaskWithUserInfo read model for task {TaskId}", taskId);
            
            // Implementation would depend on having access to task and user data
            // For now, this is a placeholder for the projection logic
            await Task.CompletedTask;
        }

        private async Task UpdateUserActivityForTaskEvent(string userId, string activityType, string taskId, string taskTitle, Models.TaskStatus? previousStatus = null, Models.TaskStatus? newStatus = null)
        {
            try
            {
                var userActivity = await _persistenceService.GetAsync<UserActivityReadModel>(userId, USER_ACTIVITY_CONTAINER, userId);
                if (userActivity == null)
                {
                    _logger.LogWarning("UserActivity read model not found for user {UserId}", userId);
                    return;
                }

                // Update activity
                userActivity.LastActivity = DateTime.UtcNow;
                userActivity.LastProjectionUpdate = DateTime.UtcNow;

                // Add to recent activities
                var recentActivity = new RecentTaskActivity
                {
                    TaskId = taskId,
                    TaskTitle = taskTitle,
                    ActivityType = activityType,
                    ActivityDate = DateTime.UtcNow,
                    PreviousStatus = previousStatus,
                    NewStatus = newStatus
                };

                userActivity.RecentActivities.Insert(0, recentActivity);
                
                // Keep only last 10 activities
                if (userActivity.RecentActivities.Count > 10)
                {
                    userActivity.RecentActivities = userActivity.RecentActivities.Take(10).ToList();
                }

                // Update statistics based on activity type
                switch (activityType)
                {
                    case "TaskCreated":
                    case "TaskCreatedByUser":
                        userActivity.TasksCreatedByUser++;
                        break;
                    case "TaskCompleted":
                        userActivity.TasksCompleted++;
                        break;
                }

                await _persistenceService.UpdateAsync(userActivity, USER_ACTIVITY_CONTAINER, userActivity.Id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to update UserActivity for user {UserId}", userId);
            }
        }

        private async Task UpdateTaskReadModelsForUser(string userId, string userName, string email)
        {
            // Update all TaskWithUserInfo read models that reference this user
            _logger.LogInformation("Updating TaskWithUserInfo read models for user {UserId}", userId);
            
            // Implementation would query for tasks assigned to or created by this user
            // and update the denormalized user information
            await Task.CompletedTask;
        }

        private async Task UpdateTaskReadModelsForDeletedUser(string userId)
        {
            // Mark user information as inactive in TaskWithUserInfo read models
            _logger.LogInformation("Updating TaskWithUserInfo read models for deleted user {UserId}", userId);
            await Task.CompletedTask;
        }

        private async Task UpdateDashboardMetrics()
        {
            try
            {
                // This would aggregate data from various containers to update dashboard metrics
                _logger.LogInformation("Updating dashboard metrics");
                
                // Implementation would:
                // 1. Count tasks by status
                // 2. Count active users
                // 3. Calculate performance metrics
                // 4. Update the DashboardMetricsReadModel
                await Task.CompletedTask;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to update dashboard metrics");
            }
        }
    }
}
