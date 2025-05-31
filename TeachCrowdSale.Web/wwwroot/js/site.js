// TeacherSupport Platform - Home Page JavaScript
'use strict';

// ================================================
// GLOBAL CONFIGURATION
// ================================================
const CONFIG = {
    BASE_URL: '', // Web controller endpoints
    ANIMATION_DURATION: 1000,
    UPDATE_INTERVAL: 30000, // 30 seconds
    COUNTER_ANIMATION_SPEED: 2000,
    CHART_THEME: 'Material3Dark'
};

// Global state management
const AppState = {
    isLoaded: false,
    apiData: {},
    charts: {},
    animationQueues: []
};

// ================================================
// UTILITY FUNCTIONS
// ================================================
const Utils = {
    // Format numbers with appropriate suffixes
    formatNumber(num, decimals = 2) {
        if (num === null || num === undefined) return '0';

        const absNum = Math.abs(num);

        if (absNum >= 1e9) {
            return (num / 1e9).toFixed(decimals) + 'B';
        } else if (absNum >= 1e6) {
            return (num / 1e6).toFixed(decimals) + 'M';
        } else if (absNum >= 1e3) {
            return (num / 1e3).toFixed(decimals) + 'K';
        }

        return num.toLocaleString(undefined, {
            minimumFractionDigits: 0,
            maximumFractionDigits: decimals
        });
    },

    // Format currency values
    formatCurrency(amount, currency = 'USD', decimals = 4) {
        if (amount === null || amount === undefined) return '$0.00';

        return new Intl.NumberFormat('en-US', {
            style: 'currency',
            currency: currency,
            minimumFractionDigits: decimals,
            maximumFractionDigits: decimals
        }).format(amount);
    },

    // Format percentage
    formatPercentage(value, decimals = 1) {
        if (value === null || value === undefined) return '0%';
        return `${value.toFixed(decimals)}%`;
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

    // Smooth scroll to element
    scrollToElement(elementId, offset = 80) {
        const element = document.getElementById(elementId);
        if (element) {
            const elementPosition = element.getBoundingClientRect().top;
            const offsetPosition = elementPosition + window.pageYOffset - offset;

            window.scrollTo({
                top: offsetPosition,
                behavior: 'smooth'
            });
        }
    },

    // Navigate to buy page
    navigateToBuyPage(tierId = null) {
        const url = tierId ? `/buy?tier=${tierId}` : '/buy';
        window.location.href = url;
    },

    // Check if element is in viewport
    isInViewport(element, threshold = 0.1) {
        const rect = element.getBoundingClientRect();
        const windowHeight = window.innerHeight || document.documentElement.clientHeight;

        return (
            rect.top <= windowHeight * (1 - threshold) &&
            rect.bottom >= windowHeight * threshold
        );
    }
};

// ================================================
// API SERVICE (Web Controller Endpoints)
// ================================================
const ApiService = {
    // Generic API call wrapper
    async makeRequest(endpoint, options = {}) {
        try {
            const response = await fetch(`${CONFIG.BASE_URL}${endpoint}`, {
                headers: {
                    'Content-Type': 'application/json',
                    ...options.headers
                },
                ...options
            });

            if (!response.ok) {
                throw new Error(`HTTP error! status: ${response.status}`);
            }

            return await response.json();
        } catch (error) {
            console.error(`Web API request failed for ${endpoint}:`, error);
            return null;
        }
    },

    // Fetch live statistics
    async getLiveStats() {
        return await this.makeRequest('/Home/GetLiveStats');
    },

    // Fetch tier data
    async getTierData() {
        return await this.makeRequest('/Home/GetTierData');
    },

    // Fetch contract information
    async getContractInfo() {
        return await this.makeRequest('/Home/GetContractInfo');
    },

    // Fetch investment highlights
    async getInvestmentHighlights() {
        return await this.makeRequest('/Home/GetInvestmentHighlights');
    },

    // Health check
    async checkHealth() {
        return await this.makeRequest('/Home/HealthCheck');
    }
};

// ================================================
// ANIMATION CONTROLLER
// ================================================
const AnimationController = {
    // Counter animation for numbers
    animateCounter(element, targetValue, duration = CONFIG.COUNTER_ANIMATION_SPEED, formatter = null) {
        const startValue = 0;
        const startTime = performance.now();

        const animate = (currentTime) => {
            const elapsed = currentTime - startTime;
            const progress = Math.min(elapsed / duration, 1);

            // Easing function (ease-out)
            const easeOut = 1 - Math.pow(1 - progress, 3);
            const currentValue = startValue + (targetValue - startValue) * easeOut;

            if (formatter && typeof formatter === 'function') {
                element.textContent = formatter(currentValue);
            } else {
                element.textContent = Utils.formatNumber(currentValue);
            }

            if (progress < 1) {
                requestAnimationFrame(animate);
            } else {
                element.textContent = formatter ? formatter(targetValue) : Utils.formatNumber(targetValue);
            }
        };

        requestAnimationFrame(animate);
    },

    // Progress bar animation
    animateProgressBar(element, targetPercentage, duration = 1500) {
        element.style.width = '0%';

        setTimeout(() => {
            element.style.transition = `width ${duration}ms ease-out`;
            element.style.width = `${targetPercentage}%`;
        }, 100);
    },

    // Fade in animation for elements
    fadeInElements(selector, delay = 0, stagger = 200) {
        const elements = document.querySelectorAll(selector);
        elements.forEach((element, index) => {
            element.style.opacity = '0';
            element.style.transform = 'translateY(30px)';

            setTimeout(() => {
                element.style.transition = 'opacity 0.8s ease-out, transform 0.8s ease-out';
                element.style.opacity = '1';
                element.style.transform = 'translateY(0)';
            }, delay + (index * stagger));
        });
    },

    // Initialize intersection observer for scroll animations
    initScrollAnimations() {
        const observer = new IntersectionObserver((entries) => {
            entries.forEach(entry => {
                if (entry.isIntersecting) {
                    entry.target.classList.add('animate-fade-in');

                    // Trigger specific animations based on element type
                    if (entry.target.classList.contains('metric-card')) {
                        this.animateMetricCard(entry.target);
                    } else if (entry.target.classList.contains('tier-card')) {
                        this.animateTierCard(entry.target);
                    }
                }
            });
        }, { threshold: 0.2 });

        // Observe elements
        document.querySelectorAll('.metric-card, .tier-card, .chart-container').forEach(el => {
            observer.observe(el);
        });
    },

    // Animate metric cards
    animateMetricCard(card) {
        const valueElement = card.querySelector('.metric-value');
        const progressBar = card.querySelector('.progress-fill');

        if (valueElement) {
            const targetValue = parseFloat(valueElement.dataset.value || valueElement.textContent.replace(/[^\d.-]/g, ''));
            const formatter = valueElement.dataset.formatter;

            let formatterFunc = null;
            if (formatter === 'currency') {
                formatterFunc = (val) => Utils.formatCurrency(val);
            } else if (formatter === 'percentage') {
                formatterFunc = (val) => Utils.formatPercentage(val);
            } else if (formatter === 'number') {
                formatterFunc = (val) => Utils.formatNumber(val);
            }

            this.animateCounter(valueElement, targetValue, CONFIG.COUNTER_ANIMATION_SPEED, formatterFunc);
        }

        if (progressBar) {
            const targetPercentage = parseFloat(progressBar.dataset.percentage || 0);
            this.animateProgressBar(progressBar, targetPercentage);
        }
    },

    // Animate tier cards
    animateTierCard(card) {
        const progressBar = card.querySelector('.progress-fill');
        if (progressBar) {
            const targetPercentage = parseFloat(progressBar.dataset.percentage || 0);
            this.animateProgressBar(progressBar, targetPercentage);
        }
    }
};

// ================================================
// CHART MANAGEMENT
// ================================================
const ChartManager = {
    // Initialize all charts
    async initializeCharts() {
        await this.createTokenDistributionChart();
        await this.createTierProgressChart();
        await this.createPriceHistoryChart();
    },

    // Token distribution donut chart
    async createTokenDistributionChart() {
        const chartContainer = document.getElementById('tokenDistributionChart');
        if (!chartContainer) return;

        const data = [
            { category: 'Public Presale', value: 25, color: '#4f46e5' },
            { category: 'Community Incentives', value: 24, color: '#06b6d4' },
            { category: 'Platform Ecosystem', value: 20, color: '#8b5cf6' },
            { category: 'Initial Liquidity', value: 12, color: '#10b981' },
            { category: 'Team & Development', value: 8, color: '#f59e0b' },
            { category: 'Educational Partners', value: 7, color: '#ec4899' },
            { category: 'Reserve Fund', value: 4, color: '#ef4444' }
        ];

        // Create Syncfusion donut chart
        const chart = new ej.charts.AccumulationChart({
            series: [{
                dataSource: data,
                xName: 'category',
                yName: 'value',
                innerRadius: '40%',
                dataLabel: {
                    visible: true,
                    name: 'category',
                    position: 'Outside',
                    font: { color: '#ffffff', size: '12px' }
                },
                palettes: data.map(item => item.color)
            }],
            theme: CONFIG.CHART_THEME,
            background: 'transparent',
            legendSettings: {
                visible: true,
                position: 'Bottom',
                textStyle: { color: '#ffffff' }
            },
            tooltip: {
                enable: true,
                format: '${point.x}: ${point.y}%'
            }
        });

        chart.appendTo(chartContainer);
        AppState.charts.tokenDistribution = chart;
    },

    // Tier progress chart
    async createTierProgressChart() {
        const chartContainer = document.getElementById('tierProgressChart');
        if (!chartContainer) return;

        try {
            const tiersData = await ApiService.getTierData();
            if (!tiersData || tiersData.error) return;

            const data = tiersData.map(tier => ({
                tier: tier.name || `Tier ${tier.id}`,
                sold: tier.sold || 0,
                remaining: (tier.totalAllocation || 0) - (tier.sold || 0),
                percentage: tier.totalAllocation > 0 ? ((tier.sold / tier.totalAllocation) * 100) : 0
            }));

            const chart = new ej.charts.Chart({
                primaryXAxis: {
                    valueType: 'Category',
                    labelStyle: { color: '#ffffff' },
                    lineStyle: { color: '#333333' }
                },
                primaryYAxis: {
                    labelStyle: { color: '#ffffff' },
                    lineStyle: { color: '#333333' },
                    majorGridLines: { color: '#333333' }
                },
                series: [{
                    type: 'StackingColumn',
                    dataSource: data,
                    xName: 'tier',
                    yName: 'sold',
                    name: 'Sold',
                    fill: '#4f46e5'
                }, {
                    type: 'StackingColumn',
                    dataSource: data,
                    xName: 'tier',
                    yName: 'remaining',
                    name: 'Remaining',
                    fill: '#1e2235'
                }],
                theme: CONFIG.CHART_THEME,
                background: 'transparent',
                legendSettings: {
                    visible: true,
                    textStyle: { color: '#ffffff' }
                },
                tooltip: {
                    enable: true
                }
            });

            chart.appendTo(chartContainer);
            AppState.charts.tierProgress = chart;
        } catch (error) {
            console.error('Error creating tier progress chart:', error);
        }
    },

    // Price history line chart (mock data for now)
    async createPriceHistoryChart() {
        const chartContainer = document.getElementById('priceHistoryChart');
        if (!chartContainer) return;

        // Mock price data - in real implementation, fetch from API
        const data = [
            { date: new Date('2024-01-01'), price: 0.04 },
            { date: new Date('2024-02-01'), price: 0.045 },
            { date: new Date('2024-03-01'), price: 0.05 },
            { date: new Date('2024-04-01'), price: 0.055 },
            { date: new Date('2024-05-01'), price: 0.06 },
            { date: new Date('2024-06-01'), price: 0.065 }
        ];

        const chart = new ej.charts.Chart({
            primaryXAxis: {
                valueType: 'DateTime',
                labelStyle: { color: '#ffffff' },
                lineStyle: { color: '#333333' }
            },
            primaryYAxis: {
                labelFormat: '${value}',
                labelStyle: { color: '#ffffff' },
                lineStyle: { color: '#333333' },
                majorGridLines: { color: '#333333' }
            },
            series: [{
                type: 'Line',
                dataSource: data,
                xName: 'date',
                yName: 'price',
                name: 'TEACH Price',
                fill: '#4f46e5',
                width: 3,
                marker: {
                    visible: true,
                    fill: '#4f46e5',
                    border: { color: '#ffffff', width: 2 }
                }
            }],
            theme: CONFIG.CHART_THEME,
            background: 'transparent',
            tooltip: {
                enable: true,
                format: 'Date: ${point.x}<br/>Price: ${point.y}'
            }
        });

        chart.appendTo(chartContainer);
        AppState.charts.priceHistory = chart;
    }
};

// ================================================
// DATA MANAGEMENT
// ================================================
const DataManager = {
    // Load all initial data
    async loadInitialData() {
        try {
            // Check if we have initial data from server
            if (window.initialData) {
                AppState.apiData = window.initialData;
                this.updateUI();
                return true;
            }

            // Otherwise fetch from endpoints
            const [liveStats, tierData, contractInfo, highlights] = await Promise.all([
                ApiService.getLiveStats(),
                ApiService.getTierData(),
                ApiService.getContractInfo(),
                ApiService.getInvestmentHighlights()
            ]);

            AppState.apiData = {
                liveStats,
                tierData,
                contractInfo,
                highlights
            };

            this.updateUI();
            return true;
        } catch (error) {
            console.error('Error loading initial data:', error);
            this.showFallbackData();
            return false;
        }
    },

    // Update UI with loaded data
    updateUI() {
        this.updateHeroStats();
        this.updateMetricCards();
        this.updateTierCards();
    },

    // Update hero section statistics
    updateHeroStats() {
        const { liveStats } = AppState.apiData;

        if (liveStats && !liveStats.error) {
            this.updateElement('[data-stat="totalRaised"]', liveStats.totalRaised, 'currency');
            this.updateElement('[data-stat="tokensSold"]', liveStats.tokensSold, 'number');
            this.updateElement('[data-stat="participants"]', liveStats.participantsCount, 'number');
            this.updateElement('[data-stat="currentPrice"]', liveStats.tokenPrice, 'currency');
            this.updateElement('[data-stat="marketCap"]', liveStats.marketCap, 'currency');
            this.updateElement('[data-stat="holders"]', liveStats.holdersCount, 'number');
        }
    },

    // Update metric cards
    updateMetricCards() {
        const { liveStats } = AppState.apiData;

        if (liveStats && !liveStats.error) {
            this.updateElement('[data-metric="currentTierPrice"]', liveStats.currentTierPrice, 'currency');
            this.updateElement('[data-metric="currentTierName"]', liveStats.currentTierName, 'text');
            this.updateProgressBar('[data-progress="currentTier"]', liveStats.currentTierProgress);

            this.updateElement('[data-metric="totalRaised"]', liveStats.totalRaised, 'currency');

            // Calculate funding progress if we have both values
            const fundingGoal = 87500000; // From fallback data
            const fundingProgress = liveStats.totalRaised > 0 ?
                (liveStats.totalRaised / fundingGoal) * 100 : 0;
            this.updateProgressBar('[data-progress="funding"]', fundingProgress);
        }
    },

    // Update tier cards
    updateTierCards() {
        const { tierData } = AppState.apiData;

        if (tierData && Array.isArray(tierData) && !tierData.error) {
            tierData.forEach((tier) => {
                const tierCard = document.querySelector(`[data-tier="${tier.id}"]`);
                if (tierCard) {
                    this.updateElement(tierCard.querySelector('[data-tier-price]'), tier.price, 'currency');
                    this.updateElement(tierCard.querySelector('[data-tier-sold]'), tier.sold, 'number');
                    this.updateElement(tierCard.querySelector('[data-tier-remaining]'), tier.remaining, 'number');

                    const progressBar = tierCard.querySelector('.progress-fill');
                    if (progressBar) {
                        progressBar.dataset.percentage = tier.progress;
                    }

                    if (tier.isActive) {
                        tierCard.classList.add('active');
                    }
                }
            });
        }
    },

    // Update individual element
    updateElement(selector, value, type = 'text') {
        const element = typeof selector === 'string' ? document.querySelector(selector) : selector;
        if (!element || value === null || value === undefined) return;

        element.dataset.value = value;
        element.dataset.formatter = type;

        switch (type) {
            case 'currency':
                element.textContent = Utils.formatCurrency(value);
                break;
            case 'number':
                element.textContent = Utils.formatNumber(value);
                break;
            case 'percentage':
                element.textContent = Utils.formatPercentage(value);
                break;
            default:
                element.textContent = value;
        }
    },

    // Update progress bar
    updateProgressBar(selector, percentage) {
        const progressBar = document.querySelector(selector);
        if (progressBar) {
            progressBar.dataset.percentage = Math.min(percentage, 100);
        }
    },

    // Show fallback data when API fails
    showFallbackData() {
        console.warn('Using fallback data due to API failure');

        // Set fallback values
        this.updateElement('[data-stat="totalRaised"]', 12500000, 'currency');
        this.updateElement('[data-stat="tokensSold"]', 750000000, 'number');
        this.updateElement('[data-stat="participants"]', 2847, 'number');
        this.updateElement('[data-stat="currentPrice"]', 0.065, 'currency');

        // Show warning message
        this.showNotification('Using cached data - some information may not be current', 'warning');
    },

    // Show notification to user
    showNotification(message, type = 'info') {
        const notification = document.createElement('div');
        notification.className = `notification notification-${type}`;
        notification.textContent = message;
        notification.style.cssText = `
            position: fixed;
            top: 20px;
            right: 20px;
            padding: 15px 20px;
            background: var(--warning-color);
            color: white;
            border-radius: 8px;
            z-index: 10000;
            opacity: 0;
            transform: translateX(100%);
            transition: all 0.3s ease;
        `;

        document.body.appendChild(notification);

        // Animate in
        setTimeout(() => {
            notification.style.opacity = '1';
            notification.style.transform = 'translateX(0)';
        }, 100);

        // Auto remove
        setTimeout(() => {
            notification.style.opacity = '0';
            notification.style.transform = 'translateX(100%)';
            setTimeout(() => notification.remove(), 300);
        }, 5000);
    }
};

// ================================================
// EVENT HANDLERS
// ================================================
const EventHandlers = {
    // Initialize all event listeners
    init() {
        this.setupNavigationHandlers();
        this.setupScrollHandlers();
        this.setupButtonHandlers();
        this.setupResizeHandler();
    },

    // Navigation event handlers
    setupNavigationHandlers() {
        // Smooth scrolling for navigation links
        document.querySelectorAll('a[href^="#"]').forEach(link => {
            link.addEventListener('click', (e) => {
                e.preventDefault();
                const targetId = link.getAttribute('href').substring(1);
                Utils.scrollToElement(targetId);
            });
        });

        // Mobile menu toggle (if implemented)
        const mobileToggle = document.querySelector('.mobile-menu-toggle');
        if (mobileToggle) {
            mobileToggle.addEventListener('click', () => {
                document.querySelector('.nav-menu').classList.toggle('active');
            });
        }
    },

    // Scroll event handlers
    setupScrollHandlers() {
        const navbar = document.querySelector('.navbar');

        const handleScroll = Utils.debounce(() => {
            if (window.scrollY > 100) {
                navbar?.classList.add('scrolled');
            } else {
                navbar?.classList.remove('scrolled');
            }
        }, 10);

        window.addEventListener('scroll', handleScroll);
    },

    // Button event handlers
    setupButtonHandlers() {
        // CTA button handlers
        document.querySelectorAll('[data-action]').forEach(button => {
            button.addEventListener('click', (e) => {
                const action = button.dataset.action;
                this.handleButtonAction(action, e);
            });
        });
    },

    // Handle button actions
    handleButtonAction(action, event) {
        switch (action) {
            case 'buy-tokens':
                const tierId = event.target.dataset.tierId;
                Utils.navigateToBuyPage(tierId);
                break;
            case 'view-tokenomics':
                Utils.scrollToElement('tokenomics');
                break;
            case 'connect-wallet':
                this.handleWalletConnection();
                break;
            case 'view-tier':
                const tierIdView = event.target.dataset.tierId;
                this.showTierDetails(tierIdView);
                break;
            default:
                console.log(`Action not implemented: ${action}`);
        }
    },

    // Handle wallet connection (placeholder)
    handleWalletConnection() {
        DataManager.showNotification('Wallet connection coming soon!', 'info');
    },

    // Show tier details (placeholder)
    showTierDetails(tierId) {
        console.log(`Showing details for tier: ${tierId}`);
    },

    // Resize handler for responsive charts
    setupResizeHandler() {
        const handleResize = Utils.debounce(() => {
            Object.values(AppState.charts).forEach(chart => {
                if (chart && chart.refresh) {
                    chart.refresh();
                }
            });
        }, 300);

        window.addEventListener('resize', handleResize);
    }
};

// ================================================
// APPLICATION INITIALIZATION
// ================================================
const App = {
    async init() {
        try {
            console.log('Initializing TeacherSupport Platform...');

            // Initialize event handlers
            EventHandlers.init();

            // Load initial data
            await DataManager.loadInitialData();

            // Initialize animations
            AnimationController.initScrollAnimations();

            // Initialize charts after a short delay
            setTimeout(async () => {
                await ChartManager.initializeCharts();
            }, 500);

            // Start periodic data updates
            this.startPeriodicUpdates();

            // Trigger initial animations
            setTimeout(() => {
                AnimationController.fadeInElements('.hero-stat', 200, 100);
            }, 1000);

            AppState.isLoaded = true;
            console.log('TeacherSupport Platform initialized successfully');

        } catch (error) {
            console.error('Failed to initialize application:', error);
            DataManager.showNotification('Application failed to load completely', 'error');
        }
    },

    // Start periodic data updates
    startPeriodicUpdates() {
        setInterval(async () => {
            if (document.visibilityState === 'visible') {
                console.log('Updating data...');
                await DataManager.loadInitialData();
            }
        }, CONFIG.UPDATE_INTERVAL);
    }
};

// ================================================
// DOCUMENT READY & INITIALIZATION
// ================================================
document.addEventListener('DOMContentLoaded', () => {
    App.init();
});

// Handle page visibility changes
document.addEventListener('visibilitychange', () => {
    if (document.visibilityState === 'visible' && AppState.isLoaded) {
        // Refresh data when page becomes visible
        DataManager.loadInitialData();
    }
});

// Export for global access if needed
window.TeacherSupportApp = {
    Utils,
    ApiService,
    AnimationController,
    ChartManager,
    DataManager,
    AppState
};