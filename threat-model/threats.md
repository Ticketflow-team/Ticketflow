# Threat Registry — TicketFlow (T01–T25)

**Official, curated** registry of the project's threats. Source of truth for the
`T##` mapping referenced in the code, the pull requests (template), the DoR/DoD
and `guardian.yml`.

- **Discovery engine**: [`tm.py`](tm.py) (pytm) generates the DFD and candidate
  threats. This table is the **consolidated, stable** version.
- **Method**: STRIDE (Spoofing, Tampering, Repudiation, Information disclosure,
  Denial of service, Elevation of privilege).
- **Status**: `Covered` (control in place) · `Partial` · `To do` (threat to
  design before the matching feature ships — the application code is still a
  skeleton).
- **Risk Gate rule** (`SECURITY.md`): blocks on CISA KEV, EPSS ≥ 0.1,
  reachable, or CVSS ≥ 9 without a signed VEX. *The AI proposes, a server-side
  rule or a human decides.*

> ⚠️ **Do not renumber** T12, T13, T14 and T20: these IDs are already used in
> `guardian.yml`, `Directory.Build.props` and `NuGet.config`.

## Application & business logic

| ID | STRIDE | Component | Threat | Countermeasure | Status | Traceability |
|----|--------|-----------|--------|----------------|--------|--------------|
| T01 | Spoofing | Client → Keycloak | Identity spoofing at login | OIDC + passkeys, MFA, password policy | To do | `keycloak/ticketflow-realm.sanitized.json` |
| T02 | Tampering | Client → API | Client sets/alters the ticket price | Price computed **server-side**, authoritative | To do | `TicketFlow.Application` (business rule) |
| T03 | Tampering | Client → API (payment) | Replay of a payment request (double charge) | Mandatory `Idempotency-Key` on financial writes | To do | `TicketFlow.Api` + Valkey (`f05`) |
| T04 | Information disclosure | API → PostgreSQL | BOLA/IDOR: access to another user's orders/tickets | **Object-level** authorization bound to the token `sub` | To do | `TicketFlow.Api` / `Application` |
| T05 | Elevation of privilege | API | Role escalation (user → organizer/admin) | Server-validated claims, Keycloak RBAC, no client-supplied role | To do | Keycloak realm + `Api` |
| T06 | Denial of service | Client → API | Purchase flood / login brute force | Rate limiting via Valkey, back-off | To do | Valkey (`f05`) |
| T07 | Tampering | API → PostgreSQL | Overselling / double-spend of tickets (inventory race condition) | Atomic transaction + optimistic lock / `SELECT … FOR UPDATE` | To do | `Infrastructure` (data access) |
| T08 | Repudiation | API | Buyer denies a transaction | Append-only, timestamped, correlated audit log | To do | `Api` (logging) |
| T09 | Spoofing | API → Keycloak | Theft / replay of a JWT | JWKS signature validation, `exp`/`aud`, `jti` denylist | Partial | `f03`, Valkey denylist |
| T10 | Information disclosure | API → PostgreSQL | SQL injection | **Parameterized** queries / ORM, no SQL concatenation | To do | `Infrastructure` |
| T11 | Tampering | PSP → API | Forged payment webhook | **Webhook signature** verification, anti-CSRF | To do | `f07` |

## Secrets & supply chain

| ID | STRIDE | Component | Threat | Countermeasure | Status | Traceability |
|----|--------|-----------|--------|----------------|--------|--------------|
| T12 | Information disclosure | Repo / CI | Committed secret (key, password, connection string) | Gitleaks at pre-commit **and** CI over full history, `--redact` | **Covered** | `.pre-commit-config.yaml`, `guardian.yml` (`secrets` job) |
| T13 | Information disclosure (supply) | NuGet dependencies | Vulnerable (known) NuGet package | SCA: `dotnet list package --vulnerable`, blocks Critical/High | **Covered** | `guardian.yml` (`sca` job) |
| T14 | Tampering | Supply chain | Dependency confusion (substituted package) | **Single** NuGet source + `packageSourceMapping` | **Covered** | `NuGet.config` |
| T15 | Tampering | Build | Unpinned dependency / tampered lockfile | `RestorePackagesWithLockFile`, `--locked-mode` restore, NuGet audit | **Covered** | `Directory.Build.props`, `guardian.yml` |
| T16 | Tampering | Dependencies | Typosquatting / malicious package | Dependency review, `NuGetAudit=all` / level `low` | Partial | `Directory.Build.props` |
| T17 | Information disclosure | CI / artifacts | Secret leaked in build logs or artifacts | No plaintext secrets, masking, controlled artifacts | Partial | `guardian.yml` |
| T18 | Spoofing | Keycloak | Misconfigured / compromised IdP | Hardened realm, key rotation, **sanitized** config in repo | To do | `keycloak/ticketflow-realm.sanitized.json` |
| T19 | Tampering | Containers | Vulnerable / unpinned base image | Minimal **pinned** base image, image scanning (Trivy) | To do | `docker-compose.dev.yml` |

## CI/CD, infrastructure & governance

| ID | STRIDE | Component | Threat | Countermeasure | Status | Traceability |
|----|--------|-----------|--------|----------------|--------|--------------|
| T20 | Elevation of privilege | CI / code | Excessive CI permissions; ignored security warning | `permissions: read-all`, **SHA-pinned** actions, Roslyn SAST `warnings=errors` | **Covered** | `guardian.yml`, `Directory.Build.props` |
| T21 | Tampering | GitHub Actions | Third-party action pinned to a mutable tag (compromise) | Pin by **full commit SHA** only | **Covered** | `guardian.yml` |
| T22 | Elevation of privilege | CI workflow | Script injection (untrusted input → `run:`) | No untrusted input interpolated into `run`, dedicated env | To do | `guardian.yml` |
| T23 | Information disclosure | Dev infra | Publicly exposed service (DB / Keycloak / DefectDojo) | Ports bound to `127.0.0.1`, no public exposure | **Covered** | `docker-compose.dev.yml`, `docs/defectdojo-setup.md` |
| T24 | Tampering | Delivery gate | Merging an exploitable vulnerability | **Risk Gate** OPA/Conftest (KEV, EPSS ≥ 0.1, reachable, CVSS ≥ 9 without VEX) | To do | `SECURITY.md` (OPA to be implemented) |
| T25 | Repudiation | Supply chain | No provenance / artifact traceability | SBOM (CycloneDX), SLSA provenance, artifact signing | To do | *(to be implemented)* |

---

## Code / test traceability

Every **testable** threat must be covered by an xUnit abuse test named after its
ID, e.g. `T02_ClientCannotSetThePrice`, `T03_PaymentIsIdempotentOnReplay`,
`T07_NoTicketOverselling`. Pipeline threats (T12–T25) are verified by the
`guardian.yml` jobs and the Risk Gate.

Any PR touching one of these surfaces references the relevant `T##` (required by
the PR template) and updates this table when a status changes.
