using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using Microsoft.Azure.Cosmos; 
using TaskManagement.Shared.Services;
using System;

var host = new HostBuilder()
 .ConfigureFunctionsWorkerDefaults(worker =>
 {
     // Configure CORS for Blazor WebAssembly
     worker.Services.AddCors(options =>
     {
         options.AddDefaultPolicy(policy =>
         {
             policy.AllowAnyOrigin()
                   .AllowAnyMethod()
                   .AllowAnyHeader();
         });
     });
 }).ConfigureServices(services =>
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

        // Register event publisher (Service Bus with fallback to logging)
        services.AddSingleton<IEventPublisher, ServiceBusEventPublisher>();

        // Register materialized view service for Cosmos DB native views
        services.AddSingleton<IMaterializedViewService, MaterializedViewService>();

        // Register HttpClient for inter-service communication
        services.AddHttpClient();
        
        // Register database initializer
        services.AddSingleton<DatabaseInitializer>();
        
        // Initialize database on startup
        var serviceProvider = services.BuildServiceProvider();
        var dbInitializer = serviceProvider.GetRequiredService<DatabaseInitializer>();
        dbInitializer.InitializeAsync(databaseName, "tasks").GetAwaiter().GetResult();
    })
    .Build();

host.Run();
