// TeachToken Roadmap Page JavaScript
// Follows established patterns from site.js

class RoadmapDashboard {
    constructor() {
        this.init();
        this.bindEvents();
        this.startProgressAnimations();
        this.initCharts();
    }

    init() {
        // Initialize components
        this.modal = document.getElementById('milestoneModal');
        this.modalBackdrop = document.getElementById('modalBackdrop');
        this.modalClose = document.getElementById('modalClose');
        this.modalTitle = document.getElementById('modalTitle');
        this.modalBody = document.getElementById('modalBody');

        // Cache DOM elements
        this.progressBars = document.querySelectorAll('.progress-fill[data-progress]');
        this.milestoneCards = document.querySelectorAll('.milestone-card[data-milestone-id]');
        this.upcomingCards = document.querySelectorAll('.upcoming-card[data-milestone-id]');
        this.showMoreBtn = document.getElementById('showMoreCompleted');

        // State
        this.isModalOpen = false;
        this.currentMilestoneId = null;
        this.charts = {};

        console.log('Roadmap Dashboard initialized');
    }

    bindEvents() {
        // Modal events
        if (this.modalClose) {
            this.modalClose.addEventListener('click', () => this.closeModal());
        }

        if (this.modalBackdrop) {
            this.modalBackdrop.addEventListener('click', () => this.closeModal());
        }

        // Escape key to close modal
        document.addEventListener('keydown', (e) => {
            if (e.key === 'Escape' && this.isModalOpen) {
                this.closeModal();
            }
        });

        // Milestone card clicks
        this.milestoneCards.forEach(card => {
            const detailsBtn = card.querySelector('.milestone-details-btn');
            if (detailsBtn) {
                detailsBtn.addEventListener('click', (e) => {
                    e.preventDefault();
                    e.stopPropagation();
                    const milestoneId = card.dataset.milestoneId;
                    this.showMilestoneDetails(milestoneId);
                });
            }

            // Make whole card clickable
            card.addEventListener('click', () => {
                const milestoneId = card.dataset.milestoneId;
                this.showMilestoneDetails(milestoneId);
            });
        });

        // Upcoming milestone card clicks
        this.upcomingCards.forEach(card => {
            card.addEventListener('click', () => {
                const milestoneId = card.dataset.milestoneId;
                this.showMilestoneDetails(milestoneId);
            });
        });

        // Show more completed milestones
        if (this.showMoreBtn) {
            this.showMoreBtn.addEventListener('click', () => {
                this.loadMoreCompletedMilestones();
            });
        }

        // Navbar scroll effect (following site.js pattern)
        this.initNavbarScroll();

        // Auto-refresh data every 5 minutes
        this.startAutoRefresh();
    }

    initNavbarScroll() {
        const navbar = document.getElementById('navbar');
        if (!navbar) return;

        let lastScrollY = window.scrollY;

        window.addEventListener('scroll', () => {
            const currentScrollY = window.scrollY;

            if (currentScrollY > 100) {
                navbar.classList.add('scrolled');
            } else {
                navbar.classList.remove('scrolled');
            }

            lastScrollY = currentScrollY;
        });
    }

    startProgressAnimations() {
        // Animate progress bars when they come into view
        const observerOptions = {
            threshold: 0.5,
            rootMargin: '0px 0px -50px 0px'
        };

        const observer = new IntersectionObserver((entries) => {
            entries.forEach(entry => {
                if (entry.isIntersecting) {
                    this.animateProgressBar(entry.target);
                    observer.unobserve(entry.target);
                }
            });
        }, observerOptions);

        this.progressBars.forEach(bar => {
            observer.observe(bar);
        });
    }

    animateProgressBar(progressBar) {
        const targetProgress = parseFloat(progressBar.dataset.progress);
        let currentProgress = 0;
        const duration = 1500; // 1.5 seconds
        const startTime = performance.now();

        const animate = (currentTime) => {
            const elapsed = currentTime - startTime;
            const progress = Math.min(elapsed / duration, 1);

            // Easing function for smooth animation
            const easeOutCubic = 1 - Math.pow(1 - progress, 3);
            currentProgress = targetProgress * easeOutCubic;

            progressBar.style.width = `${currentProgress}%`;

            if (progress < 1) {
                requestAnimationFrame(animate);
            }
        };

        requestAnimationFrame(animate);
    }

    async showMilestoneDetails(milestoneId) {
        if (!milestoneId) return;

        this.currentMilestoneId = milestoneId;
        this.openModal();
        this.showLoadingState();

        try {
            const response = await fetch(`/roadmap/api/milestone/${milestoneId}`, {
                method: 'GET',
                headers: {
                    'Content-Type': 'application/json',
                    'Cache-Control': 'max-age=180' // 3 minutes cache
                }
            });

            if (!response.ok) {
                throw new Error(`HTTP ${response.status}: ${response.statusText}`);
            }

            const data = await response.json();

            if (data.success && data.data) {
                this.renderMilestoneDetails(data.data);
            } else {
                throw new Error(data.message || 'Failed to load milestone details');
            }
        } catch (error) {
            console.error('Error loading milestone details:', error);
            this.showErrorState(error.message);
        }
    }

    renderMilestoneDetails(milestone) {
        this.modalTitle.textContent = milestone.title;

        const detailsHtml = `
            <div class="milestone-details">
                <!-- Header Section -->
                <div class="milestone-detail-header">
                    <div class="milestone-meta">
                        <span class="category-badge">
                            <span class="category-icon">${milestone.categoryIcon}</span>
                            ${milestone.category}
                        </span>
                        <span class="priority-badge ${milestone.priorityClass}">${milestone.priority}</span>
                        <span class="status-badge ${milestone.statusClass}">${milestone.status}</span>
                    </div>
                    <div class="milestone-progress-summary">
                        <div class="progress-circle" data-progress="${milestone.progressPercentage}">
                            <div class="progress-text">${milestone.progressPercentage}%</div>
                        </div>
                    </div>
                </div>

                <!-- Description -->
                <div class="milestone-description">
                    <h4>Description</h4>
                    <p>${milestone.description}</p>
                </div>

                <!-- Timeline -->
                <div class="milestone-timeline">
                    <div class="timeline-grid">
                        <div class="timeline-item">
                            <span class="timeline-label">Started</span>
                            <span class="timeline-value">${milestone.startDateFormatted}</span>
                        </div>
                        <div class="timeline-item">
                            <span class="timeline-label">Target Completion</span>
                            <span class="timeline-value">${milestone.estimatedCompletionFormatted}</span>
                        </div>
                        ${milestone.actualCompletionFormatted ? `
                        <div class="timeline-item">
                            <span class="timeline-label">Actual Completion</span>
                            <span class="timeline-value">${milestone.actualCompletionFormatted}</span>
                        </div>
                        ` : ''}
                        <div class="timeline-item">
                            <span class="timeline-label">Duration</span>
                            <span class="timeline-value">${milestone.durationEstimate || 'TBD'}</span>
                        </div>
                        <div class="timeline-item">
                            <span class="timeline-label">Time Remaining</span>
                            <span class="timeline-value ${milestone.isOverdue ? 'overdue' : ''}">${milestone.timeRemaining}</span>
                        </div>
                        <div class="timeline-item">
                            <span class="timeline-label">Progress</span>
                            <span class="timeline-value">${milestone.progressText}</span>
                        </div>
                    </div>
                </div>

                <!-- Tasks Section -->
                ${milestone.tasks && milestone.tasks.length > 0 ? `
                <div class="milestone-tasks">
                    <h4>Tasks (${milestone.tasks.length})</h4>
                    <div class="tasks-list">
                        ${milestone.tasks.map(task => `
                            <div class="task-item ${task.statusClass}">
                                <div class="task-header">
                                    <div class="task-info">
                                        <h5 class="task-title">${task.title}</h5>
                                        <div class="task-meta">
                                            <span class="task-assignee">${task.assigneeDisplay}</span>
                                            ${task.dueDateFormatted ? `<span class="task-due">Due: ${task.dueDateFormatted}</span>` : ''}
                                            ${task.timeTrackingText ? `<span class="task-time">${task.timeTrackingText}</span>` : ''}
                                        </div>
                                    </div>
                                    <div class="task-badges">
                                        <span class="priority-badge ${task.priorityClass}">${task.priority}</span>
                                        <span class="status-badge ${task.statusClass}">${task.status}</span>
                                    </div>
                                </div>
                                <div class="task-progress">
                                    <div class="progress-bar small">
                                        <div class="progress-fill" style="width: ${task.progressPercentage}%"></div>
                                    </div>
                                    <span class="progress-percentage">${task.progressPercentage}%</span>
                                </div>
                            </div>
                        `).join('')}
                    </div>
                </div>
                ` : ''}

                <!-- Updates Section -->
                ${milestone.updates && milestone.updates.length > 0 ? `
                <div class="milestone-updates">
                    <h4>Recent Updates (${milestone.updates.length})</h4>
                    <div class="updates-list">
                        ${milestone.updates.map(update => `
                            <div class="update-item">
                                <div class="update-header">
                                    <div class="update-info">
                                        <span class="update-icon">${update.updateTypeIcon}</span>
                                        <h5 class="update-title">${update.title}</h5>
                                    </div>
                                    <div class="update-meta">
                                        <span class="update-author">${update.authorDisplay}</span>
                                        <span class="update-date">${update.createdAtFormatted}</span>
                                    </div>
                                </div>
                                <div class="update-content">
                                    <p>${update.contentPreview}</p>
                                    ${update.hasTags ? `
                                    <div class="update-tags">
                                        ${update.tags ? update.tags.map(tag => `<span class="tag">${tag}</span>`).join('') : ''}
                                    </div>
                                    ` : ''}
                                </div>
                            </div>
                        `).join('')}
                    </div>
                </div>
                ` : ''}

                <!-- Dependencies Section -->
                ${milestone.dependencies && milestone.dependencies.length > 0 ? `
                <div class="milestone-dependencies">
                    <h4>Dependencies (${milestone.dependencies.length})</h4>
                    <div class="dependencies-list">
                        ${milestone.dependencies.map(dep => `
                            <div class="dependency-item ${dep.isActive ? 'active' : 'inactive'}">
                                <span class="dependency-icon">${dep.dependencyTypeIcon}</span>
                                <div class="dependency-content">
                                    <div class="dependency-text">${dep.dependencyText}</div>
                                    ${dep.description ? `<div class="dependency-description">${dep.description}</div>` : ''}
                                </div>
                                <span class="dependency-type">${dep.dependencyType}</span>
                            </div>
                        `).join('')}
                    </div>
                </div>
                ` : ''}
            </div>
        `;

        this.modalBody.innerHTML = detailsHtml;

        // Animate progress circles
        setTimeout(() => {
            this.animateProgressCircles();
        }, 100);
    }

    animateProgressCircles() {
        const progressCircles = this.modalBody.querySelectorAll('.progress-circle[data-progress]');

        progressCircles.forEach(circle => {
            const progress = parseFloat(circle.dataset.progress);
            const circumference = 2 * Math.PI * 45; // radius = 45
            const offset = circumference - (progress / 100) * circumference;

            // Create SVG if it doesn't exist
            if (!circle.querySelector('svg')) {
                circle.innerHTML = `
                    <svg width="100" height="100" class="progress-svg">
                        <circle cx="50" cy="50" r="45" fill="none" stroke="rgba(255,255,255,0.1)" stroke-width="8"/>
                        <circle cx="50" cy="50" r="45" fill="none" stroke="var(--roadmap-primary)" stroke-width="8" 
                                stroke-linecap="round" stroke-dasharray="${circumference}" 
                                stroke-dashoffset="${circumference}" class="progress-circle-fill"/>
                    </svg>
                    <div class="progress-text">${progress}%</div>
                `;
            }

            const progressFill = circle.querySelector('.progress-circle-fill');
            if (progressFill) {
                setTimeout(() => {
                    progressFill.style.strokeDashoffset = offset;
                    progressFill.style.transition = 'stroke-dashoffset 1.5s ease-out';
                }, 100);
            }
        });
    }

    showLoadingState() {
        this.modalBody.innerHTML = `
            <div class="loading-spinner">
                <div class="spinner"></div>
                <span>Loading milestone details...</span>
            </div>
        `;
    }

    showErrorState(message) {
        this.modalBody.innerHTML = `
            <div class="error-state">
                <div class="error-icon">⚠️</div>
                <h4>Failed to Load Details</h4>
                <p>${message}</p>
                <button class="retry-btn" onclick="roadmapDashboard.showMilestoneDetails(${this.currentMilestoneId})">
                    Try Again
                </button>
            </div>
        `;
    }

    openModal() {
        if (this.modal) {
            this.modal.classList.add('active');
            this.isModalOpen = true;
            document.body.style.overflow = 'hidden';
        }
    }

    closeModal() {
        if (this.modal) {
            this.modal.classList.remove('active');
            this.isModalOpen = false;
            document.body.style.overflow = '';
            this.currentMilestoneId = null;
        }
    }

    async loadMoreCompletedMilestones() {
        if (!this.showMoreBtn) return;

        const originalText = this.showMoreBtn.textContent;
        this.showMoreBtn.textContent = 'Loading...';
        this.showMoreBtn.disabled = true;

        try {
            const response = await fetch('/roadmap/api/milestones?status=completed&page=2', {
                method: 'GET',
                headers: {
                    'Content-Type': 'application/json'
                }
            });

            if (!response.ok) {
                throw new Error(`HTTP ${response.status}: ${response.statusText}`);
            }

            const data = await response.json();

            if (data.success && data.data) {
                this.renderAdditionalCompletedMilestones(data.data);
                this.showMoreBtn.style.display = 'none';
            } else {
                throw new Error(data.message || 'Failed to load more milestones');
            }
        } catch (error) {
            console.error('Error loading more completed milestones:', error);
            this.showMoreBtn.textContent = 'Error - Try Again';
            setTimeout(() => {
                this.showMoreBtn.textContent = originalText;
                this.showMoreBtn.disabled = false;
            }, 3000);
        }
    }

    renderAdditionalCompletedMilestones(milestones) {
        const completedList = document.querySelector('.completed-list');
        if (!completedList) return;

        const newItems = milestones.map(milestone => `
            <div class="completed-item">
                <div class="completed-icon">✅</div>
                <div class="completed-content">
                    <h4 class="completed-title">${milestone.title}</h4>
                    <div class="completed-meta">
                        <span class="completed-category">${milestone.category}</span>
                        <span class="completed-date">${milestone.actualCompletionFormatted}</span>
                    </div>
                </div>
            </div>
        `).join('');

        completedList.insertAdjacentHTML('beforeend', newItems);
    }

    initCharts() {
        // Initialize Syncfusion charts for data visualization
        this.initProgressChart();
        this.initActivityChart();
    }

    initProgressChart() {
        const progressChartElement = document.getElementById('progressChart');
        if (!progressChartElement) return;

        // Placeholder for Syncfusion Chart
        // Will be implemented with actual chart data
        console.log('Progress chart placeholder initialized');
    }

    initActivityChart() {
        const activityChartElement = document.getElementById('activityChart');
        if (!activityChartElement) return;

        // Placeholder for Syncfusion Chart
        // Will be implemented with actual chart data
        console.log('Activity chart placeholder initialized');
    }

    startAutoRefresh() {
        // Refresh data every 5 minutes
        setInterval(() => {
            this.refreshDashboardData();
        }, 5 * 60 * 1000);
    }

    async refreshDashboardData() {
        try {
            const response = await fetch('/roadmap/api/progress', {
                method: 'GET',
                headers: {
                    'Content-Type': 'application/json'
                }
            });

            if (response.ok) {
                const data = await response.json();
                if (data.success) {
                    this.updateProgressMetrics(data.data);
                }
            }
        } catch (error) {
            console.warn('Failed to refresh dashboard data:', error);
        }
    }

    updateProgressMetrics(data) {
        // Update progress percentages
        const progressElements = document.querySelectorAll('[data-auto-update="progress"]');
        progressElements.forEach(element => {
            const metric = element.dataset.metric;
            if (data.overall && data.overall[metric]) {
                element.textContent = data.overall[metric];
            }
        });

        // Update last updated timestamp
        const lastUpdatedElements = document.querySelectorAll('[data-auto-update="timestamp"]');
        lastUpdatedElements.forEach(element => {
            element.textContent = new Date().toLocaleString();
        });

        console.log('Dashboard data refreshed');
    }

    // Utility methods
    formatDate(dateString) {
        if (!dateString) return 'N/A';

        const date = new Date(dateString);
        return date.toLocaleDateString('en-US', {
            month: 'short',
            day: 'numeric',
            year: 'numeric'
        });
    }

    formatDateTime(dateString) {
        if (!dateString) return 'N/A';

        const date = new Date(dateString);
        return date.toLocaleDateString('en-US', {
            month: 'short',
            day: 'numeric',
            year: 'numeric',
            hour: 'numeric',
            minute: '2-digit',
            hour12: true
        });
    }

    debounce(func, wait) {
        let timeout;
        return function executedFunction(...args) {
            const later = () => {
                clearTimeout(timeout);
                func(...args);
            };
            clearTimeout(timeout);
            timeout = setTimeout(later, wait);
        };
    }
}

// Initialize when DOM is loaded
document.addEventListener('DOMContentLoaded', () => {
    window.roadmapDashboard = new RoadmapDashboard();
});

// Handle navigation menu toggle (following site.js pattern)
document.addEventListener('DOMContentLoaded', () => {
    const navToggle = document.getElementById('nav-toggle');
    const navMenu = document.querySelector('.nav-menu');

    if (navToggle && navMenu) {
        navToggle.addEventListener('click', () => {
            navToggle.classList.toggle('active');
            navMenu.classList.toggle('active');
        });

        // Close menu when clicking outside
        document.addEventListener('click', (e) => {
            if (!navToggle.contains(e.target) && !navMenu.contains(e.target)) {
                navToggle.classList.remove('active');
                navMenu.classList.remove('active');
            }
        });

        // Close menu when clicking on links
        const navLinks = navMenu.querySelectorAll('.nav-link');
        navLinks.forEach(link => {
            link.addEventListener('click', () => {
                navToggle.classList.remove('active');
                navMenu.classList.remove('active');
            });
        });
    }
});

// Smooth scrolling for anchor links
document.addEventListener('DOMContentLoaded', () => {
    const scrollLinks = document.querySelectorAll('a[href^="#"]');

    scrollLinks.forEach(link => {
        link.addEventListener('click', (e) => {
            e.preventDefault();

            const targetId = link.getAttribute('href').substring(1);
            const targetElement = document.getElementById(targetId);

            if (targetElement) {
                const navbarHeight = document.querySelector('.navbar')?.offsetHeight || 80;
                const targetPosition = targetElement.offsetTop - navbarHeight;

                window.scrollTo({
                    top: targetPosition,
                    behavior: 'smooth'
                });
            }
        });
    });
});

// Performance optimization: Intersection Observer for animations
document.addEventListener('DOMContentLoaded', () => {
    const animateOnScroll = (entries, observer) => {
        entries.forEach(entry => {
            if (entry.isIntersecting) {
                entry.target.classList.add('animate-in');
                observer.unobserve(entry.target);
            }
        });
    };

    const observer = new IntersectionObserver(animateOnScroll, {
        threshold: 0.1,
        rootMargin: '0px 0px -50px 0px'
    });

    // Observe elements with animation classes
    const animatedElements = document.querySelectorAll('.milestone-card, .github-card, .dev-stat-card, .upcoming-card');
    animatedElements.forEach(el => observer.observe(el));
});