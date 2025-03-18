var currentCopies = [];
var selectedCopies = [];

function updateSubmitButtonState() {
    if (selectedCopies.length === 0 || JSON.stringify(currentCopies) == JSON.stringify(selectedCopies))
        $('#btnSubmit').addClass('d-none');
    else
        $('#btnSubmit').removeClass('d-none');
}

function updateSelectedCopies() {
    selectedCopies = [];

    let copies = $('.js-copy');

    copies.each(function (index, input) {
        let $input = $(input);
        $input.attr('name', `selectedCopies[${index}]`);
        $input.attr('id', `selectedCopies_${index}_`);
        selectedCopies.push({ serial: $input.val(), bookId: $input.data('book-id') });
        // note: (ممكن نكتفي بالاتربيوت الي اسمه "نيم" بس لان هو ده الي بيحصل بيه ربط لما بنبعت الفورم للاكشن)
    });

    updateSubmitButtonState();
}

function onAddCopySuccess(copy) {
    $('.js-book-copy-search-input').val(''); // Clear the search input

    let bookId = $(copy).find('.js-copy').data('book-id'); // Get the book id of the selected copy

    // Check if this book is already selected
    if (selectedCopies.some(x => x.bookId === bookId)) {
        showErrorToast('This book is already selected');
        return;
    }

    // Check if this book is in the removed copies
    let removedCopy = $(`.js-removed[data-book-id=${bookId}]`);
    if (removedCopy.length > 0) {
        showErrorToast('You have already removed this book. You can re-add it if needed.');
        return;
    }

    // Add the returned copy to the form
    $('#CopiesForm').prepend(copy);
    $('#CopiesForm').find(':submit').removeClass('d-none');

    // Update the selectedCopies array after adding the new copy to the form
    updateSelectedCopies();
}

document.addEventListener("DOMContentLoaded", function () {

    updateSelectedCopies();
    currentCopies = selectedCopies;

    $('.js-search-button').on('click', function (e) {
        // check if the user has selected the maximum allowed copies 
        if (selectedCopies.length >= maxAllowedCopies) {
            e.preventDefault();
            showErrorToast(`You can only Add ${maxAllowedCopies} Book(s)`);
            return;
        }

        // Check if the copy is already selected
        var serial = $('.js-book-copy-search-input').val(); // the value of the input search
        if (selectedCopies.find(c => c.serial == serial)) {
            e.preventDefault();
            showErrorToast('You cannot add the same copy');
            return;
        }

        // Check if this book is in the removed copies
        let removedCopy = $(`.js-removed input[value=${serial}]`);
        if (removedCopy.length > 0) {
            showErrorToast('You have already removed this book. You can re-add it if needed.');
            return;
        }

    });

    $(document).on('click', '.js-remove-copy-button', function () {
        var btn = $(this);
        let itemContainer = btn.closest('.js-copy-container');

        btn.toggleClass('btn-light-danger btn-light-success js-remove-copy-button js-readd-button').text('Re-Add');
        itemContainer.find('img').css('opacity', '0.5');
        itemContainer.find('h4').css('text-decoration', 'line-through');
        itemContainer.find('.js-copy').toggleClass('js-copy js-removed').removeAttr('name').removeAttr('id');

        updateSelectedCopies();
    });

    $(document).on('click', '.js-readd-button', function () {
        var btn = $(this);
        var itemContainer = btn.closest('.js-copy-container');

        // check if the user already selected the maximum allowed copies and he tries to add more
        if (selectedCopies.length >= maxAllowedCopies) {
            showErrorToast(`You can only Add ${maxAllowedCopies} Book(s)`);
            return;
        }

        // check if the user has select the same copy of the removed copy (مش هتحصل اصلا لان السيرفر هيرجع انها اصلا متاجرة)
        var serial = itemContainer.find('.js-removed').val();
        if (selectedCopies.find(c => c.serial == serial)) {
            showErrorToast('You cannot add the same copy');
            return;
        }

        // check if the user has select a copy of the same book of the removed copy
        var bookId = itemContainer.find('.js-removed').data('book-id');
        if (selectedCopies.some(x => x.bookId === bookId)) {
            showErrorToast('You cannot add a copy of the same book');
            return;
        }

        btn.toggleClass('btn-light-danger btn-light-success js-remove-copy-button js-readd-button').text('Remove');
        itemContainer.find('img').css('opacity', '1');
        itemContainer.find('h4').css('text-decoration', 'none');
        itemContainer.find('.js-removed').toggleClass('js-copy js-removed');

        updateSelectedCopies();
    });

    updateSelectedCopies();
});