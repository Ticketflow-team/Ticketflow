variable "environment" {
  type        = string
  description = "Target deployment environment (dev, staging, prod)"
  default     = "dev"
}

variable "location" {
  type        = string
  description = "Azure region for TicketFlow infrastructure"
  default     = "westeurope"
}

variable "resource_prefix" {
  type        = string
  description = "Prefix for all TicketFlow resource names"
  default     = "ticketflow"
}

# CKV_AZURE_88 — Azure Files access key (injected from Key Vault / GitHub Secret)
variable "storage_access_key" {
  type        = string
  description = "Azure Storage Account access key for Azure Files mount (sensitive)"
  sensitive   = true
  default     = ""
}

# CKV_AZURE_16 — EasyAuth v2 client application ID
variable "app_client_id" {
  type        = string
  description = "Azure AD App Registration client ID for App Service Authentication"
  default     = ""
}
