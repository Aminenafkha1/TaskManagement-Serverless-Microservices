variable "resource_group_name" {
  description = "Name of the resource group"
  type        = string
}

variable "location" {
  description = "Azure region"
  type        = string
}

variable "environment" {
  description = "Environment name"
  type        = string
}

variable "app_name" {
  description = "Application name"
  type        = string
}

variable "tags" {
  description = "Resource tags"
  type        = map(string)
  default     = {}
}

variable "storage_account_name" {
  description = "Storage account name"
  type        = string
}

variable "storage_account_access_key" {
  description = "Storage account access key"
  type        = string
  sensitive   = true
}

variable "application_insights_key" {
  description = "Application Insights instrumentation key"
  type        = string
  sensitive   = true
}

variable "service_bus_connection" {
  description = "Service Bus connection string"
  type        = string
  sensitive   = true
}

variable "users_connection_string" {
  description = "Users database connection string"
  type        = string
  sensitive   = true
}

variable "tasks_connection_string" {
  description = "Tasks database connection string"
  type        = string
  sensitive   = true
}
