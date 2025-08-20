@echo off
echo 🚀 TaskManagement Function Apps Deployment
echo.

REM Check if Azure Functions Core Tools is installed
func --version >nul 2>&1
if %errorlevel% neq 0 (
    echo ⚠️  Azure Functions Core Tools not found. Installing...
    echo 📥 Please install via: npm install -g azure-functions-core-tools@4
    echo Or via winget: winget install Microsoft.AzureFunctionsCoreTools
    pause
    exit /b 1
)

echo ✅ Azure Functions Core Tools found
echo.

REM Get Function App names
echo 📋 Getting Function App names from Azure...
for /f %%i in ('az functionapp list --resource-group "rg-taskmanagement-dev" --query "[?contains(name,'tasks')].name" -o tsv') do set TASKS_APP=%%i
for /f %%i in ('az functionapp list --resource-group "rg-taskmanagement-dev" --query "[?contains(name,'users')].name" -o tsv') do set USERS_APP=%%i
for /f %%i in ('az functionapp list --resource-group "rg-taskmanagement-dev" --query "[?contains(name,'notif')].name" -o tsv') do set NOTIF_APP=%%i
for /f %%i in ('az functionapp list --resource-group "rg-taskmanagement-dev" --query "[?contains(name,'report')].name" -o tsv') do set REPORT_APP=%%i

echo 📱 Function Apps found:
echo    Tasks: %TASKS_APP%
echo    Users: %USERS_APP%
echo    Notifications: %NOTIF_APP%
echo    Reporting: %REPORT_APP%
echo.

REM Deploy Tasks Service
echo 🔧 Deploying Tasks Service...
cd "c:\Users\amine\Desktop\TaskManagement\src\services\TaskManagement.TasksService"
dotnet build -c Release
if %errorlevel% equ 0 (
    func azure functionapp publish %TASKS_APP%
    echo ✅ Tasks Service deployed
) else (
    echo ❌ Tasks Service build failed
)
echo.

REM Deploy Users Service
echo 🔧 Deploying Users Service...
cd "c:\Users\amine\Desktop\TaskManagement\src\services\TaskManagement.UsersService"
dotnet build -c Release
if %errorlevel% equ 0 (
    func azure functionapp publish %USERS_APP%
    echo ✅ Users Service deployed
) else (
    echo ❌ Users Service build failed
)
echo.

REM Deploy Notifications Service
echo 🔧 Deploying Notifications Service...
cd "c:\Users\amine\Desktop\TaskManagement\src\services\TaskManagement.NotificationsService"
dotnet build -c Release
if %errorlevel% equ 0 (
    func azure functionapp publish %NOTIF_APP%
    echo ✅ Notifications Service deployed
) else (
    echo ❌ Notifications Service build failed
)
echo.

REM Deploy Reporting Service
echo 🔧 Deploying Reporting Service...
cd "c:\Users\amine\Desktop\TaskManagement\src\services\TaskManagement.ReportingService"
dotnet build -c Release
if %errorlevel% equ 0 (
    func azure functionapp publish %REPORT_APP%
    echo ✅ Reporting Service deployed
) else (
    echo ❌ Reporting Service build failed
)
echo.

echo 🎉 All Function Apps deployment completed!
echo 🔗 Check Azure Portal to verify deployments
pause
