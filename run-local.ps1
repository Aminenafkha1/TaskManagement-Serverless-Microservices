# Local Development Setup Script
# This script helps you run the TaskManagement app locally

Write-Host "=== TaskManagement Local Development Setup ===" -ForegroundColor Green

# Step 1: Get Cosmos DB connection strings
Write-Host "`n1. Getting Cosmos DB connection strings..." -ForegroundColor Yellow

$usersCosmosName = "taskmanagement-dev-users-cosmos-3qf8zs"
$tasksCosmosName = "taskmanagement-dev-tasks-cosmos-3qf8zs"
$rgName = "rg-taskmanagement-dev"

try {
    $usersKey = az cosmosdb keys list --name $usersCosmosName --resource-group $rgName --query "primaryMasterKey" -o tsv
    $tasksKey = az cosmosdb keys list --name $tasksCosmosName --resource-group $rgName --query "primaryMasterKey" -o tsv
    
    if ($usersKey -and $tasksKey) {
        Write-Host "✅ Got Cosmos DB keys successfully" -ForegroundColor Green
        
        # Update Users Service local.settings.json
        $usersSettingsPath = "src\services\TaskManagement.UsersService\local.settings.json"
        $usersSettings = Get-Content $usersSettingsPath | ConvertFrom-Json
        $usersSettings.Values.CosmosConnectionString = "AccountEndpoint=https://$usersCosmosName.documents.azure.com:443/;AccountKey=$usersKey;"
        $usersSettings | ConvertTo-Json -Depth 3 | Set-Content $usersSettingsPath
        
        # Update Tasks Service local.settings.json
        $tasksSettingsPath = "src\services\TaskManagement.TasksService\local.settings.json"
        $tasksSettings = Get-Content $tasksSettingsPath | ConvertFrom-Json
        $tasksSettings.Values.CosmosConnectionString = "AccountEndpoint=https://$tasksCosmosName.documents.azure.com:443/;AccountKey=$tasksKey;"
        $tasksSettings | ConvertTo-Json -Depth 3 | Set-Content $tasksSettingsPath
        
        Write-Host "✅ Updated local.settings.json files" -ForegroundColor Green
    } else {
        Write-Host "❌ Failed to get Cosmos DB keys" -ForegroundColor Red
    }
} catch {
    Write-Host "❌ Error getting Cosmos DB keys: $_" -ForegroundColor Red
}

# Step 2: Instructions for running services
Write-Host "`n2. How to run the services:" -ForegroundColor Yellow
Write-Host "Open 3 separate terminals and run:" -ForegroundColor Cyan

Write-Host "`nTerminal 1 - Users Service:" -ForegroundColor White
Write-Host "cd src\services\TaskManagement.UsersService" -ForegroundColor Gray
Write-Host "func start --port 7071" -ForegroundColor Gray

Write-Host "`nTerminal 2 - Tasks Service:" -ForegroundColor White
Write-Host "cd src\services\TaskManagement.TasksService" -ForegroundColor Gray
Write-Host "func start --port 7072" -ForegroundColor Gray

Write-Host "`nTerminal 3 - Blazor Client:" -ForegroundColor White
Write-Host "cd src\client\TaskManagement.Client" -ForegroundColor Gray
Write-Host "dotnet run" -ForegroundColor Gray

# Step 3: API Testing URLs
Write-Host "`n3. API Testing URLs:" -ForegroundColor Yellow
Write-Host "Users Service:" -ForegroundColor Cyan
Write-Host "  • Register: POST http://localhost:7071/api/register" -ForegroundColor White
Write-Host "  • Login: POST http://localhost:7071/api/login" -ForegroundColor White
Write-Host "  • Get Me: GET http://localhost:7071/api/me" -ForegroundColor White

Write-Host "`nTasks Service:" -ForegroundColor Cyan
Write-Host "  • Create Task: POST http://localhost:7072/api/tasks" -ForegroundColor White
Write-Host "  • Get Tasks: GET http://localhost:7072/api/tasks" -ForegroundColor White

Write-Host "`nBlazor Client:" -ForegroundColor Cyan
Write-Host "  • App URL: https://localhost:5001" -ForegroundColor White

# Step 4: Sample API calls
Write-Host "`n4. Sample API Testing:" -ForegroundColor Yellow
Write-Host "Use these curl commands or Postman:" -ForegroundColor Cyan

Write-Host "`nRegister User:" -ForegroundColor White
@"
curl -X POST http://localhost:7071/api/register \
  -H "Content-Type: application/json" \
  -d '{"email":"test@example.com","password":"Password123!","firstName":"Test","lastName":"User"}'
"@ | Write-Host -ForegroundColor Gray

Write-Host "`nLogin:" -ForegroundColor White
@"
curl -X POST http://localhost:7071/api/login \
  -H "Content-Type: application/json" \
  -d '{"email":"test@example.com","password":"Password123!"}'
"@ | Write-Host -ForegroundColor Gray

Write-Host "`n🎉 Setup completed! Follow the instructions above to run your app." -ForegroundColor Green
