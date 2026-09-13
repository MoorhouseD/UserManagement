# Azure infrastructure

This directory defines a parameterised Azure Container Apps deployment for UserManagement. It does not deploy resources automatically. The design keeps the web container stateless and uses Azure Database for PostgreSQL Flexible Server for durable data.

## Architecture

`main.bicep` composes focused modules for:

- Azure Container Registry with admin credentials disabled;
- Log Analytics workspace for Container Apps logs;
- Azure Container Apps environment;
- PostgreSQL Flexible Server and the `usermanagement` database;
- a Container App with managed identity, health probes and conservative scaling.

The Container App pulls its image using a user-assigned managed identity with the `AcrPull` role. The database connection string is stored as a Container Apps secret and injected into the application through `ConnectionStrings__UserManagement`. No password is written to the repository, outputs or image.

## Prerequisites

- Azure CLI with the Bicep extension available: `az bicep version`.
- An approved Azure subscription and resource group.
- Permission to create the listed resources and role assignments.
- An immutable image already available in the configured registry, using a commit SHA tag or digest.

The current repository does not deploy Azure resources. Do not run `az deployment group create` without explicit approval.

## Parameters

Copy `main.parameters.example.json` to a local, untracked parameter file and replace the image reference and password at deployment time. Do not commit the copied file or any secret-bearing parameter file.

Important parameters include:

- `location` and `environmentName` for resource placement and naming;
- `imageReference` for an immutable registry image tag or digest;
- `databaseAdministratorLogin` and secure `databaseAdministratorPassword`;
- `databaseSkuName`, `minReplicas` and `maxReplicas` for cost and scale controls.

Use Azure deployment parameter handling or a secure CI secret source for the password. Never put it in Bicep outputs, shell history, source control or documentation.

## Validate

From the repository root:

```bash
az bicep build --file infra/main.bicep
az bicep lint --file infra/main.bicep
```

Run these commands without a cloud deployment. A resource-group `what-if` is useful only after an explicitly approved subscription and resource group are selected:

```bash
az deployment group what-if \
  --resource-group REPLACE_WITH_APPROVED_RESOURCE_GROUP \
  --template-file infra/main.bicep \
  --parameters @infra/main.parameters.local.json
```

## Deploy

After review and approval, deploy the template to a selected resource group using a secure parameter source:

```bash
az deployment group create \
  --resource-group REPLACE_WITH_APPROVED_RESOURCE_GROUP \
  --template-file infra/main.bicep \
  --parameters @infra/main.parameters.local.json
```

The deployment outputs the application URL, registry name and PostgreSQL server name. It does not contain secrets.

EF migrations remain an explicit release activity. Apply a reviewed migration bundle or controlled migration command before routing production traffic; do not rely on every web replica calling `Database.Migrate()` concurrently in production.

## Verify

After an approved deployment, verify:

```bash
curl --fail https://REPLACE_WITH_APP_HOST/health/live
curl --fail https://REPLACE_WITH_APP_HOST/health/ready
```

Review Container Apps revision health, PostgreSQL connectivity, Log Analytics output and the migration result. Confirm the deployed image is the requested immutable tag or digest.

## Rollback and teardown

Container Apps retains revisions. Roll back by activating the previous known-good revision and routing traffic back to it, then investigate application logs and database compatibility before retrying.

Database migrations must be rolled back only through a reviewed, backwards-compatible migration plan. Do not delete the database as a rollback shortcut.

For an intentionally temporary environment, remove the resource group only after confirming that its data and logs are no longer required:

```bash
az group delete --name REPLACE_WITH_APPROVED_RESOURCE_GROUP --yes --no-wait
```

## Cost and security notes

The example uses burstable PostgreSQL and low Container Apps replica limits for development-like environments. Review production sizing, backup retention, network access and private connectivity before use. The PostgreSQL example enables public network access to keep the template broadly deployable; production environments should restrict access using private networking and firewall rules approved by the platform owner.
