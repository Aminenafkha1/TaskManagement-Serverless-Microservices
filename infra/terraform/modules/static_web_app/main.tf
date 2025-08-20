resource "azurerm_static_web_app" "main" {
  name                = "${var.app_name}-${var.environment}-client"
  resource_group_name = var.resource_group_name
  location            = var.location
  sku_tier            = "Free"
  sku_size            = "Free"

  tags = var.tags
}
