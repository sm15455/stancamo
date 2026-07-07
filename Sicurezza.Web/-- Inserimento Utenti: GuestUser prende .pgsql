-- Inserimento Utenti: GuestUser prende l'ID 1
INSERT INTO "users" ("Username", "Password", "Role") 
VALUES 
('GuestUser', 'password_ospite_123', 'Guest'),          -- ID 1
('admin', 'SuperSegretaAdmin2026!', 'Administrator'),    -- ID 2
('andrea', 'password_studente_cs', 'Developer');          -- ID 3

-- Inserimento Carte di test collegate agli utenti (opzionale, se ti servono)
INSERT INTO "cards" ("CardNumber", "Owner", "UserId")
VALUES 
('1111-2222-3333-4444', 'Mario Rossi', 1), -- Collegata a GuestUser
('5555-6666-7777-8888', 'Admin Root', 2);  -- Collegata ad Admin