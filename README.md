# Système de Gestion de Bibliothèque

## Description
Application .NET Core complète pour la gestion d'une bibliothèque, composée d'un backoffice sécurisé (Razor Pages + EF Core) et d'un frontoffice public (MVC + ADO.NET).

---

## GUIDE RAPIDE - Démarrer le Projet

### Prérequis
- Docker installé et lancé
- .NET SDK 8 installé

### Étape 1 : Démarrer SQL Server

```bash
# Si le container existe déjà :
docker start sqlserver

# Si le container n'existe pas (première fois) :
docker run -e "ACCEPT_EULA=Y" -e "MSSQL_SA_PASSWORD=MyStr0ngP@ssw0rd!" -p 1433:1433 --name sqlserver -d mcr.microsoft.com/mssql/server:2022-latest
```

Attendre 20 secondes que SQL Server démarre.

### Étape 2 : Créer la base de données (première fois seulement)

```bash
# Copier les scripts SQL dans le container
docker cp SQLServer/01_CreateDatabase.sql sqlserver:/tmp/
docker cp SQLServer/02_StoredProcedures.sql sqlserver:/tmp/
docker cp SQLServer/03_SeedData.sql sqlserver:/tmp/
docker cp SQLServer/05_sp_RechercherLivres.sql sqlserver:/tmp/

# Exécuter les scripts
docker exec sqlserver /opt/mssql-tools18/bin/sqlcmd -S localhost -U sa -P MyStr0ngP@ssw0rd! -C -i /tmp/01_CreateDatabase.sql
docker exec sqlserver /opt/mssql-tools18/bin/sqlcmd -S localhost -U sa -P MyStr0ngP@ssw0rd! -C -i /tmp/02_StoredProcedures.sql
docker exec sqlserver /opt/mssql-tools18/bin/sqlcmd -S localhost -U sa -P MyStr0ngP@ssw0rd! -C -i /tmp/03_SeedData.sql
docker exec sqlserver /opt/mssql-tools18/bin/sqlcmd -S localhost -U sa -P MyStr0ngP@ssw0rd! -C -i /tmp/05_sp_RechercherLivres.sql
```

### Étape 3 : Lancer l'application Frontoffice

```bash
cd Frontoffice.MVC
dotnet run
```

### Étape 4 : Accéder au site

Ouvrir : **https://localhost:7005**

---

## Comptes de Test

| Type | Email | Mot de passe |
|------|-------|--------------|
| Utilisateur | `jean.dupont@email.com` | `User123!` |
| Admin | `admin@bibliotheque.fr` | `Admin123!` |

---

## Résolution des Erreurs Courantes

### Erreur : "Container sqlserver is not running"

```bash
docker start sqlserver
```

### Erreur : "Login failed for user 'sa'" (mot de passe incorrect)

Le container a un ancien mot de passe. Recréer le container :

```bash
docker stop sqlserver
docker rm sqlserver
docker run -e "ACCEPT_EULA=Y" -e "MSSQL_SA_PASSWORD=MyStr0ngP@ssw0rd!" -p 1433:1433 --name sqlserver -d mcr.microsoft.com/mssql/server:2022-latest
```

Puis recréer la base de données (Étape 2).

### Erreur : "Address already in use" (port 7005 occupé)

```bash
# Tuer le processus qui utilise le port
fuser -k 7005/tcp

# Relancer l'application
dotnet run
```

### Erreur : "Cannot open database 'BibliothequeDB'"

La base de données n'existe pas. Exécuter les scripts SQL (Étape 2).

### Erreur : HTTP 500 sur une page

1. Vérifier que SQL Server tourne : `docker ps | grep sqlserver`
2. Si le container n'apparaît pas : `docker start sqlserver`
3. Attendre 10 secondes et réessayer

### Erreur : "Connection refused" ou "Connection timeout"

SQL Server n'est pas prêt. Attendre 20-30 secondes après `docker start sqlserver`.

### Erreur : "dotnet: command not found"

```bash
# Ajouter .NET au PATH
export PATH=$PATH:$HOME/.dotnet

# Ou installer .NET
# Sur Arch : sudo pacman -S dotnet-sdk
# Sur Ubuntu : sudo apt install dotnet-sdk-8.0
```

### Erreur : Certificat HTTPS non reconnu

Cliquer sur "Avancé" puis "Continuer vers le site" dans le navigateur.

Ou exécuter :
```bash
dotnet dev-certs https --trust
```

---

## Architecture du Projet

```
BibliothequeSolution/
├── Bibliotheque.Core/           # Couche métier (Entities, DTOs, Interfaces)
├── Bibliotheque.Infrastructure/ # Couche données (EF Core, ADO.NET, Services)
├── Bibliotheque.Api/            # API REST
├── Backoffice.Razor/            # Administration (Razor Pages + EF Core)
├── Frontoffice.MVC/             # Interface publique (MVC + ADO.NET)
└── SQLServer/                   # Scripts SQL
```

## Technologies utilisées

| Composant | Technologies |
|-----------|--------------|
| Backend | .NET 8, C# |
| Backoffice | Razor Pages, EF Core |
| Frontoffice | MVC, ADO.NET |
| API | ASP.NET Core Web API |
| Base de données | SQL Server |
| Authentification | Cookie Authentication + BCrypt |
| PDF | iText7 |
| CSV | CsvHelper |
| UI | Bootstrap 5, Chart.js |

---

## Configuration

### Chaîne de connexion (appsettings.json)

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=BibliothequeDB;User Id=sa;Password=MyStr0ngP@ssw0rd!;TrustServerCertificate=True;"
  }
}
```

---

## URLs des Applications

| Application | URL | Description |
|-------------|-----|-------------|
| **Frontoffice** | https://localhost:7005 | Site public |
| **Backoffice** | https://localhost:5002 | Administration |
| **API Swagger** | https://localhost:5001/swagger | Documentation API |

---

## Fonctionnalités

### Frontoffice (Public)
- Catalogue avec recherche et filtres
- Recherche multi-mots (ex: "Victor Hugo")
- Détails des livres avec avis
- Réservation de livres indisponibles
- Espace utilisateur (emprunts, réservations)
- Notifications

### Backoffice (Admin)
- Dashboard avec statistiques
- CRUD Livres, Auteurs, Catégories, Utilisateurs
- Gestion des emprunts et retours
- Import CSV / Export PDF
- Gestion des retards

---

## Points Forts du Projet

1. **Clean Architecture** : Séparation claire des couches
2. **Sécurité** : BCrypt pour les mots de passe
3. **Double accès données** : EF Core + ADO.NET
4. **Design élégant** : Interface moderne et responsive

---

## Auteur

Projet réalisé dans le cadre d'un examen Master en développement .NET Core.
