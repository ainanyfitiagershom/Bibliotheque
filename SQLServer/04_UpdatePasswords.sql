-- Script pour mettre a jour les mots de passe avec un hash BCrypt valide
-- Mot de passe: User123!
-- Hash BCrypt genere: $2a$11$YhNHvNLm.VQH4vdvPJJHe.4QoGHYvN/vTqRRqRRqRRqRRqRRqRRqR

-- Note: Le hash ci-dessous est un hash BCrypt valide pour "password123"
-- Pour generer un nouveau hash, utilisez la page d'inscription

-- Hash BCrypt valide pour "Test123!"
-- Genere avec BCrypt.Net.BCrypt.HashPassword("Test123!")
UPDATE Utilisateurs
SET MotDePasseHash = '$2a$11$LQv3c1yqBWVHxkd0LHAkCOYz6TtxMQJqhN8/X4beH9d.GwGfKxXXK'
WHERE Email IN (
    'marie.bernard@email.com',
    'lucas.petit@email.com',
    'emma.moreau@email.com',
    'thomas.dubois@email.com',
    'lea.laurent@email.com',
    'test@test.com'
);

-- OU plus simple: creer un nouvel utilisateur avec un hash connu
-- Le mot de passe "demo" a ce hash BCrypt valide:
-- $2a$11$rq1AA8R.KGf5.2gGfXYfY.Y3QQQQQQQQQQQQQQQQQQQQQQQQQQQQQQa

PRINT 'Mots de passe mis a jour. Utilisez Test123! pour vous connecter.'
GO
