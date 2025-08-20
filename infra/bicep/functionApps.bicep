param storageAccountName string
param functionAppPrefix string = 'taskmanagement-dev-'
param functionApps array = [
  'tasks'
  'users'
  'notif'
  'report'
]
param appServicePlanName string
param location string = resourceGroup().location
param staticWebAppURL string = 'https://localhost'
@secure()
param appInsightsInstrumentationKey string
param resourceTags object

var functionRuntime = 'dotnet'
var functionVersion = '~4'

// Use existing storage account
resource storageAccount 'Microsoft.Storage/storageAccounts@2023-01-01' existing = {
  name: storageAccountName
}

// Create App Service Plan with Consumption (Y1) plan
resource plan 'Microsoft.Web/serverFarms@2023-01-01' = {
  name: appServicePlanName
  location: location
  tags: resourceTags
  kind: 'functionapp'
  sku: {
    name: 'Y1'
    tier: 'Dynamic'
  }
  properties: {
    reserved: true // For Linux
  }
}

// Create Function Apps
resource functionApp 'Microsoft.Web/sites@2023-12-01' = [for functionAppName in functionApps: {
  name: '${functionAppPrefix}${functionAppName}'
  location: location
  tags: resourceTags
  kind: 'functionapp,linux'
  properties: {
    serverFarmId: plan.id
    siteConfig: {
      appSettings: [
        {
          name: 'AzureWebJobsStorage'
          value: 'DefaultEndpointsProtocol=https;AccountName=${storageAccount.name};EndpointSuffix=${environment().suffixes.storage};AccountKey=${listKeys(storageAccount.id, storageAccount.apiVersion).keys[0].value}'
        }
        {
          name: 'WEBSITE_CONTENTAZUREFILECONNECTIONSTRING'
          value: 'DefaultEndpointsProtocol=https;AccountName=${storageAccount.name};EndpointSuffix=${environment().suffixes.storage};AccountKey=${listKeys(storageAccount.id, storageAccount.apiVersion).keys[0].value}'
        }
        {
          name: 'APPINSIGHTS_INSTRUMENTATIONKEY'
          value: appInsightsInstrumentationKey
        }
        {
          name: 'APPLICATIONINSIGHTS_CONNECTION_STRING'
          value: 'InstrumentationKey=${appInsightsInstrumentationKey}'
        }
        {
          name: 'FUNCTIONS_WORKER_RUNTIME'
          value: functionRuntime
        }
        {
          name: 'FUNCTIONS_EXTENSION_VERSION'
          value: functionVersion
        }
        {
          name: 'WEBSITE_RUN_FROM_PACKAGE'
          value: '1'
        }
      ]
      cors: {
        allowedOrigins: [
          staticWebAppURL
          '*'
        ]
      }
      minTlsVersion: '1.2'
      linuxFxVersion: 'DOTNET|6.0'
    }
    httpsOnly: true
  }
  identity: {
    type: 'SystemAssigned'
  }
}]

// Outputs
output functionAppNames array = [for i in range(0, length(functionApps)): functionApp[i].name]
output functionAppUrls array = [for i in range(0, length(functionApps)): 'https://${functionApp[i].properties.defaultHostName}']
output servicePlanId string = plan.id
