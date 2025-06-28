// PencilImpact.Web.Concept/wwwroot/js/platform-demo.js

/**
 * PencilImpact Demo Page JavaScript
 * Handles interactive demo functionality and simulations
 */

window.PlatformDemo = (function () {
    'use strict';

    // State management
    let state = {
        currentDemo: 'browse',
        selectedProject: null,
        donationAmount: 100,
        filteredProjects: [],
        allProjects: [],
        tokenAmount: 125
    };

    // Demo data
    let demoData = {};

    /**
     * Initialize demo functionality
     */
    function init() {
        // Load demo data from server
        if (window.DemoData) {
            demoData = window.DemoData;
            state.allProjects = demoData.demoProjects || [];
            state.filteredProjects = [...state.allProjects];
        }

        initializeDemoNavigation();
        initializeProjectFilters();
        initializeDonationCalculator();
        initializeTokenDonationDemo();
        initializeProjectInteractions();
        setupDemoAnimations();

        console.log('PlatformDemo initialized');
    }

    /**
     * Demo navigation between sections
     */
    function initializeDemoNavigation() {
        const navButtons = document.querySelectorAll('.demo-nav-btn');
        const demoPanels = document.querySelectorAll('.demo-panel');

        navButtons.forEach(button => {
            button.addEventListener('click', function () {
                const demoType = this.getAttribute('data-demo');
                switchToDemo(demoType);

                // Track demo navigation
                window.PlatformCore.trackEvent('demo_navigation', {
                    fromDemo: state.currentDemo,
                    toDemo: demoType,
                    timestamp: new Date().toISOString()
                });
            });
        });

        // Initialize first demo
        switchToDemo('browse');
    }

    function switchToDemo(demoType) {
        // Update navigation
        document.querySelectorAll('.demo-nav-btn').forEach(btn => {
            btn.classList.remove('active');
        });
        document.querySelector(`[data-demo="${demoType}"]`).classList.add('active');

        // Update panels
        document.querySelectorAll('.demo-panel').forEach(panel => {
            panel.classList.remove('active');
        });

        const targetPanel = document.getElementById(`${demoType}Demo`);
        if (targetPanel) {
            targetPanel.classList.add('active');

            // Trigger animations for the new panel
            setTimeout(() => {
                animatePanelContent(targetPanel);
            }, 100);
        }

        state.currentDemo = demoType;

        // Update URL hash without scrolling
        const currentHash = window.location.hash;
        const newHash = `#demo-${demoType}`;
        if (currentHash !== newHash) {
            history.replaceState(null, null, newHash);
        }
    }

    /**
     * Project filtering functionality
     */
    function initializeProjectFilters() {
        const categoryFilter = document.getElementById('categoryFilter');
        const stateFilter = document.getElementById('stateFilter');
        const urgencyFilter = document.getElementById('urgencyFilter');

        if (categoryFilter) {
            categoryFilter.addEventListener('change', applyFilters);
        }
        if (stateFilter) {
            stateFilter.addEventListener('change', applyFilters);
        }
        if (urgencyFilter) {
            urgencyFilter.addEventListener('change', applyFilters);
        }

        // Initial filter application
        applyFilters();
    }

    function applyFilters() {
        const categoryFilter = document.getElementById('categoryFilter')?.value;
        const stateFilter = document.getElementById('stateFilter')?.value;
        const urgencyFilter = document.getElementById('urgencyFilter')?.value;

        const projectCards = document.querySelectorAll('.demo-project-card');
        let visibleCount = 0;

        projectCards.forEach(card => {
            let shouldShow = true;

            // Category filter
            if (categoryFilter && card.getAttribute('data-category') !== categoryFilter) {
                shouldShow = false;
            }

            // State filter
            if (stateFilter && card.getAttribute('data-state') !== stateFilter) {
                shouldShow = false;
            }

            // Urgency filter
            if (urgencyFilter) {
                if (urgencyFilter === 'urgent' && card.getAttribute('data-urgent') !== 'true') {
                    shouldShow = false;
                } else if (urgencyFilter === 'featured' && card.getAttribute('data-featured') !== 'true') {
                    shouldShow = false;
                }
            }

            // Show/hide card with animation
            if (shouldShow) {
                card.style.display = 'block';
                card.classList.add('fade-in');
                visibleCount++;
            } else {
                card.style.display = 'none';
                card.classList.remove('fade-in');
            }
        });

        // Track filtering activity
        window.PlatformCore.trackEvent('demo_projects_filtered', {
            category: categoryFilter || 'all',
            state: stateFilter || 'all',
            urgency: urgencyFilter || 'all',
            visibleProjects: visibleCount
        });

        // Update filtered projects state
        updateFilteredProjectsState();
    }

    function updateFilteredProjectsState() {
        const visibleCards = document.querySelectorAll('.demo-project-card[style*="block"], .demo-project-card:not([style*="none"])');
        state.filteredProjects = Array.from(visibleCards).map(card => {
            return state.allProjects.find(p => p.Id === card.getAttribute('data-project-id'));
        }).filter(Boolean);
    }

    /**
     * Project interaction handlers
     */
    function initializeProjectInteractions() {
        const projectCards = document.querySelectorAll('.demo-project-card');
        const donateButtons = document.querySelectorAll('.demo-donate-btn');

        // Project card hover effects
        projectCards.forEach(card => {
            card.addEventListener('mouseenter', function () {
                this.style.transform = 'translateY(-5px) scale(1.02)';
                this.style.boxShadow = '0 15px 35px rgba(0, 0, 0, 0.3)';
            });

            card.addEventListener('mouseleave', function () {
                this.style.transform = 'translateY(0) scale(1)';
                this.style.boxShadow = '';
            });

            card.addEventListener('click', function (e) {
                if (!e.target.closest('.demo-donate-btn')) {
                    selectProject(this);
                }
            });
        });

        // Donate button handlers
        donateButtons.forEach(button => {
            button.addEventListener('click', function (e) {
                e.stopPropagation();
                const projectCard = this.closest('.demo-project-card');
                selectProject(projectCard);
                switchToDemo('donate');
            });
        });
    }

    function selectProject(projectCard) {
        // Remove previous selection
        document.querySelectorAll('.demo-project-card').forEach(card => {
            card.classList.remove('selected');
        });

        // Add selection to clicked card
        projectCard.classList.add('selected');

        // Extract project data
        const projectData = {
            title: projectCard.querySelector('.demo-project-title').textContent,
            description: projectCard.querySelector('.demo-project-description').textContent,
            category: projectCard.querySelector('.project-category').textContent,
            location: projectCard.querySelector('.project-location').textContent.replace('📍 ', ''),
            teacher: projectCard.querySelector('.project-teacher').textContent.replace('👩‍🏫 ', ''),
            students: parseInt(projectCard.querySelector('.project-students').textContent.match(/\d+/)[0]),
            goal: parseFloat(projectCard.querySelector('.funding-stats').textContent.match(/\$[\d,]+/g)[1].replace(/[$,]/g, '')),
            current: parseFloat(projectCard.querySelector('.funding-stats').textContent.match(/\$[\d,]+/g)[0].replace(/[$,]/g, ''))
        };

        state.selectedProject = projectData;
        updateSelectedProjectDisplay();
        updateImpactCalculation();

        // Track project selection
        window.PlatformCore.trackEvent('demo_project_selected', {
            projectTitle: projectData.title,
            projectCategory: projectData.category,
            projectLocation: projectData.location
        });
    }

    function updateSelectedProjectDisplay() {
        const selectedProjectDiv = document.getElementById('selectedProject');
        if (!selectedProjectDiv || !state.selectedProject) return;

        selectedProjectDiv.innerHTML = `
            <div class="selected-project-content">
                <div class="project-header">
                    <div class="project-category">${state.selectedProject.category}</div>
                    <h4>${state.selectedProject.title}</h4>
                </div>
                <p class="project-description">${state.selectedProject.description}</p>
                <div class="project-details">
                    <div class="detail-item">
                        <span class="detail-icon">📍</span>
                        <span>${state.selectedProject.location}</span>
                    </div>
                    <div class="detail-item">
                        <span class="detail-icon">👩‍🏫</span>
                        <span>${state.selectedProject.teacher}</span>
                    </div>
                    <div class="detail-item">
                        <span class="detail-icon">🎓</span>
                        <span>${state.selectedProject.students} students</span>
                    </div>
                </div>
                <div class="project-funding-status">
                    <div class="funding-goal">Goal: $${state.selectedProject.goal.toLocaleString()}</div>
                    <div class="funding-current">Current: $${state.selectedProject.current.toLocaleString()}</div>
                </div>
            </div>
        `;

        selectedProjectDiv.classList.add('has-project');
    }

    /**
     * Donation calculator functionality
     */
    function initializeDonationCalculator() {
        const amountButtons = document.querySelectorAll('.amount-btn');
        const customAmountInput = document.getElementById('customAmount');

        // Amount button handlers
        amountButtons.forEach(button => {
            button.addEventListener('click', function () {
                const amount = parseFloat(this.getAttribute('data-amount'));
                setDonationAmount(amount);

                // Update button states
                amountButtons.forEach(btn => btn.classList.remove('active'));
                this.classList.add('active');

                // Clear custom input
                if (customAmountInput) {
                    customAmountInput.value = '';
                }
            });
        });

        // Custom amount input handler
        if (customAmountInput) {
            customAmountInput.addEventListener('input', function () {
                const amount = parseFloat(this.value);
                if (amount > 0) {
                    setDonationAmount(amount);
                    // Remove active state from preset buttons
                    amountButtons.forEach(btn => btn.classList.remove('active'));
                }
            });
        }

        // Initialize with default amount
        updateImpactCalculation();
    }

    function setDonationAmount(amount) {
        state.donationAmount = amount;
        updateImpactCalculation();

        // Track donation amount changes
        window.PlatformCore.trackEvent('demo_donation_amount_changed', {
            amount: amount,
            hasSelectedProject: !!state.selectedProject
        });
    }

    function updateImpactCalculation() {
        if (!state.selectedProject) return;

        const amount = state.donationAmount;
        const project = state.selectedProject;

        // Calculate impact metrics
        const fundingPercentage = ((amount / project.goal) * 100).toFixed(1);
        const studentsHelped = project.students;
        const platformFee = (amount * 0.05).toFixed(2); // 5% fee for traditional payment

        // Update display
        const fundingPercentageEl = document.getElementById('fundingPercentage');
        const studentsHelpedEl = document.getElementById('studentsHelped');
        const platformFeeEl = document.getElementById('platformFee');

        if (fundingPercentageEl) fundingPercentageEl.textContent = `${fundingPercentage}%`;
        if (studentsHelpedEl) studentsHelpedEl.textContent = studentsHelped;
        if (platformFeeEl) platformFeeEl.textContent = `$${platformFee}`;

        // Animate the changes
        [fundingPercentageEl, studentsHelpedEl, platformFeeEl].forEach(el => {
            if (el) {
                el.classList.add('updated');
                setTimeout(() => el.classList.remove('updated'), 500);
            }
        });
    }

    /**
     * Token donation demo functionality
     */
    function initializeTokenDonationDemo() {
        const tokenAmountInput = document.getElementById('tokenAmount');
        const simulateButton = document.getElementById('simulateDonation');

        if (tokenAmountInput) {
            tokenAmountInput.addEventListener('input', function () {
                const amount = parseFloat(this.value) || 0;
                state.tokenAmount = amount;
                updatePencilCalculation();
            });
        }

        if (simulateButton) {
            simulateButton.addEventListener('click', simulateTokenDonation);
        }

        // Initialize calculation
        updatePencilCalculation();
    }

    function updatePencilCalculation() {
        const tokenAmount = state.tokenAmount;
        const tokenValue = tokenAmount * 0.001; // Assuming $0.001 per token
        const pencilCount = Math.floor(tokenValue / 10 * 125); // $10 = 125 pencils

        // Update displays
        const pencilCountEl = document.getElementById('pencilCount');
        const tokenValueEl = document.querySelector('.token-value');

        if (pencilCountEl) {
            pencilCountEl.textContent = pencilCount;
            // Animate number change
            pencilCountEl.classList.add('updated');
            setTimeout(() => pencilCountEl.classList.remove('updated'), 500);
        }

        if (tokenValueEl) {
            tokenValueEl.textContent = `≈ $${tokenValue.toFixed(2)} USD`;
        }

        // Update pencil visual
        updatePencilVisual(pencilCount);
    }

    function updatePencilVisual(count) {
        const pencilVisual = document.querySelector('.pencil-visual');
        if (!pencilVisual) return;

        let pencilEmojis = '';
        const displayCount = Math.min(count, 10); // Show max 10 pencil emojis

        for (let i = 0; i < displayCount; i++) {
            pencilEmojis += '✏️';
        }

        if (count > 10) {
            pencilEmojis += ` +${count - 10}`;
        }

        pencilVisual.textContent = pencilEmojis;
    }

    function simulateTokenDonation() {
        const tokenAmount = state.tokenAmount;
        const pencilCount = Math.floor(tokenAmount * 0.001 / 10 * 125);

        // Show donation simulation
        showDonationSimulation(tokenAmount, pencilCount);

        // Track simulation
        window.PlatformCore.trackEvent('demo_token_donation_simulated', {
            tokenAmount: tokenAmount,
            pencilCount: pencilCount,
            dollarValue: (tokenAmount * 0.001).toFixed(2)
        });
    }

    function showDonationSimulation(tokens, pencils) {
        // Create simulation overlay
        const overlay = document.createElement('div');
        overlay.className = 'donation-simulation-overlay';
        overlay.innerHTML = `
            <div class="simulation-modal">
                <div class="simulation-header">
                    <h3>🎉 Donation Simulated!</h3>
                    <button class="close-simulation">&times;</button>
                </div>
                <div class="simulation-content">
                    <div class="simulation-step">
                        <div class="step-icon">🪙</div>
                        <div class="step-text">
                            <strong>${tokens} TEACH Tokens</strong>
                            <p>Transferred to platform treasury</p>
                        </div>
                    </div>
                    <div class="simulation-arrow">↓</div>
                    <div class="simulation-step">
                        <div class="step-icon">✏️</div>
                        <div class="step-text">
                            <strong>${pencils} Pencils</strong>
                            <p>Will be distributed to schools in need</p>
                        </div>
                    </div>
                    <div class="simulation-arrow">↓</div>
                    <div class="simulation-step">
                        <div class="step-icon">🏆</div>
                        <div class="step-text">
                            <strong>NFT Reward Earned!</strong>
                            <p>Pencil Drive Contributor Badge minted to your wallet</p>
                        </div>
                    </div>
                </div>
                <div class="simulation-footer">
                    <p>This is a demonstration. In the real platform, your tokens would be processed and pencils distributed to verified schools.</p>
                    <button class="btn btn-primary close-simulation">Try Again</button>
                </div>
            </div>
        `;

        document.body.appendChild(overlay);

        // Show with animation
        setTimeout(() => overlay.classList.add('show'), 10);

        // Close handlers
        overlay.querySelectorAll('.close-simulation').forEach(btn => {
            btn.addEventListener('click', () => {
                overlay.classList.remove('show');
                setTimeout(() => overlay.remove(), 300);
            });
        });

        // Close on background click
        overlay.addEventListener('click', (e) => {
            if (e.target === overlay) {
                overlay.classList.remove('show');
                setTimeout(() => overlay.remove(), 300);
            }
        });
    }

    /**
     * Animation helpers
     */
    function setupDemoAnimations() {
        // Intersection observer for demo elements
        const observer = new IntersectionObserver((entries) => {
            entries.forEach(entry => {
                if (entry.isIntersecting) {
                    entry.target.classList.add('animate-in');
                }
            });
        }, { threshold: 0.2 });

        // Observe demo elements
        document.querySelectorAll('.demo-step, .comparison-card, .dashboard-stat').forEach(el => {
            observer.observe(el);
        });
    }

    function animatePanelContent(panel) {
        const animatableElements = panel.querySelectorAll('.demo-project-card, .comparison-card, .dashboard-stat, .update-item');

        animatableElements.forEach((el, index) => {
            setTimeout(() => {
                el.classList.add('slide-up');
            }, index * 100);
        });
    }

    /**
     * Public API
     */
    return {
        init: init,
        switchToDemo: switchToDemo,
        selectProject: selectProject,
        setDonationAmount: setDonationAmount,
        simulateTokenDonation: simulateTokenDonation
    };
})();

// Initialize when DOM is ready
document.addEventListener('DOMContentLoaded', function () {
    window.PlatformDemo.init();

    // Handle URL hash on load
    const hash = window.location.hash;
    if (hash && hash.startsWith('#demo-')) {
        const demoType = hash.replace('#demo-', '');
        setTimeout(() => {
            window.PlatformDemo.switchToDemo(demoType);
        }, 500);
    }
});