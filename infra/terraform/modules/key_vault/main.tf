data "azurerm_client_config" "current" {}

resource "random_string" "kv_suffix" {
  length  = 6
  special = false
  upper   = false
}

resource "azurerm_key_vault" "main" {
  name                       = "${substr(var.app_name, 0, 8)}-${var.environment}-${random_string.kv_suffix.result}"
  location                   = var.location
  resource_group_name        = var.resource_group_name
  tenant_id                  = data.azurerm_client_config.current.tenant_id
  sku_name                   = "standard"
  soft_delete_retention_days = 7

  access_policy {
    tenant_id = data.azurerm_client_config.current.tenant_id
    object_id = data.azurerm_client_config.current.object_id

    key_permissions = [
      "Create",
      "Get",
      "List",
      "Delete",
      "Update",
      "Recover",
      "Purge"
    ]

    secret_permissions = [
      "Get",
      "List",
      "Set",
      "Delete",
      "Recover",
      "Purge"
    ]

    certificate_permissions = [
      "Get",
      "List",
      "Create",
      "Delete",
      "Update",
      "Recover",
      "Purge"
    ]
  }

  tags = var.tags
}
