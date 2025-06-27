document.addEventListener("DOMContentLoaded", function () {

    $('body').on('click', '.js-delete-community-book', function () {
        const btn = $(this);
        const name = btn.data("name") || "item";
        const url = btn.data("url");

        if (!url) {
            showErrorMessage("Missing delete URL.");
            return;
        }

        bootbox.confirm({
            title: `<i class="fa fa-exclamation-circle text-warning"></i> <span class="fw-bolder fs-5">Confirm Action</span>`,
            message: `<p class="text-dark fs-6">
                        <strong class="fw-bolder">Are you sure you want to delete this <span class="text-primary">${name}</span>?</strong>
                      </p>`,
            buttons: {
                confirm: {
                    label: '<i class="fa fa-check"></i> <span class="fw-bolder">Yes, Delete</span>',
                    className: 'btn btn-danger btn-sm'
                },
                cancel: {
                    label: '<i class="fa fa-times"></i> <span class="fw-bolder">No, Cancel</span>',
                    className: 'btn btn-secondary btn-sm'
                }
            },
            callback: function (result) {
                if (!result) return;

                $.ajax({
                    url: url,
                    type: "POST",
                    data: {
                        '__RequestVerificationToken': $('input[name="__RequestVerificationToken"]').val()
                    },
                    beforeSend: function () {
                        showLoadingMessage("Processing...", "Deleting, please wait...");
                    },
                    success: function (response) {
                        Swal.close();

                        if (response && response.success) {
                            const bookContainer = btn.closest('.community-book-container');
                            const redirectUrl = btn.data("redirect-url"); 

                            if (redirectUrl) {
                                showSuccessToast(response.message || "Item deleted successfully.");
                                setTimeout(() => {
                                    window.location.href = redirectUrl;
                                }, 1000);
                                return; // stop further processing
                            }

                            // remove the book container from DOM
                            bookContainer.fadeOut(400, function () {
                                $(this).remove();
                                showSuccessToast(response.message || "Item deleted successfully.");
                            });
                        } else {
                            showErrorMessage(response?.message || "Failed to delete item.");
                        }
                    },
                    error: function (xhr, status, error) {
                        Swal.close();

                        let errorMsg = "Something went wrong!";
                        if (xhr.responseJSON?.message) {
                            errorMsg = xhr.responseJSON.message;
                        } else if (xhr.responseText) {
                            errorMsg = xhr.responseText;
                        }

                        showErrorMessage(errorMsg);
                    }
                });
            }
        });
    });
});