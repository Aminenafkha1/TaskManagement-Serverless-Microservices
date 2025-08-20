resource "random_string" "servicebus_suffix" {
  length  = 6
  special = false
  upper   = false
}

resource "azurerm_servicebus_namespace" "main" {
  name                = "${var.app_name}-${var.environment}-servicebus-${random_string.servicebus_suffix.result}"
  location            = var.location
  resource_group_name = var.resource_group_name
  sku                 = "Standard"

  tags = var.tags
}

resource "azurerm_servicebus_topic" "task_events" {
  name         = "task-events"
  namespace_id = azurerm_servicebus_namespace.main.id

  partitioning_enabled               = true
  requires_duplicate_detection       = true
  duplicate_detection_history_time_window = "PT10M"
}

resource "azurerm_servicebus_topic" "user_events" {
  name         = "user-events"
  namespace_id = azurerm_servicebus_namespace.main.id

  partitioning_enabled               = true
  requires_duplicate_detection       = true
  duplicate_detection_history_time_window = "PT10M"
}

resource "azurerm_servicebus_topic" "notification_events" {
  name         = "notification-events"
  namespace_id = azurerm_servicebus_namespace.main.id

  partitioning_enabled               = true
  requires_duplicate_detection       = true
  duplicate_detection_history_time_window = "PT10M"
}

resource "azurerm_servicebus_topic" "report_events" {
  name         = "report-events"
  namespace_id = azurerm_servicebus_namespace.main.id

  partitioning_enabled               = true
  requires_duplicate_detection       = true
  duplicate_detection_history_time_window = "PT10M"
}

resource "azurerm_servicebus_topic" "system_events" {
  name         = "system-events"
  namespace_id = azurerm_servicebus_namespace.main.id

  partitioning_enabled               = true
  requires_duplicate_detection       = true
  duplicate_detection_history_time_window = "PT10M"
}

# Task Event Subscriptions
resource "azurerm_servicebus_subscription" "task_notifications" {
  name     = "task-notifications"
  topic_id = azurerm_servicebus_topic.task_events.id

  max_delivery_count                = 3
  requires_session                  = false
  dead_lettering_on_message_expiration = true
  
  default_message_ttl = "P14D"
}

resource "azurerm_servicebus_subscription" "task_reporting" {
  name     = "task-reporting"
  topic_id = azurerm_servicebus_topic.task_events.id

  max_delivery_count                = 3
  requires_session                  = false
  dead_lettering_on_message_expiration = true
  
  default_message_ttl = "P14D"
}

# User Event Subscriptions
resource "azurerm_servicebus_subscription" "user_notifications" {
  name     = "user-notifications"
  topic_id = azurerm_servicebus_topic.user_events.id

  max_delivery_count                = 3
  requires_session                  = false
  dead_lettering_on_message_expiration = true
  
  default_message_ttl = "P14D"
}

resource "azurerm_servicebus_subscription" "user_reporting" {
  name     = "user-reporting"
  topic_id = azurerm_servicebus_topic.user_events.id

  max_delivery_count                = 3
  requires_session                  = false
  dead_lettering_on_message_expiration = true
  
  default_message_ttl = "P14D"
}

# Notification Event Subscriptions
resource "azurerm_servicebus_subscription" "notification_analytics" {
  name     = "notification-analytics"
  topic_id = azurerm_servicebus_topic.notification_events.id

  max_delivery_count                = 3
  requires_session                  = false
  dead_lettering_on_message_expiration = true
  
  default_message_ttl = "P14D"
}

# Report Event Subscriptions
resource "azurerm_servicebus_subscription" "report_notifications" {
  name     = "report-notifications"
  topic_id = azurerm_servicebus_topic.report_events.id

  max_delivery_count                = 3
  requires_session                  = false
  dead_lettering_on_message_expiration = true
  
  default_message_ttl = "P14D"
}

# System Event Subscriptions
resource "azurerm_servicebus_subscription" "system_monitoring" {
  name     = "system-monitoring"
  topic_id = azurerm_servicebus_topic.system_events.id

  max_delivery_count                = 3
  requires_session                  = false
  dead_lettering_on_message_expiration = true
  
  default_message_ttl = "P14D"
}
