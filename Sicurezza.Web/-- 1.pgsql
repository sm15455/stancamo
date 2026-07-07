-- 1. Elimina le tabelle con la MAIUSCOLA (usando i doppi apici)
DROP TABLE IF EXISTS "Cards" CASCADE;
DROP TABLE IF EXISTS "Users" CASCADE;

-- 2. Elimina le tabelle con la minuscola (se presenti)
DROP TABLE IF EXISTS cards CASCADE;
DROP TABLE IF EXISTS users CASCADE;

-- 3. Ora crea da zero le tabelle esclusivamente in minuscolo
CREATE TABLE users (
    id SERIAL PRIMARY KEY,
    username VARCHAR(100) NOT NULL,
    password VARCHAR(255) NOT NULL,
    role VARCHAR(50) NOT NULL
);

CREATE TABLE cards (
    id SERIAL PRIMARY KEY,
    cardnumber VARCHAR(50) NOT NULL,
    owner VARCHAR(100) NOT NULL,
    userid INTEGER REFERENCES users(id) ON DELETE CASCADE
);

-- 4. Inserisci i dati con GuestUser come primissimo record (id = 1)
INSERT INTO users (username, password, role) 
VALUES 
('GuestUser', 'password_ospite_123', 'Guest'),          -- id 1
('admin', 'SuperSegretaAdmin2026!', 'Administrator'),    -- id 2
('andrea', 'password_studente_cs', 'Developer');          -- id 3

INSERT INTO cards (cardnumber, owner, userid)
VALUES 
('1111-2222-3333-4444', 'Mario Rossi', 1),
('5555-6666-7777-8888', 'Admin Root', 2);