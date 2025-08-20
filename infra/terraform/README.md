# TaskManagement Azure Infrastructure

This Terraform configuration deploys a complete serverless microservices architecture on Azure for the TaskManagement application.

## Architecture Overview

The infrastructure includes:

- **Resource Group**: Contains all Azure resources
- **Storage Account**: For Azure Functions storage and application data
- **Application Insights**: Monitoring and telemetry
- **Key Vault**: Secure secrets management
- **Service Bus**: Message queuing between services
- **Function Apps**: Serverless microservices (Tasks, Users, Notifications, Reporting)
- **Static Web App**: Hosting for Blazor WebAssembly client

## Prerequisites

1. [Terraform](https://www.terraform.io/downloads.html) >= 1.0
2. [Azure CLI](https://docs.microsoft.com/en-us/cli/azure/install-azure-cli)
3. Azure subscription with appropriate permissions

## Setup

1. **Login to Azure**:
   ```bash
   az login
   ```

2. **Set the active subscription**:
   ```bash
   az account set --subscription "your-subscription-id"
   ```

3. **Copy and customize the variables file**:
   ```bash
   cp terraform.tfvars.example terraform.tfvars
   ```
   Edit `terraform.tfvars` with your specific values.

## Deployment

1. **Initialize Terraform**:
   ```bash
   terraform init
   ```

2. **Plan the deployment**:
   ```bash
   terraform plan
   ```

3. **Apply the configuration**:
   ```bash
   terraform apply
   ```

4. **Get outputs**:
   ```bash
   terraform output
   ```

## Module Structure

```
modules/
├── storage_account/     # Azure Storage Account with containers and queues
├── application_insights/ # Application Insights and Log Analytics
├── key_vault/          # Azure Key Vault for secrets
├── service_bus/        # Service Bus with topics and subscriptions
├── function_apps/      # Azure Function Apps for microservices
└── static_web_app/     # Static Web App for Blazor client
```

## Environment Configuration

The infrastructure supports multiple environments (dev, staging, prod) through variable configuration:

- `environment`: Environment name (dev, staging, prod)
- `app_name`: Application name prefix
- `location`: Azure region
- `tags`: Resource tags for organization

## Outputs

After deployment, you'll get:
- Function App URLs for each microservice
- Static Web App URL for the client
- Storage account details
- Application Insights keys
- Key Vault URI

## Clean Up

To destroy the infrastructure:
```bash
terraform destroy
```

## Security Notes

- All secrets are stored in Azure Key Vault
- Function Apps use managed identities where possible
- HTTPS is enforced on all endpoints
- CORS is configured for client-service communication

## Monitoring

Application Insights provides:
- Performance monitoring
- Error tracking
- Custom metrics
- Dependency tracking across microservices
