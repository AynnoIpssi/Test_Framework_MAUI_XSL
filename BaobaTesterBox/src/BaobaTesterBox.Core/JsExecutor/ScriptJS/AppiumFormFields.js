// Fichier : AppiumFillFormFields.js
// Remplit une liste de champs (input/select/textarea/checkbox) à partir d'un dictionnaire {id: valeur} reçu en JSON (arguments[0]).
// Déclenche les events input/change pour rester compatible avec les listeners JS/jQuery de la page.
return (function() {
    var valeursParId = JSON.parse(arguments[0]);
    var resultats = {};

    Object.keys(valeursParId).forEach(function(id) {
        var el = document.getElementById(id);
        if (!el) {
            resultats[id] = 'ERR_NOT_FOUND';
            return;
        }

        var valeur = valeursParId[id];

        if (el.type === 'checkbox' || el.type === 'radio') {
            el.checked = (valeur === '1' || valeur === true || valeur === 'true');
        } else {
            el.value = valeur;
        }

        el.dispatchEvent(new Event('input', { bubbles: true }));
        el.dispatchEvent(new Event('change', { bubbles: true }));

        resultats[id] = 'OK';
    });

    return JSON.stringify(resultats);
})();