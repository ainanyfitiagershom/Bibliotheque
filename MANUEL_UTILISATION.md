# Manuel d'utilisation - Bibliotheca

## Introduction

Bibliotheca est un systeme de gestion de bibliotheque compose de trois applications :

- **Frontoffice** (https://localhost:7005) - Interface pour les lecteurs
- **Backoffice** (https://localhost:7003) - Interface d'administration pour les bibliothecaires
- **API** (https://localhost:7001) - Services backend

---

## 1. Frontoffice - Interface Lecteur

### 1.1 Page d'accueil

La page d'accueil affiche :
- Les **nouveautes** : derniers livres ajoutes
- Les **livres populaires** : les plus empruntes
- Les **categories** : parcourir par genre

### 1.2 Catalogue de livres

**Acceder au catalogue** : Cliquez sur "Catalogue" dans le menu

**Rechercher un livre** :
- Utilisez la barre de recherche en haut de page
- Tapez un titre, auteur ou ISBN
- Les resultats s'affichent automatiquement

**Filtrer les livres** :
- Par categorie (Roman, Science-Fiction, Histoire...)
- Par disponibilite
- Trier par popularite ou date d'ajout

### 1.3 Details d'un livre

En cliquant sur un livre, vous verrez :
- Couverture et titre
- Auteur (cliquable pour voir ses autres livres)
- Description complete
- Disponibilite (badge vert = disponible, rose = indisponible)
- Notes et avis des lecteurs

### 1.4 Creer un compte

1. Cliquez sur **"S'inscrire"** en haut a droite
2. Remplissez le formulaire :
   - Nom et prenom
   - Email (servira d'identifiant)
   - Mot de passe (minimum 6 caracteres)
3. Cliquez sur "Creer mon compte"

### 1.5 Se connecter

1. Cliquez sur **"Connexion"**
2. Entrez votre email et mot de passe
3. Cliquez sur "Se connecter"

### 1.6 Mon espace personnel

Une fois connecte, cliquez sur votre avatar pour acceder a :

| Menu | Description |
|------|-------------|
| Mon espace | Vue d'ensemble du compte |
| Mes emprunts | Liste des emprunts en cours et passes |
| Reservations | Livres reserves en attente |
| Notifications | Alertes et rappels |
| Parametres | Modifier vos informations |

### 1.7 Emprunter un livre

**Important** : L'emprunt physique se fait uniquement sur place.

1. Recherchez le livre sur le site
2. Verifiez qu'il est disponible (badge vert)
3. Presentez-vous a la bibliotheque
4. Le bibliothecaire enregistre l'emprunt

**Conditions** :
- Duree : 21 jours par emprunt
- Maximum : 3 livres simultanement
- Retard : Penalites possibles

### 1.8 Reserver un livre indisponible

Si un livre est indisponible :
1. Connectez-vous a votre compte
2. Allez sur la page du livre
3. Cliquez sur **"Reserver ce livre"**
4. Vous serez notifie quand il sera disponible

---

## 2. Backoffice - Administration

### 2.1 Connexion administrateur

Acces : https://localhost:7003

Compte par defaut :
- Email : `admin@bibliotheca.fr`
- Mot de passe : `Admin123!`

### 2.2 Tableau de bord

Le dashboard affiche :
- Statistiques generales (livres, utilisateurs, emprunts)
- Emprunts en retard
- Reservations en attente
- Activite recente

### 2.3 Gestion des livres

**Menu** : Livres

| Action | Description |
|--------|-------------|
| Liste | Voir tous les livres avec recherche et filtres |
| Ajouter | Creer un nouveau livre |
| Modifier | Editer les informations d'un livre |
| Supprimer | Retirer un livre du catalogue |

**Champs d'un livre** :
- Titre, ISBN
- Auteur, Categorie
- Description
- Annee de publication
- Nombre de pages
- Stock total et disponible
- Image de couverture (URL)

### 2.4 Gestion des auteurs

**Menu** : Auteurs

- Ajouter/modifier/supprimer des auteurs
- Voir les livres de chaque auteur
- Biographie et nationalite

### 2.5 Gestion des categories

**Menu** : Categories

- Creer des categories (Roman, BD, Histoire...)
- Organiser le catalogue
- Associer des livres aux categories

### 2.6 Gestion des utilisateurs

**Menu** : Utilisateurs

| Action | Description |
|--------|-------------|
| Liste | Voir tous les membres |
| Ajouter | Inscrire un nouveau membre |
| Modifier | Changer les informations |
| Bloquer | Suspendre un compte |

**Informations utilisateur** :
- Nom, prenom, email
- Telephone, adresse
- Role (Lecteur, Bibliothecaire, Admin)
- Nombre d'emprunts max
- Statut (actif/bloque)

### 2.7 Gestion des emprunts

**Menu** : Emprunts

**Enregistrer un emprunt** :
1. Cliquez sur "Nouvel emprunt"
2. Selectionnez l'utilisateur
3. Selectionnez le(s) livre(s)
4. Validez

**Enregistrer un retour** :
1. Trouvez l'emprunt dans la liste
2. Cliquez sur "Retourner"
3. Le stock est automatiquement mis a jour

**Statuts des emprunts** :
- `EnCours` : Emprunt actif
- `EnRetard` : Date de retour depassee
- `Rendu` : Livre retourne

### 2.8 Gestion des reservations

**Menu** : Reservations

- Voir les reservations en attente
- Notifier les utilisateurs quand un livre est disponible
- Annuler une reservation

### 2.9 Statistiques

**Menu** : Statistiques

Rapports disponibles :
- Livres les plus empruntes
- Categories populaires
- Activite par periode
- Utilisateurs actifs

---

## 3. Architecture technique

### 3.1 Structure du projet

```
BibliothequeSolution/
├── Bibliotheque.Core/        # Entites, DTOs, Interfaces
├── Bibliotheque.Infrastructure/  # Repositories, Services, DbContext
├── Bibliotheque.Api/         # API REST
├── Backoffice.Razor/         # Administration (Razor Pages)
└── Frontoffice.MVC/          # Site public (MVC)
```

### 3.2 Base de donnees

- **SQL Server** sur port 1433
- **Connection string** : voir `appsettings.json`

### 3.3 Lancer l'application

```bash
# Terminal 1 - API
cd Bibliotheque.Api
dotnet run --urls "https://localhost:7001;http://localhost:7000"

# Terminal 2 - Backoffice
cd Backoffice.Razor
dotnet run --urls "https://localhost:7003;http://localhost:7002"

# Terminal 3 - Frontoffice
cd Frontoffice.MVC
dotnet run --urls "https://localhost:7005;http://localhost:7004"
```

### 3.4 Ports utilises

| Application | HTTP | HTTPS |
|-------------|------|-------|
| API | 7000 | 7001 |
| Backoffice | 7002 | 7003 |
| Frontoffice | 7004 | 7005 |

---

## 4. Aide et support

### Contact

- Email : contact@bibliotheca.fr
- Telephone : +33 1 23 45 67 89
- Adresse : 123 Rue des Livres, 75001 Paris

### En cas de probleme

1. Verifiez que SQL Server est demarre
2. Verifiez que l'API est accessible (https://localhost:7001/swagger)
3. Consultez les logs dans la console

---

*Documentation Bibliotheca - Systeme de gestion de bibliotheque*
