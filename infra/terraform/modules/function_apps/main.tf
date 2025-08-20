resource "azurerm_service_plan" "main" {
  name                = "${var.app_name}-${var.environment}-plan-${random_string.function_suffix.result}"
  resource_group_name = var.resource_group_name
  location            = var.location
  os_type             = "Linux"
  sku_name            = "Y1" # Use Consumption plan

  tags = var.tags
}

resource "random_string" "function_suffix" {
  length  = 6
  special = false
  upper   = false
}

# Tasks Service Function App
resource "azurerm_linux_function_app" "tasks_service" {
  name                = "${var.app_name}-${var.environment}-tasks-${random_string.function_suffix.result}"
  resource_group_name = var.resource_group_name
  location            = var.location

  storage_account_name       = var.storage_account_name
  storage_account_access_key = var.storage_account_access_key
  service_plan_id            = azurerm_service_plan.main.id

  site_config {
    application_stack {
      dotnet_version              = "8.0"
      use_dotnet_isolated_runtime = true
    }

    cors {
      allowed_origins     = ["*"]
      support_credentials = false
    }
  }

  app_settings = {
    "FUNCTIONS_WORKER_RUNTIME"       = "dotnet-isolated"
    "APPINSIGHTS_INSTRUMENTATIONKEY" = var.application_insights_key
    "ServiceBusConnection"           = var.service_bus_connection
    "WEBSITE_RUN_FROM_PACKAGE"       = "1"
    "AzureWebJobsStorage"            = "DefaultEndpointsProtocol=https;AccountName=${var.storage_account_name};AccountKey=${var.storage_account_access_key};EndpointSuffix=core.windows.net"
    "CosmosConnectionString"         = var.tasks_connection_string
    "CosmosDbDatabaseName"           = "TasksDB"
    "JwtSecretKey"                   = "your-super-secret-jwt-key-here-make-it-at-least-32-characters-long"
    "JwtIssuer"                      = "TaskManagement.UsersService"
    "JwtAudience"                    = "TaskManagement.Client"
    "UsersServiceUrl"                = "https://${var.app_name}-${var.environment}-users.azurewebsites.net"
  }

  tags = var.tags
}

# Users Service Function App
resource "azurerm_linux_function_app" "users_service" {
  name                = "${var.app_name}-${var.environment}-users-${random_string.function_suffix.result}"
  resource_group_name = var.resource_group_name
  location            = var.location

  storage_account_name       = var.storage_account_name
  storage_account_access_key = var.storage_account_access_key
  service_plan_id            = azurerm_service_plan.main.id

  site_config {
    application_stack {
      dotnet_version              = "8.0"
      use_dotnet_isolated_runtime = true
    }

    cors {
      allowed_origins     = ["*"]
      support_credentials = false
    }
  }

  app_settings = {
    "FUNCTIONS_WORKER_RUNTIME"       = "dotnet-isolated"
    "APPINSIGHTS_INSTRUMENTATIONKEY" = var.application_insights_key
    "ServiceBusConnection"           = var.service_bus_connection
    "WEBSITE_RUN_FROM_PACKAGE"       = "1"
    "AzureWebJobsStorage"            = "DefaultEndpointsProtocol=https;AccountName=${var.storage_account_name};AccountKey=${var.storage_account_access_key};EndpointSuffix=core.windows.net"
    "CosmosConnectionString"         = var.users_connection_string
    "CosmosDbDatabaseName"           = "UsersDB"
    "JwtSecretKey"                   = "your-super-secret-jwt-key-here-make-it-at-least-32-characters-long"
    "JwtIssuer"                      = "TaskManagement.UsersService"
    "JwtAudience"                    = "TaskManagement.Client"
    "JwtExpirationMinutes"           = "60"
    "EventGridConnectionString"      = ""
  }

  tags = var.tags
}
