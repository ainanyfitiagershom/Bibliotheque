USE BibliothequeDB;
GO

CREATE OR ALTER PROCEDURE sp_RechercherLivres
    @Recherche NVARCHAR(200) = NULL,
    @CategorieId INT = NULL,
    @Page INT = 1,
    @PageSize INT = 12,
    @Tri NVARCHAR(50) = 'titre'
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @Offset INT = (@Page - 1) * @PageSize;

    -- Table temporaire pour stocker les résultats
    CREATE TABLE #Resultats (
        IdLivre INT,
        ISBN NVARCHAR(20),
        Titre NVARCHAR(200),
        NomAuteur NVARCHAR(200),
        Annee INT,
        Editeur NVARCHAR(100),
        Description NVARCHAR(MAX),
        ImageCouverture NVARCHAR(500),
        Stock INT,
        StockDisponible INT,
        NoteMoyenne FLOAT
    );

    -- Insérer les résultats filtrés
    -- La recherche est faite sur: titre, nom auteur (Prénom Nom ou Nom Prénom), ISBN, description
    INSERT INTO #Resultats
    SELECT
        l.IdLivre,
        l.ISBN,
        l.Titre,
        ISNULL(a.Prenom + ' ', '') + a.Nom AS NomAuteur,
        l.Annee,
        l.Editeur,
        l.Description,
        l.ImageCouverture,
        l.Stock,
        l.StockDisponible,
        (SELECT AVG(CAST(Note AS FLOAT)) FROM Avis WHERE IdLivre = l.IdLivre) AS NoteMoyenne
    FROM Livres l
    LEFT JOIN Auteurs a ON l.IdAuteur = a.IdAuteur
    LEFT JOIN LivreCategories lc ON l.IdLivre = lc.IdLivre
    WHERE l.Actif = 1
        AND (@CategorieId IS NULL OR lc.IdCategorie = @CategorieId)
        AND (@Recherche IS NULL OR @Recherche = '' OR
             l.Titre LIKE '%' + @Recherche + '%' OR
             a.Nom LIKE '%' + @Recherche + '%' OR
             a.Prenom LIKE '%' + @Recherche + '%' OR
             ISNULL(a.Prenom + ' ', '') + a.Nom LIKE '%' + @Recherche + '%' OR
             a.Nom + ' ' + ISNULL(a.Prenom, '') LIKE '%' + @Recherche + '%' OR
             l.ISBN LIKE '%' + @Recherche + '%' OR
             l.Description LIKE '%' + @Recherche + '%');

    -- Supprimer les doublons (un livre peut avoir plusieurs catégories)
    ;WITH CTE AS (
        SELECT *, ROW_NUMBER() OVER (PARTITION BY IdLivre ORDER BY IdLivre) AS RowNum
        FROM #Resultats
    )
    DELETE FROM CTE WHERE RowNum > 1;

    -- Retourner les résultats paginés avec tri
    SELECT * FROM #Resultats
    ORDER BY
        CASE WHEN @Tri = 'titre' THEN Titre END ASC,
        CASE WHEN @Tri = 'titre_desc' THEN Titre END DESC,
        CASE WHEN @Tri = 'annee' THEN Annee END DESC,
        CASE WHEN @Tri = 'note' THEN NoteMoyenne END DESC,
        CASE WHEN @Tri = 'recent' THEN IdLivre END DESC
    OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY;

    -- Retourner le nombre total
    SELECT COUNT(*) AS TotalCount FROM #Resultats;

    DROP TABLE #Resultats;
END
GO

-- Test de la procédure avec "Victor Hugo"
EXEC sp_RechercherLivres @Recherche = 'Victor Hugo';
GO
