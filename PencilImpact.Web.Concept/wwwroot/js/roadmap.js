// PencilImpact - Roadmap Page JavaScript

class RoadmapManager {
    constructor() {
        this.initializeEventListeners();
        this.initializeScrollAnimations();
        this.initializeProgressAnimations();
        this.initializeNewsletterForm();
    }

    initializeEventListeners() {
        // Smooth scrolling for anchor links
        document.querySelectorAll('a[href^="#"]').forEach(anchor => {
            anchor.addEventListener('click', (e) => {
                e.preventDefault();
                const target = document.querySelector(anchor.getAttribute('href'));
                if (target) {
                    this.smoothScrollTo(target);
                }
            });
        });

        // Phase card hover effects
        document.querySelectorAll('.phase-card').forEach(card => {
            card.addEventListener('mouseenter', () => this.onPhaseCardHover(card));
            card.addEventListener('mouseleave', () => this.onPhaseCardLeave(card));
        });

        // Milestone card interactions
        document.querySelectorAll('.milestone-card').forEach(card => {
            card.addEventListener('click', () => this.onMilestoneClick(card));
        });

        // Token phase card interactions
        document.querySelectorAll('.token-phase-card').forEach(card => {
            card.addEventListener('click', () => this.onTokenPhaseClick(card));
        });

        // Social buttons tracking
        document.querySelectorAll('.social-btn').forEach(btn => {
            btn.addEventListener('click', (e) => this.trackSocialClick(e, btn));
        });
    }

    initializeScrollAnimations() {
        // Intersection Observer for scroll animations
        const observerOptions = {
            threshold: 0.1,
            rootMargin: '0px 0px -100px 0px'
        };

        const observer = new IntersectionObserver((entries) => {
            entries.forEach(entry => {
                if (entry.isIntersecting) {
                    this.animateElement(entry.target);
                }
            });
        }, observerOptions);

        // Observe elements for animation
        document.querySelectorAll('.phase-card, .milestone-card, .token-phase-card, .benefit-card').forEach(el => {
            observer.observe(el);
        });

        // Observe progress indicators
        document.querySelectorAll('.progress-fill, .visual-path').forEach(el => {
            observer.observe(el);
        });
    }

    initializeProgressAnimations() {
        // Animate progress bars when they come into view
        const progressObserver = new IntersectionObserver((entries) => {
            entries.forEach(entry => {
                if (entry.isIntersecting) {
                    this.animateProgressBar(entry.target);
                }
            });
        }, { threshold: 0.5 });

        document.querySelectorAll('.progress-fill').forEach(el => {
            progressObserver.observe(el);
        });
    }

    initializeNewsletterForm() {
        const form = document.getElementById('developmentNewsletterForm');
        if (form) {
            form.addEventListener('submit', (e) => this.handleNewsletterSubmit(e));
        }
    }

    // Animation methods
    animateElement(element) {
        element.style.opacity = '1';
        element.style.transform = 'translateY(0)';

        // Add staggered animation for child elements
        const childElements = element.querySelectorAll('.token-feature, .deliverables-list li');
        childElements.forEach((child, index) => {
            setTimeout(() => {
                child.style.opacity = '1';
                child.style.transform = 'translateX(0)';
            }, index * 100);
        });
    }

    animateProgressBar(progressElement) {
        const width = progressElement.style.width;
        progressElement.style.width = '0%';

        setTimeout(() => {
            progressElement.style.transition = 'width 2s ease-out';
            progressElement.style.width = width;
        }, 100);
    }

    // Interaction handlers
    onPhaseCardHover(card) {
        // Add hover glow effect
        card.style.boxShadow = '0 25px 50px rgba(79, 70, 229, 0.2)';

        // Animate deliverables
        const deliverables = card.querySelectorAll('.deliverables-list li');
        deliverables.forEach((item, index) => {
            setTimeout(() => {
                item.style.transform = 'translateX(10px)';
            }, index * 50);
        });
    }

    onPhaseCardLeave(card) {
        card.style.boxShadow = '';

        const deliverables = card.querySelectorAll('.deliverables-list li');
        deliverables.forEach(item => {
            item.style.transform = 'translateX(0)';
        });
    }

    onMilestoneClick(card) {
        // Show milestone details modal or expand
        const title = card.querySelector('.milestone-title').textContent;
        const description = card.querySelector('.milestone-description').textContent;

        this.showMilestoneDetails(title, description);
        this.trackEvent('milestone_click', { milestone: title });
    }

    onTokenPhaseClick(card) {
        // Expand token phase details
        const isExpanded = card.classList.contains('expanded');

        // Close all other expanded cards
        document.querySelectorAll('.token-phase-card.expanded').forEach(c => {
            c.classList.remove('expanded');
        });

        if (!isExpanded) {
            card.classList.add('expanded');
            this.trackEvent('token_phase_expand', {
                phase: card.querySelector('.token-phase-title').textContent
            });
        }
    }

    showMilestoneDetails(title, description) {
        // Create or show milestone detail modal
        let modal = document.getElementById('milestoneModal');

        if (!modal) {
            modal = this.createMilestoneModal();
        }

        modal.querySelector('.modal-title').textContent = title;
        modal.querySelector('.modal-description').textContent = description;
        modal.style.display = 'flex';

        // Close modal on background click
        modal.addEventListener('click', (e) => {
            if (e.target === modal) {
                modal.style.display = 'none';
            }
        });
    }

    createMilestoneModal() {
        const modal = document.createElement('div');
        modal.id = 'milestoneModal';
        modal.className = 'milestone-modal';
        modal.innerHTML = `
            <div class="modal-content">
                <div class="modal-header">
                    <h3 class="modal-title"></h3>
                    <button class="modal-close">&times;</button>
                </div>
                <div class="modal-body">
                    <p class="modal-description"></p>
                </div>
            </div>
        `;

        document.body.appendChild(modal);

        // Close button handler
        modal.querySelector('.modal-close').addEventListener('click', () => {
            modal.style.display = 'none';
        });

        return modal;
    }

    async handleNewsletterSubmit(e) {
        e.preventDefault();

        const form = e.target;
        const email = form.querySelector('input[type="email"]').value;
        const submitBtn = form.querySelector('button[type="submit"]');

        // Show loading state
        const originalText = submitBtn.textContent;
        submitBtn.textContent = 'Subscribing...';
        submitBtn.disabled = true;

        try {
            const response = await fetch('/api/newsletter/subscribe', {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json',
                },
                body: JSON.stringify({
                    email: email,
                    source: 'roadmap_page',
                    type: 'development_updates'
                })
            });

            if (response.ok) {
                this.showSuccessMessage('Thank you! You\'ll receive development updates soon.');
                form.reset();
                this.trackEvent('newsletter_subscribe', { source: 'roadmap' });
            } else {
                throw new Error('Subscription failed');
            }
        } catch (error) {
            console.error('Newsletter subscription error:', error);
            this.showErrorMessage('Something went wrong. Please try again.');
        } finally {
            submitBtn.textContent = originalText;
            submitBtn.disabled = false;
        }
    }

    trackSocialClick(e, button) {
        const platform = button.classList.contains('github') ? 'github' :
            button.classList.contains('discord') ? 'discord' :
                button.classList.contains('twitter') ? 'twitter' : 'unknown';

        this.trackEvent('social_click', { platform: platform, source: 'roadmap' });
    }

    // Utility methods
    smoothScrollTo(target) {
        const targetPosition = target.getBoundingClientRect().top + window.pageYOffset;
        const startPosition = window.pageYOffset;
        const distance = targetPosition - startPosition;
        const duration = 1000;
        let start = null;

        function animation(currentTime) {
            if (start === null) start = currentTime;
            const timeElapsed = currentTime - start;
            const run = this.easeInOutQuad(timeElapsed, startPosition, distance, duration);
            window.scrollTo(0, run);
            if (timeElapsed < duration) requestAnimationFrame(animation.bind(this));
        }

        requestAnimationFrame(animation.bind(this));
    }

    easeInOutQuad(t, b, c, d) {
        t /= d / 2;
        if (t < 1) return c / 2 * t * t + b;
        t--;
        return -c / 2 * (t * (t - 2) - 1) + b;
    }

    showSuccessMessage(message) {
        this.showNotification(message, 'success');
    }

    showErrorMessage(message) {
        this.showNotification(message, 'error');
    }

    showNotification(message, type = 'info') {
        // Create notification element
        const notification = document.createElement('div');
        notification.className = `notification notification-${type}`;
        notification.textContent = message;

        // Style the notification
        Object.assign(notification.style, {
            position: 'fixed',
            top: '20px',
            right: '20px',
            padding: '1rem 2rem',
            borderRadius: '0.5rem',
            color: 'white',
            fontWeight: '500',
            zIndex: '10000',
            transform: 'translateX(100%)',
            transition: 'transform 0.3s ease'
        });

        if (type === 'success') {
            notification.style.background = 'var(--success-color)';
        } else if (type === 'error') {
            notification.style.background = 'var(--danger-color)';
        } else {
            notification.style.background = 'var(--primary-color)';
        }

        document.body.appendChild(notification);

        // Animate in
        setTimeout(() => {
            notification.style.transform = 'translateX(0)';
        }, 100);

        // Remove after delay
        setTimeout(() => {
            notification.style.transform = 'translateX(100%)';
            setTimeout(() => {
                document.body.removeChild(notification);
            }, 300);
        }, 3000);
    }

    trackEvent(eventName, data = {}) {
        // Analytics tracking
        if (typeof gtag !== 'undefined') {
            gtag('event', eventName, {
                page_location: window.location.href,
                page_title: document.title,
                ...data
            });
        }

        // Custom analytics endpoint
        fetch('/api/analytics/track', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
            },
            body: JSON.stringify({
                event: eventName,
                data: data,
                url: window.location.href,
                timestamp: new Date().toISOString()
            })
        }).catch(error => {
            console.log('Analytics tracking failed:', error);
        });
    }

    // Update progress indicators based on real data
    updateProgress(phaseData) {
        if (phaseData && phaseData.completionPercentage) {
            const progressBars = document.querySelectorAll('.progress-fill');
            progressBars.forEach(bar => {
                bar.style.width = `${phaseData.completionPercentage}%`;
            });

            const progressTexts = document.querySelectorAll('.progress-text');
            progressTexts.forEach(text => {
                text.textContent = `${phaseData.completionPercentage}% Complete`;
            });
        }
    }

    // Load dynamic roadmap data
    async loadRoadmapData() {
        try {
            const response = await fetch('/api/roadmap/current-status');
            if (response.ok) {
                const data = await response.json();
                this.updateProgress(data);
                this.updateMilestoneStatuses(data.milestones);
            }
        } catch (error) {
            console.log('Failed to load roadmap data:', error);
        }
    }

    updateMilestoneStatuses(milestones) {
        if (!milestones) return;

        milestones.forEach(milestone => {
            const card = document.querySelector(`[data-milestone-id="${milestone.id}"]`);
            if (card) {
                const statusElement = card.querySelector('.milestone-status');
                if (statusElement) {
                    statusElement.textContent = milestone.status;
                    statusElement.className = `milestone-status status-${milestone.status.toLowerCase().replace(' ', '-')}`;
                }
            }
        });
    }
}

// CSS for modal and animations
const roadmapStyles = `
.milestone-modal {
    position: fixed;
    top: 0;
    left: 0;
    width: 100%;
    height: 100%;
    background: rgba(0, 0, 0, 0.8);
    display: none;
    align-items: center;
    justify-content: center;
    z-index: 10000;
}

.modal-content {
    background: var(--card-bg);
    border-radius: 1rem;
    border: 1px solid rgba(255, 255, 255, 0.1);
    max-width: 500px;
    width: 90%;
    max-height: 80vh;
    overflow-y: auto;
}

.modal-header {
    display: flex;
    justify-content: space-between;
    align-items: center;
    padding: 2rem 2rem 1rem;
    border-bottom: 1px solid rgba(255, 255, 255, 0.1);
}

.modal-title {
    margin: 0;
    color: var(--text-primary);
    font-size: 1.5rem;
    font-weight: 700;
}

.modal-close {
    background: none;
    border: none;
    color: var(--text-secondary);
    font-size: 2rem;
    cursor: pointer;
    padding: 0;
    width: 30px;
    height: 30px;
    display: flex;
    align-items: center;
    justify-content: center;
    border-radius: 50%;
    transition: var(--transition-smooth);
}

.modal-close:hover {
    background: rgba(255, 255, 255, 0.1);
    color: var(--text-primary);
}

.modal-body {
    padding: 1rem 2rem 2rem;
}

.modal-description {
    color: var(--text-secondary);
    line-height: 1.6;
    margin: 0;
}

.token-phase-card.expanded {
    transform: scale(1.02);
    border-color: var(--primary-color);
    box-shadow: 0 20px 40px rgba(79, 70, 229, 0.3);
}

.token-feature {
    opacity: 0;
    transform: translateX(-20px);
    transition: all 0.3s ease;
}

.token-phase-card.expanded .token-feature {
    opacity: 1;
    transform: translateX(0);
}

.deliverables-list li {
    opacity: 0;
    transform: translateX(-10px);
    transition: all 0.3s ease;
}

.phase-card:hover .deliverables-list li {
    opacity: 1;
    transform: translateX(0);
}
`;

// Inject styles
const styleSheet = document.createElement('style');
styleSheet.textContent = roadmapStyles;
document.head.appendChild(styleSheet);

// Initialize when DOM is loaded
document.addEventListener('DOMContentLoaded', () => {
    const roadmapManager = new RoadmapManager();

    // Load dynamic data
    roadmapManager.loadRoadmapData();

    // Track page view
    roadmapManager.trackEvent('page_view', { page: 'roadmap' });
});

// Export for potential external use
window.RoadmapManager = RoadmapManager;