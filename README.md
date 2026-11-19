# TicketFlow

Plateforme de billetterie ASP.NET Core securisee par une pipeline DevSecOps
augmentee par IA (risk-based gates : EPSS · CISA KEV · atteignabilite · VEX).

## Architecture
- `src/TicketFlow.Api` — API Minimal (.NET 10)
- `src/TicketFlow.Application` — cas d'usage
- `src/TicketFlow.Domain` — entites metier (sans dependance)
- `src/TicketFlow.Infrastructure` — acces donnees
- `src/TicketFlow.Guardian` — enrichissement risque (EPSS/KEV)

## Equipe
| | Domaine |
|---|---|
| A | Paiement, threat model |
| B | Auth, front, DAST |
| C | Pipeline, cloud, supply chain |
| D | Mobile, IA |

## Demarrer
```bash
dotnet build
dotnet test
```
# test
