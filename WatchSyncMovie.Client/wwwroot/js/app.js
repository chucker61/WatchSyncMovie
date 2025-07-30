// Helper functions for WatchSync application

window.scrollToBottom = (element) => {
    if (element) {
        element.scrollTop = element.scrollHeight;
    }
};

window.copyToClipboard = async (text) => {
    try {
        await navigator.clipboard.writeText(text);
        return true;
    } catch (err) {
        console.error('Failed to copy to clipboard:', err);
        return false;
    }
};

window.focusElement = (elementId) => {
    const element = document.getElementById(elementId);
    if (element) {
        element.focus();
    }
};

// Notification support
window.showNotification = (title, message) => {
    if ('Notification' in window) {
        if (Notification.permission === 'granted') {
            new Notification(title, {
                body: message,
                icon: '/icon-192.png'
            });
        } else if (Notification.permission !== 'denied') {
            Notification.requestPermission().then(permission => {
                if (permission === 'granted') {
                    new Notification(title, {
                        body: message,
                        icon: '/icon-192.png'
                    });
                }
            });
        }
    }
};

// Full screen API helpers
window.requestFullscreen = (element) => {
    if (element.requestFullscreen) {
        element.requestFullscreen();
    } else if (element.webkitRequestFullscreen) {
        element.webkitRequestFullscreen();
    } else if (element.msRequestFullscreen) {
        element.msRequestFullscreen();
    }
};

window.exitFullscreen = () => {
    if (document.exitFullscreen) {
        document.exitFullscreen();
    } else if (document.webkitExitFullscreen) {
        document.webkitExitFullscreen();
    } else if (document.msExitFullscreen) {
        document.msExitFullscreen();
    }
};

// Keyboard shortcuts
document.addEventListener('keydown', (e) => {
    // Global keyboard shortcuts can be added here
    if (e.ctrlKey || e.metaKey) {
        switch (e.key) {
            case 'Enter':
                // Handle global shortcuts
                break;
        }
    }
}); 