using System;
using System.Linq;
using NUnit.Framework;
using OpenQA.Selenium;
using Serilog;
using TestICA.Core.Starter;
using TestICA.Core.SmartLocator;

namespace TestICA.Tests;

[TestFixture]
public class ScannPage: AppiumStarter
{
    [Test]
    public void Lancer_Analyse_Profonde_WebView()
    {
        // Ces lignes écrivent partout (Console NUnit + Logs)
        Console.WriteLine("=======================================================================");
        Console.WriteLine("=== [DÉBUT] EXTRACTION DES PARAMÈTRES DU BODY (INPUTS JS) ===");
        Console.WriteLine("=======================================================================");
        Log.Information("=== [DÉBUT] EXTRACTION DES PARAMÈTRES DU BODY (INPUTS JS) ===");

        try
        {
            // 1. Initialisation standard du Driver Appium
            InitDriver();
            System.Threading.Thread.Sleep(3000); // Sécurité pour laisser l'application charger

            // 2. Bascule indispensable vers la WebView
            Console.WriteLine("[Robot] Bascule vers le contexte WebView...");
            BasculeVersWebView();
            Console.WriteLine($"[Robot] Contexte actuel : {Driver!.Context}");

            // 3. Script JS pour capturer l'intégralité des inputs/paramètres du body
            var jsExecutor = (IJavaScriptExecutor)Driver;
            
            string scriptExtractionPure = @"
                var inputs = document.querySelectorAll('input, select, textarea');
                var lignes = [];
                
                lignes.push('--- [TOUS LES CHAMPS DE SAISIE DETECTÉS DANS LE BODY] ---');
                
                inputs.forEach(function(el) {
                    // On filtre les boutons pour n'avoir que les variables et les données
                    if (el.type === 'button' || el.type === 'submit' || el.type === 'reset') return;
                    
                    var id = el.id || el.name || '[SANS ID/NAME]';
                    var val = el.value || '';
                    
                    // Aligner proprement le texte (35 caractères de large pour l'ID)
                    var idFormate = id.padEnd(35);
                    
                    lignes.push('ID: ' + idFormate + ' | Valeur: \'' + val + '\'');
                });
                
                return lignes.join('\n');
            ";

            Console.WriteLine("[Robot] Scan de la page en cours via injection JS...");
            string resultatParametresBody = jsExecutor.ExecuteScript(scriptExtractionPure).ToString();
            
            // 4. FORÇAGE DE L'AFFICHAGE DANS L'OUTPUT DE NUNIT
            // On découpe par ligne pour être sûr que la console de test ne coupe pas le texte
            string[] lignesResultat = resultatParametresBody.Split('\n');
            
            Console.WriteLine("\n📊 --- ENTRÉE DU RAPPORT DANS L'OUTPUT DE TEST ---");
            foreach (var ligne in lignesResultat)
            {
                // Écrit dans l'onglet 'Output' du test sous Visual Studio / Rider
                Console.WriteLine(ligne); 
                
                // Garde aussi une copie propre dans tes fichiers de logs Serilog au cas où
                Log.Information(ligne); 
            }
            Console.WriteLine("📊 --- FIN DU RAPPORT ---\n");

        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Erreur affichée dans la console : {ex.Message}");
            Log.Error($"❌ Une erreur est survenue durant l'extraction : {ex.Message}");
        }

        Console.WriteLine("=======================================================================");
        Console.WriteLine("=== [FIN] Extraction terminée. Tu peux copier le bloc ci-dessus ===");
        Console.WriteLine("=======================================================================");
    }
}