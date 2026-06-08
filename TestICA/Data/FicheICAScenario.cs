namespace TestICA.Data
{
    /// <summary>
    /// Représente les données d'un scénario de test pour la Fiche ICA.
    /// Chaque champ correspond à un champ du formulaire.
    /// </summary>
    public class FicheICAScenario
    {
        public string? TestId { get; set; }
        public string? Description { get; set; }
        public string? ResultatAttendu { get; set; }

        // Données du formulaire
        public string? Lot { get; set; }              // Valeur du select lot
        public string? TypeLot { get; set; }          // planned / technical / setup
        public string? DateFiche { get; set; }        // Format YYYY-MM-DD
        public string? DateEnlevement { get; set; }   // Format YYYY-MM-DD
        public string? DateReception { get; set; }    // Format YYYY-MM-DD
        public string? Abattoir { get; set; }         // Nom de l'abattoir
        public string? NbMale { get; set; }
        public string? PoidsMale { get; set; }
        public string? NbFemelle { get; set; }
        public string? PoidsFemelle { get; set; }
        public string? NbTV { get; set; }             // Tout venant
        public string? PoidsTV { get; set; }
        public string? EtatGeneral { get; set; }
        public string? BruluresTarse { get; set; }
        public string? LesionsBrechet { get; set; }
        public string? Griffures { get; set; }
        public string? Homogeneite { get; set; }
        public string? Observations { get; set; }
        public bool MiseAJeun { get; set; }
        public bool AvecSignature { get; set; }

        // Données attendues en base après sauvegarde
        public string? TableAttendue { get; set; }
        public string? AlerteAttendue { get; set; }
        public bool SauvegardeAttendue { get; set; }
    }
}