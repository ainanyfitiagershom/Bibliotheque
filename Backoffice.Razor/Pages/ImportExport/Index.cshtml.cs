using System.Globalization;
using System.Text;
using Bibliotheque.Core.Entities;
using Bibliotheque.Core.Interfaces;
using CsvHelper;
using CsvHelper.Configuration;
using iText.Kernel.Pdf;
using iText.Layout;
using iText.Layout.Element;
using iText.Layout.Properties;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Backoffice.Razor.Pages.ImportExport
{
    [Authorize]
    public class IndexModel : PageModel
    {
        private readonly IUnitOfWork _unitOfWork;

        public IndexModel(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public void OnGet()
        {
        }

        public async Task<IActionResult> OnPostImportAsync(string type, IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                TempData["Error"] = "Veuillez sélectionner un fichier.";
                return RedirectToPage();
            }

            try
            {
                using var reader = new StreamReader(file.OpenReadStream(), Encoding.UTF8);
                var config = new CsvConfiguration(CultureInfo.InvariantCulture)
                {
                    Delimiter = ";",
                    HasHeaderRecord = true
                };
                using var csv = new CsvReader(reader, config);

                int count = 0;
                int updated = 0;

                switch (type)
                {
                    case "livres":
                        (count, updated) = await ImportLivresAsync(csv);
                        break;
                    case "auteurs":
                        (count, updated) = await ImportAuteursAsync(csv);
                        break;
                    case "categories":
                        (count, updated) = await ImportCategoriesAsync(csv);
                        break;
                    case "utilisateurs":
                        (count, updated) = await ImportUtilisateursAsync(csv);
                        break;
                }

                var message = new List<string>();
                if (count > 0) message.Add($"{count} créé(s)");
                if (updated > 0) message.Add($"{updated} mis à jour");
                TempData["Success"] = message.Count > 0
                    ? $"Import réussi : {string.Join(", ", message)}."
                    : "Aucun enregistrement à importer.";
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Erreur lors de l'import : {ex.Message}";
            }

            return RedirectToPage();
        }

        public async Task<IActionResult> OnGetExportCsvAsync(string type)
        {
            var csv = new StringBuilder();

            switch (type)
            {
                case "livres":
                    var livres = await _unitOfWork.Livres.GetAllAsync();
                    csv.AppendLine("ISBN;Titre;Auteur;Annee;Editeur;Stock;StockDisponible");
                    foreach (var l in livres.Where(l => l.Actif))
                    {
                        csv.AppendLine($"{l.ISBN};{l.Titre};{l.Auteur?.NomComplet};{l.Annee};{l.Editeur};{l.Stock};{l.StockDisponible}");
                    }
                    break;

                case "auteurs":
                    var auteurs = await _unitOfWork.Auteurs.GetAllAsync();
                    csv.AppendLine("Nom;Prenom;Nationalite;DateNaissance");
                    foreach (var a in auteurs.Where(a => a.Actif))
                    {
                        csv.AppendLine($"{a.Nom};{a.Prenom};{a.Nationalite};{a.DateNaissance?.ToString("yyyy-MM-dd")}");
                    }
                    break;

                case "categories":
                    var cats = await _unitOfWork.Categories.GetAllAsync();
                    csv.AppendLine("Nom;Description;Couleur");
                    foreach (var c in cats.Where(c => c.Actif))
                    {
                        csv.AppendLine($"{c.Nom};{c.Description};{c.Couleur}");
                    }
                    break;

                case "utilisateurs":
                    var users = await _unitOfWork.Utilisateurs.GetAllAsync();
                    csv.AppendLine("NumeroAbonne;Nom;Prenom;Email;Telephone;DateInscription;Statut");
                    foreach (var u in users)
                    {
                        csv.AppendLine($"{u.NumeroAbonne};{u.Nom};{u.Prenom};{u.Email};{u.Telephone};{u.DateInscription:yyyy-MM-dd};{u.Statut}");
                    }
                    break;

                case "emprunts":
                    var emprunts = await _unitOfWork.Emprunts.GetAllWithDetailsAsync();
                    csv.AppendLine("Livre;Utilisateur;DateEmprunt;DateRetourPrevue;DateRetourEffective;Statut");
                    foreach (var e in emprunts)
                    {
                        csv.AppendLine($"{e.Livre?.Titre};{e.Utilisateur?.Nom} {e.Utilisateur?.Prenom};{e.DateEmprunt:yyyy-MM-dd};{e.DateRetourPrevue:yyyy-MM-dd};{e.DateRetourEffective?.ToString("yyyy-MM-dd")};{e.Statut}");
                    }
                    break;
            }

            var bytes = Encoding.UTF8.GetBytes(csv.ToString());
            return File(bytes, "text/csv", $"export_{type}_{DateTime.Now:yyyyMMdd}.csv");
        }

        public async Task<IActionResult> OnGetExportPdfAsync(string type)
        {
            using var stream = new MemoryStream();
            using var writer = new PdfWriter(stream);
            using var pdf = new PdfDocument(writer);
            using var document = new Document(pdf);

            document.Add(new Paragraph($"Bibliothèque - Rapport {type}")
                .SetFontSize(20)
                .SetTextAlignment(TextAlignment.CENTER));
            document.Add(new Paragraph($"Généré le {DateTime.Now:dd/MM/yyyy HH:mm}")
                .SetFontSize(10)
                .SetTextAlignment(TextAlignment.CENTER));
            document.Add(new Paragraph(" "));

            switch (type)
            {
                case "livres":
                    await GenerateLivresPdfAsync(document);
                    break;
                case "emprunts":
                    await GenerateEmpruntsPdfAsync(document);
                    break;
                case "statistiques":
                    await GenerateStatistiquesPdfAsync(document);
                    break;
            }

            document.Close();
            return File(stream.ToArray(), "application/pdf", $"rapport_{type}_{DateTime.Now:yyyyMMdd}.pdf");
        }

        private async Task<(int created, int updated)> ImportLivresAsync(CsvReader csv)
        {
            var records = csv.GetRecords<dynamic>().ToList();
            int count = 0;
            int updated = 0;

            // Charger tous les auteurs et livres existants
            var auteurs = (await _unitOfWork.Auteurs.GetAllAsync()).ToList();
            var livresExistants = (await _unitOfWork.Livres.GetAllAsync()).ToList();

            foreach (var record in records)
            {
                int? annee = null;
                int stock = 1;

                if (int.TryParse(record.Annee?.ToString(), out int anneeValue))
                {
                    annee = anneeValue;
                }
                int.TryParse(record.Stock?.ToString(), out stock);

                // Chercher l'auteur par ID ou par nom
                int idAuteur;
                string auteurValue = record.Auteur?.ToString() ?? record.IdAuteur?.ToString() ?? "";

                if (int.TryParse(auteurValue, out int auteurId))
                {
                    // C'est un ID, vérifier s'il existe
                    var auteurExistant = auteurs.FirstOrDefault(a => a.IdAuteur == auteurId);
                    if (auteurExistant != null)
                    {
                        idAuteur = auteurId;
                    }
                    else
                    {
                        // Créer un auteur "Inconnu" avec cet ID n'est pas possible, créer un nouvel auteur
                        var nouvelAuteur = new Auteur
                        {
                            Nom = $"Auteur #{auteurId}",
                            Prenom = "",
                            Actif = true
                        };
                        await _unitOfWork.Auteurs.AddAsync(nouvelAuteur);
                        await _unitOfWork.SaveChangesAsync();
                        auteurs.Add(nouvelAuteur);
                        idAuteur = nouvelAuteur.IdAuteur;
                    }
                }
                else
                {
                    // C'est un nom, chercher ou créer l'auteur
                    var auteurExistant = auteurs.FirstOrDefault(a =>
                        a.NomComplet.Equals(auteurValue, StringComparison.OrdinalIgnoreCase) ||
                        a.Nom.Equals(auteurValue, StringComparison.OrdinalIgnoreCase));

                    if (auteurExistant != null)
                    {
                        idAuteur = auteurExistant.IdAuteur;
                    }
                    else
                    {
                        // Créer un nouvel auteur avec ce nom
                        var parts = auteurValue.Split(' ', 2);
                        var nouvelAuteur = new Auteur
                        {
                            Nom = parts.Length > 1 ? parts[1] : parts[0],
                            Prenom = parts.Length > 1 ? parts[0] : "",
                            Actif = true
                        };
                        await _unitOfWork.Auteurs.AddAsync(nouvelAuteur);
                        await _unitOfWork.SaveChangesAsync();
                        auteurs.Add(nouvelAuteur);
                        idAuteur = nouvelAuteur.IdAuteur;
                    }
                }

                // Vérifier si un livre avec cet ISBN existe déjà
                string isbn = record.ISBN?.ToString() ?? "";
                var livreExistant = !string.IsNullOrEmpty(isbn)
                    ? livresExistants.FirstOrDefault(l => l.ISBN == isbn)
                    : null;

                if (livreExistant != null)
                {
                    // Mettre à jour le livre existant
                    livreExistant.Titre = record.Titre;
                    livreExistant.IdAuteur = idAuteur;
                    livreExistant.Annee = annee;
                    livreExistant.Editeur = record.Editeur;
                    livreExistant.Stock = stock;
                    livreExistant.Description = record.Description;
                    livreExistant.DateModification = DateTime.Now;

                    await _unitOfWork.Livres.UpdateAsync(livreExistant);
                    updated++;
                }
                else
                {
                    // Créer un nouveau livre
                    var livre = new Livre
                    {
                        ISBN = isbn,
                        Titre = record.Titre,
                        IdAuteur = idAuteur,
                        Annee = annee,
                        Editeur = record.Editeur,
                        Stock = stock,
                        StockDisponible = stock,
                        Description = record.Description,
                        DateAjout = DateTime.Now,
                        Actif = true
                    };

                    await _unitOfWork.Livres.AddAsync(livre);
                    count++;
                }
            }

            await _unitOfWork.SaveChangesAsync();

            return (count, updated);
        }

        private async Task<(int created, int updated)> ImportAuteursAsync(CsvReader csv)
        {
            var records = csv.GetRecords<dynamic>().ToList();
            int count = 0;
            int updated = 0;

            var auteursExistants = (await _unitOfWork.Auteurs.GetAllAsync()).ToList();

            foreach (var record in records)
            {
                string nom = record.Nom?.ToString() ?? "";
                string prenom = record.Prenom?.ToString() ?? "";

                // Vérifier si l'auteur existe déjà (par nom + prénom)
                var auteurExistant = auteursExistants.FirstOrDefault(a =>
                    a.Nom.Equals(nom, StringComparison.OrdinalIgnoreCase) &&
                    (a.Prenom ?? "").Equals(prenom, StringComparison.OrdinalIgnoreCase));

                if (auteurExistant != null)
                {
                    // Mettre à jour
                    auteurExistant.Nationalite = record.Nationalite;
                    auteurExistant.DateNaissance = DateTime.TryParse(record.DateNaissance?.ToString(), out DateTime dateN) ? dateN : null;
                    auteurExistant.DateDeces = DateTime.TryParse(record.DateDeces?.ToString(), out DateTime dateD) ? dateD : null;
                    auteurExistant.Biographie = record.Biographie;

                    await _unitOfWork.Auteurs.UpdateAsync(auteurExistant);
                    updated++;
                }
                else
                {
                    var auteur = new Auteur
                    {
                        Nom = nom,
                        Prenom = prenom,
                        Nationalite = record.Nationalite,
                        DateNaissance = DateTime.TryParse(record.DateNaissance?.ToString(), out DateTime date) ? date : null,
                        DateDeces = DateTime.TryParse(record.DateDeces?.ToString(), out DateTime dateD) ? dateD : null,
                        Biographie = record.Biographie,
                        Actif = true
                    };

                    await _unitOfWork.Auteurs.AddAsync(auteur);
                    auteursExistants.Add(auteur);
                    count++;
                }
            }

            await _unitOfWork.SaveChangesAsync();
            return (count, updated);
        }

        private async Task<(int created, int updated)> ImportCategoriesAsync(CsvReader csv)
        {
            var records = csv.GetRecords<dynamic>().ToList();
            int count = 0;
            int updated = 0;

            var categoriesExistantes = (await _unitOfWork.Categories.GetAllAsync()).ToList();

            foreach (var record in records)
            {
                string nom = record.Nom?.ToString() ?? "";

                // Vérifier si la catégorie existe déjà (par nom)
                var categorieExistante = categoriesExistantes.FirstOrDefault(c =>
                    c.Nom.Equals(nom, StringComparison.OrdinalIgnoreCase));

                if (categorieExistante != null)
                {
                    // Mettre à jour
                    categorieExistante.Description = record.Description;
                    categorieExistante.Couleur = record.Couleur ?? categorieExistante.Couleur;
                    categorieExistante.Icone = record.Icone ?? categorieExistante.Icone;

                    await _unitOfWork.Categories.UpdateAsync(categorieExistante);
                    updated++;
                }
                else
                {
                    var categorie = new Categorie
                    {
                        Nom = nom,
                        Description = record.Description,
                        Couleur = record.Couleur ?? "#6c757d",
                        Icone = record.Icone ?? "bi-book",
                        Actif = true
                    };

                    await _unitOfWork.Categories.AddAsync(categorie);
                    categoriesExistantes.Add(categorie);
                    count++;
                }
            }

            await _unitOfWork.SaveChangesAsync();
            return (count, updated);
        }

        private async Task<(int created, int updated)> ImportUtilisateursAsync(CsvReader csv)
        {
            var records = csv.GetRecords<dynamic>().ToList();
            int count = 0;
            int updated = 0;

            var utilisateursExistants = (await _unitOfWork.Utilisateurs.GetAllAsync()).ToList();

            foreach (var record in records)
            {
                string email = record.Email?.ToString() ?? "";

                // Vérifier si l'utilisateur existe déjà (par email)
                var userExistant = utilisateursExistants.FirstOrDefault(u =>
                    u.Email.Equals(email, StringComparison.OrdinalIgnoreCase));

                if (userExistant != null)
                {
                    // Mettre à jour (sauf mot de passe)
                    userExistant.Nom = record.Nom;
                    userExistant.Prenom = record.Prenom;
                    userExistant.Telephone = record.Telephone;
                    userExistant.Adresse = record.Adresse;
                    userExistant.DateNaissance = DateTime.TryParse(record.DateNaissance?.ToString(), out DateTime dateN) ? dateN : null;

                    await _unitOfWork.Utilisateurs.UpdateAsync(userExistant);
                    updated++;
                }
                else
                {
                    string motDePasse = record.MotDePasse?.ToString() ?? "password123";

                    var user = new Utilisateur
                    {
                        Nom = record.Nom,
                        Prenom = record.Prenom,
                        Email = email,
                        Telephone = record.Telephone,
                        Adresse = record.Adresse,
                        DateNaissance = DateTime.TryParse(record.DateNaissance?.ToString(), out DateTime dateN) ? dateN : null,
                        MotDePasseHash = BCrypt.Net.BCrypt.HashPassword(motDePasse),
                        DateInscription = DateTime.Now,
                        Actif = true,
                        EstBloque = false
                    };

                    await _unitOfWork.Utilisateurs.AddAsync(user);
                    utilisateursExistants.Add(user);
                    count++;
                }
            }

            await _unitOfWork.SaveChangesAsync();
            return (count, updated);
        }

        private async Task GenerateLivresPdfAsync(Document document)
        {
            var livres = await _unitOfWork.Livres.GetAllAsync();
            var table = new Table(5).UseAllAvailableWidth();

            table.AddHeaderCell("Titre");
            table.AddHeaderCell("Auteur");
            table.AddHeaderCell("ISBN");
            table.AddHeaderCell("Année");
            table.AddHeaderCell("Stock");

            foreach (var livre in livres.Where(l => l.Actif).OrderBy(l => l.Titre))
            {
                table.AddCell(livre.Titre);
                table.AddCell(livre.Auteur?.NomComplet ?? "");
                table.AddCell(livre.ISBN ?? "");
                table.AddCell(livre.Annee?.ToString() ?? "");
                table.AddCell($"{livre.StockDisponible}/{livre.Stock}");
            }

            document.Add(table);
        }

        private async Task GenerateEmpruntsPdfAsync(Document document)
        {
            var emprunts = await _unitOfWork.Emprunts.GetAllWithDetailsAsync();
            var empruntsActifs = emprunts.Where(e => e.Statut == "EnCours" || e.Statut == "EnRetard").ToList();

            document.Add(new Paragraph($"Emprunts en cours : {empruntsActifs.Count}").SetFontSize(14));
            document.Add(new Paragraph(" "));

            var table = new Table(5).UseAllAvailableWidth();

            table.AddHeaderCell("Livre");
            table.AddHeaderCell("Utilisateur");
            table.AddHeaderCell("Date emprunt");
            table.AddHeaderCell("Retour prévu");
            table.AddHeaderCell("Statut");

            foreach (var emprunt in empruntsActifs.OrderBy(e => e.DateRetourPrevue))
            {
                table.AddCell(emprunt.Livre?.Titre ?? "");
                table.AddCell($"{emprunt.Utilisateur?.Nom} {emprunt.Utilisateur?.Prenom}");
                table.AddCell(emprunt.DateEmprunt.ToString("dd/MM/yyyy"));
                table.AddCell(emprunt.DateRetourPrevue.ToString("dd/MM/yyyy"));
                table.AddCell(emprunt.Statut ?? "");
            }

            document.Add(table);
        }

        private async Task GenerateStatistiquesPdfAsync(Document document)
        {
            var livres = await _unitOfWork.Livres.GetAllAsync();
            var emprunts = await _unitOfWork.Emprunts.GetAllAsync();
            var users = await _unitOfWork.Utilisateurs.GetAllAsync();

            document.Add(new Paragraph("Statistiques générales").SetFontSize(16));
            document.Add(new Paragraph(" "));

            var statsTable = new Table(2).UseAllAvailableWidth();
            statsTable.AddCell("Total livres");
            statsTable.AddCell(livres.Count(l => l.Actif).ToString());
            statsTable.AddCell("Total utilisateurs");
            statsTable.AddCell(users.Count().ToString());
            statsTable.AddCell("Emprunts en cours");
            statsTable.AddCell(emprunts.Count(e => e.Statut == "EnCours").ToString());
            statsTable.AddCell("Emprunts en retard");
            statsTable.AddCell(emprunts.Count(e => e.Statut == "EnRetard").ToString());
            statsTable.AddCell("Total emprunts ce mois");
            statsTable.AddCell(emprunts.Count(e => e.DateEmprunt.Month == DateTime.Now.Month && e.DateEmprunt.Year == DateTime.Now.Year).ToString());

            document.Add(statsTable);
        }
    }
}
