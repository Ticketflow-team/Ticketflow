# Démonstration P17 / T23 : Injection de Dépendance Hallucinée (Slopsquatting)

> **Branche de preuve :** `demo/vuln-ai-inject`
> **Jalon couvert :** P17 — Garde-fous de l'agent IA & Allowlist OPA anti-slopsquatting
> **Menaces démontrées :** T21 (Prompt injection indirecte), T23 (Slopsquatting / dépendance hostile), T28 (Excessive Agency)
> **Résultat attendu :** Pipeline CI **ROUGE** bloqué par le job `Risk Gate (OPA & Conftest)` sur la règle `[D5-SLOPSQUATTING]`.

---

## 1. Scénario d'Attaque

1. Un attaquant injecte une directive malveillante dans un commentaire ou un composant tiers (Prompt Injection indirecte — T21).
2. L'agent IA de triage ou d'auto-remédiation, sous influence de l'injection, propose l'installation d'une dépendance non approuvée : `AiInjected.Malicious.SlopHelper` (hallucination ou paquet empoisonné enregistré sur NuGet.org — T23).
3. La Pull Request est soumise au pipeline `Guardian DevSecOps`.

## 2. Garde-fou OPA Activé

Le job `Risk Gate (OPA & Conftest)` évalue la politique [`policies/gate/deny.rego`](../../policies/gate/deny.rego) contre [`policies/allowlist/package_allowlist.json`](../../policies/allowlist/package_allowlist.json).

La règle suivante se déclenche :

```rego
deny contains msg if {
	count(allowed_prefixes) > 0
	some target_framework, pkgs in input.dependencies
	some pkg_name, pkg_info in pkgs
	pkg_info.type in {"Direct", "Transitive"}
	not is_allowed_package(pkg_name)
	msg := sprintf("[D5-SLOPSQUATTING] Dependance non autorisee detectee (%s) : '%s'. Risque de package hallucine / slopsquatting (T23 / LLM03).", [target_framework, pkg_name])
}
```

## 3. Preuve d'Échec et de Non-Déploiement

- Le job `risk-gate` échoue avec le message :
  `[D5-SLOPSQUATTING] Dependance non autorisee detectee (net10.0) : 'AiInjected.Malicious.SlopHelper'.`
- Le job `cloud-auth-test` (authentification Azure OIDC) est **immédiatement sauté** (`skipped`), garantissant l'étanchéité du cloud.
- Le job `ai-triage` se déclenche en mode sandboxed (`if: failure()`) avec un token restreint à `pull-requests:write` (aucun droit d'écriture sur `main`).
