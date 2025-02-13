document.addEventListener("DOMContentLoaded", function () { 

    // handle the search input to filter the table 
    // metronic theme hide the orginal search input of the datatable and add a new one so we need to handle it manually 
    // by get the value of the new search input and send it to the datatable orignal search input
    $('[data-kt-filter="search"]').on('keyup', function () {
        datatable.search(this.value).draw();
    });


    datatable = $('#Books').DataTable({
        serverSide: true, // Enable server-side processing (thats means the data will be fetched from the server)
        processing: true, // Enable the processing indicator ( the loading spinner)
        stateSave: true, // Enable state saving. When enabled aDataTables will store state information such as pagination position, display length, filtering and sorting so that it can be restored when the user reloads the page
        language: { // for the spinner style
            processing: '<div class="d-flex justify-content-center text-primary align-items-center dt-spinner"><div class="spinner-border" role="status"><span class="visually-hidden">Loading...</span></div><span class="text-muted ps-2">Loading...</span></div>'
        },
        ajax: { 
            url: '/Books/GetBooks', // The URL to fetch the data from the server
            type: 'POST', // why post not get? because we will sending a lot of data to the server in the request body (e.g. search term, pagination, sorting, etc.)
            headers: {
                'RequestVerificationToken': $('input[name="__RequestVerificationToken"]').val() // Send the anti-forgery token to the server 
            }
        },
        'drawCallback': function () {
            KTMenu.createInstances(); // Reinitialize the menu for Metronic theme after the table is drawn (menu is the actions button in the table)
        },
        order: [[1, 'asc']], // Order the table by the second column (index 1) in ascending order (title column)
        columnDefs: [ // Define the columns properties (e.g. width, visibility, etc.)
            {
                targets: 0, // The first column is the ID column
                visible: false, // We don't want to show the ID column
                searchable: false // We don't want to search in the ID column
            },
            {
                targets: 3, // Published Date column index
                width: "122.25px" // Set specific width
            },
            {
                targets: 6, // Status column index
                width: "61.25px" // Set specific width
            },
            {
                target: 7, // Actions column index (note if you will uncomment the categories column you should change this index to 8)
                width: "120px", // Set specific width
            }
        ],
        // note that tha "data" value should be written in camelCase (e.g. authorName) because the data that will be returned from the ajax request will be in camelCase, and the "name" value should be written as it written in the model (e.g. AuthorName)
        columns: [ // Define the columns that will be shown in the table and how to render them (e.g. format the date, show an image, etc.)
            { "data": "id", "name": "Id", "className": "d-none" }, // The first column is the ID column and we don't want to show it in the table but we need it to get the ID of the book when we click on the actions button to edit or delete the book 
            {
                "name": "Title",
                "className": "d-flex align-items-center", // Add a class to the cell to make it flex and align the items in the center
                "render": function (data, type, row) {
                    return `<div class="symbol symbol-50px overflow-hidden me-3">
                                <a href="/Books/Details/${row.id}"> 
                                    <div class="symbol-label h-70px">
                                        <img src="${(row.imageThumbnailUrl === null ? '/images/books/no-book.jpg' : row.imageThumbnailUrl)}" alt="cover" class="w-100">
                                    </div>
                                </a>
                            </div>
                            <div class="d-flex flex-column">
                                <a href="/Books/Details/${row.id}" class="text-primary fw-bolder mb-1">${row.title}</a>
                                <span>${row.author}</span>
                            </div>`;
                }
            },
            { "data": "publisher", "name": "Publisher" },
            {
                "name": "PublishedDate",
                "render": function (data, type, row) {
                    return moment(row.publishedDate).format('ll') // show the date in human readable format (e.g. Jan 1, 2021)
                }
            },
            { "data": "hall", "name": "Hall" },
            // { "data": "categories", "name": "Categories", "orderable": false }, 
            {
                "name": "IsAvailableForRental",
                "render": function (data, type, row) {
                    return `<span class="badge badge-light-${(row.isAvailableForRental ? 'success' : 'warning')}">
                                ${(row.isAvailableForRental ? 'Available' : 'Not Available')}
                            </span>`;
                }
            },
            {
                "name": "IsDeleted",
                "render": function (data, type, row) {
                    return `<span class="badge badge-light-${(row.isDeleted ? 'danger' : 'success')} js-status">
                                ${(row.isDeleted ? 'Deleted' : 'Available')}
                            </span>`;
                }
            },
            {
                "className": 'text-end',
                "orderable": false,
                "render": function (data, type, row) {
                    return `<a href="#" class="btn btn-light btn-active-light-primary btn-sm text-end" data-kt-menu-trigger="click" data-kt-menu-placement="bottom-end">
                                Actions
                                <span class="svg-icon svg-icon-5 m-0">
                                    <svg width="24" height="24" viewBox="0 0 24 24" fill="none" xmlns="http://www.w3.org/2000/svg">
                                        <path d="M11.4343 12.7344L7.25 8.55005C6.83579 8.13583 6.16421 8.13584 5.75 8.55005C5.33579 8.96426 5.33579 9.63583 5.75 10.05L11.2929 15.5929C11.6834 15.9835 12.3166 15.9835 12.7071 15.5929L18.25 10.05C18.6642 9.63584 18.6642 8.96426 18.25 8.55005C17.8358 8.13584 17.1642 8.13584 16.75 8.55005L12.5657 12.7344C12.2533 13.0468 11.7467 13.0468 11.4343 12.7344Z" fill="currentColor"></path>
                                    </svg>
                                </span>
                            </a>
                            <div class="menu menu-sub menu-sub-dropdown menu-column menu-rounded menu-gray-800 menu-state-bg-light-primary fw-semibold w-200px py-3" data-kt-menu="true" style="">
                                <div class="menu-item px-3">
                                    <a href="/Books/Edit/${row.id}" class="menu-link px-3">
                                        Edit
                                    </a>
                                </div>
                                <div class="menu-item px-3">
                                    <a href="javascript:;" class="menu-link flex-stack px-3 js-toggle-status" data-url="/Books/ToggleStatus/${row.id}">
                                        Toggle Status
                                    </a>
                                </div>
                                <div class="menu-item px-3">
                                    <a href="javascript:;" class="menu-link flex-stack px-3 js-physical-delete" data-url="/Books/Delete/${row.id}">
                                        physical delete
                                    </a>
                                </div>
                            </div>`;
                }
            },
        ]
    });

    // begin handle the physical delete action
    $('body').on('click', '.js-physical-delete', function () {
        var btn = $(this);

        bootbox.confirm({
            title: 'Delete Item (Danger)',
            message: 'Are you sure you want to delete this item! This action cannot be undone.',
            buttons: {
                confirm: {
                    label: 'Yes',
                    className: 'btn-danger'
                },
                cancel: {
                    label: 'No',
                    className: 'btn-secondary'
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
                        success: function (lastUpdatedOn) {
                            var row = btn.parents("tr");
                            datatable.row(row).remove().draw(false);
                            showSuccessMessage('item has been deleted successfully');
                        },
                        error: function () {
                            showErrorMessage('An error occurred while deleting the item');
                        }
                    });
                }
            }
        });
    });
    // end handle the physical delete action
});