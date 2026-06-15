// Fichier : AppiumInputFetchVisible.js : sert a affiché tout les inputs visible d'une page
(function() {
    var allInputs = document.querySelectorAll('input:not([type="submit"]):not([type="button"]):not([type="checkbox"]):not([type="radio"]):not([type="hidden"])');

    var visibleInputs = Array.from(allInputs).filter(function(input) {
        var style = window.getComputedStyle(input);
        return !!(input.offsetWidth || input.offsetHeight || input.getClientRects().length)
            && style.display !== 'none'
            && style.visibility !== 'hidden'
            && style.opacity !== '0';
    });

    // Le return est uniquement ici, bien au chaud dans la fonction !
    return visibleInputs;
})();