using System;
using NUnit.Framework;
using OpenQA.Selenium;
using Serilog;
using TestICA.Core.Starter;
using TestICA.Core.SmartLocator;
using TestICA.Core.JsNavigator;

namespace TestICA.Tests;

[TestFixture]
public class NavigationParcoursIca : AppiumStarter
{
    [Test]
    public void Executer_Parcours_Fiche_ICA()
    {
        Log.Information("=======================================================================");
        Log.Information("=== [PARCOURS] NAVIGATION VIA LOT PLANIFIÉ 592 -> LOT RÉEL 2877      ===");
        Log.Information("=======================================================================");

        // 1. Initialisation standard de l'application
        InitDriver();

        // 2. Attente de rendu et bascule vers le contexte Web
        System.Threading.Thread.Sleep(4000);
        BasculeVersWebView();
        SmartLocator.Init(Driver!);

        // 3. Initialisation du navigateur JavaScript
        JsNavigator navigator = new JsNavigator(Driver!);

        string rubriqueIca = "512";
        string plannedBatchId = "592";  // Point d'entrée obligatoire
        string batchIdReel = "2877";    // Lot réel pour charger les données métiers

        // STRATÉGIE PARCOURS :
        // On demande à la fonction go d'ouvrir la rubrique 512, sur le lot planifié 592, 
        // en lui injectant le lot technique 2877 dans le 4ème paramètre (l'action ou la sous-clé)
        // pour reproduire le lien logique de l'application.
        string templateJsParcours = "go(1, 512, '{0}', '" + batchIdReel + "', '', '')"; 

        Log.Information($"[Robot] Envoi de la commande : go(1, 512, '{plannedBatchId}', '{batchIdReel}', '', '')");
        
        // On exécute la navigation en se basant sur le lot planifié en ID principal
        bool arrived = navigator.GoToWithParams(rubriqueIca, templateJsParcours, plannedBatchId);

        if (arrived)
        {
            Log.Information("✅ Transition effectuée par le JsNavigator. Stabilisation de l'affichage...");
            System.Threading.Thread.Sleep(3000); // Laisse le temps au moteur XSL/JS de fusionner les deux contextes

            // 4. On s'assure par JavaScript que le DOM a bien lié les deux variables
            IJavaScriptExecutor js = (IJavaScriptExecutor)Driver!;
            string scriptVerification = @"
                try {
                    var report = {
                        plannedId: $('#poultryPlannedBatchId').val(),
                        realId: $('#dif_enr3').val() || $('[id*=\'batch\']').val(),
                        inputsCount: $('input, select').length
                    };
                    return JSON.stringify(report);
                } catch(e) {
                    return 'ERREUR_JS: ' + e.message;
                }
            ";

            string jsonResult = (string)js.ExecuteScript(scriptVerification);
            Log.Information($"[Robot] État des liaisons dans la page : {jsonResult}");

            Log.Information("=======================================================================");
            Log.Information("Séquence terminée ! Lance un scan maintenant.");
            Log.Information("Si le lien a fonctionné, la Fiche ICA est là avec le bon farmId !");
            Log.Information("=======================================================================");
        }
        else
        {
            Assert.Fail("Le navigateur n'a pas pu injecter la commande de parcours combinée.");
        }
    }
}