using TaskManagement.Models;

namespace TaskManagement.Shared.Services
{
    /// <summary>
    /// Service interface for querying Azure Cosmos DB materialized views
    /// </summary>
    public interface IMaterializedViewService
    {
        /// <summary>
        /// Gets tasks with denormalized user information for a specific user
        /// </summary>
        /// <param name="userId">The user ID to filter tasks for</param>
        /// <returns>List of tasks with user information</returns>
        Task<IEnumerable<TaskWithUserInfoView>> GetTasksWithUserInfoAsync(string userId);

        /// <summary>
        /// Gets all tasks with denormalized user information
        /// </summary>
        /// <returns>List of all tasks with user information</returns>
        Task<IEnumerable<TaskWithUserInfoView>> GetAllTasksWithUserInfoAsync();

        /// <summary>
        /// Gets user activity and statistics for a specific user
        /// </summary>
        /// <param name="userId">The user ID to get activity for</param>
        /// <returns>User activity view with statistics</returns>
        Task<UserActivityView?> GetUserActivityAsync(string userId);

        /// <summary>
        /// Gets user activity for all users
        /// </summary>
        /// <returns>List of user activity views</returns>
        Task<IEnumerable<UserActivityView>> GetAllUserActivityAsync();

        /// <summary>
        /// Gets system-wide dashboard metrics and statistics
        /// </summary>
        /// <returns>Dashboard view with system metrics</returns>
        Task<DashboardView?> GetDashboardViewAsync();

        /// <summary>
        /// Initializes the materialized view containers if they don't exist
        /// </summary>
        /// <returns>Task representing the async operation</returns>
        Task InitializeMaterializedViewsAsync();

        /// <summary>
        /// Gets tasks with user info filtered by status
        /// </summary>
        /// <param name="status">Task status to filter by</param>
        /// <returns>List of tasks with user information</returns>
        Task<IEnumerable<TaskWithUserInfoView>> GetTasksWithUserInfoByStatusAsync(Models.TaskStatus status);

        /// <summary>
        /// Gets overdue tasks with user information
        /// </summary>
        /// <returns>List of overdue tasks with user information</returns>
        Task<IEnumerable<TaskWithUserInfoView>> GetOverdueTasksWithUserInfoAsync();

        /// <summary>
        /// Gets top performing users based on task completion
        /// </summary>
        /// <param name="topCount">Number of top users to return</param>
        /// <returns>List of top performing users</returns>
        Task<IEnumerable<UserActivityView>> GetTopPerformingUsersAsync(int topCount = 10);
    }
}
