using System.ComponentModel.DataAnnotations;
using TaskManagement.Models;

namespace TaskManagement.Models.ReadModels
{
    /// <summary>
    /// Read model that combines task information with user details
    /// This is a denormalized view optimized for read operations
    /// </summary>
    public class TaskWithUserInfoReadModel
    {
        [Required]
        public string Id { get; set; } = string.Empty;
        
        [Required]
        public string Title { get; set; } = string.Empty;
        
        public string Description { get; set; } = string.Empty;
        
        [Required]
        public TaskStatus Status { get; set; }
        
        [Required]
        public TaskPriority Priority { get; set; }
        
        [Required]
        public DateTime CreatedAt { get; set; }
        
        public DateTime? UpdatedAt { get; set; }
        
        public DateTime? DueDate { get; set; }
        
        // Denormalized user information
        [Required]
        public string AssignedToUserId { get; set; } = string.Empty;
        
        public string AssignedToUserName { get; set; } = string.Empty;
        
        public string AssignedToUserEmail { get; set; } = string.Empty;
        
        [Required]
        public string CreatedByUserId { get; set; } = string.Empty;
        
        public string CreatedByUserName { get; set; } = string.Empty;
        
        public string CreatedByUserEmail { get; set; } = string.Empty;
        
        // Metadata for tracking and optimization
        public DateTime LastProjectionUpdate { get; set; }
        
        public string ProjectionVersion { get; set; } = "1.0";
        
        // Partition key for Cosmos DB
        public string PartitionKey => AssignedToUserId;
        
        // Search and filtering optimization fields
        public List<string> Tags { get; set; } = new List<string>();
        
        public bool IsOverdue => DueDate.HasValue && DueDate < DateTime.UtcNow && Status != TaskStatus.Done;
        
        public int DaysUntilDue => DueDate.HasValue ? (int)(DueDate.Value - DateTime.UtcNow).TotalDays : 0;
        
        // Computed fields for easier querying
        public string StatusDisplayName => Status.ToString();
        
        public string PriorityDisplayName => Priority.ToString();
        
        public string AssignedToDisplayName => !string.IsNullOrEmpty(AssignedToUserName) ? AssignedToUserName : AssignedToUserEmail;
        
        public string CreatedByDisplayName => !string.IsNullOrEmpty(CreatedByUserName) ? CreatedByUserName : CreatedByUserEmail;
    }
}
