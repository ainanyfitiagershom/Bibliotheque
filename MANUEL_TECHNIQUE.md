# Manuel Technique - Bibliotheca
## Guide Complet pour Comprendre et Maîtriser le Projet

---

## Table des Matières

1. [Vue d'Ensemble de l'Architecture](#1-vue-densemble-de-larchitecture)
2. [Structure des Projets](#2-structure-des-projets)
3. [La Couche Core (Domaine)](#3-la-couche-core-domaine)
4. [La Couche Infrastructure](#4-la-couche-infrastructure)
5. [Le Pattern Repository](#5-le-pattern-repository)
6. [Le Pattern Unit of Work](#6-le-pattern-unit-of-work)
7. [Les Services Métier](#7-les-services-métier)
8. [Le Backoffice (Razor Pages)](#8-le-backoffice-razor-pages)
9. [Le Frontoffice (MVC)](#9-le-frontoffice-mvc)
10. [L'API REST](#10-lapi-rest)
11. [Entity Framework vs ADO.NET](#11-entity-framework-vs-adonet)
12. [L'Authentification](#12-lauthentification)
13. [La Pagination](#13-la-pagination)
14. [La Base de Données](#14-la-base-de-données)
15. [L'Injection de Dépendances](#15-linjection-de-dépendances)
16. [Les Bonnes Pratiques Utilisées](#16-les-bonnes-pratiques-utilisées)

---

## 1. Vue d'Ensemble de l'Architecture

### Pourquoi cette architecture ?

Ce projet utilise une **architecture en couches (Layered Architecture)** qui sépare les responsabilités :

```
┌─────────────────────────────────────────────────────────┐
│                  COUCHE PRÉSENTATION                    │
│  ┌─────────────────┐  ┌─────────────────┐  ┌─────────┐ │
│  │ Backoffice.Razor│  │ Frontoffice.MVC │  │   API   │ │
│  │  (Admin Panel)  │  │ (Site Public)   │  │  REST   │ │
│  └────────┬────────┘  └────────┬────────┘  └────┬────┘ │
└───────────┼─────────────────────┼───────────────┼──────┘
            │                     │               │
┌───────────▼─────────────────────▼───────────────▼──────┐
│                  COUCHE LOGIQUE MÉTIER                  │
│         (Services : EmpruntService, AuthService, etc.)  │
└───────────────────────────┬─────────────────────────────┘
                            │
┌───────────────────────────▼─────────────────────────────┐
│                  COUCHE ACCÈS AUX DONNÉES               │
│     (UnitOfWork, Repositories, BibliothequeDbContext)   │
└───────────────────────────┬─────────────────────────────┘
                            │
┌───────────────────────────▼─────────────────────────────┐
│                  BASE DE DONNÉES                        │
│                    SQL Server                           │
└─────────────────────────────────────────────────────────┘
```

**Avantages de cette approche :**
- **Séparation des préoccupations** : Chaque couche a une responsabilité unique
- **Testabilité** : On peut tester chaque couche indépendamment
- **Maintenabilité** : Modifier une couche n'impacte pas les autres
- **Réutilisabilité** : La logique métier peut être partagée entre plusieurs interfaces

---

## 2. Structure des Projets

### 2.1 Les 5 Projets de la Solution

```
BibliothequeSolution/
├── Bibliotheque.Core/           # Entités, DTOs, Interfaces (le "contrat")
├── Bibliotheque.Infrastructure/ # Implémentations (repositories, services, DbContext)
├── Bibliotheque.Api/            # API REST (pour intégrations externes)
├── Backoffice.Razor/            # Panel Admin (Razor Pages)
└── Frontoffice.MVC/             # Site Public (MVC + ADO.NET)
```

### 2.2 Dépendances entre Projets

```
Bibliotheque.Core          ← Ne dépend de rien (couche la plus pure)
        ↑
Bibliotheque.Infrastructure ← Dépend de Core
        ↑
┌───────┴───────┐
│   │   │       │
API  Backoffice  Frontoffice  ← Dépendent de Core + Infrastructure
```

**Pourquoi Core ne dépend de rien ?**
- C'est le **domaine métier** pur
- Il définit les **contrats** (interfaces) sans savoir comment ils seront implémentés
- Permet de changer l'implémentation (ex: changer de base de données) sans toucher au domaine

---

## 3. La Couche Core (Domaine)

### 3.1 Les Entités

Une **entité** représente un objet métier qui a une identité (un ID).

**Exemple : Livre.cs**
```csharp
[Table("Livres")]                    // Nom de la table en BDD
public class Livre
{
    [Key]                            // Clé primaire
    public int IdLivre { get; set; }

    [Required]                       // Champ obligatoire
    [StringLength(255)]              // Validation de longueur
    public string Titre { get; set; }

    public int IdAuteur { get; set; } // Clé étrangère

    // Navigation : permet d'accéder à l'auteur depuis un livre
    [ForeignKey("IdAuteur")]
    public virtual Auteur? Auteur { get; set; }

    // Collection : un livre a plusieurs emprunts
    public virtual ICollection<Emprunt> Emprunts { get; set; }

    // Propriété calculée (pas en BDD)
    [NotMapped]
    public bool EstDisponible => StockDisponible > 0;
}
```

**Concepts clés :**

| Attribut | Rôle |
|----------|------|
| `[Key]` | Définit la clé primaire |
| `[Required]` | Champ obligatoire (validation) |
| `[StringLength(n)]` | Limite de caractères |
| `[ForeignKey]` | Relation avec une autre table |
| `[NotMapped]` | Propriété qui n'est pas stockée en BDD |
| `virtual` | Permet le **lazy loading** (chargement à la demande) |

### 3.2 Les DTOs (Data Transfer Objects)

Un **DTO** est un objet simple pour transférer des données entre couches.

**Pourquoi utiliser des DTOs ?**
- Ne pas exposer toutes les propriétés de l'entité
- Combiner des données de plusieurs entités
- Éviter les problèmes de sérialisation (boucles infinies)

```csharp
public class LivreDTO
{
    public int IdLivre { get; set; }
    public string Titre { get; set; }
    public string? NomAuteur { get; set; }  // Combinaison Prenom + Nom
    public int StockDisponible { get; set; }
    // Pas de navigation, pas de collections
}
```

### 3.3 Les Interfaces

Une **interface** définit un contrat : "Voici ce que tu dois savoir faire".

```csharp
public interface ILivreRepository : IRepository<Livre>
{
    Task<Livre?> GetByIdWithDetailsAsync(int id);
    Task<IEnumerable<Livre>> GetDisponiblesAsync();
    Task<bool> IsbnExisteAsync(string isbn, int? excludeId = null);
}
```

**Pourquoi des interfaces ?**
- **Abstraction** : Le code utilise l'interface, pas l'implémentation
- **Injection de dépendances** : On peut injecter différentes implémentations
- **Tests** : On peut créer des "mocks" pour les tests unitaires

---

## 4. La Couche Infrastructure

### 4.1 Le DbContext (Entity Framework Core)

Le `DbContext` est le pont entre le code C# et la base de données.

```csharp
public class BibliothequeDbContext : DbContext
{
    // Chaque DbSet représente une table
    public DbSet<Livre> Livres { get; set; }
    public DbSet<Auteur> Auteurs { get; set; }
    public DbSet<Emprunt> Emprunts { get; set; }
    // ...

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Configuration des relations
        modelBuilder.Entity<LivreCategorie>()
            .HasKey(lc => new { lc.IdLivre, lc.IdCategorie }); // Clé composite

        modelBuilder.Entity<Livre>()
            .HasOne(l => l.Auteur)          // Un livre a UN auteur
            .WithMany(a => a.Livres)        // Un auteur a PLUSIEURS livres
            .HasForeignKey(l => l.IdAuteur)
            .OnDelete(DeleteBehavior.Restrict); // Ne pas supprimer l'auteur si livres existent
    }
}
```

**Comportements de suppression :**
| DeleteBehavior | Effet |
|----------------|-------|
| `Cascade` | Supprime les enfants automatiquement |
| `Restrict` | Empêche la suppression si enfants existent |
| `SetNull` | Met la FK à null |

### 4.2 Configuration des Index

```csharp
// Index unique sur ISBN (mais nullable)
modelBuilder.Entity<Livre>()
    .HasIndex(l => l.ISBN)
    .IsUnique()
    .HasFilter("[ISBN] IS NOT NULL");

// Index simple pour accélérer les recherches
modelBuilder.Entity<Livre>()
    .HasIndex(l => l.Titre);
```

---

## 5. Le Pattern Repository

### 5.1 Qu'est-ce qu'un Repository ?

Un **Repository** encapsule la logique d'accès aux données. Il cache la complexité de EF Core.

### 5.2 Le Repository Générique

```csharp
public class Repository<T> : IRepository<T> where T : class
{
    protected readonly BibliothequeDbContext _context;
    protected readonly DbSet<T> _dbSet;

    public Repository(BibliothequeDbContext context)
    {
        _context = context;
        _dbSet = context.Set<T>(); // Récupère le DbSet correspondant au type T
    }

    // CRUD de base
    public virtual async Task<T?> GetByIdAsync(int id)
    {
        return await _dbSet.FindAsync(id);
    }

    public virtual async Task<IEnumerable<T>> GetAllAsync()
    {
        return await _dbSet.ToListAsync();
    }

    public virtual async Task<T> AddAsync(T entity)
    {
        await _dbSet.AddAsync(entity);
        return entity;
    }

    public virtual Task UpdateAsync(T entity)
    {
        _dbSet.Update(entity);
        return Task.CompletedTask;
    }

    public virtual Task DeleteAsync(T entity)
    {
        _dbSet.Remove(entity);
        return Task.CompletedTask;
    }

    // Recherche avec expression lambda
    public virtual async Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate)
    {
        return await _dbSet.Where(predicate).ToListAsync();
    }
}
```

**Avantages du Repository Générique :**
- Évite la duplication de code
- Toutes les entités ont les mêmes opérations de base
- On peut étendre pour des cas spécifiques

### 5.3 Repository Spécialisé

```csharp
public class LivreRepository : Repository<Livre>, ILivreRepository
{
    public LivreRepository(BibliothequeDbContext context) : base(context) { }

    // Méthode spécifique aux livres
    public async Task<Livre?> GetByIdWithDetailsAsync(int id)
    {
        return await _dbSet
            .Include(l => l.Auteur)              // Charge l'auteur
            .Include(l => l.LivreCategories)     // Charge les catégories
                .ThenInclude(lc => lc.Categorie) // Puis les détails des catégories
            .FirstOrDefaultAsync(l => l.IdLivre == id);
    }
}
```

**Include vs ThenInclude :**
- `Include()` : Charge une navigation directe
- `ThenInclude()` : Charge une navigation à partir de la précédente (navigation imbriquée)

---

## 6. Le Pattern Unit of Work

### 6.1 Pourquoi Unit of Work ?

Le **Unit of Work** coordonne plusieurs repositories et gère les transactions.

**Problème sans Unit of Work :**
```csharp
// ❌ Chaque repository a son propre SaveChanges
await livreRepository.AddAsync(livre);
await livreRepository.SaveChangesAsync();

await empruntRepository.AddAsync(emprunt);
await empruntRepository.SaveChangesAsync(); // Si ça échoue, le livre est déjà ajouté !
```

**Avec Unit of Work :**
```csharp
// ✅ Un seul SaveChanges pour tout
await _unitOfWork.Livres.AddAsync(livre);
await _unitOfWork.Emprunts.AddAsync(emprunt);
await _unitOfWork.SaveChangesAsync(); // Tout ou rien !
```

### 6.2 Implémentation

```csharp
public class UnitOfWork : IUnitOfWork
{
    private readonly BibliothequeDbContext _context;
    private IDbContextTransaction? _transaction;

    // Repositories (chargement paresseux)
    private ILivreRepository? _livres;
    private IEmpruntRepository? _emprunts;

    public ILivreRepository Livres =>
        _livres ??= new LivreRepository(_context);
    // ??= signifie : si null, créer et assigner

    public IEmpruntRepository Emprunts =>
        _emprunts ??= new EmpruntRepository(_context);

    // Sauvegarde toutes les modifications
    public async Task<int> SaveChangesAsync()
    {
        return await _context.SaveChangesAsync();
    }

    // Gestion des transactions
    public async Task BeginTransactionAsync()
    {
        _transaction = await _context.Database.BeginTransactionAsync();
    }

    public async Task CommitTransactionAsync()
    {
        if (_transaction != null)
        {
            await _transaction.CommitAsync();
            await _transaction.DisposeAsync();
            _transaction = null;
        }
    }

    public async Task RollbackTransactionAsync()
    {
        if (_transaction != null)
        {
            await _transaction.RollbackAsync();
            await _transaction.DisposeAsync();
            _transaction = null;
        }
    }
}
```

### 6.3 Utilisation des Transactions

```csharp
public async Task<(bool, string)> EffectuerEmpruntAsync(int idLivre, int idUtilisateur)
{
    await _unitOfWork.BeginTransactionAsync();  // Démarre la transaction

    try
    {
        // Toutes les opérations
        var livre = await _unitOfWork.Livres.GetByIdAsync(idLivre);
        livre.StockDisponible -= 1;
        await _unitOfWork.Livres.UpdateAsync(livre);

        var emprunt = new Emprunt { ... };
        await _unitOfWork.Emprunts.AddAsync(emprunt);

        await _unitOfWork.SaveChangesAsync();
        await _unitOfWork.CommitTransactionAsync();  // ✅ Tout réussi

        return (true, "Succès");
    }
    catch (Exception ex)
    {
        await _unitOfWork.RollbackTransactionAsync();  // ❌ Annule tout
        return (false, ex.Message);
    }
}
```

---

## 7. Les Services Métier

### 7.1 Rôle des Services

Les **services** contiennent la **logique métier** complexe qui implique plusieurs entités.

**Différence Repository vs Service :**
| Repository | Service |
|------------|---------|
| CRUD simple sur UNE entité | Opérations complexes sur PLUSIEURS entités |
| `GetById`, `Add`, `Update`, `Delete` | `EffectuerEmprunt`, `CalculerPenalite` |
| Pas de logique métier | Contient les règles métier |

### 7.2 Exemple : EmpruntService

```csharp
public class EmpruntService : IEmpruntService
{
    private readonly IUnitOfWork _unitOfWork;
    private const decimal PENALITE_PAR_JOUR = 0.50m;

    public async Task<(bool Succes, string Message, int? IdEmprunt)> EffectuerEmpruntAsync(
        int idLivre, int idUtilisateur, int dureeJours = 14)
    {
        await _unitOfWork.BeginTransactionAsync();

        try
        {
            // 1. Vérifications métier
            var livre = await _unitOfWork.Livres.GetByIdAsync(idLivre);
            if (livre == null)
                return (false, "Livre non trouvé.", null);

            if (livre.StockDisponible <= 0)
                return (false, "Ce livre n'est plus disponible.", null);

            var utilisateur = await _unitOfWork.Utilisateurs.GetByIdAsync(idUtilisateur);
            if (utilisateur.EstBloque)
                return (false, "Votre compte est bloqué.", null);

            // Vérifier la limite d'emprunts
            var empruntsEnCours = await _unitOfWork.Emprunts
                .CompterEmpruntsEnCoursAsync(idUtilisateur);
            if (empruntsEnCours >= utilisateur.NombreEmpruntsMax)
                return (false, "Limite d'emprunts atteinte.", null);

            // 2. Créer l'emprunt
            var emprunt = new Emprunt
            {
                IdLivre = idLivre,
                IdUtilisateur = idUtilisateur,
                DateEmprunt = DateTime.Now,
                DateRetourPrevue = DateTime.Now.AddDays(dureeJours),
                Statut = "EnCours"
            };
            await _unitOfWork.Emprunts.AddAsync(emprunt);

            // 3. Mettre à jour le stock
            livre.StockDisponible -= 1;
            livre.NombreEmprunts += 1;
            await _unitOfWork.Livres.UpdateAsync(livre);

            // 4. Convertir une éventuelle réservation
            var reservation = await _unitOfWork.Reservations
                .FirstOrDefaultAsync(r => r.IdLivre == idLivre &&
                    r.IdUtilisateur == idUtilisateur &&
                    r.Statut == "Disponible");
            if (reservation != null)
            {
                reservation.Statut = "Convertie";
                await _unitOfWork.Reservations.UpdateAsync(reservation);
            }

            await _unitOfWork.SaveChangesAsync();
            await _unitOfWork.CommitTransactionAsync();

            return (true, "Emprunt effectué.", emprunt.IdEmprunt);
        }
        catch (Exception ex)
        {
            await _unitOfWork.RollbackTransactionAsync();
            return (false, $"Erreur : {ex.Message}", null);
        }
    }
}
```

### 7.3 Logique de Retour avec Pénalités

```csharp
public async Task<(bool, string, decimal)> EffectuerRetourAsync(int idEmprunt)
{
    // ... validation ...

    // Calcul de la pénalité
    decimal penalite = 0;
    var joursRetard = (DateTime.Now.Date - emprunt.DateRetourPrevue.Date).Days;
    if (joursRetard > 0)
    {
        penalite = joursRetard * PENALITE_PAR_JOUR; // 0.50€ par jour
    }

    emprunt.DateRetourEffective = DateTime.Now;
    emprunt.Statut = "Termine";
    emprunt.Penalite = penalite;

    // Remettre le livre en stock
    livre.StockDisponible += 1;

    // Notifier le prochain dans la file d'attente
    var prochaineReservation = await _unitOfWork.Reservations
        .GetProchaineEnAttenteAsync(idLivre);
    if (prochaineReservation != null)
    {
        prochaineReservation.Statut = "Disponible";
        prochaineReservation.DateExpiration = DateTime.Now.AddDays(3);
        // Créer notification...
    }

    // ...
}
```

---

## 8. Le Backoffice (Razor Pages)

### 8.1 Qu'est-ce que Razor Pages ?

**Razor Pages** est un modèle de programmation où chaque page est autonome :
- Un fichier `.cshtml` (la vue HTML)
- Un fichier `.cshtml.cs` (la logique = PageModel)

### 8.2 Structure d'une Page

**Index.cshtml** (Vue) :
```html
@page
@model Backoffice.Razor.Pages.Livres.IndexModel

<h1>Liste des livres</h1>

@foreach (var livre in Model.Livres)
{
    <tr>
        <td>@livre.Titre</td>
        <td>@livre.Auteur?.NomComplet</td>
    </tr>
}
```

**Index.cshtml.cs** (Logique) :
```csharp
public class IndexModel : PageModel
{
    private readonly IUnitOfWork _unitOfWork;

    public IndexModel(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;  // Injection de dépendance
    }

    public List<Livre> Livres { get; set; } = new();

    // Appelé quand on accède à la page (GET)
    public async Task OnGetAsync()
    {
        var livres = await _unitOfWork.Livres.GetAllWithDetailsAsync();
        Livres = livres.Where(l => l.Actif).ToList();
    }
}
```

### 8.3 Gestion des Formulaires

```csharp
public class CreateModel : PageModel
{
    [BindProperty]  // Lie automatiquement les données du formulaire
    public Livre Livre { get; set; } = new();

    // Appelé quand on soumet le formulaire (POST)
    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            return Page();  // Réaffiche la page avec erreurs
        }

        await _unitOfWork.Livres.AddAsync(Livre);
        await _unitOfWork.SaveChangesAsync();

        TempData["Success"] = "Livre créé !";  // Message flash
        return RedirectToPage("Index");
    }
}
```

### 8.4 Handlers (Plusieurs Actions par Page)

```csharp
public class IndexModel : PageModel
{
    // POST vers ?handler=Supprimer
    public async Task<IActionResult> OnPostSupprimerAsync(int id)
    {
        var livre = await _unitOfWork.Livres.GetByIdAsync(id);
        livre.Actif = false;  // Suppression logique
        await _unitOfWork.SaveChangesAsync();
        return RedirectToPage();
    }

    // POST vers ?handler=Notifier
    public async Task<IActionResult> OnPostNotifierAsync(int id)
    {
        // ...
    }
}
```

**Dans la vue :**
```html
<form method="post" asp-page-handler="Supprimer">
    <input type="hidden" name="id" value="@livre.IdLivre" />
    <button type="submit">Supprimer</button>
</form>
```

### 8.5 Binding des Paramètres de Requête

```csharp
// Ces propriétés sont automatiquement remplies depuis l'URL
// Ex: /Livres/Index?search=Harry&currentPage=2

[BindProperty(SupportsGet = true)]
public string? Search { get; set; }

[BindProperty(SupportsGet = true)]
public int CurrentPage { get; set; } = 1;
```

---

## 9. Le Frontoffice (MVC)

### 9.1 Différence avec Razor Pages

| Razor Pages | MVC |
|-------------|-----|
| 1 fichier = 1 page | Controllers + Views séparés |
| PageModel | Controller + ViewModel |
| Simple pour CRUD | Meilleur pour applications complexes |
| Utilisé pour le Backoffice | Utilisé pour le Frontoffice |

### 9.2 Structure MVC

**Controller :**
```csharp
public class LivresController : Controller
{
    private readonly ILivreService _livreService;

    public LivresController(ILivreService livreService)
    {
        _livreService = livreService;
    }

    // GET /Livres
    public async Task<IActionResult> Index(string? search, int page = 1)
    {
        var result = await _livreService.RechercherAsync(search, null, page, 12);
        return View(result);  // Passe les données à la vue
    }

    // GET /Livres/Details/5
    public async Task<IActionResult> Details(int id)
    {
        var livre = await _livreService.GetByIdAsync(id);
        if (livre == null)
            return NotFound();
        return View(livre);
    }
}
```

**Vue (Views/Livres/Index.cshtml) :**
```html
@model PagedResultDTO<LivreDTO>

@foreach (var livre in Model.Items)
{
    <div class="card">
        <h3>@livre.Titre</h3>
        <a asp-action="Details" asp-route-id="@livre.IdLivre">Voir</a>
    </div>
}
```

---

## 10. L'API REST

### 10.1 Structure d'un Controller API

```csharp
[Route("api/[controller]")]
[ApiController]
public class LivresController : ControllerBase
{
    private readonly IUnitOfWork _unitOfWork;

    // GET api/livres
    [HttpGet]
    public async Task<ActionResult<IEnumerable<LivreDTO>>> GetAll()
    {
        var livres = await _unitOfWork.Livres.GetAllWithDetailsAsync();
        return Ok(livres.Select(l => new LivreDTO { ... }));
    }

    // GET api/livres/5
    [HttpGet("{id}")]
    public async Task<ActionResult<LivreDTO>> GetById(int id)
    {
        var livre = await _unitOfWork.Livres.GetByIdAsync(id);
        if (livre == null)
            return NotFound();
        return Ok(new LivreDTO { ... });
    }

    // POST api/livres
    [HttpPost]
    public async Task<ActionResult<Livre>> Create(Livre livre)
    {
        await _unitOfWork.Livres.AddAsync(livre);
        await _unitOfWork.SaveChangesAsync();
        return CreatedAtAction(nameof(GetById), new { id = livre.IdLivre }, livre);
    }

    // PUT api/livres/5
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, Livre livre)
    {
        if (id != livre.IdLivre)
            return BadRequest();
        await _unitOfWork.Livres.UpdateAsync(livre);
        await _unitOfWork.SaveChangesAsync();
        return NoContent();
    }

    // DELETE api/livres/5
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var livre = await _unitOfWork.Livres.GetByIdAsync(id);
        if (livre == null)
            return NotFound();
        await _unitOfWork.Livres.DeleteAsync(livre);
        await _unitOfWork.SaveChangesAsync();
        return NoContent();
    }
}
```

### 10.2 Codes HTTP Standards

| Code | Signification | Quand l'utiliser |
|------|--------------|------------------|
| 200 OK | Succès | GET réussi |
| 201 Created | Créé | POST réussi |
| 204 No Content | Succès sans contenu | PUT/DELETE réussi |
| 400 Bad Request | Requête invalide | Validation échouée |
| 401 Unauthorized | Non authentifié | Token manquant |
| 403 Forbidden | Non autorisé | Pas les droits |
| 404 Not Found | Non trouvé | Ressource inexistante |

---

## 11. Entity Framework vs ADO.NET

### 11.1 Pourquoi Deux Approches ?

| Critère | Entity Framework | ADO.NET |
|---------|------------------|---------|
| Facilité | Plus simple | Plus complexe |
| Performance | Bon pour la plupart des cas | Meilleur pour requêtes complexes |
| Contrôle | Abstrait le SQL | SQL direct |
| Utilisé dans | Backoffice, API | Frontoffice |

### 11.2 Entity Framework (Backoffice)

```csharp
// Simple et lisible
var livres = await _context.Livres
    .Include(l => l.Auteur)
    .Where(l => l.StockDisponible > 0)
    .OrderBy(l => l.Titre)
    .ToListAsync();
```

### 11.3 ADO.NET (Frontoffice)

```csharp
public async Task<LivreDTO?> GetByIdAsync(int id)
{
    using var connection = _context.CreateConnection();
    await connection.OpenAsync();

    var sql = @"
        SELECT l.*, a.Nom AS NomAuteur
        FROM Livres l
        LEFT JOIN Auteurs a ON l.IdAuteur = a.IdAuteur
        WHERE l.IdLivre = @Id";

    using var cmd = new SqlCommand(sql, connection);
    cmd.Parameters.AddWithValue("@Id", id);

    using var reader = await cmd.ExecuteReaderAsync();

    if (await reader.ReadAsync())
    {
        return new LivreDTO
        {
            IdLivre = reader.GetInt32("IdLivre"),
            Titre = reader.GetString("Titre"),
            // ... mapping manuel
        };
    }
    return null;
}
```

### 11.4 Procédures Stockées

Le Frontoffice utilise des procédures stockées pour les recherches complexes :

```csharp
using var cmd = new SqlCommand("sp_RechercherLivres", connection);
cmd.CommandType = CommandType.StoredProcedure;
cmd.Parameters.AddWithValue("@Recherche", search ?? (object)DBNull.Value);
cmd.Parameters.AddWithValue("@Page", page);
cmd.Parameters.AddWithValue("@PageSize", pageSize);
```

**Avantages des procédures stockées :**
- Optimisées par SQL Server
- Sécurisées contre l'injection SQL
- Plan d'exécution mis en cache

---

## 12. L'Authentification

### 12.1 Cookie Authentication

```csharp
// Dans Program.cs
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Admin/Login";      // Page de connexion
        options.LogoutPath = "/Admin/Logout";    // Page de déconnexion
        options.AccessDeniedPath = "/Admin/AccessDenied";
        options.ExpireTimeSpan = TimeSpan.FromHours(8);  // Durée du cookie
        options.SlidingExpiration = true;  // Renouvelle si activité
    });
```

### 12.2 Processus de Connexion

```csharp
public async Task<IActionResult> OnPostAsync()
{
    // 1. Vérifier les credentials
    var (succes, admin, message) = await _authService.AuthentifierAdminAsync(
        Input.Email, Input.MotDePasse);

    if (!succes)
    {
        ErrorMessage = message;
        return Page();
    }

    // 2. Créer les Claims (informations sur l'utilisateur)
    var claims = new List<Claim>
    {
        new Claim(ClaimTypes.NameIdentifier, admin.IdAdmin.ToString()),
        new Claim(ClaimTypes.Name, admin.NomComplet),
        new Claim(ClaimTypes.Email, admin.Email),
        new Claim(ClaimTypes.Role, "Admin")
    };

    // 3. Créer le cookie d'authentification
    var claimsIdentity = new ClaimsIdentity(claims,
        CookieAuthenticationDefaults.AuthenticationScheme);

    await HttpContext.SignInAsync(
        CookieAuthenticationDefaults.AuthenticationScheme,
        new ClaimsPrincipal(claimsIdentity),
        new AuthenticationProperties { IsPersistent = true });

    return RedirectToPage("/Index");
}
```

### 12.3 Protection des Pages

```csharp
[Authorize]  // Requiert une authentification
public class IndexModel : PageModel
{
    // Cette page n'est accessible qu'aux utilisateurs connectés
}
```

### 12.4 Hashage des Mots de Passe (BCrypt)

```csharp
// Lors de l'inscription
string hash = BCrypt.Net.BCrypt.HashPassword(motDePasse);

// Lors de la connexion
bool valide = BCrypt.Net.BCrypt.Verify(motDePasse, admin.MotDePasseHash);
```

**Pourquoi BCrypt ?**
- Algorithme lent intentionnellement (protection contre brute force)
- Salt automatique (deux mêmes mots de passe = deux hashs différents)
- Sécurisé et éprouvé

---

## 13. La Pagination

### 13.1 Pourquoi Paginer ?

- Éviter de charger 10 000 livres en mémoire
- Améliorer les performances
- Meilleure expérience utilisateur

### 13.2 Implémentation Backend

```csharp
public class IndexModel : PageModel
{
    private const int PageSize = 15;

    public List<Livre> Livres { get; set; } = new();
    public int TotalItems { get; set; }
    public int TotalPages { get; set; }

    [BindProperty(SupportsGet = true)]
    public int CurrentPage { get; set; } = 1;

    [BindProperty(SupportsGet = true)]
    public string? Search { get; set; }

    public async Task OnGetAsync()
    {
        var query = await _unitOfWork.Livres.GetAllAsync();

        // Filtrer si recherche
        if (!string.IsNullOrEmpty(Search))
        {
            query = query.Where(l => l.Titre.Contains(Search));
        }

        TotalItems = query.Count();
        TotalPages = (int)Math.Ceiling(TotalItems / (double)PageSize);

        // Récupérer seulement la page demandée
        Livres = query
            .Skip((CurrentPage - 1) * PageSize)  // Sauter les pages précédentes
            .Take(PageSize)                       // Prendre PageSize éléments
            .ToList();
    }
}
```

### 13.3 Implémentation Frontend

```html
@if (Model.TotalPages > 1)
{
    <nav>
        <ul class="pagination">
            <!-- Bouton Précédent -->
            <li class="page-item @(Model.CurrentPage == 1 ? "disabled" : "")">
                <a class="page-link"
                   href="?currentPage=@(Model.CurrentPage - 1)@(string.IsNullOrEmpty(Model.Search) ? "" : $"&search={Uri.EscapeDataString(Model.Search)}")">
                    Précédent
                </a>
            </li>

            <!-- Numéros de pages -->
            @for (int i = 1; i <= Model.TotalPages; i++)
            {
                <li class="page-item @(i == Model.CurrentPage ? "active" : "")">
                    <a class="page-link"
                       href="?currentPage=@i@(string.IsNullOrEmpty(Model.Search) ? "" : $"&search={Uri.EscapeDataString(Model.Search)}")">
                        @i
                    </a>
                </li>
            }

            <!-- Bouton Suivant -->
            <li class="page-item @(Model.CurrentPage == Model.TotalPages ? "disabled" : "")">
                <a class="page-link"
                   href="?currentPage=@(Model.CurrentPage + 1)...">
                    Suivant
                </a>
            </li>
        </ul>
    </nav>
}
```

### 13.4 Formule de Pagination

```
Skip = (CurrentPage - 1) * PageSize
Take = PageSize

Exemple avec PageSize = 15 :
- Page 1 : Skip 0, Take 15 → éléments 1-15
- Page 2 : Skip 15, Take 15 → éléments 16-30
- Page 3 : Skip 30, Take 15 → éléments 31-45
```

---

## 14. La Base de Données

### 14.1 Schéma Simplifié

```
┌─────────────┐     ┌─────────────┐     ┌──────────────┐
│   Auteurs   │────<│   Livres    │>────│  Categories  │
└─────────────┘     └──────┬──────┘     └──────────────┘
                           │                    ↑
                           │            ┌───────┴───────┐
                    ┌──────┴──────┐     │LivreCategories│
                    │             │     └───────────────┘
              ┌─────┴─────┐ ┌─────┴─────┐
              │  Emprunts │ │Reservations│
              └─────┬─────┘ └─────┬─────┘
                    │             │
              ┌─────┴─────────────┴─────┐
              │      Utilisateurs       │
              └────────────┬────────────┘
                           │
                    ┌──────┴──────┐
                    │Notifications│
                    └─────────────┘
```

### 14.2 Relations Principales

| Relation | Type | Description |
|----------|------|-------------|
| Auteur → Livres | 1:N | Un auteur écrit plusieurs livres |
| Livre ↔ Categories | N:N | Via LivreCategories |
| Utilisateur → Emprunts | 1:N | Un utilisateur fait plusieurs emprunts |
| Livre → Emprunts | 1:N | Un livre est emprunté plusieurs fois |
| Utilisateur → Reservations | 1:N | File d'attente |

### 14.3 Scripts SQL Importants

**01_CreateDatabase.sql** : Crée les tables
**02_StoredProcedures.sql** : Procédures stockées
**03_SeedData.sql** : Données de test
**05_sp_RechercherLivres.sql** : Recherche avancée

---

## 15. L'Injection de Dépendances

### 15.1 Qu'est-ce que c'est ?

L'**injection de dépendances (DI)** consiste à fournir les objets dont une classe a besoin plutôt qu'elle les crée elle-même.

**Sans DI (mauvais) :**
```csharp
public class LivresController
{
    public LivresController()
    {
        // ❌ La classe crée elle-même ses dépendances
        var context = new BibliothequeDbContext(...);
        var unitOfWork = new UnitOfWork(context);
    }
}
```

**Avec DI (bien) :**
```csharp
public class LivresController
{
    private readonly IUnitOfWork _unitOfWork;

    public LivresController(IUnitOfWork unitOfWork)
    {
        // ✅ La dépendance est injectée par le conteneur
        _unitOfWork = unitOfWork;
    }
}
```

### 15.2 Configuration dans Program.cs

```csharp
// Enregistrement des services
builder.Services.AddDbContext<BibliothequeDbContext>(options =>
    options.UseSqlServer(connectionString));

// Scoped = une instance par requête HTTP
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
builder.Services.AddScoped<IEmpruntService, EmpruntService>();
builder.Services.AddScoped<IAuthService, AuthService>();

// Singleton = une seule instance pour toute l'application
builder.Services.AddHostedService<NotificationBackgroundService>();
```

### 15.3 Durées de Vie

| Durée | Description | Exemple |
|-------|-------------|---------|
| Transient | Nouvelle instance à chaque demande | Services légers |
| Scoped | Une instance par requête HTTP | Repositories, UnitOfWork |
| Singleton | Une instance pour toute l'application | Services de configuration |

---

## 16. Les Bonnes Pratiques Utilisées

### 16.1 Suppression Logique

```csharp
// ❌ Suppression physique
await _unitOfWork.Livres.DeleteAsync(livre);

// ✅ Suppression logique (on garde l'historique)
livre.Actif = false;
await _unitOfWork.Livres.UpdateAsync(livre);
```

### 16.2 Validation avec Data Annotations

```csharp
[Required(ErrorMessage = "Le titre est obligatoire")]
[StringLength(255, ErrorMessage = "Le titre ne peut pas dépasser 255 caractères")]
public string Titre { get; set; }

[Range(1, 5, ErrorMessage = "La note doit être entre 1 et 5")]
public int Note { get; set; }

[EmailAddress(ErrorMessage = "Email invalide")]
public string Email { get; set; }
```

### 16.3 Messages Flash avec TempData

```csharp
// Dans le PageModel
TempData["Success"] = "Livre créé avec succès !";
TempData["Error"] = "Une erreur s'est produite.";

// Dans la vue
@if (TempData["Success"] != null)
{
    <div class="alert alert-success">@TempData["Success"]</div>
}
```

### 16.4 Async/Await Partout

```csharp
// ✅ Toutes les opérations I/O sont async
public async Task<IActionResult> OnGetAsync()
{
    var livres = await _unitOfWork.Livres.GetAllAsync();
    return Page();
}
```

### 16.5 Retour de Tuple pour Résultats Multiples

```csharp
// Retourne succès/échec + message + données
public async Task<(bool Succes, string Message, int? Id)> CreerEmpruntAsync(...)
{
    if (erreur)
        return (false, "Livre non disponible", null);

    return (true, "Succès", emprunt.Id);
}

// Utilisation
var (succes, message, id) = await _service.CreerEmpruntAsync(...);
if (!succes)
{
    TempData["Error"] = message;
}
```

---

## Conclusion

Ce projet utilise une architecture propre et des patterns éprouvés :

1. **Architecture en couches** pour séparer les responsabilités
2. **Repository Pattern** pour abstraire l'accès aux données
3. **Unit of Work** pour gérer les transactions
4. **Services métier** pour la logique complexe
5. **Injection de dépendances** pour le découplage
6. **Entity Framework + ADO.NET** selon les besoins
7. **Cookie Authentication** pour la sécurité
8. **Pagination** pour les performances

Ces choix permettent d'avoir un code maintenable, testable et évolutif.

---

*Document généré pour le projet Bibliotheca*
