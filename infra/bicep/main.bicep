targetScope = 'resourceGroup'

param location string = 'East US'
param environment string = 'dev'
param appName string = 'taskmanagement'

var resourceTags = {
  Environment: environment
  Project: 'TaskManagement'
  Owner: 'DevTeam'
  CostCenter: 'IT'
}

// Get existing storage account
resource storageAccount 'Microsoft.Storage/storageAccounts@2023-01-01' existing = {
  name: 'taskmanagementdevfsu0dg'
}

// Get existing Application Insights
resource appInsights 'Microsoft.Insights/components@2020-02-02' existing = {
  name: '${appName}-${environment}-insights'
}

// Deploy Function Apps
module functionApps 'functionApps.bicep' = {
  name: 'functionApps'
  params: {
    storageAccountName: storageAccount.name
    functionAppPrefix: '${appName}-${environment}-'
    appServicePlanName: '${appName}-${environment}-plan'
    location: location
    staticWebAppURL: 'https://localhost'
    appInsightsInstrumentationKey: appInsights.properties.InstrumentationKey
    resourceTags: resourceTags
  }
}

// Outputs
output functionAppNames array = functionApps.outputs.functionAppNames
output functionAppUrls array = functionApps.outputs.functionAppUrls
output servicePlanId string = functionApps.outputs.servicePlanId
