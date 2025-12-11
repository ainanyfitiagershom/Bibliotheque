namespace Bibliotheque.Core.Enums
{
    public enum StatutEmprunt
    {
        EnCours,
        Termine,
        EnRetard
    }

    public enum StatutReservation
    {
        EnAttente,
        Disponible,
        Annulee,
        Convertie
    }

    public enum TypeNotification
    {
        Retard,
        Disponibilite,
        Rappel,
        Systeme,
        Bienvenue
    }

    public enum ActionHistorique
    {
        Insert,
        Update,
        Delete
    }

    public enum TriLivre
    {
        Titre,
        Annee,
        Popularite,
        Note,
        Recent
    }
}
