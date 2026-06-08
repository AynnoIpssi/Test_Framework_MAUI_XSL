namespace TestICA.Core.Config.Models;

public class GeneratedModel
{
    public string RawId { get; set; } = string.Empty;
    public string MethodName { get; set; } = string.Empty; // Nom propre (ex: "Valider")
    public string ActionType { get; set; } = string.Empty; // "Click" ou "FillField"
    public bool IsInput { get; set; }
}