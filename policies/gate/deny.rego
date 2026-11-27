# ── P14 & P17 : Risk Gate & Agent Guardrails — OPA / Conftest Policy ─────────
# Package: guardian.gate
#
# Innovating beyond raw CVSS: Prioritize by real-world threat:
#   D1 — In CISA KEV (Known Exploited Vulnerabilities) => Immediate block
#   D2 — Reachable in code AND EPSS >= 0.10 (10% daily exploitation chance) => Block
#   D3 — Reachable in code AND CVSS >= 9.0 (Critical fallback) => Block
#   D4 — Container image (Trivy) CRITICAL vulnerability => Block
#   D5 — Slopsquatting / Hallucinated package (T23 / LLM03) outside allowlist => Block
#
# Exemptions:
#   OpenVEX statements with status "not_affected" or "fixed" exempt findings.

package guardian.gate

import rego.v1

# ── Extract VEX statements from data (supports direct or nested .vex) ──────────
vex_statements contains s if {
	some s in data.vex.statements
}

vex_statements contains s if {
	some s in data.statements
}

# ── Vulnerability ID matching helper ──────────────────────────────────────────
vulnerability_matches(vuln, id) if {
	vuln.name == id
}

vulnerability_matches(vuln, id) if {
	vuln.id == id
}

# ── VEX exemption check for a given ID ─────────────────────────────────────────
vex_not_affected(id) if {
	some s in vex_statements
	s.status in {"not_affected", "fixed"}
	vulnerability_matches(s.vulnerability, id)
}

# ── Check if finding or any of its aliases is exempted ─────────────────────────
finding_is_vex_exempt(f) if {
	vex_not_affected(f.id)
}

finding_is_vex_exempt(f) if {
	some alias in f.aliases
	vex_not_affected(alias)
}

# ── Extract Approved Packages & Prefixes (T23 / LLM03 Anti-Slopsquatting) ──────
allowed_packages contains pkg if {
	some pkg in data.package_allowlist.allowed_packages
}

allowed_packages contains pkg if {
	some pkg in data.allowed_packages
}

allowed_prefixes contains prefix if {
	some prefix in data.package_allowlist.allowed_prefixes
}

allowed_prefixes contains prefix if {
	some prefix in data.allowed_prefixes
}

is_allowed_package(pkg_name) if {
	pkg_name in allowed_packages
}

is_allowed_package(pkg_name) if {
	some prefix in allowed_prefixes
	startswith(pkg_name, prefix)
}

# ── D1 : CISA KEV (Known Exploited Vulnerabilities) ───────────────────────────
deny contains msg if {
	some f in input.findings
	f.kev == true
	not finding_is_vex_exempt(f)
	msg := sprintf("[D1-KEV] %s (%s) est dans CISA KEV : exploitation active confirmee — blocage immediat.", [f.id, f.package])
}

# ── D2 : EPSS >= 0.10 AND Reachable ───────────────────────────────────────────
deny contains msg if {
	some f in input.findings
	f.reachable != false
	f.epss >= 0.10
	not finding_is_vex_exempt(f)
	msg := sprintf("[D2-EPSS] %s (%s) atteignable dans le code avec EPSS=%.4f (>= 0.10) : blocage.", [f.id, f.package, f.epss])
}

# ── D3 : Critical CVSS (>= 9.0) AND Reachable (Safety Fallback) ───────────────
deny contains msg if {
	some f in input.findings
	f.reachable != false
	f.cvss >= 9.0
	not finding_is_vex_exempt(f)
	msg := sprintf("[D3-CVSS] %s (%s) atteignable avec CVSS=%.1f (>= 9.0) : blocage de secours.", [f.id, f.package, f.cvss])
}

# ── D4 : Container Image (Trivy JSON) CRITICAL ────────────────────────────────
deny contains msg if {
	some result in input.Results
	some vuln in result.Vulnerabilities
	vuln.Severity == "CRITICAL"
	not vex_not_affected(vuln.VulnerabilityID)
	msg := sprintf("[D4-CONTAINER] Image conteneur : %s dans %s est CRITICAL.", [vuln.VulnerabilityID, vuln.PkgName])
}

# ── D5 : Anti-Slopsquatting (T23 / LLM03) — Lockfile package verification ───────
deny contains msg if {
	count(allowed_prefixes) > 0
	some target_framework, pkgs in input.dependencies
	some pkg_name, pkg_info in pkgs
	pkg_info.type in {"Direct", "Transitive"}
	not is_allowed_package(pkg_name)
	msg := sprintf("[D5-SLOPSQUATTING] Dependance non autorisee detectee (%s) : '%s'. Risque de package hallucine / slopsquatting (T23 / LLM03).", [target_framework, pkg_name])
}

# ── D5b : Anti-Slopsquatting on Findings (e.g. proposed or detected) ───────────
deny contains msg if {
	count(allowed_prefixes) > 0
	some f in input.findings
	f.package != ""
	not is_allowed_package(f.package)
	msg := sprintf("[D5-SLOPSQUATTING] Package '%s' dans les findings n'est pas dans l'allowlist approuvee (T23 / LLM03).", [f.package])
}

# ── W1 : High EPSS but NOT reachable (Monitoring only) ─────────────────────────
warn contains msg if {
	some f in input.findings
	f.reachable == false
	f.epss >= 0.10
	not finding_is_vex_exempt(f)
	msg := sprintf("[W1-EPSS-NON-ATTEIGNABLE] %s (%s) a EPSS=%.4f mais le code vulnerable n'est pas appele : a surveiller.", [f.id, f.package, f.epss])
}

# ── W2 : Moderate EPSS (0.05 <= EPSS < 0.10) ──────────────────────────────────
warn contains msg if {
	some f in input.findings
	f.epss >= 0.05
	f.epss < 0.10
	not finding_is_vex_exempt(f)
	msg := sprintf("[W2-EPSS-MODERE] %s (%s) a EPSS=%.4f : revue planifiee au prochain sprint.", [f.id, f.package, f.epss])
}
