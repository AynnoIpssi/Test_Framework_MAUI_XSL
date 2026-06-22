window.__SIGNATURE_READY = false;

window.waitSignatureCanvas = function (timeout = 5000) {

    return new Promise(function (resolve, reject) {

        var start = Date.now();

        var interval = setInterval(function () {

            var container = document.getElementById("contenuSignature");
            if (!container) return;

            var canvas = container.querySelector("canvas");

            if (canvas && canvas.offsetWidth > 50 && canvas.offsetHeight > 50) {

                clearInterval(interval);
                window.__SIGNATURE_READY = true;
                resolve(canvas);
                return;
            }

            if (Date.now() - start > timeout) {
                clearInterval(interval);
                reject("CANVAS_NOT_READY");
            }

        }, 100);
    });
};


/* ================================
   SIGNATURE TEST SAFE (NO TOUCH)
================================ */
window.signCanvasTest = async function () {

    try {

        var canvas = await window.waitSignatureCanvas(5000);

        var ctx = canvas.getContext("2d");
        if (!ctx) return "NO_CTX";

        // 🔥 signature simple stable
        ctx.beginPath();
        ctx.moveTo(30, 30);
        ctx.lineTo(200, 80);
        ctx.lineTo(350, 40);
        ctx.stroke();

        // events fallback (jqSignature compatible)
        canvas.dispatchEvent(new Event("mousedown", { bubbles: true }));
        canvas.dispatchEvent(new Event("mouseup", { bubbles: true }));
        canvas.dispatchEvent(new Event("change", { bubbles: true }));

        return "SIGNATURE_OK";

    } catch (e) {
        return "ERR_" + e;
    }
};


/* ================================
   VALIDATION
================================ */
window.isSignatureValid = function () {

    var container = document.getElementById("contenuSignature");
    if (!container) return false;

    var canvas = container.querySelector("canvas");
    if (!canvas) return false;

    var ctx = canvas.getContext("2d");
    if (!ctx) return false;

    var data = ctx.getImageData(0, 0, canvas.width, canvas.height).data;

    return data.some(function (p) { return p !== 0; });
};