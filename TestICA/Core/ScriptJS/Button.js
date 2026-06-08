const button = document.querySelectorAll('button');
const res = []

button.forEach((button, index) => {
    res.push({
        id: index,
        text: button.textContent.trim(),
        isActive: !button.disabled
    });
});

return JSON.stringify(res);