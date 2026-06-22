(function() {
    var cssSelector = arguments[0];
    var element = document.querySelector(cssSelector);

    if (element) {
        element.scrollIntoView({ behavior: 'smooth', block: 'center', inline: 'nearest' });
        return true;
    }
    return false;
})()