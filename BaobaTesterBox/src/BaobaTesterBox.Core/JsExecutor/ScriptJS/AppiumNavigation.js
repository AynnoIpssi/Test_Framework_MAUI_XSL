(function() {
    var isModule = arguments[0];

    if (isModule) {
        var moduleId = arguments[1];
        var rubriqueId = arguments[2];
        var batchId = arguments[3];

        // 🚀 Désynchronise l'exécution pour rendre la main à Appium AVANT le changement de page
        setTimeout(function() {
            openPoultryModal(moduleId, rubriqueId, null, batchId);
        }, 0);

        return "SUCCESS_MODULE_TRIGGERED";
    } else {
        var actionId = arguments[1];
        var rubriqueId = arguments[2];
        var p1 = arguments[3] || '';
        var p2 = arguments[4] || '';
        var p3 = arguments[5] || '';
        var p4 = arguments[6] || '';

        // 🚀 Idem ici pour la navigation standard
        setTimeout(function() {
            go(actionId, rubriqueId, p1, p2, p3, p4);
        }, 0);

        return "SUCCESS_STANDARD_TRIGGERED";
    }
})()