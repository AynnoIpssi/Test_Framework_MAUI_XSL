// Fichier : AppiumFormFieldsFetchVisible.js
// Analyse les champs visibles (input + select + textarea) + détecte le mode (création/édition)
// via la présence du canvas de signature (#contenuSignature n'existe que si aucune ICA n'existe déjà pour ce lot).
return (function() {
    var mode = document.getElementById('contenuSignature') ? 'creation' : 'existing';

    var labelMap = {};
    document.querySelectorAll('label[for]').forEach(function(lbl) {
        var forId = lbl.getAttribute('for');
        if (!forId) return;
        labelMap[forId] = {
            text: lbl.textContent.trim(),
            obligatory: lbl.classList.contains('obligatory')
        };
    });

    function isVisible(el) {
        var style = window.getComputedStyle(el);
        return !!(el.offsetWidth || el.offsetHeight || el.getClientRects().length)
            && style.display !== 'none'
            && style.visibility !== 'hidden'
            && style.opacity !== '0';
    }

    var candidats = document.querySelectorAll(
        'input:not([type="hidden"]):not([type="submit"]):not([type="button"]), select, textarea'
    );
    var resultats = [];

    candidats.forEach(function(el) {
        if (!isVisible(el)) return;

        var tag = el.tagName.toLowerCase();
        var info = {
            tag: tag,
            id: el.id || '',
            name: el.name || '',
            type: tag === 'select' ? el.type : (tag === 'textarea' ? 'textarea' : (el.type || 'text')),
            value: el.value || '',
            checked: (el.type === 'checkbox' || el.type === 'radio') ? el.checked : null,
            selectedText: '',
            options: [],
            label: '',
            obligatory: false,
            placeholder: el.getAttribute('placeholder') || ''
        };

        if (tag === 'select') {
            var opts = [];
            for (var i = 0; i < el.options.length; i++) {
                opts.push({ value: el.options[i].value, text: el.options[i].text });
            }
            info.options = opts;
            info.selectedText = el.selectedIndex >= 0 ? el.options[el.selectedIndex].text : '';
        }

        var labelInfo = labelMap[el.id];
        if (labelInfo) {
            info.label = labelInfo.text;
            info.obligatory = labelInfo.obligatory;
        }

        resultats.push(info);
    });

    return JSON.stringify({ mode: mode, fields: resultats });
})();