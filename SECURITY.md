# Politique de securite — TicketFlow

Principe : *l'IA propose, une regle serveur ou un humain decide.*

- Signalement : ouvrir une issue privee ou contacter le Security Champion du sprint.
- Le Risk Gate bloque : CISA KEV, EPSS >= 0,1, atteignable, ou CVSS >= 9 sans VEX signe.
- Aucun secret en clair : Gitleaks au pre-commit + CI.
