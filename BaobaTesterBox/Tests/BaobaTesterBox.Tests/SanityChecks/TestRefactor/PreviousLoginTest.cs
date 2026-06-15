// using System;
// using System.Collections.Generic;
// using NUnit.Framework;
// using OpenQA.Selenium;
// using Serilog;
// using TestICA.Core.Starter;
// using TestICA.Core.SmartLocator;
// using TestICA.Pages;
//
// namespace TestICA.Tests;
//
// [TestFixture]
// public class LoginTests : AppiumStarter
// {
//     [Test]
//     public void Test_Connexion_En_Dur()
//     {
//         Log.Information("=== [DEBUT] Test de connexion ===");
//
//         // 1. Initialisation standard du driver
//         InitDriver();
//
//         // 2. ⚡ NETTOYAGE NATIVE : Relance propre de l'application
//         // Puisque 'noReset' laisse l'application dans son état précédent, on la redémarre proprement
//         Log.Information("[Appium] Redémarrage forcé de l'application pour réinitialiser la WebView...");
//         try
//         {
//             // Récupère le package de l'application depuis le driver (baoba.devUITestsUAT)
//             string appPackage = "baoba.devUITestsUAT";
//             
//             // Termine l'application si elle tourne déjà, puis la relance à zéro
//             Driver!.ExecuteScript("mobile: terminateApp", new Dictionary<string, object> { { "bundleId", appPackage } });
//             System.Threading.Thread.Sleep(1000);
//             Driver!.ExecuteScript("mobile: activateApp", new Dictionary<string, object> { { "bundleId", appPackage } });
//             
//             Log.Information("[Appium] Application relancée à neuf.");
//         }
//         catch (Exception ex)
//         {
//             Log.Warning($"[Appium] Impossible de forcer le redémarrage via script mobile, poursuite standard : {ex.Message}");
//         }
//
//         // 3. Attente et bascule dans le contexte de la WebView rafraîchie
//         System.Threading.Thread.Sleep(3000);
//         BasculeVersWebView();
//         SmartLocator.Init(Driver!);
//
//         var js = (IJavaScriptExecutor)Driver!;
//
//         // 4. Moteur XSL + Bootstrap : Ouverture forcée de la modale d'authentification
//         Log.Information("[Moteur XSL] Envoi de la commande JS pour ouvrir #authenticateModal...");
//         try
//         {
//             // On s'assure d'abord que JQuery est bien dispo avant d'appeler la modale
//             js.ExecuteScript(@"
//                 if (typeof $ === 'undefined' && typeof jQuery !== 'undefined') {
//                     $ = jQuery;
//                 }
//             ");
//             
//             js.ExecuteScript("$('#authenticateModal').modal('show');");
//             Log.Information("[Moteur XSL] ✅ Commande d'ouverture envoyée avec succès.");
//         }
//         catch (Exception ex)
//         {
//             Log.Error($"[Moteur XSL] Impossible d'ouvrir la modale : {ex.Message}");
//             Assert.Fail("Le framework Bootstrap de la WebView n'a pas répondu à la commande d'ouverture.");
//         }
//
//         // Pause pour laisser la modale s'ouvrir graphiquement
//         System.Threading.Thread.Sleep(2500);
//
//         // 5. Saisie des identifiants (Recherche sécurisée par mots-clés)
//         Log.Information("[Moteur XSL] Saisie des identifiants via requêtes d'attributs JavaScript...");
//         try
//         {
//             string injectionScript = @"
//                 var emailField = document.querySelector('input[id*=""email""], input[name*=""email""]');
//                 var passwordField = document.querySelector('input[id*=""password""], input[name*=""password""]');
//                 
//                 if (!emailField || !passwordField) {
//                     var modal = document.getElementById('authenticateModal');
//                     if (modal) {
//                         var inputs = modal.getElementsByTagName('input');
//                         if (inputs.length >= 2) {
//                             emailField = inputs[0];
//                             passwordField = inputs[1];
//                         }
//                     }
//                 }
//
//                 if (emailField && passwordField) {
//                     emailField.focus();
//                     emailField.value = 'baoba.uat.tessa1@yopmail.com';
//                     emailField.dispatchEvent(new Event('input', { bubbles: true }));
//                     emailField.dispatchEvent(new Event('change', { bubbles: true }));
//
//                     passwordField.focus();
//                     passwordField.value = '@123456789Abc';
//                     passwordField.dispatchEvent(new Event('input', { bubbles: true }));
//                     passwordField.dispatchEvent(new Event('change', { bubbles: true }));
//
//                     return 'OK_SAISI';
//                 }
//                 return 'CHAMPS_INTROUVABLES';
//             ";
//
//             string resultatSaisie = (string)js.ExecuteScript(injectionScript);
//             Log.Information($"[Moteur XSL] Résultat de l'écriture en JS : {resultatSaisie}");
//
//             if (resultatSaisie != "OK_SAISI")
//             {
//                 Assert.Fail($"Impossible de localiser les champs Email/Password dans le DOM (Statut : {resultatSaisie}).");
//             }
//         }
//         catch (Exception ex)
//         {
//             Log.Error($"[Moteur XSL] Échec critique lors de la saisie : {ex.Message}");
//             Assert.Fail("L'injection des identifiants a échoué.");
//         }
//
//         System.Threading.Thread.Sleep(1000);
//
//         // 6. Clic de validation final
//         Log.Information("[Login] Déclenchement du clic sur le bouton de validation...");
//         try
//         {
//             string scriptClicFinal = @"
//                     var btn = document.getElementById('btnValidateSynchro')
//                               || document.querySelector('input[value=""Valider""]')
//                               || document.querySelector('input[onclick*=""loginDuglu""]');
//
//                     if (btn) {
//                         btn.focus();
//                         btn.click();
//                         return true;
//                     }
//                     return false;
//                 ";
//
//             bool validationLancee = (bool)js.ExecuteScript(scriptClicFinal);
//             if (!validationLancee)
//             {
//                 throw new Exception("Le bouton de validation n'a pas pu être localisé.");
//             }
//             Log.Information("[Login] ✅ Ordre de clic envoyé au bouton de validation.");
//         }
//         catch (Exception ex)
//         {
//             Log.Error($"[Login] Échec lors de la validation finale : {ex.Message}");
//             Assert.Fail("Le bouton de validation n'a pas pu être activé.");
//         }
//
//         System.Threading.Thread.Sleep(4000);
//         Log.Information("=== [FIN] Test terminé ===");
//     }
// }