using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Configuration;
using TaskManagement.Shared.Services;
using TaskManagement.AuthService.Services;

var host = new HostBuilder()
    .ConfigureFunctionsWorkerDefaults()
    .ConfigureServices((context, services) =>
    {
        var configuration = context.Configuration;
        
        // HTTP Client
        services.AddHttpClient();
        
        // Cosmos DB
        var cosmosConnectionString = configuration.GetConnectionString("CosmosConnectionString") 
            ?? configuration["CosmosConnectionString"];
        services.AddSingleton<IPersistenceService>(sp => 
            new CosmosPersistenceService(cosmosConnectionString, "TaskManagementDB"));
        
        // Event Publisher
        var eventGridConnectionString = configuration.GetConnectionString("EventGridConnectionString") 
            ?? configuration["EventGridConnectionString"];
        services.AddSingleton<IEventPublisher>(sp => 
            new EventGridPublisher(eventGridConnectionString));
        
        // Auth Services
        services.AddSingleton<IJwtService, JwtService>();
        services.AddSingleton<IPasswordService, PasswordService>();
        services.AddSingleton<IAuthService, AuthService.Services.AuthService>();
        
        services.AddApplicationInsightsTelemetryWorkerService();
        services.ConfigureFunctionsApplicationInsights();
    })
    .Build();

host.Run();
