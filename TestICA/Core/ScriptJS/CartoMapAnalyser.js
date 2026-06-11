(function() {
    var foundSelectors = [];

    // 1. Isolation du conteneur central (on évite de scanner le body entier si possible)
    var mainContainer = document.querySelector('#content, #main, .main-container, .content-wrapper') || document.body;

    // 2. Extraction des éléments interactifs standards dans cette zone
    var elements = mainContainer.querySelectorAll('button, a, [onclick], .btn, input[type="button"], input[type="submit"]');

    elements.forEach(function(el) {
        // 🛡️ FILTRE 1 : On ignore complètement si l'élément appartient au Header ou à la Sidebar
        if (el.closest('#header, .header, #sidebar, .sidebar, #menu, .menu-bar, .navbar')) {
            return;
        }

        // 🛡️ FILTRE 2 : On ignore les fenêtres flottantes, modales et popups (demande explicite)
        if (el.closest('.modal, .popup, .dialog, #popup, #modal, [class*="modal-"], [id*="popup"]')) {
            return;
        }

        // 🛡️ FILTRE 3 : On s'assure que l'élément est bien visible et cliquable à l'écran
        var style = window.getComputedStyle(el);
        if (style.display !== 'none' && style.visibility !== 'hidden' && el.offsetWidth > 0 && el.offsetHeight > 0) {
            if (el.id) {
                foundSelectors.push('#' + el.id);
            } else if (el.className) {
                // Nettoyage des classes multiples pour former un sélecteur CSS valide (ex: btn.btn-success)
                var cleanClass = el.className.trim().split(/\s+/).filter(c => !c.startsWith('leaflet-')).join('.');
                if (cleanClass) {
                    foundSelectors.push(el.tagName.toLowerCase() + '.' + cleanClass);
                }
            }
        }
    });

    // 3. 🗺️ Extraction des éléments vectoriels de la carte interactive Leaflet
    if (typeof map !== 'undefined' && map !== null) {
        map.eachLayer(function(layer) {
            // Détection des Parcelles / Bâtiments d'exploitation
            if (layer.fieldId && layer.fieldId !== -1) {
                foundSelectors.push('leaflet-field-' + layer.fieldId);
            }
            // Détection des Troupeaux / Cheptels cliquables
            else if (layer.herdId) {
                foundSelectors.push('leaflet-herd-' + layer.herdId);
            }
        });
    }

    // Exportation conforme à la norme window.jsResult attendue par ton JsExecutor
    window.jsResult = JSON.stringify([...new Set(foundSelectors)]);
})();