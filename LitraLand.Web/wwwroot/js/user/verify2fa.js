function focusWhenReady() {
    const firstInput = document.getElementById("firstInput");
    if (firstInput)
        firstInput.focus();
    else
        setTimeout(focusWhenReady, 100);
}

document.addEventListener("DOMContentLoaded", function () {

    focusWhenReady();

    document.querySelectorAll('input[type="text"]').forEach((input, index, inputs) => {
        input.addEventListener('input', () => {
            if (input.value && index < inputs.length - 1) {
                inputs[index + 1].focus();
            }
        });

        input.addEventListener('keydown', (e) => {
            if (e.key === 'Backspace' && !input.value && index > 0) {
                inputs[index - 1].focus();
            }
        });
    });


});
// end Document Ready