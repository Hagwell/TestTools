// webScraper.js - Handles front-end logic for the Web Scraper page
$(document).ready(function () {
    let urls = [];
    let scrapedElements = [];
    
    // Initialize button states on page load
    $('#generatePOMBtn').prop('disabled', true);

    // URL validation function
    function isValidUrl(string) {
        try {
            const url = new URL(string);
            // Only allow http and https protocols
            return url.protocol === 'http:' || url.protocol === 'https:';
        } catch (_) {
            return false;
        }
    }

    // Show validation feedback
    function showUrlValidation(isValid, message = '') {
        const $input = $('#urlInput');
        const $feedback = $('#urlValidationFeedback');
        
        if (isValid) {
            $input.removeClass('is-invalid').addClass('is-valid');
            $feedback.removeClass('invalid-feedback').addClass('valid-feedback').text('Valid URL');
        } else {
            $input.removeClass('is-valid').addClass('is-invalid');
            $feedback.removeClass('valid-feedback').addClass('invalid-feedback').text(message || 'Please enter a valid URL (must start with http:// or https://)');
        }
    }

    // Clear validation feedback
    function clearUrlValidation() {
        const $input = $('#urlInput');
        const $feedback = $('#urlValidationFeedback');
        $input.removeClass('is-valid is-invalid');
        $feedback.text('');
    }

    function updateUrlList() {
        const $list = $('#urlList');
        $list.empty();
        urls.forEach((url, idx) => {
            $list.append(`<li class="list-group-item d-flex justify-content-between align-items-center">
                <span>${url}</span>
                <button class="btn btn-sm btn-danger remove-url-btn" data-idx="${idx}">Remove</button>
            </li>`);
        });
        $('#noUrlsMessage').toggle(urls.length === 0);
        $('#scrapeBtn').prop('disabled', urls.length === 0);
    }

    $('#addUrlBtn').on('click', function () {
        const url = $('#urlInput').val().trim();
        
        if (!url) {
            showUrlValidation(false, 'Please enter a URL');
            return;
        }
        
        if (!isValidUrl(url)) {
            showUrlValidation(false, 'Please enter a valid URL (must start with http:// or https://)');
            return;
        }
        
        if (urls.includes(url)) {
            showUrlValidation(false, 'This URL has already been added');
            return;
        }
        
        // URL is valid and unique, add it to the list
        urls.push(url);
        $('#urlInput').val('');
        clearUrlValidation();
        updateUrlList();
    });
    $('#urlInput').on('keypress', function (e) {
        if (e.which === 13) {
            $('#addUrlBtn').click();
        }
    });
    
    // Real-time URL validation as user types
    $('#urlInput').on('input', function () {
        const url = $(this).val().trim();
        
        if (!url) {
            clearUrlValidation();
            return;
        }
        
        if (isValidUrl(url)) {
            if (urls.includes(url)) {
                showUrlValidation(false, 'This URL has already been added');
            } else {
                showUrlValidation(true);
            }
        } else {
            showUrlValidation(false, 'Please enter a valid URL (must start with http:// or https://)');
        }
    });
    $('#urlList').on('click', '.remove-url-btn', function () {
        const idx = $(this).data('idx');
        urls.splice(idx, 1);
        updateUrlList();
    });

    $('#scrapeBtn').on('click', function () {
        if (urls.length === 0) return;
        $('#loadingSpinner').removeClass('d-none');
        $('#resultsSection').addClass('d-none');
        // Disable the Generate POM button while scraping
        $('#generatePOMBtn').prop('disabled', true);
        $.ajax({
            url: '/Home/Scrape',
            method: 'POST',
            contentType: 'application/json',
            data: JSON.stringify({ urls }),
            success: function (result) {
                $('#loadingSpinner').addClass('d-none');
                if (result.success) {
                    scrapedElements = result.elements;
                    renderElementsTable();
                    $('#resultsSection').removeClass('d-none');
                    // Enable the Generate POM button when scraped elements are available
                    $('#generatePOMBtn').prop('disabled', false);
                } else {
                    alert('Scraping failed: ' + (result.errorMessage || 'Unknown error'));
                    // Keep Generate POM button disabled on failure
                    $('#generatePOMBtn').prop('disabled', true);
                }
            },
            error: function (xhr) {
                $('#loadingSpinner').addClass('d-none');
                alert('Scraping failed: ' + xhr.responseText);
                // Keep Generate POM button disabled on error
                $('#generatePOMBtn').prop('disabled', true);
            }
        });
    });

    function renderElementsTable() {
        const $tbody = $('#elementsTable tbody');
        $tbody.empty();
        scrapedElements.forEach((el, idx) => {
            $tbody.append(`<tr>
                <td><input class="form-check-input element-select" type="checkbox" data-idx="${idx}" checked></td>
                <td>${el.name}</td>
                <td>${el.id}</td>
                <td class="d-none d-lg-table-cell">${el.relativeXPath}</td>
                <td class="d-none d-xl-table-cell">${el.fullXPath}</td>
            </tr>`);
        });
        // Update the select all state after rendering
        updateSelectAllState();
    }

    // Function to update the header checkbox state and button states based on individual checkboxes
    function updateSelectAllState() {
        const $checkboxes = $('#elementsTable tbody .element-select');
        const totalCheckboxes = $checkboxes.length;
        const checkedCheckboxes = $checkboxes.filter(':checked').length;
        
        const $selectAllCheckbox = $('#selectAll');
        const $selectAllBtn = $('#selectAllBtn');
        const $deselectAllBtn = $('#deselectAllBtn');
        
        if (checkedCheckboxes === 0) {
            // No checkboxes selected - Deselect All is active
            $selectAllCheckbox.prop('checked', false);
            $selectAllCheckbox.prop('indeterminate', false);
            
            // Update button states - Deselect All is active (primary), Select All is inactive (outline)
            $selectAllBtn.removeClass('btn-primary').addClass('btn-outline-primary');
            $deselectAllBtn.removeClass('btn-outline-primary').addClass('btn-primary');
        } else if (checkedCheckboxes === totalCheckboxes) {
            // All checkboxes selected - Select All is active
            $selectAllCheckbox.prop('checked', true);
            $selectAllCheckbox.prop('indeterminate', false);
            
            // Update button states - Select All is active (primary), Deselect All is inactive (outline)
            $selectAllBtn.removeClass('btn-outline-primary').addClass('btn-primary');
            $deselectAllBtn.removeClass('btn-primary').addClass('btn-outline-primary');
        } else {
            // Some checkboxes selected (indeterminate state) - neither button is fully active
            $selectAllCheckbox.prop('checked', false);
            $selectAllCheckbox.prop('indeterminate', true);
            
            // Update button states - both outlined to show partial state
            $selectAllBtn.removeClass('btn-primary').addClass('btn-outline-primary');
            $deselectAllBtn.removeClass('btn-primary').addClass('btn-outline-primary');
        }
    }

    // Select All button click
    $('#selectAllBtn').on('click', function () {
        $('#elementsTable tbody .element-select').prop('checked', true);
        $('#selectAll').prop('checked', true);
        $('#selectAll').prop('indeterminate', false);
        
        // Update button states immediately
        $(this).removeClass('btn-outline-primary').addClass('btn-primary');
        $('#deselectAllBtn').removeClass('btn-primary').addClass('btn-outline-primary');
    });

    // Deselect All button click
    $('#deselectAllBtn').on('click', function () {
        $('#elementsTable tbody .element-select').prop('checked', false);
        $('#selectAll').prop('checked', false);
        $('#selectAll').prop('indeterminate', false);
        
        // Update button states immediately
        $(this).removeClass('btn-outline-primary').addClass('btn-primary');
        $('#selectAllBtn').removeClass('btn-primary').addClass('btn-outline-primary');
    });

    // Header checkbox click
    $('#selectAll').on('click', function () {
        const isChecked = $(this).prop('checked');
        $('#elementsTable tbody .element-select').prop('checked', isChecked);
        $(this).prop('indeterminate', false);
    });

    // Individual checkbox change event
    $(document).on('change', '#elementsTable tbody .element-select', function () {
        updateSelectAllState();
    });

    $('#generatePOMBtn').on('click', function () {
        const selected = scrapedElements.filter((el, idx) => $('#elementsTable tbody .element-select').eq(idx).prop('checked'));
        if (selected.length === 0) {
            alert('Please select at least one element to generate the POM.');
            return;
        }
        $.ajax({
            url: '/Home/GeneratePOM',
            method: 'POST',
            contentType: 'application/json',
            data: JSON.stringify(selected),
            xhrFields: { responseType: 'blob' },
            success: function (blob, status, xhr) {
                const filename = xhr.getResponseHeader('Content-Disposition')?.split('filename=')[1]?.replace(/"/g, '') || 'PageObjectModel.txt';
                const url = window.URL.createObjectURL(blob);
                const a = document.createElement('a');
                a.href = url;
                a.download = filename;
                document.body.appendChild(a);
                a.click();
                setTimeout(() => {
                    window.URL.revokeObjectURL(url);
                    document.body.removeChild(a);
                }, 0);
            },
            error: function (xhr) {
                alert('Failed to generate POM: ' + xhr.responseText);
            }
        });
    });
});
