// Hot reload toggle for HTML preview card
window.enableHtmlPreviewHotReload = true; // Set to false for deployment

(function () {
    // Utility to show preview in a card with actions
    function showPreviewInCard(cardId, previewType, url, contentType, previewText, fileName) {
        const card = document.getElementById(cardId);
        if (!card) return;
        card.style.display = '';
        // Content and actions containers
        const contentId = cardId.replace('Card', 'Content');
        const actionsId = cardId.replace('Card', 'Actions');
        const content = document.getElementById(contentId);
        const actions = document.getElementById(actionsId);
        if (content) content.innerHTML = '';
        if (actions) actions.style.display = '';
        // Download/discard/close/web preview buttons
        const downloadBtn = actions ? actions.querySelector('a.btn-success') : null;
        const discardBtn = actions ? actions.querySelector('button.btn-outline-danger') : null;
        const closeBtn = actions ? actions.querySelector('button.btn-secondary') : null;
        const webPreviewBtn = actions ? actions.querySelector('button.btn-info') : null;
        if (downloadBtn) {
            downloadBtn.href = url;
            downloadBtn.download = fileName || '';
        }
        if (discardBtn) {
            discardBtn.onclick = async function () {
                if (!fileName) return;
                try {
                    const fd = new FormData();
                    fd.append('fileName', fileName);
                    await fetch('/convert/delete', { method: 'POST', body: fd });
                    card.style.display = 'none';
                    if (actions) actions.style.display = 'none';
                } catch (e) { console.error(e); }
            };
        }
        if (closeBtn) {
            closeBtn.onclick = function () {
                card.style.display = 'none';
                if (actions) actions.style.display = 'none';
            };
        }
        if (webPreviewBtn) {
            webPreviewBtn.style.display = 'none'; // Hide by default
            webPreviewBtn.onclick = function () {
                window.open(url, '_blank');
            };
        }
        // Preview rendering
        if (!content) return;
        // --- Enhanced HTML preview handling ---
        if ((previewType === 'html' || (previewType === 'iframe' && contentType === 'text/html'))) {
            // Create toggle buttons container
            const toggleContainer = document.createElement('div');
            toggleContainer.className = 'btn-group mb-2';
            toggleContainer.role = 'group';
            toggleContainer.style = 'width:100%';
            // Raw HTML button
            const rawBtn = document.createElement('button');
            rawBtn.className = 'btn btn-outline-primary active';
            rawBtn.textContent = 'Raw HTML';
            rawBtn.type = 'button';
            // Web Preview button
            const webBtn = document.createElement('button');
            webBtn.className = 'btn btn-outline-primary';
            webBtn.textContent = 'Web Preview';
            webBtn.type = 'button';
            toggleContainer.appendChild(rawBtn);
            toggleContainer.appendChild(webBtn);
            content.appendChild(toggleContainer);
            // Preview containers
            const rawContainer = document.createElement('div');
            const webContainer = document.createElement('div');
            rawContainer.style = '';
            webContainer.style = 'display:none;';
            // Raw HTML preview
            fetch(url)
                .then(r => r.text())
                .then(html => {
                    const pre = document.createElement('pre');
                    pre.className = 'cm-s-default cm-html';
                    pre.style = 'max-height:400px;overflow:auto;border-radius:6px;padding:12px;background:#f8f8f8;border:1px solid #e0e0e0;';
                    pre.textContent = html;
                    rawContainer.appendChild(pre);
                });
            // Web preview (iframe)
            const iframe = document.createElement('iframe');
            iframe.src = url;
            iframe.className = 'w-100';
            iframe.style.minHeight = '70vh';
            // Add sandbox to prevent navigation and form submission
            iframe.setAttribute('sandbox', 'allow-scripts allow-same-origin');
            // Prevent iframe from causing page reloads
            iframe.addEventListener('load', function () {
                try {
                    const doc = iframe.contentDocument || iframe.contentWindow.document;
                    if (doc) {
                        // Disable all forms inside iframe
                        const forms = doc.querySelectorAll('form');
                        forms.forEach(f => {
                            f.addEventListener('submit', function (e) {
                                e.preventDefault();
                                return false;
                            });
                        });
                    }
                } catch (e) { /* ignore cross-origin */ }
            });
            webContainer.appendChild(iframe);
            content.appendChild(rawContainer);
            content.appendChild(webContainer);
            // Toggle logic
            rawBtn.onclick = function () {
                rawBtn.classList.add('active');
                webBtn.classList.remove('active');
                rawContainer.style.display = '';
                webContainer.style.display = 'none';
            };
            webBtn.onclick = function () {
                webBtn.classList.add('active');
                rawBtn.classList.remove('active');
                rawContainer.style.display = 'none';
                webContainer.style.display = '';
            };
            if (webPreviewBtn) webPreviewBtn.style.display = '';
            card.style.display = '';
        } else if (previewType === 'image') {
            const img = document.createElement('img');
            img.src = url;
            img.className = 'img-fluid mx-auto d-block';
            content.appendChild(img);
        } else if (previewType === 'iframe') {
            const iframe = document.createElement('iframe');
            iframe.src = url;
            iframe.className = 'w-100';
            iframe.style.minHeight = '70vh';
            content.appendChild(iframe);
        } else if (previewType === 'text') {
            fetch(url).then(r => r.text()).then(t => {
                const pre = document.createElement('pre');
                pre.textContent = t;
                content.appendChild(pre);
            });
        } else if (previewType === 'download' && previewText) {
            const pre = document.createElement('pre');
            pre.textContent = previewText;
            content.appendChild(pre);
        } else {
            const p = document.createElement('p');
            p.textContent = 'Preview not available. You can download the file.';
            content.appendChild(p);
        }
    }

    // Base64 encode
    const b64EncodeForm = document.getElementById('b64EncodeForm');
    const b64PreviewBtn = document.getElementById('b64PreviewBtn');
    const b64CopyBtn = document.getElementById('b64CopyBtn');
    const b64Result = document.getElementById('b64Result');
    if (b64EncodeForm) {
        b64EncodeForm.addEventListener('submit', async (e) => {
            e.preventDefault();
            const fd = new FormData(b64EncodeForm);
            b64Result.value = 'Converting...';
            // Hide preview, actions, and result until conversion is done
            document.getElementById('b64EncodePreviewCard').style.display = 'none';
            document.getElementById('b64EncodePreviewActions').style.display = 'none';
            document.getElementById('b64ResultCard').style.display = 'none';
            document.getElementById('b64EncodeActionsCard').style.display = 'none';
            try {
                const res = await fetch('/convert/tobase64', { method: 'POST', body: fd });
                const j = await res.json();
                if (j.success) {
                    b64Result.value = j.base64;
                    b64PreviewBtn.disabled = false;
                    b64CopyBtn.disabled = false;
                    document.getElementById('b64ResultCard').style.display = '';
                    document.getElementById('b64EncodeActionsCard').style.display = '';
                } else {
                    b64Result.value = j.message || 'Conversion failed';
                }
            } catch (err) {
                b64Result.value = String(err);
            }
        });
        if (b64PreviewBtn) {
            b64PreviewBtn.addEventListener('click', () => {
                // Show preview card and actions only when preview is requested
                document.getElementById('b64EncodePreviewCard').style.display = '';
                document.getElementById('b64EncodePreviewActions').style.display = '';
                if (b64Result.value) showPreviewInCard('b64EncodePreviewCard', 'image', b64Result.value, '', null, null);
            });
        }
        if (b64CopyBtn) {
            b64CopyBtn.addEventListener('click', async () => {
                if (!b64Result.value) return;
                try { await navigator.clipboard.writeText(b64Result.value); b64CopyBtn.textContent = 'Copied!'; setTimeout(() => b64CopyBtn.textContent = 'Copy', 1500); } catch (e) { alert('Copy failed'); }
            });
        }
    }

    // Base64 decode (unchanged)
    const b64DecodeForm = document.getElementById('b64DecodeForm');
    if (b64DecodeForm) {
        b64DecodeForm.addEventListener('submit', async (e) => {
            e.preventDefault();
            const status = document.getElementById('b64DecodeStatus');
            status.innerHTML = '<div class="text-muted">Decoding...';
            document.getElementById('b64DecodePreviewCard').style.display = 'none';
            document.getElementById('b64DecodePreviewActions').style.display = 'none';
            const fd = new FormData(b64DecodeForm);
            try {
                const res = await fetch('/convert/frombase64', { method: 'POST', body: fd });
                const j = await res.json();
                if (j.success) {
                    status.innerHTML = `<div class="alert alert-success">Decoded to ${j.fileName}</div>`;
                    document.getElementById('b64DecodePreviewCard').style.display = '';
                    document.getElementById('b64DecodePreviewActions').style.display = '';
                    showPreviewInCard('b64DecodePreviewCard', j.previewType, j.url, j.contentType, null, j.fileName);
                } else {
                    status.innerHTML = `<div class="alert alert-danger">${j.message || 'Decode failed'}`;
                }
            } catch (err) {
                status.innerHTML = `<div class="alert alert-danger">${err}`;
            }
        });
    }

    // Document conversion form handler
    const docForm = document.getElementById('docForm');
    const docStatus = document.getElementById('docStatus');
    const docPreviewCard = document.getElementById('docPreviewCard');
    const docPreviewContent = document.getElementById('docPreviewContent');
    const docDownloadBtn = document.getElementById('docDownloadBtn');
    const docWebPreviewBtn = document.getElementById('docWebPreviewBtn');
    const docDiscardBtn = document.getElementById('docDiscardBtn');
    const docClosePreviewBtn = document.getElementById('docClosePreviewBtn');
    if (docForm) {
        docForm.addEventListener('submit', async function (e) {
            e.preventDefault();
            if (docStatus) docStatus.innerHTML = '<span class="text-muted">Converting...</span>';
            if (docPreviewCard) docPreviewCard.style.display = 'none';
            if (docPreviewContent) docPreviewContent.innerHTML = '';
            const fd = new FormData(docForm);
            try {
                const res = await fetch('/convert/document', { method: 'POST', body: fd });
                const j = await res.json();
                if (j.success) {
                    if (docStatus) docStatus.innerHTML = '<div class="alert alert-success">Conversion successful!</div>';
                    showPreviewInCard('docPreviewCard', j.previewType, j.url, j.contentType, j.previewText, j.fileName);
                } else {
                    if (docStatus) docStatus.innerHTML = `<div class='alert alert-danger'>${j.message || 'Conversion failed'}</div>`;
                }
            } catch (err) {
                if (docStatus) docStatus.innerHTML = `<div class='alert alert-danger'>${err}</div>`;
            }
        });
    }

    // Image conversion form handler
    const imgForm = document.getElementById('imgForm');
    const imgStatus = document.getElementById('imgStatus');
    const imgPreviewCard = document.getElementById('imgPreviewCard');
    const imgPreviewContent = document.getElementById('imgPreviewContent');

    if (imgForm) {
        imgForm.addEventListener('submit', async function (e) {
            e.preventDefault();
            if (imgStatus) imgStatus.innerHTML = '<span class="text-muted">Converting...</span>';
            if (imgPreviewCard) imgPreviewCard.style.display = 'none';
            if (imgPreviewContent) imgPreviewContent.innerHTML = '';
            const fd = new FormData(imgForm);
            try {
                const res = await fetch('/convert/image', { method: 'POST', body: fd });
                const j = await res.json();
                if (j.success) {
                    if (imgStatus) imgStatus.innerHTML = '<div class="alert alert-success">Conversion successful!</div>';
                    showPreviewInCard('imgPreviewCard', j.previewType, j.url, j.contentType, j.previewText, j.fileName);
                } else {
                    if (imgStatus) imgStatus.innerHTML = `<div class='alert alert-danger'>${j.message || 'Conversion failed'}</div>`;
                }
            } catch (err) {
                if (imgStatus) imgStatus.innerHTML = `<div class='alert alert-danger'>${err}</div>`;
            }
        });
    }

    // --- Hot reload protection for /converted/ preview files (development only) ---
    if (window.enableHtmlPreviewHotReload) {
        // Prevent page reload if preview card is open and a /converted/ file is being viewed
        window.addEventListener('beforeunload', function (e) {
            var previewCard = document.getElementById('docPreviewCard');
            var previewContent = document.getElementById('docPreviewContent');
            if (previewCard && previewCard.style.display !== 'none' && previewContent) {
                // Check if preview contains an iframe with src in /converted/
                var iframe = previewContent.querySelector('iframe');
                if (iframe && iframe.src && iframe.src.indexOf('/converted/') !== -1) {
                    // Block reload
                    e.preventDefault();
                    e.returnValue = '';
                    return '';
                }
            }
        });
        // Optionally, block websocket reloads (for some dev servers)
        if (window.WebSocket) {
            var _ws = window.WebSocket;
            window.WebSocket = function (url, protocols) {
                if (url && url.indexOf('ws://') === 0 && url.indexOf('hot') !== -1) {
                    // Block hot reload websocket
                    return {};
                }
                return new _ws(url, protocols);
            };
        }
    }
})();