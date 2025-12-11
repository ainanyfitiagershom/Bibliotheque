using Bibliotheque.Core.Entities;
using Microsoft.EntityFrameworkCore;

namespace Bibliotheque.Infrastructure.Data
{
    /// <summary>
    /// Contexte Entity Framework Core pour la base de données Bibliothèque
    /// </summary>
    public class BibliothequeDbContext : DbContext
    {
        public BibliothequeDbContext(DbContextOptions<BibliothequeDbContext> options)
            : base(options)
        {
        }

        // DbSets
        public DbSet<Admin> Admins { get; set; }
        public DbSet<Utilisateur> Utilisateurs { get; set; }
        public DbSet<Categorie> Categories { get; set; }
        public DbSet<Auteur> Auteurs { get; set; }
        public DbSet<Livre> Livres { get; set; }
        public DbSet<LivreCategorie> LivreCategories { get; set; }
        public DbSet<Emprunt> Emprunts { get; set; }
        public DbSet<Reservation> Reservations { get; set; }
        public DbSet<Avis> Avis { get; set; }
        public DbSet<Notification> Notifications { get; set; }
        public DbSet<Historique> Historiques { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configuration de la table de liaison LivreCategories (clé composite)
            modelBuilder.Entity<LivreCategorie>()
                .HasKey(lc => new { lc.IdLivre, lc.IdCategorie });

            modelBuilder.Entity<LivreCategorie>()
                .HasOne(lc => lc.Livre)
                .WithMany(l => l.LivreCategories)
                .HasForeignKey(lc => lc.IdLivre)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<LivreCategorie>()
                .HasOne(lc => lc.Categorie)
                .WithMany(c => c.LivreCategories)
                .HasForeignKey(lc => lc.IdCategorie)
                .OnDelete(DeleteBehavior.Cascade);

            // Configuration Livre
            modelBuilder.Entity<Livre>()
                .HasOne(l => l.Auteur)
                .WithMany(a => a.Livres)
                .HasForeignKey(l => l.IdAuteur)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Livre>()
                .HasIndex(l => l.ISBN)
                .IsUnique()
                .HasFilter("[ISBN] IS NOT NULL");

            modelBuilder.Entity<Livre>()
                .HasIndex(l => l.Titre);

            modelBuilder.Entity<Livre>()
                .Property(l => l.NoteMoyenne)
                .HasColumnType("decimal(3,2)");

            // Configuration Emprunt
            modelBuilder.Entity<Emprunt>()
                .HasOne(e => e.Livre)
                .WithMany(l => l.Emprunts)
                .HasForeignKey(e => e.IdLivre)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Emprunt>()
                .HasOne(e => e.Utilisateur)
                .WithMany(u => u.Emprunts)
                .HasForeignKey(e => e.IdUtilisateur)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Emprunt>()
                .Property(e => e.Penalite)
                .HasColumnType("decimal(10,2)");

            modelBuilder.Entity<Emprunt>()
                .HasIndex(e => e.Statut);

            modelBuilder.Entity<Emprunt>()
                .HasIndex(e => e.DateRetourPrevue);

            // Configuration Reservation
            modelBuilder.Entity<Reservation>()
                .HasOne(r => r.Livre)
                .WithMany(l => l.Reservations)
                .HasForeignKey(r => r.IdLivre)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Reservation>()
                .HasOne(r => r.Utilisateur)
                .WithMany(u => u.Reservations)
                .HasForeignKey(r => r.IdUtilisateur)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Reservation>()
                .HasIndex(r => r.Statut);

            // Configuration Avis
            modelBuilder.Entity<Avis>()
                .HasOne(a => a.Livre)
                .WithMany(l => l.Avis)
                .HasForeignKey(a => a.IdLivre)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Avis>()
                .HasOne(a => a.Utilisateur)
                .WithMany(u => u.Avis)
                .HasForeignKey(a => a.IdUtilisateur)
                .OnDelete(DeleteBehavior.Cascade);

            // Un utilisateur ne peut donner qu'un seul avis par livre
            modelBuilder.Entity<Avis>()
                .HasIndex(a => new { a.IdLivre, a.IdUtilisateur })
                .IsUnique();

            // Configuration Notification
            modelBuilder.Entity<Notification>()
                .HasOne(n => n.Utilisateur)
                .WithMany(u => u.Notifications)
                .HasForeignKey(n => n.IdUtilisateur)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Notification>()
                .HasIndex(n => n.EstLue);

            // Configuration Historique
            modelBuilder.Entity<Historique>()
                .HasOne(h => h.Admin)
                .WithMany()
                .HasForeignKey(h => h.IdAdmin)
                .OnDelete(DeleteBehavior.SetNull);

            // Configuration Admin
            modelBuilder.Entity<Admin>()
                .HasIndex(a => a.Email)
                .IsUnique();

            // Configuration Utilisateur
            modelBuilder.Entity<Utilisateur>()
                .HasIndex(u => u.Email)
                .IsUnique();

            // Configuration Categorie
            modelBuilder.Entity<Categorie>()
                .HasIndex(c => c.Nom)
                .IsUnique();

            // Configuration Auteur
            modelBuilder.Entity<Auteur>()
                .HasIndex(a => a.Nom);
        }
    }
}
