
// start Document Ready
document.addEventListener("DOMContentLoaded", function () { // This is the same as $(document).ready(function () {});
    // sweetalert2
    let message = $('#Message').text();
    if (message !== '') {
        showSuccessMessage(message);
    }
    // end sweetalert2

    // DataTables
    KTDatatables.init();
    // end DataTables

    // select2
    applySelect2();
    // end select2

    // Datepicker
    applyDatepicker();
    // end Datepicker

    // Handle Bootstrap's Modal
    $('body').on('click', '.js-render-modal', function () {
        let btn = $(this); // Button that triggered the modal (this is the button that we clicked on, and it has the class js-render-modal)
        let modal = $('#Modal'); // Modal that we want to show
        let modalTitleLabel = modal.find('#ModalLabel'); // Modal's title label

        modalTitleLabel.text(btn.data('title')); // Set the title of the modal to the data-title attribute of the button

        if (btn.data('update') !== undefined) { // If the button has a data-update attribute
            updatedRow = btn.parents('tr'); // Set the updatedRow variable to the parent tr of the button
        }

        $.ajax({
            url: btn.data('url'),
            type: 'GET',
            success: function (form) {
                modal.find('.modal-body').html(form); // Set the modal's body to the form that we got from the server
                $.validator.unobtrusive.parse(modal); // Reinitialize unobtrusive validation on the modal form elements (this is needed because the form is loaded dynamically and the validation needs to be reinitialized)
                modal.modal('show');// Show the modal
                applySelect2(); // Reinitialize select2
                applyDatepicker(); // Reinitialize datepicker
            },
            error: function (error) {
                showErrorMessage(error ? error : 'An error occurred while loading the form');
            }
        });

    });
    // end Handle Bootstrap's Modal

    // Handle Toggle Status
    $('body').on('click', '.js-toggle-status', function () {
        var btn = $(this);
        var name = btn.data("name") || "item"; // Default fallback if data-name is missing

        bootbox.confirm({
            title: `<i class="fa fa-exclamation-circle text-warning"></i> <span class="fw-bolder fs-5">Confirm Action</span>`,
            message: `<p class="text-dark fs-6">
                        <strong class="fw-bolder">Are you sure you want to toggle the status of this <span class="text-primary">${name}</span>?</strong>
                      </p>`,
            buttons: {
                confirm: {
                    label: '<i class="fa fa-check"></i> <span class="fw-bolder">Yes, Toggle</span>',
                    className: 'btn btn-danger btn-sm'
                },
                cancel: {
                    label: '<i class="fa fa-times"></i> <span class="fw-bolder">No, Cancel</span>',
                    className: 'btn btn-secondary btn-sm'
                }
            },
            callback: function (result) {
                if (result) {
                    $.ajax({
                        url: btn.data("url"),
                        type: "POST",
                        data: { // Send anti-forgery token
                            '__RequestVerificationToken': $('input[name="__RequestVerificationToken"]').val()
                        },
                        beforeSend: function () {
                            showLoadingMessage("Processing...", "Toggling status, please wait...");
                        },
                        success: function (data) {
                            Swal.close(); // Close the loading message
                            // Ensure data contains the expected properties before using them (Message (optional), LastUpdatedOn))
                            if (!data || !data.lastUpdatedOn) { 
                                showErrorMessage('Unexpected response from the server!');
                                return;
                            }

                            var row = btn.parents("tr");
                            var status = row.find(".js-status");
                            var newStatus = status.text().trim() === "Available" ? "Deleted" : "Available";

                            status.text(newStatus);
                            status.toggleClass("badge-light-success badge-light-danger");

                            // Update the "Last Updated On" field
                            var lastUpdatedOnElement = row.find(".js-updated-on");
                            lastUpdatedOnElement.text(data.lastUpdatedOn);

                            // Animate the row to indicate success
                            row.addClass("animate__flash animate__animated bg-light-success");

                            // Remove the animation classes after animation ends
                            row.on('animationend', function () {
                                row.removeClass("animate__flash animate__animated bg-light-success");
                            });

                            // Show success message using the returned message from the server
                            showSuccessToast(data.message ? data.message : 'Item status has been toggled successfully');
                        },
                        error: function (errorMessage) {
                            Swal.close(); // Close the loading message
                            showErrorMessage(errorMessage ? errorMessage : 'Something went wrong!');
                        }
                    });
                }
            }
        });
    });
    // end Handle Toggle Status

    // begin Handle Confirm
    $('body').on('click', '.js-confirm', function () {
        var btn = $(this);
        var title = btn.data('title') || 'Confirm Action';
        var message = btn.data('message') || 'Are you sure you want to perform this action?';

        bootbox.confirm({
            title: `
                <div class="d-flex align-items-center">
                    <i class="fa fa-exclamation-circle text-warning fs-3 me-2"></i>
                    <span class="fw-bold fs-5 text-dark">${title}</span>
                </div>`,
            message: `
                <div class="text-center">
                    <p class="text-muted fs-6 mb-3">
                        <strong class="fw-bold">${message}</strong>
                    </p>
                </div>`,
            buttons: {
                confirm: {
                    label: '<i class="fa fa-check-circle"></i> <span class="fw-bold">Confirm</span>',
                    className: 'btn btn-success btn-sm px-4 shadow-sm'
                },
                cancel: {
                    label: '<i class="fa fa-times-circle"></i> <span class="fw-bold">Cancel</span>',
                    className: 'btn btn-light btn-sm px-4 shadow-sm'
                }
            },
            backdrop: true,
            callback: function (result) {
                if (result) {
                    $.ajax({
                        url: btn.data("url"),
                        type: "POST",
                        data: {
                            '__RequestVerificationToken': $('input[name="__RequestVerificationToken"]').val()
                        },
                        beforeSend: function () {
                            showLoadingMessage();
                        },
                        success: function (message) {
                            Swal.close();

                            // Show success message
                            showSuccessMessage(message || "Action has been performed successfully");
                        },
                        error: function (xhr) {
                            Swal.close();
                            showErrorMessage(xhr.responseText || "An error occurred while processing the request");
                        }
                    });
                }
            }
        });
    });
    // end Handle Confirm

    // begin handle the physical delete action
    $('body').on('click', '.js-physical-delete', function () {
        var btn = $(this);
        var name = btn.data("name") || "item"; // Default fallback if data-name is missing

        bootbox.confirm({
            title: `<i class="fa fa-exclamation-triangle text-danger"></i> <span class="fw-bolder fs-5">Delete ${name} Record (Danger)</span>`,
            message: `<p class="text-dark fs-6">
                        <strong class="fw-bolder">Are you sure you want to delete this ${name} record?</strong><br>
                        This action <span class="text-danger fw-bolder">cannot</span> be undone.
                      </p>`,
            buttons: {
                confirm: {
                    label: '<i class="fa fa-trash"></i> <span class="fw-bolder">Yes, Delete</span>',
                    className: 'btn btn-danger btn-sm'
                },
                cancel: {
                    label: '<i class="fa fa-times"></i> <span class="fw-bolder">No, Cancel</span>',
                    className: 'btn btn-secondary btn-sm'
                }
            },
            callback: function (result) {
                if (result) {
                    $.ajax({
                        url: btn.data("url"),
                        type: "POST",
                        data: { // Send anti-forgery token
                            '__RequestVerificationToken': $('input[name="__RequestVerificationToken"]').val()
                        },
                        beforeSend: function () {
                            showLoadingMessage("Processing...", "Deleting record, please wait...");
                        },
                        success: function (successMessage) {
                            Swal.close(); // Close the loading message
                            var row = btn.parents("tr");
                            datatable.row(row).remove().draw(false);
                            showSuccessMessage(successMessage ? successMessage : 'record has been deleted successfully');
                        },
                        error: function (errorMessage) {
                            Swal.close(); // Close the loading message
                            showErrorMessage(errorMessage ? errorMessage : 'An error occurred while deleting the record');
                        }
                    });
                }
            }
        });
    });
    // end handle the physical delete action

    // TinyMCE
    // check if the textarea with the class js-tinymce exists on the page before initializing TinyMCE
    if ($('.js-tinymce').length > 0) {
        var options = {
            selector: ".js-tinymce",
            height: "430",
            plugins: [
                // Core editing features
                'anchor', 'autolink', 'charmap', 'codesample', 'emoticons', 'image', 'link', 'lists', 'media', 'searchreplace', 'table', 'visualblocks', 'wordcount', 'fontsize'
            ],
            toolbar: 'undo redo | bold italic underline | fontsizeselect | alignleft aligncenter alignright | bullist numlist outdent indent | removeformat',
            fontsize_formats: '10pt 12pt 14pt 16pt 18pt 24pt 36pt',
            content_style: 'body { font-size: 14pt; }'
        };

        if (KTThemeMode.getMode() === "dark") {
            options["skin"] = "oxide-dark";
            options["content_css"] = "dark";
        }

        tinymce.init(options);
    }
    // end TinyMCE

    // disable submit button in forms to prevent multiple submits and show loading indicator
    $('form').not('#SignOutForm').on('submit', function (e) {
        /*
            // old code
            var form = $(this);
            var isValid = form.valid(); // Check if the form is valid or not

            // If the form is valid, disable the submit button and show the loading indicator
            if (isValid)
                disableSubmitButton();
        */

        var form = $(this);
        var validator = form.validate();

        // Validate the form before disabling the submit button
        validator.form();

        var isValid = form.valid();

        if (!isValid) {
            e.preventDefault(); // ❌ امنع الإرسال فعليًا
            return; // ❌ متشغلش disableSubmitButton
        }

        if (isValid)
            disableSubmitButton(form.find(':submit'));
    });

    // Handle sign out
    $('.js-sign-out').on('click', function () {
        $('#SignOutForm').trigger('submit');

    });

    // Handle password strength meter
    setupPasswordStrengthMeter();

    // Handle password visibility toggle
    togglePasswordVisibility();

});
// end Document Ready


// Re-enable the submit button after an AJAX request is completed
$(document).ajaxComplete(function () {
    enableSubmitButton();
});

$(document).ajaxSuccess(function () {
    enableSubmitButton();
});