// Gamer LFG - Main JavaScript

// Initialize Lucide icons after DOM load
document.addEventListener('DOMContentLoaded', function () {
    if (typeof lucide !== 'undefined') {
        lucide.createIcons();
    }

    // Mobile menu toggle
    const mobileMenuToggle = document.getElementById('mobileMenuToggle');
    const navLinks = document.querySelector('.nav-links');

    if (mobileMenuToggle && navLinks) {
        mobileMenuToggle.addEventListener('click', function () {
            navLinks.classList.toggle('mobile-open');
        });
    }

    // User dropdown toggle
    const userAvatarBtn = document.getElementById('userAvatarBtn');
    const userDropdown = userAvatarBtn?.closest('.user-dropdown');

    if (userAvatarBtn && userDropdown) {
        userAvatarBtn.addEventListener('click', function (e) {
            e.stopPropagation();
            userDropdown.classList.toggle('open');
        });

        // Close dropdown when clicking outside
        document.addEventListener('click', function (e) {
            if (!userDropdown.contains(e.target)) {
                userDropdown.classList.remove('open');
            }
        });

        // Close on Escape key
        document.addEventListener('keydown', function (e) {
            if (e.key === 'Escape') {
                userDropdown.classList.remove('open');
            }
        });
    }
});

// API Helper for Ajax calls
const API = {
    baseUrl: '',

    async request(url, options = {}) {
        const defaultOptions = {
            headers: {
                'Content-Type': 'application/json',
            },
            credentials: 'same-origin'
        };

        const mergedOptions = { ...defaultOptions, ...options };

        try {
            const response = await fetch(this.baseUrl + url, mergedOptions);
            const data = await response.json();
            return { success: response.ok, status: response.status, data };
        } catch (error) {
            console.error('API Error:', error);
            return { success: false, error: error.message };
        }
    },

    async get(url) {
        return this.request(url, { method: 'GET' });
    },

    async post(url, data) {
        return this.request(url, {
            method: 'POST',
            body: JSON.stringify(data)
        });
    },

    async put(url, data) {
        return this.request(url, {
            method: 'PUT',
            body: JSON.stringify(data)
        });
    },

    async delete(url) {
        return this.request(url, { method: 'DELETE' });
    }
};

// Lobby related functions
const Lobby = {
    async loadLobbies(filters = {}) {
        const queryParams = new URLSearchParams(filters).toString();
        const url = `/api/lobbies${queryParams ? '?' + queryParams : ''}`;
        return await API.get(url);
    },

    async createLobby(lobbyData) {
        return await API.post('/api/lobbies', lobbyData);
    },

    async applyToLobby(lobbyId, applicationData) {
        return await API.post(`/api/lobbies/${lobbyId}/apply`, applicationData);
    },

    async cancelApplication(lobbyId) {
        return await API.delete(`/api/lobbies/${lobbyId}/apply`);
    }
};

// Toast notifications
const Toast = {
    show(message, type = 'info', duration = 3000) {
        const toast = document.createElement('div');
        toast.className = `toast toast-${type}`;
        toast.textContent = message;

        const container = document.getElementById('toast-container') || document.body;
        container.appendChild(toast);

        setTimeout(() => {
            toast.classList.add('fade-out');
            setTimeout(() => toast.remove(), 300);
        }, duration);
    },

    success(message) { this.show(message, 'success'); },
    error(message) { this.show(message, 'error'); },
    info(message) { this.show(message, 'info'); }
};

// Form validation helper
function validateForm(formElement) {
    const inputs = formElement.querySelectorAll('[required]');
    let isValid = true;

    inputs.forEach(input => {
        if (!input.value.trim()) {
            input.classList.add('error');
            isValid = false;
        } else {
            input.classList.remove('error');
        }
    });

    return isValid;
}

// Format relative time
function formatRelativeTime(dateString) {
    const date = new Date(dateString);
    const now = new Date();
    const diffMs = now - date;
    const diffMins = Math.floor(diffMs / 60000);

    if (diffMins < 1) return 'just now';
    if (diffMins < 60) return `${diffMins}m ago`;

    const diffHours = Math.floor(diffMins / 60);
    if (diffHours < 24) return `${diffHours}h ago`;

    const diffDays = Math.floor(diffHours / 24);
    if (diffDays < 7) return `${diffDays}d ago`;

    return date.toLocaleDateString();
}

// Notification System
const Notifications = {
    async loadUnreadCount() {
        const badge = document.getElementById('unreadCount');
        if (!badge) return;

        try {
            const response = await API.get('/api/NotificationApi/unread-count');
            if (response.success) {
                if (response.data.count > 0) {
                    badge.textContent = response.data.count > 99 ? '99+' : response.data.count;
                    badge.classList.remove('hidden');
                } else {
                    badge.classList.add('hidden');
                }
            }
        } catch (e) {
            console.error("Failed to load notifications count", e);
        }
    },

    async loadNotifications() {
        const list = document.getElementById('notificationItems');
        if (!list) return;

        list.innerHTML = '<div class="p-4 text-center text-xs text-white/50">Loading...</div>';

        try {
            const response = await API.get('/api/NotificationApi');

            if (response.success && response.data.notifications && response.data.notifications.length > 0) {
                list.innerHTML = response.data.notifications.map(n => `
                    <div class="px-4 py-3 border-b border-white/5 hover:bg-white/5 transition-colors ${!n.isRead ? 'bg-white/5' : ''}">
                        <div class="flex justify-between items-start gap-2">
                            <p class="text-xs text-white">${truncateString(n.message, 60)}</p>
                            ${!n.isRead ? '<span class="size-2 rounded-full bg-primary flex-shrink-0 mt-1"></span>' : ''}
                        </div>
                        <div class="flex justify-between items-center mt-2">
                            <span class="text-[10px] text-white/40">${formatRelativeTime(n.createdAt)}</span>
                            ${!n.isRead ? `<button onclick="event.stopPropagation(); Notifications.markRead('${n.id}')" class="text-[10px] text-primary hover:underline">Mark read</button>` : ''}
                        </div>
                    </div>
                `).join('');
            } else {
                list.innerHTML = '<div class="p-4 text-center text-xs text-white/50">No notifications</div>';
            }
        } catch (e) {
            console.error("Failed to load notifications", e);
            list.innerHTML = '<div class="p-4 text-center text-xs text-red-500">Failed to load</div>';
        }
    },

    async markRead(id) {
        try {
            await API.post(`/api/NotificationApi/read/${id}`, {});
            this.loadUnreadCount();
            // Refresh list if dropdown is open
            const dropdown = document.getElementById('notificationList');
            if (dropdown && !dropdown.classList.contains('hidden')) {
                this.loadNotifications();
            }
        } catch (e) {
            console.error("Failed to mark read", e);
        }
    },

    async markAllRead() {
        try {
            await API.post('/api/NotificationApi/read-all', {});
            this.loadUnreadCount();
            this.loadNotifications();
        } catch (e) {
            console.error("Failed to mark all read", e);
        }
    }
};

function truncateString(str, n) {
    if (!str) return '';
    return (str.length > n) ? str.substr(0, n - 1) + '...' : str;
}

// Global functions for inline onclick handlers
window.toggleNotifications = function () {
    const list = document.getElementById('notificationList');
    if (list) {
        if (list.classList.contains('hidden')) {
            list.classList.remove('hidden', 'opacity-0', 'invisible');
            Notifications.loadNotifications();
        } else {
            list.classList.add('hidden', 'opacity-0', 'invisible');
        }
    }
};

window.markAllRead = function () {
    Notifications.markAllRead();
};

// Initialize Notifications
document.addEventListener('DOMContentLoaded', () => {
    // Initial check
    setTimeout(() => Notifications.loadUnreadCount(), 1000);

    // Poll every 60 seconds
    setInterval(() => Notifications.loadUnreadCount(), 60000);

    // Close dropdown when clicking outside
    document.addEventListener('click', function (e) {
        const dropdownWrapper = document.getElementById('notificationDropdown');
        const list = document.getElementById('notificationList');

        if (list && !list.classList.contains('hidden') &&
            dropdownWrapper && !dropdownWrapper.contains(e.target)) {
            list.classList.add('hidden', 'opacity-0', 'invisible');
        }
    });
});
