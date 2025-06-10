/**
 * School Integration Components - Phase 7
 * Handles school selection interface and reward distribution visualization
 * Implements the 50/50 reward split core to TeachToken's educational mission
 */

class SchoolIntegrationManager {
    constructor(stakingDashboard) {
        this.stakingDashboard = stakingDashboard;
        this.selectedSchool = null;
        this.schools = [];
        this.impactData = {};
        this.searchTerm = '';
        this.filters = {
            location: '',
            studentCount: '',
            fundingNeed: ''
        };

        // Bind methods
        this.searchSchools = this.searchSchools.bind(this);
        this.selectSchool = this.selectSchool.bind(this);
        this.updateFilters = this.updateFilters.bind(this);
        this.calculateImpact = this.calculateImpact.bind(this);

        this.init();
    }

    async init() {
        try {
            await this.loadSchools();
            this.setupSchoolSelection();
            this.renderSchoolSelection();
            this.setupEventListeners();
        } catch (error) {
            console.error('Failed to initialize school integration:', error);
        }
    }

    async loadSchools() {
        try {
            this.schools = await this.stakingDashboard.fetchWithCache('schools', '/GetSchools', 600000);
            this.renderSchoolGrid();
        } catch (error) {
            console.error('Error loading schools:', error);
            this.schools = this.getFallbackSchools();
            this.renderSchoolGrid();
        }
    }

    setupSchoolSelection() {
        const container = document.getElementById('school-selection-container');
        if (!container) return;

        container.innerHTML = this.renderSchoolSelectionInterface();
    }

    renderSchoolSelectionInterface() {
        return `
            <div class="school-selection-section">
                <div class="school-header">
                    <h2 class="school-title">Select Your School Beneficiary</h2>
                    <p class="school-subtitle">50% of your staking rewards will support your chosen school</p>
                </div>

                <div class="school-search-container">
                    <div class="search-input-wrapper">
                        <input type="text" 
                               id="school-search" 
                               class="school-search-input" 
                               placeholder="Search schools by name or location..."
                               value="${this.searchTerm}">
                        <div class="search-icon">🔍</div>
                    </div>
                    
                    <div class="school-filters">
                        <select id="location-filter" class="filter-select">
                            <option value="">All Locations</option>
                            <option value="urban">Urban</option>
                            <option value="suburban">Suburban</option>
                            <option value="rural">Rural</option>
                        </select>
                        
                        <select id="student-count-filter" class="filter-select">
                            <option value="">All Sizes</option>
                            <option value="small">Small (< 300 students)</option>
                            <option value="medium">Medium (300-800 students)</option>
                            <option value="large">Large (> 800 students)</option>
                        </select>
                        
                        <select id="funding-need-filter" class="filter-select">
                            <option value="">All Funding Levels</option>
                            <option value="high">High Need</option>
                            <option value="medium">Medium Need</option>
                            <option value="low">Low Need</option>
                        </select>
                    </div>
                </div>

                <div id="school-grid" class="school-grid">
                    <!-- Schools will be rendered here -->
                </div>

                <div id="selected-school-info" class="selected-school-info" style="display: none;">
                    <!-- Selected school info will be rendered here -->
                </div>

                <div class="impact-visualization">
                    <h3 class="impact-title">Your Projected Impact</h3>
                    <div id="impact-calculator" class="impact-calculator">
                        <!-- Impact calculations will be rendered here -->
                    </div>
                </div>
            </div>
        `;
    }

    renderSchoolGrid() {
        const container = document.getElementById('school-grid');
        if (!container) return;

        const filteredSchools = this.getFilteredSchools();

        if (filteredSchools.length === 0) {
            container.innerHTML = this.renderNoSchoolsFound();
            return;
        }

        container.innerHTML = filteredSchools.map(school => this.renderSchoolCard(school)).join('');
        this.animateSchoolCards();
    }

    renderSchoolCard(school) {
        const isSelected = this.selectedSchool?.id === school.id;
        const impactLevel = this.calculateSchoolImpactLevel(school);
        const fundingProgress = (school.totalFundsReceived / school.fundingGoal) * 100;

        return `
            <div class="school-card ${isSelected ? 'selected' : ''}" 
                 data-school-id="${school.id}"
                 onclick="window.schoolIntegration?.selectSchool('${school.id}')">
                
                <div class="school-card-header">
                    <div class="school-info">
                        <h3 class="school-name">${school.name}</h3>
                        <p class="school-location">${school.location}</p>
                    </div>
                    <div class="school-status">
                        <span class="impact-badge ${impactLevel.toLowerCase()}">${impactLevel} Impact</span>
                        ${isSelected ? '<div class="selected-indicator">✓ Selected</div>' : ''}
                    </div>
                </div>

                <div class="school-description">
                    <p>${school.description}</p>
                </div>

                <div class="school-metrics">
                    <div class="metric-row">
                        <div class="metric-item">
                            <span class="metric-label">Students</span>
                            <span class="metric-value">${school.studentsCount.toLocaleString()}</span>
                        </div>
                        <div class="metric-item">
                            <span class="metric-label">Grade Levels</span>
                            <span class="metric-value">${school.gradeLevels || 'K-12'}</span>
                        </div>
                    </div>
                </div>

                <div class="funding-progress">
                    <div class="progress-header">
                        <span class="progress-label">Community Support</span>
                        <span class="progress-amount">$${school.totalFundsReceived.toLocaleString()}</span>
                    </div>
                    <div class="progress-bar">
                        <div class="progress-fill" style="width: ${Math.min(fundingProgress, 100)}%"></div>
                    </div>
                    <div class="progress-footer">
                        <span class="supporters-count">${school.supportersCount || 0} supporters</span>
                        <span class="funding-goal">Goal: $${school.fundingGoal?.toLocaleString() || '50,000'}</span>
                    </div>
                </div>

                <div class="school-impact-preview">
                    <h4>Recent Impact</h4>
                    <ul class="impact-list">
                        ${this.renderSchoolImpactItems(school)}
                    </ul>
                </div>

                <div class="school-card-actions">
                    <button class="select-school-btn ${isSelected ? 'selected' : ''}" 
                            data-school-id="${school.id}">
                        ${isSelected ? 'Selected' : 'Select School'}
                    </button>
                    <button class="view-details-btn" 
                            onclick="window.schoolIntegration?.showSchoolDetails('${school.id}')">
                        View Details
                    </button>
                </div>
            </div>
        `;
    }

    renderSchoolImpactItems(school) {
        const impacts = school.recentImpacts || [
            'New computer lab equipment',
            'Science program funding',
            'Teacher training workshops'
        ];

        return impacts.slice(0, 3).map(impact => `<li>${impact}</li>`).join('');
    }

    renderNoSchoolsFound() {
        return `
            <div class="no-schools-found">
                <div class="no-schools-icon">🏫</div>
                <h3>No Schools Found</h3>
                <p>Try adjusting your search criteria or browse all available schools.</p>
                <button class="clear-filters-btn" onclick="window.schoolIntegration?.clearFilters()">
                    Clear All Filters
                </button>
            </div>
        `;
    }

    selectSchool(schoolId) {
        const school = this.schools.find(s => s.id === schoolId);
        if (!school) return;

        this.selectedSchool = school;
        this.updateSelectedSchoolDisplay();
        this.updateImpactCalculator();
        this.renderSchoolGrid(); // Re-render to show selection

        // Save selection to localStorage for persistence
        localStorage.setItem('selectedSchool', JSON.stringify(school));

        this.showSuccessMessage(`Selected ${school.name} as your beneficiary school`);

        // Trigger callback to update staking dashboard
        if (this.stakingDashboard && typeof this.stakingDashboard.onSchoolSelected === 'function') {
            this.stakingDashboard.onSchoolSelected(school);
        }
    }

    updateSelectedSchoolDisplay() {
        const container = document.getElementById('selected-school-info');
        if (!container || !this.selectedSchool) {
            container.style.display = 'none';
            return;
        }

        container.style.display = 'block';
        container.innerHTML = this.renderSelectedSchoolInfo();
    }

    renderSelectedSchoolInfo() {
        const school = this.selectedSchool;
        if (!school) return '';

        return `
            <div class="selected-school-banner">
                <div class="selected-school-content">
                    <div class="selected-school-header">
                        <h3>Your Selected School</h3>
                        <button class="change-school-btn" onclick="window.schoolIntegration?.changeSchool()">
                            Change School
                        </button>
                    </div>
                    
                    <div class="selected-school-details">
                        <div class="school-main-info">
                            <h4>${school.name}</h4>
                            <p>${school.location}</p>
                            <p class="school-description">${school.description}</p>
                        </div>
                        
                        <div class="school-impact-stats">
                            <div class="stat-item">
                                <span class="stat-value">${school.studentsCount.toLocaleString()}</span>
                                <span class="stat-label">Students Supported</span>
                            </div>
                            <div class="stat-item">
                                <span class="stat-value">$${school.totalFundsReceived.toLocaleString()}</span>
                                <span class="stat-label">Total Funds Received</span>
                            </div>
                            <div class="stat-item">
                                <span class="stat-value">${school.supportersCount || 0}</span>
                                <span class="stat-label">Community Supporters</span>
                            </div>
                        </div>
                    </div>
                </div>
            </div>
        `;
    }

    updateImpactCalculator() {
        const container = document.getElementById('impact-calculator');
        if (!container) return;

        const stakingAmount = this.getCurrentStakingAmount();
        const projectedReward = this.calculateProjectedReward(stakingAmount);
        const schoolShare = projectedReward * 0.5;

        container.innerHTML = this.renderImpactCalculation(stakingAmount, schoolShare);
    }

    renderImpactCalculation(stakingAmount, schoolShare) {
        if (!this.selectedSchool || stakingAmount <= 0) {
            return `
                <div class="impact-placeholder">
                    <p>Select a school and enter a staking amount to see your projected impact</p>
                </div>
            `;
        }

        const monthlyImpact = schoolShare / 12;
        const costPerStudent = 50; // Estimated monthly cost per student
        const studentsSupported = Math.floor(monthlyImpact / costPerStudent);

        return `
            <div class="impact-calculation">
                <div class="impact-summary">
                    <h4>Your Annual Impact on ${this.selectedSchool.name}</h4>
                    <div class="impact-breakdown">
                        <div class="breakdown-item">
                            <span class="breakdown-label">Your Staking Amount:</span>
                            <span class="breakdown-value">${stakingAmount.toLocaleString()} TEACH</span>
                        </div>
                        <div class="breakdown-item">
                            <span class="breakdown-label">Annual Rewards (50% to school):</span>
                            <span class="breakdown-value highlight">$${schoolShare.toLocaleString()}</span>
                        </div>
                        <div class="breakdown-item">
                            <span class="breakdown-label">Students Directly Supported:</span>
                            <span class="breakdown-value highlight">${studentsSupported} students</span>
                        </div>
                    </div>
                </div>

                <div class="impact-visualization-chart">
                    <h5>50/50 Reward Distribution</h5>
                    <div class="reward-split-visual">
                        <div class="split-section user-split">
                            <div class="split-amount">$${schoolShare.toLocaleString()}</div>
                            <div class="split-label">Your Share</div>
                        </div>
                        <div class="split-divider">
                            <div class="split-icon">🤝</div>
                        </div>
                        <div class="split-section school-split">
                            <div class="split-amount">$${schoolShare.toLocaleString()}</div>
                            <div class="split-label">${this.selectedSchool.name}</div>
                        </div>
                    </div>
                </div>

                <div class="impact-timeline">
                    <h5>Impact Timeline</h5>
                    <div class="timeline-items">
                        <div class="timeline-item">
                            <span class="timeline-period">Monthly</span>
                            <span class="timeline-impact">$${monthlyImpact.toFixed(0)} → ${Math.ceil(studentsSupported / 12)} students</span>
                        </div>
                        <div class="timeline-item">
                            <span class="timeline-period">Quarterly</span>
                            <span class="timeline-impact">$${(monthlyImpact * 3).toFixed(0)} → ${Math.ceil(studentsSupported / 4)} students</span>
                        </div>
                        <div class="timeline-item">
                            <span class="timeline-period">Annually</span>
                            <span class="timeline-impact">$${schoolShare.toFixed(0)} → ${studentsSupported} students</span>
                        </div>
                    </div>
                </div>
            </div>
        `;
    }

    setupEventListeners() {
        // Search input
        const searchInput = document.getElementById('school-search');
        if (searchInput) {
            searchInput.addEventListener('input', this.searchSchools);
        }

        // Filter dropdowns
        const filters = ['location-filter', 'student-count-filter', 'funding-need-filter'];
        filters.forEach(filterId => {
            const filter = document.getElementById(filterId);
            if (filter) {
                filter.addEventListener('change', this.updateFilters);
            }
        });

        // School card clicks
        document.addEventListener('click', (e) => {
            if (e.target.matches('.select-school-btn')) {
                e.stopPropagation();
                const schoolId = e.target.dataset.schoolId;
                this.selectSchool(schoolId);
            }
        });
    }

    searchSchools(e) {
        this.searchTerm = e.target.value.toLowerCase();
        this.renderSchoolGrid();
    }

    updateFilters(e) {
        const filterId = e.target.id;
        const value = e.target.value;

        switch (filterId) {
            case 'location-filter':
                this.filters.location = value;
                break;
            case 'student-count-filter':
                this.filters.studentCount = value;
                break;
            case 'funding-need-filter':
                this.filters.fundingNeed = value;
                break;
        }

        this.renderSchoolGrid();
    }

    getFilteredSchools() {
        return this.schools.filter(school => {
            // Search term filter
            if (this.searchTerm &&
                !school.name.toLowerCase().includes(this.searchTerm) &&
                !school.location.toLowerCase().includes(this.searchTerm)) {
                return false;
            }

            // Location filter
            if (this.filters.location) {
                const schoolLocation = this.getSchoolLocationType(school);
                if (schoolLocation !== this.filters.location) {
                    return false;
                }
            }

            // Student count filter
            if (this.filters.studentCount) {
                const studentCount = school.studentsCount;
                switch (this.filters.studentCount) {
                    case 'small':
                        if (studentCount >= 300) return false;
                        break;
                    case 'medium':
                        if (studentCount < 300 || studentCount > 800) return false;
                        break;
                    case 'large':
                        if (studentCount <= 800) return false;
                        break;
                }
            }

            // Funding need filter
            if (this.filters.fundingNeed) {
                const fundingLevel = this.getSchoolFundingLevel(school);
                if (fundingLevel !== this.filters.fundingNeed) {
                    return false;
                }
            }

            return true;
        });
    }

    getSchoolLocationType(school) {
        // Simple heuristic based on location string
        const location = school.location.toLowerCase();
        if (location.includes('rural') || location.includes('county')) return 'rural';
        if (location.includes('urban') || location.includes('city')) return 'urban';
        return 'suburban';
    }

    getSchoolFundingLevel(school) {
        const fundingRatio = school.totalFundsReceived / (school.fundingGoal || 50000);
        if (fundingRatio < 0.3) return 'high';
        if (fundingRatio < 0.7) return 'medium';
        return 'low';
    }

    calculateSchoolImpactLevel(school) {
        const studentsPerDollar = school.studentsCount / (school.totalFundsReceived || 1);
        if (studentsPerDollar > 0.1) return 'High';
        if (studentsPerDollar > 0.05) return 'Medium';
        return 'Standard';
    }

    getCurrentStakingAmount() {
        const amountInput = document.getElementById('stake-amount');
        return amountInput ? parseFloat(amountInput.value) || 0 : 10000; // Default for calculation
    }

    calculateProjectedReward(stakingAmount) {
        // Get current pool APY (simplified calculation)
        const currentApy = 0.08; // 8% default
        return stakingAmount * currentApy;
    }

    animateSchoolCards() {
        const cards = document.querySelectorAll('.school-card');
        cards.forEach((card, index) => {
            setTimeout(() => {
                card.style.animation = 'slideInUp 0.6s ease-out forwards';
            }, index * 100);
        });
    }

    showSchoolDetails(schoolId) {
        const school = this.schools.find(s => s.id === schoolId);
        if (!school) return;

        // Implementation for detailed school modal would go here
        console.log('Show details for school:', school);
    }

    changeSchool() {
        this.selectedSchool = null;
        localStorage.removeItem('selectedSchool');
        this.updateSelectedSchoolDisplay();
        this.updateImpactCalculator();
        this.renderSchoolGrid();
    }

    clearFilters() {
        this.searchTerm = '';
        this.filters = { location: '', studentCount: '', fundingNeed: '' };

        // Reset form elements
        const searchInput = document.getElementById('school-search');
        if (searchInput) searchInput.value = '';

        const filterSelects = document.querySelectorAll('.filter-select');
        filterSelects.forEach(select => select.value = '');

        this.renderSchoolGrid();
    }

    calculateImpact(stakingAmount, period) {
        if (!this.selectedSchool || !stakingAmount) return null;

        const apy = 0.08; // 8% APY
        const totalReward = stakingAmount * apy * (period / 365);
        const schoolShare = totalReward * 0.5;
        const costPerStudent = 50; // Monthly cost per student
        const studentsSupported = Math.floor((schoolShare / 12) / costPerStudent);

        return {
            totalReward,
            schoolShare,
            studentsSupported,
            monthlyImpact: schoolShare / 12
        };
    }

    loadSelectedSchool() {
        try {
            const savedSchool = localStorage.getItem('selectedSchool');
            if (savedSchool) {
                this.selectedSchool = JSON.parse(savedSchool);
                this.updateSelectedSchoolDisplay();
                this.updateImpactCalculator();
            }
        } catch (error) {
            console.error('Error loading saved school:', error);
        }
    }

    getFallbackSchools() {
        return [
            {
                id: 'lincoln-elementary',
                name: 'Lincoln Elementary School',
                location: 'Springfield, IL',
                description: 'Supporting STEM education for underserved communities with innovative programs.',
                studentsCount: 450,
                totalFundsReceived: 15000,
                fundingGoal: 50000,
                supportersCount: 32,
                gradeLevels: 'K-5',
                recentImpacts: [
                    'New computer lab with 20 tablets',
                    'After-school coding program',
                    'Teacher professional development'
                ]
            },
            {
                id: 'jefferson-middle',
                name: 'Jefferson Middle School',
                location: 'Madison, WI',
                description: 'Enhancing technology access and digital literacy for rural students.',
                studentsCount: 280,
                totalFundsReceived: 8500,
                fundingGoal: 35000,
                supportersCount: 18,
                gradeLevels: '6-8',
                recentImpacts: [
                    'Internet connectivity upgrade',
                    'Science lab equipment',
                    'Library book expansion'
                ]
            }
        ];
    }

    showSuccessMessage(message) {
        // Integration with main dashboard messaging system
        if (this.stakingDashboard && typeof this.stakingDashboard.showSuccessMessage === 'function') {
            this.stakingDashboard.showSuccessMessage(message);
        }
    }

    // Public method to get current selection
    getSelectedSchool() {
        return this.selectedSchool;
    }

    // Public method to refresh school data
    async refreshSchools() {
        await this.loadSchools();
    }
}

// Initialize school integration when DOM is ready
document.addEventListener('DOMContentLoaded', () => {
    // Wait for main staking dashboard to be ready
    if (window.stakingDashboard) {
        window.schoolIntegration = new SchoolIntegrationManager(window.stakingDashboard);

        // Load previously selected school
        window.schoolIntegration.loadSelectedSchool();

        // Set up callback for staking amount changes
        window.stakingDashboard.onSchoolSelected = (school) => {
            console.log('School selected callback:', school);
        };
    }
});