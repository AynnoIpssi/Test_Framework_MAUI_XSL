// using System;
// using TestICA.Pages; 
// using TestICA.Core.Config.Models; // <-- Permet de comprendre ce qu'est un écran/transition si besoin
//
// namespace TestICA.Core.Navigation;
//
// public static class AppMap
// {
//     public static void Initialize(ConnexionPage connexionPage, AccueilPage accueilPage)
//     {
//         ApiNavigator.RegisterScreen("Connexion", "input_email_23");
//         ApiNavigator.RegisterScreen("Accueil", "button_deconnexion");
//
//         ApiNavigator.RegisterTransition("Connexion", "Accueil", () => 
//         {
//             connexionPage.FillEmail("admin@test.com");
//             connexionPage.ClickValider();
//         });
//     }
// }