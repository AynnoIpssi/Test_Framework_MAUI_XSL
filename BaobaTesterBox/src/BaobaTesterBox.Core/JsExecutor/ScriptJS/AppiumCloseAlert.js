(function() {
    // On cherche le bouton de validation (OK, Confirmer, ou Fermer)
    var closeBtn = document.querySelector('.bootbox-accept, .modal-footer .btn-primary, [data-bb-handler="confirm"], #myAlertClose');
    if (closeBtn) {
        closeBtn.click();
        return true;
    }
    return false;
})()