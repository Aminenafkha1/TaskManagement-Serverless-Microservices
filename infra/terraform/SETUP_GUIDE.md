# 🚀 TaskManagement Azure Infrastructure - Complete Setup Guide

## What You Need to Run This Infrastructure

### 📋 Prerequisites

1. **Windows Machine** (You're using Windows PowerShell)
2. **Azure Subscription** with contributor permissions
3. **Software Requirements**:
   - [Azure CLI](https://docs.microsoft.com/en-us/cli/azure/install-azure-cli) (Required)
   - [Terraform](https://www.terraform.io/downloads) (Will auto-install if missing)
   - [Git](https://git-scm.com/download/win) (Optional, for version control)

### 🔧 Quick Software Installation

```powershell
# Install Azure CLI (if not installed)
winget install Microsoft.AzureCLI

# Install Terraform (if not installed)
winget install HashiCorp.Terraform

# Verify installations
az --version
terraform --version
```

## 🎯 Step-by-Step Deployment Process

### Step 1: Prepare Your Environment

1. **Login to Azure**:
   ```powershell
   az login
   ```

2. **Set your subscription** (if you have multiple):
   ```powershell
   az account list --output table
   az account set --subscription "your-subscription-name-or-id"
   ```

3. **Navigate to the terraform directory**:
   ```powershell
   cd "c:\Users\amine\Desktop\TaskManagement\infra\terraform"
   ```

### Step 2: Configure Your Environment

1. **Copy the example configuration**:
   ```powershell
   copy terraform.tfvars.example terraform.tfvars
   ```

2. **Edit the configuration** (modify these values):
   ```hcl
   # Edit terraform.tfvars
   resource_group_name = "rg-taskmanagement-dev"  # Your resource group name
   location           = "East US"                  # Your preferred Azure region
   environment        = "dev"                      # dev, staging, or prod
   app_name          = "taskmanagement"           # Your app name (lowercase)
   
   tags = {
     Environment = "dev"
     Project     = "TaskManagement"
     Owner       = "YourName"
     CostCenter  = "IT"
   }
   ```

### Step 3: Deploy Infrastructure

Choose one of these methods:

#### Option A: Automated PowerShell Script (Recommended)
```powershell
# For development environment
.\deploy.ps1

# For planning only (preview changes)
.\deploy.ps1 -PlanOnly

# For different environment
.\deploy.ps1 -Environment staging

# To destroy infrastructure
.\deploy.ps1 -Destroy
```

#### Option B: Simple Batch File
```cmd
deploy.bat
```

#### Option C: Manual Terraform Commands
```powershell
# Initialize
terraform init

# Validate
terraform validate

# Plan
terraform plan

# Apply
terraform apply
```

## 📊 What Gets Created

### Azure Resources:
- **Resource Group**: Container for all resources
- **Storage Account**: For Azure Functions and app data
- **4 Function Apps**: 
  - Tasks Service
  - Users Service  
  - Notifications Service
  - Reporting Service
- **Static Web App**: For your Blazor client
- **Application Insights**: Monitoring and logging
- **Service Bus**: Message queuing between services
- **Key Vault**: Secure secrets storage

### Estimated Monthly Cost (Development):
- **Function Apps (Consumption)**: ~$0-10/month
- **Storage Account**: ~$1-5/month
- **Application Insights**: ~$2-10/month
- **Service Bus (Standard)**: ~$10/month
- **Static Web App (Free tier)**: $0/month
- **Key Vault**: ~$1/month
- **Total**: ~$15-40/month for development

## 🔍 Verification Steps

After deployment, verify everything works:

1. **Check Azure Portal**:
   - Go to your resource group
   - Verify all resources are created
   - Check Function Apps are running

2. **Test outputs**:
   ```powershell
   terraform output
   ```

3. **Check Function App URLs**:
   ```powershell
   terraform output function_app_urls
   ```

## 🚀 Next Steps After Infrastructure Deployment

### 1. Deploy Your .NET Function Apps

For each service (Tasks, Users, Notifications, Reporting):

```powershell
# Build and publish
cd "c:\Users\amine\Desktop\TaskManagement\src\services\TaskManagement.TasksService"
dotnet publish -c Release

# Deploy to Azure (you'll need Azure Functions Core Tools)
func azure functionapp publish taskmanagement-dev-tasks
```

### 2. Deploy Your Blazor WebAssembly Client

```powershell
cd "c:\Users\amine\Desktop\TaskManagement\src\client\TaskManagement.Client"
dotnet publish -c Release

# The Static Web App can be connected to your GitHub repo for automatic deployment
```

### 3. Set up CI/CD (Optional)

Create GitHub Actions workflows for:
- Automatic infrastructure updates
- Function App deployments
- Blazor client deployments

## 🛠️ Troubleshooting

### Common Issues:

1. **"Subscription not found"**:
   ```powershell
   az account set --subscription "your-subscription-id"
   ```

2. **"Resource names must be unique"**:
   - Modify `app_name` in `terraform.tfvars`
   - Add a unique suffix

3. **"Insufficient permissions"**:
   - Ensure you have Contributor role on the subscription
   - Contact your Azure administrator

4. **"Region not available"**:
   - Change `location` in `terraform.tfvars`
   - Use regions like "East US", "West Europe", etc.

### Getting Help:

```powershell
# Check what will be created
terraform plan

# See current state
terraform show

# List all resources
terraform state list

# Get specific output
terraform output storage_account_name
```

## 🧹 Cleanup

To remove everything:

```powershell
# Using the script
.\deploy.ps1 -Destroy

# Or manually
terraform destroy
```

## 📚 Additional Resources

- [Azure Functions Documentation](https://docs.microsoft.com/en-us/azure/azure-functions/)
- [Blazor WebAssembly Documentation](https://docs.microsoft.com/en-us/aspnet/core/blazor/)
- [Terraform Azure Provider](https://registry.terraform.io/providers/hashicorp/azurerm/latest/docs)
- [Azure Static Web Apps](https://docs.microsoft.com/en-us/azure/static-web-apps/)

---

🎉 **You're ready to deploy your TaskManagement microservices architecture!**
