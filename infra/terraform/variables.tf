variable "resource_group_name" {
  description = "Name of the resource group"
  type        = string
  default     = "rg-taskmanagement"
}

variable "location" {
  description = "Azure region for resources"
  type        = string
  default     = "germanywestcentral"
}

variable "environment" {
  description = "Environment name"
  type        = string
  default     = "dev"
}

variable "app_name" {
  description = "Application name"
  type        = string
  default     = "taskmanagement"
}

variable "tags" {
  description = "Resource tags"
  type        = map(string)
  default = {
    Environment = "dev"
    Project     = "TaskManagement"
    Owner       = "DevTeam"
    CostCenter  = "IT"
  }
}
