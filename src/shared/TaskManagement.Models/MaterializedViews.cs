using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace TaskManagement.Models
{
    /// <summary>
    /// Materialized view combining task information with user details
    /// </summary>
    public class TaskWithUserInfoView
    {
        [JsonPropertyName("id")]
        public string id { get; set; } = string.Empty;

        [JsonPropertyName("title")]
        public string title { get; set; } = string.Empty;

        [JsonPropertyName("description")]
        public string? description { get; set; }

        [JsonPropertyName("status")]
        public TaskStatus status { get; set; }

        [JsonPropertyName("priority")]
        public TaskPriority priority { get; set; }

        [JsonPropertyName("createdAt")]
        public DateTime createdAt { get; set; }

        [JsonPropertyName("updatedAt")]
        public DateTime? updatedAt { get; set; }

        [JsonPropertyName("dueDate")]
        public DateTime? dueDate { get; set; }

        [JsonPropertyName("assignedToUserId")]
        public string assignedToUserId { get; set; } = string.Empty;

        [JsonPropertyName("assignedToUserName")]
        public string assignedToUserName { get; set; } = string.Empty;

        [JsonPropertyName("assignedToUserEmail")]
        public string assignedToUserEmail { get; set; } = string.Empty;

        [JsonPropertyName("createdByUserId")]
        public string createdByUserId { get; set; } = string.Empty;

        [JsonPropertyName("createdByUserName")]
        public string createdByUserName { get; set; } = string.Empty;

        [JsonPropertyName("createdByUserEmail")]
        public string createdByUserEmail { get; set; } = string.Empty;

        [JsonPropertyName("lastUpdated")]
        public DateTime lastUpdated { get; set; }
    }

    /// <summary>
    /// Materialized view for user activity and statistics
    /// </summary>
    public class UserActivityView
    {
        [JsonPropertyName("id")]
        public string id { get; set; } = string.Empty;

        [JsonPropertyName("userId")]
        public string userId { get; set; } = string.Empty;

        [JsonPropertyName("userName")]
        public string userName { get; set; } = string.Empty;

        [JsonPropertyName("email")]
        public string email { get; set; } = string.Empty;

        [JsonPropertyName("role")]
        public UserRole role { get; set; }

        [JsonPropertyName("totalTasksAssigned")]
        public int totalTasksAssigned { get; set; }

        [JsonPropertyName("tasksCompleted")]
        public int tasksCompleted { get; set; }

        [JsonPropertyName("tasksInProgress")]
        public int tasksInProgress { get; set; }

        [JsonPropertyName("tasksPending")]
        public int tasksPending { get; set; }

        [JsonPropertyName("tasksOverdue")]
        public int tasksOverdue { get; set; }

        [JsonPropertyName("averageCompletionDays")]
        public double? averageCompletionDays { get; set; }

        [JsonPropertyName("lastActivity")]
        public DateTime lastActivity { get; set; }

        [JsonPropertyName("recentTaskIds")]
        public List<string> recentTaskIds { get; set; } = new List<string>();

        [JsonPropertyName("lastUpdated")]
        public DateTime lastUpdated { get; set; }
    }

    /// <summary>
    /// Materialized view for dashboard metrics and system overview
    /// </summary>
    public class DashboardView
    {
        [JsonPropertyName("id")]
        public string id { get; set; } = string.Empty;

        [JsonPropertyName("totalTasks")]
        public int totalTasks { get; set; }

        [JsonPropertyName("tasksCompleted")]
        public int tasksCompleted { get; set; }

        [JsonPropertyName("tasksInProgress")]
        public int tasksInProgress { get; set; }

        [JsonPropertyName("tasksPending")]
        public int tasksPending { get; set; }

        [JsonPropertyName("tasksOverdue")]
        public int tasksOverdue { get; set; }

        [JsonPropertyName("totalUsers")]
        public int totalUsers { get; set; }

        [JsonPropertyName("activeUsers")]
        public int activeUsers { get; set; }

        [JsonPropertyName("dailyMetrics")]
        public List<DailyMetric> dailyMetrics { get; set; } = new List<DailyMetric>();

        [JsonPropertyName("lastUpdated")]
        public DateTime lastUpdated { get; set; }
    }

    /// <summary>
    /// Daily metrics for dashboard tracking
    /// </summary>
    public class DailyMetric
    {
        [JsonPropertyName("date")]
        public DateTime date { get; set; }

        [JsonPropertyName("tasksCreated")]
        public int tasksCreated { get; set; }

        [JsonPropertyName("tasksCompleted")]
        public int tasksCompleted { get; set; }

        [JsonPropertyName("activeUsers")]
        public int activeUsers { get; set; }
    }
}
