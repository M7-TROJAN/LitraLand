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