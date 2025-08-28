function openTab(evt, tabName) {
    // Hide all tabcontent
    var tabcontent = document.getElementsByClassName("tabcontent");
    for (var i = 0; i < tabcontent.length; i++) {
        tabcontent[i].style.display = "none";
    }

    // Remove 'active' class from all tablinks
    var tablinks = document.getElementsByClassName("tablinks");
    for (var i = 0; i < tablinks.length; i++) {
        tablinks[i].className = tablinks[i].className.replace(" active", "");
    }

    // Show the current tab and add 'active' class to the button
    document.getElementById(tabName).style.display = "block";
    evt.currentTarget.className += " active";
}

// Document ready function
$(document).ready(function () {
    // Open default tab on page load
    var defaultOpen = document.getElementById("defaultOpen");
    if (defaultOpen) {
        defaultOpen.click();
    }

    // AJAX upload for document forms
    $(document).on('submit', '.upload-document-form', function (e) {
        e.preventDefault();
        var $form = $(this);
        var tab = $form.data('tab');
        var formData = new FormData(this);
        // Determine which category is selected
        var category = $form.find('input[name="Category"]:checked').val();
        var tableId = '';
        var spinnerId = '';
        var tableUrl = '';
        if (category === 'Education') {
            tableId = '#education-docs-table';
            spinnerId = '#spinner-education';
            tableUrl = '/Home/GetDocumentsTable?category=Education';
        } else if (category === 'Framework') {
            tableId = '#framework-docs-table';
            spinnerId = '#spinner-framework';
            tableUrl = '/Home/GetDocumentsTable?category=Framework';
        } else if (category === 'Support') {
            tableId = '#support-docs-table';
            spinnerId = '#spinner-support';
            tableUrl = '/Home/GetDocumentsTable?category=Support';
        } else if (category === 'Instructions') {
            tableId = '#instructions-docs-table';
            spinnerId = '#spinner-instructions';
            tableUrl = '/Home/GetDocumentsTable?category=Instructions';
        } else {
            return;
        }

        // Show spinner, hide table
        $(spinnerId).removeClass('d-none');
        $(tableId).hide();

        $.ajax({
            url: $form.attr('action') || window.location.pathname,
            type: $form.attr('method'),
            data: formData,
            processData: false,
            contentType: false,
            success: function () {
                // Reload only the relevant table
                $.get(tableUrl, function (data) {
                    $(tableId).html(data);
                    $(spinnerId).addClass('d-none');
                    $(tableId).show();
                });
            },
            error: function (xhr) {
                var errorMsg = 'Upload failed.';
                if (xhr.responseText) errorMsg = xhr.responseText;
                $(spinnerId).addClass('d-none');
                $(tableId).show();
                $(tableId).prepend('<div class="alert alert-danger" role="alert">' + errorMsg + '</div>');
            }
        });
    });

    // Handle document viewing
    $(document).on('click', '.view-document', function () {
        const documentPath = $(this).data('document-path');
        const documentName = $(this).data('document-name');
        const tabcontent = $(this).closest('.tabcontent');

        // Set iframe source
        const iframe = tabcontent.find('.document-iframe');
        iframe.attr('src', documentPath);

        // Set document name
        tabcontent.find('.document-name').text(documentName);

        // Show viewer
        tabcontent.find('.document-viewer').removeClass('d-none');
    });

    // Handle document closing
    $(document).on('click', '.close-document', function () {
        const tabcontent = $(this).closest('.tabcontent');
        const iframe = tabcontent.find('.document-iframe');

        // Clear iframe source
        iframe.attr('src', '');

        // Hide viewer
        tabcontent.find('.document-viewer').addClass('d-none');
    });
});