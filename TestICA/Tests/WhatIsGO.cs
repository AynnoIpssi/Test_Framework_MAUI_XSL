using System;
using NUnit.Framework;
using OpenQA.Selenium;
using Serilog;
using TestICA.Core.Starter;
using TestICA.Core.SmartLocator;

namespace TestICA.Tests;

[TestFixture]
public class OuvrirFicheIcaViaModal : AppiumStarter
{
    [Test]
    public void Executer_Ouverture_Directe_Modal_ICA()
    {
        Log.Information("=======================================================================");
        Log.Information("=== [SÉQUENCE] APPEL DIRECT DE openPoultryModal (CODE DEV)          ===");
        Log.Information("=======================================================================");

        // 1. Initialisation standard
        InitDriver();

        // 2. Bascule WebView
        System.Threading.Thread.Sleep(4000);
        BasculeVersWebView();
        SmartLocator.Init(Driver!);

        IJavaScriptExecutor js = (IJavaScriptExecutor)Driver!;

        Log.Information("[Robot] Injection de la commande openPoultryModal récupérée auprès des devs...");

        // On enveloppe l'appel dans un try/catch JS pour intercepter une éventuelle erreur si une variable est absente
        string scriptModalDirect = @"
            try {
                // Sécurité : On vérifie si les variables globales des dev existent bien dans le contexte actuel
                if (typeof globalPoultryModuleid !== 'undefined' && typeof globalPoultryBatchId !== 'undefined') {
                    
                    // L'appel exact fourni par les développeurs
                    openPoultryModal(globalPoultryModuleid, 542, null, globalPoultryBatchId);
                    
                    return 'SUCCÈS: openPoultryModal exécuté avec succès !';
                } else {
                    // Si jamais les variables globales ne sont pas chargées, on tente avec tes IDs connus en dur
                    // openPoultryModal(1, 542, null, '592'); // Alternative de secours
                    return 'ERREUR: Les variables globales globalPoultryModuleid ou globalPoultryBatchId ne sont pas définies sur cette page.';
                }
            } catch (e) {
                return 'ERREUR_JS_MODAL: ' + e.message;
            }
        ";

        string result = (string)js.ExecuteScript(scriptModalDirect);
        Log.Information($"[Robot] Résultat de l'exécution : {result}");

        // 3. Attente d'ouverture de la pop-in / modal
        Log.Information("[Robot] Attente du déploiement de la modal Fiche ICA...");
        System.Threading.Thread.Sleep(4000);

        // 4. Vérification si les champs ICA sont là
        string checkChamps = @"
            try {
                return $('input, select, textarea').length > 0 ? 'CHAMPS_DE_SAISIE_TROUVÉS' : 'MODAL_VIDE_OU_ABSENTE';
            } catch(e) { return 'Erreur check: ' + e.message; }
        ";
        string statusFinal = (string)js.ExecuteScript(checkChamps);
        Log.Information($"[Robot] État de l'écran : {statusFinal}");
        
        Log.Information("=======================================================================");
    }
}