using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace TaskManagement.Models
{
    // =================== USER SERVICE MODELS ===================
    public class User
    {
        [JsonProperty("id")]
        public string Id { get; set; } = Guid.NewGuid().ToString();

        [Required]
        [StringLength(100)]
        [JsonProperty("firstName")]
        public string FirstName { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        [JsonProperty("lastName")]
        public string LastName { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        [StringLength(255)]
        [JsonProperty("email")]
        public string Email { get; set; } = string.Empty;

        [StringLength(500)]
        public string? PasswordHash { get; set; }

        [StringLength(50)]
        public string PhoneNumber { get; set; } = string.Empty;

        [Required]
        public UserRole Role { get; set; } = UserRole.User;

        public bool IsActive { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        [StringLength(500)]
        public string? ProfileImageUrl { get; set; }

        // Azure AD Integration
        [StringLength(255)]
        public string? AzureAdObjectId { get; set; }
    }

    public enum UserRole
    {
        User = 0,
        Manager = 1,
        Admin = 2
    }

    // =================== TASK SERVICE MODELS ===================
    public class TaskItem
    {
        [Key]
        [JsonPropertyName("id")]
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        [StringLength(200)]
        public string Title { get; set; } = string.Empty;

        [StringLength(1000)]
        public string Description { get; set; } = string.Empty;

        [Required]
        public TaskStatus Status { get; set; } = TaskStatus.Todo;

        [Required]
        public TaskPriority Priority { get; set; } = TaskPriority.Medium;

        public Guid AssignedToUserId { get; set; } // Reference to User service
        public Guid CreatedByUserId { get; set; } // Reference to User service

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? DueDate { get; set; }
        public DateTime? CompletedAt { get; set; }

        [StringLength(100)]
        public string Category { get; set; } = string.Empty;

        public List<string> Tags { get; set; } = new List<string>();
    }

    public enum TaskStatus
    {
        Todo = 0,
        InProgress = 1,
        Review = 2,
        Done = 3,
        Cancelled = 4
    }

    public enum TaskPriority
    {
        Low = 0,
        Medium = 1,
        High = 2,
        Critical = 3
    }

    // =================== NOTIFICATION SERVICE MODELS ===================
    public class Notification
    {
        [Key]
        [JsonPropertyName("id")]
        public Guid Id { get; set; } = Guid.NewGuid();

        public Guid UserId { get; set; } // Reference to User service

        [Required]
        [StringLength(200)]
        public string Title { get; set; } = string.Empty;

        [Required]
        [StringLength(1000)]
        public string Message { get; set; } = string.Empty;

        [Required]
        public NotificationType Type { get; set; }

        public bool IsRead { get; set; } = false;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? ReadAt { get; set; }

        public Guid? RelatedEntityId { get; set; } // Task ID, User ID, etc.

        [StringLength(50)]
        public string? RelatedEntityType { get; set; } // "Task", "User", etc.
    }

    public enum NotificationType
    {
        TaskAssigned = 0,
        TaskStatusChanged = 1,
        TaskDueSoon = 2,
        TaskOverdue = 3,
        UserMentioned = 4,
        SystemAlert = 5
    }

    public enum NotificationPriority
    {
        Low = 0,
        Normal = 1,
        High = 2,
        Critical = 3
    }

    public enum NotificationChannel
    {
        InApp = 0,
        Email = 1,
        SMS = 2,
        Push = 3
    }

    // =================== REPORTING SERVICE MODELS ===================
    public class Report
    {
        [Key]
        [JsonPropertyName("id")]
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        [StringLength(200)]
        public string Title { get; set; } = string.Empty;

        [Required]
        public ReportType Type { get; set; }

        public Guid GeneratedByUserId { get; set; } // Reference to User service

        public DateTime GeneratedAt { get; set; } = DateTime.UtcNow;
        public DateTime PeriodStart { get; set; }
        public DateTime PeriodEnd { get; set; }

        public string Data { get; set; } = string.Empty; // JSON data

        [StringLength(500)]
        public string? FilePath { get; set; } // For PDF/Excel reports
    }

    public enum ReportType
    {
        TaskSummary = 0,
        UserProductivity = 1,
        TeamPerformance = 2,
        ProjectProgress = 3
    }

    public enum ReportStatus
    {
        Pending = 0,
        InProgress = 1,
        Completed = 2,
        Failed = 3
    }

    // =================== CONFIGURATION MODELS ===================
    public class ServiceBusSettings
    {
        public string ConnectionString { get; set; } = string.Empty;
        public string UserEventsTopic { get; set; } = string.Empty;
        public string TaskEventsTopic { get; set; } = string.Empty;
        public string NotificationEventsTopic { get; set; } = string.Empty;
        public string ReportEventsTopic { get; set; } = string.Empty;
    }
}
