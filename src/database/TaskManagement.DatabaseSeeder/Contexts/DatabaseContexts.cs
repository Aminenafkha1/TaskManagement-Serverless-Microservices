using Microsoft.EntityFrameworkCore;
using TaskManagement.Models; 

namespace TaskManagement.DatabaseSeeder.Contexts
{
    public class UsersDbContext : DbContext
    {
        public UsersDbContext(DbContextOptions<UsersDbContext> options) : base(options) { }

        public DbSet<User> Users { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<User>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => e.Email).IsUnique();
                entity.Property(e => e.FirstName).HasMaxLength(100).IsRequired();
                entity.Property(e => e.LastName).HasMaxLength(100).IsRequired();
                entity.Property(e => e.Email).HasMaxLength(255).IsRequired();
                entity.Property(e => e.PhoneNumber).HasMaxLength(50);
                entity.Property(e => e.ProfileImageUrl).HasMaxLength(500);
                entity.Property(e => e.AzureAdObjectId).HasMaxLength(255);
            });
        }
    }

    public class TasksDbContext : DbContext
    {
        public TasksDbContext(DbContextOptions<TasksDbContext> options) : base(options) { }

        public DbSet<TaskItem> Tasks { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<TaskItem>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Title).HasMaxLength(200).IsRequired();
                entity.Property(e => e.Description).HasMaxLength(1000);
                entity.Property(e => e.Category).HasMaxLength(100);
                entity.Property(e => e.Tags)
                    .HasConversion(
                        v => string.Join(',', v),
                        v => v.Split(',', StringSplitOptions.RemoveEmptyEntries).ToList()
                    );
            });
        }
    }

    public class NotificationsDbContext : DbContext
    {
        public NotificationsDbContext(DbContextOptions<NotificationsDbContext> options) : base(options) { }

        public DbSet<Notification> Notifications { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Notification>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Title).HasMaxLength(200).IsRequired();
                entity.Property(e => e.Message).HasMaxLength(1000).IsRequired();
                entity.Property(e => e.RelatedEntityType).HasMaxLength(50);
            });
        }
    }

    public class ReportsDbContext : DbContext
    {
        public ReportsDbContext(DbContextOptions<ReportsDbContext> options) : base(options) { }

        public DbSet<Report> Reports { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Report>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Title).HasMaxLength(200).IsRequired();
                entity.Property(e => e.Data).IsRequired();
                entity.Property(e => e.FilePath).HasMaxLength(500);
            });
        }
    }
}
