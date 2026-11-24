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
# CKV_AZURE_109 / CKV2_AZURE_195: Private endpoint & public access disabled.
# In a Student subscription, private endpoints are not supported — handled via
# skip-check in .checkov.yaml.  Network ACLs already deny public traffic.
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

  # CKV_AZURE_189 — disable public network access (strict deny via network_acls)
  public_network_access_enabled = false

  network_acls {
    default_action = "Deny"
    bypass         = "AzureServices"
    ip_rules       = []
  }

  tags = {
    Environment = var.environment
    ManagedBy   = "OpenTofu"
    Project     = "TicketFlow"
  }
}

# ── Azure Container Registry (Hardened — Admin Disabled — T14 / T20) ───────────
# CKV_AZURE_164 (zone redundancy) & CKV_AZURE_165 (geo-replication): Premium SKU
# features not supported in Azure for Students — skipped in .checkov.yaml.
resource "azurerm_container_registry" "acr" {
  name                = "acr${var.resource_prefix}${var.environment}"
  resource_group_name = azurerm_resource_group.rg.name
  location            = azurerm_resource_group.rg.location
  sku                 = "Premium"
  admin_enabled       = false # Enforces OIDC / Managed Identity (zero static secrets)

  # CKV_AZURE_237 — disable public network access
  public_network_access_enabled = false

  # CKV_AZURE_138 — enable dedicated data endpoints
  data_endpoint_enabled = true

  # CKV_AZURE_167 — enable quarantine policy
  quarantine_policy_enabled = true

  # CKV_AZURE_166 — zone redundancy (Premium feature — skipped in .checkov.yaml
  # for student subscription; left as comment for production reference)
  # zone_redundancy_enabled = true

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
# CKV_AZURE_172 (zone redundancy) & CKV_AZURE_225 (minimum instances): require
# production subscription and higher SKU — skipped in .checkov.yaml.
resource "azurerm_service_plan" "asp" {
  name                = "asp-${var.resource_prefix}-${var.environment}"
  resource_group_name = azurerm_resource_group.rg.name
  location            = azurerm_resource_group.rg.location
  os_type             = "Linux"
  sku_name            = "P1v3"

  # worker_count = 2  # CKV_AZURE_225 — requires higher quota in Students plan

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

  # CKV_AZURE_222 — disable public network access (traffic routed via private endpoint
  # or Azure Front Door in production; acceptable for student demo via HTTPS only)
  public_network_access_enabled = false

  # CKV_AZURE_213 — configure health check path
  health_check_path = "/health"

  # CKV_AZURE_17 — enable client certificates for mutual TLS
  client_certificate_enabled = true
  client_certificate_mode    = "Required"

  identity {
    type = "SystemAssigned"
  }

  site_config {
    always_on           = true
    ftps_state          = "Disabled"
    http2_enabled       = true
    minimum_tls_version = "1.2"

    application_stack {
      dotnet_version = "10.0"
    }
  }

  # CKV_AZURE_13 / CKV_AZURE_63 / CKV_AZURE_66 — HTTP logging, detailed errors,
  # failed request tracing
  logs {
    http_logs {
      retention_in_days = 7
    }
    detailed_error_messages = true
    failed_request_tracing  = true
  }

  # CKV_AZURE_88 — use Azure Files for content storage
  storage_account {
    name         = "staticcontent"
    type         = "AzureFiles"
    account_name = "stg${var.resource_prefix}${var.environment}"
    share_name   = "appdata"
    access_key   = var.storage_access_key
    mount_path   = "/srv/static"
  }

  # CKV_AZURE_16 — enable App Service Authentication (EasyAuth v2)
  auth_settings_v2 {
    auth_enabled           = true
    require_authentication = true
    unauthenticated_action = "RedirectToLoginPage"

    active_directory_v2 {
      client_id            = var.app_client_id
      tenant_auth_endpoint = "https://login.microsoftonline.com/${data.azurerm_client_config.current.tenant_id}/v2.0"
    }

    login {}
  }

  app_settings = {
    "WEBSITE_RUN_FROM_PACKAGE" = "1"
    "ASPNETCORE_ENVIRONMENT"   = "Production"
    "DOTNET_EnableDiagnostics" = "0"
    "KEYVAULT_URI"             = azurerm_key_vault.kv.vault_uri
  }

  tags = {
    Environment = var.environment
    ManagedBy   = "OpenTofu"
    Project     = "TicketFlow"
  }
}
