use [Master]
-- 1. Crea un account di accesso al server (Login)
CREATE LOGIN app_reader WITH PASSWORD = 'PasswordSicura123!';

-- 2. Spostati nel database specifico della tua applicazione (sostituisci NomeDB col tuo)
USE [db-sqlserver]; 

-- 3. Crea l'utente interno al database associato al login
CREATE USER app_reader FOR LOGIN app_reader;

-- 4. Concedi SOLO il permesso di lettura (SELECT) sulle tabelle Cards e Users
GRANT SELECT ON Cards TO app_reader;
GRANT SELECT ON Users TO app_reader;

-- Pulizia se per caso c'era già qualcosa
IF OBJECT_ID('dbo.Cards', 'U') IS NOT NULL DROP TABLE dbo.Cards;
IF OBJECT_ID('dbo.Users', 'U') IS NOT NULL DROP TABLE dbo.Users;
GO

-- Creazione della tabella Cards
CREATE TABLE [dbo].[Cards](
    [Number] [varchar](16) NOT NULL,
    [Code] [varchar](3) NOT NULL,
    [ExpiryDate] [date] NOT NULL,
    [Issuer] [varchar](50) NOT NULL,
 CONSTRAINT [PK_Cards] PRIMARY KEY CLUSTERED ([UserId] ASC, [Number] ASC)
) ON [PRIMARY]
GO

-- Creazione della tabella Users
CREATE TABLE [dbo].[Users](
    [Username] [varchar](100) NOT NULL,
    [Password] [varchar](100) NOT NULL,
    [FirstName] [varchar](100) NOT NULL,
    [LastName] [varchar](100) NOT NULL,
 CONSTRAINT [PK_Users] PRIMARY KEY CLUSTERED ([Username] ASC)
) ON [PRIMARY]
GO

-- Inserimento dati reali nelle tue tabelle
INSERT [dbo].[Users] ([Username], [Password], [FirstName], [LastName]) VALUES (N'GuestUser', N'pwd', N'guestus', N'guests')
INSERT [dbo].[Users] ([Username], [Password], [FirstName], [LastName]) VALUES (N'sadm', N'pwd', N'admin', N'admin')
INSERT [dbo].[Users] ([Username], [Password], [FirstName], [LastName]) VALUES (N'utente', N'pwdguest', N'guest', N'guest')

INSERT [dbo].[Cards] ([Username], [Number], [Code], [ExpiryDate], [Issuer]) VALUES (N'GuestUser', N'1234888888881234', N'222', '2026-01-29', N'Mastercard')
INSERT [dbo].[Cards] ([Username], [Number], [Code], [ExpiryDate], [Issuer]) VALUES (N'GuestUser', N'1234123412341234', N'222', '2026-01-31', N'Mastercard')
INSERT [dbo].[Cards] ([Username], [Number], [Code], [ExpiryDate], [Issuer]) VALUES (N'utente', N'9876543210987654', N'222', '2026-01-31', N'Visa')

GO
