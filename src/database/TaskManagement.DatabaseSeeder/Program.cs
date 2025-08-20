using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using TaskManagement.DatabaseSeeder.Contexts;
using TaskManagement.DatabaseSeeder.Seeders;

namespace TaskManagement.DatabaseSeeder
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            Console.WriteLine("TaskManagement Database Seeder");
            Console.WriteLine("==============================");

            var host = CreateHostBuilder(args).Build();

            using var scope = host.Services.CreateScope();
            var services = scope.ServiceProvider;
            var logger = services.GetRequiredService<ILogger<Program>>();

            try
            {
                logger.LogInformation("Starting database seeding process...");

                // Migrate and seed all databases
                await MigrateAndSeedDatabase<UsersDbContext>(services, "Users Database", UsersSeeder.SeedAsync);
                await MigrateAndSeedDatabase<TasksDbContext>(services, "Tasks Database", TasksSeeder.SeedAsync);
                await MigrateAndSeedDatabase<NotificationsDbContext>(services, "Notifications Database", NotificationsSeeder.SeedAsync);
                await MigrateAndSeedDatabase<ReportsDbContext>(services, "Reports Database", ReportsSeeder.SeedAsync);

                logger.LogInformation("Database seeding completed successfully!");
                Console.WriteLine("\n✅ All databases have been created and seeded successfully!");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "An error occurred while seeding the databases");
                Console.WriteLine($"\n❌ Error: {ex.Message}");
                Environment.Exit(1);
            }
        }

        private static async Task MigrateAndSeedDatabase<TContext>(
            IServiceProvider services,
            string databaseName,
            Func<TContext, ILogger, Task> seedFunc) where TContext : DbContext
        {
            var logger = services.GetRequiredService<ILogger<Program>>();
            var context = services.GetRequiredService<TContext>();

            logger.LogInformation("Processing {DatabaseName}...", databaseName);
            Console.WriteLine($"\n🔄 Processing {databaseName}...");

            try
            {
                // Ensure database is created and apply migrations
                await context.Database.MigrateAsync();
                logger.LogInformation("{DatabaseName} migration completed", databaseName);
                Console.WriteLine($"   ✅ Migration completed for {databaseName}");

                // Seed data
                await seedFunc(context, logger);
                logger.LogInformation("{DatabaseName} seeding completed", databaseName);
                Console.WriteLine($"   ✅ Seeding completed for {databaseName}");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error processing {DatabaseName}", databaseName);
                Console.WriteLine($"   ❌ Error processing {databaseName}: {ex.Message}");
                throw;
            }
        }

        private static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureAppConfiguration((context, config) =>
                {
                    config.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);
                    config.AddJsonFile($"appsettings.{context.HostingEnvironment.EnvironmentName}.json", optional: true);
                    config.AddEnvironmentVariables();
                    config.AddCommandLine(args);
                })
                .ConfigureServices((context, services) =>
                {
                    var configuration = context.Configuration;

                    // Register all DbContexts
                    services.AddDbContext<UsersDbContext>(options =>
                        options.UseSqlServer(configuration.GetConnectionString("UsersDbConnectionString")));

                    services.AddDbContext<TasksDbContext>(options =>
                        options.UseSqlServer(configuration.GetConnectionString("TasksDbConnectionString")));

                    services.AddDbContext<NotificationsDbContext>(options =>
                        options.UseSqlServer(configuration.GetConnectionString("NotificationsDbConnectionString")));

                    services.AddDbContext<ReportsDbContext>(options =>
                        options.UseSqlServer(configuration.GetConnectionString("ReportsDbConnectionString")));
                })
                .ConfigureLogging(logging =>
                {
                    logging.ClearProviders();
                    logging.AddConsole();
                });
    }
}
