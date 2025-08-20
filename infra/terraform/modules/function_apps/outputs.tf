output "function_app_urls" {
  description = "URLs of the function apps"
  value = {
    tasks_service = "https://${azurerm_linux_function_app.tasks_service.default_hostname}"
    users_service = "https://${azurerm_linux_function_app.users_service.default_hostname}"
  }
}

output "function_app_names" {
  description = "Names of the function apps"
  value = {
    tasks_service = azurerm_linux_function_app.tasks_service.name
    users_service = azurerm_linux_function_app.users_service.name
  }
}

output "service_plan_id" {
  description = "Service plan ID"
  value       = azurerm_service_plan.main.id
}
