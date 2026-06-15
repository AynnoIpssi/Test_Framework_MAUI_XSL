// Fichier : .../Models/AppiumElementScannerOptions.cs
namespace BaobaTesterBox.Core.Config.Models;

public sealed class AppiumElementScannerOptions
{
    /// <summary>
    /// La balise HTML racine à partir de laquelle le scan commence (ex: "body", "app").
    /// </summary>
    public string RootContainerTagName { get; set; } = "body";
}