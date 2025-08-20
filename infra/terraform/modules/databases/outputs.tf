# Users Cosmos DB outputs
output "users_cosmosdb_endpoint" {
  description = "Users Cosmos DB endpoint"
  value       = azurerm_cosmosdb_account.users_cosmosdb.endpoint
}

output "users_cosmosdb_primary_key" {
  description = "Users Cosmos DB primary key"
  value       = azurerm_cosmosdb_account.users_cosmosdb.primary_key
  sensitive   = true
}

output "users_connection_string" {
  description = "Users Cosmos DB connection string"
  value       = "AccountEndpoint=${azurerm_cosmosdb_account.users_cosmosdb.endpoint};AccountKey=${azurerm_cosmosdb_account.users_cosmosdb.primary_key};"
  sensitive   = true
}

output "users_db_name" {
  description = "Users database name"
  value       = azurerm_cosmosdb_sql_database.users_db.name
}

# Tasks Cosmos DB outputs
output "tasks_cosmosdb_endpoint" {
  description = "Tasks Cosmos DB endpoint"
  value       = azurerm_cosmosdb_account.tasks_cosmosdb.endpoint
}

output "tasks_cosmosdb_primary_key" {
  description = "Tasks Cosmos DB primary key"
  value       = azurerm_cosmosdb_account.tasks_cosmosdb.primary_key
  sensitive   = true
}

output "tasks_connection_string" {
  description = "Tasks Cosmos DB connection string"
  value       = "AccountEndpoint=${azurerm_cosmosdb_account.tasks_cosmosdb.endpoint};AccountKey=${azurerm_cosmosdb_account.tasks_cosmosdb.primary_key};"
  sensitive   = true
}

output "tasks_db_name" {
  description = "Tasks database name"
  value       = azurerm_cosmosdb_sql_database.tasks_db.name
}
