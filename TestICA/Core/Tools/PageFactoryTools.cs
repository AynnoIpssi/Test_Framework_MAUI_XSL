using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Globalization;
using TestICA.Core.Config.Models;

namespace TestICA.Core.Tools;

public static class PageFactoryTools
{
    /// <summary>
    /// Génère automatiquement le fichier de page C# en analysant les IDs détectés.
    /// </summary>
    /// <param name="pageName">Nom de la page à créer (ex: "Connexion")</param>
    /// <param name="detectedIds">Liste des IDs bruts récupérés par le SmartLocator</param>
    /// <param name="outputDirectory">Le chemin du dossier Pages de ton projet</param>
    public static void CreatePageFile(string pageName, List<string> detectedIds, string outputDirectory)
    {
        // Correction ici : Utilisation du bon type 'GeneratedModel'
        var methodsToGenerate = new List<GeneratedModel>();

        // 1. Analyse et nettoyage de chaque ID
        foreach (var id in detectedIds)
        {
            if (string.IsNullOrWhiteSpace(id)) continue;

            methodsToGenerate.Add(AnalyzeAndCleanId(id));
        }

        // 2. Assemblage du code C#
        string classCode = AssembleClassCode(pageName, methodsToGenerate);

        // 3. Écriture du fichier sur le disque
        if (!Directory.Exists(outputDirectory))
        {
            Directory.CreateDirectory(outputDirectory);
        }

        string finalPath = Path.Combine(outputDirectory, $"{pageName}Page.cs");
        File.WriteAllText(finalPath, classCode, Encoding.UTF8);

        Console.WriteLine($"[FACTORY] 🚀 La page '{pageName}Page.cs' a été générée proprement.");
    }

    /// <summary>
    /// Détecte le type d'action (clic/saisie) et nettoie les préfixes/index chiffrés.
    /// </summary>
    private static GeneratedModel AnalyzeAndCleanId(string id)
    {
        string lowerId = id.ToLower();
        bool isInput = lowerId.Contains("input") || lowerId.Contains("txt") || lowerId.Contains("field");

        // Nettoyage de la pollution (remplace les tirets et underscores par des espaces)
        string cleaned = id.Replace("_", " ").Replace("-", " ");
        string[] words = cleaned.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
        
        var textInfo = CultureInfo.InvariantCulture.TextInfo;
        string cleanName = "";

        foreach (var word in words)
        {
            string lowerWord = word.ToLower();
            // On zappe les mots techniques polluants et les numéros dynamiques (ex: _23, _1)
            if (lowerWord is "button" or "btn" or "input" or "txt" or "field" || int.TryParse(word, out _))
            {
                continue;
            }
            cleanName += textInfo.ToTitleCase(word);
        }

        // Sécurité si l'ID ne contenait que de la pollution
        if (string.IsNullOrEmpty(cleanName))
        {
            cleanName = "Element" + textInfo.ToTitleCase(id).Replace(" ", "");
        }

        // Correction ici : Instanciation du bon modèle 'GeneratedModel'
        return new GeneratedModel
        {
            RawId = id,
            MethodName = cleanName,
            ActionType = isInput ? "FillField" : "Click",
            IsInput = isInput
        };
    }

    /// <summary>
    /// Génère le squelette textuel du fichier C# final avec l'héritage de PageFunction.
    /// </summary>
    private static string AssembleClassCode(string pageName, List<GeneratedModel> methods)
    {
        var sb = new StringBuilder();

        // Header et Usings du fichier généré
        sb.AppendLine("using System;");
        sb.AppendLine("using OpenQA.Selenium.Appium.Android;");
        sb.AppendLine("using TestICA.Core;");
        sb.AppendLine();
        sb.AppendLine("namespace TestICA.Pages;");
        sb.AppendLine();
        
        // Déclaration de la classe avec héritage direct de ta classe de base PageFunction
        sb.AppendLine($"public class {pageName}Page : PageFunction");
        sb.AppendLine("{");
        
        // Constructeur qui passe le driver à la classe mère
        sb.AppendLine($"    public {pageName}Page(AndroidDriver driver) : base(driver) {{ }}");
        sb.AppendLine();
        sb.AppendLine("    // ================= ACTIONS DYNAMIQUES =================");
        sb.AppendLine();

        // Génération des méthodes (Correction "Ambiguous invocation" via l'échappement strict des guillemets)
        foreach (var method in methods)
        {
            if (method.IsInput)
            {
                sb.AppendLine($"    public void Fill{method.MethodName}(string text)");
                sb.AppendLine("    {");
                sb.AppendLine($"        FillField(\"{method.RawId}\", text);");
                sb.AppendLine("    }");
            }
            else
            {
                sb.AppendLine($"    public void Click{method.MethodName}()");
                sb.AppendLine("    {");
                sb.AppendLine($"        Click(\"{method.RawId}\");");
                sb.AppendLine("    }");
            }
            sb.AppendLine();
        }

        sb.AppendLine("}");
        return sb.ToString();
    }
}