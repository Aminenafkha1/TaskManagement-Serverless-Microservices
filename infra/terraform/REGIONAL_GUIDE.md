# Azure Student Subscription Regional Guide

## Overview
Azure Student subscriptions have regional restrictions that limit where resources can be deployed. This guide helps you choose the right region for your TaskManagement infrastructure.

## Allowed Regions for Student Subscriptions

The following regions are typically allowed for Azure Student subscriptions:

### ✅ Recommended Regions (High Availability)
- **Germany West Central** (`germanywestcentral`) - **RECOMMENDED**
- **UK South** (`uksouth`)
- **Switzerland North** (`switzerlandnorth`)

### ✅ Alternative Regions
- **Poland Central** (`polandcentral`)
- **Italy North** (`italynorth`)

## Current Configuration
The infrastructure is configured to deploy to: **Germany West Central**

## Service Availability by Region

### Germany West Central ✅
- ✅ Cosmos DB (Serverless)
- ✅ Azure Functions (Consumption Plan)
- ✅ Application Insights
- ✅ Key Vault
- ✅ Service Bus
- ✅ Storage Account
- ✅ Static Web Apps

### UK South ✅
- ✅ All services available
- ⚠️ Sometimes has capacity issues

### Switzerland North ✅
- ✅ All services available
- 💰 Slightly higher costs

## How to Change Region

### 1. Update terraform.tfvars
```hcl
location = "germanywestcentral"  # Change this value
```

### 2. Update variables.tf (optional - for default)
```hcl
variable "location" {
  description = "Azure region for resources"
  type        = string
  default     = "germanywestcentral"
}
```

### 3. Redeploy Infrastructure
```bash
terraform destroy -auto-approve
terraform apply -auto-approve
```

## Regional Policy Errors
If you see errors like:
```
RequestDisallowedByPolicy: Resource was disallowed by policy. 
Reasons: 'You can create resources in the following regions 
germanywestcentral, polandcentral, uksouth, italynorth, 
switzerlandnorth only.'
```

This means:
1. Your region is not in the allowed list
2. Update the region in `terraform.tfvars`
3. Clean up and redeploy

## Performance Considerations

### Latency from Different Locations
- **Europe**: Germany West Central is optimal
- **UK**: UK South for lowest latency
- **Rest of Europe**: Switzerland North or Germany West Central

### Cost Optimization
- **Germany West Central**: Most cost-effective for most services
- **Switzerland North**: Premium pricing but high availability
- **UK South**: Standard pricing, good performance

## Troubleshooting Regional Issues

### 1. Check Current Region
```bash
terraform output location
```

### 2. Verify Service Availability
Use Azure CLI to check service availability:
```bash
az provider list --query "[?namespace=='Microsoft.DocumentDB'].resourceTypes[?resourceType=='databaseAccounts'].locations" -o table
```

### 3. Test Deployment
```bash
terraform plan  # Check for regional issues before applying
```

## Best Practices

1. **Always use Germany West Central** for student subscriptions (most reliable)
2. **Test in smaller regions first** if you need to change
3. **Monitor costs** as some regions are more expensive
4. **Keep regions consistent** across all your Azure projects

## Support Resources
- [Azure Student Portal](https://azure.microsoft.com/en-us/free/students/)
- [Azure Regional Availability](https://azure.microsoft.com/en-us/explore/global-infrastructure/products-by-region/)
- [Azure Pricing Calculator](https://azure.microsoft.com/en-us/pricing/calculator/)
