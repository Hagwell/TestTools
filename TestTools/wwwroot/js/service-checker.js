// Service Checker JavaScript Functions

// Global variables
let isCheckingAll = false;

// Initialize when DOM is loaded
document.addEventListener('DOMContentLoaded', function() {
    initializeServiceChecker();
    initializeAddEndpoint();
    initializeViewMetrics();
    // Inject status key popover content for the Status column
    var statusKeyHtml = "<table class='table table-sm mb-0'><tr><td><span class='status-light status-active legend-dot'></span></td><td><strong>Active</strong></td><td>JSON Response: 200 OK, &lt; 200ms</td></tr><tr><td><span class='status-light status-slow legend-dot'></span></td><td><strong>Slow</strong></td><td>JSON Response: 200 OK, Response Time &gt; 500ms</td></tr><tr><td><span class='status-light status-unavailable legend-dot'></span></td><td><strong>Unavailable</strong></td><td>JSON Response: 500 OK, Response Time &lt; 200ms</td></tr><tr><td><span class='status-light status-unknown legend-dot'></span></td><td><strong>Unknown</strong></td><td>Endpoint not checked</td></tr></table>";
    var popoverBtn = document.getElementById('statusKeyPopoverBtn');
    if (popoverBtn) {
        new bootstrap.Popover(popoverBtn, {
            html: true,
            content: statusKeyHtml,
            trigger: 'focus',
            placement: 'bottom',
            title: 'Status Key'
        });
    }
    // Enable HTML popovers for all other popovers
    var popoverTriggerList = [].slice.call(document.querySelectorAll('[data-bs-toggle="popover"]'));
    popoverTriggerList.forEach(function (popoverTriggerEl) {
        if (popoverTriggerEl.id !== 'statusKeyPopoverBtn') {
            new bootstrap.Popover(popoverTriggerEl, { html: true });
        }
    });
});

function initializeServiceChecker() {
    // Add event listeners for individual check buttons
    document.querySelectorAll('.check-btn').forEach(btn => {
        btn.addEventListener('click', function () {
            const row = this.closest('tr');
            checkEndpoint(row);
        });
    });

    // Add event listener for check all button
    const checkAllBtn = document.getElementById('checkAllBtn');
    if (checkAllBtn) {
        checkAllBtn.addEventListener('click', checkAllEndpoints);
    }
}

function setActionButtonsDisabled(disabled) {
    document.querySelectorAll('.action-btn').forEach(btn => {
        btn.disabled = disabled;
    });
}

function setStatus(cell, status, detail, responseTime) {
    const light = cell.querySelector('.status-light');
    const text = cell.querySelector('.status-text');
    const responseTimeCell = cell.closest('tr').querySelector('.response-time-text');
    
    // Reset classes
    light.className = 'status-light';
    
    // Set status light and text
    switch (status) {
        case 'Active': 
            light.classList.add('status-active'); 
            text.textContent = 'Active';
            break;
        case 'Slow': 
            light.classList.add('status-slow'); 
            text.textContent = 'Slow';
            break;
        case 'Unavailable': 
            light.classList.add('status-unavailable'); 
            text.textContent = 'Unavailable';
            break;
        default: 
            light.classList.add('status-unknown'); 
            text.textContent = 'Unknown';
    }
    
    // Set response time with appropriate styling
    if (responseTimeCell && responseTime !== undefined) {
        responseTimeCell.textContent = responseTime + 'ms';
        responseTimeCell.className = 'response-time-text';
        
        if (responseTime < 200) {
            responseTimeCell.classList.add('response-time-fast');
        } else if (responseTime < 500) {
            // Normal color
        } else if (responseTime < 1000) {
            responseTimeCell.classList.add('response-time-slow');
        } else {
            responseTimeCell.classList.add('response-time-very-slow');
        }
    }
    
    // Add tooltip with full details
    if (detail) {
        text.title = detail;
        if (responseTimeCell) {
            responseTimeCell.title = detail;
        }
    }
}

function updateMetricsAfterCheck() {
    // If metrics section is visible, update the chart
    const metricsSection = document.getElementById('serviceMetricsSection');
    if (metricsSection && !metricsSection.classList.contains('d-none')) {
        if (typeof initializeViewMetrics === 'function') {
            initializeViewMetrics();
        }
    }
}

async function checkEndpoint(row) {
    if (row.classList.contains('checking')) {
        return; // Already checking this endpoint
    }
    
    const url = row.getAttribute('data-url');
    const statusCell = row.querySelector('.status-cell');
    const checkBtn = row.querySelector('.check-btn');
    
    // Set checking state
    row.classList.add('checking');
    checkBtn.innerHTML = '<i class="fas fa-spinner fa-spin"></i> Checking...';
    setStatus(statusCell, 'Unknown', 'Checking...', 0);
    
    try {
        const response = await fetch(`/Home/CheckEndpoint?url=${encodeURIComponent(url)}`);
        const data = await response.json();
        setStatus(statusCell, data.status, data.statusDetail, data.responseTimeMs);
    } catch (error) {
        setStatus(statusCell, 'Unavailable', 'Network error: ' + error.message, 0);
    } finally {
        // Reset checking state
        row.classList.remove('checking');
        checkBtn.innerHTML = '<i class="fas fa-check"></i> Check';
        updateMetricsAfterCheck(); // Update metrics after individual check
    }
}

// Updated: Check endpoints one at a time, updating UI after each
async function checkAllEndpoints() {
    if (isCheckingAll) {
        return; // Already checking all
    }
    isCheckingAll = true;
    setActionButtonsDisabled(true); // Disable all action buttons
    const checkAllBtn = document.getElementById('checkAllBtn');
    const originalText = checkAllBtn.innerHTML;
    checkAllBtn.innerHTML = '<i class="fas fa-sync-alt spinning"></i> Checking All...';
    checkAllBtn.disabled = true;
    const rows = document.querySelectorAll('#apiTable tbody tr');
    // Disable all check buttons
    rows.forEach(row => {
        const checkBtn = row.querySelector('.check-btn');
        checkBtn.disabled = true;
    });
    for (const row of rows) {
        row.classList.add('checking');
        const checkBtn = row.querySelector('.check-btn');
        checkBtn.innerHTML = '<i class="fas fa-spinner fa-spin"></i> Checking...';
        const statusCell = row.querySelector('.status-cell');
        setStatus(statusCell, 'Unknown', 'Checking...', 0);
        const url = row.getAttribute('data-url');
        try {
            const response = await fetch(`/Home/CheckEndpoint?url=${encodeURIComponent(url)}`);
            const data = await response.json();
            setStatus(statusCell, data.status, data.statusDetail, data.responseTimeMs);
        } catch (error) {
            setStatus(statusCell, 'Unavailable', 'Network error: ' + error.message, 0);
        }
        row.classList.remove('checking');
        checkBtn.innerHTML = '<i class="fas fa-check"></i> Check';
    }
    // Re-enable all check buttons
    rows.forEach(row => {
        const checkBtn = row.querySelector('.check-btn');
        checkBtn.disabled = false;
    });
    checkAllBtn.innerHTML = originalText;
    checkAllBtn.disabled = false;
    isCheckingAll = false;
    setActionButtonsDisabled(false); // Re-enable all action buttons
    updateMetricsAfterCheck(); // Update metrics after group check
    // Future: For true streaming, consider SignalR or server events
}

// Quick action functions
function checkActiveServices() {
    const activeRows = document.querySelectorAll('#apiTable tbody tr').forEach(row => {
        const statusText = row.querySelector('.status-text').textContent;
        if (statusText === 'Active' || statusText === 'Unknown') {
            checkEndpoint(row);
        }
    });
}

function checkFailedServices() {
    const failedRows = document.querySelectorAll('#apiTable tbody tr').forEach(row => {
        const statusText = row.querySelector('.status-text').textContent;
        if (statusText === 'Unavailable' || statusText === 'Slow') {
            checkEndpoint(row);
        }
    });
}

function exportResults() {
    // Export Service Status Table
    const results = [];
    const rows = document.querySelectorAll('#apiTable tbody tr');
    rows.forEach(row => {
        const name = row.querySelector('td:first-child strong').textContent;
        const url = row.getAttribute('data-url');
        const status = row.querySelector('.status-text').textContent;
        const responseTime = row.querySelector('.response-time-text').textContent;
        results.push({
            name: name,
            url: url,
            status: status,
            responseTime: responseTime,
            timestamp: new Date().toISOString()
        });
    });
    let csvContent = 'Service Status Table\n';
    csvContent += 'Service Name,URL,Status,Response Time,Checked At\n';
    csvContent += results.map(r => `"${r.name}","${r.url}","${r.status}","${r.responseTime}","${r.timestamp}"`).join("\n");

    // Export Service Metrics Table (if available)
    csvContent += '\n\nService Metrics Table\n';
    if (window.lastMetricsData && Array.isArray(window.lastMetricsData) && window.lastMetricsData.length > 0) {
        csvContent += 'Service Name,Response Time (ms),Status\n';
        csvContent += window.lastMetricsData.map(m => `"${m.name}","${m.responseTimeMs}","${m.status}"`).join("\n");
    } else {
        csvContent += 'No metrics data available. Click View Metrics to generate.\n';
    }

    // Download CSV file
    const encodedUri = 'data:text/csv;charset=utf-8,' + encodeURIComponent(csvContent);
    const link = document.createElement("a");
    const dateStr = new Date().toISOString().split('T')[0];
    link.setAttribute("href", encodedUri);
    link.setAttribute("download", `service-check-results-${dateStr}.csv`);
    document.body.appendChild(link);
    link.click();
    document.body.removeChild(link);

    // Export metrics chart as PNG if present
    const metricsCanvas = document.getElementById('metricsChart');
    if (metricsCanvas && metricsCanvas.toDataURL) {
        // Give the chart a white background for export
        const tmpCanvas = document.createElement('canvas');
        tmpCanvas.width = metricsCanvas.width;
        tmpCanvas.height = metricsCanvas.height;
        const ctx = tmpCanvas.getContext('2d');
        ctx.fillStyle = '#fff';
        ctx.fillRect(0, 0, tmpCanvas.width, tmpCanvas.height);
        ctx.drawImage(metricsCanvas, 0, 0);
        const imgUrl = tmpCanvas.toDataURL('image/png');
        const imgLink = document.createElement('a');
        imgLink.href = imgUrl;
        imgLink.download = `service-metrics-graph-${dateStr}.png`;
        document.body.appendChild(imgLink);
        imgLink.click();
        document.body.removeChild(imgLink);
    }
}

// Utility function to refresh the page data
function refreshServiceList() {
    window.location.reload();
}

// --- Add Endpoint Modal Logic ---
function initializeAddEndpoint() {
    const addEndpointForm = document.getElementById('addEndpointForm');
    const submitBtn = document.getElementById('submitAddEndpoint');
    if (!addEndpointForm || !submitBtn) return;

    submitBtn.addEventListener('click', async function(e) {
        e.preventDefault();
        const name = document.getElementById('endpointName').value.trim();
        const url = document.getElementById('endpointUrl').value.trim();
        const errorDiv = document.getElementById('addEndpointError');
        errorDiv.classList.add('d-none');
        errorDiv.textContent = '';
        if (!name || !url) {
            errorDiv.textContent = 'Both name and URL are required.';
            errorDiv.classList.remove('d-none');
            return;
        }
        try {
            const resp = await fetch('/Home/AddApiEndpoint', {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify({ name, url })
            });
            if (resp.ok) {
                // Close modal and refresh
                const modal = bootstrap.Modal.getOrCreateInstance(document.getElementById('addEndpointModal'));
                modal.hide();
                setTimeout(refreshServiceList, 250);
            } else {
                const msg = await resp.text();
                errorDiv.textContent = msg || 'Failed to add endpoint.';
                errorDiv.classList.remove('d-none');
            }
        } catch (err) {
            errorDiv.textContent = err.message || 'Failed to add endpoint.';
            errorDiv.classList.remove('d-none');
        }
    });
}

// --- View Metrics Logic (Chart.js) ---
// Updated: View Metrics checks endpoints one at a time and updates chart after each
function initializeViewMetrics() {
    const btn = document.getElementById('viewMetricsBtn');
    const chartTypeSelect = document.getElementById('metricsChartType');
    // Use global lastMetricsData
    let lastChartType = 'bar';
    if (!btn) return;

    // Chart type change event
    if (chartTypeSelect) {
        chartTypeSelect.addEventListener('change', function() {
            lastChartType = chartTypeSelect.value;
            if (window.lastMetricsData) {
                showMetricsChart(window.lastMetricsData, lastChartType);
            }
        });
    }

    btn.addEventListener('click', async function() {
        btn.disabled = true;
        btn.innerHTML = '<i class="fas fa-spinner fa-spin"></i> Loading Metrics...';
        try {
            // Fetch endpoint list
            const resp = await fetch('/Home/GetEndpointMetrics');
            let endpoints = await resp.json();
            let metrics = [];
            for (let i = 0; i < endpoints.length; i++) {
                const endpoint = endpoints[i];
                const resp = await fetch(`/Home/CheckEndpoint?url=${encodeURIComponent(endpoint.url)}`);
                const data = await resp.json();
                metrics.push({
                    name: endpoint.name,
                    responseTimeMs: data.responseTimeMs,
                    status: data.status
                });
                // Update the status table as well
                const row = document.querySelector(`#apiTable tbody tr[data-url="${endpoint.url}"]`);
                if (row) {
                    const statusCell = row.querySelector('.status-cell');
                    setStatus(statusCell, data.status, data.statusDetail, data.responseTimeMs);
                }
                window.lastMetricsData = metrics.slice(); // Store globally for export
                lastChartType = chartTypeSelect ? chartTypeSelect.value : 'bar';
                showMetricsChart(metrics, lastChartType); // Update chart after each
                await new Promise(res => setTimeout(res, 100));
            }
        } catch (err) {
            alert('Failed to load metrics: ' + err.message);
        } finally {
            btn.disabled = false;
            btn.innerHTML = '<i class="fas fa-chart-bar"></i> View Metrics';
        }
    });
    // Future: For true streaming, consider SignalR or server events
}

let metricsChartInstance = null;

function showMetricsChart(metrics, chartType = 'bar') {
    const section = document.getElementById('serviceMetricsSection');
    section.classList.remove('d-none');
    const ctx = document.getElementById('metricsChart').getContext('2d');
    const names = metrics.map(m => m.name);
    const times = metrics.map(m => m.responseTimeMs);
    const statuses = metrics.map(m => m.status);
    // Status color map for both bar and line
    const statusColorMap = {
        'Active': 'rgba(40, 167, 69, 0.7)', // Bootstrap green
        'Slow': 'rgba(255, 193, 7, 0.7)',   // Bootstrap yellow
        'Unavailable': 'rgba(220, 53, 69, 0.7)', // Bootstrap red
        'Unknown': 'rgba(108, 117, 125, 0.5)' // Bootstrap gray
    };
    const bgColors = statuses.map(s => statusColorMap[s] || statusColorMap['Unknown']);
    if (window.metricsChartInstance) {
        window.metricsChartInstance.destroy();
    }
    let datasetConfig = {
        label: 'Response Time (ms)',
        data: times,
        backgroundColor: bgColors
    };
    if (chartType === 'line') {
        datasetConfig = {
            ...datasetConfig,
            fill: false,
            borderColor: 'rgba(33, 150, 243, 0.7)',
            pointStyle: 'circle',
            pointRadius: 8,
            pointBackgroundColor: bgColors, // Circle color per point
            pointBorderColor: '#fff',
            pointBorderWidth: 2
        };
    }
    window.metricsChartInstance = new Chart(ctx, {
        type: chartType,
        data: {
            labels: names,
            datasets: [datasetConfig]
        },
        options: {
            plugins: {
                legend: { display: false }, // Hide Chart.js legend, use HTML legend
                tooltip: {
                    callbacks: {
                        label: function(context) {
                            return `${context.dataset.label}: ${context.parsed.y} ms`;
                        }
                    }
                }
            },
            scales: {
                y: {
                    beginAtZero: true,
                    title: { display: true, text: 'Response Time (ms)' }
                },
                x: {
                    title: { display: true, text: 'API Endpoint' }
                }
            }
        }
    });
}


// Auto-refresh functionality (optional)
let autoRefreshInterval = null;

function startAutoRefresh(intervalMinutes = 5) {
    if (autoRefreshInterval) {
        clearInterval(autoRefreshInterval);
    }
    
    autoRefreshInterval = setInterval(() => {
        if (!isCheckingAll) {
            checkAllEndpoints();
        }
    }, intervalMinutes * 60 * 1000);
}

function stopAutoRefresh() {
    if (autoRefreshInterval) {
        clearInterval(autoRefreshInterval);
        autoRefreshInterval = null;
    }
}
