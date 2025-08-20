using System.ComponentModel.DataAnnotations;
using TaskManagement.Models;

namespace TaskManagement.Models.ReadModels
{
    /// <summary>
    /// Read model for dashboard overview combining system-wide metrics
    /// This provides aggregated statistics for administrators and managers
    /// </summary>
    public class DashboardMetricsReadModel
    {
        [Required]
        public string Id { get; set; } = "dashboard-metrics";
        
        public DateTime GeneratedAt { get; set; }
        
        // System-wide task statistics
        public int TotalTasks { get; set; }
        
        public int TasksCompleted { get; set; }
        
        public int TasksInProgress { get; set; }
        
        public int TasksPending { get; set; }
        
        public int TasksOverdue { get; set; }
        
        // User statistics
        public int TotalUsers { get; set; }
        
        public int ActiveUsersThisWeek { get; set; }
        
        public int ActiveUsersThisMonth { get; set; }
        
        // Priority distribution
        public TaskPriorityDistribution PriorityDistribution { get; set; } = new();
        
        // Status distribution
        public TaskStatusDistribution StatusDistribution { get; set; } = new();
        
        // Performance metrics
        public double AverageTaskCompletionTime { get; set; } // in days
        
        public double SystemProductivityScore { get; set; }
        
        // Recent trends (last 30 days)
        public List<DailyTaskMetrics> RecentTrends { get; set; } = new List<DailyTaskMetrics>();
        
        // Top performers
        public List<TopPerformer> TopPerformers { get; set; } = new List<TopPerformer>();
        
        // Overdue tasks by user
        public List<OverdueTaskSummary> OverdueTasksByUser { get; set; } = new List<OverdueTaskSummary>();
        
        // Metadata
        public DateTime LastProjectionUpdate { get; set; }
        
        public string ProjectionVersion { get; set; } = "1.0";
        
        // Partition key for Cosmos DB
        public string PartitionKey => "dashboard";
        
        // Computed properties
        public double TaskCompletionRate => TotalTasks > 0 ? (double)TasksCompleted / TotalTasks * 100 : 0;
        
        public double OverdueTaskPercentage => TotalTasks > 0 ? (double)TasksOverdue / TotalTasks * 100 : 0;
        
        public string SystemHealthStatus
        {
            get
            {
                if (OverdueTaskPercentage > 30) return "Critical";
                if (OverdueTaskPercentage > 20) return "Warning";
                if (TaskCompletionRate > 80) return "Excellent";
                if (TaskCompletionRate > 60) return "Good";
                return "Average";
            }
        }
    }
    
    public class TaskPriorityDistribution
    {
        public int High { get; set; }
        public int Medium { get; set; }
        public int Low { get; set; }
    }
    
    public class TaskStatusDistribution
    {
        public int Todo { get; set; }
        public int InProgress { get; set; }
        public int Review { get; set; }
        public int Done { get; set; }
        public int Cancelled { get; set; }
    }
    
    public class DailyTaskMetrics
    {
        public DateTime Date { get; set; }
        public int TasksCreated { get; set; }
        public int TasksCompleted { get; set; }
        public int TasksOverdue { get; set; }
        public int ActiveUsers { get; set; }
    }
    
    public class TopPerformer
    {
        public string UserId { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public int TasksCompleted { get; set; }
        public double CompletionRate { get; set; }
        public double AverageCompletionTime { get; set; }
    }
    
    public class OverdueTaskSummary
    {
        public string UserId { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public int OverdueTaskCount { get; set; }
        public int DaysOldestOverdueTask { get; set; }
        public List<OverdueTaskInfo> OverdueTasks { get; set; } = new List<OverdueTaskInfo>();
    }
    
    public class OverdueTaskInfo
    {
        public string TaskId { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public TaskPriority Priority { get; set; }
        public DateTime DueDate { get; set; }
        public int DaysOverdue { get; set; }
    }
}
