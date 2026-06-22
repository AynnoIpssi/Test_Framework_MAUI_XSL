using System;
using System.Threading;
using BaobaTesterBox.Core.ScriptExecutor;
using BaobaTesterBox.Core.ScriptExecutor.Enum;
using BaobaTesterBox.Core.Loggings;
using BaobaTesterBox.Core.Tools; // Pour accéder à tes services de Touch et Scroll

namespace BaobaTesterBox.Domain.Pages.FicheIca;

public class FicheIcaPage
{
    private readonly AppiumJsExecutor _jsExecutor;
    private readonly AppiumScrollService _scrollService;
    private readonly AppiumTouchService _touchService;
    private const string Origine = "FicheIcaPage";

    public FicheIcaPage()
    {
        _jsExecutor = new AppiumJsExecutor();
        _scrollService = new AppiumScrollService();
        _touchService = new AppiumTouchService();
    }

    /// <summary>
    /// Remplit les données en JS, puis utilise les services natifs pour Scroller, Signer et Envoyer.
    /// </summary>
    public bool RemplirSignerEtEnvoyerTout()
    {
        AppiumLoggerService.LogInfo("=== Étape 1 : Remplissage des données du formulaire (JS) ===", Origine);
        
        var resultatJs = _jsExecutor.Execute(JsScriptType.FeedFicheIca);

        if (resultatJs?.ToString() != "REMPLISSAGE_DONNEES_OK")
        {
            AppiumLoggerService.LogError($"❌ Échec du remplissage initial : {resultatJs}", Origine);
            return false;
        }

        // === Étape 2 : Gestion de la Signature avec tes services ===
        AppiumLoggerService.LogInfo("=== Étape 2 : Centrage et signature sur le Canvas (Natif) ===", Origine);
        
        // Sélecteur générique du canvas de signature dans la fiche ICA
        string canvasSelector = "canvas"; 
        
        // On scroll jusqu'au canvas pour qu'il soit bien visible et cliquable au centre de l'écran
        _scrollService.ScrollToElement(canvasSelector);
        
        // On applique ton tracé au doigt natif
        _touchService.SignerDansCanvas(canvasSelector);


        // === Étape 3 : Validation et Envoi final ===
        AppiumLoggerService.LogInfo("=== Étape 3 : Défilement et clic sur envoyer ===", Origine);
        
        string boutonEnvoyerSelector = "#btn-save-popin";
        
        // On descend jusqu'au bouton de validation
        _scrollService.ScrollToElement(boutonEnvoyerSelector);

        // Clic sur le bouton via l'action officielle de ton framework
        var resultatClic = _jsExecutor.Execute(JsScriptType.AppiumClickElement, boutonEnvoyerSelector);

        if (resultatClic?.ToString() == "SUCCESS_CLICKED")
        {
            AppiumLoggerService.LogInfo("✔ Fiche ICA complétée, signée au doigt et envoyée avec succès !", Origine);
            return true;
        }

        AppiumLoggerService.LogError($"❌ Impossible de cliquer sur le bouton d'envoi : {resultatClic}", Origine);
        return false;
    }
}