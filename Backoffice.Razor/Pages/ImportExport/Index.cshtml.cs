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

        public List<ImportRecord> ImportHistory { get; set; } = new();

        public void OnGet()
        {
            // Charger l'historique depuis TempData ou session
            if (TempData["ImportHistory"] is string historyJson)
            {
                ImportHistory = System.Text.Json.JsonSerializer.Deserialize<List<ImportRecord>>(historyJson) ?? new();
            }
        }

        public async Task<IActionResult> OnPostImportAsync(string type, IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                TempData["Error"] = "Veuillez sélectionner un fichier.";
                return RedirectToPage();
            }

            var importRecord = new ImportRecord
            {
                Date = DateTime.Now,
                Type = type,
                Filename = file.FileName
            };

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

                switch (type)
                {
                    case "livres":
                        count = await ImportLivresAsync(csv);
                        break;
                    case "auteurs":
                        count = await ImportAuteursAsync(csv);
                        break;
                    case "categories":
                        count = await ImportCategoriesAsync(csv);
                        break;
                    case "utilisateurs":
                        count = await ImportUtilisateursAsync(csv);
                        break;
                }

                importRecord.Success = true;
                importRecord.Count = count;
                TempData["Success"] = $"{count} enregistrement(s) importé(s) avec succès.";
            }
            catch (Exception ex)
            {
                importRecord.Success = false;
                TempData["Error"] = $"Erreur lors de l'import : {ex.Message}";
            }

            // Sauvegarder l'historique
            var history = new List<ImportRecord> { importRecord };
            TempData["ImportHistory"] = System.Text.Json.JsonSerializer.Serialize(history);

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

        private async Task<int> ImportLivresAsync(CsvReader csv)
        {
            var records = csv.GetRecords<dynamic>().ToList();
            int count = 0;

            foreach (var record in records)
            {
                int idAuteur = 1;
                int? annee = null;
                int stock = 1;

                int.TryParse(record.IdAuteur?.ToString(), out idAuteur);
                if (int.TryParse(record.Annee?.ToString(), out int anneeValue))
                {
                    annee = anneeValue;
                }
                int.TryParse(record.Stock?.ToString(), out stock);

                var livre = new Livre
                {
                    ISBN = record.ISBN,
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

            await _unitOfWork.SaveChangesAsync();
            return count;
        }

        private async Task<int> ImportAuteursAsync(CsvReader csv)
        {
            var records = csv.GetRecords<dynamic>().ToList();
            int count = 0;

            foreach (var record in records)
            {
                var auteur = new Auteur
                {
                    Nom = record.Nom,
                    Prenom = record.Prenom,
                    Nationalite = record.Nationalite,
                    DateNaissance = DateTime.TryParse(record.DateNaissance?.ToString(), out DateTime date) ? date : null,
                    Actif = true
                };

                await _unitOfWork.Auteurs.AddAsync(auteur);
                count++;
            }

            await _unitOfWork.SaveChangesAsync();
            return count;
        }

        private async Task<int> ImportCategoriesAsync(CsvReader csv)
        {
            var records = csv.GetRecords<dynamic>().ToList();
            int count = 0;

            foreach (var record in records)
            {
                var categorie = new Categorie
                {
                    Nom = record.Nom,
                    Description = record.Description,
                    Couleur = record.Couleur ?? "#6c757d",
                    Actif = true
                };

                await _unitOfWork.Categories.AddAsync(categorie);
                count++;
            }

            await _unitOfWork.SaveChangesAsync();
            return count;
        }

        private async Task<int> ImportUtilisateursAsync(CsvReader csv)
        {
            var records = csv.GetRecords<dynamic>().ToList();
            int count = 0;

            foreach (var record in records)
            {
                var user = new Utilisateur
                {
                    Nom = record.Nom,
                    Prenom = record.Prenom,
                    Email = record.Email,
                    Telephone = record.Telephone,
                    MotDePasseHash = BCrypt.Net.BCrypt.HashPassword("password123"),
                    DateInscription = DateTime.Now,
                    Actif = true,
                    EstBloque = false
                };

                await _unitOfWork.Utilisateurs.AddAsync(user);
                count++;
            }

            await _unitOfWork.SaveChangesAsync();
            return count;
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

        public class ImportRecord
        {
            public DateTime Date { get; set; }
            public string Type { get; set; } = string.Empty;
            public string Filename { get; set; } = string.Empty;
            public bool Success { get; set; }
            public int Count { get; set; }
        }
    }
}
