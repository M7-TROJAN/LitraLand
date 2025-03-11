// Set default page size (number of subscribers per page)
const pageSize = 9;

// Function to update the pagination display in the UI
const updatePaginationDisplay = (page, pageSize, totalRecords) => {
    // Calculate the first record number on the current page
    const from = (page - 1) * pageSize + 1;
    // Calculate the last record number on the current page
    const to = Math.min(page * pageSize, totalRecords);
    // Update the UI elements with calculated values
    $('#currentPage').text(from);
    $('#currentPageTo').text(to);
    $('#totalEntries').text(totalRecords);
};


// Function to render subscriber cards in the container
const renderSubscribers = (subscribers) => {
    const container = $('#subscribersContainer');
    container.empty();

    // If no subscribers were found, display the alert message
    if (subscribers.length === 0) {
        container.html(`
            <div class="alert bg-light-warning border border-warning border-3 border-dashed d-flex flex-column flex-sm-row align-items-center justify-content-between w-100 p-5">
                <div class="d-flex flex-column">
                    <span>No Subscribers were found!</span>
                </div>
                <a href="/Subscribers/Create" class="btn btn-primary ms-sm-auto mt-5 mt-sm-0">Add Subscriber</a>
            </div>
        `);
        return;
    }

    // Loop over each subscriber and create the card HTML
    $.each(subscribers, (index, subscriber) => {
        const card = `
                <div class="col-md-6 col-xxl-4">
                    <div class="card">
                        <div class="card-body d-flex flex-center flex-column pt-12 p-9">
                            <div class="symbol symbol-65px symbol-circle mb-5">
                                <img src="${subscriber.imageThumbnailUrl || '~/assets/images/avatar.png'}" alt="image">
                            </div>
                            <span class="fs-4 text-gray-800 text-hover-primary fw-bold mb-0">${subscriber.fullName}</span>
                            <div class="fw-semibold text-gray-500 mb-6">${subscriber.email}</div>
                            <div>
                                <!-- Link to subscriber details page using the encrypted key -->
                                <a href="/Subscribers/Details/${subscriber.key}" class="btn btn-primary">
                                    Details
                                </a>
                            </div>
                        </div>
                    </div>
                </div>`;
        container.append(card);

    });
};

// Function to render pagination controls based on total records and current page
const renderPagination = (totalRecords, currentPage) => {
    const totalPages = Math.ceil(totalRecords / pageSize);
    const pagination = $('.pagination');
    pagination.empty();

    // Previous button
    pagination.append(`
            <li class="page-item ${currentPage === 1 ? 'disabled' : ''}">
                <a href="#" class="page-link" data-page="${currentPage - 1}">«</a>
            </li>
        `);

    // Page number buttons
    for (let i = 1; i <= totalPages; i++) {
        pagination.append(`
                <li class="page-item ${i === currentPage ? 'active' : ''}">
                    <a href="#" class="page-link" data-page="${i}">${i}</a>
                </li>
            `);
    }

    // Next button
    pagination.append(`
            <li class="page-item ${currentPage === totalPages ? 'disabled' : ''}">
                <a href="#" class="page-link" data-page="${currentPage + 1}">»</a>
            </li>
        `);
};


// in document.ready
document.addEventListener("DOMContentLoaded", function () {

    // Function to load subscribers via AJAX
    const loadSubscribers = (page = 1, search = '') => {
        $.ajax({
            url: '/Subscribers/GetSubscribers',
            type: 'POST',
            data: {
                // parameters that are sent to the Action method
                start: (page - 1) * pageSize,
                length: pageSize,
                searchTerm: search
            },
            headers: {
                // Include the anti-forgery token
                "RequestVerificationToken": $('input[name="__RequestVerificationToken"]').val()
            },
            success: (response) => {
                // Render the subscriber cards and pagination links
                renderSubscribers(response.data);
                renderPagination(response.recordsTotal, page);
                // Update the pagination display (from, to, total entries)
                updatePaginationDisplay(page, pageSize, response.recordsTotal);
            },
            error: () => {
                showErrorMessage("An error occurred while loading subscribers.");
            }
        });
    };

    // Event handler for pagination link clicks
    $(document).on('click', '.pagination .page-link', (e) => {
        e.preventDefault(); // this means that the default action of the event will not be triggered (e.g. clicking a link)
        const page = $(e.currentTarget).data('page');
        if (page) {
            loadSubscribers(page, $('#kt_filter_search').val());
        }
    });

    // Event handler for search input keyup event
    $('#kt_filter_search').on('keyup', () => {
        var searchValue = $('#kt_filter_search').val();
        // Always load the first page for a new search query
        loadSubscribers(1, searchValue);
    });

    // Initial load of subscribers when the page is ready
    loadSubscribers();
});


// the old code that was replaced by the above code
/*
<script>
        $(document).ready(function () {
            let pageSize = 10;
            function loadSubscribers(page = 1, search = '') {
                $.ajax({
                    url: '/Subscribers/GetSubscribers',
                    type: 'POST',
                    data: {
                        start: (page - 1) * pageSize,
                        length: pageSize,
                        'search[value]': search
                    },
                    headers: { "RequestVerificationToken": $('input[name="__RequestVerificationToken"]').val() },
                    success: function (response) {
                        renderSubscribers(response.data);
                        renderPagination(response.recordsTotal, page);

                        let to = Math.min(page * pageSize, response.recordsTotal);
                        $('#currentPage').text(page);
                        $('#currentPageTo').text(to);
                        $('#totalEntries').text(response.recordsTotal);
                    },
                    error: function () {
                        showErrorMessage("An error occurred while loading subscribers.");
                    }
                });
            }

            function renderSubscribers(subscribers) {
                let container = $('#subscribersContainer');
                container.empty();
                $.each(subscribers, function (index, subscriber) {
                    let card = `
                        <div class="col-md-6 col-xxl-4">
                            <div class="card">
                                <div class="card-body d-flex flex-center flex-column pt-12 p-9">
                                    <div class="symbol symbol-65px symbol-circle mb-5">
                                        <img src="${subscriber.imageThumbnailUrl || '/default-avatar.jpg'}" alt="image">
                                    </div>
                                    <span class="fs-4 text-gray-800 text-hover-primary fw-bold mb-0">${subscriber.fullName}</span>
                                    <div class="fw-semibold text-gray-500 mb-6">${subscriber.email}</div>
                                    <div class="">
                                        <a href="/Subscribers/Details/${subscriber.key}" class="btn btn-primary">
                                            Details
                                        </a>
                                    </div>
                                </div>
                            </div>
                        </div>`;
                    container.append(card);
                });
            }

            function renderPagination(totalRecords, currentPage) {
                let totalPages = Math.ceil(totalRecords / pageSize);
                let pagination = $('.pagination');
                pagination.empty();

                pagination.append(`<li class="page-item ${currentPage === 1 ? 'disabled' : ''}">
                    <a href="#" class="page-link" data-page="${currentPage - 1}">«</a>
                </li>`);

                for (let i = 1; i <= totalPages; i++) {
                    pagination.append(`<li class="page-item ${i === currentPage ? 'active' : ''}">
                        <a href="#" class="page-link" data-page="${i}">${i}</a>
                    </li>`);
                }

                pagination.append(`<li class="page-item ${currentPage === totalPages ? 'disabled' : ''}">
                    <a href="#" class="page-link" data-page="${currentPage + 1}">»</a>
                </li>`);
            }

            $(document).on('click', '.pagination .page-link', function (e) {
                e.preventDefault();
                let page = $(this).data('page');
                if (page) {
                    loadSubscribers(page, $('#kt_filter_search').val());
                }
            });

            $('#kt_filter_search').on('keyup', function () {
                loadSubscribers(1, $(this).val());
            });

            loadSubscribers();
        });
</script>
*/