// Fichier : ScriptJS/AppiumClickElement.js
(function() {
    // arguments[0] contient le sélecteur CSS passé par le C# (ex: "#btnValidateSynchro")
    var selector = arguments[0];

    if (!selector) {
        return "ERR_NO_SELECTOR_PROVIDED";
    }

    // Recherche de l'élément dans le DOM
    var element = document.querySelector(selector);

    // Si on ne le trouve pas, on tente le fallback d'origine pour ton bouton UAT
    if (!element && selector.includes("btnValidateSynchro")) {
        element = document.querySelector('input[value="Valider"]')
            || document.querySelector('input[onclick*="loginDuglu"]');
    }

    if (element) {
        element.focus();
        element.click();
        return "SUCCESS_CLICKED";
    }

    return "ERR_ELEMENT_NOT_FOUND";
})();