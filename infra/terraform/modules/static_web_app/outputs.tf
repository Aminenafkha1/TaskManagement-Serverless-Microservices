output "default_hostname" {
  description = "Default hostname of the static web app"
  value       = azurerm_static_web_app.main.default_host_name
}

output "api_key" {
  description = "API key for the static web app"
  value       = azurerm_static_web_app.main.api_key
  sensitive   = true
}

output "id" {
  description = "ID of the static web app"
  value       = azurerm_static_web_app.main.id
}
