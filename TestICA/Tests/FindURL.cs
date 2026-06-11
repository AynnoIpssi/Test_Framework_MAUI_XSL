using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using OpenQA.Selenium;
using OpenQA.Selenium.Appium;
using OpenQA.Selenium.Appium.Android;
using OpenQA.Selenium.Support.UI;
using Serilog;
using TestICA.Core.Config;
using TestICA.Core.Starter;
using TestICA.Core.SmartLocator;
using TestICA.Core.SmartWaiter;
using TestICA.Core.JsStateReader;
using TestICA.Core.JsNavigator;

namespace TestICA.Tests;

[TestFixture]
public class FindURL : AppiumStarter
{
    [SetUp]
    public void SetUpNavigationTest()
    {
        Log.Information("[SetUp] Initialisation automatique du Driver et bascule WebView...");
        InitDriver();
        BasculeVersWebView();
    }

    [Test]
    public void Test_Navigation_Vers_Carte_Via_Clic_Permissif()
    {
        Log.Information("[Test-Carte] Démarrage du scénario de navigation par mot-clé...");
        
        if (Driver == null)
        {
            Assert.Fail("[Test-Carte] ❌ Le 'Driver' est null.");
        }

        var jsExecutor = (IJavaScriptExecutor)Driver;
        var stateReader = new JsStateReader(Driver);

        // =========================================================================
        // 1. TIMING & SÉCURITÉ : On attend que la liste soit bien rendue graphiquement
        // =========================================================================
        System.Threading.Thread.Sleep(2000); 

        // =========================================================================
        // 2. CONFIGURATION DU MOT-CLÉ VISIBLE
        // =========================================================================
        // Mets ici un mot-clé UNIQUE et SIMPLE présent dans le nom de la ferme visible à l'écran
        // Exemple : "LISQUILLY", "CHAMP", "GRAND", etc.
        string motCleExploitation = "LISQUILLY"; 
        string rubriqueCible = "642";

        Log.Information($"[Test-Carte] Recherche permissive de l'élément contenant : '{motCleExploitation}'...");

        string scriptSequenceClic = $@"
            (function() {{
                var token = '{motCleExploitation.ToLower()}';
                var target = null;
                
                // Récupération de tous les éléments du DOM
                var elements = document.getElementsByTagName('*');
                
                for (var i = 0; i < elements.length; i++) {{
                    var text = elements[i].textContent;
                    
                    if (text && text.toLowerCase().includes(token)) {{
                        // On cherche l'élément le plus bas/profond dans l'arbre DOM (pas un parent global)
                        if (elements[i].children.length === 0 || elements[i].tagName === 'SPAN' || elements[i].tagName === 'DIV') {{
                            target = elements[i];
                            // Si on trouve une correspondance parfaite sur un élément enfant, on s'arrête
                            if (text.trim().toLowerCase() === token) break; 
                        }}
                    }}
                }}
                
                if (!target) {{
                    throw new Error('Aucun élément trouvé dans le DOM contenant le mot-clé : ' + token);
                }}

                var tagName = target.tagName;
                var targetId = target.id || 'Aucun ID';

                // Génération de la séquence d'événements tactiles alternatifs
                var pointerDown = new PointerEvent('pointerdown', {{ bubbles: true, cancelable: true, pointerType: 'touch', isPrimary: true }});
                var pointerUp = new PointerEvent('pointerup', {{ bubbles: true, cancelable: true, pointerType: 'touch', isPrimary: true }});
                var clickEvent = new MouseEvent('click', {{ bubbles: true, cancelable: true, view: window, detail: 1 }});
                
                // Envoi de la séquence sur la cible principale
                target.dispatchEvent(pointerDown);
                target.dispatchEvent(pointerUp);
                var dispatchResult = target.dispatchEvent(clickEvent);
                
                // Envoi de secours sur le parent direct si le clic principal n'a pas déclenché d'action native
                if (target.parentElement) {{
                    target.parentElement.dispatchEvent(pointerDown);
                    target.parentElement.dispatchEvent(pointerUp);
                    target.parentElement.dispatchEvent(clickEvent);
                }}
                
                return 'Cible identifiée: <' + tagName + '> (ID: ' + targetId + '). Événements injectés.';
            }})();
        ";

        try
        {
            string reponseMoteur = (string)jsExecutor.ExecuteScript(scriptSequenceClic);
            Log.Information($"[Test-Carte] ✅ Réponse WebView : {reponseMoteur}");
        }
        catch (Exception ex)
        {
            Assert.Fail($"[Test-Carte] ❌ Échec de la détection/simulation JS : {ex.Message}");
        }

        // =========================================================================
        // 3. VALIDATION DE LA REDIRECTION
        // =========================================================================
        Log.Information("[Test-Carte] Attente de la transition d'écran (3 secondes)...");
        System.Threading.Thread.Sleep(3000);

        var stateFinal = stateReader.ReadCurrentState();
        if (stateFinal == null)
        {
            Assert.Fail("[Test-Carte] ❌ Impossible de lire l'état de l'application après injection.");
        }

        Log.Information($"[Test-Carte] Position finale détectée -> Page: [{stateFinal.OldRubriqueId}] {stateFinal.PageNom}");

        if (stateFinal.OldRubriqueId == rubriqueCible)
        {
            Log.Information("=======================================================================");
            Log.Information($"🎉 SUCCÈS ! Redirection réussie vers la Carte [{rubriqueCible}].");
            Log.Information("=======================================================================");
            Assert.Pass($"L'application a migré vers la rubrique {rubriqueCible}.");
        }
        else
        {
            Assert.Fail($"[Test-Carte] ❌ Redirection manquante ou incomplète. L'application est restée sur : [{stateFinal.OldRubriqueId}] {stateFinal.PageNom}");
        }
    }
}