// TeachCrowdSale.Web/wwwroot/js/analytics.js
'use strict';

// ================================================
// ANALYTICS DASHBOARD CONFIGURATION
// ================================================
const ANALYTICS_CONFIG = {
    BASE_URL: '',
    UPDATE_INTERVAL: 30000, // 30 seconds
    CHART_THEME: 'Material3Dark',
    ANIMATION_DURATION: 1000,
    CACHE_DURATION: 300000, // 5 minutes
    LIVE_UPDATE_INTERVAL: 15000, // 15 seconds for live data
    CONNECTION_RETRY_DELAY: 5000, // 5 seconds
    MAX_RETRY_ATTEMPTS: 3
};

// Global analytics state
const AnalyticsState = {
    isLoaded: false,
    charts: {},
    data: {},
    updateTimer: null,
    liveUpdateTimer: null,
    connectionStatus: 'connected',
    retryAttempts: 0,
    filters: {
        dateRange: '30d',
        startDate: null,
        endDate: null
    },
    lastUpdate: null,
    isUpdating: false
};

// ================================================
// LIVE DATA UPDATES & CONNECTION MANAGEMENT
// ================================================
const LiveDataManager = {
    // Start live data updates
    startLiveUpdates() {
        console.log('Starting live data updates...');

        if (AnalyticsState.liveUpdateTimer) {
            clearInterval(AnalyticsState.liveUpdateTimer);
        }

        AnalyticsState.liveUpdateTimer = setInterval(() => {
            this.performLiveUpdate();
        }, ANALYTICS_CONFIG.LIVE_UPDATE_INTERVAL);

        // Show live indicator
        this.showLiveIndicator();
    },

    // Stop live updates
    stopLiveUpdates() {
        if (AnalyticsState.liveUpdateTimer) {
            clearInterval(AnalyticsState.liveUpdateTimer);
            AnalyticsState.liveUpdateTimer = null;
        }

        this.hideLiveIndicator();
    },

    // Perform live data update
    async performLiveUpdate() {
        if (AnalyticsState.isUpdating) return;

        try {
            AnalyticsState.isUpdating = true;
            this.showUpdateIndicator();

            // Get live metrics
            const liveData = await this.fetchLiveMetrics();

            if (liveData) {
                // Update charts with animation
                await this.updateChartsWithAnimation(liveData);

                // Update UI metrics
                this.updateLiveMetrics(liveData);

                // Reset retry attempts on success
                AnalyticsState.retryAttempts = 0;
                AnalyticsState.connectionStatus = 'connected';
                AnalyticsState.lastUpdate = new Date();

                this.updateConnectionStatus();
            }

        } catch (error) {
            console.warn('Live update failed:', error);
            this.handleUpdateError();
        } finally {
            AnalyticsState.isUpdating = false;
            this.hideUpdateIndicator();
        }
    },

    // Fetch live metrics
    async fetchLiveMetrics() {
        try {
            const response = await fetch('/Analytics/GetLiveMetrics', {
                method: 'GET',
                headers: {
                    'Cache-Control': 'no-cache',
                    'Pragma': 'no-cache'
                }
            });

            if (!response.ok) {
                throw new Error(`HTTP ${response.status}`);
            }

            return await response.json();
        } catch (error) {
            throw new Error(`Failed to fetch live metrics: ${error.message}`);
        }
    },

    // Update charts with smooth animations
    async updateChartsWithAnimation(liveData) {
        const promises = [];

        // Update price chart if new data
        if (liveData.latestPrice && AnalyticsState.charts.priceHistory) {
            promises.push(this.updatePriceChart(liveData.latestPrice));
        }

        // Update volume chart if new data
        if (liveData.latestVolume && AnalyticsState.charts.volumeHistory) {
            promises.push(this.updateVolumeChart(liveData.latestVolume));
        }

        // Update tier performance
        if (liveData.tierProgress && AnalyticsState.charts.tierPerformance) {
            promises.push(this.updateTierChart(liveData.tierProgress));
        }

        await Promise.all(promises);
    },

    // Update price chart with new data point
    updatePriceChart(priceData) {
        return new Promise((resolve) => {
            const chart = AnalyticsState.charts.priceHistory;
            if (!chart) return resolve();

            // Add new data point with animation
            const currentData = chart.series[0].dataSource;
            const newDataPoint = {
                x: new Date(priceData.timestamp),
                y: priceData.price
            };

            // Keep last 50 points for performance
            const updatedData = [...currentData, newDataPoint].slice(-50);

            // Update with animation
            chart.series[0].setData(updatedData, true, true, {
                duration: ANALYTICS_CONFIG.ANIMATION_DURATION
            });

            setTimeout(resolve, ANALYTICS_CONFIG.ANIMATION_DURATION);
        });
    },

    // Update volume chart
    updateVolumeChart(volumeData) {
        return new Promise((resolve) => {
            const chart = AnalyticsState.charts.volumeHistory;
            if (!chart) return resolve();

            const currentData = chart.series[0].dataSource;
            const newDataPoint = {
                x: new Date(volumeData.timestamp),
                y: volumeData.volume
            };

            const updatedData = [...currentData, newDataPoint].slice(-24); // Last 24 hours

            chart.series[0].setData(updatedData, true, true, {
                duration: ANALYTICS_CONFIG.ANIMATION_DURATION
            });

            setTimeout(resolve, ANALYTICS_CONFIG.ANIMATION_DURATION);
        });
    },

    // Update tier performance chart
    updateTierChart(tierData) {
        return new Promise((resolve) => {
            const chart = AnalyticsState.charts.tierPerformance;
            if (!chart) return resolve();

            // Update tier progress with smooth animation
            tierData.forEach((tier, index) => {
                if (chart.series[0] && chart.series[0].points[index]) {
                    chart.series[0].points[index].update(tier.sold / 1000000, true, {
                        duration: ANALYTICS_CONFIG.ANIMATION_DURATION
                    });
                }
            });

            setTimeout(resolve, ANALYTICS_CONFIG.ANIMATION_DURATION);
        });
    },

    // Update live UI metrics
    updateLiveMetrics(liveData) {
        // Update price displays
        if (liveData.latestPrice) {
            this.animateValueUpdate('.live-price', liveData.latestPrice.price, this.formatCurrency);
            this.animateValueUpdate('.price-change', liveData.latestPrice.change24h, this.formatPercentage);
        }

        // Update volume
        if (liveData.latestVolume) {
            this.animateValueUpdate('.live-volume', liveData.latestVolume.volume, this.formatCurrency);
        }

        // Update market cap
        if (liveData.marketCap) {
            this.animateValueUpdate('.live-market-cap', liveData.marketCap, this.formatCurrency);
        }

        // Update participant count
        if (liveData.participantsCount) {
            this.animateValueUpdate('.live-participants', liveData.participantsCount, this.formatNumber);
        }

        // Update total raised
        if (liveData.totalRaised) {
            this.animateValueUpdate('.live-total-raised', liveData.totalRaised, this.formatCurrency);
        }
    },

    // Animate value updates with counter effect
    animateValueUpdate(selector, newValue, formatter) {
        const elements = document.querySelectorAll(selector);

        elements.forEach(element => {
            const currentValue = parseFloat(element.dataset.value || element.textContent.replace(/[^0-9.-]/g, '')) || 0;
            const targetValue = parseFloat(newValue) || 0;

            if (Math.abs(currentValue - targetValue) < 0.01) return; // Skip if no significant change

            // Add pulse animation class
            element.classList.add('value-updating');

            // Animate counter
            this.animateCounter(element, currentValue, targetValue, formatter);

            // Remove animation class after animation
            setTimeout(() => {
                element.classList.remove('value-updating');
            }, ANALYTICS_CONFIG.ANIMATION_DURATION);
        });
    },

    // Counter animation utility
    animateCounter(element, startValue, endValue, formatter) {
        const duration = ANALYTICS_CONFIG.ANIMATION_DURATION;
        const startTime = performance.now();

        const animate = (currentTime) => {
            const elapsed = currentTime - startTime;
            const progress = Math.min(elapsed / duration, 1);

            // Easing function (ease-out)
            const easeOut = 1 - Math.pow(1 - progress, 3);
            const currentValue = startValue + (endValue - startValue) * easeOut;

            element.textContent = formatter ? formatter(currentValue) : currentValue.toFixed(2);
            element.dataset.value = currentValue;

            if (progress < 1) {
                requestAnimationFrame(animate);
            } else {
                element.textContent = formatter ? formatter(endValue) : endValue.toFixed(2);
                element.dataset.value = endValue;
            }
        };

        requestAnimationFrame(animate);
    },

    // Handle update errors with retry logic
    handleUpdateError() {
        AnalyticsState.retryAttempts++;
        AnalyticsState.connectionStatus = 'error';

        if (AnalyticsState.retryAttempts >= ANALYTICS_CONFIG.MAX_RETRY_ATTEMPTS) {
            console.warn('Max retry attempts reached, switching to fallback mode');
            AnalyticsState.connectionStatus = 'disconnected';
            this.showConnectionError();
        }

        this.updateConnectionStatus();
    },

    // Show/hide live indicator
    showLiveIndicator() {
        const indicator = document.getElementById('liveIndicator');
        if (indicator) {
            indicator.style.display = 'flex';
            indicator.classList.add('live-active');
        }
    },

    hideLiveIndicator() {
        const indicator = document.getElementById('liveIndicator');
        if (indicator) {
            indicator.style.display = 'none';
            indicator.classList.remove('live-active');
        }
    },

    // Show/hide update indicator
    showUpdateIndicator() {
        const indicator = document.getElementById('updateIndicator');
        if (indicator) {
            indicator.style.display = 'block';
            indicator.classList.add('updating');
        }
    },

    hideUpdateIndicator() {
        const indicator = document.getElementById('updateIndicator');
        if (indicator) {
            indicator.style.display = 'none';
            indicator.classList.remove('updating');
        }
    },

    // Update connection status display
    updateConnectionStatus() {
        const statusElement = document.getElementById('connectionStatus');
        if (!statusElement) return;

        statusElement.className = `connection-status ${AnalyticsState.connectionStatus}`;

        switch (AnalyticsState.connectionStatus) {
            case 'connected':
                statusElement.innerHTML = '<span class="status-dot"></span>Live';
                break;
            case 'error':
                statusElement.innerHTML = '<span class="status-dot"></span>Reconnecting...';
                break;
            case 'disconnected':
                statusElement.innerHTML = '<span class="status-dot"></span>Offline';
                break;
        }
    },

    // Show connection error message
    showConnectionError() {
        const errorElement = document.getElementById('connectionError');
        if (errorElement) {
            errorElement.style.display = 'block';
            setTimeout(() => {
                errorElement.style.display = 'none';
            }, 5000);
        }
    },

    // Utility formatters
    formatCurrency(value) {
        return new Intl.NumberFormat('en-US', {
            style: 'currency',
            currency: 'USD',
            minimumFractionDigits: 2,
            maximumFractionDigits: 6
        }).format(value);
    },

    formatPercentage(value) {
        const sign = value >= 0 ? '+' : '';
        return `${sign}${value.toFixed(2)}%`;
    },

    formatNumber(value) {
        return new Intl.NumberFormat('en-US').format(Math.round(value));
    }
};
const AnalyticsApiService = {
    // Get dashboard data
    async getDashboardData() {
        try {
            const response = await fetch('/Analytics/GetDashboardData');
            if (!response.ok) throw new Error(`HTTP ${response.status}`);
            return await response.json();
        } catch (error) {
            console.error('Failed to fetch dashboard data:', error);
            return this.getFallbackDashboardData();
        }
    },

    // Get chart-specific data
    async getChartData(chartType, params = {}) {
        try {
            const queryString = new URLSearchParams(params).toString();
            const response = await fetch(`/Analytics/GetChartData?type=${chartType}&${queryString}`);
            if (!response.ok) throw new Error(`HTTP ${response.status}`);
            return await response.json();
        } catch (error) {
            console.error(`Failed to fetch ${chartType} chart data:`, error);
            return this.getFallbackChartData(chartType);
        }
    },

    // Fallback data for when API fails
    getFallbackDashboardData() {
        return {
            tokenAnalytics: {
                currentPrice: 0.065,
                marketCap: 325000000,
                volume24h: 2500000,
                priceChange24h: 4.8,
                holdersCount: 3247
            },
            presaleAnalytics: {
                totalRaised: 12500000,
                fundingProgress: 14.3,
                participantsCount: 2847,
                currentTier: { name: 'Community Round', price: 0.06, progress: 45 }
            }
        };
    },

    getFallbackChartData(chartType) {
        const fallbackData = {
            priceHistory: this.generateMockPriceData(),
            volumeHistory: this.generateMockVolumeData(),
            tokenDistribution: this.getMockTokenDistribution(),
            treasuryAllocation: this.getMockTreasuryAllocation(),
            tierPerformance: this.getMockTierPerformance()
        };
        return fallbackData[chartType] || [];
    },

    // Mock data generators
    generateMockPriceData() {
        const data = [];
        const basePrice = 0.04;
        const now = new Date();

        for (let i = 30; i >= 0; i--) {
            const date = new Date(now.getTime() - (i * 24 * 60 * 60 * 1000));
            const price = basePrice + (Math.random() * 0.03) + (i * 0.001);
            data.push({
                x: date,
                y: price,
                volume: 1000000 + (Math.random() * 2000000)
            });
        }
        return data;
    },

    generateMockVolumeData() {
        const data = [];
        const now = new Date();

        for (let i = 7; i >= 0; i--) {
            const date = new Date(now.getTime() - (i * 24 * 60 * 60 * 1000));
            data.push({
                x: date,
                y: 1000000 + (Math.random() * 3000000)
            });
        }
        return data;
    },

    getMockTokenDistribution() {
        return [
            { x: 'Public Presale', y: 25, color: '#4f46e5' },
            { x: 'Community Incentives', y: 24, color: '#06b6d4' },
            { x: 'Platform Ecosystem', y: 20, color: '#8b5cf6' },
            { x: 'Initial Liquidity', y: 12, color: '#10b981' },
            { x: 'Team & Development', y: 8, color: '#f59e0b' },
            { x: 'Educational Partners', y: 7, color: '#ec4899' },
            { x: 'Reserve Fund', y: 4, color: '#ef4444' }
        ];
    },

    getMockTreasuryAllocation() {
        return [
            { x: 'Development', y: 30, color: '#4f46e5' },
            { x: 'Marketing', y: 20, color: '#06b6d4' },
            { x: 'Operations', y: 15, color: '#8b5cf6' },
            { x: 'Legal & Compliance', y: 10, color: '#10b981' },
            { x: 'Partnerships', y: 10, color: '#f59e0b' },
            { x: 'Reserve Fund', y: 15, color: '#ef4444' }
        ];
    },

    getMockTierPerformance() {
        return [
            { x: 'Seed Round', y: 250000000, sold: 250000000, color: '#4f46e5' },
            { x: 'Community Round', y: 375000000, sold: 169000000, color: '#06b6d4' },
            { x: 'Growth Round', y: 375000000, sold: 0, color: '#8b5cf6' },
            { x: 'Final Round', y: 250000000, sold: 0, color: '#10b981' }
        ];
    }
};

// ================================================
// CHART MANAGEMENT SYSTEM
// ================================================
const AnalyticsChartManager = {
    // Initialize all charts
    async initializeCharts() {
        console.log('Initializing analytics charts...');

        try {
            // Load chart data
            await this.loadChartData();

            // Create charts
            await Promise.all([
                this.createPriceHistoryChart(),
                this.createVolumeHistoryChart(),
                this.createTokenDistributionChart(),
                this.createTreasuryAllocationChart(),
                this.createTierPerformanceChart(),
                this.createTimeSeriesChart()
            ]);

            console.log('All analytics charts initialized successfully');
        } catch (error) {
            console.error('Error initializing charts:', error);
        }
    },

    // Load chart data from API
    async loadChartData() {
        const dashboardData = await AnalyticsApiService.getDashboardData();
        AnalyticsState.data = dashboardData;

        // Load specific chart data
        const chartDataPromises = [
            AnalyticsApiService.getChartData('priceHistory', AnalyticsState.filters),
            AnalyticsApiService.getChartData('volumeHistory', AnalyticsState.filters),
            AnalyticsApiService.getChartData('tokenDistribution'),
            AnalyticsApiService.getChartData('treasuryAllocation'),
            AnalyticsApiService.getChartData('tierPerformance')
        ];

        const [priceHistory, volumeHistory, tokenDistribution, treasuryAllocation, tierPerformance] =
            await Promise.all(chartDataPromises);

        AnalyticsState.data.charts = {
            priceHistory,
            volumeHistory,
            tokenDistribution,
            treasuryAllocation,
            tierPerformance
        };
    },

    // Price history line chart
    async createPriceHistoryChart() {
        const container = document.getElementById('priceHistoryChart');
        if (!container) return;

        const data = AnalyticsState.data.charts?.priceHistory ||
            AnalyticsApiService.getFallbackChartData('priceHistory');

        const chart = new ej.charts.Chart({
            primaryXAxis: {
                valueType: 'DateTime',
                labelFormat: 'MMM dd',
                majorGridLines: { color: '#333333' },
                lineStyle: { color: '#333333' },
                labelStyle: { color: '#ffffff' }
            },
            primaryYAxis: {
                labelFormat: '${value}',
                majorGridLines: { color: '#333333' },
                lineStyle: { color: '#333333' },
                labelStyle: { color: '#ffffff' }
            },
            series: [{
                type: 'Line',
                dataSource: data,
                xName: 'x',
                yName: 'y',
                name: 'TEACH Price',
                width: 3,
                fill: '#4f46e5',
                marker: {
                    visible: true,
                    width: 6,
                    height: 6,
                    fill: '#4f46e5',
                    border: { color: '#ffffff', width: 2 }
                }
            }],
            theme: ANALYTICS_CONFIG.CHART_THEME,
            background: 'transparent',
            legendSettings: { visible: false },
            tooltip: {
                enable: true,
                format: 'Date: ${point.x}<br/>Price: $${point.y}',
                textStyle: { color: '#ffffff' }
            },
            crosshair: {
                enable: true,
                lineType: 'Both'
            },
            zoomSettings: {
                enableSelectionZooming: true,
                enablePinchZooming: true
            }
        });

        chart.appendTo(container);
        AnalyticsState.charts.priceHistory = chart;
    },

    // Volume history histogram
    async createVolumeHistoryChart() {
        const container = document.getElementById('volumeHistoryChart');
        if (!container) return;

        const data = AnalyticsState.data.charts?.volumeHistory ||
            AnalyticsApiService.getFallbackChartData('volumeHistory');

        const chart = new ej.charts.Chart({
            primaryXAxis: {
                valueType: 'DateTime',
                labelFormat: 'MMM dd',
                majorGridLines: { color: '#333333' },
                lineStyle: { color: '#333333' },
                labelStyle: { color: '#ffffff' }
            },
            primaryYAxis: {
                labelFormat: '${value}',
                majorGridLines: { color: '#333333' },
                lineStyle: { color: '#333333' },
                labelStyle: { color: '#ffffff' }
            },
            series: [{
                type: 'Column',
                dataSource: data,
                xName: 'x',
                yName: 'y',
                name: '24h Volume',
                fill: '#06b6d4',
                cornerRadius: { topLeft: 4, topRight: 4 }
            }],
            theme: ANALYTICS_CONFIG.CHART_THEME,
            background: 'transparent',
            legendSettings: { visible: false },
            tooltip: {
                enable: true,
                format: 'Date: ${point.x}<br/>Volume: $${point.y}',
                textStyle: { color: '#ffffff' }
            }
        });

        chart.appendTo(container);
        AnalyticsState.charts.volumeHistory = chart;
    },

    // Token distribution donut chart
    async createTokenDistributionChart() {
        const container = document.getElementById('tokenDistributionChart');
        if (!container) return;

        const data = AnalyticsState.data.charts?.tokenDistribution ||
            AnalyticsApiService.getFallbackChartData('tokenDistribution');

        const chart = new ej.charts.AccumulationChart({
            series: [{
                dataSource: data,
                xName: 'x',
                yName: 'y',
                innerRadius: '40%',
                radius: '90%',
                dataLabel: {
                    visible: true,
                    name: 'x',
                    position: 'Outside',
                    connectorStyle: { color: '#ffffff' },
                    font: { color: '#ffffff', size: '12px', fontWeight: '500' }
                },
                palettes: data.map(item => item.color || '#4f46e5')
            }],
            theme: ANALYTICS_CONFIG.CHART_THEME,
            background: 'transparent',
            legendSettings: {
                visible: true,
                position: 'Bottom',
                textStyle: { color: '#ffffff' },
                padding: 10
            },
            tooltip: {
                enable: true,
                format: '${point.x}: ${point.y}%',
                textStyle: { color: '#ffffff' }
            }
        });

        chart.appendTo(container);
        AnalyticsState.charts.tokenDistribution = chart;
    },

    // Treasury allocation pie chart
    async createTreasuryAllocationChart() {
        const container = document.getElementById('treasuryAllocationChart');
        if (!container) return;

        const data = AnalyticsState.data.charts?.treasuryAllocation ||
            AnalyticsApiService.getFallbackChartData('treasuryAllocation');

        const chart = new ej.charts.AccumulationChart({
            series: [{
                dataSource: data,
                xName: 'x',
                yName: 'y',
                radius: '90%',
                dataLabel: {
                    visible: true,
                    name: 'x',
                    position: 'Outside',
                    connectorStyle: { color: '#ffffff' },
                    font: { color: '#ffffff', size: '12px', fontWeight: '500' }
                },
                palettes: data.map(item => item.color || '#4f46e5')
            }],
            theme: ANALYTICS_CONFIG.CHART_THEME,
            background: 'transparent',
            legendSettings: {
                visible: true,
                position: 'Bottom',
                textStyle: { color: '#ffffff' },
                padding: 10
            },
            tooltip: {
                enable: true,
                format: '${point.x}: ${point.y}%',
                textStyle: { color: '#ffffff' }
            }
        });

        chart.appendTo(container);
        AnalyticsState.charts.treasuryAllocation = chart;
    },

    // Tier performance bar chart
    async createTierPerformanceChart() {
        const container = document.getElementById('tierPerformanceChart');
        if (!container) return;

        const data = AnalyticsState.data.charts?.tierPerformance ||
            AnalyticsApiService.getFallbackChartData('tierPerformance');

        const chart = new ej.charts.Chart({
            primaryXAxis: {
                valueType: 'Category',
                majorGridLines: { color: '#333333' },
                lineStyle: { color: '#333333' },
                labelStyle: { color: '#ffffff' }
            },
            primaryYAxis: {
                labelFormat: '{value}M',
                majorGridLines: { color: '#333333' },
                lineStyle: { color: '#333333' },
                labelStyle: { color: '#ffffff' }
            },
            series: [{
                type: 'StackingColumn',
                dataSource: data,
                xName: 'x',
                yName: 'sold',
                name: 'Sold',
                fill: '#4f46e5'
            }, {
                type: 'StackingColumn',
                dataSource: data.map(item => ({
                    x: item.x,
                    y: (item.y - item.sold) / 1000000
                })),
                xName: 'x',
                yName: 'y',
                name: 'Remaining',
                fill: '#1e2235'
            }],
            theme: ANALYTICS_CONFIG.CHART_THEME,
            background: 'transparent',
            legendSettings: {
                visible: true,
                textStyle: { color: '#ffffff' }
            },
            tooltip: {
                enable: true,
                textStyle: { color: '#ffffff' }
            }
        });

        chart.appendTo(container);
        AnalyticsState.charts.tierPerformance = chart;
    },

    // Time-series analytics chart
    async createTimeSeriesChart() {
        const container = document.getElementById('timeSeriesChart');
        if (!container) return;

        // Combine multiple metrics into one time-series view
        const priceData = AnalyticsState.data.charts?.priceHistory ||
            AnalyticsApiService.getFallbackChartData('priceHistory');
        const volumeData = AnalyticsState.data.charts?.volumeHistory ||
            AnalyticsApiService.getFallbackChartData('volumeHistory');

        const chart = new ej.charts.Chart({
            primaryXAxis: {
                valueType: 'DateTime',
                labelFormat: 'MMM dd',
                majorGridLines: { color: '#333333' },
                lineStyle: { color: '#333333' },
                labelStyle: { color: '#ffffff' }
            },
            primaryYAxis: {
                labelFormat: '${value}',
                title: 'Price',
                titleStyle: { color: '#ffffff' },
                majorGridLines: { color: '#333333' },
                lineStyle: { color: '#333333' },
                labelStyle: { color: '#ffffff' }
            },
            axes: [{
                name: 'secondary',
                opposedPosition: true,
                labelFormat: '${value}',
                title: 'Volume',
                titleStyle: { color: '#ffffff' },
                majorGridLines: { color: 'transparent' },
                lineStyle: { color: '#333333' },
                labelStyle: { color: '#ffffff' }
            }],
            series: [{
                type: 'Line',
                dataSource: priceData,
                xName: 'x',
                yName: 'y',
                name: 'Price',
                width: 2,
                fill: '#4f46e5'
            }, {
                type: 'Column',
                dataSource: volumeData,
                xName: 'x',
                yName: 'y',
                name: 'Volume',
                yAxisName: 'secondary',
                fill: '#06b6d4',
                opacity: 0.6
            }],
            theme: ANALYTICS_CONFIG.CHART_THEME,
            background: 'transparent',
            legendSettings: {
                visible: true,
                textStyle: { color: '#ffffff' }
            },
            tooltip: {
                enable: true,
                shared: true,
                textStyle: { color: '#ffffff' }
            }
        });

        chart.appendTo(container);
        AnalyticsState.charts.timeSeries = chart;
    },

    // Refresh all charts with new data
    async refreshCharts() {
        try {
            await this.loadChartData();

            // Update each chart with new data
            Object.values(AnalyticsState.charts).forEach(chart => {
                if (chart && chart.refresh) {
                    chart.refresh();
                }
            });

            console.log('Charts refreshed successfully');
        } catch (error) {
            console.error('Error refreshing charts:', error);
        }
    },

    // Handle window resize
    handleResize() {
        Object.values(AnalyticsState.charts).forEach(chart => {
            if (chart && chart.refresh) {
                chart.refresh();
            }
        });
    },

    // Export chart as image
    exportChart(chartName, format = 'PNG') {
        const chart = AnalyticsState.charts[chartName];
        if (chart && chart.export) {
            chart.export(format, `TeachToken_${chartName}_${new Date().toISOString().split('T')[0]}`);
        }
    }
};

// ================================================
// ANALYTICS DASHBOARD CONTROLLER
// ================================================
const AnalyticsDashboard = {
    // Initialize the analytics dashboard
    async init() {
        console.log('Initializing Analytics Dashboard...');

        try {
            // Initialize chart management
            await AnalyticsChartManager.initializeCharts();

            // Setup event listeners
            this.setupEventListeners();

            // Start auto-refresh
            this.startAutoRefresh();

            // Mark as loaded
            AnalyticsState.isLoaded = true;

            console.log('Analytics Dashboard initialized successfully');
        } catch (error) {
            console.error('Failed to initialize Analytics Dashboard:', error);
        }
    },

    // Setup event listeners
    setupEventListeners() {
        // Window resize handler
        window.addEventListener('resize', () => {
            AnalyticsChartManager.handleResize();
        });

        // Date range filter changes
        const dateRangeSelector = document.getElementById('dateRangeSelector');
        if (dateRangeSelector) {
            dateRangeSelector.addEventListener('change', (e) => {
                this.handleDateRangeChange(e.target.value);
            });
        }

        // Export buttons
        document.querySelectorAll('[data-export-chart]').forEach(button => {
            button.addEventListener('click', (e) => {
                const chartName = e.target.dataset.exportChart;
                const format = e.target.dataset.exportFormat || 'PNG';
                AnalyticsChartManager.exportChart(chartName, format);
            });
        });

        // Toggle live updates button
        const liveToggle = document.getElementById('toggleLiveUpdates');
        if (liveToggle) {
            liveToggle.addEventListener('click', () => {
                this.toggleLiveUpdates();
            });
        }

        // Retry connection button
        const retryButton = document.getElementById('retryConnection');
        if (retryButton) {
            retryButton.addEventListener('click', () => {
                this.retryConnection();
            });
        }

        // Refresh button
        const refreshButton = document.getElementById('refreshAnalytics');
        if (refreshButton) {
            refreshButton.addEventListener('click', () => {
                this.refreshDashboard();
            });
        }
    },

    // Handle date range changes
    async handleDateRangeChange(range) {
        AnalyticsState.filters.dateRange = range;

        // Calculate date range
        const now = new Date();
        switch (range) {
            case '7d':
                AnalyticsState.filters.startDate = new Date(now.getTime() - (7 * 24 * 60 * 60 * 1000));
                break;
            case '30d':
                AnalyticsState.filters.startDate = new Date(now.getTime() - (30 * 24 * 60 * 60 * 1000));
                break;
            case '90d':
                AnalyticsState.filters.startDate = new Date(now.getTime() - (90 * 24 * 60 * 60 * 1000));
                break;
            case '1y':
                AnalyticsState.filters.startDate = new Date(now.getTime() - (365 * 24 * 60 * 60 * 1000));
                break;
        }
        AnalyticsState.filters.endDate = now;

        // Refresh charts
        await AnalyticsChartManager.refreshCharts();
    },

    // Start auto-refresh timer
    startAutoRefresh() {
        if (AnalyticsState.updateTimer) {
            clearInterval(AnalyticsState.updateTimer);
        }

        AnalyticsState.updateTimer = setInterval(() => {
            this.refreshDashboard();
        }, ANALYTICS_CONFIG.UPDATE_INTERVAL);

        // Start live updates
        LiveDataManager.startLiveUpdates();
    },

    // Stop auto-refresh
    stopAutoRefresh() {
        if (AnalyticsState.updateTimer) {
            clearInterval(AnalyticsState.updateTimer);
            AnalyticsState.updateTimer = null;
        }

        // Stop live updates
        LiveDataManager.stopLiveUpdates();
    },

    // Refresh dashboard data
    async refreshDashboard() {
        try {
            console.log('Refreshing analytics dashboard...');
            await AnalyticsChartManager.refreshCharts();

            // Update timestamp
            const timestampEl = document.getElementById('lastUpdated');
            if (timestampEl) {
                timestampEl.textContent = `Last updated: ${new Date().toLocaleTimeString()}`;
            }
        } catch (error) {
            console.error('Error refreshing dashboard:', error);
        }
    }
};
};

// ================================================
// INITIALIZATION
// ================================================
document.addEventListener('DOMContentLoaded', () => {
    // Initialize analytics dashboard
    AnalyticsDashboard.init();
});

// Handle page visibility changes
document.addEventListener('visibilitychange', () => {
    if (document.visibilityState === 'visible' && AnalyticsState.isLoaded) {
        AnalyticsDashboard.refreshDashboard();
    }
});

// Export for global access
window.AnalyticsDashboard = AnalyticsDashboard;
window.AnalyticsChartManager = AnalyticsChartManager;