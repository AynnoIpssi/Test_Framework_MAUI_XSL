using System;
using System.IO;
using System.Linq; // ⚡ AJOUT CRUCIAL POUR LE TRIAUX ÉLÉMENTS
using NUnit.Framework;
using TestICA.Core.Starter;
using TestICA.Core.ScriptExecutor;
using TestICA.Core.Config.Models;
using Enum = TestICA.Core.ScriptExecutor.Enum;

namespace TestICA.Tests;

[TestFixture]
public class PageAnalyserRobustTest : AppiumStarter
{
    [Test]
    public void Extraction_Et_Affichage_Total_Filtre()
    {
        InitDriver();

        Console.WriteLine("==========================================================");
        Console.WriteLine("🚀 APPLICATION LANCÉE - ANALYSEUR ROBUSTE");
        Console.WriteLine("👉 Navigue jusqu'à ta fiche ICA sur le téléphone.");
        Console.WriteLine("==========================================================");

        System.Threading.Thread.Sleep(25000); 

        BasculeVersWebView();
        Driver!.Context = Driver.Context;

        var executor = new JsExecutor
        {
            configDriver = Driver,
            pathScript = Path.Combine(AppContext.BaseDirectory, "../../../Core/ScriptJS")
        };

        Console.WriteLine("📊 Aspiration globale du DOM en cours...");
        var result = executor.Execute(Enum.JsScriptType.PageAnalyser);
        Assert.That(result, Is.Not.Null, "Erreur lors de l'extraction des données.");
        var model = (PageAnalyserModel)result!;

        // =========================================================================
        // 🌐 PAGE 1 : L'INTÉGRALITÉ DU DOM (BRUT TOTAL - SANS AUCUN FILTRE)
        // =========================================================================
        Console.WriteLine("\n🌐 =========================================================");
        Console.WriteLine("🌐 PAGE 1 : INTÉGRALITÉ DU DOM BRUT (FLOOD COMPLET)");
        Console.WriteLine("🌐 =========================================================");
        
        Console.WriteLine("\n--- [TOUS LES TEXTES] ---");
        foreach (var p in model.TextParagraphs)
        {
            Console.WriteLine($"• {p.Text}");
        }

        Console.WriteLine("\n--- [TOUS LES CHAMPS DE SAISIE] ---");
        foreach (var field in model.FormFields)
        {
            Console.WriteLine($"ID: {field.Key,-25} | Valeur: '{field.Value}'");
        }

        // =========================================================================
        // 📱 PAGE 2 : UNIQUEMENT LES ÉLÉMENTS VISIBLES À L'ÉCRAN (TA FICHE ICA REELLE)
        // =========================================================================
        Console.WriteLine("\n📱 =========================================================");
        Console.WriteLine("📱 PAGE 2 : UNIQUEMENT CE QUI EST AFFICHÉ À L'ÉCRAN (FICHE ICA)");
        Console.WriteLine("📱 =========================================================");

        Console.WriteLine("\n--- [TEXTES VISIBLES] ---");
        foreach (var p in model.TextParagraphs)
        {
            if (p.IsVisible) Console.WriteLine($"• {p.Text}");
        }

        Console.WriteLine("\n--- [CHAMPS VISIBLES] ---");
        foreach (var field in model.FormFields)
        {
            // Correction avec FirstOrDefault pour cibler l'ID de l'élément interactif
            var element = model.InteractiveElements.FirstOrDefault(e => e.Id == field.Key);
            if (element != null && element.IsVisible)
            {
                string val = string.IsNullOrEmpty(field.Value) ? "[VIDE]" : $"'{field.Value}'";
                Console.WriteLine($"👉 ID: {field.Key,-25} | Valeur: {val}");
            }
        }

        // =========================================================================
        // 👻 PAGE 3 : UNIQUEMENT LES ÉLÉMENTS EN ARRIÈRE-PLAN (POP-UPS CACHÉES)
        // =========================================================================
        Console.WriteLine("\n👻 =========================================================");
        Console.WriteLine("👻 PAGE 3 : UNIQUEMENT LES ÉLÉMENTS CACHÉS (POP-UPS FANTÔMES)");
        Console.WriteLine("👻 =========================================================");

        Console.WriteLine("\n--- [TEXTES CACHÉS] ---");
        foreach (var p in model.TextParagraphs)
        {
            if (!p.IsVisible) Console.WriteLine($"• {p.Text}");
        }

        Console.WriteLine("\n--- [CHAMPS CACHÉS] ---");
        foreach (var field in model.FormFields)
        {
            var element = model.InteractiveElements.FirstOrDefault(e => e.Id == field.Key);
            if (element != null && !element.IsVisible)
            {
                Console.WriteLine($"❌ ID: {field.Key,-25} | Valeur stockée: '{field.Value}'");
            }
        }

        Console.WriteLine("\n=========================================================");
        Console.WriteLine("✅ FIN DE L'AFFICHAGE DES 3 PAGES COMPLÈTES");
        Console.WriteLine("=========================================================");
    }
}