-- ============================================================
-- DONNÉES DE TEST - SYSTÈME DE GESTION DE BIBLIOTHÈQUE
-- ============================================================

USE BibliothequeDB;
GO

-- ============================================================
-- ADMINS (mot de passe: Admin123!)
-- Hash BCrypt de "Admin123!" - À générer via l'application
-- ============================================================
INSERT INTO Admins (Nom, Prenom, Email, MotDePasseHash) VALUES
('Dupont', 'Jean', 'admin@bibliotheque.com', '$2a$11$rBNdBKCZPI.nhxkVb/VXaOhZp7XgVpL9PGpLXK.YptPqXqKqXqKqK'),
('Martin', 'Sophie', 'sophie.martin@bibliotheque.com', '$2a$11$rBNdBKCZPI.nhxkVb/VXaOhZp7XgVpL9PGpLXK.YptPqXqKqXqKqK');
GO

-- ============================================================
-- CATEGORIES
-- ============================================================
INSERT INTO Categories (Nom, Description, Couleur, Icone) VALUES
('Roman', 'Romans et fiction littéraire', '#28a745', 'bi-book'),
('Science-Fiction', 'Science-fiction et fantasy', '#17a2b8', 'bi-rocket'),
('Policier', 'Romans policiers et thrillers', '#dc3545', 'bi-search'),
('Histoire', 'Livres historiques et biographies', '#ffc107', 'bi-clock-history'),
('Sciences', 'Sciences et technologies', '#6f42c1', 'bi-lightbulb'),
('Philosophie', 'Philosophie et essais', '#fd7e14', 'bi-chat-quote'),
('Jeunesse', 'Littérature jeunesse', '#e83e8c', 'bi-emoji-smile'),
('BD & Manga', 'Bandes dessinées et mangas', '#20c997', 'bi-image'),
('Poésie', 'Poésie et théâtre', '#6c757d', 'bi-feather'),
('Informatique', 'Programmation et informatique', '#007bff', 'bi-code-slash');
GO

-- ============================================================
-- AUTEURS
-- ============================================================
INSERT INTO Auteurs (Nom, Prenom, Nationalite, DateNaissance, DateDeces, Biographie) VALUES
('Hugo', 'Victor', 'Française', '1802-02-26', '1885-05-22', 'Poète, dramaturge, écrivain, romancier et dessinateur romantique français.'),
('Orwell', 'George', 'Britannique', '1903-06-25', '1950-01-21', 'Écrivain et journaliste britannique, auteur de 1984 et La Ferme des animaux.'),
('Rowling', 'J.K.', 'Britannique', '1965-07-31', NULL, 'Romancière britannique, auteure de la saga Harry Potter.'),
('Camus', 'Albert', 'Française', '1913-11-07', '1960-01-04', 'Écrivain, philosophe, romancier et dramaturge français. Prix Nobel de littérature 1957.'),
('Asimov', 'Isaac', 'Américaine', '1920-01-02', '1992-04-06', 'Écrivain américano-russe, auteur majeur de science-fiction.'),
('Christie', 'Agatha', 'Britannique', '1890-09-15', '1976-01-12', 'Femme de lettres britannique, auteure de nombreux romans policiers.'),
('Tolkien', 'J.R.R.', 'Britannique', '1892-01-03', '1973-09-02', 'Écrivain, poète, philologue et professeur britannique, auteur du Seigneur des Anneaux.'),
('Saint-Exupéry', 'Antoine de', 'Française', '1900-06-29', '1944-07-31', 'Écrivain, poète et aviateur français, auteur du Petit Prince.'),
('Zola', 'Émile', 'Française', '1840-04-02', '1902-09-29', 'Écrivain et journaliste français, considéré comme le chef de file du naturalisme.'),
('Molière', NULL, 'Française', '1622-01-15', '1673-02-17', 'Dramaturge, comédien et chef de troupe français.'),
('Verne', 'Jules', 'Française', '1828-02-08', '1905-03-24', 'Écrivain français, auteur de romans d''aventures et de science-fiction.'),
('King', 'Stephen', 'Américaine', '1947-09-21', NULL, 'Écrivain américain de romans d''horreur, de suspense et de fantastique.'),
('Murakami', 'Haruki', 'Japonaise', '1949-01-12', NULL, 'Écrivain japonais contemporain, plusieurs fois pressenti pour le prix Nobel.'),
('Dumas', 'Alexandre', 'Française', '1802-07-24', '1870-12-05', 'Écrivain français, auteur des Trois Mousquetaires et du Comte de Monte-Cristo.'),
('Martin', 'Robert C.', 'Américaine', '1952-12-05', NULL, 'Ingénieur logiciel et auteur américain, connu pour ses livres sur le Clean Code.');
GO

-- ============================================================
-- LIVRES
-- ============================================================
INSERT INTO Livres (ISBN, Titre, IdAuteur, Annee, Editeur, NombrePages, Langue, Description, Stock, StockDisponible, Emplacement) VALUES
('978-2070409228', 'Les Misérables', 1, 1862, 'Gallimard', 1900, 'Français', 'Roman historique de Victor Hugo, considéré comme l''un des plus grands romans du XIXe siècle.', 3, 2, 'A-01'),
('978-2070368228', 'Notre-Dame de Paris', 1, 1831, 'Gallimard', 940, 'Français', 'Roman historique de Victor Hugo se déroulant dans le Paris médiéval.', 2, 2, 'A-02'),
('978-2070368013', '1984', 2, 1949, 'Gallimard', 438, 'Français', 'Roman dystopique de George Orwell décrivant un régime totalitaire.', 4, 3, 'B-01'),
('978-2070417612', 'La Ferme des animaux', 2, 1945, 'Gallimard', 150, 'Français', 'Fable satirique de George Orwell dénonçant le totalitarisme.', 3, 3, 'B-02'),
('978-2070584628', 'Harry Potter à l''école des sorciers', 3, 1997, 'Gallimard Jeunesse', 320, 'Français', 'Premier tome de la saga Harry Potter.', 5, 4, 'C-01'),
('978-2070584635', 'Harry Potter et la Chambre des Secrets', 3, 1998, 'Gallimard Jeunesse', 352, 'Français', 'Deuxième tome de la saga Harry Potter.', 4, 4, 'C-02'),
('978-2070360024', 'L''Étranger', 4, 1942, 'Gallimard', 186, 'Français', 'Roman d''Albert Camus, récit à la première personne.', 3, 2, 'D-01'),
('978-2070360031', 'La Peste', 4, 1947, 'Gallimard', 352, 'Français', 'Roman d''Albert Camus décrivant une épidémie de peste à Oran.', 2, 1, 'D-02'),
('978-2070360048', 'Le Mythe de Sisyphe', 4, 1942, 'Gallimard', 192, 'Français', 'Essai philosophique d''Albert Camus sur l''absurde.', 2, 2, 'D-03'),
('978-2070415700', 'Fondation', 5, 1951, 'Gallimard', 416, 'Français', 'Premier tome du cycle de Fondation d''Isaac Asimov.', 3, 3, 'E-01'),
('978-2070415717', 'Fondation et Empire', 5, 1952, 'Gallimard', 320, 'Français', 'Deuxième tome du cycle de Fondation.', 2, 2, 'E-02'),
('978-2253010265', 'Le Crime de l''Orient-Express', 6, 1934, 'Le Livre de Poche', 256, 'Français', 'Roman policier d''Agatha Christie mettant en scène Hercule Poirot.', 3, 2, 'F-01'),
('978-2253006329', 'Dix Petits Nègres', 6, 1939, 'Le Livre de Poche', 224, 'Français', 'Roman policier d''Agatha Christie.', 2, 2, 'F-02'),
('978-2266286268', 'Le Seigneur des Anneaux - La Communauté de l''Anneau', 7, 1954, 'Pocket', 544, 'Français', 'Premier tome de la trilogie du Seigneur des Anneaux.', 4, 3, 'G-01'),
('978-2070612758', 'Le Petit Prince', 8, 1943, 'Gallimard', 96, 'Français', 'Conte poétique et philosophique d''Antoine de Saint-Exupéry.', 6, 5, 'H-01'),
('978-2070409341', 'Germinal', 9, 1885, 'Gallimard', 592, 'Français', 'Roman d''Émile Zola sur les conditions de vie des mineurs.', 2, 2, 'I-01'),
('978-2070409358', 'L''Assommoir', 9, 1877, 'Gallimard', 512, 'Français', 'Roman d''Émile Zola sur l''alcoolisme dans le milieu ouvrier.', 2, 1, 'I-02'),
('978-2070413119', 'Le Malade imaginaire', 10, 1673, 'Gallimard', 192, 'Français', 'Comédie-ballet de Molière.', 3, 3, 'J-01'),
('978-2070413126', 'Le Tartuffe', 10, 1664, 'Gallimard', 160, 'Français', 'Comédie de Molière sur l''hypocrisie religieuse.', 2, 2, 'J-02'),
('978-2253004226', 'Vingt mille lieues sous les mers', 11, 1870, 'Le Livre de Poche', 544, 'Français', 'Roman d''aventures de Jules Verne.', 3, 3, 'K-01'),
('978-2253012702', 'Le Tour du monde en quatre-vingts jours', 11, 1872, 'Le Livre de Poche', 320, 'Français', 'Roman d''aventures de Jules Verne.', 2, 2, 'K-02'),
('978-2253151531', 'Ça', 12, 1986, 'Le Livre de Poche', 1536, 'Français', 'Roman d''horreur de Stephen King.', 2, 1, 'L-01'),
('978-2253004578', 'Shining', 12, 1977, 'Le Livre de Poche', 544, 'Français', 'Roman d''horreur de Stephen King.', 2, 2, 'L-02'),
('978-2714479457', 'Kafka sur le rivage', 13, 2002, 'Belfond', 638, 'Français', 'Roman de Haruki Murakami.', 2, 2, 'M-01'),
('978-2253153825', 'Les Trois Mousquetaires', 14, 1844, 'Le Livre de Poche', 896, 'Français', 'Roman de cape et d''épée d''Alexandre Dumas.', 3, 3, 'N-01'),
('978-2253098058', 'Le Comte de Monte-Cristo', 14, 1844, 'Le Livre de Poche', 1504, 'Français', 'Roman d''aventures d''Alexandre Dumas.', 2, 2, 'N-02'),
('978-0132350884', 'Clean Code', 15, 2008, 'Prentice Hall', 464, 'Anglais', 'Guide pratique pour écrire du code propre et maintenable.', 3, 3, 'O-01'),
('978-0134494166', 'Clean Architecture', 15, 2017, 'Prentice Hall', 432, 'Anglais', 'Guide pour concevoir des architectures logicielles durables.', 2, 2, 'O-02');
GO

-- ============================================================
-- ASSOCIATION LIVRES-CATEGORIES
-- ============================================================
INSERT INTO LivreCategories (IdLivre, IdCategorie) VALUES
(1, 1), (1, 4), -- Les Misérables: Roman, Histoire
(2, 1), (2, 4), -- Notre-Dame de Paris: Roman, Histoire
(3, 1), (3, 2), -- 1984: Roman, Science-Fiction
(4, 1), -- La Ferme des animaux: Roman
(5, 7), (5, 1), -- Harry Potter 1: Jeunesse, Roman
(6, 7), (6, 1), -- Harry Potter 2: Jeunesse, Roman
(7, 1), (7, 6), -- L'Étranger: Roman, Philosophie
(8, 1), -- La Peste: Roman
(9, 6), -- Le Mythe de Sisyphe: Philosophie
(10, 2), -- Fondation: Science-Fiction
(11, 2), -- Fondation et Empire: Science-Fiction
(12, 3), -- Le Crime de l'Orient-Express: Policier
(13, 3), -- Dix Petits Nègres: Policier
(14, 2), (14, 1), -- Le Seigneur des Anneaux: Science-Fiction, Roman
(15, 7), (15, 6), -- Le Petit Prince: Jeunesse, Philosophie
(16, 1), (16, 4), -- Germinal: Roman, Histoire
(17, 1), -- L'Assommoir: Roman
(18, 9), -- Le Malade imaginaire: Poésie (Théâtre)
(19, 9), -- Le Tartuffe: Poésie (Théâtre)
(20, 2), (20, 1), -- Vingt mille lieues: Science-Fiction, Roman
(21, 1), -- Le Tour du monde: Roman
(22, 3), (22, 1), -- Ça: Policier (Horreur), Roman
(23, 3), (23, 1), -- Shining: Policier (Horreur), Roman
(24, 1), -- Kafka sur le rivage: Roman
(25, 1), (25, 4), -- Les Trois Mousquetaires: Roman, Histoire
(26, 1), (26, 4), -- Le Comte de Monte-Cristo: Roman, Histoire
(27, 10), -- Clean Code: Informatique
(28, 10); -- Clean Architecture: Informatique
GO

-- ============================================================
-- UTILISATEURS (mot de passe: User123!)
-- Hash BCrypt genere pour "User123!" : $2a$11$8K1p/a0dL1LXMIgoEDFrwOfMQkf9.8xA6WB.P1oMD7.v0VYxYYxYu
-- ============================================================
INSERT INTO Utilisateurs (Nom, Prenom, Email, Telephone, Adresse, MotDePasseHash) VALUES
('Bernard', 'Marie', 'marie.bernard@email.com', '0612345678', '15 Rue de la Paix, 75001 Paris', '$2a$11$8K1p/a0dL1LXMIgoEDFrwOfMQkf9.8xA6WB.P1oMD7.v0VYxYYxYu'),
('Petit', 'Lucas', 'lucas.petit@email.com', '0623456789', '28 Avenue des Champs, 69001 Lyon', '$2a$11$8K1p/a0dL1LXMIgoEDFrwOfMQkf9.8xA6WB.P1oMD7.v0VYxYYxYu'),
('Moreau', 'Emma', 'emma.moreau@email.com', '0634567890', '42 Boulevard Victor Hugo, 33000 Bordeaux', '$2a$11$8K1p/a0dL1LXMIgoEDFrwOfMQkf9.8xA6WB.P1oMD7.v0VYxYYxYu'),
('Dubois', 'Thomas', 'thomas.dubois@email.com', '0645678901', '7 Place de la République, 44000 Nantes', '$2a$11$8K1p/a0dL1LXMIgoEDFrwOfMQkf9.8xA6WB.P1oMD7.v0VYxYYxYu'),
('Laurent', 'Léa', 'lea.laurent@email.com', '0656789012', '33 Rue Nationale, 59000 Lille', '$2a$11$8K1p/a0dL1LXMIgoEDFrwOfMQkf9.8xA6WB.P1oMD7.v0VYxYYxYu'),
('Test', 'Utilisateur', 'test@test.com', '0600000000', '1 Rue Test, 75000 Paris', '$2a$11$8K1p/a0dL1LXMIgoEDFrwOfMQkf9.8xA6WB.P1oMD7.v0VYxYYxYu');
GO

-- ============================================================
-- EMPRUNTS DE TEST
-- ============================================================
INSERT INTO Emprunts (IdLivre, IdUtilisateur, DateEmprunt, DateRetourPrevue, Statut) VALUES
(1, 1, DATEADD(DAY, -10, GETDATE()), DATEADD(DAY, 4, GETDATE()), 'EnCours'),  -- Marie a emprunté Les Misérables
(3, 1, DATEADD(DAY, -20, GETDATE()), DATEADD(DAY, -6, GETDATE()), 'EnRetard'), -- Marie en retard sur 1984
(5, 2, DATEADD(DAY, -5, GETDATE()), DATEADD(DAY, 9, GETDATE()), 'EnCours'),   -- Lucas a emprunté Harry Potter
(15, 3, DATEADD(DAY, -7, GETDATE()), DATEADD(DAY, 7, GETDATE()), 'EnCours'),  -- Emma a emprunté Le Petit Prince
(8, 4, DATEADD(DAY, -30, GETDATE()), DATEADD(DAY, -16, GETDATE()), 'Termine'), -- Thomas a rendu La Peste
(22, 5, DATEADD(DAY, -3, GETDATE()), DATEADD(DAY, 11, GETDATE()), 'EnCours'); -- Léa a emprunté Ça
GO

-- Mettre à jour DateRetourEffective pour l'emprunt terminé
UPDATE Emprunts SET DateRetourEffective = DATEADD(DAY, -18, GETDATE()) WHERE IdEmprunt = 5;
GO

-- Mettre à jour les stocks
UPDATE Livres SET StockDisponible = StockDisponible - 1 WHERE IdLivre IN (1, 3, 5, 15, 22);
UPDATE Livres SET NombreEmprunts = NombreEmprunts + 1 WHERE IdLivre IN (1, 3, 5, 8, 15, 22);
GO

-- ============================================================
-- RESERVATIONS DE TEST
-- ============================================================
INSERT INTO Reservations (IdLivre, IdUtilisateur, DateReservation, DateExpiration, PositionFile, Statut) VALUES
(3, 2, DATEADD(DAY, -2, GETDATE()), DATEADD(DAY, 5, GETDATE()), 1, 'EnAttente'), -- Lucas attend 1984
(1, 4, DATEADD(DAY, -1, GETDATE()), DATEADD(DAY, 6, GETDATE()), 1, 'EnAttente'); -- Thomas attend Les Misérables
GO

-- ============================================================
-- AVIS DE TEST
-- ============================================================
INSERT INTO Avis (IdLivre, IdUtilisateur, Note, Commentaire, Approuve) VALUES
(15, 1, 5, 'Un chef-d''œuvre intemporel ! À lire absolument.', 1),
(15, 2, 5, 'Magnifique conte philosophique. Parfait pour tous les âges.', 1),
(3, 3, 4, 'Terrifiant et visionnaire. Toujours d''actualité.', 1),
(5, 4, 5, 'Magique ! Le début d''une aventure extraordinaire.', 1),
(8, 1, 4, 'Une réflexion profonde sur l''humanité face à la catastrophe.', 1);
GO

-- Mettre à jour les notes moyennes
UPDATE Livres SET NoteMoyenne = 5.0 WHERE IdLivre = 15;
UPDATE Livres SET NoteMoyenne = 4.0 WHERE IdLivre = 3;
UPDATE Livres SET NoteMoyenne = 5.0 WHERE IdLivre = 5;
UPDATE Livres SET NoteMoyenne = 4.0 WHERE IdLivre = 8;
GO

-- ============================================================
-- NOTIFICATIONS DE TEST
-- ============================================================
INSERT INTO Notifications (IdUtilisateur, Type, Titre, Message, Lien) VALUES
(1, 'Retard', 'Retard de retour', 'Le livre "1984" devait être retourné il y a 6 jours. Veuillez le retourner dès que possible.', '/Emprunts/Details/2'),
(1, 'Bienvenue', 'Bienvenue à la bibliothèque !', 'Votre compte a été créé avec succès. Vous pouvez emprunter jusqu''à 3 livres simultanément.', '/'),
(2, 'Rappel', 'Rappel de retour', 'N''oubliez pas de retourner "Harry Potter à l''école des sorciers" avant le ' + FORMAT(DATEADD(DAY, 9, GETDATE()), 'dd/MM/yyyy') + '.', '/Emprunts/Details/3');
GO

PRINT 'Données de test insérées avec succès!';
GO
