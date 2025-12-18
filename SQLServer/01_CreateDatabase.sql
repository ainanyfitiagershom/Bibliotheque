-- ============================================================
-- SCRIPT SQL - SYSTÈME DE GESTION DE BIBLIOTHÈQUE
-- Base de données : BibliothequeDB
-- Version : 1.0
-- ============================================================

-- Création de la base de données
IF NOT EXISTS (SELECT name FROM sys.databases WHERE name = 'BibliothequeDB')
BEGIN
    CREATE DATABASE BibliothequeDB;
END
GO

USE BibliothequeDB;
GO

-- ============================================================
-- SUPPRESSION DES TABLES EXISTANTES (ordre inverse des FK)
-- ============================================================
IF OBJECT_ID('dbo.Notifications', 'U') IS NOT NULL DROP TABLE dbo.Notifications;
IF OBJECT_ID('dbo.Historiques', 'U') IS NOT NULL DROP TABLE dbo.Historiques;
IF OBJECT_ID('dbo.Avis', 'U') IS NOT NULL DROP TABLE dbo.Avis;
IF OBJECT_ID('dbo.Reservations', 'U') IS NOT NULL DROP TABLE dbo.Reservations;
IF OBJECT_ID('dbo.Emprunts', 'U') IS NOT NULL DROP TABLE dbo.Emprunts;
IF OBJECT_ID('dbo.LivreCategories', 'U') IS NOT NULL DROP TABLE dbo.LivreCategories;
IF OBJECT_ID('dbo.Livres', 'U') IS NOT NULL DROP TABLE dbo.Livres;
IF OBJECT_ID('dbo.Auteurs', 'U') IS NOT NULL DROP TABLE dbo.Auteurs;
IF OBJECT_ID('dbo.Categories', 'U') IS NOT NULL DROP TABLE dbo.Categories;
IF OBJECT_ID('dbo.Utilisateurs', 'U') IS NOT NULL DROP TABLE dbo.Utilisateurs;
IF OBJECT_ID('dbo.Admins', 'U') IS NOT NULL DROP TABLE dbo.Admins;
GO

-- ============================================================
-- TABLE : Admins (Administrateurs du backoffice)
-- ============================================================
CREATE TABLE dbo.Admins (
    IdAdmin INT IDENTITY(1,1) PRIMARY KEY,
    Nom NVARCHAR(100) NOT NULL,
    Prenom NVARCHAR(100) NOT NULL,
    Email NVARCHAR(255) NOT NULL UNIQUE,
    MotDePasseHash NVARCHAR(255) NOT NULL,
    DateCreation DATETIME2 DEFAULT GETDATE(),
    DerniereConnexion DATETIME2 NULL,
    Actif BIT DEFAULT 1
);
GO

-- ============================================================
-- TABLE : Utilisateurs (Lecteurs de la bibliothèque)
-- ============================================================
CREATE TABLE dbo.Utilisateurs (
    IdUtilisateur INT IDENTITY(1,1) PRIMARY KEY,
    Nom NVARCHAR(100) NOT NULL,
    Prenom NVARCHAR(100) NOT NULL,
    Email NVARCHAR(255) NOT NULL UNIQUE,
    Telephone NVARCHAR(20) NULL,
    Adresse NVARCHAR(500) NULL,
    DateNaissance DATE NULL,
    MotDePasseHash NVARCHAR(255) NOT NULL,
    DateInscription DATETIME2 DEFAULT GETDATE(),
    DerniereConnexion DATETIME2 NULL,
    NombreEmpruntsMax INT DEFAULT 3,
    Actif BIT DEFAULT 1,
    EstBloque BIT DEFAULT 0,
    RaisonBlocage NVARCHAR(500) NULL
);
GO

-- ============================================================
-- TABLE : Categories
-- ============================================================
CREATE TABLE dbo.Categories (
    IdCategorie INT IDENTITY(1,1) PRIMARY KEY,
    Nom NVARCHAR(100) NOT NULL UNIQUE,
    Description NVARCHAR(500) NULL,
    Couleur NVARCHAR(7) DEFAULT '#007bff', -- Code couleur hex pour UI
    Icone NVARCHAR(50) DEFAULT 'bi-book',  -- Icône Bootstrap
    DateCreation DATETIME2 DEFAULT GETDATE(),
    Actif BIT DEFAULT 1
);
GO

-- ============================================================
-- TABLE : Auteurs
-- ============================================================
CREATE TABLE dbo.Auteurs (
    IdAuteur INT IDENTITY(1,1) PRIMARY KEY,
    Nom NVARCHAR(100) NOT NULL,
    Prenom NVARCHAR(100) NULL,
    Nationalite NVARCHAR(100) NULL,
    DateNaissance DATE NULL,
    DateDeces DATE NULL,
    Biographie NVARCHAR(MAX) NULL,
    PhotoUrl NVARCHAR(500) NULL,
    DateCreation DATETIME2 DEFAULT GETDATE(),
    Actif BIT DEFAULT 1
);
GO

-- ============================================================
-- TABLE : Livres
-- ============================================================
CREATE TABLE dbo.Livres (
    IdLivre INT IDENTITY(1,1) PRIMARY KEY,
    ISBN NVARCHAR(20) NULL UNIQUE,
    Titre NVARCHAR(255) NOT NULL,
    IdAuteur INT NOT NULL,
    Annee INT NULL,
    Editeur NVARCHAR(200) NULL,
    NombrePages INT NULL,
    Langue NVARCHAR(50) DEFAULT 'Français',
    Description NVARCHAR(MAX) NULL,
    ImageCouverture NVARCHAR(500) NULL,
    Stock INT DEFAULT 1,
    StockDisponible INT DEFAULT 1,
    Emplacement NVARCHAR(50) NULL, -- Ex: "Rayon A-15"
    DateAjout DATETIME2 DEFAULT GETDATE(),
    DateModification DATETIME2 NULL,
    NombreEmprunts INT DEFAULT 0, -- Pour statistiques popularité
    NoteMoyenne DECIMAL(3,2) DEFAULT 0, -- Note moyenne des avis
    Actif BIT DEFAULT 1,

    CONSTRAINT FK_Livres_Auteurs FOREIGN KEY (IdAuteur) REFERENCES dbo.Auteurs(IdAuteur),
    CONSTRAINT CK_Livres_Stock CHECK (Stock >= 0),
    CONSTRAINT CK_Livres_StockDisponible CHECK (StockDisponible >= 0 AND StockDisponible <= Stock)
);
GO

-- ============================================================
-- TABLE : LivreCategories (Relation N:N entre Livres et Categories)
-- ============================================================
CREATE TABLE dbo.LivreCategories (
    IdLivre INT NOT NULL,
    IdCategorie INT NOT NULL,

    PRIMARY KEY (IdLivre, IdCategorie),
    CONSTRAINT FK_LivreCategories_Livres FOREIGN KEY (IdLivre) REFERENCES dbo.Livres(IdLivre) ON DELETE CASCADE,
    CONSTRAINT FK_LivreCategories_Categories FOREIGN KEY (IdCategorie) REFERENCES dbo.Categories(IdCategorie) ON DELETE CASCADE
);
GO

-- ============================================================
-- TABLE : Emprunts
-- ============================================================
CREATE TABLE dbo.Emprunts (
    IdEmprunt INT IDENTITY(1,1) PRIMARY KEY,
    IdLivre INT NOT NULL,
    IdUtilisateur INT NOT NULL,
    DateEmprunt DATETIME2 DEFAULT GETDATE(),
    DateRetourPrevue DATETIME2 NOT NULL,
    DateRetourEffective DATETIME2 NULL,
    Statut NVARCHAR(20) DEFAULT 'EnCours', -- EnCours, Termine, EnRetard
    NombreProlongations INT DEFAULT 0,
    MaxProlongations INT DEFAULT 2,
    Penalite DECIMAL(10,2) DEFAULT 0,
    Notes NVARCHAR(500) NULL,

    CONSTRAINT FK_Emprunts_Livres FOREIGN KEY (IdLivre) REFERENCES dbo.Livres(IdLivre),
    CONSTRAINT FK_Emprunts_Utilisateurs FOREIGN KEY (IdUtilisateur) REFERENCES dbo.Utilisateurs(IdUtilisateur),
    CONSTRAINT CK_Emprunts_Statut CHECK (Statut IN ('EnCours', 'Termine', 'EnRetard'))
);
GO

-- ============================================================
-- TABLE : Reservations
-- ============================================================
CREATE TABLE dbo.Reservations (
    IdReservation INT IDENTITY(1,1) PRIMARY KEY,
    IdLivre INT NOT NULL,
    IdUtilisateur INT NOT NULL,
    DateReservation DATETIME2 DEFAULT GETDATE(),
    DateExpiration DATETIME2 NOT NULL, -- Expiration si non récupéré
    PositionFile INT DEFAULT 1, -- Position dans la file d'attente
    Statut NVARCHAR(20) DEFAULT 'EnAttente', -- EnAttente, Disponible, Annulee, Convertie
    DateNotification DATETIME2 NULL, -- Quand l'utilisateur a été notifié

    CONSTRAINT FK_Reservations_Livres FOREIGN KEY (IdLivre) REFERENCES dbo.Livres(IdLivre),
    CONSTRAINT FK_Reservations_Utilisateurs FOREIGN KEY (IdUtilisateur) REFERENCES dbo.Utilisateurs(IdUtilisateur),
    CONSTRAINT CK_Reservations_Statut CHECK (Statut IN ('EnAttente', 'Disponible', 'Annulee', 'Convertie'))
);
GO

-- ============================================================
-- TABLE : Avis (Notes et commentaires sur les livres)
-- ============================================================
CREATE TABLE dbo.Avis (
    IdAvis INT IDENTITY(1,1) PRIMARY KEY,
    IdLivre INT NOT NULL,
    IdUtilisateur INT NOT NULL,
    Note INT NOT NULL, -- 1 à 5 étoiles
    Commentaire NVARCHAR(MAX) NULL,
    DateAvis DATETIME2 DEFAULT GETDATE(),
    Approuve BIT DEFAULT 0, -- Modération admin

    CONSTRAINT FK_Avis_Livres FOREIGN KEY (IdLivre) REFERENCES dbo.Livres(IdLivre),
    CONSTRAINT FK_Avis_Utilisateurs FOREIGN KEY (IdUtilisateur) REFERENCES dbo.Utilisateurs(IdUtilisateur),
    CONSTRAINT CK_Avis_Note CHECK (Note >= 1 AND Note <= 5),
    CONSTRAINT UQ_Avis_LivreUtilisateur UNIQUE (IdLivre, IdUtilisateur) -- Un avis par utilisateur par livre
);
GO

-- ============================================================
-- TABLE : Notifications
-- ============================================================
CREATE TABLE dbo.Notifications (
    IdNotification INT IDENTITY(1,1) PRIMARY KEY,
    IdUtilisateur INT NOT NULL,
    Type NVARCHAR(50) NOT NULL, -- Retard, Disponibilite, Rappel, Systeme
    Titre NVARCHAR(200) NOT NULL,
    Message NVARCHAR(MAX) NOT NULL,
    DateCreation DATETIME2 DEFAULT GETDATE(),
    DateLecture DATETIME2 NULL,
    EstLue BIT DEFAULT 0,
    Lien NVARCHAR(500) NULL, -- Lien vers la ressource concernée

    CONSTRAINT FK_Notifications_Utilisateurs FOREIGN KEY (IdUtilisateur) REFERENCES dbo.Utilisateurs(IdUtilisateur),
    CONSTRAINT CK_Notifications_Type CHECK (Type IN ('Retard', 'Disponibilite', 'Rappel', 'Systeme', 'Bienvenue'))
);
GO

-- ============================================================
-- TABLE : Historiques (Audit trail)
-- ============================================================
CREATE TABLE dbo.Historiques (
    IdHistorique INT IDENTITY(1,1) PRIMARY KEY,
    TableNom NVARCHAR(50) NOT NULL,
    IdEnregistrement INT NOT NULL,
    Action NVARCHAR(20) NOT NULL, -- Insert, Update, Delete
    AncienneValeur NVARCHAR(MAX) NULL,
    NouvelleValeur NVARCHAR(MAX) NULL,
    IdAdmin INT NULL,
    DateAction DATETIME2 DEFAULT GETDATE(),
    AdresseIP NVARCHAR(50) NULL,

    CONSTRAINT FK_Historiques_Admins FOREIGN KEY (IdAdmin) REFERENCES dbo.Admins(IdAdmin)
);
GO

-- ============================================================
-- INDEX POUR PERFORMANCES
-- ============================================================
CREATE INDEX IX_Livres_Titre ON dbo.Livres(Titre);
CREATE INDEX IX_Livres_IdAuteur ON dbo.Livres(IdAuteur);
CREATE INDEX IX_Livres_Annee ON dbo.Livres(Annee);
CREATE INDEX IX_Livres_StockDisponible ON dbo.Livres(StockDisponible);
CREATE INDEX IX_Emprunts_IdUtilisateur ON dbo.Emprunts(IdUtilisateur);
CREATE INDEX IX_Emprunts_IdLivre ON dbo.Emprunts(IdLivre);
CREATE INDEX IX_Emprunts_Statut ON dbo.Emprunts(Statut);
CREATE INDEX IX_Emprunts_DateRetourPrevue ON dbo.Emprunts(DateRetourPrevue);
CREATE INDEX IX_Reservations_IdLivre ON dbo.Reservations(IdLivre);
CREATE INDEX IX_Reservations_IdUtilisateur ON dbo.Reservations(IdUtilisateur);
CREATE INDEX IX_Reservations_Statut ON dbo.Reservations(Statut);
CREATE INDEX IX_Notifications_IdUtilisateur ON dbo.Notifications(IdUtilisateur);
CREATE INDEX IX_Notifications_EstLue ON dbo.Notifications(EstLue);
CREATE INDEX IX_Auteurs_Nom ON dbo.Auteurs(Nom);
CREATE INDEX IX_Utilisateurs_Email ON dbo.Utilisateurs(Email);
GO

PRINT 'Base de données BibliothequeDB créée avec succès!';
GO
