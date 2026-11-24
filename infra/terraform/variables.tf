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
