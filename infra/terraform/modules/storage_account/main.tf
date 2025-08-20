resource "random_string" "storage_suffix" {
  length  = 6
  special = false
  upper   = false
}

resource "azurerm_storage_account" "main" {
  name                     = "${lower(replace(var.app_name, "-", ""))}${var.environment}${random_string.storage_suffix.result}"
  resource_group_name      = var.resource_group_name
  location                 = var.location
  account_tier             = "Standard"
  account_replication_type = "LRS"
  account_kind             = "StorageV2"

  https_traffic_only_enabled = true
  min_tls_version          = "TLS1_2"

  blob_properties {
    cors_rule {
      allowed_headers    = ["*"]
      allowed_methods    = ["DELETE", "GET", "HEAD", "MERGE", "POST", "OPTIONS", "PUT"]
      allowed_origins    = ["*"]
      exposed_headers    = ["*"]
      max_age_in_seconds = 3600
    }
  }

  tags = var.tags
}

resource "azurerm_storage_container" "function_apps" {
  name                  = "function-apps"
  storage_account_name  = azurerm_storage_account.main.name
  container_access_type = "private"
}

resource "azurerm_storage_container" "uploads" {
  name                  = "uploads"
  storage_account_name  = azurerm_storage_account.main.name
  container_access_type = "private"
}

resource "azurerm_storage_queue" "task_notifications" {
  name                 = "task-notifications"
  storage_account_name = azurerm_storage_account.main.name
}

resource "azurerm_storage_queue" "reports" {
  name                 = "reports"
  storage_account_name = azurerm_storage_account.main.name
}