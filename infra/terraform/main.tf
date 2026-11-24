data "azurerm_client_config" "current" {}

# ── Resource Group ─────────────────────────────────────────────────────────────
resource "azurerm_resource_group" "rg" {
  name     = "rg-${var.resource_prefix}-${var.environment}"
  location = var.location

  tags = {
    Environment = var.environment
    ManagedBy   = "OpenTofu"
    Project     = "TicketFlow"
  }
}

# ── Azure Key Vault (Hardened — T20 / Zero Secrets) ────────────────────────────
resource "azurerm_key_vault" "kv" {
  name                        = "kv-${var.resource_prefix}-${var.environment}"
  location                    = azurerm_resource_group.rg.location
  resource_group_name         = azurerm_resource_group.rg.name
  enabled_for_disk_encryption = true
  tenant_id                   = data.azurerm_client_config.current.tenant_id
  soft_delete_retention_days  = 90
  purge_protection_enabled    = true
  enable_rbac_authorization   = true
  sku_name                    = "standard"

  network_acls {
    default_action = "Deny"
    bypass         = "AzureServices"
  }

  tags = {
    Environment = var.environment
    ManagedBy   = "OpenTofu"
    Project     = "TicketFlow"
  }
}

# ── Azure Container Registry (Hardened — Admin Disabled — T14 / T20) ───────────
resource "azurerm_container_registry" "acr" {
  name                = "acr${var.resource_prefix}${var.environment}"
  resource_group_name = azurerm_resource_group.rg.name
  location            = azurerm_resource_group.rg.location
  sku                 = "Premium"
  admin_enabled       = false # Enforces OIDC / Managed Identity (zero static secrets)

  retention_policy {
    enabled = true
    days    = 30
  }

  trust_policy {
    enabled = true
  }

  tags = {
    Environment = var.environment
    ManagedBy   = "OpenTofu"
    Project     = "TicketFlow"
  }
}

# ── App Service Plan (Linux) ───────────────────────────────────────────────────
resource "azurerm_service_plan" "asp" {
  name                = "asp-${var.resource_prefix}-${var.environment}"
  resource_group_name = azurerm_resource_group.rg.name
  location            = azurerm_resource_group.rg.location
  os_type             = "Linux"
  sku_name            = "P1v3"

  tags = {
    Environment = var.environment
    ManagedBy   = "OpenTofu"
    Project     = "TicketFlow"
  }
}

# ── Linux Web App (Hardened HTTPS / Managed Identity — A02 / T20) ───────────────
resource "azurerm_linux_web_app" "app" {
  name                = "app-${var.resource_prefix}-${var.environment}"
  resource_group_name = azurerm_resource_group.rg.name
  location            = azurerm_resource_group.rg.location
  service_plan_id     = azurerm_service_plan.asp.id
  https_only          = true

  identity {
    type = "SystemAssigned"
  }

  site_config {
    always_on         = true
    ftps_state        = "Disabled"
    http2_enabled     = true
    minimum_tls_version = "1.2"

    application_stack {
      dotnet_version = "10.0"
    }
  }

  app_settings = {
    "WEBSITE_RUN_FROM_PACKAGE"       = "1"
    "ASPNETCORE_ENVIRONMENT"         = "Production"
    "DOTNET_EnableDiagnostics"       = "0"
    "KEYVAULT_URI"                   = azurerm_key_vault.kv.vault_uri
  }

  tags = {
    Environment = var.environment
    ManagedBy   = "OpenTofu"
    Project     = "TicketFlow"
  }
}
