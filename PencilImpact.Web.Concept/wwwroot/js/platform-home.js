// PencilImpact.Web.Concept/wwwroot/js/platform-home.js

/**
 * PencilImpact Home Page JavaScript
 * Handles waitlist signup, animations, and home-specific interactions
 */

window.PlatformHome = (function () {
    'use strict';

    // State management
    let state = {
        waitlistForm: null,
        isSubmitting: false,
        animationObserver: null,
        statsAnimated: false
    };

    /**
     * Initialize home page functionality
     */
    function init() {
        initializeWaitlistForm();
        initializeAnimations();
        initializeCounterAnimations();
        initializeHeroInteractions();
        initializeProjectCarousel();
        setupConditionalFields();

        console.log('PlatformHome initialized');
    }

    /**
     * Waitlist form functionality
     */
    function initializeWaitlistForm() {
        state.waitlistForm = document.getElementById('waitlistForm');
        if (!state.waitlistForm) return;

        // Form validation
        setupFormValidation();

        // Form submission
        state.waitlistForm.addEventListener('submit', handleWaitlistSubmit);

        // Real-time validation
        const emailInput = document.getElementById('email');
        if (emailInput) {
            emailInput.addEventListener('blur', validateEmail);
            emailInput.addEventListener('input', window.PlatformCore.debounce(clearEmailError, 300));
        }

        // User type change handler
        const userTypeSelect = document.getElementById('userType');
        if (userTypeSelect) {
            userTypeSelect.addEventListener('change', handleUserTypeChange);
        }
    }

    function setupFormValidation() {
        const form = state.waitlistForm;
        const inputs = form.querySelectorAll('input[required], select[required]');

        inputs.forEach(input => {
            input.addEventListener('blur', function () {
                validateField(this);
            });

            input.addEventListener('input', function () {
                if (this.classList.contains('error')) {
                    window.PlatformCore.clearFieldError(this);
                }
            });
        });
    }

    function validateField(field) {
        const value = field.value.trim();
        let isValid = true;
        let errorMessage = '';

        // Required field validation
        if (field.hasAttribute('required') && !value) {
            isValid = false;
            errorMessage = `${getFieldLabel(field)} is required`;
        }

        // Email validation
        if (field.type === 'email' && value) {
            const emailRegex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
            if (!emailRegex.test(value)) {
                isValid = false;
                errorMessage = 'Please enter a valid email address';
            }
        }

        if (!isValid) {
            window.PlatformCore.showFieldError(field, errorMessage);
        } else {
            window.PlatformCore.clearFieldError(field);
        }

        return isValid;
    }

    function validateEmail() {
        const emailInput = document.getElementById('email');
        if (emailInput) {
            validateField(emailInput);
        }
    }

    function clearEmailError() {
        const emailInput = document.getElementById('email');
        if (emailInput && emailInput.classList.contains('error')) {
            window.PlatformCore.clearFieldError(emailInput);
        }
    }

    function handleUserTypeChange(e) {
        const userType = e.target.value;
        const educatorFields = document.getElementById('educatorFields');
        const description = document.getElementById('userTypeDescription');

        // Show/hide educator-specific fields
        if (educatorFields) {
            if (userType === 'Educator') {
                educatorFields.style.display = 'block';
                educatorFields.classList.add('fade-in');
            } else {
                educatorFields.style.display = 'none';
                educatorFields.classList.remove('fade-in');
            }
        }

        // Update description text
        if (description) {
            const selectedOption = e.target.selectedOptions[0];
            const descriptionText = selectedOption ? selectedOption.getAttribute('data-description') : '';
            description.textContent = descriptionText;
            description.style.display = descriptionText ? 'block' : 'none';
        }

        // Track user type selection
        window.PlatformCore.trackEvent('waitlist_user_type_selected', {
            userType: userType,
            timestamp: new Date().toISOString()
        });
    }

    async function handleWaitlistSubmit(e) {
        e.preventDefault();

        if (state.isSubmitting) return;

        const form = e.target;
        const submitButton = document.getElementById('submitButton');
        const formSuccess = document.getElementById('formSuccess');
        const formError = document.getElementById('formError');

        // Validate all fields
        let isFormValid = true;
        const requiredFields = form.querySelectorAll('input[required], select[required]');

        requiredFields.forEach(field => {
            if (!validateField(field)) {
                isFormValid = false;
            }
        });

        if (!isFormValid) {
            window.PlatformCore.showNotification('Please fix the errors above', 'error');
            return;
        }

        // Prepare form data
        const formData = {
            email: document.getElementById('email').value.trim(),
            userType: document.getElementById('userType').value,
            schoolDistrict: document.getElementById('schoolDistrict')?.value?.trim() || null,
            teachingSubject: document.getElementById('teachingSubject')?.value || null,
            interestedInTEACHTokens: document.getElementById('interestedInTokens')?.checked || false,
            subscribeToUpdates: document.getElementById('subscribeToUpdates')?.checked || true
        };

        state.isSubmitting = true;

        // Update UI to loading state
        window.PlatformCore.setLoadingState(submitButton, true);
        const originalText = submitButton.querySelector('.btn-text').textContent;
        submitButton.querySelector('.btn-text').textContent = 'Submitting...';
        submitButton.querySelector('.btn-spinner').style.display = 'block';

        // Hide previous messages
        formSuccess.style.display = 'none';
        formError.style.display = 'none';

        try {
            const response = await fetch('/Home/SubmitWaitlist', {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json',
                },
                body: JSON.stringify(formData)
            });

            const result = await response.json();

            if (result.success) {
                // Show success message
                formSuccess.style.display = 'block';
                formSuccess.classList.add('fade-in');

                // Hide form
                form.style.display = 'none';

                // Track successful signup
                window.PlatformCore.trackEvent('waitlist_signup_success', {
                    userType: formData.userType,
                    interestedInTokens: formData.interestedInTEACHTokens,
                    waitlistId: result.waitlistId
                });

                // Show notification
                window.PlatformCore.showNotification(
                    result.message || 'Successfully joined waitlist!',
                    'success'
                );

                // Scroll to success message
                formSuccess.scrollIntoView({ behavior: 'smooth', block: 'center' });

            } else {
                // Handle specific error cases
                if (result.errorCode === 'EMAIL_EXISTS') {
                    const emailField = document.getElementById('email');
                    window.PlatformCore.showFieldError(emailField, 'This email is already on our waitlist');
                } else {
                    // Show general error
                    formError.textContent = result.message || 'An error occurred. Please try again.';
                    formError.style.display = 'block';
                }

                // Track failed signup
                window.PlatformCore.trackEvent('waitlist_signup_error', {
                    errorCode: result.errorCode,
                    userType: formData.userType
                });
            }

        } catch (error) {
            console.error('Waitlist submission error:', error);

            formError.textContent = 'Network error. Please check your connection and try again.';
            formError.style.display = 'block';

            window.PlatformCore.trackEvent('waitlist_signup_network_error', {
                error: error.message
            });
        } finally {
            state.isSubmitting = false;

            // Reset button state
            window.PlatformCore.setLoadingState(submitButton, false);
            submitButton.querySelector('.btn-text').textContent = originalText;
            submitButton.querySelector('.btn-spinner').style.display = 'none';
        }
    }

    /**
     * Animation functionality
     */
    function initializeAnimations() {
        // Intersection Observer for scroll animations
        const observerOptions = {
            threshold: 0.2,
            rootMargin: '0px 0px -50px 0px'
        };

        state.animationObserver = new IntersectionObserver((entries) => {
            entries.forEach(entry => {
                if (entry.isIntersecting) {
                    // Add animation class
                    entry.target.classList.add('slide-up');

                    // Special handling for different elements
                    if (entry.target.classList.contains('metric-card')) {
                        animateMetricCard(entry.target);
                    }

                    if (entry.target.classList.contains('project-card')) {
                        animateProjectCard(entry.target);
                    }

                    if (entry.target.classList.contains('drive-progress-bar')) {
                        animateDriveProgress(entry.target);
                    }
                }
            });
        }, observerOptions);

        // Observe elements for animation
        const animatedElements = document.querySelectorAll(
            '.metric-card, .feature-card, .project-card, .benefit-card, .drive-progress-bar, .hero-stats-card'
        );

        animatedElements.forEach(el => {
            state.animationObserver.observe(el);
        });
    }

    function animateMetricCard(card) {
        const valueElement = card.querySelector('.metric-value');
        if (!valueElement) return;

        const finalValue = valueElement.textContent;
        const numericValue = parseFloat(finalValue.replace(/[^0-9.]/g, ''));

        if (isNaN(numericValue)) return;

        // Animate the number counting up
        animateNumber(valueElement, 0, numericValue, 2000, finalValue);
    }

    function animateProjectCard(card) {
        const progressFill = card.querySelector('.progress-fill');
        if (!progressFill) return;

        const targetWidth = progressFill.style.width;
        progressFill.style.width = '0%';

        setTimeout(() => {
            progressFill.style.transition = 'width 1.5s ease-out';
            progressFill.style.width = targetWidth;
        }, 300);
    }

    function animateDriveProgress(progressBar) {
        const progressFill = progressBar.querySelector('.progress-fill');
        if (!progressFill) return;

        const targetWidth = progressFill.style.width;
        progressFill.style.width = '0%';

        setTimeout(() => {
            progressFill.style.transition = 'width 2s ease-out';
            progressFill.style.width = targetWidth;
        }, 500);
    }

    function animateNumber(element, start, end, duration, finalText) {
        const startTime = performance.now();
        const range = end - start;

        function updateNumber(currentTime) {
            const elapsed = currentTime - startTime;
            const progress = Math.min(elapsed / duration, 1);

            // Easing function (ease-out)
            const easedProgress = 1 - Math.pow(1 - progress, 3);
            const currentValue = start + (range * easedProgress);

            // Format number based on original format
            if (finalText.includes('')) {
                element.textContent = ' + Math.floor(currentValue).toLocaleString();'
            } else if (finalText.includes('M')) {
            element.textContent = (currentValue / 1000000).toFixed(1) + 'M';
        } else if (finalText.includes('K')) {
            element.textContent = (currentValue / 1000).toFixed(0) + 'K';
        } else if (finalText.includes('+')) {
            element.textContent = Math.floor(currentValue).toLocaleString() + '+';
        } else {
            element.textContent = Math.floor(currentValue).toLocaleString();
        }

        if (progress < 1) {
            requestAnimationFrame(updateNumber);
        } else {
            element.textContent = finalText;
        }
    }

    requestAnimationFrame(updateNumber);
}

    /**
     * Counter animations for statistics
     */
    function initializeCounterAnimations() {
    const statsCards = document.querySelectorAll('.hero-stats-card, .token-stat');

    const statsObserver = new IntersectionObserver((entries) => {
        entries.forEach(entry => {
            if (entry.isIntersecting && !state.statsAnimated) {
                state.statsAnimated = true;
                animateStatsCard(entry.target);
            }
        });
    }, { threshold: 0.5 });

    statsCards.forEach(card => {
        statsObserver.observe(card);
    });
}

function animateStatsCard(card) {
    const statValues = card.querySelectorAll('.stat-value');

    statValues.forEach((valueElement, index) => {
        setTimeout(() => {
            const value = valueElement.textContent;
            const numericValue = parseFloat(value.replace(/[^0-9.]/g, ''));

            if (!isNaN(numericValue)) {
                animateNumber(valueElement, 0, numericValue, 1500, value);
            }
        }, index * 200);
    });
}

/**
 * Hero section interactions
 */
function initializeHeroInteractions() {
    // Floating animation for hero particles
    const heroParticles = document.querySelector('.hero-particles');
    if (heroParticles) {
        // Add mouse parallax effect
        document.addEventListener('mousemove', window.PlatformCore.throttle((e) => {
            const x = (e.clientX / window.innerWidth) * 100;
            const y = (e.clientY / window.innerHeight) * 100;

            heroParticles.style.transform = `translate(${x * 0.05}px, ${y * 0.05}px)`;
        }, 16));
    }

    // Hero CTA button interactions
    const heroCTAs = document.querySelectorAll('.hero-cta, .hero-cta-secondary');
    heroCTAs.forEach(button => {
        button.addEventListener('mouseenter', function () {
            this.style.transform = 'translateY(-3px) scale(1.02)';
        });

        button.addEventListener('mouseleave', function () {
            this.style.transform = 'translateY(0) scale(1)';
        });

        button.addEventListener('click', function () {
            window.PlatformCore.trackEvent('hero_cta_click', {
                buttonText: this.textContent.trim(),
                buttonType: this.classList.contains('hero-cta') ? 'primary' : 'secondary'
            });
        });
    });
}

/**
 * Project carousel functionality
 */
function initializeProjectCarousel() {
    const projectsGrid = document.querySelector('.projects-grid');
    if (!projectsGrid) return;

    // Add hover effects to project cards
    const projectCards = projectsGrid.querySelectorAll('.project-card');
    projectCards.forEach(card => {
        card.addEventListener('mouseenter', function () {
            this.style.transform = 'translateY(-8px) scale(1.02)';

            // Track project card hover
            const projectTitle = this.querySelector('.project-title')?.textContent?.trim();
            window.PlatformCore.trackEvent('project_card_hover', {
                projectTitle: projectTitle
            });
        });

        card.addEventListener('mouseleave', function () {
            this.style.transform = 'translateY(0) scale(1)';
        });

        // Track clicks on project cards
        card.addEventListener('click', function () {
            const projectTitle = this.querySelector('.project-title')?.textContent?.trim();
            const projectCategory = this.querySelector('.project-category')?.textContent?.trim();

            window.PlatformCore.trackEvent('project_card_click', {
                projectTitle: projectTitle,
                projectCategory: projectCategory
            });
        });
    });
}

/**
 * Conditional fields setup
 */
function setupConditionalFields() {
    // Initialize conditional field visibility
    const userTypeSelect = document.getElementById('userType');
    if (userTypeSelect && userTypeSelect.value) {
        handleUserTypeChange({ target: userTypeSelect });
    }
}

/**
 * Utility functions
 */
function getFieldLabel(field) {
    const label = field.closest('.form-group')?.querySelector('label');
    return label ? label.textContent.replace('*', '').trim() : field.name;
}

function formatNumber(num) {
    if (num >= 1000000) {
        return (num / 1000000).toFixed(1) + 'M';
    }
    if (num >= 1000) {
        return (num / 1000).toFixed(0) + 'K';
    }
    return num.toString();
}

/**
 * Public API
 */
return {
    init: init,
    validateField: validateField,
    handleWaitlistSubmit: handleWaitlistSubmit,
    animateNumber: animateNumber
};
}) ();

// Initialize when DOM is ready
document.addEventListener('DOMContentLoaded', function () {
    window.PlatformHome.init();
});

// Handle page visibility changes
document.addEventListener('visibilitychange', function () {
    if (document.hidden) {
        window.PlatformCore.trackEvent('page_hidden', {
            url: window.location.pathname,
            timestamp: new Date().toISOString()
        });
    } else {
        window.PlatformCore.trackEvent('page_visible', {
            url: window.location.pathname,
            timestamp: new Date().toISOString()
        });
    }
});