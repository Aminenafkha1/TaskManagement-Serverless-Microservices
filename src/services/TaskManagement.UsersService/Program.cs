using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using Microsoft.Azure.Cosmos;
using TaskManagement.UsersService.Services;
using TaskManagement.Shared.Services;
using System;

var host = new HostBuilder()
    .ConfigureFunctionsWorkerDefaults()
    .ConfigureServices(services =>
    {
        // Get Cosmos DB connection string from environment
        var cosmosConnectionString = Environment.GetEnvironmentVariable("CosmosConnectionString");
        
        if (string.IsNullOrEmpty(cosmosConnectionString))
        {
            throw new InvalidOperationException("CosmosConnectionString environment variable is required");
        }


        // Register Cosmos DB client
        services.AddSingleton<CosmosClient>(provider =>
        {
            return new CosmosClient(cosmosConnectionString);
        });

        // Register Cosmos DB persistence service
        var databaseName = Environment.GetEnvironmentVariable("CosmosDbDatabaseName") ?? "TaskManagementDB";
        services.AddSingleton<IPersistenceService>(provider =>
        {
            var logger = provider.GetRequiredService<ILogger<CosmosPersistenceService>>();
            return new CosmosPersistenceService(cosmosConnectionString, databaseName, logger);
        });

        // Register authentication services
        services.AddSingleton<IPasswordService, PasswordService>();
        services.AddSingleton<IJwtService, JwtService>();
        services.AddSingleton<IAuthService, AuthService>();
        
        // Register event publisher (Service Bus with fallback to logging)
        services.AddSingleton<IEventPublisher, ServiceBusEventPublisher>();
        
        // Register database initializer
        services.AddSingleton<DatabaseInitializer>();
        
        // Initialize database on startup
        var serviceProvider = services.BuildServiceProvider();
        var dbInitializer = serviceProvider.GetRequiredService<DatabaseInitializer>();
        dbInitializer.InitializeAsync(databaseName, "users").GetAwaiter().GetResult();
    })
    .Build();

host.Run();
