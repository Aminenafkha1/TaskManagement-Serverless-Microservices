output "namespace_name" {
  description = "Service Bus namespace name"
  value       = azurerm_servicebus_namespace.main.name
}

output "connection_string" {
  description = "Service Bus connection string"
  value       = azurerm_servicebus_namespace.main.default_primary_connection_string
  sensitive   = true
}

output "namespace_id" {
  description = "Service Bus namespace ID"
  value       = azurerm_servicebus_namespace.main.id
}
