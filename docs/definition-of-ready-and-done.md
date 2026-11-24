# Definition of Ready (DoR) & Definition of Done (DoD)

## Definition of Ready (DoR)

Une tâche est considérée comme **Ready** lorsqu'elle contient les éléments nécessaires pour être prise en charge par l'équipe de développement.

- [ ] Le ticket Jira est clairement identifié (`TF-[XX]`).
- [ ] Le besoin fonctionnel est décrit et compréhensible.
- [ ] Les critères d'acceptation sont définis.
- [ ] Les dépendances connues sont identifiées.
- [ ] Les risques de sécurité pertinents sont identifiés.
- [ ] L'élément STRIDE associé est identifié lorsque applicable.
- [ ] Les données sensibles ou exigences de confidentialité sont identifiées lorsque applicable.
- [ ] Les exigences d'intégrité financière / anti-fraude sont identifiées lorsque applicable.
- [ ] Les informations nécessaires à l'implémentation sont disponibles.

## Definition of Done (DoD)

Une tâche est considérée comme **Done** lorsque les critères suivants sont satisfaits :

### Fonctionnel

- [ ] L'implémentation répond aux critères d'acceptation.
- [ ] Le comportement attendu est vérifié.

### Qualité du code

- [ ] Le code respecte la Clean Architecture et la séparation des responsabilités.
- [ ] `dotnet build` passe avec zéro warning.
- [ ] Le code a été relu par un Code Owner d'un autre domaine lorsque requis.

### Tests

- [ ] Les tests unitaires nécessaires sont présents et passent.
- [ ] Les tests d'intégration nécessaires sont présents et passent.
- [ ] Les tests d'abus / sécurité `T##` sont présents lorsque applicables.
- [ ] Les tests de mutation sont présents lorsque requis.

### Sécurité

- [ ] Aucun secret, mot de passe, clé API ou chaîne de connexion sensible n'est commité.
- [ ] Gitleaks passe avec succès.
- [ ] Les contrôles SAST / SCA passent lorsque applicables.
- [ ] Les exigences du Threat Model / STRIDE sont traitées lorsque applicables.

### Intégrité financière / Anti-fraude

- [ ] Les montants sont calculés côté serveur lorsque applicable.
- [ ] Le client ne peut pas imposer le prix.
- [ ] Une `Idempotency-Key` est utilisée pour les écritures financières sensibles lorsque applicable.

### CI/CD

- [ ] La pipeline `guardian.yml` est verte.
- [ ] Les contrôles de sécurité applicables sont passés.
- [ ] Le Risk Gate OPA / Conftest est validé lorsque applicable.
- [ ] Un VEX signé est fourni lorsqu'un faux positif de sécurité persiste et qu'il est requis.

### Documentation & preuves

- [ ] La documentation nécessaire est mise à jour.
- [ ] Les captures d'écran, logs ou autres preuves nécessaires sont disponibles.
- [ ] Le ticket Jira est correctement tracé dans la PR.
