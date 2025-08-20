using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace TaskManagement.Models.Events
{
    // Base Event Class
    public abstract class BaseEvent
    {
        public Guid EventId { get; set; } = Guid.NewGuid();
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
        public string EventType { get; set; } = string.Empty;
        public string Source { get; set; } = string.Empty;
        public string Version { get; set; } = "1.0";
    }

    // Task Events
    public class TaskCreatedEvent : BaseEvent
    {
        public TaskCreatedEvent()
        {
            EventType = "TaskCreated";
            Source = "TasksService";
        }

        public Guid TaskId { get; set; }
        public string Title { get; set; } = string.Empty;
        public Guid AssignedToUserId { get; set; }
        public Guid CreatedByUserId { get; set; }
        public TaskPriority Priority { get; set; }
        public DateTime? DueDate { get; set; }
    }

    public class TaskUpdatedEvent : BaseEvent
    {
        public TaskUpdatedEvent()
        {
            EventType = "TaskUpdated";
            Source = "TasksService";
        }

        public Guid TaskId { get; set; }
        public string Title { get; set; } = string.Empty;
        public TaskStatus OldStatus { get; set; }
        public TaskStatus NewStatus { get; set; }
        public TaskPriority OldPriority { get; set; }
        public TaskPriority NewPriority { get; set; }
        public Guid? OldAssignedToUserId { get; set; }
        public Guid? NewAssignedToUserId { get; set; }
        public Guid UpdatedByUserId { get; set; }
        public Dictionary<string, object> Changes { get; set; } = new();
    }

    public class TaskCompletedEvent : BaseEvent
    {
        public TaskCompletedEvent()
        {
            EventType = "TaskCompleted";
            Source = "TasksService";
        }

        public Guid TaskId { get; set; }
        public string Title { get; set; } = string.Empty;
        public Guid AssignedToUserId { get; set; }
        public Guid CompletedByUserId { get; set; }
        public DateTime CompletedAt { get; set; }
        public DateTime? DueDate { get; set; }
        public bool IsOverdue { get; set; }
    }

    public class TaskAssignedEvent : BaseEvent
    {
        public TaskAssignedEvent()
        {
            EventType = "TaskAssigned";
            Source = "TasksService";
        }

        public Guid TaskId { get; set; }
        public string Title { get; set; } = string.Empty;
        public Guid AssignedToUserId { get; set; }
        public Guid AssignedByUserId { get; set; }
        public TaskPriority Priority { get; set; }
        public DateTime? DueDate { get; set; }
    }

    public class TaskDeletedEvent : BaseEvent
    {
        public TaskDeletedEvent()
        {
            EventType = "TaskDeleted";
            Source = "TasksService";
        }

        public Guid TaskId { get; set; }
        public string Title { get; set; } = string.Empty;
        public Guid DeletedByUserId { get; set; }
    }

    // User Events
    public class UserCreatedEvent : BaseEvent
    {
        public UserCreatedEvent()
        {
            EventType = "UserCreated";
            Source = "UsersService";
        }

        public string UserId { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Department { get; set; } = string.Empty;
        public UserRole Role { get; set; }
    }

    public class UserUpdatedEvent : BaseEvent
    {
        public UserUpdatedEvent()
        {
            EventType = "UserUpdated";
            Source = "UsersService";
        }

        public string UserId { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Department { get; set; } = string.Empty;
        public UserRole Role { get; set; }
        public Dictionary<string, object> Changes { get; set; } = new();
    }

    public class UserDeletedEvent : BaseEvent
    {
        public UserDeletedEvent()
        {
            EventType = "UserDeleted";
            Source = "UsersService";
        }

        public string UserId { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public UserRole Role { get; set; }
    }

    public class UserLoginEvent : BaseEvent
    {
        public UserLoginEvent()
        {
            EventType = "UserLogin";
            Source = "UsersService";
        }

        public string UserId { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public DateTime LoginTime { get; set; }
        public string? IpAddress { get; set; }
        public string? UserAgent { get; set; }
    }

    public class UserLogoutEvent : BaseEvent
    {
        public UserLogoutEvent()
        {
            EventType = "UserLogout";
            Source = "UsersService";
        }

        public string UserId { get; set; } = string.Empty;
        public DateTime LogoutTime { get; set; }
    }
}
