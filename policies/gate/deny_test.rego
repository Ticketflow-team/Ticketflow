package guardian.gate_test

import rego.v1
import data.guardian.gate

# ── Test 1 : Rapport vide sans vulnérabilité => Aucune interdiction ────────────
test_empty_findings_allowed if {
	count(gate.deny) == 0 with input as {"findings": []}
}

# ── Test 2 : Vulnérabilité présente dans CISA KEV => Interdiction immédiate ───
test_kev_finding_denied if {
	report := {"findings": [{
		"id": "CVE-2023-38606",
		"package": "System.Text.Json",
		"version": "8.0.0",
		"kev": true,
		"reachable": true,
		"epss": 0.02,
		"cvss": 7.5,
	}]}

	count(gate.deny) > 0 with input as report
}

# ── Test 3 : Vulnérabilité avec fort EPSS et atteignable => Interdiction ──────
test_epss_reachable_denied if {
	report := {"findings": [{
		"id": "CVE-2024-12345",
		"package": "Example.Lib",
		"version": "1.0.0",
		"kev": false,
		"reachable": true,
		"epss": 0.45,
		"cvss": 8.0,
	}]}

	count(gate.deny) > 0 with input as report
}

# ── Test 4 : Vulnérabilité avec fort EPSS mais NON atteignable => Warning seul
test_epss_unreachable_warned if {
	report := {"findings": [{
		"id": "CVE-2024-54321",
		"package": "Example.Unused",
		"version": "2.0.0",
		"kev": false,
		"reachable": false,
		"epss": 0.35,
		"cvss": 8.0,
	}]}

	count(gate.deny) == 0 with input as report
	count(gate.warn) > 0 with input as report
}

# ── Test 5 : Vulnérabilité KEV couverte par un VEX 'not_affected' => Autorisée
test_vex_exemption_allows_kev if {
	report := {"findings": [{
		"id": "CVE-2023-38606",
		"package": "System.Text.Json",
		"version": "8.0.0",
		"kev": true,
		"reachable": true,
		"epss": 0.02,
		"cvss": 7.5,
	}]}

	vex_data := {"statements": [{
		"vulnerability": {"id": "CVE-2023-38606", "name": "CVE-2023-38606"},
		"status": "not_affected",
	}]}

	count(gate.deny) == 0 with input as report with data.statements as vex_data.statements
}

# ── Test 6 : Image Trivy avec vulnérabilité CRITICAL => Interdiction ──────────
test_container_trivy_critical_denied if {
	trivy_input := {"Results": [{
		"Vulnerabilities": [{
			"VulnerabilityID": "CVE-2024-9999",
			"PkgName": "libssl3",
			"Severity": "CRITICAL",
		}],
	}]}

	count(gate.deny) > 0 with input as trivy_input
}
