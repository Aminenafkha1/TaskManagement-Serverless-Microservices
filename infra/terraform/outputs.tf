output "resource_group_name" {
  description = "Name of the resource group"
  value       = azurerm_resource_group.main.name
}

output "storage_account_name" {
  description = "Name of the storage account"
  value       = module.storage_account.storage_account_name
}

output "storage_account_primary_blob_endpoint" {
  description = "Primary blob endpoint of the storage account"
  value       = module.storage_account.primary_blob_endpoint
}

output "application_insights_instrumentation_key" {
  description = "Application Insights instrumentation key"
  value       = module.application_insights.instrumentation_key
  sensitive   = true
}

output "key_vault_uri" {
  description = "Key Vault URI"
  value       = module.key_vault.vault_uri
}

output "service_bus_namespace" {
  description = "Service Bus namespace"
  value       = module.service_bus.namespace_name
}

output "function_app_urls" {
  description = "URLs of the function apps"
  value       = module.function_apps.function_app_urls
}

output "users_cosmosdb_endpoint" {
  description = "Users Cosmos DB endpoint"
  value       = module.databases.users_cosmosdb_endpoint
}

output "tasks_cosmosdb_endpoint" {
  description = "Tasks Cosmos DB endpoint"
  value       = module.databases.tasks_cosmosdb_endpoint
}

output "users_database_name" {
  description = "Users database name"
  value       = module.databases.users_db_name
}

output "tasks_database_name" {
  description = "Tasks database name"
  value       = module.databases.tasks_db_name
}

# Static Web App not available in student subscription regions
# output "static_web_app_url" {
#   description = "URL of the static web app"
#   value       = module.static_web_app.default_hostname
# }
