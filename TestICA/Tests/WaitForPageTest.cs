using System;
using System.Collections.Generic;
using NUnit.Framework;
using OpenQA.Selenium;
using Serilog;
using TestICA.Core.Starter;
using TestICA.Core.SmartLocator;
using TestICA.Core.SmartWaiter;
using TestICA.Core.JsStateReader;
using TestICA.Pages;

namespace TestICA.Tests;

[TestFixture]
public class WaitForPageTest : AppiumStarter
{
    [Test]
    public void Test_Connexion_En_Dur()
    {
        Log.Information("=== [DEBUT] Test de connexion & Stabilisation ===");

        // 1. Initialisation standard du driver
        InitDriver();

        // 2. ⚡ NETTOYAGE NATIVE : Relance propre de l'application
        Log.Information("[Appium] Redémarrage forcé de l'application pour réinitialiser la WebView...");
        try
        {
            string appPackage = "baoba.devUITestsUAT";
            
            Driver!.ExecuteScript("mobile: terminateApp", new Dictionary<string, object> { { "bundleId", appPackage } });
            System.Threading.Thread.Sleep(1000);
            Driver!.ExecuteScript("mobile: activateApp", new Dictionary<string, object> { { "bundleId", appPackage } });
            
            Log.Information("[Appium] Application relancée à neuf.");
        }
        catch (Exception ex)
        {
            Log.Warning($"[Appium] Impossible de forcer le redémarrage via script mobile, poursuite standard : {ex.Message}");
        }

        // 3. Attente et bascule dans le contexte de la WebView rafraîchie
        System.Threading.Thread.Sleep(3000);
        BasculeVersWebView();
        SmartLocator.Init(Driver!);

        var js = (IJavaScriptExecutor)Driver!;

        // 4. Moteur XSL + Bootstrap : Ouverture forcée de la modale d'authentification
        Log.Information("[Moteur XSL] Envoi de la commande JS pour ouvrir #authenticateModal...");
        try
        {
            js.ExecuteScript(@"
                if (typeof $ === 'undefined' && typeof jQuery !== 'undefined') {
                    $ = jQuery;
                }
            ");
            
            js.ExecuteScript("$('#authenticateModal').modal('show');");
            Log.Information("[Moteur XSL] ✅ Commande d'ouverture envoyée avec succès.");
        }
        catch (Exception ex)
        {
            Log.Error($"[Moteur XSL] Impossible d'ouvrir la modale : {ex.Message}");
            Assert.Fail("Le framework Bootstrap de la WebView n'a pas répondu à la commande d'ouverture.");
        }

        // Pause pour laisser la modale s'ouvrir graphiquement
        System.Threading.Thread.Sleep(2500);

        // 5. Saisie des identifiants (Recherche sécurisée par mots-clés)
        Log.Information("[Moteur XSL] Saisie des identifiants via requêtes d'attributs JavaScript...");
        try
        {
            string injectionScript = @"
                var emailField = document.querySelector('input[id*=""email""], input[name*=""email""]');
                var passwordField = document.querySelector('input[id*=""password""], input[name*=""password""]');
                
                if (!emailField || !passwordField) {
                    var modal = document.getElementById('authenticateModal');
                    if (modal) {
                        var inputs = modal.getElementsByTagName('input');
                        if (inputs.length >= 2) {
                            emailField = inputs[0];
                            passwordField = inputs[1];
                        }
                    }
                }

                if (emailField && passwordField) {
                    emailField.focus();
                    emailField.value = 'baoba.uat.tessa1@yopmail.com';
                    emailField.dispatchEvent(new Event('input', { bubbles: true }));
                    emailField.dispatchEvent(new Event('change', { bubbles: true }));

                    passwordField.focus();
                    passwordField.value = '@123456789Abc';
                    passwordField.dispatchEvent(new Event('input', { bubbles: true }));
                    passwordField.dispatchEvent(new Event('change', { bubbles: true }));

                    return 'OK_SAISI';
                }
                return 'CHAMPS_INTROUVABLES';
            ";

            string resultatSaisie = (string)js.ExecuteScript(injectionScript);
            Log.Information($"[Moteur XSL] Résultat de l'écriture en JS : {resultatSaisie}");

            if (resultatSaisie != "OK_SAISI")
            {
                Assert.Fail($"Impossible de localiser les champs Email/Password dans le DOM (Statut : {resultatSaisie}).");
            }
        }
        catch (Exception ex)
        {
            Log.Error($"[Moteur XSL] Échec critique lors de la saisie : {ex.Message}");
            Assert.Fail("L'injection des identifiants a échoué.");
        }

        System.Threading.Thread.Sleep(1000);

        // 6. Clic de validation final
        Log.Information("[Login] Déclenchement du clic sur le bouton de validation...");
        try
        {
            string scriptClicFinal = @"
                    var btn = document.getElementById('btnValidateSynchro')
                              || document.querySelector('input[value=""Valider""]')
                              || document.querySelector('input[onclick*=""loginDuglu""]');

                    if (btn) {
                        btn.focus();
                        btn.click();
                        return true;
                    }
                    return false;
                ";

            bool validationLancee = (bool)js.ExecuteScript(scriptClicFinal);
            if (!validationLancee)
            {
                throw new Exception("Le bouton de validation n'a pas pu être localisé.");
            }
            Log.Information("[Login] ✅ Ordre de clic envoyé au bouton de validation.");
        }
        catch (Exception ex)
        {
            Log.Error($"[Login] Échec lors de la validation finale : {ex.Message}");
            Assert.Fail("Le bouton de validation n'a pas pu être activé.");
        }

        // =========================================================================
        // 7. 🕒 INTERCEPTION AVEC DIAGNOSTIQUE POST-TIMEOUT (SmartWaiter + AppMap)
        // =========================================================================
        Log.Information("[SmartWaiter] Attente active de la fin de synchronisation (Timeout: 30s)...");
        
        var waiter = new SmartWaiter(Driver!, timeoutSeconds: 30);
        string rubriquePageAccueil = "580"; 

        bool estArriveSurPageAccueil = waiter.WaitForPage(rubriquePageAccueil);

        if (estArriveSurPageAccueil)
        {
            Log.Information("[SmartWaiter] 🎉 REUSSITE : La page d'accueil '500' a été détectée à l'écran !");
            var stateReader = new TestICA.Core.JsStateReader.JsStateReader(Driver!);
            var stateFinal = stateReader.ReadCurrentState();
            Log.Information($"[JsStateReader] 📍 Position actuelle confirmée : [{stateFinal.OldRubriqueId}] {stateFinal.PageNom}");
        }
        else
        {
            Log.Warning("[SmartWaiter] ⚠️ Timeout atteint. Extraction immédiate des données de la WebView...");
            
            try
            {
                // On force la lecture complète du DOM à l'instant T pour voir ce qu'il s'y cache
                var stateReader = new TestICA.Core.JsStateReader.JsStateReader(Driver!);
                var stateBrut = stateReader.ReadCurrentState();
                
                Log.Information("==========================================================");
                Log.Information("🔍 ANCHOR DIAGNOSTIC (ÉTAT RÉEL AU TIMEOUT) :");
                Log.Information($"-> rubriqueid injecté dans le DOM     : '{stateBrut.RubriqueId}'");
                Log.Information($"-> oldrubriqueid injecté dans le DOM  : '{stateBrut.OldRubriqueId}'");
                Log.Information($"-> FarmId actif                       : '{stateBrut.FarmId}'");
                Log.Information("==========================================================");
            }
            catch (Exception ex)
            {
                Log.Error($"[Diagnostic] Impossible d'interroger la WebView : {ex.Message}");
            }

            Assert.Fail($"Le test a échoué. Le SmartWaiter cherchait '500' mais l'application n'a pas mis à jour le champ masqué à temps.");
        }

        Log.Information("=== [FIN] Test terminé ===");
    }
}