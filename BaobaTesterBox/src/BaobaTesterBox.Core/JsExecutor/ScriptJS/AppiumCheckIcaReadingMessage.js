(function() {
    // Si la fonction globale existe sur la page, on l'exécute directement pour le test
    if (typeof getNoReadingDates === 'function') {
        return getNoReadingDates();
    }
    return "FONCTION_INTROUVABLE";
})()