// PencilImpact.Web.Concept/wwwroot/js/site.js

/**
 * PencilImpact Site-wide JavaScript
 * Global functionality and utilities
 */

(function () {
    'use strict';

    // Global configuration
    window.PencilImpactConfig = {
        apiBaseUrl: '/api',
        analyticsEnabled: true,
        version: '1.0.0',
        environment: 'concept'
    };

    /**
     * Initialize site-wide functionality
     */
    function initializeSite() {
        console.log('PencilImpact Site JavaScript loaded');

        // Initialize global features
        initializeGlobalScrollEffects();
        initializeGlobalKeyboardShortcuts();
        initializeGlobalAccessibility();
        initializePerformanceTracking();

        // Log initialization
        console.log('Site initialization complete');
    }

    /**
     * Global scroll effects and behaviors
     */
    function initializeGlobalScrollEffects() {
        let ticking = false;

        function updateScrollEffects() {
            const scrollTop = window.pageYOffset;
            const windowHeight = window.innerHeight;
            const documentHeight = document.documentElement.scrollHeight;

            // Update scroll progress
            const scrollProgress = (scrollTop / (documentHeight - windowHeight)) * 100;
            document.body.insertBefore(skipLink, document.body.firstChild);

            // Ensure main content has ID
            const mainContent = document.querySelector('main, .main-content');
            if (mainContent && !mainContent.id) {
                mainContent.id = 'main-content';
            }
        }
    }

    function setupModalFocusManagement() {
        // Focus trap for modals
        document.addEventListener('keydown', function (e) {
            const modal = document.querySelector('.modal:not([style*="display: none"])');
            if (modal && e.key === 'Tab') {
                trapFocus(modal, e);
            }
        });
    }

    function trapFocus(container, event) {
        const focusableElements = container.querySelectorAll(
            'button, [href], input, select, textarea, [tabindex]:not([tabindex="-1"])'
        );
        const firstFocusable = focusableElements[0];
        const lastFocusable = focusableElements[focusableElements.length - 1];

        if (event.shiftKey) {
            if (document.activeElement === firstFocusable) {
                lastFocusable.focus();
                event.preventDefault();
            }
        } else {
            if (document.activeElement === lastFocusable) {
                firstFocusable.focus();
                event.preventDefault();
            }
        }
    }

    function setupKeyboardNavigation() {
        // Arrow key navigation for carousels
        const carousels = document.querySelectorAll('.carousel, .features-carousel');
        carousels.forEach(carousel => {
            carousel.addEventListener('keydown', function (e) {
                if (e.key === 'ArrowLeft') {
                    const prevBtn = this.querySelector('.carousel-prev');
                    if (prevBtn) prevBtn.click();
                } else if (e.key === 'ArrowRight') {
                    const nextBtn = this.querySelector('.carousel-next');
                    if (nextBtn) nextBtn.click();
                }
            });
        });

        // Enter/Space for clickable cards
        const clickableCards = document.querySelectorAll('.project-card, .milestone-card, .phase-card');
        clickableCards.forEach(card => {
            if (!card.getAttribute('tabindex')) {
                card.setAttribute('tabindex', '0');
            }

            card.addEventListener('keydown', function (e) {
                if (e.key === 'Enter' || e.key === ' ') {
                    e.preventDefault();
                    this.click();
                }
            });
        });
    }

    function detectHighContrastMode() {
        // Detect Windows high contrast mode
        if (window.matchMedia('(-ms-high-contrast: active)').matches) {
            document.body.classList.add('high-contrast');
        }

        // Detect prefers-contrast
        if (window.matchMedia('(prefers-contrast: high)').matches) {
            document.body.classList.add('prefers-high-contrast');
        }
    }

    /**
     * Performance tracking
     */
    function initializePerformanceTracking() {
        // Track page load performance
        window.addEventListener('load', function () {
            setTimeout(() => {
                if (window.performance) {
                    const navigation = performance.getEntriesByType('navigation')[0];
                    const paintEntries = performance.getEntriesByType('paint');

                    const perfData = {
                        loadTime: navigation.loadEventEnd - navigation.loadEventStart,
                        domContentLoaded: navigation.domContentLoadedEventEnd - navigation.domContentLoadedEventStart,
                        firstPaint: paintEntries.find(entry => entry.name === 'first-paint')?.startTime,
                        firstContentfulPaint: paintEntries.find(entry => entry.name === 'first-contentful-paint')?.startTime
                    };

                    // Track performance if analytics is available
                    if (window.PlatformCore) {
                        window.PlatformCore.trackEvent('page_performance', perfData);
                    }
                }
            }, 0);
        });

        // Track Core Web Vitals
        if ('web-vitals' in window) {
            // This would require web-vitals library
            // For now, we'll track basic metrics
        }
    }

    /**
     * Utility functions
     */
    function isElementInViewport(element) {
        const rect = element.getBoundingClientRect();
        return (
            rect.top >= 0 &&
            rect.left >= 0 &&
            rect.bottom <= (window.innerHeight || document.documentElement.clientHeight) &&
            rect.right <= (window.innerWidth || document.documentElement.clientWidth)
        );
    }

    function closeAllModals() {
        const modals = document.querySelectorAll('.modal, .milestone-modal');
        modals.forEach(modal => {
            modal.style.display = 'none';
            modal.classList.remove('show');
        });
    }

    function closeAllDropdowns() {
        const dropdowns = document.querySelectorAll('.dropdown-open, .nav-menu-open');
        dropdowns.forEach(dropdown => {
            dropdown.classList.remove('dropdown-open', 'nav-menu-open');
        });
    }

    /**
     * Global error handling
     */
    function setupGlobalErrorHandling() {
        window.addEventListener('error', function (e) {
            console.error('Global error:', e.error);

            // Track error if analytics is available
            if (window.PlatformCore) {
                window.PlatformCore.trackEvent('javascript_error', {
                    message: e.message,
                    filename: e.filename,
                    lineno: e.lineno,
                    colno: e.colno,
                    stack: e.error?.stack
                });
            }
        });

        window.addEventListener('unhandledrejection', function (e) {
            console.error('Unhandled promise rejection:', e.reason);

            // Track promise rejection if analytics is available
            if (window.PlatformCore) {
                window.PlatformCore.trackEvent('promise_rejection', {
                    reason: e.reason?.toString(),
                    stack: e.reason?.stack
                });
            }
        });
    }

    /**
     * Initialize everything when DOM is ready
     */
    if (document.readyState === 'loading') {
        document.addEventListener('DOMContentLoaded', initializeSite);
    } else {
        initializeSite();
    }

    // Setup error handling immediately
    setupGlobalErrorHandling();

    /**
     * Global utility functions available to other scripts
     */
    window.PencilImpactUtils = {
        isElementInViewport: isElementInViewport,
        closeAllModals: closeAllModals,
        closeAllDropdowns: closeAllDropdowns,
        trapFocus: trapFocus
    };

    /**
     * Polyfills for older browsers
     */
    (function loadPolyfills() {
        // IntersectionObserver polyfill check
        if (!('IntersectionObserver' in window)) {
            console.warn('IntersectionObserver not supported, animations may not work');
        }

        // matches polyfill
        if (!Element.prototype.matches) {
            Element.prototype.matches = Element.prototype.msMatchesSelector || Element.prototype.webkitMatchesSelector;
        }

        // closest polyfill
        if (!Element.prototype.closest) {
            Element.prototype.closest = function (s) {
                var el = this;
                do {
                    if (Element.prototype.matches.call(el, s)) return el;
                    el = el.parentElement || el.parentNode;
                } while (el !== null && el.nodeType === 1);
                return null;
            };
        }

        // NodeList.forEach polyfill
        if (window.NodeList && !NodeList.prototype.forEach) {
            NodeList.prototype.forEach = Array.prototype.forEach;
        }
    })();

    /**
     * Console branding (fun touch)
     */
    if (typeof console !== 'undefined') {
        const styles = [
            'color: #4f46e5',
            'font-size: 16px',
            'font-weight: bold',
            'text-shadow: 1px 1px 2px rgba(0,0,0,0.3)'
        ].join(';');

        console.log('%cPencilImpact 🖊️', styles);
        console.log('%cSmall Tokens, Big Impacts', 'color: #06b6d4; font-size: 12px;');
        console.log('Version: ' + window.PencilImpactConfig.version);

        if (window.PencilImpactConfig.environment === 'concept') {
            console.log('%c🚧 This is a concept site showcasing our vision', 'color: #f59e0b; font-weight: bold;');
        }
    }

})(); documentElement.style.setProperty('--scroll-progress', scrollProgress + '%');

// Parallax effects for hero sections
const heroSections = document.querySelectorAll('.hero-section, .roadmap-hero-section, .vision-hero-section');
heroSections.forEach(hero => {
    if (isElementInViewport(hero)) {
        const rate = scrollTop * -0.5;
        hero.style.transform = `translateY(${rate}px)`;
    }
});
q
function requestScrollUpdate() {
    if (!ticking) {
        requestAnimationFrame(updateScrollEffects);
        ticking = true;
    }
}

window.addEventListener('scroll', requestScrollUpdate, { passive: true });
    }

/**
 * Global keyboard shortcuts
 */
function initializeGlobalKeyboardShortcuts() {
    document.addEventListener('keydown', function (e) {
        // Escape key - close modals, dropdowns, etc.
        if (e.key === 'Escape') {
            closeAllModals();
            closeAllDropdowns();
        }

        // Ctrl/Cmd + K - Focus search (if exists)
        if ((e.ctrlKey || e.metaKey) && e.key === 'k') {
            e.preventDefault();
            const searchInput = document.querySelector('input[type="search"], .search-input');
            if (searchInput) {
                searchInput.focus();
            }
        }

        // Alt + H - Go to home
        if (e.altKey && e.key === 'h') {
            e.preventDefault();
            window.location.href = '/';
        }

        // Alt + V - Go to vision
        if (e.altKey && e.key === 'v') {
            e.preventDefault();
            window.location.href = '/vision';
        }

        // Alt + D - Go to demo
        if (e.altKey && e.key === 'd') {
            e.preventDefault();
            window.location.href = '/demo';
        }

        // Alt + R - Go to roadmap
        if (e.altKey && e.key === 'r') {
            e.preventDefault();
            window.location.href = '/roadmap';
        }
    });
}

/**
 * Global accessibility enhancements
 */
function initializeGlobalAccessibility() {
    // Skip to main content link
    addSkipToMainContent();

    // Focus management for modals
    setupModalFocusManagement();

    // Keyboard navigation for custom components
    setupKeyboardNavigation();

    // High contrast mode detection
    detectHighContrastMode();
}

function addSkipToMainContent() {
    if (!document.querySelector('.skip-to-main')) {
        const skipLink = document.createElement('a');
        skipLink.href = '#main-content';
        skipLink.className = 'skip-to-main sr-only-focusable';
        skipLink.textContent = 'Skip to main content';
        skipLink.style.cssText = `
                position: absolute;
                top: -40px;
                left: 6px;
                z-index: 1000;
                color: white;
                background: var(--primary-color);
                padding: 8px 16px;
                text-decoration: none;
                border-radius: 4px;
                transition: top 0.3s;
            `;

        skipLink.addEventListener('focus', function () {
            this.style.top = '6px';
        });

        skipLink.addEventListener('blur', function () {
            this.style.top = '-40px';
        });

        document.body.insertBefore(skipLink, document.body.firstChild);

        // Ensure main content has ID
        const mainContent = document.querySelector('main, .main-content');
        if (mainContent && !mainContent.id) {
            mainContent.id = 'main-content';
        }
    }
}

function setupModalFocusManagement() {
    // Focus trap for modals
    document.addEventListener('keydown', function (e) {
        const modal = document.querySelector('.modal:not([style*="display: none"])');
        if (modal && e.key === 'Tab') {
            trapFocus(modal, e);
        }
    });
}

function trapFocus(container, event) {
    const focusableElements = container.querySelectorAll(
        'button, [href], input, select, textarea, [tabindex]:not([tabindex="-1"])'
    );
    const firstFocusable = focusableElements[0];
    const lastFocusable = focusableElements[focusableElements.length - 1];

    if (event.shiftKey) {
        if (document.activeElement === firstFocusable) {
            lastFocusable.focus();
            event.preventDefault();
        }
    } else {
        if (document.activeElement === lastFocusable) {
            firstFocusable.focus();
            event.preventDefault();
        }
    }
}

function setupKeyboardNavigation() {
    // Arrow key navigation for carousels
    const carousels = document.querySelectorAll('.carousel, .features-carousel');
    carousels.forEach(carousel => {
        carousel.addEventListener('keydown', function (e) {
            if (e.key === 'ArrowLeft') {
                const prevBtn = this.querySelector('.carousel-prev');
                if (prevBtn) prevBtn.click();
            } else if (e.key === 'ArrowRight') {
                const nextBtn = this.querySelector('.carousel-next');
                if (nextBtn) nextBtn.click();
            }
        });
    });

    // Enter/Space for clickable cards
    const clickableCards = document.querySelectorAll('.project-card, .milestone-card, .phase-card');
    clickableCards.forEach(card => {
        if (!card.getAttribute('tabindex')) {
            card.setAttribute('tabindex', '0');
        }

        card.addEventListener('keydown', function (e) {
            if (e.key === 'Enter' || e.key === ' ') {
                e.preventDefault();
                this.click();
            }
        });
    });
}

function detectHighContrastMode() {
    // Detect Windows high contrast mode
    if (window.matchMedia('(-ms-high-contrast: active)').matches) {
        document.body.classList.add('high-contrast');
    }

    // Detect prefers-contrast
    if (window.matchMedia('(prefers-contrast: high)').matches) {
        document.body.classList.add('prefers-high-contrast');
    }
}

/**
 * Performance tracking
 */
function initializePerformanceTracking() {
    // Track page load performance
    window.addEventListener('load', function () {
        setTimeout(() => {
            if (window.performance) {
                const navigation = performance.getEntriesByType('navigation')[0];
                const paintEntries = performance.getEntriesByType('paint');

                const perfData = {
                    loadTime: navigation.loadEventEnd - navigation.loadEventStart,
                    domContentLoaded: navigation.domContentLoadedEventEnd - navigation.domContentLoadedEventStart,
                    firstPaint: paintEntries.find(entry => entry.name === 'first-paint')?.startTime,
                    firstContentfulPaint: paintEntries.find(entry => entry.name === 'first-contentful-paint')?.startTime
                };

                // Track performance if analytics is available
                if (window.PlatformCore) {
                    window.PlatformCore.trackEvent('page_performance', perfData);
                }
            }
        }, 0);
    });

    // Track Core Web Vitals
    if ('web-vitals' in window) {
        // This would require web-vitals library
        // For now, we'll track basic metrics
    }
}

/**
 * Utility functions
 */
function isElementInViewport(element) {
    const rect = element.getBoundingClientRect();
    return (
        rect.top >= 0 &&
        rect.left >= 0 &&
        rect.bottom <= (window.innerHeight || document.documentElement.clientHeight) &&
        rect.right <= (window.innerWidth || document.documentElement.clientWidth)
    );
}

function closeAllModals() {
    const modals = document.querySelectorAll('.modal, .milestone-modal');
    modals.forEach(modal => {
        modal.style.display = 'none';
        modal.classList.remove('show');
    });
}

function closeAllDropdowns() {
    const dropdowns = document.querySelectorAll('.dropdown-open, .nav-menu-open');
    dropdowns.forEach(dropdown => {
        dropdown.classList.remove('dropdown-open', 'nav-menu-open');
    });
}

/**
 * Global error handling
 */
function setupGlobalErrorHandling() {
    window.addEventListener('error', function (e) {
        console.error('Global error:', e.error);

        // Track error if analytics is available
        if (window.PlatformCore) {
            window.PlatformCore.trackEvent('javascript_error', {
                message: e.message,
                filename: e.filename,
                lineno: e.lineno,
                colno: e.colno,
                stack: e.error?.stack
            });
        }
    });

    window.addEventListener('unhandledrejection', function (e) {
        console.error('Unhandled promise rejection:', e.reason);

        // Track promise rejection if analytics is available
        if (window.PlatformCore) {
            window.PlatformCore.trackEvent('promise_rejection', {
                reason: e.reason?.toString(),
                stack: e.reason?.stack
            });
        }
    });
}

/**
 * Initialize everything when DOM is ready
 */
if (document.readyState === 'loading') {
    document.addEventListener('DOMContentLoaded', initializeSite);
} else {
    initializeSite();
}

// Setup error handling immediately
setupGlobalErrorHandling();

/**
 * Global utility functions available to other scripts
 */
window.PencilImpactUtils = {
    isElementInViewport: isElementInViewport,
    closeAllModals: closeAllModals,
    closeAllDropdowns: closeAllDropdowns,
    trapFocus: trapFocus
};

/**
 * Polyfills for older browsers
 */
(function loadPolyfills() {
    // IntersectionObserver polyfill check
    if (!('IntersectionObserver' in window)) {
        console.warn('IntersectionObserver not supported, animations may not work');
    }

    // matches polyfill
    if (!Element.prototype.matches) {
        Element.prototype.matches = Element.prototype.msMatchesSelector || Element.prototype.webkitMatchesSelector;
    }

    // closest polyfill
    if (!Element.prototype.closest) {
        Element.prototype.closest = function (s) {
            var el = this;
            do {
                if (Element.prototype.matches.call(el, s)) return el;
                el = el.parentElement || el.parentNode;
            } while (el !== null && el.nodeType === 1);
            return null;
        };
    }

    // NodeList.forEach polyfill
    if (window.NodeList && !NodeList.prototype.forEach) {
        NodeList.prototype.forEach = Array.prototype.forEach;
    }
})();

/**
 * Console branding (fun touch)
 */
if (typeof console !== 'undefined') {
    const styles = [
        'color: #4f46e5',
        'font-size: 16px',
        'font-weight: bold',
        'text-shadow: 1px 1px 2px rgba(0,0,0,0.3)'
    ].join(';');

    console.log('%cPencilImpact 🖊️', styles);
    console.log('%cSmall Tokens, Big Impacts', 'color: #06b6d4; font-size: 12px;');
    console.log('Version: ' + window.PencilImpactConfig.version);

    if (window.PencilImpactConfig.environment === 'concept') {
        console.log('%c🚧 This is a concept site showcasing our vision', 'color: #f59e0b; font-weight: bold;');
    }
}

/**
 * Service Worker registration (for future PWA features)
 */
function registerServiceWorker() {
    if ('serviceWorker' in navigator && window.location.protocol === 'https:') {
        navigator.serviceWorker.register('/sw.js')
            .then(function (registration) {
                console.log('ServiceWorker registration successful:', registration.scope);
            })
            .catch(function (error) {
                console.log('ServiceWorker registration failed:', error);
            });
    }
}

/**
 * Dark/Light theme toggle (for future theme switching)
 */
function initializeThemeToggle() {
    const themeToggle = document.querySelector('.theme-toggle');
    if (themeToggle) {
        themeToggle.addEventListener('click', function () {
            const currentTheme = document.documentElement.getAttribute('data-theme') || 'dark';
            const newTheme = currentTheme === 'dark' ? 'light' : 'dark';

            document.documentElement.setAttribute('data-theme', newTheme);
            localStorage.setItem('preferred-theme', newTheme);

            // Track theme change
            if (window.PlatformCore) {
                window.PlatformCore.trackEvent('theme_change', { theme: newTheme });
            }
        });
    }

    // Apply saved theme preference
    const savedTheme = localStorage.getItem('preferred-theme');
    if (savedTheme) {
        document.documentElement.setAttribute('data-theme', savedTheme);
    }
}

/**
 * Image lazy loading (for performance)
 */
function initializeLazyLoading() {
    if ('IntersectionObserver' in window) {
        const imageObserver = new IntersectionObserver((entries, observer) => {
            entries.forEach(entry => {
                if (entry.isIntersecting) {
                    const img = entry.target;
                    img.src = img.dataset.src;
                    img.classList.remove('lazy');
                    img.classList.add('loaded');
                    observer.unobserve(img);
                }
            });
        });

        const lazyImages = document.querySelectorAll('img[data-src]');
        lazyImages.forEach(img => {
            img.classList.add('lazy');
            imageObserver.observe(img);
        });
    } else {
        // Fallback for browsers without IntersectionObserver
        const lazyImages = document.querySelectorAll('img[data-src]');
        lazyImages.forEach(img => {
            img.src = img.dataset.src;
            img.classList.add('loaded');
        });
    }
}

/**
 * Form enhancement utilities
 */
window.PencilImpactForms = {
    /**
     * Enhanced form validation
     */
    validateForm: function (form) {
        const requiredFields = form.querySelectorAll('[required]');
        let isValid = true;

        requiredFields.forEach(field => {
            if (!this.validateField(field)) {
                isValid = false;
            }
        });

        return isValid;
    },

    /**
     * Field validation with custom messages
     */
    validateField: function (field) {
        const value = field.value.trim();
        const fieldType = field.type || field.tagName.toLowerCase();
        let isValid = true;
        let message = '';

        // Clear previous errors
        this.clearFieldError(field);

        // Required field check
        if (field.hasAttribute('required') && !value) {
            isValid = false;
            message = `${this.getFieldLabel(field)} is required`;
        }

        // Email validation
        if (fieldType === 'email' && value) {
            const emailRegex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
            if (!emailRegex.test(value)) {
                isValid = false;
                message = 'Please enter a valid email address';
            }
        }

        // Phone validation
        if (field.dataset.validate === 'phone' && value) {
            const phoneRegex = /^[\+]?[1-9][\d]{0,15}$/;
            if (!phoneRegex.test(value.replace(/[\s\-\(\)]/g, ''))) {
                isValid = false;
                message = 'Please enter a valid phone number';
            }
        }

        // Custom validation
        if (field.dataset.customValidation) {
            const customResult = this.runCustomValidation(field, value);
            if (!customResult.isValid) {
                isValid = false;
                message = customResult.message;
            }
        }

        // Show error if invalid
        if (!isValid) {
            this.showFieldError(field, message);
        }

        return isValid;
    },

    /**
     * Show field error
     */
    showFieldError: function (field, message) {
        field.classList.add('field-error');

        // Create or update error message
        let errorElement = field.parentNode.querySelector('.field-error-message');
        if (!errorElement) {
            errorElement = document.createElement('div');
            errorElement.className = 'field-error-message';
            field.parentNode.appendChild(errorElement);
        }

        errorElement.textContent = message;
        errorElement.style.display = 'block';
    },

    /**
     * Clear field error
     */
    clearFieldError: function (field) {
        field.classList.remove('field-error');
        const errorElement = field.parentNode.querySelector('.field-error-message');
        if (errorElement) {
            errorElement.style.display = 'none';
        }
    },

    /**
     * Get field label text
     */
    getFieldLabel: function (field) {
        const label = field.closest('.form-group')?.querySelector('label');
        return label ? label.textContent.replace('*', '').trim() : field.name || 'Field';
    },

    /**
     * Run custom validation rules
     */
    runCustomValidation: function (field, value) {
        const validationType = field.dataset.customValidation;

        switch (validationType) {
            case 'school-name':
                if (value.length < 3) {
                    return { isValid: false, message: 'School name must be at least 3 characters' };
                }
                break;

            case 'donation-amount':
                const amount = parseFloat(value);
                if (isNaN(amount) || amount < 5) {
                    return { isValid: false, message: 'Minimum donation amount is $5' };
                }
                if (amount > 10000) {
                    return { isValid: false, message: 'Maximum donation amount is $10,000' };
                }
                break;

            case 'token-amount':
                const tokens = parseFloat(value);
                if (isNaN(tokens) || tokens < 1) {
                    return { isValid: false, message: 'Minimum token amount is 1 TEACH' };
                }
                break;
        }

        return { isValid: true };
    }
};

/**
 * Animation utilities
 */
window.PencilImpactAnimations = {
    /**
     * Smooth scroll to element
     */
    scrollToElement: function (element, offset = 0) {
        const targetPosition = element.getBoundingClientRect().top + window.pageYOffset - offset;

        window.scrollTo({
            top: targetPosition,
            behavior: 'smooth'
        });
    },

    /**
     * Fade in element
     */
    fadeIn: function (element, duration = 300) {
        element.style.opacity = '0';
        element.style.display = 'block';

        let start = null;
        function animate(timestamp) {
            if (!start) start = timestamp;
            const progress = timestamp - start;

            element.style.opacity = Math.min(progress / duration, 1);

            if (progress < duration) {
                requestAnimationFrame(animate);
            }
        }

        requestAnimationFrame(animate);
    },

    /**
     * Fade out element
     */
    fadeOut: function (element, duration = 300) {
        let start = null;
        const initialOpacity = parseFloat(getComputedStyle(element).opacity);

        function animate(timestamp) {
            if (!start) start = timestamp;
            const progress = timestamp - start;

            element.style.opacity = initialOpacity * (1 - progress / duration);

            if (progress < duration) {
                requestAnimationFrame(animate);
            } else {
                element.style.display = 'none';
            }
        }

        requestAnimationFrame(animate);
    },

    /**
     * Slide down element
     */
    slideDown: function (element, duration = 300) {
        element.style.overflow = 'hidden';
        element.style.height = '0';
        element.style.display = 'block';

        const targetHeight = element.scrollHeight;
        let start = null;

        function animate(timestamp) {
            if (!start) start = timestamp;
            const progress = timestamp - start;

            element.style.height = (targetHeight * Math.min(progress / duration, 1)) + 'px';

            if (progress < duration) {
                requestAnimationFrame(animate);
            } else {
                element.style.height = '';
                element.style.overflow = '';
            }
        }

        requestAnimationFrame(animate);
    },

    /**
     * Slide up element
     */
    slideUp: function (element, duration = 300) {
        element.style.overflow = 'hidden';
        const initialHeight = element.offsetHeight;
        let start = null;

        function animate(timestamp) {
            if (!start) start = timestamp;
            const progress = timestamp - start;

            element.style.height = (initialHeight * (1 - Math.min(progress / duration, 1))) + 'px';

            if (progress < duration) {
                requestAnimationFrame(animate);
            } else {
                element.style.display = 'none';
                element.style.height = '';
                element.style.overflow = '';
            }
        }

        requestAnimationFrame(animate);
    }
};

/**
 * Cookie utilities (for GDPR compliance when needed)
 */
window.PencilImpactCookies = {
    set: function (name, value, days = 365) {
        const expires = new Date();
        expires.setTime(expires.getTime() + (days * 24 * 60 * 60 * 1000));
        document.cookie = `${name}=${value};expires=${expires.toUTCString()};path=/;SameSite=Lax`;
    },

    get: function (name) {
        const nameEQ = name + "=";
        const ca = document.cookie.split(';');
        for (let i = 0; i < ca.length; i++) {
            let c = ca[i];
            while (c.charAt(0) === ' ') c = c.substring(1, c.length);
            if (c.indexOf(nameEQ) === 0) return c.substring(nameEQ.length, c.length);
        }
        return null;
    },

    delete: function (name) {
        document.cookie = `${name}=;expires=Thu, 01 Jan 1970 00:00:00 UTC;path=/;`;
    }
};

/**
 * Initialize additional features when DOM is ready
 */
document.addEventListener('DOMContentLoaded', function () {
    initializeLazyLoading();
    initializeThemeToggle();

    // Register service worker in production
    if (window.location.hostname !== 'localhost') {
        registerServiceWorker();
    }
});
x Vioions