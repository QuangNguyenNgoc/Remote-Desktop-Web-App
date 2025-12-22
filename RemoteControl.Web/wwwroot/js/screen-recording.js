// screen-recording.js: MediaRecorder for screen recording
// Draws base64 frames to canvas and records as WebM video

window.screenRecording = {
    canvas: null,
    ctx: null,
    mediaRecorder: null,
    recordedChunks: [],
    isRecording: false,

    // Initialize canvas for recording
    init: function (width, height) {
        this.canvas = document.createElement('canvas');
        this.canvas.width = width || 1920;
        this.canvas.height = height || 1080;
        this.ctx = this.canvas.getContext('2d');
        this.recordedChunks = [];
        console.log('[ScreenRecording] Canvas initialized:', this.canvas.width, 'x', this.canvas.height);
    },

    // Draw base64 image to canvas
    drawFrame: function (base64Image, format) {
        if (!this.ctx) return;
        
        const img = new Image();
        img.onload = () => {
            // Resize canvas if needed
            if (img.width !== this.canvas.width || img.height !== this.canvas.height) {
                this.canvas.width = img.width;
                this.canvas.height = img.height;
            }
            this.ctx.drawImage(img, 0, 0);
        };
        img.src = `data:image/${format || 'jpeg'};base64,${base64Image}`;
    },

    // Start recording
    startRecording: function (fps) {
        if (this.isRecording) return false;
        if (!this.canvas) {
            this.init(1920, 1080);
        }

        this.recordedChunks = [];
        
        try {
            // Get stream from canvas at specified FPS
            const stream = this.canvas.captureStream(fps || 20);
            
            // Create MediaRecorder with WebM format
            const options = {
                mimeType: 'video/webm;codecs=vp9',
                videoBitsPerSecond: 5000000 // 5 Mbps
            };
            
            // Fallback if VP9 not supported
            if (!MediaRecorder.isTypeSupported(options.mimeType)) {
                options.mimeType = 'video/webm;codecs=vp8';
            }
            if (!MediaRecorder.isTypeSupported(options.mimeType)) {
                options.mimeType = 'video/webm';
            }
            
            this.mediaRecorder = new MediaRecorder(stream, options);
            
            this.mediaRecorder.ondataavailable = (event) => {
                if (event.data && event.data.size > 0) {
                    this.recordedChunks.push(event.data);
                }
            };
            
            this.mediaRecorder.start(100); // Collect data every 100ms
            this.isRecording = true;
            console.log('[ScreenRecording] Recording started with', options.mimeType);
            return true;
        } catch (error) {
            console.error('[ScreenRecording] Failed to start:', error);
            return false;
        }
    },

    // Stop recording
    stopRecording: function () {
        return new Promise((resolve) => {
            if (!this.isRecording || !this.mediaRecorder) {
                resolve(false);
                return;
            }

            this.mediaRecorder.onstop = () => {
                this.isRecording = false;
                console.log('[ScreenRecording] Recording stopped, chunks:', this.recordedChunks.length);
                resolve(true);
            };

            this.mediaRecorder.stop();
        });
    },

    // Download the recorded video
    downloadVideo: function (filename) {
        if (this.recordedChunks.length === 0) {
            console.warn('[ScreenRecording] No recorded data');
            return false;
        }

        const blob = new Blob(this.recordedChunks, { type: 'video/webm' });
        const url = URL.createObjectURL(blob);
        
        const a = document.createElement('a');
        a.href = url;
        a.download = filename || `screen-recording-${Date.now()}.webm`;
        document.body.appendChild(a);
        a.click();
        document.body.removeChild(a);
        
        URL.revokeObjectURL(url);
        console.log('[ScreenRecording] Video downloaded:', a.download);
        return true;
    },

    // Get video blob for preview or other uses
    getVideoBlob: function () {
        if (this.recordedChunks.length === 0) return null;
        return new Blob(this.recordedChunks, { type: 'video/webm' });
    },

    // Clear recorded data
    clear: function () {
        this.recordedChunks = [];
        console.log('[ScreenRecording] Cleared');
    }
};
