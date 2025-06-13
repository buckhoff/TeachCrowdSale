// TeachCrowdSale.Web/wwwroot/js/roadmap.js
// Phase 5: Core JavaScript Functionality for Roadmap Dashboard
// Following established patterns from analytics.js and site.js

// ================================================
// ROADMAP CONFIGURATION
// ================================================
const ROADMAP_CONFIG = {
    REFRESH_INTERVAL: 300000, // 5 minutes
    SEARCH_DEBOUNCE: 300,
    ANIMATION_DURATION: 300,
    MAX_RETRY_ATTEMPTS: 3,
    CACHE_DURATION: 900000, // 15 minutes
    API_ENDPOINTS: {
        getMilestones: '/Roadmap/GetMilestones',
        getMilestoneDetails: '/Roadmap/GetMilestoneDetails',
        getProgressData: '/Roadmap/GetProgressData',
        getFilteredData: '/Roadmap/GetFilteredMilestones',
        searchMilestones: '/Roadmap/Search',
        exportData: '/Roadmap/Export'
    }
};

// ================================================
// ROADMAP STATE MANAGEMENT
// ================================================
const RoadmapState = {
    data: {
        milestones: [],
        progressData: {},
        githubStats: {},
        recentUpdates: []
    },
    filters: {
        search: '',
        status: '',
        category: '',
        priority: '',
        dateRange: '6M'
    },
    ui: {
        activeModal: null,
        selectedMilestone: null,
        isLoading: false,
        connectionStatus: 'connected'
    },
    cache: new Map(),
    retryAttempts: 0
};

// ================================================
// ROADMAP API SERVICE
// ================================================
const RoadmapApiService = {
    // Get milestone data
    async getMilestones(filters = {}) {
        try {
            const cacheKey = `milestones_${JSON.stringify(filters)}`;
            const cached = this.getCachedData(cacheKey);
            if (cached) return cached;

            const queryParams = new URLSearchParams(filters).toString();
            const response = await fetch(`${ROADMAP_CONFIG.API_ENDPOINTS.getMilestones}?${queryParams}`);

            if (!response.ok) throw new Error(`HTTP ${response.status}: ${response.statusText}`);

            const data = await response.json();
            this.setCachedData(cacheKey, data);
            return data;
        } catch (error) {
            console.error('Failed to fetch milestones:', error);
            return this.getFallbackMilestones();
        }
    },

    // Get milestone details
    async getMilestoneDetails(milestoneId) {
        try {
            const cacheKey = `milestone_${milestoneId}`;
            const cached = this.getCachedData(cacheKey);
            if (cached) return cached;

            const response = await fetch(`${ROADMAP_CONFIG.API_ENDPOINTS.getMilestoneDetails}/${milestoneId}`);

            if (!response.ok) throw new Error(`HTTP ${response.status}: ${response.statusText}`);

            const data = await response.json();
            this.setCachedData(cacheKey, data);
            return data;
        } catch (error) {
            console.error(`Failed to fetch milestone ${milestoneId}:`, error);
            return this.getFallbackMilestoneDetails(milestoneId);
        }
    },

    // Get progress data for charts
    async getProgressData(params = {}) {
        try {
            const cacheKey = `progress_${JSON.stringify(params)}`;
            const cached = this.getCachedData(cacheKey);
            if (cached) return cached;

            const queryParams = new URLSearchParams(params).toString();
            const response = await fetch(`${ROADMAP_CONFIG.API_ENDPOINTS.getProgressData}?${queryParams}`);

            if (!response.ok) throw new Error(`HTTP ${response.status}: ${response.statusText}`);

            const data = await response.json();
            this.setCachedData(cacheKey, data, ROADMAP_CONFIG.CACHE_DURATION);
            return data;
        } catch (error) {
            console.error('Failed to fetch progress data:', error);
            return this.getFallbackProgressData();
        }
    },

    // Search milestones
    async searchMilestones(query, filters = {}) {
        try {
            if (!query.trim()) return [];

            const searchParams = new URLSearchParams({
                query: query.trim(),
                ...filters
            });

            const response = await fetch(`${ROADMAP_CONFIG.API_ENDPOINTS.searchMilestones}?${searchParams}`);

            if (!response.ok) throw new Error(`HTTP ${response.status}: ${response.statusText}`);

            return await response.json();
        } catch (error) {
            console.error('Search failed:', error);
            return this.performClientSideSearch(query);
        }
    },

    // Export milestone data
    async exportData(format = 'pdf', filters = {}) {
        try {
            const exportParams = new URLSearchParams({
                format,
                ...filters
            });

            const response = await fetch(`${ROADMAP_CONFIG.API_ENDPOINTS.exportData}?${exportParams}`);

            if (!response.ok) throw new Error(`HTTP ${response.status}: ${response.statusText}`);

            const blob = await response.blob();
            const url = window.URL.createObjectURL(blob);
            const a = document.createElement('a');
            a.href = url;
            a.download = `roadmap-export-${new Date().toISOString().split('T')[0]}.${format}`;
            document.body.appendChild(a);
            a.click();
            document.body.removeChild(a);
            window.URL.revokeObjectURL(url);

            RoadmapUtils.showNotification('Export completed successfully', 'success');
        } catch (error) {
            console.error('Export failed:', error);
            RoadmapUtils.showNotification('Export failed. Please try again.', 'error');
        }
    },

    // Cache management
    getCachedData(key) {
        const cached = RoadmapState.cache.get(key);
        if (cached && cached.expires > Date.now()) {
            return cached.data;
        }
        RoadmapState.cache.delete(key);
        return null;
    },

    setCachedData(key, data, duration = ROADMAP_CONFIG.CACHE_DURATION) {
        RoadmapState.cache.set(key, {
            data,
            expires: Date.now() + duration
        });
    },

    // Client-side search fallback
    performClientSideSearch(query) {
        const searchTerms = query.toLowerCase().split(' ');
        return RoadmapState.data.milestones.filter(milestone => {
            const searchText = `${milestone.title} ${milestone.description} ${milestone.category}`.toLowerCase();
            return searchTerms.every(term => searchText.includes(term));
        });
    },

    // Fallback data methods
    getFallbackMilestones() {
        return [
            {
                id: 1,
                title: 'Phase 1: Core Infrastructure',
                description: 'Foundation setup with API and database architecture',
                status: 'completed',
                category: 'Infrastructure',
                priority: 'high',
                progress: 100,
                startDate: '2024-01-01',
                endDate: '2024-03-31',
                tasks: ['API Development', 'Database Setup', 'Authentication']
            },
            {
                id: 2,
                title: 'Phase 2: Frontend Development',
                description: 'User interface and dashboard implementation',
                status: 'in-progress',
                category: 'Frontend',
                priority: 'high',
                progress: 75,
                startDate: '2024-03-01',
                endDate: '2024-06-30',
                tasks: ['Home Page', 'Analytics Dashboard', 'Roadmap Page']
            },
            {
                id: 3,
                title: 'Phase 3: Integration Testing',
                description: 'Comprehensive testing and quality assurance',
                status: 'planned',
                category: 'Testing',
                priority: 'medium',
                progress: 25,
                startDate: '2024-06-01',
                endDate: '2024-09-30',
                tasks: ['Unit Tests', 'Integration Tests', 'User Acceptance Testing']
            }
        ];
    },

    getFallbackMilestoneDetails(id) {
        const milestone = this.getFallbackMilestones().find(m => m.id === parseInt(id));
        return milestone ? {
            ...milestone,
            detailedDescription: 'Detailed description not available in offline mode.',
            dependencies: [],
            assignedTeam: 'Development Team',
            estimatedHours: 120,
            actualHours: milestone.progress === 100 ? 115 : Math.floor(120 * (milestone.progress / 100))
        } : null;
    },

    getFallbackProgressData() {
        return {
            overall: { completed: 42, inProgress: 18, planned: 15 },
            byCategory: {
                infrastructure: 85,
                frontend: 65,
                backend: 70,
                testing: 25,
                security: 40
            },
            timeline: this.generateTimelineData(),
            velocity: this.generateVelocityData()
        };
    },

    generateTimelineData() {
        const timeline = [];
        const baseDate = new Date('2024-01-01');

        for (let i = 0; i < 12; i++) {
            const date = new Date(baseDate);
            date.setMonth(baseDate.getMonth() + i);
            timeline.push({
                date: date.toISOString(),
                planned: 10 + Math.floor(Math.random() * 5),
                completed: Math.max(0, (10 + Math.floor(Math.random() * 5)) - Math.floor(Math.random() * 3))
            });
        }
        return timeline;
    },

    generateVelocityData() {
        const velocity = [];
        const today = new Date();

        for (let i = 30; i >= 0; i--) {
            const date = new Date(today);
            date.setDate(today.getDate() - i);
            velocity.push({
                date: date.toISOString(),
                commits: Math.floor(Math.random() * 10) + 1,
                tasksCompleted: Math.floor(Math.random() * 5) + 1
            });
        }
        return velocity;
    }
};

// ================================================
// ROADMAP FILTER MANAGER
// ================================================
const RoadmapFilterManager = {
    // Initialize filter controls
    init() {
        this.setupSearchFilter();
        this.setupDropdownFilters();
        this.setupDateRangeFilter();
        this.setupFilterReset();
    },

    // Setup search functionality
    setupSearchFilter() {
        const searchInput = document.getElementById('milestone-search');
        const searchBtn = document.getElementById('search-btn');
        const clearBtn = document.getElementById('search-clear');

        if (!searchInput) return;

        // Debounced search
        const debouncedSearch = RoadmapUtils.debounce(async (query) => {
            await this.performSearch(query);
        }, ROADMAP_CONFIG.SEARCH_DEBOUNCE);

        searchInput.addEventListener('input', (e) => {
            const query = e.target.value.trim();
            RoadmapState.filters.search = query;

            // Show/hide clear button
            if (clearBtn) {
                clearBtn.style.display = query ? 'flex' : 'none';
            }

            debouncedSearch(query);
        });

        // Search button click
        if (searchBtn) {
            searchBtn.addEventListener('click', () => {
                this.performSearch(searchInput.value.trim());
            });
        }

        // Clear search
        if (clearBtn) {
            clearBtn.addEventListener('click', () => {
                searchInput.value = '';
                RoadmapState.filters.search = '';
                clearBtn.style.display = 'none';
                this.performSearch('');
            });
        }

        // Enter key search
        searchInput.addEventListener('keypress', (e) => {
            if (e.key === 'Enter') {
                e.preventDefault();
                this.performSearch(searchInput.value.trim());
            }
        });
    },

    // Setup dropdown filters
    setupDropdownFilters() {
        const filters = ['status-filter', 'category-filter', 'priority-filter'];

        filters.forEach(filterId => {
            const filterElement = document.getElementById(filterId);
            if (!filterElement) return;

            filterElement.addEventListener('change', async (e) => {
                const filterType = filterId.replace('-filter', '');
                RoadmapState.filters[filterType] = e.target.value;
                await this.applyFilters();
            });
        });
    },

    // Setup date range filter
    setupDateRangeFilter() {
        const dateRangeButtons = document.querySelectorAll('.chart-time-filter');

        dateRangeButtons.forEach(button => {
            button.addEventListener('click', async (e) => {
                e.preventDefault();

                // Update active state
                dateRangeButtons.forEach(btn => btn.classList.remove('active'));
                button.classList.add('active');

                // Update filter
                RoadmapState.filters.dateRange = button.dataset.range;
                await this.applyFilters();
            });
        });
    },

    // Setup filter reset
    setupFilterReset() {
        const resetBtn = document.getElementById('reset-filters');
        if (!resetBtn) return;

        resetBtn.addEventListener('click', () => {
            this.resetAllFilters();
        });
    },

    // Perform search
    async performSearch(query) {
        try {
            RoadmapMilestoneManager.showLoading();

            if (!query) {
                // Show all milestones when search is empty
                await this.applyFilters();
                return;
            }

            const results = await RoadmapApiService.searchMilestones(query, RoadmapState.filters);
            RoadmapMilestoneManager.displayMilestones(results);
            this.updateSearchResultsCount(results.length);

        } catch (error) {
            console.error('Search failed:', error);
            RoadmapUtils.showNotification('Search failed. Please try again.', 'error');
        } finally {
            RoadmapMilestoneManager.hideLoading();
        }
    },

    // Apply all filters
    async applyFilters() {
        try {
            RoadmapMilestoneManager.showLoading();

            const filteredData = await RoadmapApiService.getMilestones(RoadmapState.filters);
            RoadmapMilestoneManager.displayMilestones(filteredData);
            this.updateFilterResultsCount(filteredData.length);

        } catch (error) {
            console.error('Filter application failed:', error);
            RoadmapUtils.showNotification('Filter application failed. Please try again.', 'error');
        } finally {
            RoadmapMilestoneManager.hideLoading();
        }
    },

    // Reset all filters
    resetAllFilters() {
        RoadmapState.filters = {
            search: '',
            status: '',
            category: '',
            priority: '',
            dateRange: '6M'
        };

        // Reset UI elements
        const searchInput = document.getElementById('milestone-search');
        if (searchInput) {
            searchInput.value = '';
        }

        const clearBtn = document.getElementById('search-clear');
        if (clearBtn) {
            clearBtn.style.display = 'none';
        }

        const filters = ['status-filter', 'category-filter', 'priority-filter'];
        filters.forEach(filterId => {
            const element = document.getElementById(filterId);
            if (element) {
                element.value = '';
            }
        });

        // Reset date range buttons
        const dateRangeButtons = document.querySelectorAll('.chart-time-filter');
        dateRangeButtons.forEach(btn => btn.classList.remove('active'));
        const defaultBtn = document.querySelector('.chart-time-filter[data-range="6M"]');
        if (defaultBtn) {
            defaultBtn.classList.add('active');
        }

        // Apply reset filters
        this.applyFilters();
    },

    // Update search results count
    updateSearchResultsCount(count) {
        const countElement = document.getElementById('search-results-count');
        if (countElement) {
            countElement.textContent = `${count} milestone${count !== 1 ? 's' : ''} found`;
        }
    },

    // Update filter results count
    updateFilterResultsCount(count) {
        const visibleCountElement = document.getElementById('visible-count');
        if (visibleCountElement) {
            visibleCountElement.textContent = count;
        }
    }
};

// ================================================
// ROADMAP MILESTONE MANAGER
// ================================================
const RoadmapMilestoneManager = {
    // Initialize milestone display
    init() {
        this.setupMilestoneCards();
        this.setupMilestoneModal();
        this.setupProgressAnimations();
    },

    // Setup milestone card interactions
    setupMilestoneCards() {
        document.addEventListener('click', async (e) => {
            const milestoneCard = e.target.closest('.milestone-card');
            if (!milestoneCard) return;

            e.preventDefault();
            const milestoneId = milestoneCard.dataset.milestoneId;
            if (milestoneId) {
                await this.showMilestoneDetails(milestoneId);
            }
        });

        // Setup hover animations
        document.addEventListener('mouseenter', (e) => {
            const milestoneCard = e.target.closest('.milestone-card');
            if (milestoneCard) {
                this.animateMilestoneCard(milestoneCard, 'enter');
            }
        }, true);

        document.addEventListener('mouseleave', (e) => {
            const milestoneCard = e.target.closest('.milestone-card');
            if (milestoneCard) {
                this.animateMilestoneCard(milestoneCard, 'leave');
            }
        }, true);
    },

    // Setup milestone detail modal
    setupMilestoneModal() {
        const modal = document.getElementById('milestone-modal');
        const closeBtn = document.getElementById('close-milestone-modal');

        if (closeBtn) {
            closeBtn.addEventListener('click', () => {
                this.closeMilestoneModal();
            });
        }

        // Close on backdrop click
        if (modal) {
            modal.addEventListener('click', (e) => {
                if (e.target === modal) {
                    this.closeMilestoneModal();
                }
            });
        }

        // Close on Escape key
        document.addEventListener('keydown', (e) => {
            if (e.key === 'Escape' && RoadmapState.ui.activeModal) {
                this.closeMilestoneModal();
            }
        });
    },

    // Show milestone details
    async showMilestoneDetails(milestoneId) {
        try {
            const modal = document.getElementById('milestone-modal');
            const content = document.getElementById('milestone-details-content');

            if (!modal || !content) return;

            // Show loading state
            content.innerHTML = `
                <div class="loading-content">
                    <div class="loading-spinner"></div>
                    <p>Loading milestone details...</p>
                </div>
            `;

            modal.style.display = 'flex';
            RoadmapState.ui.activeModal = 'milestone';
            RoadmapState.ui.selectedMilestone = milestoneId;

            // Fetch milestone details
            const details = await RoadmapApiService.getMilestoneDetails(milestoneId);

            if (details) {
                content.innerHTML = this.generateMilestoneDetailsHTML(details);
                this.setupModalInteractions();
            } else {
                throw new Error('Milestone details not found');
            }

        } catch (error) {
            console.error('Failed to load milestone details:', error);
            const content = document.getElementById('milestone-details-content');
            if (content) {
                content.innerHTML = `
                    <div class="error-content">
                        <div class="error-icon">⚠️</div>
                        <h4>Failed to Load Details</h4>
                        <p>Unable to load milestone details. Please try again.</p>
                        <button class="btn-primary" onclick="RoadmapMilestoneManager.showMilestoneDetails('${milestoneId}')">
                            Retry
                        </button>
                    </div>
                `;
            }
        }
    },

    // Close milestone modal
    closeMilestoneModal() {
        const modal = document.getElementById('milestone-modal');
        if (modal) {
            modal.style.display = 'none';
        }

        RoadmapState.ui.activeModal = null;
        RoadmapState.ui.selectedMilestone = null;
    },

    // Generate milestone details HTML
    generateMilestoneDetailsHTML(details) {
        return `
            <div class="milestone-detail-header">
                <h3 id="modal-milestone-title">${details.title}</h3>
                <div class="milestone-detail-status">
                    <span class="milestone-status ${details.status}">${details.status.replace('-', ' ')}</span>
                    <span class="milestone-priority ${details.priority}">${details.priority} Priority</span>
                </div>
            </div>

            <div class="milestone-detail-progress">
                <div class="progress-header">
                    <span class="progress-label">Progress</span>
                    <span class="progress-percentage">${details.progress}%</span>
                </div>
                <div class="milestone-progress-bar">
                    <div class="milestone-progress-fill" style="width: ${details.progress}%"></div>
                </div>
            </div>

            <div class="milestone-detail-description">
                <h4>Description</h4>
                <p>${details.detailedDescription || details.description}</p>
            </div>

            <div class="milestone-detail-tasks">
                <h4>Tasks (${details.tasks?.length || 0})</h4>
                <div class="task-list">
                    ${details.tasks?.map(task => `
                        <div class="task-item">
                            <span class="task-icon">${task.completed ? '✅' : '⏳'}</span>
                            <span class="task-name">${task.name || task}</span>
                        </div>
                    `).join('') || '<p>No tasks available</p>'}
                </div>
            </div>

            <div class="milestone-detail-meta">
                <div class="meta-grid">
                    <div class="meta-item">
                        <span class="meta-label">Category</span>
                        <span class="meta-value">${details.category}</span>
                    </div>
                    <div class="meta-item">
                        <span class="meta-label">Start Date</span>
                        <span class="meta-value">${RoadmapUtils.formatDate(details.startDate)}</span>
                    </div>
                    <div class="meta-item">
                        <span class="meta-label">End Date</span>
                        <span class="meta-value">${RoadmapUtils.formatDate(details.endDate)}</span>
                    </div>
                    <div class="meta-item">
                        <span class="meta-label">Assigned Team</span>
                        <span class="meta-value">${details.assignedTeam || 'Development Team'}</span>
                    </div>
                </div>
            </div>

            ${details.dependencies?.length ? `
                <div class="milestone-detail-dependencies">
                    <h4>Dependencies</h4>
                    <div class="dependency-list">
                        ${details.dependencies.map(dep => `
                            <div class="dependency-item">
                                <span class="dependency-name">${dep.title}</span>
                                <span class="dependency-status ${dep.status}">${dep.status}</span>
                            </div>
                        `).join('')}
                    </div>
                </div>
            ` : ''}
        `;
    },

    // Setup modal interactions
    setupModalInteractions() {
        // Add any specific modal interactions here
        const progressBars = document.querySelectorAll('.milestone-progress-fill');
        progressBars.forEach(bar => {
            this.animateProgressBar(bar);
        });
    },

    // Display milestones
    displayMilestones(milestones) {
        const container = document.getElementById('milestones-container');
        const emptyState = document.getElementById('milestones-empty');

        if (!container) return;

        if (!milestones || milestones.length === 0) {
            container.style.display = 'none';
            if (emptyState) {
                emptyState.style.display = 'block';
            }
            return;
        }

        if (emptyState) {
            emptyState.style.display = 'none';
        }
        container.style.display = 'grid';

        container.innerHTML = milestones.map(milestone => this.generateMilestoneCardHTML(milestone)).join('');

        // Animate cards appearance
        this.animateCardsEntrance(container);
    },

    // Generate milestone card HTML
    generateMilestoneCardHTML(milestone) {
        return `
            <div class="milestone-card ${milestone.status}" data-milestone-id="${milestone.id}">
                <div class="milestone-header">
                    <div class="milestone-title-section">
                        <h4 class="milestone-title">${milestone.title}</h4>
                        <span class="milestone-status ${milestone.status}">${milestone.status.replace('-', ' ')}</span>
                    </div>
                </div>

                <div class="milestone-description">
                    ${milestone.description}
                </div>

                <div class="milestone-progress">
                    <div class="milestone-progress-label">
                        <span>Progress</span>
                        <span class="milestone-progress-percentage">${milestone.progress}%</span>
                    </div>
                    <div class="milestone-progress-bar">
                        <div class="milestone-progress-fill" data-percentage="${milestone.progress}"></div>
                    </div>
                </div>

                <div class="milestone-meta">
                    <span class="milestone-category ${milestone.priority}">${milestone.category}</span>
                    <span class="milestone-due-date">
                        <span class="date-icon">📅</span>
                        ${RoadmapUtils.formatDate(milestone.endDate)}
                    </span>
                </div>
            </div>
        `;
    },

    // Animate milestone card
    animateMilestoneCard(card, type) {
        if (type === 'enter') {
            card.style.transform = 'translateY(-8px)';
            card.style.transition = 'transform 0.3s ease';
        } else {
            card.style.transform = 'translateY(0)';
        }
    },

    // Animate cards entrance
    animateCardsEntrance(container) {
        const cards = container.querySelectorAll('.milestone-card');
        cards.forEach((card, index) => {
            card.style.opacity = '0';
            card.style.transform = 'translateY(20px)';

            setTimeout(() => {
                card.style.transition = 'opacity 0.5s ease, transform 0.5s ease';
                card.style.opacity = '1';
                card.style.transform = 'translateY(0)';

                // Animate progress bars
                const progressBar = card.querySelector('.milestone-progress-fill');
                if (progressBar) {
                    setTimeout(() => this.animateProgressBar(progressBar), 200);
                }
            }, index * 100);
        });
    },

    // Setup progress animations
    setupProgressAnimations() {
        // Animate progress bars when they come into view
        const observer = new IntersectionObserver((entries) => {
            entries.forEach(entry => {
                if (entry.isIntersecting) {
                    const progressBar = entry.target.querySelector('.milestone-progress-fill');
                    if (progressBar && !progressBar.dataset.animated) {
                        this.animateProgressBar(progressBar);
                        progressBar.dataset.animated = 'true';
                    }
                }
            });
        }, { threshold: 0.5 });

        // Observe milestone cards
        document.querySelectorAll('.milestone-card').forEach(card => {
            observer.observe(card);
        });
    },

    // Animate progress bar
    animateProgressBar(progressBar) {
        const targetPercentage = parseFloat(progressBar.dataset.percentage || 0);

        progressBar.style.width = '0%';
        progressBar.style.transition = 'width 1.5s cubic-bezier(0.4, 0, 0.2, 1)';

        setTimeout(() => {
            progressBar.style.width = `${targetPercentage}%`;
        }, 100);
    },

    // Show/hide loading state
    showLoading() {
        const loading = document.getElementById('milestones-loading');
        const container = document.getElementById('milestones-container');

        if (loading) loading.style.display = 'flex';
        if (container) container.style.opacity = '0.5';

        RoadmapState.ui.isLoading = true;
    },

    hideLoading() {
        const loading = document.getElementById('milestones-loading');
        const container = document.getElementById('milestones-container');

        if (loading) loading.style.display = 'none';
        if (container) container.style.opacity = '1';

        RoadmapState.ui.isLoading = false;
    }
};

// ================================================
// ROADMAP GITHUB INTEGRATION
// ================================================
const RoadmapGitHubManager = {
    // Initialize GitHub integration
    init() {
        this.setupGitHubStats();
        this.setupCommitPopover();
        this.startPeriodicUpdates();
    },

    // Setup GitHub stats display
    async setupGitHubStats() {
        try {
            const githubData = await this.fetchGitHubStats();
            this.displayGitHubStats(githubData);
            this.displayRecentCommits(githubData.recentCommits);
        } catch (error) {
            console.error('Failed to load GitHub stats:', error);
            this.displayFallbackGitHubStats();
        }
    },

    // Fetch GitHub statistics
    async fetchGitHubStats() {
        try {
            const response = await fetch('/Roadmap/GetGitHubStats');
            if (!response.ok) throw new Error(`HTTP ${response.status}`);
            return await response.json();
        } catch (error) {
            console.error('GitHub API failed:', error);
            return this.getFallbackGitHubStats();
        }
    },

    // Display GitHub stats
    displayGitHubStats(stats) {
        const elements = {
            commits: document.getElementById('github-commits'),
            contributors: document.getElementById('github-contributors'),
            openIssues: document.getElementById('github-issues'),
            lastCommit: document.getElementById('last-commit-time')
        };

        if (elements.commits) elements.commits.textContent = stats.totalCommits || 0;
        if (elements.contributors) elements.contributors.textContent = stats.contributors || 0;
        if (elements.openIssues) elements.openIssues.textContent = stats.openIssues || 0;
        if (elements.lastCommit) elements.lastCommit.textContent = RoadmapUtils.formatRelativeTime(stats.lastCommitDate);
    },

    // Display recent commits
    displayRecentCommits(commits) {
        const container = document.getElementById('recent-commits-container');
        if (!container || !commits) return;

        container.innerHTML = commits.slice(0, 5).map(commit => `
            <div class="commit-item" data-commit-hash="${commit.hash}">
                <div class="commit-avatar">
                    ${commit.author.charAt(0).toUpperCase()}
                </div>
                <div class="commit-details">
                    <div class="commit-message">${commit.message}</div>
                    <div class="commit-meta">
                        <span class="commit-author">${commit.author}</span>
                        <span class="commit-hash">${commit.hash.substring(0, 7)}</span>
                        <span class="commit-time">${RoadmapUtils.formatRelativeTime(commit.date)}</span>
                    </div>
                </div>
            </div>
        `).join('');
    },

    // Setup commit popover
    setupCommitPopover() {
        document.addEventListener('click', async (e) => {
            const commitItem = e.target.closest('.commit-item');
            if (!commitItem) return;

            const hash = commitItem.dataset.commitHash;
            if (hash) {
                await this.showCommitDetails(hash);
            }
        });
    },

    // Show commit details
    async showCommitDetails(hash) {
        try {
            const details = await this.fetchCommitDetails(hash);
            this.displayCommitPopover(details);
        } catch (error) {
            console.error('Failed to load commit details:', error);
            RoadmapUtils.showNotification('Failed to load commit details', 'error');
        }
    },

    // Fetch commit details
    async fetchCommitDetails(hash) {
        const response = await fetch(`/Roadmap/GetCommitDetails/${hash}`);
        if (!response.ok) throw new Error(`HTTP ${response.status}`);
        return await response.json();
    },

    // Display commit popover
    displayCommitPopover(details) {
        const popover = document.createElement('div');
        popover.className = 'commit-popover';
        popover.innerHTML = `
            <div class="popover-header">
                <h4>Commit Details</h4>
                <button class="popover-close">&times;</button>
            </div>
            <div class="popover-content">
                <div class="commit-info">
                    <strong>Hash:</strong> ${details.hash}
                </div>
                <div class="commit-info">
                    <strong>Author:</strong> ${details.author}
                </div>
                <div class="commit-info">
                    <strong>Date:</strong> ${RoadmapUtils.formatDate(details.date)}
                </div>
                <div class="commit-info">
                    <strong>Message:</strong> ${details.message}
                </div>
                <div class="commit-changes">
                    <strong>Changes:</strong>
                    <div class="changes-summary">
                        <span class="additions">+${details.additions || 0}</span>
                        <span class="deletions">-${details.deletions || 0}</span>
                    </div>
                </div>
            </div>
        `;

        document.body.appendChild(popover);

        // Position popover
        popover.style.position = 'fixed';
        popover.style.top = '50%';
        popover.style.left = '50%';
        popover.style.transform = 'translate(-50%, -50%)';
        popover.style.zIndex = '1001';

        // Close popover
        const closeBtn = popover.querySelector('.popover-close');
        closeBtn.addEventListener('click', () => {
            document.body.removeChild(popover);
        });

        // Auto-close after 10 seconds
        setTimeout(() => {
            if (document.body.contains(popover)) {
                document.body.removeChild(popover);
            }
        }, 10000);
    },

    // Start periodic updates
    startPeriodicUpdates() {
        setInterval(async () => {
            await this.setupGitHubStats();
        }, ROADMAP_CONFIG.REFRESH_INTERVAL);
    },

    // Fallback GitHub stats
    getFallbackGitHubStats() {
        return {
            totalCommits: 247,
            contributors: 5,
            openIssues: 12,
            lastCommitDate: new Date().toISOString(),
            recentCommits: [
                {
                    hash: 'abc123f',
                    message: 'Update roadmap dashboard styling',
                    author: 'Dev Team',
                    date: new Date().toISOString(),
                    additions: 45,
                    deletions: 12
                },
                {
                    hash: 'def456g',
                    message: 'Add milestone filtering functionality',
                    author: 'Dev Team',
                    date: new Date(Date.now() - 86400000).toISOString(),
                    additions: 120,
                    deletions: 8
                }
            ]
        };
    },

    // Display fallback GitHub stats
    displayFallbackGitHubStats() {
        const fallbackStats = this.getFallbackGitHubStats();
        this.displayGitHubStats(fallbackStats);
        this.displayRecentCommits(fallbackStats.recentCommits);

        RoadmapUtils.showNotification('GitHub data temporarily unavailable. Showing cached data.', 'warning');
    }
};

// ================================================
// ROADMAP EXPORT MANAGER
// ================================================
const RoadmapExportManager = {
    // Initialize export functionality
    init() {
        this.setupExportControls();
        this.setupKeyboardShortcuts();
    },

    // Setup export controls
    setupExportControls() {
        const exportBtn = document.getElementById('export-roadmap');
        if (exportBtn) {
            exportBtn.addEventListener('click', () => {
                this.showExportDialog();
            });
        }

        // Chart export buttons
        const chartExportBtns = document.querySelectorAll('.chart-export-btn');
        chartExportBtns.forEach(btn => {
            btn.addEventListener('click', (e) => {
                const chartContainer = e.target.closest('.chart-container');
                const chartId = chartContainer?.id;
                if (chartId) {
                    this.exportChart(chartId);
                }
            });
        });
    },

    // Show export dialog
    showExportDialog() {
        const dialog = document.createElement('div');
        dialog.className = 'export-dialog-overlay';
        dialog.innerHTML = `
            <div class="export-dialog">
                <div class="export-header">
                    <h3>Export Roadmap Data</h3>
                    <button class="export-close">&times;</button>
                </div>
                <div class="export-content">
                    <div class="export-options">
                        <div class="export-format">
                            <label>Export Format:</label>
                            <select id="export-format">
                                <option value="pdf">PDF Report</option>
                                <option value="csv">CSV Data</option>
                                <option value="json">JSON Data</option>
                                <option value="xlsx">Excel Spreadsheet</option>
                            </select>
                        </div>
                        <div class="export-scope">
                            <label>Include:</label>
                            <div class="checkbox-group">
                                <label><input type="checkbox" id="include-milestones" checked> Milestones</label>
                                <label><input type="checkbox" id="include-progress" checked> Progress Data</label>
                                <label><input type="checkbox" id="include-github" checked> GitHub Stats</label>
                                <label><input type="checkbox" id="include-charts"> Chart Images</label>
                            </div>
                        </div>
                        <div class="export-filters">
                            <label>Apply Current Filters:</label>
                            <label><input type="checkbox" id="apply-filters" checked> Use active filters</label>
                        </div>
                    </div>
                    <div class="export-actions">
                        <button class="btn-secondary" id="cancel-export">Cancel</button>
                        <button class="btn-primary" id="start-export">Export</button>
                    </div>
                </div>
            </div>
        `;

        document.body.appendChild(dialog);

        // Setup dialog interactions
        this.setupExportDialogInteractions(dialog);
    },

    // Setup export dialog interactions
    setupExportDialogInteractions(dialog) {
        const closeBtn = dialog.querySelector('.export-close');
        const cancelBtn = dialog.querySelector('#cancel-export');
        const exportBtn = dialog.querySelector('#start-export');

        const closeDialog = () => {
            document.body.removeChild(dialog);
        };

        closeBtn.addEventListener('click', closeDialog);
        cancelBtn.addEventListener('click', closeDialog);

        exportBtn.addEventListener('click', async () => {
            const format = dialog.querySelector('#export-format').value;
            const options = {
                includeMilestones: dialog.querySelector('#include-milestones').checked,
                includeProgress: dialog.querySelector('#include-progress').checked,
                includeGithub: dialog.querySelector('#include-github').checked,
                includeCharts: dialog.querySelector('#include-charts').checked,
                applyFilters: dialog.querySelector('#apply-filters').checked
            };

            exportBtn.disabled = true;
            exportBtn.textContent = 'Exporting...';

            try {
                await this.performExport(format, options);
                closeDialog();
            } catch (error) {
                console.error('Export failed:', error);
                RoadmapUtils.showNotification('Export failed. Please try again.', 'error');
            } finally {
                exportBtn.disabled = false;
                exportBtn.textContent = 'Export';
            }
        });

        // Close on backdrop click
        dialog.addEventListener('click', (e) => {
            if (e.target === dialog) {
                closeDialog();
            }
        });
    },

    // Perform export
    async performExport(format, options) {
        const filters = options.applyFilters ? RoadmapState.filters : {};

        await RoadmapApiService.exportData(format, {
            ...filters,
            ...options
        });
    },

    // Export individual chart
    async exportChart(chartId) {
        try {
            const chart = RoadmapChartState.charts[chartId];
            if (chart && typeof chart.export === 'function') {
                chart.export('PNG', `roadmap-${chartId}-${Date.now()}`);
                RoadmapUtils.showNotification('Chart exported successfully', 'success');
            } else {
                throw new Error('Chart export not available');
            }
        } catch (error) {
            console.error('Chart export failed:', error);
            RoadmapUtils.showNotification('Chart export failed', 'error');
        }
    },

    // Setup keyboard shortcuts
    setupKeyboardShortcuts() {
        document.addEventListener('keydown', (e) => {
            // Ctrl/Cmd + E for export
            if ((e.ctrlKey || e.metaKey) && e.key === 'e') {
                e.preventDefault();
                this.showExportDialog();
            }
        });
    }
};

// ================================================
// ROADMAP UTILITIES
// ================================================
const RoadmapUtils = {
    // Format date
    formatDate(dateString) {
        if (!dateString) return 'N/A';

        try {
            const date = new Date(dateString);
            if (isNaN(date.getTime())) return 'Invalid Date';

            return date.toLocaleDateString('en-US', {
                year: 'numeric',
                month: 'short',
                day: 'numeric'
            });
        } catch (error) {
            return 'Invalid Date';
        }
    },

    // Format relative time
    formatRelativeTime(dateString) {
        if (!dateString) return 'Unknown';

        try {
            const date = new Date(dateString);
            const now = new Date();
            const diffMs = now - date;
            const diffMins = Math.floor(diffMs / 60000);
            const diffHours = Math.floor(diffMs / 3600000);
            const diffDays = Math.floor(diffMs / 86400000);

            if (diffMins < 1) return 'Just now';
            if (diffMins < 60) return `${diffMins}m ago`;
            if (diffHours < 24) return `${diffHours}h ago`;
            if (diffDays < 7) return `${diffDays}d ago`;

            return this.formatDate(dateString);
        } catch (error) {
            return 'Unknown';
        }
    },

    // Debounce function
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
    },

    // Show notification
    showNotification(message, type = 'info', duration = 5000) {
        const notification = document.createElement('div');
        notification.className = `roadmap-notification notification-${type}`;
        notification.innerHTML = `
            <div class="notification-content">
                <span class="notification-icon">${this.getNotificationIcon(type)}</span>
                <span class="notification-message">${message}</span>
                <button class="notification-close">&times;</button>
            </div>
        `;

        notification.style.cssText = `
            position: fixed;
            top: 20px;
            right: 20px;
            padding: 16px 20px;
            background: var(--card-bg);
            border: 1px solid rgba(255, 255, 255, 0.1);
            border-left: 4px solid ${this.getNotificationColor(type)};
            border-radius: 8px;
            color: var(--text-primary);
            z-index: 1000;
            transform: translateX(100%);
            transition: transform 0.3s ease;
            max-width: 400px;
        `;

        document.body.appendChild(notification);

        // Animate in
        setTimeout(() => {
            notification.style.transform = 'translateX(0)';
        }, 100);

        // Setup close button
        const closeBtn = notification.querySelector('.notification-close');
        closeBtn.addEventListener('click', () => {
            this.removeNotification(notification);
        });

        // Auto-remove
        setTimeout(() => {
            if (document.body.contains(notification)) {
                this.removeNotification(notification);
            }
        }, duration);
    },

    // Remove notification
    removeNotification(notification) {
        notification.style.transform = 'translateX(100%)';
        setTimeout(() => {
            if (document.body.contains(notification)) {
                document.body.removeChild(notification);
            }
        }, 300);
    },

    // Get notification icon
    getNotificationIcon(type) {
        const icons = {
            success: '✅',
            error: '❌',
            warning: '⚠️',
            info: 'ℹ️'
        };
        return icons[type] || icons.info;
    },

    // Get notification color
    getNotificationColor(type) {
        const colors = {
            success: '#10b981',
            error: '#ef4444',
            warning: '#f59e0b',
            info: '#4f46e5'
        };
        return colors[type] || colors.info;
    },

    // Check if element is in viewport
    isInViewport(element, threshold = 0.1) {
        const rect = element.getBoundingClientRect();
        const windowHeight = window.innerHeight || document.documentElement.clientHeight;

        return (
            rect.top <= windowHeight * (1 - threshold) &&
            rect.bottom >= windowHeight * threshold
        );
    },

    // Smooth scroll to element
    scrollToElement(elementId, offset = 80) {
        const element = document.getElementById(elementId);
        if (!element) return;

        const elementPosition = element.getBoundingClientRect().top + window.pageYOffset;
        const offsetPosition = elementPosition - offset;

        window.scrollTo({
            top: offsetPosition,
            behavior: 'smooth'
        });
    },

    // Copy to clipboard
    async copyToClipboard(text) {
        try {
            await navigator.clipboard.writeText(text);
            this.showNotification('Copied to clipboard', 'success', 2000);
        } catch (error) {
            console.error('Failed to copy to clipboard:', error);
            this.showNotification('Failed to copy to clipboard', 'error');
        }
    }
};

// ================================================
// ROADMAP MAIN CONTROLLER
// ================================================
const RoadmapMain = {
    // Initialize roadmap application
    async init() {
        try {
            console.log('Initializing Roadmap Dashboard...');

            // Initialize components in order
            await this.loadInitialData();
            this.initializeComponents();
            this.setupGlobalEventListeners();
            this.startPeriodicUpdates();

            console.log('Roadmap Dashboard initialized successfully');

            // Show success notification
            RoadmapUtils.showNotification('Roadmap dashboard loaded successfully', 'success', 3000);

        } catch (error) {
            console.error('Failed to initialize roadmap dashboard:', error);
            this.handleInitializationError(error);
        }
    },

    // Load initial data
    async loadInitialData() {
        try {
            // Load milestone data
            const milestones = await RoadmapApiService.getMilestones();
            RoadmapState.data.milestones = milestones;

            // Load progress data
            const progressData = await RoadmapApiService.getProgressData();
            RoadmapState.data.progressData = progressData;

            // Display initial milestones
            RoadmapMilestoneManager.displayMilestones(milestones);

        } catch (error) {
            console.error('Failed to load initial data:', error);
            // Use fallback data
            RoadmapState.data.milestones = RoadmapApiService.getFallbackMilestones();
            RoadmapMilestoneManager.displayMilestones(RoadmapState.data.milestones);
        }
    },

    // Initialize all components
    initializeComponents() {
        RoadmapFilterManager.init();
        RoadmapMilestoneManager.init();
        RoadmapGitHubManager.init();
        RoadmapExportManager.init();

        // Initialize charts if chart manager is available
        if (window.RoadmapChartManager) {
            window.RoadmapChartManager.initializeCharts();
        }
    },

    // Setup global event listeners
    setupGlobalEventListeners() {
        // Visibility change handler
        document.addEventListener('visibilitychange', () => {
            if (document.visibilityState === 'visible') {
                this.refreshData();
            }
        });

        // Window focus handler
        window.addEventListener('focus', () => {
            this.refreshData();
        });

        // Global error handler
        window.addEventListener('error', (e) => {
            console.error('Global error:', e.error);
            RoadmapUtils.showNotification('An unexpected error occurred', 'error');
        });
    },

    // Start periodic updates
    startPeriodicUpdates() {
        setInterval(async () => {
            await this.refreshData();
        }, ROADMAP_CONFIG.REFRESH_INTERVAL);
    },

    // Refresh data
    async refreshData() {
        try {
            const milestones = await RoadmapApiService.getMilestones(RoadmapState.filters);
            RoadmapState.data.milestones = milestones;
            RoadmapMilestoneManager.displayMilestones(milestones);

            // Update GitHub stats
            await RoadmapGitHubManager.setupGitHubStats();

        } catch (error) {
            console.error('Failed to refresh data:', error);
        }
    },

    // Handle initialization error
    handleInitializationError(error) {
        const errorContainer = document.getElementById('roadmap-error-container');
        if (errorContainer) {
            errorContainer.innerHTML = `
                <div class="initialization-error">
                    <div class="error-icon">⚠️</div>
                    <h3>Dashboard Unavailable</h3>
                    <p>Failed to load the roadmap dashboard. Please check your connection and try again.</p>
                    <button class="btn-primary" onclick="location.reload()">Reload Page</button>
                </div>
            `;
            errorContainer.style.display = 'block';
        }

        RoadmapUtils.showNotification('Failed to load dashboard. Using offline mode.', 'error');
    }
};

// ================================================
// INITIALIZATION
// ================================================
document.addEventListener('DOMContentLoaded', async () => {
    // Wait for any dependencies to load
    await new Promise(resolve => {
        if (typeof ej !== 'undefined') {
            resolve();
        } else {
            const checkForSyncfusion = setInterval(() => {
                if (typeof ej !== 'undefined') {
                    clearInterval(checkForSyncfusion);
                    resolve();
                }
            }, 100);

            // Timeout after 5 seconds
            setTimeout(() => {
                clearInterval(checkForSyncfusion);
                console.warn('Syncfusion not loaded, continuing without charts');
                resolve();
            }, 5000);
        }
    });

    // Initialize roadmap application
    await RoadmapMain.init();
});

// Export for global access
window.RoadmapMain = RoadmapMain;
window.RoadmapState = RoadmapState;
window.RoadmapApiService = RoadmapApiService;
window.RoadmapFilterManager = RoadmapFilterManager;
window.RoadmapMilestoneManager = RoadmapMilestoneManager;
window.RoadmapGitHubManager = RoadmapGitHubManager;
window.RoadmapExportManager = RoadmapExportManager;
window.RoadmapUtils = RoadmapUtils;