window.videoPlayer = {
    player: null,
    container: null,
    isInitialized: false,
    blazorRef: null,

    initialize: function (videoElementId, videoUrl) {
        this.container = document.getElementById(videoElementId);
        
        if (!this.container) {
            console.error(`Video container element not found: ${videoElementId}. Element may not be rendered yet.`);
            return false;
        }
        
        try {
            this.player = this.container.querySelector('video');
        } catch (error) {
            console.error('Error finding video element:', error);
            return false;
        }
        
        if (!this.player) {
            this.player = document.createElement('video');
            this.player.controls = false;
            this.player.preload = 'metadata';
            this.container.appendChild(this.player);
        }

        this.player.src = videoUrl;
        this.setupCustomControls();
        this.isInitialized = true;
        console.log(`Video player initialized successfully for: ${videoUrl}`);
        return true;
    },

    setupCustomControls: function () {
        if (!this.container) {
            console.error('Cannot setup controls: container is null');
            return;
        }
        
        // Remove existing controls if any
        const existingControls = this.container.querySelector('.video-controls');
        if (existingControls) {
            existingControls.remove();
        }

        // Create custom controls
        const controls = document.createElement('div');
        controls.className = 'video-controls';
        controls.innerHTML = `
            <div class="controls-background"></div>
            <div class="controls-content">
                <div class="progress-container">
                    <div class="progress-bar">
                        <div class="progress-filled"></div>
                        <div class="progress-handle"></div>
                    </div>
                    <div class="time-display">
                        <span class="current-time">0:00</span>
                        <span class="duration">0:00</span>
                    </div>
                </div>
                <div class="control-buttons">
                    <button class="btn-play-pause">
                        <i class="fas fa-play"></i>
                    </button>
                    <div class="volume-control">
                        <button class="btn-volume">
                            <i class="fas fa-volume-up"></i>
                        </button>
                        <div class="volume-slider">
                            <input type="range" min="0" max="100" value="100" class="volume-input">
                        </div>
                    </div>
                    <button class="btn-fullscreen">
                        <i class="fas fa-expand"></i>
                    </button>
                </div>
            </div>
        `;

        this.container.appendChild(controls);
        this.bindControlEvents();
        this.updateControlsVisibility();
    },

    bindControlEvents: function () {
        const playPauseBtn = this.container.querySelector('.btn-play-pause');
        const progressBar = this.container.querySelector('.progress-bar');
        const volumeBtn = this.container.querySelector('.btn-volume');
        const volumeInput = this.container.querySelector('.volume-input');
        const fullscreenBtn = this.container.querySelector('.btn-fullscreen');

        // Play/Pause - Send to SignalR for sync
        playPauseBtn.addEventListener('click', () => {
            if (this.player.paused) {
                if (this.blazorRef) {
                    this.blazorRef.invokeMethodAsync('OnVideoPlay', this.player.currentTime);
                } else {
                    this.play(); // Fallback for local-only play
                }
            } else {
                if (this.blazorRef) {
                    this.blazorRef.invokeMethodAsync('OnVideoPause', this.player.currentTime);
                } else {
                    this.pause(); // Fallback for local-only pause
                }
            }
        });

        // Progress bar - Send to SignalR for sync
        progressBar.addEventListener('click', (e) => {
            const rect = progressBar.getBoundingClientRect();
            const pos = (e.clientX - rect.left) / rect.width;
            const newTime = pos * this.player.duration;
            
            if (this.blazorRef) {
                this.blazorRef.invokeMethodAsync('OnVideoSeek', newTime);
            } else {
                this.player.currentTime = newTime; // Fallback for local-only seek
            }
        });

        // Volume
        volumeBtn.addEventListener('click', () => {
            this.player.muted = !this.player.muted;
            this.updateVolumeIcon();
        });

        volumeInput.addEventListener('input', (e) => {
            this.player.volume = e.target.value / 100;
            this.player.muted = false;
            this.updateVolumeIcon();
        });

        // Fullscreen
        fullscreenBtn.addEventListener('click', () => {
            this.toggleFullscreen();
        });

        // Player events
        this.player.addEventListener('timeupdate', () => this.updateProgress());
        this.player.addEventListener('loadedmetadata', () => this.updateDuration());
        this.player.addEventListener('play', () => this.updatePlayPauseIcon());
        this.player.addEventListener('pause', () => this.updatePlayPauseIcon());
        this.player.addEventListener('volumechange', () => this.updateVolumeIcon());

        // Hide controls on mouse leave, show on mouse enter
        this.container.addEventListener('mouseenter', () => this.showControls());
        this.container.addEventListener('mouseleave', () => this.hideControls());
        this.container.addEventListener('mousemove', () => this.showControls());

        // Space bar to play/pause - Send to SignalR for sync
        document.addEventListener('keydown', (e) => {
            if (e.code === 'Space' && e.target.tagName !== 'INPUT') {
                e.preventDefault();
                if (this.player.paused) {
                    if (this.blazorRef) {
                        this.blazorRef.invokeMethodAsync('OnVideoPlay', this.player.currentTime);
                    } else {
                        this.play();
                    }
                } else {
                    if (this.blazorRef) {
                        this.blazorRef.invokeMethodAsync('OnVideoPause', this.player.currentTime);
                    } else {
                        this.pause();
                    }
                }
            }
        });
    },

    updateControlsVisibility: function () {
        let timeout;
        const showControls = () => {
            this.container.querySelector('.video-controls').style.opacity = '1';
            clearTimeout(timeout);
            timeout = setTimeout(() => {
                if (!this.player.paused) {
                    this.container.querySelector('.video-controls').style.opacity = '0';
                }
            }, 3000);
        };

        this.container.addEventListener('mousemove', showControls);
        showControls();
    },

    showControls: function () {
        if (!this.container) return;
        const controls = this.container.querySelector('.video-controls');
        if (controls) {
            controls.style.opacity = '1';
        }
    },

    hideControls: function () {
        if (!this.container) return;
        const controls = this.container.querySelector('.video-controls');
        if (controls && !this.player.paused) {
            controls.style.opacity = '0';
        }
    },

    updateProgress: function () {
        const progressFilled = this.container.querySelector('.progress-filled');
        const currentTimeSpan = this.container.querySelector('.current-time');
        
        if (this.player.duration) {
            const progress = (this.player.currentTime / this.player.duration) * 100;
            progressFilled.style.width = progress + '%';
        }
        
        currentTimeSpan.textContent = this.formatTime(this.player.currentTime);
    },

    updateDuration: function () {
        const durationSpan = this.container.querySelector('.duration');
        durationSpan.textContent = this.formatTime(this.player.duration);
    },

    updatePlayPauseIcon: function () {
        const icon = this.container.querySelector('.btn-play-pause i');
        icon.className = this.player.paused ? 'fas fa-play' : 'fas fa-pause';
    },

    updateVolumeIcon: function () {
        const icon = this.container.querySelector('.btn-volume i');
        if (this.player.muted || this.player.volume === 0) {
            icon.className = 'fas fa-volume-mute';
        } else if (this.player.volume < 0.5) {
            icon.className = 'fas fa-volume-down';
        } else {
            icon.className = 'fas fa-volume-up';
        }
    },

    formatTime: function (seconds) {
        if (!seconds) return '0:00';
        const minutes = Math.floor(seconds / 60);
        const remainingSeconds = Math.floor(seconds % 60);
        return `${minutes}:${remainingSeconds.toString().padStart(2, '0')}`;
    },

    play: function () {
        if (this.player) {
            this.player.play();
        }
    },

    pause: function () {
        if (this.player) {
            this.player.pause();
        }
    },

    setCurrentTime: function (seconds) {
        if (this.player) {
            this.player.currentTime = seconds;
        }
    },

    getCurrentTime: function () {
        return this.player ? this.player.currentTime : 0;
    },

    getDuration: function () {
        return this.player ? this.player.duration || 0 : 0;
    },

    isPlaying: function () {
        return this.player ? !this.player.paused : false;
    },

    setVolume: function (volume) {
        if (this.player) {
            this.player.volume = Math.max(0, Math.min(1, volume));
        }
    },

    getVolume: function () {
        return this.player ? this.player.volume : 1;
    },

    setMuted: function (muted) {
        if (this.player) {
            this.player.muted = muted;
        }
    },

    toggleFullscreen: function () {
        if (!document.fullscreenElement) {
            this.container.requestFullscreen();
        } else {
            document.exitFullscreen();
        }
    },

    loadVideo: function (videoUrl) {
        if (this.player) {
            this.player.src = videoUrl;
            this.player.load();
        }
    },

    setBlazorReference: function (blazorRef) {
        this.blazorRef = blazorRef;
        console.log('Video player Blazor reference set:', blazorRef);
    }
}; 