// TeacherSupport Platform - Tokenomics Page JavaScript
'use strict';

// ================================================
// GLOBAL CONFIGURATION
// ================================================
const CONFIG = {
    BASE_URL: '/tokenomics',
    ANIMATION_DURATION: 1000,
    UPDATE_INTERVAL: 60000, // 1 minute for live metrics
    COUNTER_ANIMATION_SPEED: 2000,
    CHART_THEME: 'Material3Dark'
};

// Global state management
const TokenomicsState = {
    isLoaded: false,
    pageData: {},
    charts: {},
    animationQueues: [],
    lastUpdate: null
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
    formatCurrency(amount, currency = 'USD', decimals = 2) {
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

    // Format dates
    formatDate(date, format = 'short') {
        if (!date) return 'TBA';

        const dateObj = new Date(date);

        if (format === 'short') {
            return dateObj.toLocaleDateString('en-US', {
                month: 'short',
                day: 'numeric',
                year: 'numeric'
            });
        } else if (format === 'relative') {
            const now = new Date();
            const diffTime = dateObj - now;
            const diffDays = Math.ceil(diffTime / (1000 * 60 * 60 * 24));

            if (diffDays < 0) {
                return `${Math.abs(diffDays)} days ago`;
            } else if (diffDays === 0) {
                return 'Today';
            } else if (diffDays === 1) {
                return 'Tomorrow';
            } else {
                return `In ${diffDays} days`;
            }
        }

        return dateObj.toLocaleDateString();
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

    // Check if element is in viewport
    isInViewport(element, threshold = 0.1) {
        const rect = element.getBoundingClientRect();
        const windowHeight = window.innerHeight || document.documentElement.clientHeight;

        return (
            rect.top <= windowHeight * (1 - threshold) &&
            rect.bottom >= windowHeight * threshold
        );
    },

    // Show notification
    showNotification(message, type = 'info', duration = 5000) {
        const notification = document.createElement('div');
        notification.className = `notification notification-${type}`;
        notification.innerHTML = `
            <span class="notification-message">${message}</span>
            <button class="notification-close">&times;</button>
        `;

        notification.style.cssText = `
            position: fixed;
            top: 20px;
            right: 20px;
            padding: 15px 20px;
            background: ${type === 'error' ? 'var(--danger-color)' :
                type === 'success' ? 'var(--success-color)' :
                    type === 'warning' ? 'var(--warning-color)' : 'var(--primary-color)'};
            color: white;
            border-radius: 8px;
            z-index: 10000;
            opacity: 0;
            transform: translateX(100%);
            transition: all 0.3s ease;
            display: flex;
            align-items: center;
            gap: 1rem;
            max-width: 400px;
        `;

        document.body.appendChild(notification);

        // Animate in
        setTimeout(() => {
            notification.style.opacity = '1';
            notification.style.transform = 'translateX(0)';
        }, 100);

        // Auto remove
        const removeNotification = () => {
            notification.style.opacity = '0';
            notification.style.transform = 'translateX(100%)';
            setTimeout(() => notification.remove(), 300);
        };

        setTimeout(removeNotification, duration);

        // Manual close
        notification.querySelector('.notification-close').addEventListener('click', removeNotification);
    },

    // Get color from CSS variable
    getCSSColor(colorName) {
        return getComputedStyle(document.documentElement).getPropertyValue(colorName).trim();
    }
};

// ================================================
// API SERVICE
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
            console.error(`API request failed for ${endpoint}:`, error);
            return null;
        }
    },

    // Fetch live token metrics
    async getLiveMetrics() {
        return await this.makeRequest('/live-metrics');
    },

    // Fetch supply breakdown
    async getSupplyBreakdown() {
        return await this.makeRequest('/supply-breakdown');
    },

    // Fetch vesting schedule
    async getVestingSchedule() {
        return await this.makeRequest('/vesting-schedule');
    },

    // Fetch burn mechanics
    async getBurnMechanics() {
        return await this.makeRequest('/burn-mechanics');
    },

    // Fetch treasury analytics
    async getTreasuryAnalytics() {
        return await this.makeRequest('/treasury-analytics');
    },

    // Fetch utility features
    async getUtilityFeatures() {
        return await this.makeRequest('/utility-features');
    },

    // Fetch governance info
    async getGovernanceInfo() {
        return await this.makeRequest('/governance-info');
    },

    // Health check
    async checkHealth() {
        return await this.makeRequest('/health');
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
                    if (entry.target.classList.contains('treasury-metric')) {
                        this.animateTreasuryMetric(entry.target);
                    } else if (entry.target.classList.contains('burn-stat')) {
                        this.animateBurnStat(entry.target);
                    } else if (entry.target.classList.contains('live-metric-value')) {
                        this.animateLiveMetric(entry.target);
                    }
                }
            });
        }, { threshold: 0.2 });

        // Observe elements
        document.querySelectorAll('.treasury-metric, .burn-stat, .live-metric-value').forEach(el => {
            observer.observe(el);
        });
    },

    // Animate treasury metrics
    animateTreasuryMetric(metric) {
        const valueElement = metric.querySelector('.metric-value');
        if (valueElement) {
            const targetValue = parseFloat(valueElement.dataset.value || valueElement.textContent.replace(/[^\d.-]/g, ''));
            const isCurrency = valueElement.textContent.includes('$');

            const formatter = isCurrency ?
                (val) => Utils.formatCurrency(val, 'USD', 1) :
                (val) => Utils.formatNumber(val, 1);

            this.animateCounter(valueElement, targetValue, CONFIG.COUNTER_ANIMATION_SPEED, formatter);
        }
    },

    // Animate burn statistics
    animateBurnStat(stat) {
        const valueElement = stat.querySelector('.burn-stat-value');
        if (valueElement) {
            const targetValue = parseFloat(valueElement.dataset.value || valueElement.textContent.replace(/[^\d.-]/g, ''));
            const isPercentage = valueElement.textContent.includes('%');

            const formatter = isPercentage ?
                (val) => Utils.formatPercentage(val) :
                (val) => Utils.formatNumber(val);

            this.animateCounter(valueElement, targetValue, CONFIG.COUNTER_ANIMATION_SPEED, formatter);
        }
    },

    // Animate live metrics
    animateLiveMetric(metric) {
        const targetValue = parseFloat(metric.dataset.value || metric.textContent.replace(/[^\d.-]/g, ''));
        const isCurrency = metric.textContent.includes('$');
        const isPercentage = metric.textContent.includes('%');

        let formatter;
        if (isCurrency) {
            formatter = (val) => Utils.formatCurrency(val, 'USD', 2);
        } else if (isPercentage) {
            formatter = (val) => Utils.formatPercentage(val);
        } else {
            formatter = (val) => Utils.formatNumber(val);
        }

        this.animateCounter(metric, targetValue, CONFIG.COUNTER_ANIMATION_SPEED, formatter);
    }
};

// ================================================
// CHART MANAGEMENT
// ================================================
const ChartManager = {
    // Initialize all charts
    async initializeCharts() {
        await this.createTokenDistributionChart();
        await this.createVestingTimelineChart();
        await this.createTreasuryAllocationChart();
        await this.createBurnProjectionChart();
    },

    // Token distribution donut chart
    async createTokenDistributionChart() {
        const chartContainer = document.getElementById('tokenDistributionChart');
        if (!chartContainer) return;

        const { pageData } = TokenomicsState;
        const supplyData = pageData.SupplyBreakdown?.Allocations || this.getFallbackDistributionData();

        const data = supplyData.map(allocation => ({
            category: allocation.Category,
            value: allocation.Percentage,
            color: allocation.Color,
            tokens: allocation.TokenAmount
        }));

        const chart = new ej.charts.AccumulationChart({
            series: [{
                dataSource: data,
                xName: 'category',
                yName: 'value',
                innerRadius: '50%',
                dataLabel: {
                    visible: true,
                    name: 'category',
                    position: 'Outside',
                    font: { color: '#ffffff', size: '11px' },
                    template: '<div>${point.x}: ${point.y}%</div>'
                },
                palettes: data.map(item => item.color)
            }],
            theme: CONFIG.CHART_THEME,
            background: 'transparent',
            legendSettings: {
                visible: true,
                position: 'Bottom',
                textStyle: { color: '#ffffff', size: '12px' },
                maximumLabelWidth: 120
            },
            tooltip: {
                enable: true,
                format: '<b>${point.x}</b><br/>Allocation: ${point.y}%<br/>Tokens: ${point.tokens}'
            },
            enableAnimation: true,
            animationDuration: 1500
        });

        chart.appendTo(chartContainer);
        TokenomicsState.charts.tokenDistribution = chart;
    },

    // Vesting timeline chart
    async createVestingTimelineChart() {
        const chartContainer = document.getElementById('vestingTimelineChart');
        if (!chartContainer) return;

        try {
            const vestingData = TokenomicsState.pageData.VestingSchedule || await ApiService.getVestingSchedule();
            if (!vestingData || !vestingData.Timeline) {
                vestingData = this.getFallbackVestingData();
            }

            const data = vestingData.Timeline.map(item => ({
                date: new Date(item.Date),
                totalUnlocked: item.TotalUnlocked / 1000000, // Convert to millions
                presale: (item.CategoryBreakdown['Public Presale'] || 0) / 1000000,
                team: (item.CategoryBreakdown['Team & Development'] || 0) / 1000000,
                community: (item.CategoryBreakdown['Community Incentives'] || 0) / 1000000
            }));

            const chart = new ej.charts.Chart({
                primaryXAxis: {
                    valueType: 'DateTime',
                    labelStyle: { color: '#ffffff' },
                    lineStyle: { color: '#333333' },
                    majorGridLines: { color: '#333333' }
                },
                primaryYAxis: {
                    title: 'Tokens Unlocked (Millions)',
                    titleStyle: { color: '#ffffff' },
                    labelStyle: { color: '#ffffff' },
                    lineStyle: { color: '#333333' },
                    majorGridLines: { color: '#333333' }
                },
                series: [{
                    type: 'StackingArea',
                    dataSource: data,
                    xName: 'date',
                    yName: 'presale',
                    name: 'Public Presale',
                    fill: '#4f46e5',
                    opacity: 0.8
                }, {
                    type: 'StackingArea',
                    dataSource: data,
                    xName: 'date',
                    yName: 'team',
                    name: 'Team & Development',
                    fill: '#f59e0b',
                    opacity: 0.8
                }, {
                    type: 'StackingArea',
                    dataSource: data,
                    xName: 'date',
                    yName: 'community',
                    name: 'Community Incentives',
                    fill: '#06b6d4',
                    opacity: 0.8
                }],
                theme: CONFIG.CHART_THEME,
                background: 'transparent',
                legendSettings: {
                    visible: true,
                    textStyle: { color: '#ffffff' }
                },
                tooltip: {
                    enable: true,
                    shared: true
                },
                enableAnimation: true
            });

            chart.appendTo(chartContainer);
            TokenomicsState.charts.vestingTimeline = chart;
        } catch (error) {
            console.error('Error creating vesting timeline chart:', error);
        }
    },

    // Treasury allocation pie chart
    async createTreasuryAllocationChart() {
        const chartContainer = document.getElementById('treasuryAllocationChart');
        if (!chartContainer) return;

        const treasuryData = TokenomicsState.pageData.TreasuryAnalytics?.Allocations || this.getFallbackTreasuryData();

        const data = treasuryData.map(allocation => ({
            category: allocation.Category,
            value: allocation.Percentage,
            amount: allocation.Value,
            color: allocation.Color
        }));

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
                    font: { color: '#ffffff', size: '11px' }
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
                format: '<b>${point.x}</b><br/>Allocation: ${point.y}%<br/>Value: ${point.amount}M'
            },
            enableAnimation: true
        });

        chart.appendTo(chartContainer);
        TokenomicsState.charts.treasuryAllocation = chart;
    },

    // Burn projection line chart
    async createBurnProjectionChart() {
        const chartContainer = document.getElementById('burnProjectionChart');
        if (!chartContainer) return;

        // Generate 5-year projection data
        const data = [];
        const startDate = new Date();
        const monthlyBurn = 2083333; // ~25M tokens per year / 12 months
        let cumulativeBurn = 0;

        for (let month = 0; month < 60; month++) { // 5 years
            const date = new Date(startDate);
            date.setMonth(date.getMonth() + month);

            cumulativeBurn += monthlyBurn;

            data.push({
                date: date,
                monthlyBurn: monthlyBurn / 1000000, // Convert to millions
                cumulativeBurn: cumulativeBurn / 1000000,
                remainingSupply: (5000 - (cumulativeBurn / 1000000)) // 5B total supply
            });
        }

        const chart = new ej.charts.Chart({
            primaryXAxis: {
                valueType: 'DateTime',
                labelStyle: { color: '#ffffff' },
                lineStyle: { color: '#333333' },
                majorGridLines: { color: '#333333' }
            },
            primaryYAxis: {
                title: 'Tokens (Millions)',
                titleStyle: { color: '#ffffff' },
                labelStyle: { color: '#ffffff' },
                lineStyle: { color: '#333333' },
                majorGridLines: { color: '#333333' }
            },
            series: [{
                type: 'Line',
                dataSource: data,
                xName: 'date',
                yName: 'cumulativeBurn',
                name: 'Cumulative Burn',
                fill: '#ef4444',
                width: 3,
                marker: {
                    visible: true,
                    fill: '#ef4444',
                    border: { color: '#ffffff', width: 2 }
                }
            }, {
                type: 'Line',
                dataSource: data,
                xName: 'date',
                yName: 'remainingSupply',
                name: 'Remaining Supply',
                fill: '#10b981',
                width: 2,
                dashArray: '5,5'
            }],
            theme: CONFIG.CHART_THEME,
            background: 'transparent',
            legendSettings: {
                visible: true,
                textStyle: { color: '#ffffff' }
            },
            tooltip: {
                enable: true,
                shared: true
            },
            enableAnimation: true
        });

        chart.appendTo(chartContainer);
        TokenomicsState.charts.burnProjection = chart;
    },

    // Fallback data methods
    getFallbackDistributionData() {
        return [
            { Category: 'Public Presale', Percentage: 25, Color: '#4f46e5', TokenAmount: 1250000000 },
            { Category: 'Community Incentives', Percentage: 24, Color: '#06b6d4', TokenAmount: 1200000000 },
            { Category: 'Platform Ecosystem', Percentage: 20, Color: '#8b5cf6', TokenAmount: 1000000000 },
            { Category: 'Initial Liquidity', Percentage: 12, Color: '#10b981', TokenAmount: 600000000 },
            { Category: 'Team & Development', Percentage: 8, Color: '#f59e0b', TokenAmount: 400000000 },
            { Category: 'Educational Partners', Percentage: 7, Color: '#ec4899', TokenAmount: 350000000 },
            { Category: 'Reserve Fund', Percentage: 4, Color: '#ef4444', TokenAmount: 200000000 }
        ];
    },

    getFallbackVestingData() {
        const now = new Date();
        return {
            Timeline: [
                {
                    Date: new Date(now.getTime() + 90 * 24 * 60 * 60 * 1000), // 90 days
                    TotalUnlocked: 250000000,
                    CategoryBreakdown: { 'Public Presale': 250000000 }
                },
                {
                    Date: new Date(now.getTime() + 180 * 24 * 60 * 60 * 1000), // 180 days
                    TotalUnlocked: 500000000,
                    CategoryBreakdown: { 'Public Presale': 500000000 }
                }
            ]
        };
    },

    getFallbackTreasuryData() {
        return [
            { Category: 'Development', Percentage: 40, Value: 35, Color: '#4f46e5' },
            { Category: 'Marketing', Percentage: 20, Value: 17.5, Color: '#06b6d4' },
            { Category: 'Operations', Percentage: 20, Value: 17.5, Color: '#10b981' },
            { Category: 'Partnerships', Percentage: 10, Value: 8.75, Color: '#8b5cf6' },
            { Category: 'Reserve', Percentage: 10, Value: 8.75, Color: '#ef4444' }
        ];
    }
};

// ================================================
// DATA MANAGEMENT
// ================================================
const DataManager = {
    // Load initial page data
    async loadPageData() {
        try {
            // Use server-side data if available, otherwise fetch from API
            if (window.TokenomicsPageData && Object.keys(window.TokenomicsPageData).length > 0) {
                TokenomicsState.pageData = window.TokenomicsPageData;
            } else {
                // If no server data, we'll use fallback data
                TokenomicsState.pageData = this.getFallbackPageData();
            }

            this.updateUI();
            return true;
        } catch (error) {
            console.error('Error loading page data:', error);
            Utils.showNotification('Error loading page data. Using cached data.', 'warning');
            TokenomicsState.pageData = this.getFallbackPageData();
            this.updateUI();
            return false;
        }
    },

    // Update UI with loaded data
    updateUI() {
        const { pageData } = TokenomicsState;

        this.updateLiveMetrics(pageData.LiveMetrics);
        this.updateSupplyBreakdown(pageData.SupplyBreakdown);
        this.updateVestingSchedule(pageData.VestingSchedule);
        this.updateTreasuryAnalytics(pageData.TreasuryAnalytics);
        this.updateBurnMechanics(pageData.BurnMechanics);
        this.updateUtilityFeatures(pageData.UtilityFeatures);
        this.updateGovernanceInfo(pageData.GovernanceInfo);
        this.updateLastUpdated();
    },

    // Update live metrics
    updateLiveMetrics(metrics) {
        if (!metrics) return;

        const liveElements = {
            currentPrice: document.querySelector('[data-live="currentPrice"]'),
            marketCap: document.querySelector('[data-live="marketCap"]'),
            volume24h: document.querySelector('[data-live="volume24h"]'),
            priceChange24h: document.querySelector('[data-live="priceChange24h"]'),
            holdersCount: document.querySelector('[data-live="holdersCount"]'),
            totalValueLocked: document.querySelector('[data-live="totalValueLocked"]')
        };

        Object.entries(liveElements).forEach(([key, element]) => {
            if (element && metrics[this.toCamelCase(key)] !== undefined) {
                const value = metrics[this.toCamelCase(key)];
                element.dataset.value = value;

                if (key.includes('Price') || key.includes('Cap') || key.includes('volume') || key.includes('Locked')) {
                    element.textContent = Utils.formatCurrency(value);
                } else if (key.includes('Change')) {
                    element.textContent = Utils.formatPercentage(value);
                    element.className = value >= 0 ? 'live-metric-value positive' : 'live-metric-value negative';
                } else {
                    element.textContent = Utils.formatNumber(value, 0);
                }
            }
        });
    },

    // Update supply breakdown
    updateSupplyBreakdown(supplyData) {
        if (!supplyData) return;

        const allocationList = document.getElementById('allocationList');
        if (allocationList && supplyData.Allocations) {
            allocationList.innerHTML = supplyData.Allocations.map(allocation => `
                <div class="allocation-item">
                    <div class="allocation-info">
                        <div class="allocation-color" style="background-color: ${allocation.Color}"></div>
                        <div class="allocation-details">
                            <h5>${allocation.Category}</h5>
                            <p>${allocation.Description}</p>
                        </div>
                    </div>
                    <div class="allocation-percentage">${allocation.Percentage}%</div>
                </div>
            `).join('');
        }

        // Update supply metrics
        if (supplyData.Metrics) {
            const metrics = supplyData.Metrics;
            this.updateElement('[data-supply="maxSupply"]', metrics.MaxSupply, 'number');
            this.updateElement('[data-supply="circulatingSupply"]', metrics.CirculatingSupply, 'number');
            this.updateElement('[data-supply="lockedSupply"]', metrics.LockedSupply, 'number');
            this.updateElement('[data-supply="burnedSupply"]', metrics.BurnedSupply, 'number');
        }
    },

    // Update vesting schedule
    updateVestingSchedule(vestingData) {
        if (!vestingData) return;

        const categoriesList = document.getElementById('vestingCategoriesList');
        if (categoriesList && vestingData.Categories) {
            categoriesList.innerHTML = vestingData.Categories.map(category => `
                <div class="vesting-category-item">
                    <div class="vesting-category-header">
                        <h5 class="vesting-category-name">${category.Category}</h5>
                        <span class="vesting-category-percentage">${category.TgePercentage}% TGE</span>
                    </div>
                    <div class="vesting-category-details">
                        ${Utils.formatNumber(category.TotalTokens)} tokens • ${category.VestingMonths} months vesting
                        <br>Start: ${Utils.formatDate(category.StartDate)} • End: ${Utils.formatDate(category.EndDate)}
                    </div>
                </div>
            `).join('');
        }

        // Update vesting summary
        if (vestingData.Summary) {
            const summary = vestingData.Summary;
            this.updateElement('[data-vesting="totalVested"]', summary.TotalVestedTokens, 'number');
            this.updateElement('[data-vesting="currentlyUnlocked"]', summary.CurrentlyUnlocked, 'number');
            this.updateElement('[data-vesting="nextUnlockDate"]', Utils.formatDate(summary.NextUnlockDate, 'relative'), 'text');
            this.updateElement('[data-vesting="nextUnlockAmount"]', summary.NextUnlockAmount, 'number');
        }
    },

    // Update treasury analytics
    updateTreasuryAnalytics(treasuryData) {
        if (!treasuryData) return;

        // Update treasury overview
        if (treasuryData.Overview) {
            const overview = treasuryData.Overview;
            this.updateElement('[data-treasury="totalValue"]', overview.TotalValue, 'currency');
            this.updateElement('[data-treasury="runwayYears"]', overview.OperationalRunwayYears, 'number');
            this.updateElement('[data-treasury="monthlyBurn"]', overview.MonthlyBurnRate, 'currency');
            this.updateElement('[data-treasury="safetyFund"]', overview.SafetyFundValue, 'currency');
        }

        // Update scenarios
        const scenariosGrid = document.getElementById('scenariosGrid');
        if (scenariosGrid && treasuryData.Scenarios) {
            scenariosGrid.innerHTML = treasuryData.Scenarios.map(scenario => `
                <div class="scenario-card">
                    <div class="scenario-header">
                        <h4 class="scenario-name">${scenario.Name}</h4>
                        <span class="scenario-probability">${scenario.Probability}%</span>
                    </div>
                    <p class="scenario-description">${scenario.Description}</p>
                    <div class="scenario-impact">
                        <span>Projected Runway:</span>
                        <span class="scenario-runway">${scenario.ProjectedRunway} years</span>
                    </div>
                    <span class="scenario-severity ${scenario.Severity.toLowerCase()}">${scenario.Severity}</span>
                </div>
            `).join('');
        }
    },

    // Update burn mechanics
    updateBurnMechanics(burnData) {
        if (!burnData) return;

        // Update burn statistics
        if (burnData.Statistics) {
            const stats = burnData.Statistics;
            this.updateElement('[data-burn="totalBurned"]', stats.TotalBurned, 'number');
            this.updateElement('[data-burn="burnedPercentage"]', stats.BurnedPercentage, 'percentage');
            this.updateElement('[data-burn="estimatedAnnualBurn"]', stats.EstimatedAnnualBurn, 'number');
            this.updateElement('[data-burn="burnRate"]', stats.BurnRate, 'percentage');
        }

        // Update burn mechanisms
        const mechanismsGrid = document.getElementById('burnMechanismsGrid');
        if (mechanismsGrid && burnData.Mechanisms) {
            mechanismsGrid.innerHTML = burnData.Mechanisms.map(mechanism => `
                <div class="burn-mechanism-card">
                    <div class="burn-mechanism-header">
                        <span class="burn-mechanism-icon">${mechanism.Icon}</span>
                        <h4 class="burn-mechanism-name">${mechanism.Name}</h4>
                        <span class="burn-mechanism-status ${mechanism.IsActive ? 'active' : 'inactive'}">
                            ${mechanism.IsActive ? 'Active' : 'Inactive'}
                        </span>
                    </div>
                    <p class="burn-mechanism-description">${mechanism.Description}</p>
                    <div class="burn-mechanism-details">
                        <span class="burn-mechanism-trigger">${mechanism.TriggerPercentage}% trigger</span>
                        <span class="burn-mechanism-frequency">${mechanism.Frequency}</span>
                    </div>
                </div>
            `).join('');
        }
    },

    // Update utility features
    updateUtilityFeatures(utilityData) {
        if (!utilityData) return;

        // Update utility categories
        const categoriesGrid = document.getElementById('utilityCategoriesGrid');
        if (categoriesGrid && utilityData.Categories) {
            categoriesGrid.innerHTML = utilityData.Categories.map(category => `
                <div class="utility-category-card">
                    <div class="utility-category-header">
                        <span class="utility-category-icon">${category.Icon}</span>
                        <h4 class="utility-category-title">${category.Name}</h4>
                        <span class="utility-category-status ${category.IsLive ? 'live' : 'coming-soon'}">
                            ${category.IsLive ? 'Live' : 'Coming Soon'}
                        </span>
                    </div>
                    <p class="utility-category-description">${category.Description}</p>
                    <div class="utility-features-list">
                        ${category.Features.map(feature => `
                            <div class="utility-feature-item">
                                <span class="utility-feature-status ${feature.IsActive ? 'active' : ''}"></span>
                                <span class="utility-feature-name">${feature.Name}</span>
                            </div>
                        `).join('')}
                    </div>
                </div>
            `).join('');
        }

        // Update utility roadmap
        const roadmapTimeline = document.getElementById('utilityRoadmapTimeline');
        if (roadmapTimeline && utilityData.Roadmap) {
            roadmapTimeline.innerHTML = utilityData.Roadmap.map(item => `
                <div class="roadmap-item">
                    <div class="roadmap-item-header">
                        <h4 class="roadmap-item-name">${item.Feature}</h4>
                        <span class="roadmap-item-date">${Utils.formatDate(item.EstimatedLaunch)}</span>
                    </div>
                    <p class="roadmap-item-description">${item.Description}</p>
                </div>
            `).join('');
        }

        // Update utility metrics
        if (utilityData.Metrics) {
            const metrics = utilityData.Metrics;
            this.updateElement('[data-utility="totalVolume"]', metrics.TotalUtilityVolume, 'currency');
            this.updateElement('[data-utility="activeUtilities"]', metrics.ActiveUtilities, 'number');
            this.updateElement('[data-utility="uniqueUsers"]', metrics.UniqueUsers, 'number');
            this.updateElement('[data-utility="growthRate"]', metrics.MonthlyGrowthRate, 'percentage');
        }
    },

    // Update governance info
    updateGovernanceInfo(governanceData) {
        if (!governanceData) return;

        // Update governance overview
        if (governanceData.Overview) {
            const overview = governanceData.Overview;
            this.updateElement('[data-governance="totalVotingPower"]', overview.TotalVotingPower, 'number');
            this.updateElement('[data-governance="activeProposals"]', overview.ActiveProposals, 'number');
            this.updateElement('[data-governance="passedProposals"]', overview.PassedProposals, 'number');
            this.updateElement('[data-governance="participationRate"]', overview.ParticipationRate, 'percentage');
        }

        // Update governance features
        const featuresGrid = document.getElementById('governanceFeaturesGrid');
        if (featuresGrid && governanceData.Features) {
            featuresGrid.innerHTML = governanceData.Features.map(feature => `
                <div class="governance-feature-card">
                    <div class="governance-feature-header">
                        <h4 class="governance-feature-name">${feature.Name}</h4>
                        <span class="governance-feature-status ${feature.IsImplemented ? 'implemented' : 'pending'}">
                            ${feature.IsImplemented ? 'Implemented' : 'Pending'}
                        </span>
                    </div>
                    <p class="governance-feature-description">${feature.Description}</p>
                    <div class="governance-feature-impact">${feature.Impact}</div>
                </div>
            `).join('');
        }

        // Update active proposals
        const proposalsList = document.getElementById('activeProposalsList');
        if (proposalsList) {
            if (governanceData.ActiveProposals && governanceData.ActiveProposals.length > 0) {
                proposalsList.innerHTML = governanceData.ActiveProposals.map(proposal => `
                    <div class="proposal-item">
                        <h5>${proposal.Title}</h5>
                        <p>${proposal.Description}</p>
                        <div class="proposal-voting">
                            <span>For: ${Utils.formatNumber(proposal.VotesFor)}</span>
                            <span>Against: ${Utils.formatNumber(proposal.VotesAgainst)}</span>
                        </div>
                    </div>
                `).join('');
            } else {
                proposalsList.innerHTML = `
                    <div class="no-proposals">
                        <span class="no-proposals-icon">🗳️</span>
                        <h4>No Active Proposals</h4>
                        <p>The governance system is being prepared. Proposal functionality will be available after TGE.</p>
                    </div>
                `;
            }
        }
    },

    // Update last updated timestamp
    updateLastUpdated() {
        const timestampElements = document.querySelectorAll('.last-updated');
        const currentTime = new Date().toLocaleString();

        timestampElements.forEach(element => {
            element.textContent = `Last updated: ${currentTime}`;
        });

        TokenomicsState.lastUpdate = new Date();
    },

    // Helper function to update individual elements
    updateElement(selector, value, type = 'text') {
        const element = document.querySelector(selector);
        if (!element) return;

        element.dataset.value = value;

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
            case 'text':
            default:
                element.textContent = value;
                break;
        }
    },

    // Convert to camelCase for API property matching
    toCamelCase(str) {
        return str.replace(/[A-Z]/g, letter => letter.toLowerCase())
            .replace(/^[a-z]/, letter => letter.toLowerCase());
    },

    // Fallback page data
    getFallbackPageData() {
        return {
            LiveMetrics: {
                CurrentPrice: 0.065,
                MarketCap: 325000000,
                Volume24h: 2500000,
                PriceChange24h: 4.8,
                HoldersCount: 3247,
                TotalValueLocked: 15000000
            },
            SupplyBreakdown: {
                Allocations: ChartManager.getFallbackDistributionData(),
                Metrics: {
                    MaxSupply: 5000000000,
                    CirculatingSupply: 1000000000,
                    LockedSupply: 4000000000,
                    BurnedSupply: 0
                }
            },
            VestingSchedule: {
                Categories: [
                    {
                        Category: 'Public Presale',
                        TotalTokens: 1250000000,
                        TgePercentage: 20,
                        VestingMonths: 6,
                        StartDate: new Date(Date.now() + 90 * 24 * 60 * 60 * 1000),
                        EndDate: new Date(Date.now() + 270 * 24 * 60 * 60 * 1000)
                    }
                ],
                Summary: {
                    TotalVestedTokens: 4000000000,
                    CurrentlyUnlocked: 1000000000,
                    NextUnlockDate: new Date(Date.now() + 90 * 24 * 60 * 60 * 1000),
                    NextUnlockAmount: 250000000
                }
            },
            TreasuryAnalytics: {
                Overview: {
                    TotalValue: 87500000,
                    OperationalRunwayYears: 10.5,
                    MonthlyBurnRate: 695000,
                    SafetyFundValue: 8750000
                },
                Scenarios: [
                    {
                        Name: 'Bull Market',
                        Description: 'Strong adoption and revenue growth',
                        Probability: 30,
                        ProjectedRunway: 15.8,
                        Severity: 'Positive'
                    }
                ]
            },
            BurnMechanics: {
                Statistics: {
                    TotalBurned: 0,
                    BurnedPercentage: 0,
                    EstimatedAnnualBurn: 25000000,
                    BurnRate: 0.5
                },
                Mechanisms: [
                    {
                        Name: 'Transaction Burn',
                        Description: '0.1% of all platform transactions burned',
                        TriggerPercentage: 0.1,
                        Frequency: 'Per Transaction',
                        IsActive: false,
                        Icon: '🔥'
                    }
                ]
            },
            UtilityFeatures: {
                Categories: [
                    {
                        Name: 'Staking & Rewards',
                        Description: 'Earn rewards by staking TEACH tokens',
                        Icon: '💰',
                        IsLive: false,
                        Features: [
                            {
                                Name: 'Single-sided Staking',
                                IsActive: false
                            }
                        ]
                    }
                ],
                Metrics: {
                    TotalUtilityVolume: 0,
                    ActiveUtilities: 0,
                    UniqueUsers: 0,
                    MonthlyGrowthRate: 0
                }
            },
            GovernanceInfo: {
                Overview: {
                    TotalVotingPower: 0,
                    ActiveProposals: 0,
                    PassedProposals: 0,
                    ParticipationRate: 0
                },
                Features: [
                    {
                        Name: 'Proposal System',
                        Description: 'Create and vote on governance proposals',
                        IsImplemented: false,
                        Impact: 'Community-driven decision making'
                    }
                ],
                ActiveProposals: []
            }
        };
    }
};

// ================================================
// LIVE DATA UPDATES
// ================================================
const LiveUpdates = {
    updateInterval: null,

    // Start live updates
    start() {
        this.updateInterval = setInterval(async () => {
            await this.refreshLiveMetrics();
        }, CONFIG.UPDATE_INTERVAL);
    },

    // Stop live updates
    stop() {
        if (this.updateInterval) {
            clearInterval(this.updateInterval);
            this.updateInterval = null;
        }
    },

    // Refresh live metrics
    async refreshLiveMetrics() {
        try {
            const liveMetrics = await ApiService.getLiveMetrics();
            if (liveMetrics) {
                TokenomicsState.pageData.LiveMetrics = liveMetrics;
                DataManager.updateLiveMetrics(liveMetrics);
                DataManager.updateLastUpdated();
            }
        } catch (error) {
            console.warn('Failed to refresh live metrics:', error);
        }
    }
};

// ================================================
// EVENT HANDLERS
// ================================================
const EventHandlers = {
    // Initialize all event handlers
    init() {
        this.initRefreshButtons();
        this.initScrollEffects();
        this.initResponsiveHandlers();
    },

    // Refresh button handlers
    initRefreshButtons() {
        const refreshButtons = document.querySelectorAll('[data-refresh]');
        refreshButtons.forEach(button => {
            button.addEventListener('click', async (e) => {
                e.preventDefault();
                const section = button.dataset.refresh;
                await this.refreshSection(section);
            });
        });
    },

    // Refresh specific section
    async refreshSection(section) {
        const button = document.querySelector(`[data-refresh="${section}"]`);
        if (button) {
            button.classList.add('loading');
            button.disabled = true;
        }

        try {
            let data = null;
            switch (section) {
                case 'live-metrics':
                    data = await ApiService.getLiveMetrics();
                    if (data) DataManager.updateLiveMetrics(data);
                    break;
                case 'supply':
                    data = await ApiService.getSupplyBreakdown();
                    if (data) DataManager.updateSupplyBreakdown(data);
                    break;
                case 'vesting':
                    data = await ApiService.getVestingSchedule();
                    if (data) DataManager.updateVestingSchedule(data);
                    break;
                case 'treasury':
                    data = await ApiService.getTreasuryAnalytics();
                    if (data) DataManager.updateTreasuryAnalytics(data);
                    break;
                case 'burn':
                    data = await ApiService.getBurnMechanics();
                    if (data) DataManager.updateBurnMechanics(data);
                    break;
                case 'utility':
                    data = await ApiService.getUtilityFeatures();
                    if (data) DataManager.updateUtilityFeatures(data);
                    break;
                case 'governance':
                    data = await ApiService.getGovernanceInfo();
                    if (data) DataManager.updateGovernanceInfo(data);
                    break;
            }

            Utils.showNotification(`${section} data refreshed successfully`, 'success');
        } catch (error) {
            console.error(`Error refreshing ${section}:`, error);
            Utils.showNotification(`Failed to refresh ${section} data`, 'error');
        } finally {
            if (button) {
                button.classList.remove('loading');
                button.disabled = false;
            }
        }
    },

    // Scroll effects
    initScrollEffects() {
        const sections = document.querySelectorAll('.tokenomics-section');
        const navLinks = document.querySelectorAll('.tokenomics-nav a');

        if (sections.length === 0 || navLinks.length === 0) return;

        const observer = new IntersectionObserver((entries) => {
            entries.forEach(entry => {
                if (entry.isIntersecting) {
                    const id = entry.target.id;
                    navLinks.forEach(link => {
                        link.classList.toggle('active', link.getAttribute('href') === `#${id}`);
                    });
                }
            });
        }, { threshold: 0.3 });

        sections.forEach(section => observer.observe(section));
    },

    // Responsive handlers
    initResponsiveHandlers() {
        const debouncedResize = Utils.debounce(() => {
            // Refresh charts on resize
            Object.values(TokenomicsState.charts).forEach(chart => {
                if (chart && chart.refresh) {
                    chart.refresh();
                }
            });
        }, 250);

        window.addEventListener('resize', debouncedResize);
    }
};

// ================================================
// MAIN INITIALIZATION
// ================================================
const TokenomicsApp = {
    // Initialize the entire application
    async init() {
        try {
            console.log('Initializing Tokenomics page...');

            // Load page data
            await DataManager.loadPageData();

            // Initialize charts
            await ChartManager.initializeCharts();

            // Initialize animations
            AnimationController.initScrollAnimations();
            AnimationController.fadeInElements('.tokenomics-section', 200, 100);

            // Initialize event handlers
            EventHandlers.init();

            // Start live updates
            LiveUpdates.start();

            // Mark as loaded
            TokenomicsState.isLoaded = true;

            console.log('Tokenomics page initialized successfully');

        } catch (error) {
            console.error('Error initializing Tokenomics page:', error);
        }
    },

    // Cleanup on page unload
    cleanup() {
        LiveUpdates.stop();

        // Dispose charts
        Object.values(TokenomicsState.charts).forEach(chart => {
            if (chart && chart.destroy) {
                chart.destroy();
            }
        });

        console.log('Tokenomics page cleaned up');
    }
};

// ================================================
// AUTO-INITIALIZATION
// ================================================
document.addEventListener('DOMContentLoaded', () => {
    TokenomicsApp.init();
});

window.addEventListener('beforeunload', () => {
    TokenomicsApp.cleanup();
});

// Expose global API for debugging and external access
window.TokenomicsApp = TokenomicsApp;
window.TokenomicsState = TokenomicsState;