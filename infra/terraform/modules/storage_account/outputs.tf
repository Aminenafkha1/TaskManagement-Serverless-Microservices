output "storage_account_name" {
  description = "Name of the storage account"
  value       = azurerm_storage_account.main.name
}

output "storage_account_access_key" {
  description = "Primary access key of the storage account"
  value       = azurerm_storage_account.main.primary_access_key
  sensitive   = true
}

output "primary_blob_endpoint" {
  description = "Primary blob endpoint"
  value       = azurerm_storage_account.main.primary_blob_endpoint
}

output "primary_connection_string" {
  description = "Primary connection string"
  value       = azurerm_storage_account.main.primary_connection_string
  sensitive   = true
}
