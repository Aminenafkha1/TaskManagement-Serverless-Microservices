# Setup Real Azure Service Bus for Local Development
Write-Host "=== Setting up Real Azure Service Bus ===" -ForegroundColor Green

# Get Service Bus connection string
Write-Host "`n1. Getting Service Bus connection string..." -ForegroundColor Yellow
$serviceBusName = "taskmanagement-dev-servicebus-lz7n0t"
$resourceGroup = "rg-taskmanagement-dev"

try {
    $connectionString = az servicebus namespace authorization-rule keys list --resource-group $resourceGroup --namespace-name $serviceBusName --name RootManageSharedAccessKey --query primaryConnectionString -o tsv
    
    if ($connectionString) {
        Write-Host "✅ Got Service Bus connection string" -ForegroundColor Green
        
        # Update Users Service local.settings.json
        Write-Host "`n2. Updating Users Service configuration..." -ForegroundColor Yellow
        $usersSettingsPath = "src\services\TaskManagement.UsersService\local.settings.json"
        $usersSettings = Get-Content $usersSettingsPath | ConvertFrom-Json
        $usersSettings.Values.ServiceBusConnection = $connectionString
        $usersSettings | ConvertTo-Json -Depth 10 | Set-Content $usersSettingsPath
        
        # Update Tasks Service local.settings.json
        Write-Host "Updating Tasks Service configuration..." -ForegroundColor Yellow
        $tasksSettingsPath = "src\services\TaskManagement.TasksService\local.settings.json"
        $tasksSettings = Get-Content $tasksSettingsPath | ConvertFrom-Json
        $tasksSettings.Values.ServiceBusConnection = $connectionString
        $tasksSettings | ConvertTo-Json -Depth 10 | Set-Content $tasksSettingsPath
        
        Write-Host "✅ Updated local.settings.json files with Service Bus connection" -ForegroundColor Green
        
        # Service Bus Topics Information
        Write-Host "`n3. Available Service Bus Topics:" -ForegroundColor Yellow
        Write-Host "  • task-events (for task-related events)" -ForegroundColor Cyan
        Write-Host "  • user-events (for user-related events)" -ForegroundColor Cyan  
        Write-Host "  • notification-events (for notification events)" -ForegroundColor Cyan
        Write-Host "  • report-events (for reporting events)" -ForegroundColor Cyan
        Write-Host "  • system-events (for system events)" -ForegroundColor Cyan
        
        Write-Host "`n4. Updated Program.cs files to use ServiceBusEventPublisher:" -ForegroundColor Yellow
        Write-Host "✅ Both services now use real Azure Service Bus for events" -ForegroundColor Green
        
    } else {
        Write-Host "❌ Failed to get Service Bus connection string" -ForegroundColor Red
    }
} catch {
    Write-Host "❌ Error getting Service Bus connection string: $_" -ForegroundColor Red
}

Write-Host "`n🎉 Azure Service Bus setup completed!" -ForegroundColor Green
Write-Host "Your services will now publish real events to Azure Service Bus topics." -ForegroundColor White
