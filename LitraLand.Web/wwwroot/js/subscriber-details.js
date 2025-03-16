document.addEventListener("DOMContentLoaded", function () {

    $('.js-renew').on('click', function () {
        let btn = $(this);
        let subscriberkey = btn.data('key');

        bootbox.confirm({
            title: `<i class="fa fa-question-circle text-warning"></i> Confirm Renewal`,
            message: `<p class="text-dark fs-6">
                            <strong class="fw-bolder">Are you sure you want to renew this <span class="text-primary">subscription</span>?</strong>
                      </p>`,
            buttons: {
                confirm: {
                    label: '<i class="fa fa-sync"></i> Renew Subscription',
                    className: 'btn btn-success btn-sm'
                },
                cancel: {
                    label: '<i class="fa fa-ban"></i> Cancel',
                    className: 'btn btn-secondary btn-sm'
                }
            },
            callback: function (result) {
                if (result) {
                    $.ajax({
                        url: `/Subscribers/RenewSubscription?sKey=${subscriberkey}`,
                        type: "POST",
                        data: { // Send anti-forgery token
                            '__RequestVerificationToken': $('input[name="__RequestVerificationToken"]').val()
                        },
                        beforeSend: function () {
                            showLoadingMessage("Processing...", "Renew, please wait...");
                        },
                        success: function (row) {
                            Swal.close(); // Close the loading message

                            // get the table and append the new row
                            let table = $('#SubscriptionsTable');
                            table.append(row);

                            // get the last row and animate it to indicate success
                            var addedRow = table.find("tr:last-child");

                            addedRow.addClass("animate__flash animate__animated bg-light-success");

                            // Remove the animation classes after animation ends
                            addedRow.on('animationend', function () {
                                addedRow.removeClass("animate__flash animate__animated bg-light-success");
                            });

                            // Update the card icon and color
                            var activeIcon = $(`#ActiveStatusIcon`);
                            activeIcon.removeClass('d-none'); // Show the active icon
                            activeIcon.siblings('svg').remove(); // Remove the other icons
                            activeIcon.parents('.card').removeClass('bg-warning').addClass('bg-success'); // Change the card color

                            // Update the card status
                            $('#CardStatus').text('Active Subscriber'); // Update the card status

                            // Update the status badge
                            $('#StatusBadge')
                                .removeClass('badge-light-warning')
                                .addClass('badge-light-success')
                                .text('Active subscriber');

                            // show the add rental button if hidden
                            $('#AddRentalButton').removeClass('d-none');

                            // Show success message
                            showSuccessToast('Subscription has been renewed successfully');
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

    $('.js-cancel-rental').on('click', function () {
        let btn = $(this);
        let rentalId = btn.data('id');

        bootbox.confirm({
            title: `<i class="fa fa-question-circle text-warning"></i> Cancel Rental`,
            message: `<p class="text-dark fs-6">
                            <strong class="fw-bolder">Are you sure you want to Cancel this <span class="text-primary">Rental</span>?</strong>
                      </p>`,
            buttons: {
                confirm: {
                    label: '<i class="fa fa-sync"></i> Yes',
                    className: 'btn btn-danger btn-sm'
                },
                cancel: {
                    label: '<i class="fa fa-ban"></i> No',
                    className: 'btn btn-secondary btn-sm'
                }
            },
            callback: function (result) {
                if (result) {
                    $.ajax({
                        url: `/Rentals/MarkAsDeleted/${rentalId}`,
                        type: "POST",
                        data: { // Send anti-forgery token
                            '__RequestVerificationToken': $('input[name="__RequestVerificationToken"]').val()
                        },
                        beforeSend: function () {
                            showLoadingMessage("Processing...", "Canceling Rental, please wait...");
                        },
                        success: function (copiesCount) {
                            Swal.close(); // Close the loading message

                            let rentalsCountElement = $('.number-of-rentals-count');
                            let currentCount = parseInt(rentalsCountElement.text()) || 0;

                            if (currentCount > 0) {
                                rentalsCountElement.text(--currentCount);
                            }

                            // select the row and animate it and remove it
                            var row = btn.parents('tr');

                            row.addClass("animate__fadeOut animate__animated");

                            // Remove the animation classes after animation ends
                            row.on('animationend', function () {
                                row.remove();

                                // check if the table has no rwos left
                                let rows = $('#RentalsTable tbody tr'); // '#RentalsTable tbody tr' means all rows in the table body of the 'RentalsTable' table

                                if (rows.length === 0) {
                                    $('#RentalsTable').fadeOut(); // Hide the table
                                    $('#Alert').fadeIn(); // Show the Alert div
                                    rentalsCountElement.text(0);
                                }
                            });

                            // Show success message
                            showSuccessToast('The rental has been Canceled successfully');
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
});