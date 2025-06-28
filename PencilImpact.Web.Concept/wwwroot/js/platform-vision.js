// PencilImpact.Web.Concept/wwwroot/js/platform-vision.js

/**
 * PencilImpact Vision Page JavaScript
 * Handles vision page animations, interactions, and content display
 */

window.PlatformVision = (function () {
    'use strict';

    // State management
    let state = {
        currentSection: 'vision',
        animationsEnabled: true,
        metricsAnimated: false,
        featuresLoaded: false
    };

    // Vision data
    let visionData = {};

    /**
     * Initialize vision page functionality
     */
    function init() {
        console.log('Initializing Platform Vision...');

        initializeScrollAnimations();
        initializeMetricsAnimations();
        initializeFeatureCarousel();
        initializeTimelineAnimations();
        initializeTokenIntegrationDemo();
        initializeRoadmapInteractions();
        setupVisionPageTracking();

        console.log('Platform Vision initialized successfully');
    }

    /**
     * Initialize scroll-triggered animations
     */
    function initializeScrollAnimations() {
        const observerOptions = {
            threshold: 0.2,
            rootMargin: '0px 0px -100px 0px'
        };

        const observer = new IntersectionObserver((entries) => {
            entries.forEach(entry => {
                if (entry.isIntersecting) {
                    animateElement(entry.target);
                }
            });
        }, observerOptions);

        // Observe elements for animation
        const animateTargets = document.querySelectorAll(
            '.vision-metric, .feature-card, .timeline-item, .roadmap-item, .token-benefit'
        );

        animateTargets.forEach(el => {
            observer.observe(el);
        });
    }

    /**
     * Animate metrics with counting effect
     */
    function initializeMetricsAnimations() {
        const metricsSection = document.querySelector('.vision-metrics');
        if (!metricsSection) return;

        const observer = new IntersectionObserver((entries) => {
            entries.forEach(entry => {
                if (entry.isIntersecting && !state.metricsAnimated) {
                    animateMetrics();
                    state.metricsAnimated = true;
                }
            });
        }, { threshold: 0.5 });

        observer.observe(metricsSection);
    }

    function animateMetrics() {
        const metricCards = document.querySelectorAll('.metric-card');

        metricCards.forEach((card, index) => {
            setTimeout(() => {
                card.classList.add('animate-in');

                const valueElement = card.querySelector('.metric-value');
                if (valueElement) {
                    animateNumber(valueElement);
                }
            }, index * 200);
        });
    }

    function animateNumber(element) {
        const finalValue = element.textContent.replace(/[^0-9.]/g, '');
        const numericValue = parseFloat(finalValue);

        if (isNaN(numericValue)) return;

        const suffix = element.textContent.replace(finalValue, '');
        const duration = 2000;
        const startTime = performance.now();

        function updateNumber(currentTime) {
            const elapsed = currentTime - startTime;
            const progress = Math.min(elapsed / duration, 1);

            // Easing function for smooth animation
            const easeOutQuart = 1 - Math.pow(1 - progress, 4);
            const currentValue = Math.round(numericValue * easeOutQuart);

            element.textContent = currentValue.toLocaleString() + suffix;

            if (progress < 1) {
                requestAnimationFrame(updateNumber);
            }
        }

        requestAnimationFrame(updateNumber);
    }

    /**
     * Feature carousel functionality
     */
    function initializeFeatureCarousel() {
        const carousel = document.querySelector('.features-carousel');
        if (!carousel) return;

        let currentSlide = 0;
        const slides = carousel.querySelectorAll('.feature-slide');
        const totalSlides = slides.length;

        if (totalSlides === 0) return;

        // Create navigation dots
        createCarouselDots(carousel, totalSlides);

        // Auto-play functionality
        setInterval(() => {
            currentSlide = (currentSlide + 1) % totalSlides;
            updateCarousel(carousel, currentSlide);
        }, 5000);

        // Manual navigation
        const prevBtn = carousel.querySelector('.carousel-prev');
        const nextBtn = carousel.querySelector('.carousel-next');

        if (prevBtn) {
            prevBtn.addEventListener('click', () => {
                currentSlide = (currentSlide - 1 + totalSlides) % totalSlides;
                updateCarousel(carousel, currentSlide);
                trackFeatureNavigation('previous', currentSlide);
            });
        }

        if (nextBtn) {
            nextBtn.addEventListener('click', () => {
                currentSlide = (currentSlide + 1) % totalSlides;
                updateCarousel(carousel, currentSlide);
                trackFeatureNavigation('next', currentSlide);
            });
        }
    }

    function createCarouselDots(carousel, count) {
        const dotsContainer = carousel.querySelector('.carousel-dots');
        if (!dotsContainer) return;

        for (let i = 0; i < count; i++) {
            const dot = document.createElement('button');
            dot.className = `carousel-dot ${i === 0 ? 'active' : ''}`;
            dot.setAttribute('data-slide', i);

            dot.addEventListener('click', () => {
                updateCarousel(carousel, i);
                trackFeatureNavigation('dot', i);
            });

            dotsContainer.appendChild(dot);
        }
    }

    function updateCarousel(carousel, slideIndex) {
        const slides = carousel.querySelectorAll('.feature-slide');
        const dots = carousel.querySelectorAll('.carousel-dot');

        slides.forEach((slide, index) => {
            slide.classList.toggle('active', index === slideIndex);
        });

        dots.forEach((dot, index) => {
            dot.classList.toggle('active', index === slideIndex);
        });
    }

    /**
     * Timeline animations
     */
    function initializeTimelineAnimations() {
        const timelineItems = document.querySelectorAll('.timeline-item');

        timelineItems.forEach((item, index) => {
            const observer = new IntersectionObserver((entries) => {
                entries.forEach(entry => {
                    if (entry.isIntersecting) {
                        setTimeout(() => {
                            entry.target.classList.add('timeline-animate');
                        }, index * 150);
                    }
                });
            }, { threshold: 0.3 });

            observer.observe(item);
        });
    }

    /**
     * Token integration demo
     */
    function initializeTokenIntegrationDemo() {
        const demoButtons = document.querySelectorAll('.token-demo-btn');
        const demoContent = document.querySelector('.token-demo-content');

        demoButtons.forEach(button => {
            button.addEventListener('click', function () {
                const demoType = this.getAttribute('data-demo');
                showTokenDemo(demoType);

                // Update active button
                demoButtons.forEach(btn => btn.classList.remove('active'));
                this.classList.add('active');

                trackTokenDemoInteraction(demoType);
            });
        });

        // Initialize first demo
        if (demoButtons.length > 0) {
            demoButtons[0].click();
        }
    }

    function showTokenDemo(demoType) {
        const demoContent = document.querySelector('.token-demo-content');
        if (!demoContent) return;

        let content = '';
        switch (demoType) {
            case 'payment':
                content = generatePaymentDemo();
                break;
            case 'governance':
                content = generateGovernanceDemo();
                break;
            case 'rewards':
                content = generateRewardsDemo();
                break;
            case 'staking':
                content = generateStakingDemo();
                break;
            default:
                content = '<p>Demo content not available</p>';
        }

        demoContent.innerHTML = content;
        demoContent.classList.add('demo-update');

        setTimeout(() => {
            demoContent.classList.remove('demo-update');
        }, 300);
    }

    function generatePaymentDemo() {
        return `
            <div class="demo-payment">
                <h4>TEACH Token Payment Flow</h4>
                <div class="payment-steps">
                    <div class="payment-step active">
                        <span class="step-number">1</span>
                        <span class="step-text">Select project to fund</span>
                    </div>
                    <div class="payment-step">
                        <span class="step-number">2</span>
                        <span class="step-text">Choose payment method: USD or TEACH</span>
                    </div>
                    <div class="payment-step">
                        <span class="step-number">3</span>
                        <span class="step-text">Instant processing with reduced fees</span>
                    </div>
                </div>
                <div class="demo-benefits">
                    <div class="benefit">✓ 50% lower transaction fees</div>
                    <div class="benefit">✓ Instant global payments</div>
                    <div class="benefit">✓ Transparent tracking</div>
                </div>
            </div>
        `;
    }

    function generateGovernanceDemo() {
        return `
            <div class="demo-governance">
                <h4>Community Governance</h4>
                <div class="governance-example">
                    <div class="proposal">
                        <h5>Active Proposal: Pencil Drive Partner Selection</h5>
                        <p>Vote on which supplier should provide pencils for the 2026 drive</p>
                        <div class="voting-options">
                            <div class="vote-option">
                                <span class="option-name">EcoPencils Inc.</span>
                                <div class="vote-bar">
                                    <div class="vote-fill" style="width: 65%"></div>
                                </div>
                                <span class="vote-percentage">65%</span>
                            </div>
                            <div class="vote-option">
                                <span class="option-name">Green Supply Co.</span>
                                <div class="vote-bar">
                                    <div class="vote-fill" style="width: 35%"></div>
                                </div>
                                <span class="vote-percentage">35%</span>
                            </div>
                        </div>
                        <div class="voting-power">Your voting power: 2,500 TEACH tokens</div>
                    </div>
                </div>
            </div>
        `;
    }

    function generateRewardsDemo() {
        return `
            <div class="demo-rewards">
                <h4>Exclusive TEACH Holder Rewards</h4>
                <div class="rewards-grid">
                    <div class="reward-item">
                        <span class="reward-icon">🎖️</span>
                        <span class="reward-text">Special donor badges</span>
                    </div>
                    <div class="reward-item">
                        <span class="reward-icon">🎁</span>
                        <span class="reward-text">Pencil Drive NFTs</span>
                    </div>
                    <div class="reward-item">
                        <span class="reward-icon">⚡</span>
                        <span class="reward-text">Early access to features</span>
                    </div>
                    <div class="reward-item">
                        <span class="reward-icon">📊</span>
                        <span class="reward-text">Enhanced impact reports</span>
                    </div>
                </div>
                <div class="reward-tiers">
                    <h5>Reward Tiers</h5>
                    <div class="tier">Bronze: 100+ TEACH tokens</div>
                    <div class="tier">Silver: 1,000+ TEACH tokens</div>
                    <div class="tier">Gold: 10,000+ TEACH tokens</div>
                </div>
            </div>
        `;
    }

    function generateStakingDemo() {
        return `
            <div class="demo-staking">
                <h4>Stake to Support Schools</h4>
                <div class="staking-example">
                    <div class="stake-info">
                        <h5>Maple Elementary School</h5>
                        <p>Current staked amount: 125,000 TEACH</p>
                        <p>Annual yield: 8% APY</p>
                        <p>Your potential stake: 5,000 TEACH</p>
                        <p>Estimated annual return: 400 TEACH</p>
                    </div>
                    <div class="stake-benefits">
                        <div class="benefit">✓ Support your local school</div>
                        <div class="benefit">✓ Earn staking rewards</div>
                        <div class="benefit">✓ Vote on school funding priorities</div>
                        <div class="benefit">✓ Receive school impact updates</div>
                    </div>
                </div>
            </div>
        `;
    }

    /**
     * Roadmap interactions
     */
    function initializeRoadmapInteractions() {
        const roadmapItems = document.querySelectorAll('.roadmap-item');

        roadmapItems.forEach(item => {
            item.addEventListener('click', function () {
                const phase = this.getAttribute('data-phase');
                showRoadmapDetails(phase, this);
            });
        });
    }

    function showRoadmapDetails(phase, element) {
        // Toggle expanded state
        const isExpanded = element.classList.contains('expanded');

        // Close all other expanded items
        document.querySelectorAll('.roadmap-item.expanded').forEach(item => {
            item.classList.remove('expanded');
        });

        if (!isExpanded) {
            element.classList.add('expanded');
            trackRoadmapInteraction(phase);
        }
    }

    /**
     * Vision page specific tracking
     */
    function setupVisionPageTracking() {
        // Track scroll depth
        let maxScrollDepth = 0;
        window.addEventListener('scroll', () => {
            const scrollDepth = Math.round((window.scrollY / (document.body.scrollHeight - window.innerHeight)) * 100);
            if (scrollDepth > maxScrollDepth) {
                maxScrollDepth = scrollDepth;

                // Track major scroll milestones
                if (scrollDepth >= 25 && scrollDepth < 50) {
                    window.PlatformCore?.trackEvent('vision_scroll_25', { depth: scrollDepth });
                } else if (scrollDepth >= 50 && scrollDepth < 75) {
                    window.PlatformCore?.trackEvent('vision_scroll_50', { depth: scrollDepth });
                } else if (scrollDepth >= 75) {
                    window.PlatformCore?.trackEvent('vision_scroll_75', { depth: scrollDepth });
                }
            }
        });

        // Track time spent on page
        const startTime = Date.now();
        window.addEventListener('beforeunload', () => {
            const timeSpent = Math.round((Date.now() - startTime) / 1000);
            window.PlatformCore?.trackEvent('vision_time_spent', {
                seconds: timeSpent,
                maxScrollDepth: maxScrollDepth
            });
        });
    }

    /**
     * Animation utilities
     */
    function animateElement(element) {
        element.classList.add('animate-in');

        // Add staggered animation for child elements
        const children = element.querySelectorAll('.metric-item, .feature-point, .timeline-point');
        children.forEach((child, index) => {
            setTimeout(() => {
                child.classList.add('animate-in');
            }, index * 100);
        });
    }

    /**
     * Tracking functions
     */
    function trackFeatureNavigation(action, slideIndex) {
        window.PlatformCore?.trackEvent('vision_feature_navigation', {
            action: action,
            slideIndex: slideIndex
        });
    }

    function trackTokenDemoInteraction(demoType) {
        window.PlatformCore?.trackEvent('vision_token_demo', {
            demoType: demoType
        });
    }

    function trackRoadmapInteraction(phase) {
        window.PlatformCore?.trackEvent('vision_roadmap_expand', {
            phase: phase
        });
    }

    /**
     * Public API
     */
    return {
        init: init,
        animateMetrics: animateMetrics,
        showTokenDemo: showTokenDemo,
        trackFeatureNavigation: trackFeatureNavigation
    };
})();

// Initialize when DOM is ready
document.addEventListener('DOMContentLoaded', function () {
    window.PlatformVision.init();
});

// Track page visibility changes specific to vision page
document.addEventListener('visibilitychange', function () {
    const action = document.hidden ? 'vision_page_hidden' : 'vision_page_visible';
    window.PlatformCore?.trackEvent(action, {
        timestamp: new Date().toISOString()
    });
});