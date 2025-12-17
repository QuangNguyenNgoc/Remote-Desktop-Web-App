// Utility functions for Blazor JS interop

// Download file to user's device
window.downloadFile = function (fileName, content) {
    const blob = new Blob([content], { type: 'text/plain' });
    const url = URL.createObjectURL(blob);
    const a = document.createElement('a');
    a.href = url;
    a.download = fileName;
    document.body.appendChild(a);
    a.click();
    document.body.removeChild(a);
    URL.revokeObjectURL(url);
};

// Download image from data URL
window.downloadImage = function (fileName, dataUrl) {
    const a = document.createElement('a');
    a.href = dataUrl;
    a.download = fileName;
    document.body.appendChild(a);
    a.click();
    document.body.removeChild(a);
};

// Copy image to clipboard
window.copyImageToClipboard = async function (dataUrl) {
    try {
        const response = await fetch(dataUrl);
        const blob = await response.blob();
        await navigator.clipboard.write([
            new ClipboardItem({ [blob.type]: blob })
        ]);
        console.log('Image copied to clipboard');
    } catch (err) {
        console.error('Failed to copy image:', err);
    }
};

// Toggle fullscreen for element
window.toggleFullscreen = function (elementId) {
    const elem = document.getElementById(elementId);
    if (!elem) {
        // If no element, use document
        if (!document.fullscreenElement) {
            document.documentElement.requestFullscreen();
        } else {
            document.exitFullscreen();
        }
        return;
    }
    
    if (!document.fullscreenElement) {
        elem.requestFullscreen();
    } else {
        document.exitFullscreen();
    }
};
