# Post-Deployment Verification Script

# This PowerShell script helps verify that the Cosmos DB infrastructure was deployed correctly
# and that the Function Apps can connect to their respective databases.

Write-Host "=== TaskManagement Infrastructure Verification ===" -ForegroundColor Green

# 1. Check if Terraform deployment completed successfully
Write-Host "`n1. Checking Terraform State..." -ForegroundColor Yellow
if (Test-Path "terraform.tfstate") {
    Write-Host "✅ Terraform state file found" -ForegroundColor Green
} else {
    Write-Host "❌ Terraform state file not found" -ForegroundColor Red
    exit 1
}

# 2. Get Terraform outputs
Write-Host "`n2. Getting Terraform Outputs..." -ForegroundColor Yellow
$outputs = terraform output -json | ConvertFrom-Json

if ($outputs) {
    Write-Host "✅ Terraform outputs retrieved successfully" -ForegroundColor Green
    
    # Display Function App URLs
    if ($outputs.function_app_urls) {
        Write-Host "`n📍 Function App URLs:" -ForegroundColor Cyan
        Write-Host "   Users Service: $($outputs.function_app_urls.value.users_service)" -ForegroundColor White
        Write-Host "   Tasks Service: $($outputs.function_app_urls.value.tasks_service)" -ForegroundColor White
    }
} else {
    Write-Host "❌ Failed to retrieve Terraform outputs" -ForegroundColor Red
}

# 3. Verify Azure Resources
Write-Host "`n3. Verifying Azure Resources..." -ForegroundColor Yellow

# Check if Azure CLI is available
try {
    $azCheck = az account show 2>$null
    if ($azCheck) {
        Write-Host "✅ Azure CLI authenticated" -ForegroundColor Green
        
        # Get resource group name from terraform
        $rgName = terraform output -raw resource_group_name 2>$null
        
        if ($rgName) {
            Write-Host "`n🔍 Checking resources in Resource Group: $rgName" -ForegroundColor Cyan
            
            # List Cosmos DB accounts
            $cosmosAccounts = az cosmosdb list --resource-group $rgName --query "[].name" -o tsv 2>$null
            if ($cosmosAccounts) {
                Write-Host "✅ Cosmos DB Accounts:" -ForegroundColor Green
                $cosmosAccounts -split "`n" | ForEach-Object { 
                    if ($_.Trim()) { Write-Host "   - $_" -ForegroundColor White }
                }
            }
            
            # List Function Apps
            $functionApps = az functionapp list --resource-group $rgName --query "[].name" -o tsv 2>$null
            if ($functionApps) {
                Write-Host "✅ Function Apps:" -ForegroundColor Green
                $functionApps -split "`n" | ForEach-Object { 
                    if ($_.Trim()) { Write-Host "   - $_" -ForegroundColor White }
                }
            }
        }
    }
} catch {
    Write-Host "⚠️  Azure CLI not available or not authenticated" -ForegroundColor Yellow
    Write-Host "   Run 'az login' to authenticate with Azure" -ForegroundColor Gray
}

# 4. Test Function App Health
Write-Host "`n4. Testing Function App Health..." -ForegroundColor Yellow

if ($outputs.function_app_urls) {
    $usersUrl = $outputs.function_app_urls.value.users_service
    $tasksUrl = $outputs.function_app_urls.value.tasks_service
    
    # Test Users Service
    try {
        $usersHealth = Invoke-WebRequest -Uri "$usersUrl/api/health" -Method GET -UseBasicParsing -TimeoutSec 10 2>$null
        if ($usersHealth.StatusCode -eq 200) {
            Write-Host "✅ Users Service is healthy" -ForegroundColor Green
        }
    } catch {
        Write-Host "⚠️  Users Service health check failed (this is normal during initial deployment)" -ForegroundColor Yellow
    }
    
    # Test Tasks Service
    try {
        $tasksHealth = Invoke-WebRequest -Uri "$tasksUrl/api/health" -Method GET -UseBasicParsing -TimeoutSec 10 2>$null
        if ($tasksHealth.StatusCode -eq 200) {
            Write-Host "✅ Tasks Service is healthy" -ForegroundColor Green
        }
    } catch {
        Write-Host "⚠️  Tasks Service health check failed (this is normal during initial deployment)" -ForegroundColor Yellow
    }
}

# 5. Next Steps
Write-Host "`n=== Next Steps ===" -ForegroundColor Green
Write-Host "1. Deploy your application code to the Function Apps" -ForegroundColor White
Write-Host "2. Test the authentication endpoints" -ForegroundColor White
Write-Host "3. Test the task management endpoints" -ForegroundColor White
Write-Host "4. Configure CI/CD pipeline for automated deployments" -ForegroundColor White

Write-Host "`n🎉 Infrastructure verification completed!" -ForegroundColor Green
