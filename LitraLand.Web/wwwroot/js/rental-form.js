let selectedCopies = []; // Array to store the selected copies

function updateSubmitButtonState() {
    const submitButton = document.getElementById("btnSubmit");
    if (selectedCopies.length === 0) {
        submitButton.classList.add("d-none");
        submitButton.disabled = true;
    } else {
        submitButton.classList.remove("d-none");
        submitButton.disabled = false;
    }
}

// helper function to update the selectedCopies array after adding or removing a copy
function updateSelectedCopies() {
    selectedCopies = []; // Clear the array before updating it to avoid duplicates

    let copies = $('.js-copy'); // Get all the copies in the form

    // Loop through all the copies and update the name and id attributes to match the index in the array (to make it work with the model binding)
    copies.each(function (index, input) {
        let $input = $(input); // Convert the input to a jQuery object to use the attr method
        $input.attr('name', `selectedCopies[${index}]`); // selectedCopies[0], selectedCopies[1], selectedCopies[2], ... (to match the model binding)
        $input.attr('id', `selectedCopies_${index}_`); // selectedCopies_0_, selectedCopies_1_, selectedCopies_2_, ...
        selectedCopies.push({ serial: $input.val(), bookId: $input.data('book-id') });
        // note: (ممكن نكتفي بالاتربيوت الي اسمه "نيم" بس لان هو ده الي بيحصل بيه ربط لما بنبعت الفورم للاكشن)
    });

    // Update the submit button state
    updateSubmitButtonState();
}

// the function that will be called when the search request to the server is successful
function onAddCopySuccess(copy) {
    $('.js-book-copy-search-input').val(''); // Clear the search input

    let bookId = $(copy).find('.js-copy').data('book-id'); // Get the book id of the selected copy

    // Check if this book is already selected
    if (selectedCopies.some(x => x.bookId === bookId)) {
        showErrorToast('This book is already selected');
        return;
    }

    // Add the returned copy to the form
    $('#CopiesForm').prepend(copy);

    // Update the selectedCopies array after adding the new copy to the form
    updateSelectedCopies();
}


// events to handle the search and remove copy buttons
document.addEventListener("DOMContentLoaded", function () {
    $('.js-search-button').on('click', function (e) {
        // check if the user has selected the maximum allowed copies 
        if (selectedCopies.length >= maxAllowedCopies) {
            e.preventDefault();
            showErrorToast(`You can only Add ${maxAllowedCopies} Book(s)`);
            return;
        }

        // Check if the copy is already selected
        var serial = $('.js-book-copy-search-input').val(); // the value of the input search
        if (selectedCopies.some(x => x.serial === serial)) {
            e.preventDefault();
            showErrorToast('This copy is already selected');
            return;
        }
    });

    $(document).on('click', '.js-remove-copy-button', function () {
        var item = $(this).closest('.js-book-item'); // the item to remove
        var separator = item.next('.separator'); // the separator after the item

        // remove the item and the separator with a fade out effect
        item.fadeOut(300, function () {
            $(this).remove();
            separator.fadeOut(300, function () {
                $(this).remove();
            });

            // after removing the item, update the selectedCopies array
            setTimeout(updateSelectedCopies, 350);
        });
    });

    // prevent the form from submitting if there are no selected copies
    $(document).on('submit', '#CopiesForm', function (e) {
        if (selectedCopies.length === 0) {
            e.preventDefault();
            showErrorToast('Please select at least one copy');
        }
    });

    // Update the selectedCopies array after loading the page
    updateSelectedCopies();
});