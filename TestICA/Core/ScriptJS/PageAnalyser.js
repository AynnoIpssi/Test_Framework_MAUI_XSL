// =========================================================================
// 1. INITIALISATION DU RAPPORT DE LA PAGE
// =========================================================================
const pageReport = {
    url: window.location.href,
    title: document.title,
    formFields: {},
    textParagraphs: [],
    interactiveElements: []
};

// Fonction robuste de visibilité qui remonte TOUS les parents jusqu'à la racine
function estReellementVisible(el) {
    if (!el) return false;

    let actuel = el;
    while (actuel) {
        const style = window.getComputedStyle(actuel);
        if (style.display === 'none' || style.visibility === 'hidden' || style.opacity === '0') {
            return false;
        }
        actuel = actuel.parentElement; // On remonte d'un parent à chaque étape
    }

    const rect = el.getBoundingClientRect();
    return rect.width > 0 && rect.height > 0;
}

// =========================================================================
// 2. EXTRACTION DES TEXTES (Élargie aux spans, labels et tableaux)
// =========================================================================
document.querySelectorAll('p, .text-block, h1, h2, h3, span, label, td').forEach(function(el) {
    if (el.innerText && el.innerText.trim() !== "") {
        const texteNettoye = el.innerText.trim();

        // Sécurité anti-doublon : si le texte est déjà pris (via un enfant ou un parent), on passe
        const dejaPris = pageReport.textParagraphs.some(p => p.text === texteNettoye);

        // On évite aussi de prendre des blocs de code ou des textes trop gigantesques (ex: tout le DOM d'un coup)
        if (!dejaPris && texteNettoye.length < 300) {
            pageReport.textParagraphs.push({
                text: texteNettoye,
                isVisible: estReellementVisible(el)
            });
        }
    }
});

// =========================================================================
// 3. EXTRACTION DES CHAMPS DE FORMULAIRE (CORRIGÉE ET SYNCHRONISÉE)
// =========================================================================
const fields = document.querySelectorAll('input, select, textarea');
fields.forEach(field => {
    const fieldKey = field.id || field.name;

    if (fieldKey) {
        const visible = estReellementVisible(field);

        // Enregistrement de la valeur pour la PAGE 1
        if (field.type === 'checkbox') {
            pageReport.formFields[fieldKey] = field.checked ? "true" : "false";
        } else {
            pageReport.formFields[fieldKey] = field.value || "";
        }

        // ⚡ LE FIX POUR LES CHAMPS : On les ajoute ici pour que le C# puisse les filtrer (PAGES 2 & 3)
        pageReport.interactiveElements.push({
            id: fieldKey,
            tagName: field.tagName.toLowerCase(),
            text: "",
            isVisible: visible,
            isEnabled: !field.disabled,
            onClickAttribute: ""
        });
    }
});

// =========================================================================
// 4. EXTRACTION DES ÉLÉMENTS INTERACTIFS (BOUTONS, LIENS)
// =========================================================================
const interactives = document.querySelectorAll('button, a, input[type="button"], input[type="submit"]');
interactives.forEach((el, index) => {
    const style = window.getComputedStyle(el);
    const isVisible = estReellementVisible(el);

    let text = el.textContent || el.value || "";
    text = text.trim().replace(/\s+/g, ' ');

    pageReport.interactiveElements.push({
        id: el.id || el.name || `dynamic_id_${index}`,
        tagName: el.tagName.toLowerCase(),
        text: text,
        isVisible: isVisible,
        isEnabled: !el.disabled,
        onClickAttribute: el.getAttribute('onclick') || ""
    });
});

// =========================================================================
// 5. STOCKAGE POUR LE C#
// =========================================================================
window.jsResult = JSON.stringify(pageReport);