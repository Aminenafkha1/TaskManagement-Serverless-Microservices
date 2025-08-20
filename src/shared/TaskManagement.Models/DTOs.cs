using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace TaskManagement.Models.DTOs
{
    // =================== COMMON DTOs ===================
    public class ApiResponseDto
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public List<string> Errors { get; set; } = new List<string>();
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    }

    public class ApiResponseDto<T> : ApiResponseDto
    {
        public T? Data { get; set; }
    }

    public class PagedResultDto<T>
    {
        public List<T> Items { get; set; } = new List<T>();
        public int TotalCount { get; set; }
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);
        public bool HasPrevious => PageNumber > 1;
        public bool HasNext => PageNumber < TotalPages;
    }

    // =================== USER DTOs ===================
    public class UserDto
    {
        public string Id { get; set; } = string.Empty;
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public UserRole Role { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public string? ProfileImageUrl { get; set; }
        public string FullName => $"{FirstName} {LastName}";
    }

    public class CreateUserDto
    {
        [Required]
        [StringLength(100)]
        public string FirstName { get; set; } = string.Empty;
        
        [Required]
        [StringLength(100)]
        public string LastName { get; set; } = string.Empty;
        
        [Required]
        [EmailAddress]
        [StringLength(255)]
        public string Email { get; set; } = string.Empty;
        
        [StringLength(50)]
        public string PhoneNumber { get; set; } = string.Empty;
        
        [Required]
        public UserRole Role { get; set; } = UserRole.User;
        
        [StringLength(500)]
        public string? ProfileImageUrl { get; set; }
        
        [StringLength(255)]
        public string? AzureAdObjectId { get; set; }
    }

    public class UpdateUserDto
    {
        [StringLength(100)]
        public string? FirstName { get; set; }
        
        [StringLength(100)]
        public string? LastName { get; set; }
        
        [EmailAddress]
        [StringLength(255)]
        public string? Email { get; set; }
        
        [StringLength(50)]
        public string? PhoneNumber { get; set; }
        
        public UserRole? Role { get; set; }
        
        public bool? IsActive { get; set; }
        
        [StringLength(500)]
        public string? ProfileImageUrl { get; set; }
    }

    public class UserFilterDto
    {
        public UserRole? Role { get; set; }
        public bool? IsActive { get; set; }
        public string? SearchTerm { get; set; }
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
    }

    // =================== TASK DTOs ===================
    public class TaskDto
    {
        public Guid Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public TaskStatus Status { get; set; }
        public TaskPriority Priority { get; set; }
        public Guid AssignedToUserId { get; set; }
        public Guid CreatedByUserId { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public DateTime? DueDate { get; set; }
        public DateTime? CompletedAt { get; set; }
        public string Category { get; set; } = string.Empty;
        public List<string> Tags { get; set; } = new List<string>();
        
        // Navigation properties (populated by service calls)
        public string? AssignedToUserName { get; set; }
        public string? CreatedByUserName { get; set; }
    }

    public class TaskItemDto
    {
        public Guid Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public TaskStatus Status { get; set; }
        public TaskPriority Priority { get; set; }
        public Guid? AssignedToUserId { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public DateTime? DueDate { get; set; }
    }

    public class CreateTaskDto
    {
        [Required]
        [StringLength(200)]
        public string Title { get; set; } = string.Empty;
        
        [StringLength(1000)]
        public string Description { get; set; } = string.Empty;
        
        [Required]
        public TaskPriority Priority { get; set; } = TaskPriority.Medium;
        
        public Guid AssignedToUserId { get; set; }
        public Guid CreatedByUserId { get; set; }
        
        public DateTime? DueDate { get; set; }
        
        [StringLength(100)]
        public string Category { get; set; } = string.Empty;
        
        public List<string> Tags { get; set; } = new List<string>();
    }

    public class CreateTaskItemDto
    {
        [Required]
        [StringLength(200)]
        public string Title { get; set; } = string.Empty;
        
        [StringLength(1000)]
        public string Description { get; set; } = string.Empty;
        
        [Required]
        public TaskPriority Priority { get; set; } = TaskPriority.Medium;
        
        [Required]
        public TaskStatus Status { get; set; } = TaskStatus.Todo;
        
        public Guid? AssignedToUserId { get; set; }
        
        public DateTime? DueDate { get; set; }
    }

    public class UpdateTaskDto
    {
        [StringLength(200)]
        public string? Title { get; set; }
        
        [StringLength(1000)]
        public string? Description { get; set; }
        
        public TaskStatus? Status { get; set; }
        public TaskPriority? Priority { get; set; }
        public Guid? AssignedToUserId { get; set; }
        public DateTime? DueDate { get; set; }
        
        [StringLength(100)]
        public string? Category { get; set; }
        
        public List<string>? Tags { get; set; }
    }

    public class UpdateTaskItemDto
    {
        [StringLength(200)]
        public string Title { get; set; } = string.Empty;
        
        [StringLength(1000)]
        public string Description { get; set; } = string.Empty;
        
        public TaskStatus Status { get; set; }
        public TaskPriority Priority { get; set; }
        public Guid AssignedToUserId { get; set; }
        public DateTime? DueDate { get; set; }
    }

    public class TaskFilterDto
    {
        public TaskStatus? Status { get; set; }
        public TaskPriority? Priority { get; set; }
        public Guid? AssignedToUserId { get; set; }
        public Guid? CreatedByUserId { get; set; }
        public string? Category { get; set; }
        public string? SearchTerm { get; set; }
        public DateTime? DueDateFrom { get; set; }
        public DateTime? DueDateTo { get; set; }
        public List<string>? Tags { get; set; }
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
    }

    public class TaskSummaryDto
    {
        public Guid Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public TaskStatus Status { get; set; }
        public TaskPriority Priority { get; set; }
        public DateTime? DueDate { get; set; }
        public string AssignedToUserName { get; set; } = string.Empty;
    }

    public class TaskCommentDto
    {
        public Guid Id { get; set; }
        public Guid TaskId { get; set; }
        public Guid UserId { get; set; }
        public string Content { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public string UserName { get; set; } = string.Empty;
    }

    public class CreateTaskCommentDto
    {
        [Required]
        public Guid TaskId { get; set; }
        
        [Required]
        public Guid UserId { get; set; }
        
        [Required]
        [StringLength(1000)]
        public string Content { get; set; } = string.Empty;
    }

    // =================== NOTIFICATION DTOs ===================
    public class NotificationDto
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public NotificationType Type { get; set; }
        public NotificationPriority Priority { get; set; }
        public bool IsRead { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? ReadAt { get; set; }
        public string? ActionUrl { get; set; }
        public Guid? RelatedEntityId { get; set; }
        public string? RelatedEntityType { get; set; }
        public Dictionary<string, object> Metadata { get; set; } = new Dictionary<string, object>();
    }

    public class CreateNotificationDto
    {
        [Required]
        public Guid UserId { get; set; }
        
        [Required]
        [StringLength(200)]
        public string Title { get; set; } = string.Empty;
        
        [Required]
        [StringLength(1000)]
        public string Message { get; set; } = string.Empty;
        
        [Required]
        public NotificationType Type { get; set; }
        
        public NotificationPriority Priority { get; set; } = NotificationPriority.Normal;
        
        [StringLength(500)]
        public string? ActionUrl { get; set; }
        
        public Dictionary<string, object> Metadata { get; set; } = new Dictionary<string, object>();
    }

    public class NotificationFilterDto
    {
        public Guid? UserId { get; set; }
        public NotificationType? Type { get; set; }
        public NotificationPriority? Priority { get; set; }
        public bool? IsRead { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
    }

    // =================== REPORTING DTOs ===================
    public class ReportDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public ReportType Type { get; set; }
        public ReportStatus Status { get; set; }
        public Guid GeneratedByUserId { get; set; }
        public DateTime GeneratedAt { get; set; }
        public DateTime? CompletedAt { get; set; }
        public string? FilePath { get; set; }
        public string? ErrorMessage { get; set; }
        public Dictionary<string, object> Parameters { get; set; } = new Dictionary<string, object>();
        public string GeneratedByUserName { get; set; } = string.Empty;
    }

    public class CreateReportDto
    {
        [Required]
        [StringLength(200)]
        public string Name { get; set; } = string.Empty;
        
        [Required]
        public ReportType Type { get; set; }
        
        [Required]
        public Guid GeneratedByUserId { get; set; }
        
        public Dictionary<string, object> Parameters { get; set; } = new Dictionary<string, object>();
    }

    public class ReportFilterDto
    {
        public ReportType? Type { get; set; }
        public ReportStatus? Status { get; set; }
        public Guid? GeneratedByUserId { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
    }

    public class ReportSummaryDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public ReportType Type { get; set; }
        public ReportStatus Status { get; set; }
        public DateTime GeneratedAt { get; set; }
        public string GeneratedByUserName { get; set; } = string.Empty;
    }

    // =================== AUTHENTICATION DTOs ===================
    public class LoginDto
    {
        [Required]
        [EmailAddress]
        [StringLength(255)]
        public string Email { get; set; } = string.Empty;
        
        [Required]
        [StringLength(100, MinimumLength = 6)]
        public string Password { get; set; } = string.Empty;
    }

    public class RegisterDto
    {
        [Required]
        [StringLength(100)]
        public string FirstName { get; set; } = string.Empty;
        
        [Required]
        [StringLength(100)]
        public string LastName { get; set; } = string.Empty;
        
        [Required]
        [EmailAddress]
        [StringLength(255)]
        public string Email { get; set; } = string.Empty;
        
        [Required]
        [StringLength(100, MinimumLength = 6)]
        public string Password { get; set; } = string.Empty;
        
        [StringLength(50)]
        public string? PhoneNumber { get; set; }
        
        public UserRole Role { get; set; } = UserRole.User;
        
        [StringLength(500)]
        public string? ProfileImageUrl { get; set; }
    }

    public class AuthResultDto
    {
        public bool Success { get; set; }
        public string Token { get; set; } = string.Empty;
        public string RefreshToken { get; set; } = string.Empty;
        public DateTime ExpiresAt { get; set; }
        public UserDto? User { get; set; }
        public string Message { get; set; } = string.Empty;
    }

    public class RefreshTokenDto
    {
        [Required]
        public string RefreshToken { get; set; } = string.Empty;
    }

    public class ChangePasswordDto
    {
        [Required]
        public string CurrentPassword { get; set; } = string.Empty;
        
        [Required]
        [StringLength(100, MinimumLength = 6)]
        public string NewPassword { get; set; } = string.Empty;
    }
}
