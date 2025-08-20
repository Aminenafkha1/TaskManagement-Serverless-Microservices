using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using TaskManagement.DatabaseSeeder.Contexts;
using TaskManagement.Models;

namespace TaskManagement.DatabaseSeeder.Seeders
{
    public static class UsersSeeder
    {
        public static async Task SeedAsync(UsersDbContext context, ILogger logger)
        {
            if (await context.Users.AnyAsync())
            {
                logger.LogInformation("Users database already contains data. Skipping seeding.");
                return;
            }

            logger.LogInformation("Seeding Users database...");

            var users = new List<User>
            {
                new User
                {
                    Id = Guid.NewGuid().ToString(),
                    FirstName = "John",
                    LastName = "Admin",
                    Email = "admin@taskmanagement.com",
                    PhoneNumber = "+1234567890",
                    Role = UserRole.Admin,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow.AddDays(-30),
                    UpdatedAt = DateTime.UtcNow.AddDays(-30)
                },
                new User
                {
                    Id = Guid.NewGuid().ToString(),
                    FirstName = "Sarah",
                    LastName = "Manager",
                    Email = "manager@taskmanagement.com",
                    PhoneNumber = "+1234567891",
                    Role = UserRole.Manager,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow.AddDays(-25),
                    UpdatedAt = DateTime.UtcNow.AddDays(-25)
                },
                new User
                {
                    Id = Guid.NewGuid().ToString(),
                    FirstName = "Mike",
                    LastName = "Developer",
                    Email = "mike@taskmanagement.com",
                    PhoneNumber = "+1234567892",
                    Role = UserRole.User,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow.AddDays(-20),
                    UpdatedAt = DateTime.UtcNow.AddDays(-20)
                },
                new User
                {
                    Id = Guid.NewGuid().ToString(),
                    FirstName = "Emma",
                    LastName = "Designer",
                    Email = "emma@taskmanagement.com",
                    PhoneNumber = "+1234567893",
                    Role = UserRole.User,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow.AddDays(-15),
                    UpdatedAt = DateTime.UtcNow.AddDays(-15)
                },
                new User
                {
                    Id = Guid.NewGuid().ToString(),
                    FirstName = "David",
                    LastName = "Tester",
                    Email = "david@taskmanagement.com",
                    PhoneNumber = "+1234567894",
                    Role = UserRole.User,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow.AddDays(-10),
                    UpdatedAt = DateTime.UtcNow.AddDays(-10)
                }
            };

            await context.Users.AddRangeAsync(users);
            await context.SaveChangesAsync();

            logger.LogInformation("Seeded {Count} users", users.Count);
        }
    }

    public static class TasksSeeder
    {
        public static async Task SeedAsync(TasksDbContext context, ILogger logger)
        {
            if (await context.Tasks.AnyAsync())
            {
                logger.LogInformation("Tasks database already contains data. Skipping seeding.");
                return;
            }

            logger.LogInformation("Seeding Tasks database...");

            var tasks = new List<TaskItem>
            {
                new TaskItem
                {
                    Id = Guid.NewGuid(),
                    Title = "Setup Development Environment",
                    Description = "Configure development tools and environment for the new project",
                    Status = Models.TaskStatus.Done,
                    Priority = TaskPriority.High,
                    AssignedToUserId = Guid.Parse("33333333-3333-3333-3333-333333333333"), // Mike
                    CreatedByUserId = Guid.Parse("22222222-2222-2222-2222-222222222222"), // Sarah
                    Category = "Development",
                    Tags = new List<string> { "setup", "environment", "development" },
                    CreatedAt = DateTime.UtcNow.AddDays(-30),
                    UpdatedAt = DateTime.UtcNow.AddDays(-25),
                    DueDate = DateTime.UtcNow.AddDays(-28),
                    CompletedAt = DateTime.UtcNow.AddDays(-25)
                },
                new TaskItem
                {
                    Id = Guid.NewGuid(),
                    Title = "Design User Interface Mockups",
                    Description = "Create wireframes and mockups for the main application screens",
                    Status = Models.TaskStatus.InProgress,
                    Priority = TaskPriority.Medium,
                    AssignedToUserId = Guid.Parse("44444444-4444-4444-4444-444444444444"), // Emma
                    CreatedByUserId = Guid.Parse("22222222-2222-2222-2222-222222222222"), // Sarah
                    Category = "Design",
                    Tags = new List<string> { "ui", "design", "mockups" },
                    CreatedAt = DateTime.UtcNow.AddDays(-20),
                    UpdatedAt = DateTime.UtcNow.AddDays(-5),
                    DueDate = DateTime.UtcNow.AddDays(5)
                },
                new TaskItem
                {
                    Id = Guid.NewGuid(),
                    Title = "Implement User Authentication",
                    Description = "Develop login, registration, and password reset functionality",
                    Status = Models.TaskStatus.Todo,
                    Priority = TaskPriority.Critical,
                    AssignedToUserId = Guid.Parse("33333333-3333-3333-3333-333333333333"), // Mike
                    CreatedByUserId = Guid.Parse("11111111-1111-1111-1111-111111111111"), // Admin
                    Category = "Development",
                    Tags = new List<string> { "authentication", "security", "backend" },
                    CreatedAt = DateTime.UtcNow.AddDays(-15),
                    UpdatedAt = DateTime.UtcNow.AddDays(-15),
                    DueDate = DateTime.UtcNow.AddDays(10)
                },
                new TaskItem
                {
                    Id = Guid.NewGuid(),
                    Title = "Create Test Plans",
                    Description = "Develop comprehensive test plans for all application features",
                    Status = Models.TaskStatus.Todo,
                    Priority = TaskPriority.Medium,
                    AssignedToUserId = Guid.Parse("55555555-5555-5555-5555-555555555555"), // David
                    CreatedByUserId = Guid.Parse("22222222-2222-2222-2222-222222222222"), // Sarah
                    Category = "Testing",
                    Tags = new List<string> { "testing", "qa", "documentation" },
                    CreatedAt = DateTime.UtcNow.AddDays(-10),
                    UpdatedAt = DateTime.UtcNow.AddDays(-10),
                    DueDate = DateTime.UtcNow.AddDays(15)
                },
                new TaskItem
                {
                    Id = Guid.NewGuid(),
                    Title = "Database Schema Design",
                    Description = "Design and implement the database schema for all microservices",
                    Status = Models.TaskStatus.Review,
                    Priority = TaskPriority.High,
                    AssignedToUserId = Guid.Parse("33333333-3333-3333-3333-333333333333"), // Mike
                    CreatedByUserId = Guid.Parse("11111111-1111-1111-1111-111111111111"), // Admin
                    Category = "Development",
                    Tags = new List<string> { "database", "schema", "architecture" },
                    CreatedAt = DateTime.UtcNow.AddDays(-18),
                    UpdatedAt = DateTime.UtcNow.AddDays(-2),
                    DueDate = DateTime.UtcNow.AddDays(3)
                }
            };

            await context.Tasks.AddRangeAsync(tasks);
            await context.SaveChangesAsync();

            logger.LogInformation("Seeded {Count} tasks", tasks.Count);
        }
    }

    public static class NotificationsSeeder
    {
        public static async Task SeedAsync(NotificationsDbContext context, ILogger logger)
        {
            if (await context.Notifications.AnyAsync())
            {
                logger.LogInformation("Notifications database already contains data. Skipping seeding.");
                return;
            }

            logger.LogInformation("Seeding Notifications database...");

            var notifications = new List<Notification>
            {
                new Notification
                {
                    Id = Guid.NewGuid(),
                    UserId = Guid.Parse("33333333-3333-3333-3333-333333333333"), // Mike
                    Title = "New Task Assigned",
                    Message = "You have been assigned a new task: 'Implement User Authentication'",
                    Type = NotificationType.TaskAssigned,
                    IsRead = false,
                    CreatedAt = DateTime.UtcNow.AddDays(-15),
                    RelatedEntityType = "Task"
                },
                new Notification
                {
                    Id = Guid.NewGuid(),
                    UserId = Guid.Parse("44444444-4444-4444-4444-444444444444"), // Emma
                    Title = "Task Due Soon",
                    Message = "Your task 'Design User Interface Mockups' is due in 5 days",
                    Type = NotificationType.TaskDueSoon,
                    IsRead = false,
                    CreatedAt = DateTime.UtcNow.AddHours(-6),
                    RelatedEntityType = "Task"
                },
                new Notification
                {
                    Id = Guid.NewGuid(),
                    UserId = Guid.Parse("22222222-2222-2222-2222-222222222222"), // Sarah
                    Title = "Task Status Updated",
                    Message = "Task 'Setup Development Environment' has been completed by Mike",
                    Type = NotificationType.TaskStatusChanged,
                    IsRead = true,
                    CreatedAt = DateTime.UtcNow.AddDays(-25),
                    ReadAt = DateTime.UtcNow.AddDays(-24),
                    RelatedEntityType = "Task"
                },
                new Notification
                {
                    Id = Guid.NewGuid(),
                    UserId = Guid.Parse("55555555-5555-5555-5555-555555555555"), // David
                    Title = "Welcome to TaskManagement",
                    Message = "Welcome to the TaskManagement system! Please complete your profile setup.",
                    Type = NotificationType.SystemAlert,
                    IsRead = false,
                    CreatedAt = DateTime.UtcNow.AddDays(-10),
                    RelatedEntityType = "User"
                },
                new Notification
                {
                    Id = Guid.NewGuid(),
                    UserId = Guid.Parse("11111111-1111-1111-1111-111111111111"), // Admin
                    Title = "System Maintenance",
                    Message = "Scheduled system maintenance will occur this weekend from 2-4 AM",
                    Type = NotificationType.SystemAlert,
                    IsRead = true,
                    CreatedAt = DateTime.UtcNow.AddDays(-3),
                    ReadAt = DateTime.UtcNow.AddDays(-3),
                    RelatedEntityType = "System"
                }
            };

            await context.Notifications.AddRangeAsync(notifications);
            await context.SaveChangesAsync();

            logger.LogInformation("Seeded {Count} notifications", notifications.Count);
        }
    }

    public static class ReportsSeeder
    {
        public static async Task SeedAsync(ReportsDbContext context, ILogger logger)
        {
            if (await context.Reports.AnyAsync())
            {
                logger.LogInformation("Reports database already contains data. Skipping seeding.");
                return;
            }

            logger.LogInformation("Seeding Reports database...");

            var reports = new List<Report>
            {
                new Report
                {
                    Id = Guid.NewGuid(),
                    Title = "Weekly Task Summary",
                    Type = ReportType.TaskSummary,
                    GeneratedByUserId = Guid.Parse("22222222-2222-2222-2222-222222222222"), // Sarah
                    GeneratedAt = DateTime.UtcNow.AddDays(-7),
                    PeriodStart = DateTime.UtcNow.AddDays(-14),
                    PeriodEnd = DateTime.UtcNow.AddDays(-7),
                    Data = """
                    {
                      "totalTasks": 25,
                      "completedTasks": 8,
                      "inProgressTasks": 12,
                      "pendingTasks": 5,
                      "completionRate": 32
                    }
                    """
                },
                new Report
                {
                    Id = Guid.NewGuid(),
                    Title = "User Productivity Report",
                    Type = ReportType.UserProductivity,
                    GeneratedByUserId = Guid.Parse("11111111-1111-1111-1111-111111111111"), // Admin
                    GeneratedAt = DateTime.UtcNow.AddDays(-5),
                    PeriodStart = DateTime.UtcNow.AddDays(-30),
                    PeriodEnd = DateTime.UtcNow.AddDays(-1),
                    Data = """
                    {
                      "users": [
                        {"userId": "33333333-3333-3333-3333-333333333333", "tasksCompleted": 15, "avgCompletionTime": 2.5},
                        {"userId": "44444444-4444-4444-4444-444444444444", "tasksCompleted": 12, "avgCompletionTime": 3.2},
                        {"userId": "55555555-5555-5555-5555-555555555555", "tasksCompleted": 8, "avgCompletionTime": 1.8}
                      ]
                    }
                    """
                },
                new Report
                {
                    Id = Guid.NewGuid(),
                    Title = "Team Performance Overview",
                    Type = ReportType.TeamPerformance,
                    GeneratedByUserId = Guid.Parse("22222222-2222-2222-2222-222222222222"), // Sarah
                    GeneratedAt = DateTime.UtcNow.AddDays(-2),
                    PeriodStart = DateTime.UtcNow.AddDays(-30),
                    PeriodEnd = DateTime.UtcNow,
                    Data = """
                    {
                      "teamMetrics": {
                        "totalTasksAssigned": 45,
                        "totalTasksCompleted": 35,
                        "averageTaskCompletionTime": 2.8,
                        "onTimeDeliveryRate": 78,
                        "teamVelocity": 12.5
                      }
                    }
                    """
                }
            };

            await context.Reports.AddRangeAsync(reports);
            await context.SaveChangesAsync();

            logger.LogInformation("Seeded {Count} reports", reports.Count);
        }
    }
}
