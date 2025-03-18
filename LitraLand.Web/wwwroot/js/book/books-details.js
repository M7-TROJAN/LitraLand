function onAddCopySuccess(row)
{
     $('#Modal').modal('hide'); // Hide the modal
    showSuccessToast("Copy added successfully!"); // Show the success message

     $('tbody').prepend(row) // Add the new row to the table at the top

     KTMenu.init(); // Reinitialize the menu for Metronic theme
     KTMenu.initHandlers(); // Reinitialize menu handlers

     // Update the copies count
     var copiesCount = $('#CopiesCount');

     // Get the current count
     var newCount = parseInt(copiesCount.text()) + 1; // Add 1 to the current count because we added a new copy

     // Update the count
     copiesCount.text(newCount);

     // Hide the alert if it's visible
     $('.js-alert').addClass("d-none");

     // Show the table if it's hidden
     $('.table').removeClass('d-none')
}
function onEditCopySuccess(row)
{
    $('#Modal').modal('hide'); // Hide the modal
    showSuccessToast("Copy updated successfully!"); // Show the success message

    $(updatedRow).replaceWith(row); // Replace the row with the updated one

    KTMenu.init(); // Reinitialize the menu for Metronic theme
    KTMenu.initHandlers(); // Reinitialize menu handlers
}