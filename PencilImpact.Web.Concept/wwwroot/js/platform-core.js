// PencilImpact.Web.Concept/wwwroot/js/platform-core.js

/**
 * PencilImpact Platform Core JavaScript
 * Handles navigation, analytics, and shared functionality
 */

window.PlatformCore = (function () {
    'use strict';

    // Configuration
    const config = {
        apiEndpoint: '/api/platform',
        analyticsEnabled: true,
        sessionTimeout: 30 * 60 * 1000, // 30 minutes
        debounceDelay: 300
    };

    // State management
    let state = {
        sessionId: generateSessionId(),
        pageStartTime: Date.now(),
        isNavigating: false,
        analytics: {
            events: [],
            lastFlush: Date.now()
        }
    };

    /**
     * Initialize platform core functionality
     */
    function init() {
        initializeNavigation();
        initializeAnalytics();
        initializeScrollBehavior();
        initializeTooltips();
        setupErrorHandling();

        console.log('PlatformCore initialized');
    }

    /**
     * Navigation functionality
     */
    function initializeNavigation() {
        const navbar = document.getElementById('navbar');
        const navToggle = document.getElementById('navToggle');
        const navMenu = document.getElementById('navMenu');

        // Mobile navigation toggle
        if (navToggle && navMenu) {
            navToggle.addEventListener('click', function () {
                navToggle.classList.toggle('active');
                navMenu.classList.toggle('active');
            });

            // Close mobile menu when clicking outside
            document.addEventListener('click', function (e) {
                if (!navToggle.contains(e.target) && !navMenu.contains(e.target)) {
                    navToggle.classList.remove('active');
                    navMenu.classList.remove('active');
                }
            });
        }

        // Navbar scroll effect
        if (navbar) {
            let lastScroll = 0;
            window.addEventListener('scroll', debounce(function () {
                const currentScroll = window.pageYOffset;

                if (currentScroll > 50) {
                    navbar.classList.add('scrolled');
                } else {
                    navbar.classList.remove('scrolled');
                }

                // Hide navbar on scroll down, show on scroll up
                if (currentScroll > lastScroll && currentScroll > 100) {
                    navbar.style.transform = 'translateY(-100%)';
                } else {
                    navbar.style.transform = 'translateY(0)';
                }

                lastScroll = currentScroll;
            }, 10));
        }

        // Smooth scroll for anchor links
        document.addEventListener('click', function (e) {
            const link = e.target.closest('a[href^="#"]');
            if (link) {
                e.preventDefault();
                const targetId = link.getAttribute('href').substring(1);
                const targetElement = document.getElementById(targetId);

                if (targetElement) {
                    targetElement.scrollIntoView({
                        behavior: 'smooth',
                        block: 'start'
                    });

                    // Track navigation event
                    trackEvent('navigation', {
                        type: 'anchor_click',
                        target: targetId,
                        source: link.textContent.trim()
                    });
                }
            }
        });
    }

    /**
     * Analytics functionality
     */
    function initializeAnalytics() {
        if (!config.analyticsEnabled) return;

        // Track page view
        trackPageView();

        // Track user interactions
        setupInteractionTracking();

        // Track time on page
        setupTimeTracking();

        // Flush analytics periodically
        setInterval(flushAnalytics, 30000); // Every 30 seconds

        // Flush analytics before page unload
        window.addEventListener('beforeunload', flushAnalytics);
    }

    function trackPageView() {
        const pageData = {
            url: window.location.pathname,
            title: document.title,
            referrer: document.referrer,
            userAgent: navigator.userAgent,
            screenResolution: `${screen.width}x${screen.height}`,
            viewportSize: `${window.innerWidth}x${window.innerHeight}`,
            timestamp: new Date().toISOString()
        };

        trackEvent('page_view', pageData);
    }

    function setupInteractionTracking() {
        // Track button clicks
        document.addEventListener('click', function (e) {
            const button = e.target.closest('button, .btn');
            if (button) {
                trackEvent('button_click', {
                    buttonText: button.textContent.trim(),
                    buttonClass: button.className,
                    buttonId: button.id || null
                });
            }
        });

        // Track form interactions
        document.addEventListener('submit', function (e) {
            const form = e.target;
            if (form.tagName === 'FORM') {
                trackEvent('form_submit', {
                    formId: form.id || null,
                    formClass: form.className,
                    formAction: form.action || null
                });
            }
        });

        // Track external link clicks
        document.addEventListener('click', function (e) {
            const link = e.target.closest('a[href]');
            if (link && link.hostname !== window.location.hostname) {
                trackEvent('external_link_click', {
                    url: link.href,
                    text: link.textContent.trim(),
                    target: link.target || '_self'
                });
            }
        });
    }

    function setupTimeTracking() {
        let timeSpent = 0;
        let isActive = true;

        // Track when user becomes inactive
        let inactivityTimer;
        function resetInactivityTimer() {
            clearTimeout(inactivityTimer);
            isActive = true;
            inactivityTimer = setTimeout(() => {
                isActive = false;
            }, 60000); // 1 minute of inactivity
        }

        // Reset timer on user activity
        ['mousedown', 'mousemove', 'keypress', 'scroll', 'touchstart'].forEach(event => {
            document.addEventListener(event, resetInactivityTimer, { passive: true });
        });

        // Track time every second
        setInterval(() => {
            if (isActive) {
                timeSpent++;
            }
        }, 1000);

        // Send time data when leaving page
        window.addEventListener('beforeunload', () => {
            if (timeSpent > 5) { // Only track if user spent more than 5 seconds
                trackEvent('time_on_page', {
                    timeSpent: timeSpent,
                    url: window.location.pathname
                });
            }
        });
    }

    /**
     * Scroll behavior enhancements
     */
    function initializeScrollBehavior() {
        // Intersection Observer for animations
        const observerOptions = {
            threshold: 0.1,
            rootMargin: '0px 0px -50px 0px'
        };

        const observer = new IntersectionObserver((entries) => {
            entries.forEach(entry => {
                if (entry.isIntersecting) {
                    entry.target.classList.add('fade-in');

                    // Track section views
                    const sectionName = entry.target.getAttribute('data-section') ||
                        entry.target.className.split(' ')[0];
                    trackEvent('section_view', { section: sectionName });
                }
            });
        }, observerOptions);

        // Observe all major sections
        document.querySelectorAll('section, .hero-section, .feature-card, .project-card').forEach(el => {
            observer.observe(el);
        });
    }

    /**
     * Tooltip functionality
     */
    function initializeTooltips() {
        const tooltipElements = document.querySelectorAll('[data-tooltip]');

        tooltipElements.forEach(element => {
            let tooltip;

            element.addEventListener('mouseenter', function () {
                const tooltipText = this.getAttribute('data-tooltip');
                if (!tooltipText) return;

                tooltip = document.createElement('div');
                tooltip.className = 'tooltip';
                tooltip.textContent = tooltipText;
                document.body.appendChild(tooltip);

                const rect = this.getBoundingClientRect();
                tooltip.style.left = rect.left + (rect.width / 2) - (tooltip.offsetWidth / 2) + 'px';
                tooltip.style.top = rect.top - tooltip.offsetHeight - 10 + 'px';

                setTimeout(() => tooltip.classList.add('visible'), 10);
            });

            element.addEventListener('mouseleave', function () {
                if (tooltip) {
                    tooltip.remove();
                    tooltip = null;
                }
            });
        });
    }

    /**
     * Error handling
     */
    function setupErrorHandling() {
        window.addEventListener('error', function (e) {
            console.error('JavaScript Error:', e.error);

            // Track error for debugging
            trackEvent('javascript_error', {
                message: e.message,
                filename: e.filename,
                lineno: e.lineno,
                colno: e.colno,
                stack: e.error ? e.error.stack : null
            });
        });

        // Handle promise rejections
        window.addEventListener('unhandledrejection', function (e) {
            console.error('Unhandled Promise Rejection:', e.reason);

            trackEvent('promise_rejection', {
                reason: e.reason ? e.reason.toString() : 'Unknown'
            });
        });
    }

    /**
     * Analytics functions
     */
    function trackEvent(eventName, data = {}) {
        if (!config.analyticsEnabled) return;

        const event = {
            sessionId: state.sessionId,
            event: eventName,
            data: data,
            timestamp: new Date().toISOString(),
            url: window.location.pathname,
            userAgent: navigator.userAgent
        };

        state.analytics.events.push(event);

        // Auto-flush if we have too many events
        if (state.analytics.events.length >= 10) {
            flushAnalytics();
        }
    }

    function flushAnalytics() {
        if (!config.analyticsEnabled || state.analytics.events.length === 0) return;

        const events = [...state.analytics.events];
        state.analytics.events = [];
        state.analytics.lastFlush = Date.now();

        // Send to server
        fetch('/Home/TrackAnalytics', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
            },
            body: JSON.stringify({
                pageUrl: window.location.pathname,
                action: 'batch_events',
                data: { events: events }
            })
        }).catch(error => {
            console.debug('Analytics tracking failed:', error);
            // Re-add events back to queue if sending failed
            state.analytics.events.unshift(...events);
        });
    }

    /**
     * Utility functions
     */
    function generateSessionId() {
        return 'sess_' + Math.random().toString(36).substr(2, 9) + '_' + Date.now();
    }

    function debounce(func, wait, immediate) {
        let timeout;
        return function executedFunction() {
            const context = this;
            const args = arguments;

            const later = function () {
                timeout = null;
                if (!immediate) func.apply(context, args);
            };

            const callNow = immediate && !timeout;
            clearTimeout(timeout);
            timeout = setTimeout(later, wait);

            if (callNow) func.apply(context, args);
        };
    }

    function throttle(func, limit) {
        let inThrottle;
        return function () {
            const args = arguments;
            const context = this;
            if (!inThrottle) {
                func.apply(context, args);
                inThrottle = true;
                setTimeout(() => inThrottle = false, limit);
            }
        };
    }

    /**
     * Form utilities
     */
    function showFieldError(field, message) {
        clearFieldError(field);

        field.classList.add('error');
        const errorDiv = document.createElement('div');
        errorDiv.className = 'field-error';
        errorDiv.textContent = message;
        errorDiv.style.display = 'block';

        field.parentNode.appendChild(errorDiv);
    }

    function clearFieldError(field) {
        field.classList.remove('error');
        const existingError = field.parentNode.querySelector('.field-error');
        if (existingError) {
            existingError.remove();
        }
    }

    function showNotification(message, type = 'info', duration = 5000) {
        const notification = document.createElement('div');
        notification.className = `notification notification-${type}`;
        notification.innerHTML = `
            <div class="notification-content">
                <span class="notification-message">${message}</span>
                <button class="notification-close">&times;</button>
            </div>
        `;

        document.body.appendChild(notification);

        // Position the notification
        notification.style.position = 'fixed';
        notification.style.top = '20px';
        notification.style.right = '20px';
        notification.style.zIndex = '10000';

        // Show notification
        setTimeout(() => notification.classList.add('show'), 100);

        // Close functionality
        const closeBtn = notification.querySelector('.notification-close');
        closeBtn.addEventListener('click', () => closeNotification(notification));

        // Auto-close
        if (duration > 0) {
            setTimeout(() => closeNotification(notification), duration);
        }

        function closeNotification(notif) {
            notif.classList.add('closing');
            setTimeout(() => notif.remove(), 300);
        }
    }

    /**
     * Loading state management
     */
    function setLoadingState(element, loading = true) {
        if (loading) {
            element.classList.add('loading');
            element.disabled = true;
        } else {
            element.classList.remove('loading');
            element.disabled = false;
        }
    }

    /**
     * Public API
     */
    return {
        init: init,
        trackEvent: trackEvent,
        showFieldError: showFieldError,
        clearFieldError: clearFieldError,
        showNotification: showNotification,
        setLoadingState: setLoadingState,
        debounce: debounce,
        throttle: throttle,
        generateSessionId: generateSessionId,
        getSessionId: () => state.sessionId,
        getAnalyticsEvents: () => state.analytics.events
    };
})();

// Auto-initialize when DOM is ready
document.addEventListener('DOMContentLoaded', function () {
    window.PlatformCore.init();
});

// Global analytics shorthand
window.PlatformAnalytics = {
    init: function () {
        // Initialization handled by PlatformCore
    },
    track: function (event, data) {
        window.PlatformCore.trackEvent(event, data);
    }
};