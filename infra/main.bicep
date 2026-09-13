targetScope = 'resourceGroup'

@description('Deployment location for all regional resources.')
param location string = resourceGroup().location

@description('Short environment identifier used in resource names.')
@minLength(2)
@maxLength(12)
param environmentName string = 'dev'

@description('Immutable container image reference, including a digest or commit SHA tag.')
param imageReference string

@description('Azure Container Apps and PostgreSQL administrator identity.')
param databaseAdministratorLogin string

@description('PostgreSQL administrator password. Supply this securely at deployment time.')
@secure()
param databaseAdministratorPassword string

@description('Complete PostgreSQL connection string for the application. Supply this securely at deployment time.')
@secure()
param databaseConnectionString string

@description('PostgreSQL Flexible Server SKU.')
param databaseSkuName string = 'B_Standard_B1ms'

@description('Container minimum replica count.')
@minValue(0)
param minReplicas int = 1

@description('Container maximum replica count.')
@minValue(1)
param maxReplicas int = 2

var names = {
  registry: 'acr${uniqueString(resourceGroup().id)}'
  logAnalytics: 'law-${environmentName}-${uniqueString(resourceGroup().id)}'
  containerEnvironment: 'cae-${environmentName}-${uniqueString(resourceGroup().id)}'
  containerApp: 'ca-${environmentName}-${uniqueString(resourceGroup().id)}'
  postgresServer: 'psql-${environmentName}-${uniqueString(resourceGroup().id)}'
  database: 'usermanagement'
}

module registry 'modules/acr.bicep' = {
  name: 'registry'
  params: {
    location: location
    registryName: names.registry
  }
}

module monitoring 'modules/monitoring.bicep' = {
  name: 'monitoring'
  params: {
    location: location
    workspaceName: names.logAnalytics
  }
}

module database 'modules/postgres.bicep' = {
  name: 'database'
  params: {
    location: location
    serverName: names.postgresServer
    databaseName: names.database
    administratorLogin: databaseAdministratorLogin
    administratorPassword: databaseAdministratorPassword
    skuName: databaseSkuName
  }
}

module containerEnvironment 'modules/container-environment.bicep' = {
  name: 'container-environment'
  params: {
    location: location
    environmentName: names.containerEnvironment
    logAnalyticsWorkspaceId: monitoring.outputs.workspaceId
    logAnalyticsSharedKey: monitoring.outputs.workspaceSharedKey
  }
}

module containerApp 'modules/container-app.bicep' = {
  name: 'container-app'
  params: {
    location: location
    appName: names.containerApp
    environmentId: containerEnvironment.outputs.environmentId
    registryLoginServer: registry.outputs.loginServer
    registryName: registry.outputs.registryName
    imageReference: imageReference
    databaseConnectionString: databaseConnectionString
    minReplicas: minReplicas
    maxReplicas: maxReplicas
  }
}

output applicationUrl string = 'https://${containerApp.outputs.fullyQualifiedDomainName}'
output registryName string = registry.outputs.registryName
output postgresServerName string = database.outputs.serverName
