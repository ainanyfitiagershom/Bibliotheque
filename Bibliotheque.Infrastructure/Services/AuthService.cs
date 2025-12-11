using Bibliotheque.Core.DTOs;
using Bibliotheque.Core.Entities;
using Bibliotheque.Core.Interfaces;
using BCrypt.Net;

namespace Bibliotheque.Infrastructure.Services
{
    public class AuthService : IAuthService
    {
        private readonly IUnitOfWork _unitOfWork;

        public AuthService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<(bool Succes, Admin? Admin, string Message)> AuthentifierAdminAsync(string email, string motDePasse)
        {
            var admin = await _unitOfWork.Admins.GetByEmailAsync(email);

            if (admin == null)
            {
                return (false, null, "Email ou mot de passe incorrect.");
            }

            if (!admin.Actif)
            {
                return (false, null, "Ce compte a été désactivé.");
            }

            if (!VerifierMotDePasse(motDePasse, admin.MotDePasseHash))
            {
                return (false, null, "Email ou mot de passe incorrect.");
            }

            // Mettre à jour la dernière connexion
            await _unitOfWork.Admins.UpdateDerniereConnexionAsync(admin.IdAdmin);
            await _unitOfWork.SaveChangesAsync();

            return (true, admin, "Connexion réussie.");
        }

        public async Task<(bool Succes, Utilisateur? Utilisateur, string Message)> AuthentifierUtilisateurAsync(string email, string motDePasse)
        {
            var utilisateur = await _unitOfWork.Utilisateurs.GetByEmailAsync(email);

            if (utilisateur == null)
            {
                return (false, null, "Email ou mot de passe incorrect.");
            }

            if (!utilisateur.Actif)
            {
                return (false, null, "Ce compte a été désactivé.");
            }

            if (utilisateur.EstBloque)
            {
                return (false, null, $"Ce compte est bloqué. Raison : {utilisateur.RaisonBlocage ?? "Non spécifiée"}");
            }

            if (!VerifierMotDePasse(motDePasse, utilisateur.MotDePasseHash))
            {
                return (false, null, "Email ou mot de passe incorrect.");
            }

            // Mettre à jour la dernière connexion
            await _unitOfWork.Utilisateurs.UpdateDerniereConnexionAsync(utilisateur.IdUtilisateur);
            await _unitOfWork.SaveChangesAsync();

            return (true, utilisateur, "Connexion réussie.");
        }

        public async Task<(bool Succes, string Message)> InscrireUtilisateurAsync(InscriptionDTO inscription)
        {
            // Vérifier si l'email existe déjà
            if (await _unitOfWork.Utilisateurs.EmailExisteAsync(inscription.Email))
            {
                return (false, "Cet email est déjà utilisé.");
            }

            // Créer le nouvel utilisateur
            var utilisateur = new Utilisateur
            {
                Nom = inscription.Nom,
                Prenom = inscription.Prenom,
                Email = inscription.Email,
                Telephone = inscription.Telephone,
                Adresse = inscription.Adresse,
                MotDePasseHash = HashMotDePasse(inscription.MotDePasse),
                DateInscription = DateTime.Now,
                NombreEmpruntsMax = 3,
                Actif = true,
                EstBloque = false
            };

            await _unitOfWork.Utilisateurs.AddAsync(utilisateur);
            await _unitOfWork.SaveChangesAsync();

            // Créer une notification de bienvenue
            await _unitOfWork.Notifications.CreerNotificationBienvenueAsync(utilisateur.IdUtilisateur);
            await _unitOfWork.SaveChangesAsync();

            return (true, "Inscription réussie ! Vous pouvez maintenant vous connecter.");
        }

        public async Task<(bool Succes, string Message)> ChangerMotDePasseAsync(int idUtilisateur, string ancienMdp, string nouveauMdp)
        {
            var utilisateur = await _unitOfWork.Utilisateurs.GetByIdAsync(idUtilisateur);

            if (utilisateur == null)
            {
                return (false, "Utilisateur non trouvé.");
            }

            if (!VerifierMotDePasse(ancienMdp, utilisateur.MotDePasseHash))
            {
                return (false, "L'ancien mot de passe est incorrect.");
            }

            utilisateur.MotDePasseHash = HashMotDePasse(nouveauMdp);
            await _unitOfWork.Utilisateurs.UpdateAsync(utilisateur);
            await _unitOfWork.SaveChangesAsync();

            return (true, "Mot de passe modifié avec succès.");
        }

        public string HashMotDePasse(string motDePasse)
        {
            return BCrypt.Net.BCrypt.HashPassword(motDePasse, 11);
        }

        public bool VerifierMotDePasse(string motDePasse, string hash)
        {
            try
            {
                return BCrypt.Net.BCrypt.Verify(motDePasse, hash);
            }
            catch
            {
                return false;
            }
        }
    }
}
