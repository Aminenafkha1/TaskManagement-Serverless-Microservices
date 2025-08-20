using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using TaskManagement.Shared.Services;
using System;

var host = new HostBuilder()
    .ConfigureFunctionsWorkerDefaults()
    .ConfigureServices(services =>
    {
        services.AddApplicationInsightsTelemetryWorkerService();
        services.ConfigureFunctionsApplicationInsights();

        // Get Cosmos DB connection string from environment
        var cosmosConnectionString = Environment.GetEnvironmentVariable("CosmosConnectionString");
        var databaseName = Environment.GetEnvironmentVariable("CosmosDbDatabaseName") ?? "TaskManagementDB";
        
        if (string.IsNullOrEmpty(cosmosConnectionString))
        {
            throw new InvalidOperationException("CosmosConnectionString environment variable is required");
        }

        // Add persistence service
        services.AddSingleton<IPersistenceService>(provider =>
        {
            var logger = provider.GetRequiredService<ILogger<CosmosPersistenceService>>();
            return new CosmosPersistenceService(cosmosConnectionString, databaseName, logger);
        });

        // Add materialized view service
        services.AddSingleton<IMaterializedViewService, MaterializedViewService>();

        // Configure logging
        services.AddLogging(builder =>
        {
            builder.AddConsole();
            builder.SetMinimumLevel(LogLevel.Information);
        });
    })
    .Build();

// Initialize materialized views on startup
using (var scope = host.Services.CreateScope())
{
    var materializedViewService = scope.ServiceProvider.GetRequiredService<IMaterializedViewService>();
    materializedViewService.InitializeMaterializedViewsAsync().GetAwaiter().GetResult();
}

host.Run();
