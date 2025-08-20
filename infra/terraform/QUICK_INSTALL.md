# Quick Setup Instructions

## Step 1: Install Prerequisites

### Install Azure CLI (Required)
1. Go to: https://aka.ms/installazurecliwindows
2. Download and run the installer
3. Restart your command prompt

### Install Terraform (Required)
1. Go to: https://www.terraform.io/downloads
2. Download "Windows AMD64" version
3. Extract the .exe file to C:\terraform\
4. Add C:\terraform to your Windows PATH:
   - Press Win + R, type "sysdm.cpl"
   - Click "Environment Variables"
   - Edit "Path" in System Variables
   - Add "C:\terraform"
   - Click OK and restart command prompt

## Step 2: Verify Installation
Open a new Command Prompt and run:
```
az --version
terraform --version
```

## Step 3: Login to Azure
```
az login
```

## Step 4: Run Deployment
```
cd "c:\Users\amine\Desktop\TaskManagement\infra\terraform"
deploy.bat
```

## Alternative: Use Azure Cloud Shell
If installation is difficult, you can use Azure Cloud Shell:
1. Go to https://shell.azure.com
2. Upload your terraform files
3. Run the terraform commands directly in the cloud

## Manual Terraform Commands (if deploy.bat doesn't work)
```
terraform init
terraform validate
terraform plan
terraform apply
```
