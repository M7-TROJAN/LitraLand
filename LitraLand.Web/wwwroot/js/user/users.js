document.addEventListener("DOMContentLoaded", function () {
    // Handle lock user
    $('body').on('click', '.js-lock-user', function () {
        var btn = $(this);
        var name = btn.data("name") || "item"; // Default fallback if data-name is missing

        bootbox.confirm({
            title: `
            <div class="d-flex align-items-center">
                <i class="fa fa-lock text-danger fs-3 me-2"></i>
                <span class="fw-bold fs-5 text-dark">Lock User</span>
            </div>`,
            message: `
            <div class="text-center">
                <p class="text-muted fs-6 mb-3">
                    <strong class="fw-bold">Are you sure you want to lock <span class="text-danger">${name}</span>?</strong>
                </p>
            </div>`,
            buttons: {
                confirm: {
                    label: '<i class="fa fa-lock"></i> <span class="fw-bold">Yes, Lock</span>',
                    className: 'btn btn-danger btn-sm px-4 shadow-sm'
                },
                cancel: {
                    label: '<i class="fa fa-times"></i> <span class="fw-bold">Cancel</span>',
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
                            showLoadingMessage("Processing...", "Locking user, please wait...");
                        },
                        success: function (data) {
                            Swal.close();

                            if (!data || !data.lastUpdatedOn) {
                                showErrorMessage("Unexpected response from the server!");
                                return;
                            }

                            var row = btn.closest("tr");

                            // Update "Last Updated On" field
                            row.find(".js-updated-on").text(data.lastUpdatedOn);

                            // Animate the row to indicate success
                            row.addClass("animate__flash animate__animated bg-light-warning");

                            // Remove the animation classes after animation ends
                            row.on('animationend', function () {
                                row.removeClass("animate__flash animate__animated bg-light-warning");
                            });

                            // Show success toast message
                            showSuccessMessage(data.message || "User has been locked successfully!");
                        },
                        error: function (xhr) {
                            Swal.close();
                            showErrorMessage(xhr.responseText || "Something went wrong!");
                        }
                    });
                }
            }
        });
    });


    // Handle unlock user
    $('body').on('click', '.js-unlock-user', function () {
        var btn = $(this);
        var name = btn.data("name") || "item"; // Default fallback if data-name is missing

        bootbox.confirm({
            title: `
                        <div class="d-flex align-items-center">
                            <i class="fa fa-unlock-alt text-success fs-3 me-2"></i>
                            <span class="fw-bold fs-5 text-dark">Unlock User</span>
                        </div>`,
            message: `
                        <div class="text-center">
                            <p class="text-muted fs-6 mb-3">
                                <strong class="fw-bold">Are you sure you want to unlock <span class="text-primary">${name}</span>?</strong>
                            </p>
                        </div>`,
            buttons: {
                confirm: {
                    label: '<i class="fa fa-unlock"></i> <span class="fw-bold">Yes, Unlock</span>',
                    className: 'btn btn-success btn-sm px-4 shadow-sm'
                },
                cancel: {
                    label: '<i class="fa fa-times"></i> <span class="fw-bold">Cancel</span>',
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
                            showLoadingMessage("Processing...", "Unlocking user, please wait...");
                        },
                        success: function (data) {
                            Swal.close();

                            if (!data || !data.lastUpdatedOn) {
                                showErrorMessage("Unexpected response from the server!");
                                return;
                            }

                            var row = btn.closest("tr");

                            // Update "Last Updated On" field
                            row.find(".js-updated-on").text(data.lastUpdatedOn);

                            // Animate the row to indicate success
                            row.addClass("animate__flash animate__animated bg-light-success");

                            // Remove the animation classes after animation ends
                            row.on('animationend', function () {
                                row.removeClass("animate__flash animate__animated bg-light-success");
                            });

                            // Show success toast message
                            showSuccessMessage(data.message || "User has been unlocked successfully!");
                        },
                        error: function (xhr) {
                            Swal.close();
                            showErrorMessage(xhr.responseText || "Something went wrong!");
                        }
                    });
                }
            }
        });
    });
});