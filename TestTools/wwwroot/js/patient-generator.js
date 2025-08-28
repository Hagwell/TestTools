console.log('TEST: patient-generator.js loaded and running');

// Patient Generator JavaScript Functions
console.log('Patient Generator JS loaded');

$(document).ready(function () {
    console.log('Document ready - initializing PatientGenerator');
    initializePatientGenerator();
});

function initializePatientGenerator() {
    console.log('Initializing PatientGenerator event handlers');
    
    // Patient Generator tab event handlers
    $('#generateForm').on('submit', handleGeneratePatients);
    $('#exportBtn').on('click', handleExportPatients);
    $('#clearBtn').on('click', handleClearPatients);

    // Master Patient tab event handlers
    $('#generateMasterBtn').on('click', handleGenerateMasterPatient);
    // Use event delegation for All Wales button to ensure it always works
    $('#generateAllWalesBtn').on('click', handleGenerateAllWalesPatient);
    $('#clearMasterBtn').on('click', handleClearMasterPatient);
    $('#saveHL7Btn').on('click', handleSaveHL7);

    // Initialize UI state
    $('#saveHL7Btn').prop('disabled', true);
    $('#clearMasterBtn').prop('disabled', true);
    $('#exportBtn').prop('disabled', true);
    $('#clearBtn').prop('disabled', true);
}

// Patient Generator Functions
async function handleGeneratePatients(e) {
    console.log('handleGeneratePatients called');
    e.preventDefault();
    
    const count = parseInt($('#patientCount').val());
    console.log('Patient count:', count);
    const submitBtn = $('#generateForm button[type="submit"]');
    
    if (count <= 0 || count > 1000) {
        showAlert('Please enter a number between 1 and 1000.', 'warning');
        return;
    }

    try {
        console.log('Starting AJAX request');
        // Show loading state
        setLoadingState(submitBtn, true);
        
        const token = $('input[name="__RequestVerificationToken"]').val();
        const response = await $.ajax({
            url: '/Home/GeneratePatients',
            method: 'POST',
            data: { 
                count: count,
                __RequestVerificationToken: token
            }
        });

        console.log('AJAX response received:', response);
        // Update the patients table
        $('#patientsTable').html(response);
        
        // Enable action buttons
        $('#exportBtn').prop('disabled', false);
        $('#clearBtn').prop('disabled', false);
        showAlert(`Successfully generated ${count} patient(s)!`, 'success');
        
    } catch (xhr) {
        console.error('AJAX error:', xhr);
        const errorMessage = xhr.responseText || 'Error generating patients.';
        showAlert(errorMessage, 'danger');
        console.error('Error generating patients:', xhr);
    } finally {
        setLoadingState(submitBtn, false);
    }
}

function handleExportPatients() {
    try {
        const rows = [];
        const headers = ['NHS Number', 'Title', 'Surname', 'Forename', 'DOB', 'Address'];
        
        // Add headers
        rows.push(headers.join(','));
        
        // Extract data from table
        $('#patientsTable table tbody tr').each(function () {
            const row = [];
            $(this).find('td').each(function () {
                let cellText = $(this).text().replace(/\n|\r/g, '').trim();
                // Handle CSV escaping for commas and quotes
                if (cellText.includes(',') || cellText.includes('"')) {
                    cellText = '"' + cellText.replace(/"/g, '""') + '"';
                }
                row.push(cellText);
            });
            rows.push(row.join(','));
        });

        if (rows.length <= 1) {
            showAlert('No patient data to export.', 'warning');
            return;
        }

        // Create and download CSV
        const csvContent = rows.join('\n');
        const blob = new Blob([csvContent], { type: 'text/csv;charset=utf-8;' });
        const link = document.createElement('a');
        
        if (link.download !== undefined) {
            const url = URL.createObjectURL(blob);
            link.setAttribute('href', url);
            link.setAttribute('download', `patients_${new Date().toISOString().slice(0, 10)}.csv`);
            link.style.visibility = 'hidden';
            document.body.appendChild(link);
            link.click();
            document.body.removeChild(link);
            
            showAlert('Patient data exported successfully!', 'success');
        } else {
            showAlert('Export not supported in this browser.', 'danger');
        }
        
    } catch (error) {
        showAlert('Error exporting patient data.', 'danger');
        console.error('Export error:', error);
    }
}

function handleClearPatients() {
    $('#patientsTable').html('<div class="alert alert-info">No patients generated yet.</div>');
    $('#exportBtn, #clearBtn').prop('disabled', true);
    showAlert('Patient data cleared.', 'info');
}

// Master Patient HL7 Functions
async function handleGenerateMasterPatient() {
    console.log('handleGenerateMasterPatient called');
    const generateBtn = $('#generateMasterBtn');
    
    try {
        console.log('Starting HL7 generation request');
        setLoadingState(generateBtn, true);
        
        const response = await $.ajax({
            url: '/Home/GenerateMasterPatient',
            method: 'GET'
        });

        // Display HL7 message
        $('#hl7Message').text(response);
        $('#saveHL7Btn').prop('disabled', false);
        $('#clearMasterBtn').prop('disabled', false);

        showAlert('HL7 message generated successfully!', 'success');
        
    } catch (xhr) {
        const errorMessage = xhr.responseText || 'Error generating HL7 message.';
        showAlert(errorMessage, 'danger');
        console.error('Error generating HL7:', xhr);
    } finally {
        setLoadingState(generateBtn, false);
    }
}

function handleClearMasterPatient() {
    $('#hl7Message').text('');
    $('#saveHL7Btn').prop('disabled', true);
    showAlert('HL7 message cleared.', 'info');
}

function handleSaveHL7() {
    try {
        const hl7Content = $('#hl7Message').text().trim();
        
        if (!hl7Content) {
            showAlert('No HL7 message to save.', 'warning');
            return;
        }

        const blob = new Blob([hl7Content], { type: 'text/plain;charset=utf-8;' });
        const link = document.createElement('a');
        
        if (link.download !== undefined) {
            const url = URL.createObjectURL(blob);
            const timestamp = new Date().toISOString().replace(/[:.]/g, '-').slice(0, 19);
            
            link.setAttribute('href', url);
            link.setAttribute('download', `master_patient_${timestamp}.hl7`);
            link.style.visibility = 'hidden';
            document.body.appendChild(link);
            link.click();
            document.body.removeChild(link);
            
            showAlert('HL7 file saved successfully!', 'success');
        } else {
            showAlert('File download not supported in this browser.', 'danger');
        }
        
    } catch (error) {
        showAlert('Error saving HL7 file.', 'danger');
        console.error('Save HL7 error:', error);
    }
}

async function handleGenerateAllWalesPatient() {
    console.log('handleGenerateAllWalesPatients called');
    const generateBtn = $('#generateAllWalesBtn');

    try {
        console.log('Starting HL7 generation request');
        setLoadingState(generateBtn, true);

        const response = await $.ajax({
            url: '/Home/GenerateAllWalesPatients',
            method: 'GET'
        });

        // Display HL7 message
        $('#hl7Message').text(response);
        $('#saveHL7Btn').prop('disabled', false);
        $('#clearMasterBtn').prop('disabled', false);

        showAlert('All Wales HL7 messages generated successfully!', 'success');

    } catch (xhr) {
        const errorMessage = xhr.responseText || 'Error generating HL7 message.';
        showAlert(errorMessage, 'danger');
        console.error('Error generating HL7:', xhr);
    } finally {
        setLoadingState(generateBtn, false);
    }
}

// Utility Functions
function setLoadingState(button, isLoading) {
    if (isLoading) {
        button.prop('disabled', true);
        button.addClass('loading');
        const originalText = button.html();
        button.data('original-text', originalText);
        button.html('<i class="fas fa-spinner fa-spin"></i> Processing...');
    } else {
        button.prop('disabled', false);
        button.removeClass('loading');
        const originalText = button.data('original-text');
        if (originalText) {
            button.html(originalText);
        }
    }
}

function showAlert(message, type) {
    // Remove existing alerts
    $('.alert-dismissible').remove();
    
    const alertHtml = `
        <div class="alert alert-${type} alert-dismissible fade show mt-3" role="alert">
            <i class="fas fa-${getAlertIcon(type)}"></i> ${message}
            <button type="button" class="btn-close" data-bs-dismiss="alert"></button>
        </div>
    `;
    
    // Add alert to the active tab
    const activeTab = $('.tab-pane.active');
    activeTab.prepend(alertHtml);
    
    // Auto-dismiss after 5 seconds
    setTimeout(() => {
        $('.alert-dismissible').fadeOut(() => {
            $('.alert-dismissible').remove();
        });
    }, 5000);
}

function getAlertIcon(type) {
    const icons = {
        'success': 'check-circle',
        'danger': 'exclamation-triangle',
        'warning': 'exclamation-circle',
        'info': 'info-circle'
    };
    return icons[type] || 'info-circle';
}

// Tab switching animation
$('button[data-bs-toggle="tab"]').on('shown.bs.tab', function (e) {
    const target = $(e.target).attr('data-bs-target');
    $(target).addClass('fade-in');
    
    setTimeout(() => {
        $(target).removeClass('fade-in');
    }, 300);
});

// Input validation
$('#patientCount').on('input', function() {
    const value = parseInt($(this).val());
    const submitBtn = $('#generateForm button[type="submit"]');
    
    if (value <= 0 || value > 1000 || isNaN(value)) {
        submitBtn.prop('disabled', true);
        $(this).addClass('is-invalid');
    } else {
        submitBtn.prop('disabled', false);
        $(this).removeClass('is-invalid');
    }
});

// Keyboard shortcuts
$(document).on('keydown', function(e) {
    // Ctrl+G for generate patients
    if (e.ctrlKey && e.key === 'g') {
        e.preventDefault();
        if ($('#generator').hasClass('active')) {
            $('#generateForm').submit();
        } else {
            $('#generateMasterBtn').click();
        }
    }
    
    // Ctrl+E for export
    if (e.ctrlKey && e.key === 'e') {
        e.preventDefault();
        if (!$('#exportBtn').prop('disabled')) {
            $('#exportBtn').click();
        }
    }
    
    // Ctrl+R for clear/reset
    if (e.ctrlKey && e.key === 'r') {
        e.preventDefault();
        if ($('#generator').hasClass('active')) {
            $('#clearBtn').click();
        } else {
            $('#clearMasterBtn').click();
        }
    }
});
