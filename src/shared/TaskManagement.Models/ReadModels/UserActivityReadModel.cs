using System.ComponentModel.DataAnnotations;
using TaskManagement.Models;

namespace TaskManagement.Models.ReadModels
{
    /// <summary>
    /// Read model that provides user activity and task statistics
    /// This is a denormalized view for dashboard and reporting features
    /// </summary>
    public class UserActivityReadModel
    {
        [Required]
        public string Id { get; set; } = string.Empty;
        
        [Required]
        public string UserId { get; set; } = string.Empty;
        
        [Required]
        public string UserName { get; set; } = string.Empty;
        
        [Required]
        public string Email { get; set; } = string.Empty;
        
        public UserRole Role { get; set; }
        
        public DateTime LastActivity { get; set; }
        
        // Task statistics
        public int TotalTasksAssigned { get; set; }
        
        public int TasksCompleted { get; set; }
        
        public int TasksInProgress { get; set; }
        
        public int TasksPending { get; set; }
        
        public int TasksOverdue { get; set; }
        
        public int TasksCreatedByUser { get; set; }
        
        // Priority breakdown
        public int HighPriorityTasks { get; set; }
        
        public int MediumPriorityTasks { get; set; }
        
        public int LowPriorityTasks { get; set; }
        
        // Performance metrics
        public double AverageTaskCompletionDays { get; set; }
        
        public int TasksCompletedThisWeek { get; set; }
        
        public int TasksCompletedThisMonth { get; set; }
        
        // Recent activity
        public List<RecentTaskActivity> RecentActivities { get; set; } = new List<RecentTaskActivity>();
        
        // Metadata
        public DateTime LastProjectionUpdate { get; set; }
        
        public string ProjectionVersion { get; set; } = "1.0";
        
        // Partition key for Cosmos DB
        public string PartitionKey => UserId;
        
        // Computed properties
        public double TaskCompletionRate => TotalTasksAssigned > 0 ? (double)TasksCompleted / TotalTasksAssigned * 100 : 0;
        
        public bool IsActiveUser => LastActivity > DateTime.UtcNow.AddDays(-7);
        
        public string ProductivityLevel
        {
            get
            {
                if (TaskCompletionRate >= 90) return "Excellent";
                if (TaskCompletionRate >= 75) return "Good";
                if (TaskCompletionRate >= 50) return "Average";
                return "Needs Improvement";
            }
        }
    }
    
    public class RecentTaskActivity
    {
        public string TaskId { get; set; } = string.Empty;
        
        public string TaskTitle { get; set; } = string.Empty;
        
        public string ActivityType { get; set; } = string.Empty; // Created, Updated, Completed, etc.
        
        public DateTime ActivityDate { get; set; }
        
        public TaskStatus? PreviousStatus { get; set; }
        
        public TaskStatus? NewStatus { get; set; }
    }
}
