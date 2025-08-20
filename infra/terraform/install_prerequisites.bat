@echo off
echo 🚀 Quick Setup for TaskManagement Deployment
echo.

echo Step 1: Installing Azure CLI...
echo Opening download page...
start https://aka.ms/installazurecliwindows
echo.

echo Step 2: Installing Terraform...
echo Opening download page...
start https://www.terraform.io/downloads
echo.

echo 📋 Manual Steps:
echo 1. Install Azure CLI from the opened page
echo 2. Download Terraform Windows AMD64 from the opened page
echo 3. Extract terraform.exe to C:\terraform\
echo 4. Add C:\terraform to your Windows PATH
echo 5. Restart Command Prompt
echo 6. Run: deploy.bat
echo.

pause
