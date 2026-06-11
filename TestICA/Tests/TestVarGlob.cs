using System;
using System.Collections.Generic;
using NUnit.Framework;
using OpenQA.Selenium;
using Serilog;
using TestICA.Core.Starter;
using TestICA.Core.SmartLocator;

namespace TestICA.Tests;

[TestFixture]
public class TestVarICA : AppiumStarter
{
    [Test]
    public void Executer_Mise_A_Jour_Variables_Et_Navigation_ICA()
    {
        Log.Information("=======================================================================");
        Log.Information("=== [DÉBUT TEST] CORRECTION ET ALIGNEMENT DES CONFIGURATIONS BATCH ===");
        Log.Information("=======================================================================");

        // 1. Initialisation standard du driver
        InitDriver();

        // 2. Attente et bascule dans le contexte de la WebView
        System.Threading.Thread.Sleep(3000);
        BasculeVersWebView();
        SmartLocator.Init(Driver!);

        var js = (IJavaScriptExecutor)Driver!;

        // 3. Alignement strict sur les correspondances du premier scan fonctionnel
        string farmIdCible = "442";
        string poultryPlannedBatchIdCible = "592"; 
        string batchIdReel = "2877"; 

        Log.Information($"[Robot] Configuration -> farmId: {farmIdCible} | plannedBatchId: {poultryPlannedBatchIdCible} | goParam: {batchIdReel}");

        try
        {
            // Sécurité globale sur l'instance jQuery
            js.ExecuteScript(@"
                if (typeof $ === 'undefined' && typeof jQuery !== 'undefined') {
                    $ = jQuery;
                }
            ");

            // 4. SCRIPT RE-CORRIGÉ : Injection ciblée des variables sans écrasement mutuel
            string scriptAjustementVariables = $@"
                if (typeof $ !== 'undefined' && typeof go === 'function') {{
                    
                    // On pré-remplit le DOM avec les vraies valeurs d'origine
                    $('#farmId').val('{farmIdCible}');
                    $('#poultryPlannedBatchId').val('{poultryPlannedBatchIdCible}');
                    $('#oldrubriqueid').val('512');
                    
                    // On pousse également dans l'objet global window au cas où le script go() s'appuie dessus
                    window.farmId = '{farmIdCible}';
                    window.poultryPlannedBatchId = '{poultryPlannedBatchIdCible}';
                    window.batchId = '{batchIdReel}';
                    
                    // Exécution du go avec le paramètre isolé du batch réel
                    go(1, 512, '{batchIdReel}', '', '', '');
                    
                    return 'ROUTAGE_ALIGNE';
                }}
                return 'ERREUR_CONTEXTE_WEB';
            ";

            Log.Information("[Moteur XSL] Lancement du routage avec les paramètres dissociés...");
            string resultat = (string)js.ExecuteScript(scriptAjustementVariables);
            Log.Information($"[Moteur XSL] Statut de l'appel : {resultat}");

            if (resultat != "ROUTAGE_ALIGNE")
            {
                Assert.Fail($"Le script n'a pas pu s'exécuter correctement. Statut renvoyé : {resultat}");
            }
        }
        catch (Exception ex)
        {
            Log.Error($"[Moteur XSL] Échec critique lors de l'alignement des paramètres : {ex.Message}");
            Assert.Fail($"L'injection avec dissociation des IDs de lots a échoué : {ex.Message}");
        }

        // 5. Attente de la reconstruction structurelle de l'écran par l'application
        Log.Information("[Robot] Pause de 5 secondes pour observer le comportement du DOM réaligné...");
        System.Threading.Thread.Sleep(5000);

        Log.Information("=======================================================================");
        Log.Information("=== [FIN] Fin de la séquence. Prêt pour l'extraction différentielle. ==");
        Log.Information("=======================================================================");
    }
}