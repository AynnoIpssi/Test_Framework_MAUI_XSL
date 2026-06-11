using System;
using System.Collections.Generic;
using NUnit.Framework;
using OpenQA.Selenium;
using Serilog;
using TestICA.Core.Starter;
using TestICA.Core.JsStateReader;

namespace TestICA.Tests;

[TestFixture]
public class NavigationTests : AppiumStarter
{
    [SetUp]
    public void SetUpNavigationTest()
    {
        Log.Information("[SetUp] Initialisation automatique du Driver et bascule WebView...");
        InitDriver();
        BasculeVersWebView();
    }

    /// <summary>
    /// Méthode magique : Analyse le DOM actuel, trouve l'élément lié à la rubrique demandée et clique dessus.
    /// </summary>
    private void CliquerSurRubriqueViaAnalyse(IJavaScriptExecutor jsExecutor, string numeroRubrique)
    {
        Log.Information($"[AnalyseRoot] Lancement du scanner pour intercepter la rubrique [{numeroRubrique}]...");

        string scriptScannerEtClic = $@"
            (function() {{
                var targetRubrique = '{numeroRubrique}';
                var cibleHtml = null;
                var detailsTrouves = '';

                var elements = document.getElementsByTagName('*');
                
                for (var i = 0; i < elements.length; i++) {{
                    var el = elements[i];
                    
                    // 1. Analyse de l'attribut onclick (ex: window.go(642, ...))
                    var onclickAttr = el.getAttribute('onclick') || '';
                    if (onclickAttr && onclickAttr.includes(targetRubrique)) {{
                        cibleHtml = el;
                        detailsTrouves = 'Attribut onclick détecté (' + onclickAttr + ')';
                        break;
                    }}
                    
                    // 2. Analyse des attributs de données du framework (data-rubrique, data-target, etc.)
                    if (el.dataset) {{
                        if (el.dataset.rubrique === targetRubrique || el.dataset.id === targetRubrique || el.dataset.target === targetRubrique) {{
                            cibleHtml = el;
                            detailsTrouves = 'Attribut HTML dataset détecté (id/rubrique/target)';
                            break;
                        }}
                    }}
                    
                    // 3. Analyse des liens href (ex: ?rubrique=642)
                    var hrefAttr = el.getAttribute('href') || '';
                    if (hrefAttr && hrefAttr.includes('rubrique=' + targetRubrique)) {{
                        cibleHtml = el;
                        detailsTrouves = 'Lien href détecté (' + hrefAttr + ')';
                        break;
                    }}
                }}

                if (!cibleHtml) {{
                    throw new Error('Analyse Root Échouée : Aucun élément de la page actuelle ne redirige vers la rubrique ' + targetRubrique);
                }}

                // Envoi de la séquence tactile ultra-permissive (Bubbling activé)
                var pDown = new PointerEvent('pointerdown', {{ bubbles: true, cancelable: true, pointerType: 'touch', isPrimary: true }});
                var pUp = new PointerEvent('pointerup', {{ bubbles: true, cancelable: true, pointerType: 'touch', isPrimary: true }});
                var click = new MouseEvent('click', {{ bubbles: true, cancelable: true, view: window, detail: 1 }});
                
                cibleHtml.dispatchEvent(pDown);
                cibleHtml.dispatchEvent(pUp);
                var result = cibleHtml.dispatchEvent(click);
                
                var nomBalise = cibleHtml.tagName;
                var texteAssocie = (cibleHtml.textContent || cibleHtml.value || '[Aucun texte/Icône]').trim().substring(0, 20);
                
                return 'Succès : Élément <' + nomBalise + '> (' + texteAssocie + ') trouvé via [' + detailsTrouves + ']. Séquence tactile injectée.';
            }})();
        ";

        try
        {
            string reponseWebView = (string)jsExecutor.ExecuteScript(scriptScannerEtClic);
            Log.Information($"[AnalyseRoot] ✅ {reponseWebView}");
        }
        catch (Exception ex)
        {
            Assert.Fail($"[AnalyseRoot] ❌ Erreur critique lors du scan/clic : {ex.Message}");
        }
    }

    [Test]
    public void Test_Navigation_Intelligente_AnalyseRoot()
    {
        Log.Information("[Test-Root] Début du scénario 100% basé sur le routage dynamique...");
        
        if (Driver == null)
        {
            Assert.Fail("[Test-Root] ❌ Le 'Driver' est null.");
        }

        var jsExecutor = (IJavaScriptExecutor)Driver;
        var stateReader = new JsStateReader(Driver);

        // Attente de stabilisation de la page de départ (580 - Carte globale)
        System.Threading.Thread.Sleep(2000); 

        // =========================================================================
        // ACTION : Clique sur le bouton de l'écran 580 qui pointe vers la rubrique 642
        // =========================================================================
        CliquerSurRubriqueViaAnalyse(jsExecutor, "642");

        // Attente de la transition d'écran
        Log.Information("[Test-Root] Clic envoyé. Attente du chargement de la liste des fermes (3 secondes)...");
        System.Threading.Thread.Sleep(3000);

        // Validation de l'état final
        var stateFinal = stateReader.ReadCurrentState();
        if (stateFinal == null)
        {
            Assert.Fail("[Test-Root] ❌ Impossible de lire l'état de l'application.");
        }

        Log.Information($"[Test-Root] Position finale détectée -> Page: [{stateFinal.OldRubriqueId}] {stateFinal.PageNom}");

        if (stateFinal.OldRubriqueId == "642")
        {
            Log.Information("=======================================================================");
            Log.Information("🎉 VICTOIRE AUTOMATIQUE ! Le scanner a trouvé la route et ouvert la page 642.");
            Log.Information("=======================================================================");
            Assert.Pass("Navigation AnalyseRoot réussie.");
        }
        else
        {
            Assert.Fail($"[Test-Root] ❌ L'application n'est pas arrivée sur la page 642. État actuel : [{stateFinal.OldRubriqueId}] {stateFinal.PageNom}");
        }
    }
}