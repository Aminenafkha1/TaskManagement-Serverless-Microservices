# Cosmos DB Infrastructure Deployment Guide

This guide explains how to deploy separate Cosmos DB databases for the User and Task microservices using Terraform.

## Architecture Overview

- **UsersDB**: Dedicated Cosmos DB for Users Service
  - Container: `users` 
  - Partition Key: `/email`
  - Features: User authentication data, unique email constraint

- **TasksDB**: Dedicated Cosmos DB for Tasks Service
  - Container: `tasks`
  - Partition Key: `/userId`
  - Features: Task management data, efficient user-based queries

## Infrastructure Components

### 1. Cosmos DB Accounts
- **Users Cosmos DB**: Serverless account for user data
- **Tasks Cosmos DB**: Serverless account for task data
- Both use BoundedStaleness consistency for optimal performance

### 2. Azure Function Apps
- **Users Service**: .NET 8 isolated runtime with JWT authentication
- **Tasks Service**: .NET 8 isolated runtime with JWT validation
- Both configured with proper CORS and environment variables

## Deployment Steps

### Prerequisites
- Azure CLI installed and logged in
- Terraform installed (version 1.0+)
- PowerShell (for Windows)

### 1. Deploy Infrastructure
```powershell
cd "c:\Users\amine\Desktop\TaskManagement\infra\terraform"
terraform init
terraform plan
terraform apply
```

### 2. Function App Configuration
The Function Apps are configured with the following key settings:

**Users Service:**
- `CosmosConnectionString`: Connection to UsersDB
- `CosmosDbDatabaseName`: "UsersDB"
- `JwtSecretKey`: JWT signing key
- `JwtIssuer`: "TaskManagement.UsersService"

**Tasks Service:**
- `CosmosConnectionString`: Connection to TasksDB  
- `CosmosDbDatabaseName`: "TasksDB"
- `UsersServiceUrl`: URL of Users Service for inter-service communication

### 3. Database Initialization
Both services automatically create their databases and containers on first run:
- **Users Service**: Creates UsersDB → users container
- **Tasks Service**: Creates TasksDB → tasks container

## Connection Strings
Terraform outputs the connection strings as sensitive values:
- `users_connection_string`: For Users Service
- `tasks_connection_string`: For Tasks Service

## Security Features
- **Managed Identity**: Ready for production deployment
- **CORS Configuration**: Allows cross-origin requests
- **JWT Authentication**: Secure service-to-service communication
- **Serverless Cosmos DB**: Cost-effective scaling

## Development vs Production
- **Development**: Uses local Cosmos DB emulator
- **Production**: Uses deployed Azure Cosmos DB accounts

## Monitoring
- **Application Insights**: Integrated for both services
- **Cosmos DB Metrics**: Available through Azure Monitor
- **Function App Logs**: Accessible via Azure Portal

## Cost Optimization
- **Serverless Cosmos DB**: Pay per operation
- **Consumption Plan**: Pay per execution
- **Shared Throughput**: Database-level throughput sharing

## Next Steps
1. Deploy services using `terraform apply`
2. Test database connectivity
3. Deploy application code to Function Apps
4. Configure CI/CD pipeline for automated deployments
