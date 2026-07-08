-- Eliminazione delle tabelle se esistono
DROP TABLE IF EXISTS Cards;
DROP TABLE IF EXISTS Users;

-- Creazione tabella Users
CREATE TABLE Users (
    Username VARCHAR(100) PRIMARY KEY,
    Password VARCHAR(100) NOT NULL,
    FirstName VARCHAR(100) NOT NULL,
    LastName VARCHAR(100) NOT NULL
);

-- Inserimento dati Users
INSERT INTO Users
(Username, Password, FirstName, LastName)
VALUES
('GuestUser', 'pwd', 'guestus', 'guests'),
('sadm', 'pwd', 'admin', 'admin'),
('utente', 'pwdguest', 'guest', 'guest');


-- Creazione tabella Cards
CREATE TABLE Cards (
    Username VARCHAR(100) NOT NULL,
    Number VARCHAR(16) NOT NULL,
    Code VARCHAR(3) NOT NULL,
    ExpiryDate DATE NOT NULL,
    Issuer VARCHAR(50) NOT NULL,

    CONSTRAINT PK_Cards PRIMARY KEY (Username, Number),
    CONSTRAINT FK_Cards_Users FOREIGN KEY (Username) REFERENCES Users(Username) ON DELETE CASCADE
);

-- Inserimento dati Cards
INSERT INTO Cards
(Username, Number, Code, ExpiryDate, Issuer)
VALUES
('GuestUser', '1234888888881234', '222', '2026-01-29', 'Mastercard'),
('GuestUser', '1234123412341234', '222', '2026-01-31', 'Mastercard'),
('utente', '9876543210987654', '222', '2026-01-31', 'Visa');
-- 1. Creiamo l'utente che mancava nel registro di Postgres
CREATE USER app_reader WITH PASSWORD 'PasswordSicura123!';


-- 2. Togliamo i permessi di default per sicurezza
REVOKE ALL ON SCHEMA public FROM app_reader;
REVOKE ALL ON ALL TABLES IN SCHEMA public FROM app_reader;

-- 3. Gli diamo solo il potere di leggere dalle tabelle
GRANT USAGE ON SCHEMA public TO app_reader;
GRANT SELECT ON ALL TABLES IN SCHEMA public TO app_reader;
ALTER DEFAULT PRIVILEGES IN SCHEMA public GRANT SELECT ON TABLES TO app_reader;