-- ============================================================
-- PROCÉDURES STOCKÉES - SYSTÈME DE GESTION DE BIBLIOTHÈQUE
-- ============================================================

USE BibliothequeDB;
GO

-- ============================================================
-- PROCÉDURE : Effectuer un emprunt
-- ============================================================
CREATE OR ALTER PROCEDURE sp_EffectuerEmprunt
    @IdLivre INT,
    @IdUtilisateur INT,
    @DureeJours INT = 14
AS
BEGIN
    SET NOCOUNT ON;
    BEGIN TRANSACTION;

    BEGIN TRY
        -- Vérifier si le livre est disponible
        DECLARE @StockDisponible INT;
        SELECT @StockDisponible = StockDisponible FROM Livres WHERE IdLivre = @IdLivre;

        IF @StockDisponible IS NULL
        BEGIN
            RAISERROR('Livre non trouvé', 16, 1);
            RETURN;
        END

        IF @StockDisponible <= 0
        BEGIN
            RAISERROR('Livre non disponible', 16, 1);
            RETURN;
        END

        -- Vérifier si l'utilisateur n'a pas atteint sa limite
        DECLARE @NombreEmpruntsActuels INT, @NombreEmpruntsMax INT;
        SELECT @NombreEmpruntsMax = NombreEmpruntsMax FROM Utilisateurs WHERE IdUtilisateur = @IdUtilisateur;
        SELECT @NombreEmpruntsActuels = COUNT(*) FROM Emprunts
        WHERE IdUtilisateur = @IdUtilisateur AND Statut = 'EnCours';

        IF @NombreEmpruntsActuels >= @NombreEmpruntsMax
        BEGIN
            RAISERROR('Limite d''emprunts atteinte', 16, 1);
            RETURN;
        END

        -- Vérifier si l'utilisateur n'a pas déjà ce livre
        IF EXISTS (SELECT 1 FROM Emprunts WHERE IdLivre = @IdLivre AND IdUtilisateur = @IdUtilisateur AND Statut = 'EnCours')
        BEGIN
            RAISERROR('Vous avez déjà emprunté ce livre', 16, 1);
            RETURN;
        END

        -- Créer l'emprunt
        INSERT INTO Emprunts (IdLivre, IdUtilisateur, DateEmprunt, DateRetourPrevue, Statut)
        VALUES (@IdLivre, @IdUtilisateur, GETDATE(), DATEADD(DAY, @DureeJours, GETDATE()), 'EnCours');

        -- Mettre à jour le stock
        UPDATE Livres SET
            StockDisponible = StockDisponible - 1,
            NombreEmprunts = NombreEmprunts + 1,
            DateModification = GETDATE()
        WHERE IdLivre = @IdLivre;

        -- Si l'utilisateur avait une réservation, la marquer comme convertie
        UPDATE Reservations SET
            Statut = 'Convertie'
        WHERE IdLivre = @IdLivre AND IdUtilisateur = @IdUtilisateur AND Statut IN ('EnAttente', 'Disponible');

        COMMIT TRANSACTION;

        SELECT 'Emprunt effectué avec succès' AS Message, SCOPE_IDENTITY() AS IdEmprunt;
    END TRY
    BEGIN CATCH
        ROLLBACK TRANSACTION;
        THROW;
    END CATCH
END
GO

-- ============================================================
-- PROCÉDURE : Effectuer un retour
-- ============================================================
CREATE OR ALTER PROCEDURE sp_EffectuerRetour
    @IdEmprunt INT
AS
BEGIN
    SET NOCOUNT ON;
    BEGIN TRANSACTION;

    BEGIN TRY
        DECLARE @IdLivre INT, @DateRetourPrevue DATETIME2, @Statut NVARCHAR(20);

        SELECT @IdLivre = IdLivre, @DateRetourPrevue = DateRetourPrevue, @Statut = Statut
        FROM Emprunts WHERE IdEmprunt = @IdEmprunt;

        IF @IdLivre IS NULL
        BEGIN
            RAISERROR('Emprunt non trouvé', 16, 1);
            RETURN;
        END

        IF @Statut = 'Termine'
        BEGIN
            RAISERROR('Cet emprunt est déjà terminé', 16, 1);
            RETURN;
        END

        -- Calculer les pénalités si retard
        DECLARE @Penalite DECIMAL(10,2) = 0;
        DECLARE @JoursRetard INT = DATEDIFF(DAY, @DateRetourPrevue, GETDATE());
        IF @JoursRetard > 0
        BEGIN
            SET @Penalite = @JoursRetard * 0.50; -- 0.50€ par jour de retard
        END

        -- Mettre à jour l'emprunt
        UPDATE Emprunts SET
            DateRetourEffective = GETDATE(),
            Statut = 'Termine',
            Penalite = @Penalite
        WHERE IdEmprunt = @IdEmprunt;

        -- Mettre à jour le stock
        UPDATE Livres SET
            StockDisponible = StockDisponible + 1,
            DateModification = GETDATE()
        WHERE IdLivre = @IdLivre;

        -- Notifier le premier en file d'attente
        DECLARE @IdUtilisateurAttente INT;
        SELECT TOP 1 @IdUtilisateurAttente = IdUtilisateur
        FROM Reservations
        WHERE IdLivre = @IdLivre AND Statut = 'EnAttente'
        ORDER BY PositionFile;

        IF @IdUtilisateurAttente IS NOT NULL
        BEGIN
            -- Marquer la réservation comme disponible
            UPDATE Reservations SET
                Statut = 'Disponible',
                DateNotification = GETDATE(),
                DateExpiration = DATEADD(DAY, 3, GETDATE())
            WHERE IdLivre = @IdLivre AND IdUtilisateur = @IdUtilisateurAttente AND Statut = 'EnAttente';

            -- Créer une notification
            DECLARE @TitreLivre NVARCHAR(255);
            SELECT @TitreLivre = Titre FROM Livres WHERE IdLivre = @IdLivre;

            INSERT INTO Notifications (IdUtilisateur, Type, Titre, Message, Lien)
            VALUES (@IdUtilisateurAttente, 'Disponibilite',
                    'Livre disponible !',
                    'Le livre "' + @TitreLivre + '" que vous avez réservé est maintenant disponible. Vous avez 3 jours pour venir le récupérer.',
                    '/Livres/Details/' + CAST(@IdLivre AS NVARCHAR));
        END

        COMMIT TRANSACTION;

        SELECT 'Retour effectué avec succès' AS Message, @Penalite AS Penalite, @JoursRetard AS JoursRetard;
    END TRY
    BEGIN CATCH
        ROLLBACK TRANSACTION;
        THROW;
    END CATCH
END
GO

-- ============================================================
-- PROCÉDURE : Prolonger un emprunt
-- ============================================================
CREATE OR ALTER PROCEDURE sp_ProlongerEmprunt
    @IdEmprunt INT,
    @NombreJours INT = 7
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @NombreProlongations INT, @MaxProlongations INT, @Statut NVARCHAR(20), @IdLivre INT;

    SELECT @NombreProlongations = NombreProlongations,
           @MaxProlongations = MaxProlongations,
           @Statut = Statut,
           @IdLivre = IdLivre
    FROM Emprunts WHERE IdEmprunt = @IdEmprunt;

    IF @Statut IS NULL
    BEGIN
        RAISERROR('Emprunt non trouvé', 16, 1);
        RETURN;
    END

    IF @Statut = 'Termine'
    BEGIN
        RAISERROR('Impossible de prolonger un emprunt terminé', 16, 1);
        RETURN;
    END

    IF @NombreProlongations >= @MaxProlongations
    BEGIN
        RAISERROR('Nombre maximum de prolongations atteint', 16, 1);
        RETURN;
    END

    -- Vérifier s'il y a des réservations en attente
    IF EXISTS (SELECT 1 FROM Reservations WHERE IdLivre = @IdLivre AND Statut = 'EnAttente')
    BEGIN
        RAISERROR('Impossible de prolonger : des réservations sont en attente pour ce livre', 16, 1);
        RETURN;
    END

    UPDATE Emprunts SET
        DateRetourPrevue = DATEADD(DAY, @NombreJours, DateRetourPrevue),
        NombreProlongations = NombreProlongations + 1,
        Statut = 'EnCours' -- Réinitialiser si était en retard
    WHERE IdEmprunt = @IdEmprunt;

    SELECT 'Emprunt prolongé avec succès' AS Message;
END
GO

-- ============================================================
-- PROCÉDURE : Détecter et notifier les retards (à exécuter quotidiennement)
-- ============================================================
CREATE OR ALTER PROCEDURE sp_DetecterRetards
AS
BEGIN
    SET NOCOUNT ON;

    -- Mettre à jour le statut des emprunts en retard
    UPDATE Emprunts SET Statut = 'EnRetard'
    WHERE Statut = 'EnCours' AND DateRetourPrevue < GETDATE();

    -- Créer des notifications pour les nouveaux retards
    INSERT INTO Notifications (IdUtilisateur, Type, Titre, Message, Lien)
    SELECT DISTINCT e.IdUtilisateur,
           'Retard',
           'Retard de retour',
           'Le livre "' + l.Titre + '" devait être retourné le ' + FORMAT(e.DateRetourPrevue, 'dd/MM/yyyy') + '. Veuillez le retourner dès que possible.',
           '/Emprunts/Details/' + CAST(e.IdEmprunt AS NVARCHAR)
    FROM Emprunts e
    INNER JOIN Livres l ON e.IdLivre = l.IdLivre
    WHERE e.Statut = 'EnRetard'
    AND NOT EXISTS (
        SELECT 1 FROM Notifications n
        WHERE n.IdUtilisateur = e.IdUtilisateur
        AND n.Type = 'Retard'
        AND n.Lien = '/Emprunts/Details/' + CAST(e.IdEmprunt AS NVARCHAR)
        AND CAST(n.DateCreation AS DATE) = CAST(GETDATE() AS DATE)
    );

    SELECT @@ROWCOUNT AS NotificationsCreees;
END
GO

-- ============================================================
-- PROCÉDURE : Expirer les réservations non récupérées
-- ============================================================
CREATE OR ALTER PROCEDURE sp_ExpirerReservations
AS
BEGIN
    SET NOCOUNT ON;

    UPDATE Reservations SET Statut = 'Annulee'
    WHERE Statut = 'Disponible' AND DateExpiration < GETDATE();

    -- Réattribuer aux suivants dans la file
    -- (logique simplifiée, à améliorer selon besoins)

    SELECT @@ROWCOUNT AS ReservationsExpirees;
END
GO

-- ============================================================
-- PROCÉDURE : Obtenir les statistiques du dashboard
-- ============================================================
CREATE OR ALTER PROCEDURE sp_GetStatistiquesDashboard
AS
BEGIN
    SET NOCOUNT ON;

    -- Stats générales
    SELECT
        (SELECT COUNT(*) FROM Livres WHERE Actif = 1) AS TotalLivres,
        (SELECT SUM(Stock) FROM Livres WHERE Actif = 1) AS TotalExemplaires,
        (SELECT COUNT(*) FROM Utilisateurs WHERE Actif = 1) AS TotalUtilisateurs,
        (SELECT COUNT(*) FROM Auteurs WHERE Actif = 1) AS TotalAuteurs,
        (SELECT COUNT(*) FROM Categories WHERE Actif = 1) AS TotalCategories,
        (SELECT COUNT(*) FROM Emprunts WHERE Statut = 'EnCours') AS EmpruntsEnCours,
        (SELECT COUNT(*) FROM Emprunts WHERE Statut = 'EnRetard') AS EmpruntsEnRetard,
        (SELECT COUNT(*) FROM Reservations WHERE Statut = 'EnAttente') AS ReservationsEnAttente,
        (SELECT COUNT(*) FROM Emprunts WHERE CAST(DateEmprunt AS DATE) = CAST(GETDATE() AS DATE)) AS EmpruntsAujourdhui,
        (SELECT COUNT(*) FROM Emprunts WHERE DateRetourEffective IS NOT NULL AND CAST(DateRetourEffective AS DATE) = CAST(GETDATE() AS DATE)) AS RetoursAujourdhui;

    -- Top 10 livres les plus empruntés
    SELECT TOP 10 l.IdLivre, l.Titre, a.Nom + ' ' + ISNULL(a.Prenom, '') AS Auteur, l.NombreEmprunts
    FROM Livres l
    INNER JOIN Auteurs a ON l.IdAuteur = a.IdAuteur
    WHERE l.Actif = 1
    ORDER BY l.NombreEmprunts DESC;

    -- Emprunts par mois (12 derniers mois)
    SELECT FORMAT(DateEmprunt, 'yyyy-MM') AS Mois, COUNT(*) AS NombreEmprunts
    FROM Emprunts
    WHERE DateEmprunt >= DATEADD(MONTH, -12, GETDATE())
    GROUP BY FORMAT(DateEmprunt, 'yyyy-MM')
    ORDER BY Mois;

    -- Emprunts par catégorie
    SELECT c.Nom AS Categorie, COUNT(e.IdEmprunt) AS NombreEmprunts
    FROM Categories c
    LEFT JOIN LivreCategories lc ON c.IdCategorie = lc.IdCategorie
    LEFT JOIN Emprunts e ON lc.IdLivre = e.IdLivre
    WHERE c.Actif = 1
    GROUP BY c.IdCategorie, c.Nom
    ORDER BY NombreEmprunts DESC;
END
GO

-- ============================================================
-- PROCÉDURE : Recherche intelligente de livres
-- ============================================================
CREATE OR ALTER PROCEDURE sp_RechercherLivres
    @Recherche NVARCHAR(200) = NULL,
    @IdCategorie INT = NULL,
    @IdAuteur INT = NULL,
    @Annee INT = NULL,
    @Disponible BIT = NULL,
    @Tri NVARCHAR(50) = 'Titre',
    @Page INT = 1,
    @TaillePage INT = 10
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @Offset INT = (@Page - 1) * @TaillePage;

    -- Compte total pour pagination
    SELECT COUNT(DISTINCT l.IdLivre) AS TotalResultats
    FROM Livres l
    INNER JOIN Auteurs a ON l.IdAuteur = a.IdAuteur
    LEFT JOIN LivreCategories lc ON l.IdLivre = lc.IdLivre
    WHERE l.Actif = 1
    AND (@Recherche IS NULL OR (l.Titre LIKE '%' + @Recherche + '%' OR a.Nom LIKE '%' + @Recherche + '%' OR l.ISBN LIKE '%' + @Recherche + '%'))
    AND (@IdCategorie IS NULL OR lc.IdCategorie = @IdCategorie)
    AND (@IdAuteur IS NULL OR l.IdAuteur = @IdAuteur)
    AND (@Annee IS NULL OR l.Annee = @Annee)
    AND (@Disponible IS NULL OR (@Disponible = 1 AND l.StockDisponible > 0) OR (@Disponible = 0 AND l.StockDisponible = 0));

    -- Résultats paginés
    SELECT DISTINCT l.IdLivre, l.ISBN, l.Titre, l.Annee, l.Description, l.ImageCouverture,
           l.Stock, l.StockDisponible, l.NombreEmprunts, l.NoteMoyenne,
           a.IdAuteur, a.Nom AS AuteurNom, a.Prenom AS AuteurPrenom,
           STUFF((SELECT ', ' + c.Nom FROM Categories c
                  INNER JOIN LivreCategories lc2 ON c.IdCategorie = lc2.IdCategorie
                  WHERE lc2.IdLivre = l.IdLivre FOR XML PATH('')), 1, 2, '') AS Categories
    FROM Livres l
    INNER JOIN Auteurs a ON l.IdAuteur = a.IdAuteur
    LEFT JOIN LivreCategories lc ON l.IdLivre = lc.IdLivre
    WHERE l.Actif = 1
    AND (@Recherche IS NULL OR (l.Titre LIKE '%' + @Recherche + '%' OR a.Nom LIKE '%' + @Recherche + '%' OR l.ISBN LIKE '%' + @Recherche + '%'))
    AND (@IdCategorie IS NULL OR lc.IdCategorie = @IdCategorie)
    AND (@IdAuteur IS NULL OR l.IdAuteur = @IdAuteur)
    AND (@Annee IS NULL OR l.Annee = @Annee)
    AND (@Disponible IS NULL OR (@Disponible = 1 AND l.StockDisponible > 0) OR (@Disponible = 0 AND l.StockDisponible = 0))
    ORDER BY
        CASE WHEN @Tri = 'Titre' THEN l.Titre END ASC,
        CASE WHEN @Tri = 'Annee' THEN l.Annee END DESC,
        CASE WHEN @Tri = 'Popularite' THEN l.NombreEmprunts END DESC,
        CASE WHEN @Tri = 'Note' THEN l.NoteMoyenne END DESC,
        CASE WHEN @Tri = 'Recent' THEN l.DateAjout END DESC
    OFFSET @Offset ROWS FETCH NEXT @TaillePage ROWS ONLY;
END
GO

-- ============================================================
-- PROCÉDURE : Recommandations de livres basées sur l'historique
-- ============================================================
CREATE OR ALTER PROCEDURE sp_GetRecommandations
    @IdUtilisateur INT,
    @NombreMax INT = 5
AS
BEGIN
    SET NOCOUNT ON;

    -- Recommandations basées sur les catégories préférées de l'utilisateur
    WITH CategoriesPreferees AS (
        SELECT TOP 3 lc.IdCategorie, COUNT(*) AS NombreEmprunts
        FROM Emprunts e
        INNER JOIN LivreCategories lc ON e.IdLivre = lc.IdLivre
        WHERE e.IdUtilisateur = @IdUtilisateur
        GROUP BY lc.IdCategorie
        ORDER BY NombreEmprunts DESC
    ),
    LivresDejaEmpruntes AS (
        SELECT DISTINCT IdLivre FROM Emprunts WHERE IdUtilisateur = @IdUtilisateur
    )
    SELECT TOP (@NombreMax) l.IdLivre, l.Titre, l.ImageCouverture, l.NoteMoyenne,
           a.Nom + ' ' + ISNULL(a.Prenom, '') AS Auteur,
           'Basé sur vos catégories préférées' AS RaisonRecommandation
    FROM Livres l
    INNER JOIN Auteurs a ON l.IdAuteur = a.IdAuteur
    INNER JOIN LivreCategories lc ON l.IdLivre = lc.IdLivre
    INNER JOIN CategoriesPreferees cp ON lc.IdCategorie = cp.IdCategorie
    WHERE l.Actif = 1
    AND l.StockDisponible > 0
    AND l.IdLivre NOT IN (SELECT IdLivre FROM LivresDejaEmpruntes)
    ORDER BY l.NoteMoyenne DESC, l.NombreEmprunts DESC;
END
GO

PRINT 'Procédures stockées créées avec succès!';
GO
