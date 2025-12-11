# Système de Gestion de Bibliothèque

## Description
Application .NET Core complète pour la gestion d'une bibliothèque, composée d'un backoffice sécurisé (Razor Pages + EF Core) et d'un frontoffice public (MVC + ADO.NET).

## Architecture

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

## Guide d'Installation Étape par Étape

### Étape 1 : Installer le SDK .NET 8

#### Sur Arch Linux :
```bash
sudo pacman -S dotnet-sdk
```

#### Sur Ubuntu/Debian :
```bash
wget https://packages.microsoft.com/config/ubuntu/22.04/packages-microsoft-prod.deb -O packages-microsoft-prod.deb
sudo dpkg -i packages-microsoft-prod.deb
rm packages-microsoft-prod.deb
sudo apt update
sudo apt install dotnet-sdk-8.0
```

#### Sur Windows :
Télécharger l'installateur depuis : https://dotnet.microsoft.com/download/dotnet/8.0

#### Vérifier l'installation :
```bash
dotnet --version
# Doit afficher 8.x.x
```

---

### Étape 2 : Installer SQL Server

#### Option A : Docker (Recommandé pour Linux/Mac)
```bash
# Télécharger et lancer SQL Server
docker run -e "ACCEPT_EULA=Y" -e "MSSQL_SA_PASSWORD=Biblio123!" \
  -p 1433:1433 --name sqlserver \
  -d mcr.microsoft.com/mssql/server:2022-latest

# Vérifier que le conteneur est lancé
docker ps
```

#### Option B : SQL Server Express (Windows)
1. Télécharger depuis : https://www.microsoft.com/sql-server/sql-server-downloads
2. Choisir "Express" (gratuit)
3. Installer avec les options par défaut

#### Option C : SQL Server sur Arch Linux
```bash
yay -S mssql-server
sudo /opt/mssql/bin/mssql-conf setup
sudo systemctl start mssql-server
sudo systemctl enable mssql-server
```

---

### Étape 3 : Installer les outils SQL (optionnel mais utile)

```bash
# Sur Arch Linux
yay -S mssql-tools

# Sur Ubuntu/Debian
curl https://packages.microsoft.com/keys/microsoft.asc | sudo apt-key add -
curl https://packages.microsoft.com/config/ubuntu/22.04/prod.list | sudo tee /etc/apt/sources.list.d/msprod.list
sudo apt update
sudo apt install mssql-tools unixodbc-dev
```

---

### Étape 4 : Créer la Base de Données

#### Option A : Avec sqlcmd (ligne de commande)
```bash
# Se placer dans le dossier du projet
cd /home/tsarajoro/Documents/net/BibliothequeSolution

# Créer la base de données
sqlcmd -S localhost -U sa -P Biblio123! -i SQLServer/01_CreateDatabase.sql

# Créer les procédures stockées
sqlcmd -S localhost -U sa -P Biblio123! -d Bibliotheque -i SQLServer/02_StoredProcedures.sql

# Insérer les données de test
sqlcmd -S localhost -U sa -P Biblio123! -d Bibliotheque -i SQLServer/03_SeedData.sql
```

#### Option B : Avec Azure Data Studio ou SSMS
1. Se connecter à `localhost` avec l'utilisateur `sa`
2. Ouvrir et exécuter `SQLServer/01_CreateDatabase.sql`
3. Ouvrir et exécuter `SQLServer/02_StoredProcedures.sql`
4. Ouvrir et exécuter `SQLServer/03_SeedData.sql`

---

### Étape 5 : Configurer les Chaînes de Connexion

Modifier les fichiers `appsettings.json` dans chaque projet :

#### Backoffice.Razor/appsettings.json :
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=Bibliotheque;User Id=sa;Password=Biblio123!;TrustServerCertificate=True;"
  }
}
```

#### Frontoffice.MVC/appsettings.json :
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=Bibliotheque;User Id=sa;Password=Biblio123!;TrustServerCertificate=True;"
  }
}
```

#### Bibliotheque.Api/appsettings.json :
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=Bibliotheque;User Id=sa;Password=Biblio123!;TrustServerCertificate=True;"
  }
}
```

---

### Étape 6 : Restaurer les Packages NuGet

```bash
cd /home/tsarajoro/Documents/net/BibliothequeSolution
dotnet restore
```

---

### Étape 7 : Compiler le Projet

```bash
dotnet build
```

Si tout est bon, vous verrez :
```
Build succeeded.
    0 Warning(s)
    0 Error(s)
```

---

### Étape 8 : Lancer les Applications

#### Terminal 1 - API REST (Swagger) :
```bash
cd /home/tsarajoro/Documents/net/BibliothequeSolution/Bibliotheque.Api
dotnet run
```
Accès : https://localhost:5001/swagger

#### Terminal 2 - Backoffice (Administration) :
```bash
cd /home/tsarajoro/Documents/net/BibliothequeSolution/Backoffice.Razor
dotnet run
```
Accès : https://localhost:5002

#### Terminal 3 - Frontoffice (Site Public) :
```bash
cd /home/tsarajoro/Documents/net/BibliothequeSolution/Frontoffice.MVC
dotnet run
```
Accès : https://localhost:5003

---

### Étape 9 : Accéder aux Applications

| Application | URL | Description |
|-------------|-----|-------------|
| **API Swagger** | https://localhost:5001/swagger | Documentation API REST interactive |
| **Backoffice** | https://localhost:5002 | Interface d'administration |
| **Frontoffice** | https://localhost:5003 | Site public pour les utilisateurs |

---

## Comptes de Test

### Administration (Backoffice)
| Champ | Valeur |
|-------|--------|
| Email | `admin@bibliotheque.fr` |
| Mot de passe | `Admin123!` |

### Utilisateur (Frontoffice)
| Champ | Valeur |
|-------|--------|
| Email | `jean.dupont@email.com` |
| Mot de passe | `User123!` |

---

## Dépannage

### Erreur : "Connection refused" SQL Server
```bash
# Vérifier que SQL Server est lancé
docker ps                          # Si Docker
sudo systemctl status mssql-server # Si installé nativement
```

### Erreur : "Certificate error"
Ajouter `TrustServerCertificate=True;` à la chaîne de connexion.

### Erreur : "Port already in use"
Modifier le port dans `Properties/launchSettings.json` de chaque projet.

### Erreur : "dotnet: command not found"
```bash
# Recharger le shell
source ~/.bashrc
# ou
export PATH=$PATH:$HOME/.dotnet
```

---

## Fonctionnalités

### Backoffice (Admin)
- [x] Authentification sécurisée
- [x] Dashboard avec statistiques et graphiques Chart.js
- [x] CRUD Livres avec pagination et recherche
- [x] CRUD Auteurs
- [x] CRUD Catégories
- [x] CRUD Utilisateurs (avec blocage/déblocage)
- [x] Gestion des emprunts (création, retour, prolongation)
- [x] Import CSV (Livres, Auteurs, Catégories, Utilisateurs)
- [x] Export PDF et CSV
- [x] Gestion automatique des retards

### Frontoffice (Public)
- [x] Catalogue avec recherche et filtres
- [x] Détails des livres
- [x] Système de réservation avec file d'attente
- [x] Espace utilisateur complet
- [x] Historique des emprunts
- [x] Notifications (retards, disponibilité)
- [x] Recommandations personnalisées

### API REST
- [x] Endpoints CRUD complets
- [x] Documentation Swagger interactive
- [x] CORS configuré

---

## Structure de la Base de Données

| Table | Description |
|-------|-------------|
| **Admins** | Administrateurs du backoffice |
| **Utilisateurs** | Membres de la bibliothèque |
| **Auteurs** | Auteurs des livres |
| **Categories** | Catégories de livres |
| **Livres** | Catalogue des livres |
| **LivreCategories** | Relation N:N Livres-Catégories |
| **Emprunts** | Historique des emprunts |
| **Reservations** | File d'attente des réservations |
| **Avis** | Notes et commentaires |
| **Notifications** | Système de notifications |
| **Historiques** | Audit trail des actions |

---

## Points Forts du Projet

1. **Clean Architecture** : Séparation claire des couches (Core, Infrastructure, Presentation)
2. **Sécurité** : BCrypt pour les mots de passe, authentification Cookie
3. **Double accès données** : EF Core (Backoffice) + ADO.NET (Frontoffice) - comme demandé
4. **UX Moderne** : Bootstrap 5, graphiques Chart.js
5. **Automatisation** : Détection des retards, notifications automatiques
6. **Import/Export** : CSV et PDF complets

---

## Auteur

Projet réalisé dans le cadre d'un examen Master en développement .NET Core.

## Licence

Ce projet est à but éducatif.
