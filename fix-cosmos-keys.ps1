# Fix Cosmos DB Connection Strings
Write-Host "Getting Cosmos DB keys..." -ForegroundColor Yellow

# Get the keys
Write-Host "Fetching Users Cosmos DB key..." -ForegroundColor Cyan
$usersKey = az cosmosdb keys list --name "taskmanagement-dev-users-cosmos-3qf8zs" --resource-group "rg-taskmanagement-dev" --query "primaryMasterKey" -o tsv

Write-Host "Fetching Tasks Cosmos DB key..." -ForegroundColor Cyan
$tasksKey = az cosmosdb keys list --name "taskmanagement-dev-tasks-cosmos-3qf8zs" --resource-group "rg-taskmanagement-dev" --query "primaryMasterKey" -o tsv

if ($usersKey -and $tasksKey) {
    Write-Host "✅ Successfully retrieved both keys" -ForegroundColor Green
    
    # Update Users Service
    Write-Host "Updating Users Service local.settings.json..." -ForegroundColor Cyan
    $usersSettingsPath = "src\services\TaskManagement.UsersService\local.settings.json"
    $usersSettings = Get-Content $usersSettingsPath | ConvertFrom-Json
    $usersSettings.Values.CosmosConnectionString = "AccountEndpoint=https://taskmanagement-dev-users-cosmos-3qf8zs.documents.azure.com:443/;AccountKey=$usersKey;"
    $usersSettings | ConvertTo-Json -Depth 10 | Set-Content $usersSettingsPath
    
    # Update Tasks Service
    Write-Host "Updating Tasks Service local.settings.json..." -ForegroundColor Cyan
    $tasksSettingsPath = "src\services\TaskManagement.TasksService\local.settings.json"
    $tasksSettings = Get-Content $tasksSettingsPath | ConvertFrom-Json
    $tasksSettings.Values.CosmosConnectionString = "AccountEndpoint=https://taskmanagement-dev-tasks-cosmos-3qf8zs.documents.azure.com:443/;AccountKey=$tasksKey;"
    $tasksSettings | ConvertTo-Json -Depth 10 | Set-Content $tasksSettingsPath
    
    Write-Host "✅ Connection strings updated successfully!" -ForegroundColor Green
    Write-Host "`nYou can now run your services:" -ForegroundColor Yellow
    Write-Host "1. cd src\services\TaskManagement.UsersService && func start --port 7071" -ForegroundColor Gray
    Write-Host "2. cd src\services\TaskManagement.TasksService && func start --port 7072" -ForegroundColor Gray
    
} else {
    Write-Host "❌ Failed to retrieve Cosmos DB keys" -ForegroundColor Red
    Write-Host "Please ensure you're logged into Azure CLI and have access to the resource group" -ForegroundColor Yellow
}
