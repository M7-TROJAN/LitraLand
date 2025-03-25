document.addEventListener("DOMContentLoaded", function () {
    $('#GovernorateId').on('change', function () {
        var governorateId = $(this).val();

        var areasDropdown = $('#AreaId');

        areasDropdown.empty();

        //add the first option to the dropdown list (the default option)
        areasDropdown.append('<option></option>');

        if (governorateId != '')
        {
            $.ajax({
                url: '/Subscribers/GetAreas?governorateId=' + governorateId,
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
                    showErrorMessage('An error occurred while loading areas.');
                }
            });
        }
    });

});