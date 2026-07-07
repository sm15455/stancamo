-- 1. Creiamo l'utente che mancava nel registro di Postgres
CREATE USER app_reader WITH PASSWORD 'PasswordSicura123!';


-- 2. Togliamo i permessi di default per sicurezza
REVOKE ALL ON SCHEMA public FROM app_reader;
REVOKE ALL ON ALL TABLES IN SCHEMA public FROM app_reader;

-- 3. Gli diamo solo il potere di vedere le carte (SELECT)
GRANT USAGE ON SCHEMA public TO app_reader;
GRANT SELECT ON ALL TABLES IN SCHEMA public TO app_reader;
ALTER DEFAULT PRIVILEGES IN SCHEMA public GRANT SELECT ON TABLES TO app_reader;