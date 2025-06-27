
document.addEventListener("DOMContentLoaded", function () {

    // handle the click event on the remove avatar button to set the value of the hidden input 'Input_ImageRemoved' to true
    $('.js-remove-avatar').on('click', function () {
        $('#Input_ImageRemoved').val(true);
    });

    // Load areas based on the selected governorate
    $('#Input_GovernorateId').on('change', function () {
        var governorateId = $(this).val();

        var areasDropdown = $('#Input_AreaId');

        areasDropdown.empty();

        //add the first option to the dropdown list (the default option)
        areasDropdown.append('<option></option>');

        if (governorateId != '') {
            $.ajax({
                url: '/Locations/GetAreas?governorateId=' + governorateId,
                type: 'GET',
                success: function (areas) {
                    $.each(areas, function (i, area) {
                        var item = $('<option></option>')
                            .attr('value', area.value) // Set the value attribute
                            .text(area.text); // Set the text content
                        areasDropdown.append(item);
                    });
                },
                error: function (error) {
                    showErrorToast('An error occurred while loading areas.');
                }
            });
        }
    });
});