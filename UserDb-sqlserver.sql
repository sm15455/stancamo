-- 1. Crea un account di accesso al server (Login)
CREATE LOGIN app_reader WITH PASSWORD = 'PasswordSicura123!';

-- 2. Spostati nel database specifico della tua applicazione (sostituisci NomeDB col tuo)
-- USE IlTuoDatabase; 

-- 3. Crea l'utente interno al database associato al login
CREATE USER app_reader FOR LOGIN app_reader;

-- 4. Concedi SOLO il permesso di lettura (SELECT) sulle tabelle Cards e Users
GRANT SELECT ON Cards TO app_reader;
GRANT SELECT ON Users TO app_reader;