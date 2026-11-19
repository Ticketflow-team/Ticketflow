## Quoi

<!-- Décrivez clairement ce que cette PR change et pourquoi. -->

## Story / Ticket

`TF-__`

## Traçabilité & Gouvernance

- **Ticket Jira associé :** `TF-[XX]`
- **Élément du Threat Model (STRIDE) :** `T[XX]` *(le cas échéant)*

## Definition of Done

- [ ] `dotnet build` vert (warnings = 0)
- [ ] Tests unitaires + tests d'abus `T##` du domaine passent
- [ ] Aucun secret commité (Gitleaks vert)
- [ ] Revue par un Code Owner d'un autre domaine
- [ ] Pipeline `guardian.yml` verte

## Checklist Qualité, Sécurité & DevSecOps

### Architecture

- [ ] Le code respecte strictement la séparation en couches de la Clean Architecture :
  - `TicketFlow.Api`
  - `TicketFlow.Application`
  - `TicketFlow.Domain`
  - `TicketFlow.Infrastructure`
  - `TicketFlow.Guardian`

### Gestion des Secrets

- [ ] Aucun secret, clé API, mot de passe ou chaîne de connexion n'est hardcodé en clair.
- [ ] Gitleaks est passé avec succès.

### Intégrité Financière / Anti-Fraude — si applicable

- [ ] Le montant total est calculé de manière autoritaire côté serveur.
- [ ] Le client ne dicte jamais le prix.
- [ ] Une `Idempotency-Key` est exigée sur les écritures financières sensibles.

### Tests

- [ ] Les tests unitaires et d'intégration (`xUnit`, `Testcontainers`, `WireMock.Net`) ont été ajoutés ou mis à jour lorsque nécessaire.
- [ ] Le test de mutation ou d'abus associé (`T##`) est valide lorsque applicable.

### Pipeline CI/CD

- [ ] Tous les jobs requis de `guardian.yml` sont passés au vert.
- [ ] SAST / SCA / image container / IaC passent lorsque applicables.
- [ ] Le Risk Gate (OPA / Conftest) est validé lorsque applicable.
- [ ] Si un faux positif persiste, un fichier VEX signé est fourni.

## Captures d'écran / Preuves

<!-- Ajoutez ici les captures d'écran, résultats de tests ou logs de pipeline nécessaires pour démontrer la modification. -->