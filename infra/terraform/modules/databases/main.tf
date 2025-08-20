# Cosmos DB Account for Users Service
resource "azurerm_cosmosdb_account" "users_cosmosdb" {
  name                = "${var.app_name}-${var.environment}-users-cosmos-${random_string.db_suffix.result}"
  location            = var.location
  resource_group_name = var.resource_group_name
  offer_type          = "Standard"
  kind                = "GlobalDocumentDB"

  consistency_policy {
    consistency_level       = "BoundedStaleness"
    max_interval_in_seconds = 86400
    max_staleness_prefix    = 1000000
  }

  geo_location {
    location          = var.location
    failover_priority = 0
  }

  capabilities {
    name = "EnableServerless"
  }

  tags = var.tags
}

# Cosmos DB Database for Users Service
resource "azurerm_cosmosdb_sql_database" "users_db" {
  name                = "UsersDB"
  resource_group_name = var.resource_group_name
  account_name        = azurerm_cosmosdb_account.users_cosmosdb.name
}

# Cosmos DB Container for Users
resource "azurerm_cosmosdb_sql_container" "users_container" {
  name                  = "users"
  resource_group_name   = var.resource_group_name
  account_name          = azurerm_cosmosdb_account.users_cosmosdb.name
  database_name         = azurerm_cosmosdb_sql_database.users_db.name
  partition_key_paths   = ["/email"]
  partition_key_version = 1

  indexing_policy {
    indexing_mode = "consistent"

    included_path {
      path = "/*"
    }

    excluded_path {
      path = "/\"_etag\"/?"
    }
  }

  unique_key {
    paths = ["/email"]
  }
}

# Cosmos DB Account for Tasks Service
resource "azurerm_cosmosdb_account" "tasks_cosmosdb" {
  name                = "${var.app_name}-${var.environment}-tasks-cosmos-${random_string.db_suffix.result}"
  location            = var.location
  resource_group_name = var.resource_group_name
  offer_type          = "Standard"
  kind                = "GlobalDocumentDB"

  consistency_policy {
    consistency_level       = "BoundedStaleness"
    max_interval_in_seconds = 86400
    max_staleness_prefix    = 1000000
  }

  geo_location {
    location          = var.location
    failover_priority = 0
  }

  capabilities {
    name = "EnableServerless"
  }

  tags = var.tags
}

# Cosmos DB Database for Tasks Service
resource "azurerm_cosmosdb_sql_database" "tasks_db" {
  name                = "TasksDB"
  resource_group_name = var.resource_group_name
  account_name        = azurerm_cosmosdb_account.tasks_cosmosdb.name
}

# Cosmos DB Container for Tasks
resource "azurerm_cosmosdb_sql_container" "tasks_container" {
  name                  = "tasks"
  resource_group_name   = var.resource_group_name
  account_name          = azurerm_cosmosdb_account.tasks_cosmosdb.name
  database_name         = azurerm_cosmosdb_sql_database.tasks_db.name
  partition_key_paths   = ["/userId"]
  partition_key_version = 1

  indexing_policy {
    indexing_mode = "consistent"

    included_path {
      path = "/*"
    }

    excluded_path {
      path = "/\"_etag\"/?"
    }
  }
}

# Random string for database naming
resource "random_string" "db_suffix" {
  length  = 6
  special = false
  upper   = false
}
