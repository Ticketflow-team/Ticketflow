-- init-db.sql — Exécuté automatiquement par l'image postgres:16-alpine au 1er démarrage
-- La base "ticketflow" est déjà créée par la variable POSTGRES_DB.
-- On crée ici une base séparée pour Keycloak afin d'isoler les deux schémas.

CREATE DATABASE keycloak;
GRANT ALL PRIVILEGES ON DATABASE keycloak TO ticketflow;
