using System;
using System.Collections.Generic;
using NUnit.Framework;
using OpenQA.Selenium;
using Serilog;
using TestICA.Core.Starter;
using TestICA.Core.SmartLocator;
using TestICA.Core.SmartWaiter;

namespace TestICA.Tests;

[TestFixture]
public class AuthentificationTests : AppiumStarter
{
    [Test]
    public void Test_Connexion_Intelligente()
    {
        Log.Information("=== [DEBUT] Test de connexion adaptatif (En dur) ===");

        // 1. Initialisation standard du driver & WebView
        InitDriver();

        // 2. Relance propre de l'application (Native Cleanup)
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
            Log.Warning($"[Appium] Impossible de forcer le redémarrage : {ex.Message}"); 
        }

        System.Threading.Thread.Sleep(3000);
        BasculeVersWebView();
        SmartLocator.Init(Driver!);

        var js = (IJavaScriptExecutor)Driver!;

        // =========================================================================
        // 3. 🔍 SMART BYPASS : Analyse de l'état pour sauter le login si déjà connecté
        // =========================================================================
        Log.Information("[SessionCheck] Lecture de l'écran actuel...");
        var stateReader = new TestICA.Core.JsStateReader.JsStateReader(Driver!);
        var stateInitial = stateReader.ReadCurrentState();

        string idPageLogin = "1"; // ID de l'écran de Login Système

        // Si on est sur n'importe quel écran SAUF l'écran de login -> session active
        bool dejaConnecte = stateInitial.OldRubriqueId != idPageLogin;

        if (dejaConnecte)
        {
            Log.Information($"[SessionCheck] 🛡️ L'application est actuellement sur la page [{stateInitial.OldRubriqueId}] {stateInitial.PageNom ?? "Interne"}.");
            Log.Information("[SessionCheck] ⏭️ Session active détectée ! Le test passe l'étape d'authentification.");
        }
        else
        {
            Log.Information("[SessionCheck] 🔑 Écran de login (ID: 1) détecté. Lancement de la procédure d'authentification...");
            
            // =========================================================================
            // 4. 🔓 EN DUR : Ouverture forcée de la modale d'authentification
            // =========================================================================
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
                Assert.Fail($"Impossible d'ouvrir la modale : {ex.Message}");
            }

            System.Threading.Thread.Sleep(2500);

            // =========================================================================
            // 5. 📝 EN DUR : Saisie des identifiants via JS
            // =========================================================================
            Log.Information("[Moteur XSL] Saisie des identifiants...");
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
                if (resultatSaisie != "OK_SAISI")
                {
                    Assert.Fail($"Impossible de localiser les champs (Statut : {resultatSaisie}).");
                }
            }
            catch (Exception ex)
            {
                Assert.Fail($"Échec critique lors de la saisie : {ex.Message}");
            }

            System.Threading.Thread.Sleep(1000);

            // =========================================================================
            // 6. 🔘 EN DUR : Clic de validation final
            // =========================================================================
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
                Assert.Fail($"Le bouton de validation n'a pas pu être activé : {ex.Message}");
            }

            // =========================================================================
            // 7. 🕒 ATTENTE DE LA FIN DE SYNCHRONISATION VIA SMARTWAITER
            // =========================================================================
            Log.Information("[SmartWaiter] Attente de la fin de synchronisation post-login...");
            var waiter = new SmartWaiter(Driver!, timeoutSeconds: 30);
            string rubriquePageAccueil = "580"; 

            if (waiter.WaitForPage(rubriquePageAccueil))
            {
                Log.Information("[SmartWaiter] 🎉 REUSSITE : Synchronisation terminée, arrivée sur la page d'accueil !");
            }
            else
            {
                Assert.Fail($"[SmartWaiter] ❌ TIMEOUT : L'écran d'accueil (ID: {rubriquePageAccueil}) n'est pas apparu après le login.");
            }
        }

        // =========================================================================
        // 8. 🚀 ETAPE SUIVANTE
        // =========================================================================
        var stateFinal = stateReader.ReadCurrentState();
        Log.Information($"[Statut Métier] Prêt pour exécuter les scénarios sur l'écran : [{stateFinal.OldRubriqueId}] {stateFinal.PageNom}");

        Log.Information("=== [FIN] Test terminé avec succès ===");
    }
}