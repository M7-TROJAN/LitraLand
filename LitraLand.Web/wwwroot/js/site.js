// start Global variables
let updatedRow = null; // variable to store the updated row in the datatable if we are updating an existing row
var table; // variable to store the table element that we are using for DataTables (this is the table that has the class js-datatables)
var datatable; // variable to store the DataTable object 
var exportedColumns = []; // array to store the columns that will be exported to PDF, Excel, CSV, etc.
// end Global variables

// start Global functions
// Function to show a success message using SweetAlert2
function showSuccessMessage(message = 'Success!') {
    Swal.fire({
        icon: 'success',
        title: 'Success!',
        text: message,
        customClass: {
            confirmButton: 'btn btn-primary'
        }
    });
}

// Function to show an error message using SweetAlert2
function showErrorMessage(message = 'Something went wrong!') {
    Swal.fire({
        icon: 'error',
        title: 'Oops...!',
        text: message.responseText != undefined ? message.responseText : message,
        customClass: {
            confirmButton: 'btn btn-primary'
        }
    });
}

// Function to show a loading message using SweetAlert2 while processing a request to the server
function showLoadingMessage(title = "Processing...", message = "Please wait...") {
    Swal.fire({
        title: title,
        text: message,
        allowOutsideClick: false,
        didOpen: () => {
            Swal.showLoading();
        }
    });
}

// Function to disable the submit button and show the loading indicator
function disableSubmitButton() {
    setTimeout(function () {
        // Get the submit button
        var button = $('body :submit'); // 'body :submit' means select all submit buttons in the body

        // Disable button to prevent multiple submits
        button.attr('disabled', 'disabled'); // the first 'disabled' is the attribute name, and the second 'disabled' is the attribute value

        // Activate indicator
        button.attr('data-kt-indicator', 'on');
    }, 500);
}

// Function to enable the submit button and remove the loading indicator
function enableSubmitButton() {
    setTimeout(function () {
        // Get the submit button
        var button = $('body :submit');

        // Enable the button
        button.removeAttr('disabled');

        // Deactivate indicator
        button.removeAttr('data-kt-indicator');
    }, 500);
}

// Function to reinitialize select2
function applySelect2() {
    $('.js-select2').select2();
    // Trigger validation when a select2 value is changed
    $('.js-select2').on('select2:select', function (e) {
        var select = $(this);
        $('form').not('#SignOutForm').validate().element('#' + select.attr('id')); // # + select.attr('id') = #CategoryId, #AuthorId, etc.
    });
}

// end Global functions

// start Modal functions
function onModalBegin() {
    disableSubmitButton();
}
function onModalSuccess(item) {
    $('#Modal').modal('hide');
    showSuccessMessage();

    // If the updatedRow is not null, then we are updating an existing row in a table (datatable)
    if (updatedRow !== null) {
        datatable.row(updatedRow).remove().draw(); // Remove the updated row from the datatable and redraw the table
        updatedRow = null; // Reset the updatedRow variable to null
    }

    var newRow = $(item);
    datatable.row.add(newRow).draw(); // Add the new row to the datatable and redraw the table

    //KTMenu.init(); // Reinitialize the menu for Metronic theme
    //KTMenu.initGlobalHandlers(); // Reinitialize global handlers

}

function onModalError(errorMessage) {
    $('#Modal').modal('hide');
    showErrorMessage(errorMessage);
}

function onModalComplete() {
    enableSubmitButton();
}
// end Modal functions

// start DataTables
var headers = $('th');
// Loop through each header and add the index of the column to the exportedColumns array if the column does not have the class js-no-export
headers.each(function (index) {
    var column = $(this); // Get the current header
    if (!column.hasClass('js-no-export')) {
        exportedColumns.push(index);
    }
});

// Function to customize PDF layout
function customizePDF(doc) {
    // Center the title and make it green
    doc.styles.title = {
        alignment: 'center',
        fontSize: 22,
        bold: true,
        color: '#009879',
        margin: [0, 20, 0, 20] // [top, right, bottom, left]
    };

    // Table header styling
    doc.styles.tableHeader = {
        alignment: 'center',
        bold: true,
        fontSize: 12,
        color: '#ffffff', // White text
        fillColor: '#009879' // Green header background
    };

    // Table body styling
    var tableBody = doc.content[1].table.body;
    var rowCount = tableBody.length;

    // Apply alternating row colors (odd rows with light grey background)
    for (var i = 1; i < rowCount; i++) {
        if (i % 2 === 0) {
            tableBody[i].forEach(function (cell) {
                cell.fillColor = '#f3f3f3'; // Light grey for even rows
            });
        }
    }

    // Add borders to each cell
    doc.styles.tableBodyOdd = {
        alignment: 'center',
        border: [0.5, 0.5, 0.5, 0.5], // Thin border for all cells
        borderColor: '#ddd', // Light grey border
        padding: 12 // Padding for better readability
    };

    // Center table content
    doc.content[1].table.widths = Array(tableBody[0].length + 1).join('*').split('');
    doc.content[1].alignment = 'center';

    // Add footer with page numbers
    doc.footer = function (currentPage, pageCount) {
        return {
            text: currentPage.toString() + ' of ' + pageCount,
            alignment: 'center',
            margin: [0, 10, 0, 0],
            fontSize: 10
        };
    };

    // Default text styling for the entire table
    doc.defaultStyle = {
        fontSize: 10
    };

    // Styling for even rows (white background)
    doc.styles.tableBodyEven = {
        fillColor: '#ffffff',
        border: [0.5, 0.5, 0.5, 0.5],
        borderColor: '#ddd',
        padding: 12
    };
}


// Function to customize print layout
function customizePrint(win) {
    // Modify table header
    $(win.document.body).find('h1').css('text-align', 'center').css('margin-bottom', '30px');

    // Style table and center text in cells
    $(win.document.body).find('table').addClass('table-bordered').css('width', '100%');
    $(win.document.body).find('table th, table td').css({
        'border': '1px solid #ddd',
        'text-align': 'center',
        'vertical-align': 'middle',
        'padding': '8px'
    });

    // Hide unnecessary elements
    $(win.document.body).find('.dataTables_length, .dataTables_filter, .dataTables_info, .dataTables_paginate').css('display', 'none');
}

// Class definition
var KTDatatables = function () {
    // Private functions
    var initDatatable = function () {
        // Init datatable --- more info on datatables: https://datatables.net/manual/
        datatable = $(table).DataTable({
            "info": false,
            'pageLength': 10,
            // Add drawCallback function to redraw the menu for Metronic theme
            "drawCallback": function (settings) {
                KTMenu.init(); // Reinitialize the menu for Metronic theme
                KTMenu.initHandlers(); // Reinitialize menu handlers
            }
        });
    }

    // Hook export buttons
    var exportButtons = () => {
        const documentTitle = $('.js-datatables').data('document-title');
        var buttons = new $.fn.dataTable.Buttons(table, {
            buttons: [
                {
                    extend: 'copyHtml5',
                    title: documentTitle,
                    exportOptions: {
                        columns: exportedColumns
                    },
                },
                {
                    extend: 'excelHtml5',
                    title: documentTitle
                },
                {
                    extend: 'csvHtml5',
                    title: documentTitle,
                    exportOptions: {
                        columns: exportedColumns
                    }
                },
                {
                    extend: 'pdfHtml5',
                    title: documentTitle,
                    exportOptions: {
                        columns: exportedColumns
                    },
                    customize: customizePDF
                },
                {
                    extend: 'print',
                    title: documentTitle,
                    exportOptions: {
                        columns: exportedColumns // Define the columns that will be printed
                    },
                    customize: customizePrint
                }

            ]
        }).container().appendTo($('#kt_datatable_example_buttons'));

        // Hook dropdown menu click event to datatable export buttons
        const exportButtons = document.querySelectorAll('#kt_datatable_example_export_menu [data-kt-export]');
        exportButtons.forEach(exportButton => {
            exportButton.addEventListener('click', e => {
                e.preventDefault();

                // Get clicked export value
                const exportValue = e.target.getAttribute('data-kt-export');
                const target = document.querySelector('.dt-buttons .buttons-' + exportValue);

                // Trigger click event on hidden datatable export buttons
                target.click();
            });
        });
    }

    // Search Datatable --- official docs reference: https://datatables.net/reference/api/search()
    var handleSearchDatatable = () => {
        const filterSearch = document.querySelector('[data-kt-filter="search"]');
        filterSearch.addEventListener('keyup', function (e) {
            datatable.search(e.target.value).draw();
        });
    }

    // Public methods
    return {
        init: function () {
            table = document.querySelector('.js-datatables');

            if (!table) {
                return;
            }

            initDatatable();
            exportButtons();
            handleSearchDatatable();
        }
    };
}();
// end DataTables


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
                            showSuccessMessage(data.message ? data.message : 'Item status has been toggled successfully');
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

    // select2
    applySelect2();
    // end select2

    // Datepicker
    $('.js-datepicker').daterangepicker({
        singleDatePicker: true, // to show only one calendar
        autoApply: true, // to close the calendar when a date is selected
        maxDate: new Date(), // to disable future dates
        drops: 'up', // to show the calendar above the input field
    });
    // end Datepicker

    // TinyMCE
    // check if the textarea with the class js-tinymce exists on the page before initializing TinyMCE
    if ($('.js-tinymce').length > 0) {
        var options = {
            selector: ".js-tinymce",
            height: "430",
            plugins: [
                // Core editing features
                'anchor', 'autolink', 'charmap', 'codesample', 'emoticons', 'image', 'link', 'lists', 'media', 'searchreplace', 'table', 'visualblocks', 'wordcount',
            ],
        };

        if (KTThemeMode.getMode() === "dark") {
            options["skin"] = "oxide-dark";
            options["content_css"] = "dark";
        }
        tinymce.init(options);
    }
    // end TinyMCE

    // disable submit button in forms to prevent multiple submits and show loading indicator
    $('form').not('#SignOutForm').on('submit', function () {
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

        if (isValid)
            disableSubmitButton(form);
    });

    // Handle sign out
    $('.js-sign-out').on('click', function () {
        $('#SignOutForm').trigger('submit');

    });
});
// end Document Ready


// Re-enable the submit button after an AJAX request is completed
$(document).ajaxComplete(function () {
    enableSubmitButton();
});

$(document).ajaxSuccess(function () {
    enableSubmitButton();
});