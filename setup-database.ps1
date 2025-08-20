# TaskManagement Database Setup Script
# This script creates the TaskManagementDB database and all required containers

Write-Host "Setting up TaskManagementDB with standardized containers..." -ForegroundColor Green

# Database and Container Configuration
$DatabaseName = "TaskManagementDB"
$Containers = @(
    @{
        Name = "tasks"
        PartitionKey = "/id"
        Description = "Main tasks container"
    },
    @{
        Name = "users" 
        PartitionKey = "/id"
        Description = "Users container"
    },
    @{
        Name = "vw_tasks_with_users"
        PartitionKey = "/id"
        Description = "Materialized view: Tasks with user information"
    },
    @{
        Name = "vw_user_activity"
        PartitionKey = "/userId"
        Description = "Materialized view: User activity and statistics"
    },
    @{
        Name = "vw_dashboard"
        PartitionKey = "/id"
        Description = "Materialized view: Dashboard metrics and analytics"
    },
    @{
        Name = "leases"
        PartitionKey = "/id"
        Description = "Change feed lease container for tasks"
    },
    @{
        Name = "leases-users"
        PartitionKey = "/id"
        Description = "Change feed lease container for users"
    }
)

Write-Host "`nDatabase: $DatabaseName" -ForegroundColor Yellow
Write-Host "Containers to be created:" -ForegroundColor Yellow

foreach ($container in $Containers) {
    Write-Host "  - $($container.Name) (PartitionKey: $($container.PartitionKey)) - $($container.Description)" -ForegroundColor Cyan
}

Write-Host "`nLocal Development Setup:" -ForegroundColor Yellow
Write-Host "1. Start Cosmos DB Emulator" -ForegroundColor White
Write-Host "2. Connection String: AccountEndpoint=https://localhost:8081/;AccountKey=C2y6yDjf5/R+ob0N8A7Cgv30VRDJIWEHLM+4QDU5DE2nQ9nDuVTqobD4b8mGGyPMbIZnqyMsEcaGQy67XIw/Jw==" -ForegroundColor White
Write-Host "3. All services will use the same database: TaskManagementDB" -ForegroundColor White

Write-Host "`nContainer Usage:" -ForegroundColor Yellow
Write-Host "- tasks: Main task storage (TasksService)" -ForegroundColor White
Write-Host "- users: User accounts and profiles (UsersService)" -ForegroundColor White  
Write-Host "- vw_tasks_with_users: Denormalized tasks with user names (MaterializedViewProcessor)" -ForegroundColor White
Write-Host "- vw_user_activity: User statistics and activity (MaterializedViewProcessor)" -ForegroundColor White
Write-Host "- vw_dashboard: System-wide metrics and analytics (MaterializedViewProcessor)" -ForegroundColor White
Write-Host "- leases/leases-users: Change feed tracking (MaterializedViewProcessor)" -ForegroundColor White

Write-Host "`nMaterialized Views Benefits:" -ForegroundColor Yellow
Write-Host "✓ Automatic updates via Change Feed" -ForegroundColor Green
Write-Host "✓ Optimized read performance" -ForegroundColor Green  
Write-Host "✓ Denormalized data for fast queries" -ForegroundColor Green
Write-Host "✓ Eventual consistency with source data" -ForegroundColor Green

Write-Host "`nNext Steps:" -ForegroundColor Yellow
Write-Host "1. Start Cosmos DB Emulator: cosmosdb-emulator.exe" -ForegroundColor White
Write-Host "2. Build solution: dotnet build" -ForegroundColor White
Write-Host "3. Start services: func start (in each service directory)" -ForegroundColor White
Write-Host "4. MaterializedViewProcessor will auto-create containers on first run" -ForegroundColor White

Write-Host "`nSetup completed successfully!" -ForegroundColor Green
