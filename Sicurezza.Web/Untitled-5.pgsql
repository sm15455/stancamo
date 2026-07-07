

-- 2. Inserisce gli utenti nell'ordine corretto (GuestUser per primo)
INSERT INTO "users" ("Username", "Password", "Role") 
VALUES 
('GuestUser', 'password_ospite_123', 'Guest'),          -- Prenderà l'ID 1
('admin', 'SuperSegretaAdmin2026!', 'Administrator'),    -- Prenderà l'ID 2
('andrea', 'password_studente_cs', 'Developer');          -- Prenderà l'ID 3