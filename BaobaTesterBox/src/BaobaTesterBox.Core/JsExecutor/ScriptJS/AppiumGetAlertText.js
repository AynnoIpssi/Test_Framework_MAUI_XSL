(function() {
    // On cherche le texte dans la modale d'alerte active (Bootbox ou modale custom)
    var alertBody = document.querySelector('.bootbox-body, .modal-body, #myAlertContent');
    if (alertBody) {
        return alertBody.innerText.trim();
    }
    return null;
})()