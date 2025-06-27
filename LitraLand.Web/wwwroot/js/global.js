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

// toast for success message
function showSuccessToast(message = 'Success!') {
    Swal.fire({
        icon: 'success',
        title: '<strong style="color: #28a745;">✔️ Success!</strong>',
        text: message,
        timer: 5000,
        showConfirmButton: false,
        toast: true,
        position: 'top-end',
        timerProgressBar: true, // 
        customClass: {
            popup: 'custom-toast-position' // 
        },
        didOpen: (toast) => {
            toast.addEventListener('mouseenter', () => Swal.stopTimer()); // 
            toast.addEventListener('mouseleave', () => Swal.resumeTimer()); // 
        }
    });
}

// toast for error message 
function showErrorToast(message = 'Something went wrong!') {
    Swal.fire({
        icon: 'error',
        title: '<strong style="color: #ff4c4c;">❌ Error!</strong>',
        text: message.responseText != undefined ? message.responseText : message,
        timer: 5000,
        showConfirmButton: false,
        toast: true,
        position: 'top-end',
        timerProgressBar: true, //
        customClass: {
            popup: 'custom-toast-position' // 
        },
        didOpen: (toast) => {
            toast.addEventListener('mouseenter', () => Swal.stopTimer()); // 
            toast.addEventListener('mouseleave', () => Swal.resumeTimer()); // 
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
function disableSubmitButton(btn) {
    setTimeout(function () {
        // لو الزرار مش موجود، استخدم كل الأزرار المتاحة
        if (!btn || btn.length === 0)
            btn = disableAllSubmitButtons();

        // لو لسه مفيش زرار، متعملش أي حاجة
        if (!btn || btn.length === 0)
            return;

        // Disable button to prevent multiple submits
        $(btn).attr('disabled', 'disabled');

        // Activate indicator
        $(btn).attr('data-kt-indicator', 'on');
    }, 500);
}

// Helper function to disable all submit buttons
function disableAllSubmitButtons() {
    return $('body :submit'); // 'body :submit' means select all submit buttons in the body
}


// Function to enable the submit button and remove the loading indicator
function enableSubmitButton(btn) {
    setTimeout(function () {
        // لو الزرار مش موجود، استخدم كل الأزرار المتاحة
        if (!btn || btn.length === 0)
            btn = enableAllSubmitButtons();

        // لو لسه مفيش زرار، متعملش أي حاجة
        if (!btn || btn.length === 0)
            return;

        // Enable button
        $(btn).removeAttr('disabled');

        // Deactivate indicator
        $(btn).removeAttr('data-kt-indicator');
    }, 500);
}

// Helper function to enable all submit buttons
function enableAllSubmitButtons() {
    return $('body :submit'); // 'body :submit' means select all submit buttons in the body
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

// Function to reinitialize datepicker
function applyDatepicker() {
    $('.js-datepicker').daterangepicker({
        singleDatePicker: true, // to show only one calendar
        autoApply: true, // to close the calendar when a date is selected
        maxDate: new Date(), // to disable future dates
        drops: 'up', // to show the calendar above the input field
    });
}

// end Global functions

// start Modal functions
function onModalBegin() {
    disableSubmitButton($('#Modal').find(':submit'));
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


// start password feilds logic 
function setupPasswordStrengthMeter() {
    const passwordInput = document.getElementById("passwordField");
    const meterSegments = document.querySelectorAll("[data-meter-segment]");

    if (!passwordInput || !meterSegments.length) return;

    const conditions = [
        function hasUpperCase(pw) {
            return /[A-Z]/.test(pw);
        },
        function hasSpecialChar(pw) {
            return /[!@#$%^&*(),.?":{}|<>]/.test(pw);
        },
        function hasNumberAndLowerCase(pw) {
            return /[0-9]/.test(pw) && /[a-z]/.test(pw);
        },
        function isLongEnough(pw) {
            return pw.length >= 8;
        }
    ];

    passwordInput.addEventListener("input", function () {
        const password = passwordInput.value;
        // حساب عدد الشروط المتحققة
        let trueCount = 0;
        conditions.forEach(function (cond) {
            if (cond(password)) {
                trueCount++;
            }
        });

        // تحديث لون الديفات: الديفات من اليسار للعدد trueCount تكون مضيئة
        meterSegments.forEach((segment, index) => {
            if (index < trueCount) {
                segment.classList.remove("bg-secondary");
                segment.classList.add("bg-success");
            } else {
                segment.classList.remove("bg-success");
                segment.classList.add("bg-secondary");
            }
        });
    });
}


function togglePasswordVisibility() {
    // get the toggle button
    var toggleButton = document.getElementById("togglePassword");

    // Check if the password field and toggle button exist
    if (!toggleButton) return;

    // Add event listener to the toggle button
    toggleButton.addEventListener("click", function () {
        var icon = this.querySelector("i");
        var passwordField = document.getElementById("passwordField");
        if (passwordField.type === "password") {
            passwordField.type = "text";
            icon.classList.remove("fa-eye");
            icon.classList.add("fa-eye-slash");
        } else {
            passwordField.type = "password";
            icon.classList.remove("fa-eye-slash");
            icon.classList.add("fa-eye");
        }
    });
}
// end password feilds logic