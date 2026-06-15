namespace BaobaTesterBox.Domain.Models;

public class AppiumNavigationOptions
{
    // --- Paramètres Communs ---
    public int RubriqueId { get; set; }

    // --- Paramètres spécifiques à "go" (Navigation sans module) ---
    public int ActionId { get; set; } = 1;
    public string Param1 { get; set; } = string.Empty;
    public string Param2 { get; set; } = string.Empty;
    public string Param3 { get; set; } = string.Empty;
    public string Param4 { get; set; } = string.Empty;

    // --- Paramètres spécifiques à "openPoultryModal" (Navigation avec module) ---
    public int? ModuleId { get; set; }
    public int? BatchId { get; set; }
}