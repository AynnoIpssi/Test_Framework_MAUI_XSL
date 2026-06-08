using NUnit.Framework;
using Serilog;
using OpenQA.Selenium;
using TestICA.Core.Starter;
using TestICA.Core.SmartLocator;

namespace TestICA.Tests;

[TestFixture]
public class DiagnosticWebViewTest : AppiumStarter
{
    [Test]
    public void Analyser_Et_Afficher_Tous_Les_XPath()
    {
        Log.Information("=== [DEBUT] Diagnostic de la Page WebView ===");

        // 1. Initialisation du Driver
        InitDriver();

        // 2. Bascule forcée dans le monde Web
        BasculeVersWebView();

        // 3. Init de ton outil de cartographie
        SmartLocator.Init(Driver!);

        // 4. On laisse 3 secondes pour s'assurer que le HTML est totalement stabilisé
        System.Threading.Thread.Sleep(3000);

        Log.Information("[Diagnostic] Récupération du code source HTML brut...");
        try
        {
            string htmlBrut = Driver.PageSource;
            Log.Information($"[Diagnostic] Taille du HTML récupéré : {htmlBrut.Length} caractères.");
        }
        catch (Exception ex)
        {
            Log.Error($"[Diagnostic] Impossible de lire le PageSource : {ex.Message}");
        }

        // 5. Exécution du scan de ton SmartLocator
        Log.Information("[Diagnostic] Lancement du scan de la page...");
        SmartLocator.AnalyzeCurrentPage();

        // 6. Extraction et affichage propre dans les logs
        var identifiants = SmartLocator.GetLastDetectedIds();
        
        Log.Information("=======================================================================");
        Log.Information($" CARTOGRAPHIE FINALE : {identifiants.Count} ÉLÉMENTS TROUVÉS");
        Log.Information("=======================================================================");

        foreach (var id in identifiants)
        {
            // On récupère l'élément du cache pour essayer de reconstruire sa structure si besoin,
            // mais ton SmartLocator loggue déjà le XPath en bleu ("Indexé -> Clé...") dans le terminal.
            Log.Information($" ID SmartLocator : {id}");
        }

        Log.Information("=======================================================================");
        Log.Information("=== [FIN] Fin du test de diagnostic — Analyse les logs ci-dessus ===");
    }
}