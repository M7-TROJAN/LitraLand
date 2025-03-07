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
                            console.log(addedRow);

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
});