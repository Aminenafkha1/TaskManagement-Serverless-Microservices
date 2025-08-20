#!/bin/bash

# Azure Resource Setup Script for Task Management Microservices
# This script creates the necessary Azure resources for dev/test/prod environments

set -e

# Configuration
RESOURCE_GROUP_PREFIX="rg-taskmanagement"
COSMOS_ACCOUNT_PREFIX="cosmos-taskmanagement"
LOCATION="East US"
SUBSCRIPTION_ID=""  # Set your subscription ID

# Environment (dev, test, prod)
ENVIRONMENT=${1:-dev}

if [ -z "$ENVIRONMENT" ]; then
    echo "Usage: $0 <environment>"
    echo "Example: $0 dev"
    exit 1
fi

echo "Setting up Azure resources for environment: $ENVIRONMENT"

# Resource names
RESOURCE_GROUP="${RESOURCE_GROUP_PREFIX}-${ENVIRONMENT}"
COSMOS_ACCOUNT="${COSMOS_ACCOUNT_PREFIX}-${ENVIRONMENT}"
DATABASE_NAME="TaskManagementDB"

echo "Creating resource group: $RESOURCE_GROUP"
az group create \
    --name $RESOURCE_GROUP \
    --location "$LOCATION"

echo "Creating Cosmos DB account: $COSMOS_ACCOUNT"
az cosmosdb create \
    --name $COSMOS_ACCOUNT \
    --resource-group $RESOURCE_GROUP \
    --kind GlobalDocumentDB \
    --locations regionName="$LOCATION" failoverPriority=0 isZoneRedundant=false \
    --default-consistency-level Session \
    --enable-free-tier false

echo "Creating Cosmos DB database: $DATABASE_NAME"
az cosmosdb sql database create \
    --account-name $COSMOS_ACCOUNT \
    --resource-group $RESOURCE_GROUP \
    --name $DATABASE_NAME \
    --throughput 400

echo "Creating containers..."

# Users container
echo "Creating users container"
az cosmosdb sql container create \
    --account-name $COSMOS_ACCOUNT \
    --resource-group $RESOURCE_GROUP \
    --database-name $DATABASE_NAME \
    --name users \
    --partition-key-path "/id" \
    --idx @users-indexing-policy.json

# Tasks container
echo "Creating tasks container"
az cosmosdb sql container create \
    --account-name $COSMOS_ACCOUNT \
    --resource-group $RESOURCE_GROUP \
    --database-name $DATABASE_NAME \
    --name tasks \
    --partition-key-path "/id" \
    --idx @tasks-indexing-policy.json

echo "Getting connection string..."
CONNECTION_STRING=$(az cosmosdb keys list \
    --name $COSMOS_ACCOUNT \
    --resource-group $RESOURCE_GROUP \
    --type connection-strings \
    --query "connectionStrings[0].connectionString" \
    --output tsv)

echo ""
echo "==================================="
echo "Azure Resources Created Successfully"
echo "==================================="
echo "Environment: $ENVIRONMENT"
echo "Resource Group: $RESOURCE_GROUP"
echo "Cosmos DB Account: $COSMOS_ACCOUNT"
echo "Database: $DATABASE_NAME"
echo ""
echo "Connection String (save this securely):"
echo "$CONNECTION_STRING"
echo ""
echo "Update your environment variables:"
echo "CosmosDbConnectionString=\"$CONNECTION_STRING\""
echo "CosmosDbDatabaseName=\"$DATABASE_NAME\""
