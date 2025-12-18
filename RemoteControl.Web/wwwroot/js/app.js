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

// Copy image to clipboard (converts to PNG because Clipboard API only supports PNG)
window.copyImageToClipboard = async function (dataUrl) {
    try {
        // Create an image element to load the data URL
        const img = new Image();
        
        await new Promise((resolve, reject) => {
            img.onload = resolve;
            img.onerror = reject;
            img.src = dataUrl;
        });

        // Draw to canvas and export as PNG
        const canvas = document.createElement('canvas');
        canvas.width = img.width;
        canvas.height = img.height;
        const ctx = canvas.getContext('2d');
        ctx.drawImage(img, 0, 0);

        // Get PNG blob from canvas
        const blob = await new Promise(resolve => canvas.toBlob(resolve, 'image/png'));
        
        // Copy to clipboard
        await navigator.clipboard.write([
            new ClipboardItem({ 'image/png': blob })
        ]);
        console.log('Image copied to clipboard as PNG');
    } catch (err) {
        console.error('Failed to copy image:', err);
        alert('Failed to copy image to clipboard: ' + err.message);
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
