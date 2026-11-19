#!/usr/bin/env python3
"""
TicketFlow — Threat Model as Code (pytm / OWASP).

Source de verite du modele de menaces. Decrit le systeme (acteurs, processus,
datastores, flux, frontieres de confiance) ; pytm genere le DFD, le diagramme
de sequence et un rapport de menaces candidates.

Le registre officiel cure des menaces est la table T01-T25 dans threats.md :
ce script est le MOTEUR DE DECOUVERTE, threats.md est le REGISTRE.

Usage (voir Makefile) :
    ./tm.py --dfd    | dot -Tpng -o out/dfd.png
    ./tm.py --seq    | java -jar plantuml.jar -tpng -pipe > out/seq.png
    ./tm.py --report report-template.md > out/report.md
"""

from pytm import TM, Server, Datastore, Dataflow, Boundary, Actor

tm = TM("TicketFlow")
tm.description = (
    "Plateforme de billetterie ASP.NET Core (.NET 10) securisee par une "
    "pipeline DevSecOps. Modele de menaces as code — STRIDE, table T01-T25."
)
tm.isOrdered = True          # numerote les flux (ordre = scenario d'achat)
tm.mergeResponses = True     # fusionne requete/reponse pour alleger le DFD

# ─────────────────────────── Frontieres de confiance ────────────────────────
# Une menace nait quand un flux traverse une de ces frontieres.
internet = Boundary("Internet")
dmz      = Boundary("DMZ (exposition publique)")
interne  = Boundary("Reseau interne")
ci       = Boundary("Pipeline CI/CD")

# ─────────────────────────────── Acteurs ────────────────────────────────────
client       = Actor("Client / Acheteur");  client.inBoundary = internet
organisateur = Actor("Organisateur");        organisateur.inBoundary = internet
admin        = Actor("Admin");               admin.inBoundary = internet

# ─────────────────────────────── Processus ──────────────────────────────────
api = Server("API TicketFlow (ASP.NET Core)")
api.inBoundary = dmz
api.sanitizesInput = True
api.validatesInput = True

kc = Server("Keycloak (IdP — OIDC/OAuth2.1/passkeys)")
kc.inBoundary = dmz

guardian = Server("Guardian (enrichissement risque EPSS/KEV)")
guardian.inBoundary = interne

psp = Server("PSP — Prestataire de paiement (tiers)")
psp.inBoundary = internet          # service externe, hors de notre controle

# ─────────────────────────────── Datastores ─────────────────────────────────
pg = Datastore("PostgreSQL (base ticketflow)")
pg.inBoundary = interne
pg.storesSensitiveData = True

kcdb = Datastore("PostgreSQL (base keycloak)")
kcdb.inBoundary = interne
kcdb.storesSensitiveData = True

valkey = Datastore("Valkey (cache / rate-limit / denylist jti / idempotency)")
valkey.inBoundary = interne

# ───────────────────────────────── Flux ─────────────────────────────────────
# Parcours d'achat
f01 = Dataflow(client, kc, "Authentification OIDC / passkey")
f01.protocol = "HTTPS"; f01.dstPort = 443; f01.isEncrypted = True
f01.data = "identifiants, assertion passkey"

f02 = Dataflow(client, api, "Achat de billet")
f02.protocol = "HTTPS"; f02.dstPort = 443; f02.isEncrypted = True
f02.data = "panier, Idempotency-Key, jeton d'acces"

f03 = Dataflow(api, kc, "Validation du jeton (JWKS)")
f03.protocol = "HTTPS"; f03.isEncrypted = True
f03.data = "cle publique de signature, claims"

f04 = Dataflow(api, pg, "Lecture/ecriture commandes & billets")
f04.protocol = "TLS"; f04.data = "commande, montant autoritatif serveur"

f05 = Dataflow(api, valkey, "Cache, rate-limit, idempotency, denylist jti")
f05.protocol = "TCP"; f05.data = "cle d'idempotence, compteur, jti revoque"

f06 = Dataflow(api, psp, "Demande de paiement")
f06.protocol = "HTTPS"; f06.isEncrypted = True
f06.data = "montant (calcule serveur), reference commande"

f07 = Dataflow(psp, api, "Webhook de confirmation de paiement")
f07.protocol = "HTTPS"; f07.isEncrypted = True
f07.data = "statut paiement, signature du webhook"

# Keycloak <-> sa base
f08 = Dataflow(kc, kcdb, "Persistance realm / sessions")
f08.protocol = "TLS"

# Administration
f09 = Dataflow(admin, api, "Operations d'administration")
f09.protocol = "HTTPS"; f09.isEncrypted = True

# Guardian : enrichissement externe (lecture seule)
f10 = Dataflow(guardian, psp, "Enrichissement EPSS / CISA KEV (sources publiques)")
f10.protocol = "HTTPS"; f10.isEncrypted = True
f10.data = "identifiants CVE (donnees publiques)"

if __name__ == "__main__":
    tm.process()
