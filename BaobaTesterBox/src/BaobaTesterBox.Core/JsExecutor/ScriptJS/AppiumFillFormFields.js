try {

    if (arguments.length < 1) {
        return JSON.stringify({
            ERROR: "NO_ARGUMENT_RECEIVED"
        });
    }

    var valeursParId = arguments[0];

    if (typeof valeursParId === "string") {
        valeursParId = JSON.parse(valeursParId);
    }

    if (!valeursParId || typeof valeursParId !== "object") {
        return JSON.stringify({
            ERROR: "INVALID_ARGUMENT"
        });
    }

    var resultats = {};

    Object.keys(valeursParId).forEach(function (id) {

        try {

            var el = document.getElementById(id);

            if (!el) {
                resultats[id] = "ERR_NOT_FOUND";
                return;
            }

            var valeur = valeursParId[id];

            // =========================================================
            // 🔥 SIGNATURE jqSignature (CAS SPECIAL)
            // =========================================================
            if (id === "contenuSignature" || id.startsWith("jq_signature")) {

                try {

                    var container = el;
                    var canvas = container.querySelector("canvas");

                    if (!canvas) {
                        resultats[id] = "ERR_NO_CANVAS";
                        return;
                    }

                    var rect = canvas.getBoundingClientRect();

                    function draw(x, y) {

                        canvas.dispatchEvent(new MouseEvent("mousedown", {
                            bubbles: true,
                            cancelable: true,
                            clientX: x,
                            clientY: y,
                            view: window
                        }));

                        canvas.dispatchEvent(new MouseEvent("mousemove", {
                            bubbles: true,
                            cancelable: true,
                            clientX: x + 2,
                            clientY: y + 2,
                            view: window
                        }));

                        canvas.dispatchEvent(new MouseEvent("mouseup", {
                            bubbles: true,
                            cancelable: true,
                            clientX: x + 4,
                            clientY: y + 4,
                            view: window
                        }));
                    }

                    var x = rect.left + rect.width / 2;
                    var y = rect.top + rect.height / 2;

                    // 🔥 signature multi-traits (important pour jqSignature)
                    draw(x, y);
                    draw(x + 20, y - 10);
                    draw(x + 40, y + 5);
                    draw(x + 70, y - 8);
                    draw(x + 100, y + 10);

                    // 🔥 forcer jqSignature / jQuery state
                    if (window.jQuery) {
                        try {
                            $(container).trigger("mousedown");
                            $(container).trigger("mousemove");
                            $(container).trigger("mouseup");
                            $(container).trigger("change");
                        } catch (e) {}
                    }

                    // 🔥 état UI global (ton XSL / YUI)
                    window.modeSignature = "signed";

                    container.dispatchEvent(new Event("input", { bubbles: true }));
                    container.dispatchEvent(new Event("change", { bubbles: true }));

                    resultats[id] = "SIGNATURE_OK";
                    return;

                } catch (e) {
                    resultats[id] = "ERR_SIGNATURE:" + e.message;
                    return;
                }
            }

            // =========================================================
            // SELECT
            // =========================================================
            if (el.tagName && el.tagName.toUpperCase() === "SELECT") {

                el.value = valeur;

                if (el.value != valeur) {
                    for (var i = 0; i < el.options.length; i++) {
                        if (
                            el.options[i].value == valeur ||
                            el.options[i].text == valeur
                        ) {
                            el.selectedIndex = i;
                            break;
                        }
                    }
                }
            }

                // =========================================================
                // CHECKBOX / RADIO
            // =========================================================
            else if (
                el.type === "checkbox" ||
                el.type === "radio"
            ) {

                el.checked =
                    valeur === true ||
                    valeur === 1 ||
                    valeur === "1" ||
                    valeur === "true";
            }

                // =========================================================
                // INPUT / TEXTAREA
            // =========================================================
            else {

                el.focus();
                el.value = valeur;
            }

            // =========================================================
            // EVENTS GLOBAUX
            // =========================================================
            el.dispatchEvent(new Event("input", { bubbles: true }));
            el.dispatchEvent(new Event("change", { bubbles: true }));
            el.dispatchEvent(new Event("blur", { bubbles: true }));

            resultats[id] = "OK";

        } catch (e) {

            resultats[id] = "ERR:" + (e && e.message ? e.message : e);
        }
    });

    return JSON.stringify(resultats);

}
catch (e) {

    return JSON.stringify({
        ERROR: e.message,
        STACK: e.stack
    });
}

try {

    // 🔥 bouton ICA
    var btn = document.getElementById("btn-save-popin");

    if (!btn) {
        resultats["btn-save-popin"] = "ERR_NOT_FOUND";
    } else {

        // 🔥 sécurité : scroll visible
        btn.scrollIntoView({ block: "center", inline: "center" });

        // 🔥 focus
        btn.focus();

        // 🔥 click natif
        btn.click();

        // 🔥 fallback événement DOM (frameworks XSL / YUI)
        btn.dispatchEvent(new MouseEvent("click", {
            bubbles: true,
            cancelable: true,
            view: window
        }));

        // 🔥 déclenchement logique métier si nécessaire
        if (typeof window.checkBeforeSave === "function") {
            window.checkBeforeSave();
        }

        resultats["btn-save-popin"] = "CLICK_OK";
    }

} catch (e) {
    resultats["btn-save-popin"] = "ERR_CLICK:" + e.message;
}