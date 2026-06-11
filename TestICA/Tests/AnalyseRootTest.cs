using System;
using NUnit.Framework;
using OpenQA.Selenium;
using Serilog;
using TestICA.Core.Starter;

namespace TestICA.Tests;

[TestFixture]
public class AuditRoutageTests : AppiumStarter
{
    [SetUp]
    public void SetUpAuditTest()
    {
        Log.Information("[Capture-Propre] Initialisation du Driver et bascule WebView...");
        InitDriver();
        BasculeVersWebView();
    }

    [Test]
    public void Test_Intercepter_Evenements_Routage_En_Direct()
    {
        Log.Information("[Capture-Propre] Injection de l'enregistreur de clics en texte brut...");
        
        if (Driver == null)
        {
            Assert.Fail("[Capture-Propre] ❌ Le 'Driver' est null.");
        }

        var jsExecutor = (IJavaScriptExecutor)Driver;
        System.Threading.Thread.Sleep(2000); 

        // Ce script écoute les clics et génère des lignes de texte simples (pas d'objets complexes, pas de JSON)
        string scriptInjectionEspionPropre = @"
            (function() {
                window.clickLogsBruts = [];
                
                window.addEventListener('click', function(event) {
                    var el = event.target;
                    if (!el) return;
                    
                    var balise = el.tagName;
                    var id = el.id || 'Aucun ID';
                    var classe = el.className || 'Aucune Classe';
                    var texte = (el.textContent || el.value || '[Icône/Image]').trim().substring(0, 30);
                    
                    // Construction d'une ligne de texte propre
                    var ligneLog = '👉 BOUTON DE REDIRECTION -> Balise: <' + balise + '> | ID: ' + id + ' | Class: ' + classe + ' | Texte: ' + texte;
                    
                    window.clickLogsBruts.push(ligneLog);
                }, true);
                
                return 'Espion texte brut opérationnel.';
            })();
        ";

        try
        {
            string statut = (string)jsExecutor.ExecuteScript(scriptInjectionEspionPropre);
            Log.Information($"[Capture-Propre] ✅ Statut : {statut}");
        }
        catch (Exception ex)
        {
            Assert.Fail($"[Capture-Propre] ❌ Échec injection : {ex.Message}");
        }

        // =========================================================================
        // ⏱️ FENÊTRE DE MANIPULATION CHRONO (12 Secondes)
        // =========================================================================
        Log.Warning("⚠️ CHRONO LANCÉ (12s) : Appuie sur le bouton de ton téléphone qui ouvre la page 642 !");
        System.Threading.Thread.Sleep(12000);

        Log.Information("[Capture-Propre] Fin du chrono. Récupération des lignes de texte...");

        // Script pour récupérer le tableau de chaînes de caractères
        string scriptRecupLogsPropre = @"
            (function() {
                if (!window.clickLogsBruts || window.clickLogsBruts.length === 0) {
                    return 'Aucun clic n\'a été capturé par l\'espion. As-tu bien cliqué sur l\'écran ?';
                }
                // On joint les lignes avec un retour à la ligne pour que ce soit lisible directement
                return window.clickLogsBruts.join('\n');
            })();
        ";

        string texteAfficheDansConsole = string.Empty;
        try
        {
            texteAfficheDansConsole = (string)jsExecutor.ExecuteScript(scriptRecupLogsPropre);
        }
        catch (Exception ex)
        {
            Assert.Fail($"[Capture-Propre] ❌ Erreur lors de la récupération : {ex.Message}");
        }

        // =========================================================================
        // AFFICHAGE FINAL SÉCURISÉ (Sorti du bloc try/catch pour Rider)
        // =========================================================================
        Log.Information("=======================================================================");
        Log.Information("🕵️ SIGNATURE TEXTUELLE DU COMPOSANT CLIQUÉ");
        Log.Information("=======================================================================");
        Log.Information(texteAfficheDansConsole);
        Log.Information("=======================================================================");

        Assert.Pass("Scan terminé. Les détails du bouton sont affichés juste au-dessus !");
    }
}