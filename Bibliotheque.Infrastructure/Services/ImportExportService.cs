using System.Globalization;
using Bibliotheque.Core.DTOs;
using Bibliotheque.Core.Entities;
using Bibliotheque.Core.Interfaces;
using CsvHelper;
using CsvHelper.Configuration;
using iText.Kernel.Pdf;
using iText.Layout;
using iText.Layout.Element;
using iText.Layout.Properties;
using iText.Kernel.Colors;

namespace Bibliotheque.Infrastructure.Services
{
    public class ImportExportService : IImportExportService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IAuthService _authService;

        public ImportExportService(IUnitOfWork unitOfWork, IAuthService authService)
        {
            _unitOfWork = unitOfWork;
            _authService = authService;
        }

        #region Import CSV

        public async Task<ImportResultDTO> ImporterLivresCsvAsync(Stream fichierCsv)
        {
            var result = new ImportResultDTO();

            try
            {
                using var reader = new StreamReader(fichierCsv);
                var config = new CsvConfiguration(CultureInfo.InvariantCulture)
                {
                    HeaderValidated = null,
                    MissingFieldFound = null,
                    Delimiter = ";"
                };

                using var csv = new CsvReader(reader, config);
                var records = csv.GetRecords<LivreImportDTO>().ToList();
                result.TotalLignes = records.Count;

                foreach (var record in records)
                {
                    try
                    {
                        // Valider les données obligatoires
                        if (string.IsNullOrWhiteSpace(record.Titre))
                        {
                            result.Erreurs.Add($"Ligne ignorée : titre manquant");
                            result.LignesErreur++;
                            continue;
                        }

                        if (string.IsNullOrWhiteSpace(record.AuteurNom))
                        {
                            result.Erreurs.Add($"Livre '{record.Titre}' : auteur manquant");
                            result.LignesErreur++;
                            continue;
                        }

                        // Vérifier si l'ISBN existe déjà
                        if (!string.IsNullOrEmpty(record.ISBN) &&
                            await _unitOfWork.Livres.IsbnExisteAsync(record.ISBN))
                        {
                            result.Erreurs.Add($"Livre '{record.Titre}' : ISBN déjà existant");
                            result.LignesErreur++;
                            continue;
                        }

                        // Créer ou récupérer l'auteur
                        var auteur = await _unitOfWork.Auteurs.GetOrCreateAsync(
                            record.AuteurNom, record.AuteurPrenom);

                        // Créer le livre
                        var livre = new Livre
                        {
                            ISBN = record.ISBN,
                            Titre = record.Titre,
                            IdAuteur = auteur.IdAuteur,
                            Annee = record.Annee,
                            Editeur = record.Editeur,
                            NombrePages = record.NombrePages,
                            Langue = record.Langue ?? "Français",
                            Description = record.Description,
                            Stock = record.Stock > 0 ? record.Stock : 1,
                            StockDisponible = record.Stock > 0 ? record.Stock : 1,
                            DateAjout = DateTime.Now,
                            Actif = true
                        };

                        await _unitOfWork.Livres.AddAsync(livre);
                        await _unitOfWork.SaveChangesAsync();

                        // Gérer les catégories
                        if (!string.IsNullOrWhiteSpace(record.Categories))
                        {
                            var categorieNoms = record.Categories.Split(',')
                                .Select(c => c.Trim())
                                .Where(c => !string.IsNullOrEmpty(c));

                            var categorieIds = new List<int>();
                            foreach (var nom in categorieNoms)
                            {
                                var categorie = await _unitOfWork.Categories.GetOrCreateAsync(nom);
                                await _unitOfWork.SaveChangesAsync();
                                categorieIds.Add(categorie.IdCategorie);
                            }

                            if (categorieIds.Any())
                            {
                                await _unitOfWork.Livres.UpdateCategoriesAsync(livre.IdLivre, categorieIds);
                                await _unitOfWork.SaveChangesAsync();
                            }
                        }

                        result.LignesImportees++;
                    }
                    catch (Exception ex)
                    {
                        result.Erreurs.Add($"Livre '{record.Titre}' : {ex.Message}");
                        result.LignesErreur++;
                    }
                }
            }
            catch (Exception ex)
            {
                result.Erreurs.Add($"Erreur de lecture du fichier CSV : {ex.Message}");
            }

            return result;
        }

        public async Task<ImportResultDTO> ImporterAuteursCsvAsync(Stream fichierCsv)
        {
            var result = new ImportResultDTO();

            try
            {
                using var reader = new StreamReader(fichierCsv);
                var config = new CsvConfiguration(CultureInfo.InvariantCulture)
                {
                    HeaderValidated = null,
                    MissingFieldFound = null,
                    Delimiter = ";"
                };

                using var csv = new CsvReader(reader, config);
                var records = csv.GetRecords<dynamic>().ToList();
                result.TotalLignes = records.Count;

                foreach (var record in records)
                {
                    try
                    {
                        var dict = (IDictionary<string, object>)record;
                        var nom = dict.ContainsKey("Nom") ? dict["Nom"]?.ToString() : null;

                        if (string.IsNullOrWhiteSpace(nom))
                        {
                            result.Erreurs.Add("Ligne ignorée : nom manquant");
                            result.LignesErreur++;
                            continue;
                        }

                        var prenom = dict.ContainsKey("Prenom") ? dict["Prenom"]?.ToString() : null;

                        // Vérifier si l'auteur existe déjà
                        var existant = await _unitOfWork.Auteurs.FirstOrDefaultAsync(a =>
                            a.Nom.ToLower() == nom.ToLower() &&
                            (prenom == null || (a.Prenom != null && a.Prenom.ToLower() == prenom.ToLower())));

                        if (existant != null)
                        {
                            result.Erreurs.Add($"Auteur '{nom}' : déjà existant");
                            result.LignesErreur++;
                            continue;
                        }

                        var auteur = new Auteur
                        {
                            Nom = nom,
                            Prenom = prenom,
                            Nationalite = dict.ContainsKey("Nationalite") ? dict["Nationalite"]?.ToString() : null,
                            Biographie = dict.ContainsKey("Biographie") ? dict["Biographie"]?.ToString() : null,
                            DateCreation = DateTime.Now,
                            Actif = true
                        };

                        await _unitOfWork.Auteurs.AddAsync(auteur);
                        result.LignesImportees++;
                    }
                    catch (Exception ex)
                    {
                        result.Erreurs.Add($"Erreur : {ex.Message}");
                        result.LignesErreur++;
                    }
                }

                await _unitOfWork.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                result.Erreurs.Add($"Erreur de lecture du fichier CSV : {ex.Message}");
            }

            return result;
        }

        public async Task<ImportResultDTO> ImporterUtilisateursCsvAsync(Stream fichierCsv)
        {
            var result = new ImportResultDTO();

            try
            {
                using var reader = new StreamReader(fichierCsv);
                var config = new CsvConfiguration(CultureInfo.InvariantCulture)
                {
                    HeaderValidated = null,
                    MissingFieldFound = null,
                    Delimiter = ";"
                };

                using var csv = new CsvReader(reader, config);
                var records = csv.GetRecords<dynamic>().ToList();
                result.TotalLignes = records.Count;

                foreach (var record in records)
                {
                    try
                    {
                        var dict = (IDictionary<string, object>)record;
                        var nom = dict.ContainsKey("Nom") ? dict["Nom"]?.ToString() : null;
                        var prenom = dict.ContainsKey("Prenom") ? dict["Prenom"]?.ToString() : null;
                        var email = dict.ContainsKey("Email") ? dict["Email"]?.ToString() : null;

                        if (string.IsNullOrWhiteSpace(nom) || string.IsNullOrWhiteSpace(email))
                        {
                            result.Erreurs.Add("Ligne ignorée : nom ou email manquant");
                            result.LignesErreur++;
                            continue;
                        }

                        // Vérifier si l'email existe déjà
                        if (await _unitOfWork.Utilisateurs.EmailExisteAsync(email))
                        {
                            result.Erreurs.Add($"Utilisateur '{email}' : email déjà existant");
                            result.LignesErreur++;
                            continue;
                        }

                        // Mot de passe par défaut
                        var motDePasseParDefaut = "Utilisateur123!";

                        var utilisateur = new Utilisateur
                        {
                            Nom = nom,
                            Prenom = prenom ?? "",
                            Email = email,
                            Telephone = dict.ContainsKey("Telephone") ? dict["Telephone"]?.ToString() : null,
                            Adresse = dict.ContainsKey("Adresse") ? dict["Adresse"]?.ToString() : null,
                            MotDePasseHash = _authService.HashMotDePasse(motDePasseParDefaut),
                            DateInscription = DateTime.Now,
                            NombreEmpruntsMax = 3,
                            Actif = true
                        };

                        await _unitOfWork.Utilisateurs.AddAsync(utilisateur);
                        result.LignesImportees++;
                    }
                    catch (Exception ex)
                    {
                        result.Erreurs.Add($"Erreur : {ex.Message}");
                        result.LignesErreur++;
                    }
                }

                await _unitOfWork.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                result.Erreurs.Add($"Erreur de lecture du fichier CSV : {ex.Message}");
            }

            return result;
        }

        #endregion

        #region Export PDF

        public async Task<byte[]> ExporterLivresPdfAsync()
        {
            var livres = await _unitOfWork.Livres.GetAllWithDetailsAsync();

            using var memoryStream = new MemoryStream();
            using var writer = new PdfWriter(memoryStream);
            using var pdf = new PdfDocument(writer);
            using var document = new Document(pdf);

            // Titre
            document.Add(new Paragraph("Liste des Livres")
                .SetFontSize(20)
                .SetBold()
                .SetTextAlignment(TextAlignment.CENTER));

            document.Add(new Paragraph($"Généré le {DateTime.Now:dd/MM/yyyy HH:mm}")
                .SetFontSize(10)
                .SetTextAlignment(TextAlignment.CENTER)
                .SetMarginBottom(20));

            // Tableau
            var table = new Table(new float[] { 2, 3, 2, 1, 1, 1 })
                .SetWidth(UnitValue.CreatePercentValue(100));

            // En-têtes
            string[] headers = { "ISBN", "Titre", "Auteur", "Année", "Stock", "Disponible" };
            foreach (var header in headers)
            {
                table.AddHeaderCell(new Cell()
                    .SetBackgroundColor(ColorConstants.LIGHT_GRAY)
                    .Add(new Paragraph(header).SetBold()));
            }

            // Données
            foreach (var livre in livres)
            {
                table.AddCell(livre.ISBN ?? "-");
                table.AddCell(livre.Titre);
                table.AddCell(livre.Auteur?.NomComplet ?? "-");
                table.AddCell(livre.Annee?.ToString() ?? "-");
                table.AddCell(livre.Stock.ToString());
                table.AddCell(livre.StockDisponible.ToString());
            }

            document.Add(table);

            // Statistiques
            document.Add(new Paragraph($"\nTotal : {livres.Count()} livres")
                .SetFontSize(10)
                .SetItalic());

            document.Close();
            return memoryStream.ToArray();
        }

        public async Task<byte[]> ExporterEmpruntsPdfAsync(EmpruntFiltreDTO? filtre = null)
        {
            filtre ??= new EmpruntFiltreDTO { TaillePage = 1000 };
            var result = await _unitOfWork.Emprunts.GetFiltreAsync(filtre);

            using var memoryStream = new MemoryStream();
            using var writer = new PdfWriter(memoryStream);
            using var pdf = new PdfDocument(writer);
            using var document = new Document(pdf);

            // Titre
            document.Add(new Paragraph("Liste des Emprunts")
                .SetFontSize(20)
                .SetBold()
                .SetTextAlignment(TextAlignment.CENTER));

            document.Add(new Paragraph($"Généré le {DateTime.Now:dd/MM/yyyy HH:mm}")
                .SetFontSize(10)
                .SetTextAlignment(TextAlignment.CENTER)
                .SetMarginBottom(20));

            // Tableau
            var table = new Table(new float[] { 2, 2, 2, 2, 1 })
                .SetWidth(UnitValue.CreatePercentValue(100));

            // En-têtes
            string[] headers = { "Livre", "Utilisateur", "Date Emprunt", "Retour Prévu", "Statut" };
            foreach (var header in headers)
            {
                table.AddHeaderCell(new Cell()
                    .SetBackgroundColor(ColorConstants.LIGHT_GRAY)
                    .Add(new Paragraph(header).SetBold()));
            }

            // Données
            foreach (var emprunt in result.Items)
            {
                table.AddCell(emprunt.LivreTitre);
                table.AddCell(emprunt.UtilisateurNom);
                table.AddCell(emprunt.DateEmprunt.ToString("dd/MM/yyyy"));
                table.AddCell(emprunt.DateRetourPrevue.ToString("dd/MM/yyyy"));

                var statutCell = new Cell().Add(new Paragraph(emprunt.StatutAffichage));
                if (emprunt.Statut == "EnRetard" || emprunt.EstEnRetard)
                    statutCell.SetBackgroundColor(new DeviceRgb(255, 200, 200));
                table.AddCell(statutCell);
            }

            document.Add(table);

            document.Add(new Paragraph($"\nTotal : {result.TotalItems} emprunts")
                .SetFontSize(10)
                .SetItalic());

            document.Close();
            return memoryStream.ToArray();
        }

        public async Task<byte[]> ExporterUtilisateursPdfAsync()
        {
            var utilisateurs = await _unitOfWork.Utilisateurs.GetAllAsync();

            using var memoryStream = new MemoryStream();
            using var writer = new PdfWriter(memoryStream);
            using var pdf = new PdfDocument(writer);
            using var document = new Document(pdf);

            // Titre
            document.Add(new Paragraph("Liste des Utilisateurs")
                .SetFontSize(20)
                .SetBold()
                .SetTextAlignment(TextAlignment.CENTER));

            document.Add(new Paragraph($"Généré le {DateTime.Now:dd/MM/yyyy HH:mm}")
                .SetFontSize(10)
                .SetTextAlignment(TextAlignment.CENTER)
                .SetMarginBottom(20));

            // Tableau
            var table = new Table(new float[] { 2, 3, 2, 2 })
                .SetWidth(UnitValue.CreatePercentValue(100));

            // En-têtes
            string[] headers = { "Nom", "Email", "Téléphone", "Inscription" };
            foreach (var header in headers)
            {
                table.AddHeaderCell(new Cell()
                    .SetBackgroundColor(ColorConstants.LIGHT_GRAY)
                    .Add(new Paragraph(header).SetBold()));
            }

            // Données
            foreach (var u in utilisateurs.Where(u => u.Actif))
            {
                table.AddCell(u.NomComplet);
                table.AddCell(u.Email);
                table.AddCell(u.Telephone ?? "-");
                table.AddCell(u.DateInscription.ToString("dd/MM/yyyy"));
            }

            document.Add(table);

            document.Add(new Paragraph($"\nTotal : {utilisateurs.Count(u => u.Actif)} utilisateurs actifs")
                .SetFontSize(10)
                .SetItalic());

            document.Close();
            return memoryStream.ToArray();
        }

        public async Task<byte[]> ExporterRapportActivitePdfAsync(DateTime dateDebut, DateTime dateFin)
        {
            var stats = await new StatistiquesService(_unitOfWork).GetRapportActiviteAsync(dateDebut, dateFin);

            using var memoryStream = new MemoryStream();
            using var writer = new PdfWriter(memoryStream);
            using var pdf = new PdfDocument(writer);
            using var document = new Document(pdf);

            // Titre
            document.Add(new Paragraph("Rapport d'Activité")
                .SetFontSize(24)
                .SetBold()
                .SetTextAlignment(TextAlignment.CENTER));

            document.Add(new Paragraph($"Période : {dateDebut:dd/MM/yyyy} - {dateFin:dd/MM/yyyy}")
                .SetFontSize(12)
                .SetTextAlignment(TextAlignment.CENTER)
                .SetMarginBottom(30));

            // Section Statistiques générales
            document.Add(new Paragraph("Statistiques générales")
                .SetFontSize(16)
                .SetBold()
                .SetMarginTop(20));

            var statsTable = new Table(2).SetWidth(UnitValue.CreatePercentValue(50));
            statsTable.AddCell("Nouveaux utilisateurs :").AddCell(stats.NouveauxUtilisateurs.ToString());
            statsTable.AddCell("Nouveaux livres :").AddCell(stats.NouveauxLivres.ToString());
            statsTable.AddCell("Total emprunts :").AddCell(stats.TotalEmprunts.ToString());
            statsTable.AddCell("Total retours :").AddCell(stats.TotalRetours.ToString());
            statsTable.AddCell("Retards :").AddCell(stats.TotalRetards.ToString());
            statsTable.AddCell("Pénalités collectées :").AddCell($"{stats.TotalPenalites:C}");
            document.Add(statsTable);

            // Top livres
            if (stats.LivresLesPlusEmpruntes.Any())
            {
                document.Add(new Paragraph("Livres les plus empruntés")
                    .SetFontSize(16)
                    .SetBold()
                    .SetMarginTop(20));

                var topTable = new Table(new float[] { 3, 2, 1 })
                    .SetWidth(UnitValue.CreatePercentValue(80));
                topTable.AddHeaderCell("Titre");
                topTable.AddHeaderCell("Auteur");
                topTable.AddHeaderCell("Emprunts");

                foreach (var livre in stats.LivresLesPlusEmpruntes)
                {
                    topTable.AddCell(livre.Titre);
                    topTable.AddCell(livre.Auteur);
                    topTable.AddCell(livre.NombreEmprunts.ToString());
                }
                document.Add(topTable);
            }

            document.Add(new Paragraph($"\nRapport généré le {DateTime.Now:dd/MM/yyyy HH:mm}")
                .SetFontSize(8)
                .SetItalic()
                .SetMarginTop(30));

            document.Close();
            return memoryStream.ToArray();
        }

        #endregion
    }
}
