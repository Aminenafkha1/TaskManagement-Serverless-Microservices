provider "azurerm" {
  features {
    key_vault {
      purge_soft_delete_on_destroy    = true
      recover_soft_deleted_key_vaults = true
    }
    resource_group {
      prevent_deletion_if_contains_resources = false
    }
  }
}

# Resource Group
resource "azurerm_resource_group" "main" {
  name     = var.resource_group_name
  location = var.location
  tags     = var.tags
}

# Storage Account Module
module "storage_account" {
  source              = "./modules/storage_account"
  resource_group_name = azurerm_resource_group.main.name
  location            = azurerm_resource_group.main.location
  environment         = var.environment
  app_name            = var.app_name
  tags                = var.tags
}

# Application Insights Module
module "application_insights" {
  source              = "./modules/application_insights"
  resource_group_name = azurerm_resource_group.main.name
  location            = azurerm_resource_group.main.location
  environment         = var.environment
  app_name            = var.app_name
  tags                = var.tags
}

# Key Vault Module
module "key_vault" {
  source              = "./modules/key_vault"
  resource_group_name = azurerm_resource_group.main.name
  location            = azurerm_resource_group.main.location
  environment         = var.environment
  app_name            = var.app_name
  tags                = var.tags
}

# Service Bus Module
module "service_bus" {
  source              = "./modules/service_bus"
  resource_group_name = azurerm_resource_group.main.name
  location            = azurerm_resource_group.main.location
  environment         = var.environment
  app_name            = var.app_name
  tags                = var.tags
}

# Databases Module
module "databases" {
  source              = "./modules/databases"
  resource_group_name = azurerm_resource_group.main.name
  location            = azurerm_resource_group.main.location
  environment         = var.environment
  app_name            = var.app_name
  tags                = var.tags
}

# Function Apps Module
module "function_apps" {
  source                     = "./modules/function_apps"
  resource_group_name        = azurerm_resource_group.main.name
  location                   = azurerm_resource_group.main.location
  environment                = var.environment
  app_name                   = var.app_name
  tags                       = var.tags
  storage_account_name       = module.storage_account.storage_account_name
  storage_account_access_key = module.storage_account.storage_account_access_key
  application_insights_key   = module.application_insights.instrumentation_key
  service_bus_connection     = module.service_bus.connection_string
  
  # Database connection strings
  users_connection_string = module.databases.users_connection_string
  tasks_connection_string = module.databases.tasks_connection_string
}

# Static Web App Module for Blazor Client
# Note: Static Web Apps not available in student subscription allowed regions
# Available regions: westus2,centralus,eastus2,westeurope,eastasia
# Student allowed regions: germanywestcentral,polandcentral,uksouth,italynorth,switzerlandnorth
# module "static_web_app" {
#   source              = "./modules/static_web_app"
#   resource_group_name = azurerm_resource_group.main.name
#   location            = azurerm_resource_group.main.location
#   environment         = var.environment
#   app_name            = var.app_name
#   tags                = var.tags
# }