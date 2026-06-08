namespace TestICA.Data
{
    /// <summary>
    /// Représente les données d'un scénario de test pour le Login.
    /// </summary>
    public class LoginScenarios
    {
        public string? TestId { get; set; }
        public string? Description { get; set; }
        public string? ResultatAttendu { get; set; }

        // Données d'authentification
        public string? Email { get; set; }
        public string? Password { get; set; }

        // Validation (pour les futurs TCs en erreur ou autre)
        public bool DevraitReussir { get; set; }
        public string? MessageErreurAttendu { get; set; }
    }
}