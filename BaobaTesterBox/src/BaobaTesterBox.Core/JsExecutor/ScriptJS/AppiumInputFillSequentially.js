// Fichier : ScriptJS/AppiumInputFillSequentially.js
(function() {
    // arguments[0] contient le tableau de chaînes envoyé par le C# (ex: ["email", "password"])
    var valuesToInput = arguments[0];

    if (!valuesToInput || valuesToInput.length === 0) {
        return "ERR_NO_VALUES_PROVIDED";
    }

    // 1. On récupère exactement la même liste d'inputs
    var allInputs = document.querySelectorAll('input:not([type="submit"]):not([type="button"]):not([type="checkbox"]):not([type="radio"]):not([type="hidden"])');

    // 2. On filtre avec les mêmes critères de visibilité stricts
    var visibleInputs = Array.from(allInputs).filter(function(input) {
        var style = window.getComputedStyle(input);
        return !!(input.offsetWidth || input.offsetHeight || input.getClientRects().length)
            && style.display !== 'none'
            && style.visibility !== 'hidden'
            && style.opacity !== '0';
    });

    if (visibleInputs.length === 0) {
        return "ERR_NO_VISIBLE_INPUTS_FOUND";
    }

    var appliedCount = 0;

    // 3. Boucle de saisie séquentielle sur les éléments visibles
    for (var i = 0; i < visibleInputs.length && i < valuesToInput.length; i++) {
        var field = visibleInputs[i];
        var value = valuesToInput[i];

        if (field && value !== null && value !== undefined) {
            field.focus();
            field.value = value;

            // Événements indispensables pour réveiller Angular / React / Vue / MAUI Blazor
            field.dispatchEvent(new Event('input', { bubbles: true }));
            field.dispatchEvent(new Event('change', { bubbles: true }));

            field.blur();
            appliedCount++;
        }
    }

    return "SUCCESS_FILLED_" + appliedCount;
})();