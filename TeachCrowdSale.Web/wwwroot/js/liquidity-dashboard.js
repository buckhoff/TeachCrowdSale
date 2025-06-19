/**
 * Liquidity Dashboard JavaScript
 * Handles main liquidity dashboard functionality (Index.cshtml)
 * Integrates with LiquidityController endpoints
 */

class LiquidityDashboard {
    constructor() {
        this.charts = {};
        this.refreshInterval = null;
        this.isLoading = false;
        this.currentFilters = {
            dex: '',
            minTvl: 0,
            minApy: 0,
            sortBy: 'TotalValueLocked',
            sortDirection: 'DESC'
        };

        this.initializeOnLoad();
    }

    /**
     * Initialize dashboard when DOM is loaded
     */
    initializeOnLoad() {
        if (document.readyState === 'loading') {
            document.addEventListener('DOMContentLoaded', () => this.initialize());
        } else {
            this.initialize();
        }
    }

    /**
     * Main initialization method
     */
    async initialize() {
        try {
            console.log('Initializing Liquidity Dashboard...');

            // Get initial data from ViewBag
            this.initialData = this.getInitialData();

            // Initialize UI components
            this.initializeCharts();
            this.initializeFilters();
            this.initializePoolCards();
            this.initializeStatCards();
            this.initializeRefreshHandlers();
            this.initializeWalletConnection();

            // Start periodic refresh
            this.startPeriodicRefresh();

            console.log('Liquidity Dashboard initialized successfully');
        } catch (error) {
            console.error('Failed to initialize dashboard:', error);
            this.showErrorMessage('Failed to initialize dashboard');
        }
    }

    /**
     * Get initial data from server-side ViewBag
     */
    getInitialData() {
        try {
            // Try to get from ViewBag.JsonData first
            const jsonDataElement = document.querySelector('script[data-json="liquidity-data"]');
            if (jsonDataElement) {
                return JSON.parse(jsonDataElement.textContent);
            }

            // Fallback to global variable if set
            if (typeof window.liquidityInitialData !== 'undefined') {
                return window.liquidityInitialData;
            }

            console.warn('No initial data found, using empty object');
            return {};
        } catch (error) {
            console.warn('Could not parse initial data:', error);
            return {};
        }
    }

    /**
     * Initialize Syncfusion charts
     */
    initializeCharts() {
        this.initializeTVLChart();
        this.initializeVolumeChart();
        this.initializeAPYChart();
        this.initializeDexDistributionChart();
    }

    /**
     * Initialize TVL trend chart
     */
    initializeTVLChart() {
        const container = document.getElementById('tvl-chart');
        if (!container) return;

        try {
            const chartData = this.prepareTVLChartData();

            this.charts.tvl = new ej.charts.Chart({
                primaryXAxis: {
                    valueType: 'DateTime',
                    title: 'Date',
                    labelFormat: 'MMM dd',
                    intervalType: 'Days'
                },
                primaryYAxis: {
                    title: 'Total Value Locked (USD)',
                    labelFormat: '${value}',
                    minimum: 0
                },
                series: [{
                    dataSource: chartData,
                    xName: 'date',
                    yName: 'tvl',
                    name: 'TVL',
                    type: 'Line',
                    marker: { visible: true, width: 6, height: 6 },
                    fill: '#00d4ff',
                    width: 3
                }],
                title: 'Total Value Locked Trend',
                theme: 'Material3Dark',
                background: 'transparent',
                tooltip: {
                    enable: true,
                    format: 'Date: ${point.x}<br/>TVL: $${point.y}'
                },
                height: '300px'
            });

            this.charts.tvl.appendTo(container);
        } catch (error) {
            console.error('Failed to initialize TVL chart:', error);
        }
    }

    /**
     * Initialize volume chart
     */
    initializeVolumeChart() {
        const container = document.getElementById('volume-chart');
        if (!container) return;

        try {
            const chartData = this.prepareVolumeChartData();

            this.charts.volume = new ej.charts.Chart({
                primaryXAxis: {
                    valueType: 'DateTime',
                    title: 'Date',
                    labelFormat: 'MMM dd'
                },
                primaryYAxis: {
                    title: '24h Volume (USD)',
                    labelFormat: '${value}',
                    minimum: 0
                },
                series: [{
                    dataSource: chartData,
                    xName: 'date',
                    yName: 'volume',
                    name: 'Volume',
                    type: 'Column',
                    fill: '#ff6b35'
                }],
                title: '24h Volume Trend',
                theme: 'Material3Dark',
                background: 'transparent',
                tooltip: {
                    enable: true,
                    format: 'Date: ${point.x}<br/>Volume: $${point.y}'
                },
                height: '300px'
            });

            this.charts.volume.appendTo(container);
        } catch (error) {
            console.error('Failed to initialize volume chart:', error);
        }
    }

    /**
     * Initialize APY distribution chart
     */
    initializeAPYChart() {
        const container = document.getElementById('apy-chart');
        if (!container) return;

        try {
            const chartData = this.prepareAPYChartData();

            this.charts.apy = new ej.charts.Chart({
                primaryXAxis: {
                    valueType: 'Category',
                    title: 'Pools'
                },
                primaryYAxis: {
                    title: 'APY (%)',
                    minimum: 0
                },
                series: [{
                    dataSource: chartData,
                    xName: 'pool',
                    yName: 'apy',
                    name: 'APY',
                    type: 'Column',
                    fill: '#4ade80'
                }],
                title: 'Pool APY Distribution',
                theme: 'Material3Dark',
                background: 'transparent',
                tooltip: {
                    enable: true,
                    format: 'Pool: ${point.x}<br/>APY: ${point.y}%'
                },
                height: '300px'
            });

            this.charts.apy.appendTo(container);
        } catch (error) {
            console.error('Failed to initialize APY chart:', error);
        }
    }

    /**
     * Initialize DEX distribution pie chart
     */
    initializeDexDistributionChart() {
        const container = document.getElementById('dex-distribution-chart');
        if (!container) return;

        try {
            const chartData = this.prepareDexDistributionData();

            this.charts.dexDistribution = new ej.charts.AccumulationChart({
                series: [{
                    dataSource: chartData,
                    xName: 'dex',
                    yName: 'tvl',
                    name: 'TVL Distribution',
                    type: 'Pie',
                    dataLabel: {
                        visible: true,
                        name: 'percentage',
                        position: 'Outside'
                    }
                }],
                title: 'TVL Distribution by DEX',
                theme: 'Material3Dark',
                background: 'transparent',
                tooltip: {
                    enable: true,
                    format: 'DEX: ${point.x}<br/>TVL: $${point.y}<br/>Share: ${point.percentage}%'
                },
                height: '300px'
            });

            this.charts.dexDistribution.appendTo(container);
        } catch (error) {
            console.error('Failed to initialize DEX distribution chart:', error);
        }
    }

    /**
     * Initialize filter controls
     */
    initializeFilters() {
        // DEX filter
        const dexFilter = document.getElementById('dex-filter');
        if (dexFilter) {
            dexFilter.addEventListener('change', (e) => {
                this.currentFilters.dex = e.target.value;
                this.applyFilters();
            });
        }

        // TVL filter
        const tvlFilter = document.getElementById('min-tvl-filter');
        if (tvlFilter) {
            tvlFilter.addEventListener('input', (e) => {
                this.currentFilters.minTvl = parseFloat(e.target.value) || 0;
                this.applyFilters();
            });
        }

        // APY filter
        const apyFilter = document.getElementById('min-apy-filter');
        if (apyFilter) {
            apyFilter.addEventListener('input', (e) => {
                this.currentFilters.minApy = parseFloat(e.target.value) || 0;
                this.applyFilters();
            });
        }

        // Sort controls
        const sortSelect = document.getElementById('sort-by');
        if (sortSelect) {
            sortSelect.addEventListener('change', (e) => {
                this.currentFilters.sortBy = e.target.value;
                this.applyFilters();
            });
        }

        const sortDirection = document.getElementById('sort-direction');
        if (sortDirection) {
            sortDirection.addEventListener('change', (e) => {
                this.currentFilters.sortDirection = e.target.value;
                this.applyFilters();
            });
        }

        // Clear filters button
        const clearFilters = document.getElementById('clear-filters');
        if (clearFilters) {
            clearFilters.addEventListener('click', () => this.clearFilters());
        }
    }

    /**
     * Initialize pool cards and interactions
     */
    initializePoolCards() {
        // Add liquidity buttons
        document.querySelectorAll('.add-liquidity-btn').forEach(btn => {
            btn.addEventListener('click', (e) => {
                const poolId = e.target.dataset.poolId;
                this.redirectToAddLiquidity(poolId);
            });
        });

        // Pool detail buttons
        document.querySelectorAll('.view-pool-btn').forEach(btn => {
            btn.addEventListener('click', (e) => {
                const poolId = e.target.dataset.poolId;
                this.showPoolDetails(poolId);
            });
        });

        // Compare pools functionality
        document.querySelectorAll('.compare-pool-checkbox').forEach(checkbox => {
            checkbox.addEventListener('change', () => this.updatePoolComparison());
        });
    }

    /**
     * Initialize statistics cards
     */
    initializeStatCards() {
        // Refresh stats button
        const refreshStatsBtn = document.getElementById('refresh-stats');
        if (refreshStatsBtn) {
            refreshStatsBtn.addEventListener('click', () => this.refreshStats());
        }

        // Initialize stat card animations
        this.animateStatCards();
    }

    /**
     * Initialize refresh handlers
     */
    initializeRefreshHandlers() {
        // Auto-refresh toggle
        const autoRefreshToggle = document.getElementById('auto-refresh');
        if (autoRefreshToggle) {
            autoRefreshToggle.addEventListener('change', (e) => {
                if (e.target.checked) {
                    this.startPeriodicRefresh();
                } else {
                    this.stopPeriodicRefresh();
                }
            });
        }

        // Manual refresh button
        const refreshBtn = document.getElementById('refresh-dashboard');
        if (refreshBtn) {
            refreshBtn.addEventListener('click', () => this.refreshDashboard());
        }
    }

    /**
     * Initialize wallet connection
     */
    initializeWalletConnection() {
        const connectBtn = document.getElementById('connect-wallet');
        if (connectBtn) {
            connectBtn.addEventListener('click', () => this.connectWallet());
        }

        const disconnectBtn = document.getElementById('disconnect-wallet');
        if (disconnectBtn) {
            disconnectBtn.addEventListener('click', () => this.disconnectWallet());
        }
    }

    /**
     * Apply current filters to pool display
     */
    async applyFilters() {
        if (this.isLoading) return;

        try {
            this.showLoadingState();

            const pools = this.initialData.liquidityPools || [];
            let filteredPools = [...pools];

            // Apply DEX filter
            if (this.currentFilters.dex) {
                filteredPools = filteredPools.filter(pool =>
                    pool.dexName.toLowerCase() === this.currentFilters.dex.toLowerCase()
                );
            }

            // Apply TVL filter
            if (this.currentFilters.minTvl > 0) {
                filteredPools = filteredPools.filter(pool =>
                    pool.totalValueLocked >= this.currentFilters.minTvl
                );
            }

            // Apply APY filter
            if (this.currentFilters.minApy > 0) {
                filteredPools = filteredPools.filter(pool =>
                    pool.apy >= this.currentFilters.minApy
                );
            }

            // Apply sorting
            filteredPools.sort((a, b) => {
                const sortField = this.currentFilters.sortBy;
                const direction = this.currentFilters.sortDirection === 'ASC' ? 1 : -1;

                const aValue = this.getPoolSortValue(a, sortField);
                const bValue = this.getPoolSortValue(b, sortField);

                return (aValue - bValue) * direction;
            });

            this.updatePoolDisplay(filteredPools);
            this.hideLoadingState();
        } catch (error) {
            console.error('Error applying filters:', error);
            this.hideLoadingState();
        }
    }

    /**
     * Clear all filters
     */
    clearFilters() {
        this.currentFilters = {
            dex: '',
            minTvl: 0,
            minApy: 0,
            sortBy: 'TotalValueLocked',
            sortDirection: 'DESC'
        };

        // Reset filter UI
        const dexFilter = document.getElementById('dex-filter');
        if (dexFilter) dexFilter.value = '';

        const tvlFilter = document.getElementById('min-tvl-filter');
        if (tvlFilter) tvlFilter.value = '';

        const apyFilter = document.getElementById('min-apy-filter');
        if (apyFilter) apyFilter.value = '';

        const sortSelect = document.getElementById('sort-by');
        if (sortSelect) sortSelect.value = 'TotalValueLocked';

        const sortDirection = document.getElementById('sort-direction');
        if (sortDirection) sortDirection.value = 'DESC';

        this.applyFilters();
    }

    /**
     * Get pool value for sorting
     */
    getPoolSortValue(pool, field) {
        switch (field) {
            case 'TotalValueLocked':
                return pool.totalValueLocked || 0;
            case 'APY':
                return pool.apy || 0;
            case 'Volume24h':
                return pool.volume24h || 0;
            case 'FeePercentage':
                return pool.feePercentage || 0;
            default:
                return 0;
        }
    }

    /**
     * Update pool cards display
     */
    updatePoolDisplay(pools) {
        const container = document.getElementById('pools-container');
        if (!container) return;

        // Clear existing content
        container.innerHTML = '';

        if (pools.length === 0) {
            container.innerHTML = `
                <div class="col-12">
                    <div class="alert alert-info">
                        <i class="fas fa-info-circle me-2"></i>
                        No pools match your current filters. Try adjusting your criteria.
                    </div>
                </div>
            `;
            return;
        }

        // Generate pool cards
        pools.forEach(pool => {
            const poolCard = this.createPoolCard(pool);
            container.appendChild(poolCard);
        });

        // Reinitialize pool interactions
        this.initializePoolCards();
    }

    /**
     * Create a pool card element
     */
    createPoolCard(pool) {
        const col = document.createElement('div');
        col.className = 'col-lg-4 col-md-6 mb-4';

        const pnlClass = pool.apy >= 50 ? 'high-yield' : pool.apy >= 20 ? 'medium-yield' : 'low-yield';
        const statusClass = pool.isActive ? 'active' : 'inactive';
        const featuredBadge = pool.isFeatured ? '<span class="badge bg-warning">Featured</span>' : '';

        col.innerHTML = `
            <div class="pool-card glass-card ${statusClass}" data-pool-id="${pool.id}">
                <div class="card-header d-flex justify-content-between align-items-center">
                    <h5 class="card-title mb-0">${pool.name}</h5>
                    ${featuredBadge}
                </div>
                <div class="card-body">
                    <div class="pool-pair mb-3">
                        <span class="token-pair">${pool.tokenPair}</span>
                        <small class="text-muted d-block">${pool.dexName}</small>
                    </div>
                    
                    <div class="pool-metrics">
                        <div class="metric-row">
                            <span class="metric-label">APY</span>
                            <span class="metric-value ${pnlClass}">${pool.apyDisplay}</span>
                        </div>
                        <div class="metric-row">
                            <span class="metric-label">TVL</span>
                            <span class="metric-value">${pool.totalValueLockedDisplay}</span>
                        </div>
                        <div class="metric-row">
                            <span class="metric-label">24h Volume</span>
                            <span class="metric-value">${pool.volume24hDisplay}</span>
                        </div>
                        <div class="metric-row">
                            <span class="metric-label">Fees</span>
                            <span class="metric-value">${pool.feeDisplay}</span>
                        </div>
                    </div>
                </div>
                <div class="card-footer">
                    <div class="d-flex gap-2">
                        <button class="btn btn-primary btn-sm add-liquidity-btn flex-grow-1" 
                                data-pool-id="${pool.id}">
                            <i class="fas fa-plus me-1"></i>Add Liquidity
                        </button>
                        <button class="btn btn-outline-secondary btn-sm view-pool-btn" 
                                data-pool-id="${pool.id}">
                            <i class="fas fa-eye"></i>
                        </button>
                        <input type="checkbox" class="btn-check compare-pool-checkbox" 
                               id="compare-${pool.id}" data-pool-id="${pool.id}">
                        <label class="btn btn-outline-info btn-sm" for="compare-${pool.id}">
                            <i class="fas fa-balance-scale"></i>
                        </label>
                    </div>
                </div>
            </div>
        `;

        return col;
    }

    /**
     * Show pool details modal
     */
    async showPoolDetails(poolId) {
        try {
            this.showLoadingState();

            const response = await fetch(`/liquidity/pool/${poolId}`);
            if (!response.ok) throw new Error('Failed to load pool details');

            const pool = await response.json();
            this.displayPoolModal(pool);

            this.hideLoadingState();
        } catch (error) {
            console.error('Error loading pool details:', error);
            this.showErrorMessage('Failed to load pool details');
            this.hideLoadingState();
        }
    }

    /**
     * Display pool details in modal
     */
    displayPoolModal(pool) {
        const modalContent = `
            <div class="modal fade" id="poolDetailsModal" tabindex="-1">
                <div class="modal-dialog modal-lg">
                    <div class="modal-content">
                        <div class="modal-header">
                            <h5 class="modal-title">${pool.name} Details</h5>
                            <button type="button" class="btn-close" data-bs-dismiss="modal"></button>
                        </div>
                        <div class="modal-body">
                            <div class="row">
                                <div class="col-md-6">
                                    <h6>Pool Information</h6>
                                    <table class="table table-sm">
                                        <tr><td>Trading Pair:</td><td>${pool.tokenPair}</td></tr>
                                        <tr><td>Exchange:</td><td>${pool.dexName}</td></tr>
                                        <tr><td>Pool Address:</td><td class="font-monospace small">${pool.poolAddress}</td></tr>
                                        <tr><td>Fee Percentage:</td><td>${pool.feeDisplay}</td></tr>
                                    </table>
                                </div>
                                <div class="col-md-6">
                                    <h6>Performance Metrics</h6>
                                    <table class="table table-sm">
                                        <tr><td>Current APY:</td><td class="text-success">${pool.apyDisplay}</td></tr>
                                        <tr><td>Total Value Locked:</td><td>${pool.totalValueLockedDisplay}</td></tr>
                                        <tr><td>24h Volume:</td><td>${pool.volume24hDisplay}</td></tr>
                                        <tr><td>Current Price:</td><td>${pool.currentPriceDisplay}</td></tr>
                                    </table>
                                </div>
                            </div>
                        </div>
                        <div class="modal-footer">
                            <button type="button" class="btn btn-secondary" data-bs-dismiss="modal">Close</button>
                            <button type="button" class="btn btn-primary" onclick="liquidityDashboard.redirectToAddLiquidity('${pool.id}')">
                                Add Liquidity
                            </button>
                        </div>
                    </div>
                </div>
            </div>
        `;

        // Remove existing modal
        const existingModal = document.getElementById('poolDetailsModal');
        if (existingModal) existingModal.remove();

        // Add new modal
        document.body.insertAdjacentHTML('beforeend', modalContent);

        // Show modal
        const modal = new bootstrap.Modal(document.getElementById('poolDetailsModal'));
        modal.show();
    }

    /**
     * Update pool comparison
     */
    updatePoolComparison() {
        const selectedPools = [];
        document.querySelectorAll('.compare-pool-checkbox:checked').forEach(checkbox => {
            selectedPools.push(parseInt(checkbox.dataset.poolId));
        });

        const compareBtn = document.getElementById('compare-pools-btn');
        if (compareBtn) {
            compareBtn.disabled = selectedPools.length < 2;
            compareBtn.textContent = `Compare Pools (${selectedPools.length})`;
        }

        if (selectedPools.length >= 2) {
            this.showPoolComparison(selectedPools);
        }
    }

    /**
     * Show pool comparison
     */
    async showPoolComparison(poolIds) {
        try {
            // Get pools from initial data instead of API call
            const pools = this.initialData.liquidityPools || [];
            const selectedPools = pools.filter(pool => poolIds.includes(pool.id));

            this.displayPoolComparison(selectedPools);
        } catch (error) {
            console.error('Error comparing pools:', error);
            this.showErrorMessage('Failed to compare pools');
        }
    }

    /**
     * Display pool comparison table
     */
    displayPoolComparison(pools) {
        const container = document.getElementById('pool-comparison');
        if (!container) return;

        let comparisonHtml = `
            <div class="card glass-card mt-4">
                <div class="card-header">
                    <h5 class="card-title mb-0">Pool Comparison</h5>
                </div>
                <div class="card-body">
                    <div class="table-responsive">
                        <table class="table table-hover">
                            <thead>
                                <tr>
                                    <th>Pool</th>
                                    <th>Exchange</th>
                                    <th>APY</th>
                                    <th>TVL</th>
                                    <th>24h Volume</th>
                                    <th>Fees</th>
                                    <th>Action</th>
                                </tr>
                            </thead>
                            <tbody>
        `;

        pools.forEach(pool => {
            comparisonHtml += `
                <tr>
                    <td>
                        <strong>${pool.name}</strong><br>
                        <small class="text-muted">${pool.tokenPair}</small>
                    </td>
                    <td>${pool.dexName}</td>
                    <td class="text-success">${pool.apyDisplay}</td>
                    <td>${pool.totalValueLockedDisplay}</td>
                    <td>${pool.volume24hDisplay}</td>
                    <td>${pool.feeDisplay}</td>
                    <td>
                        <button class="btn btn-primary btn-sm add-liquidity-btn" data-pool-id="${pool.id}">
                            Add Liquidity
                        </button>
                    </td>
                </tr>
            `;
        });

        comparisonHtml += `
                            </tbody>
                        </table>
                    </div>
                </div>
            </div>
        `;

        container.innerHTML = comparisonHtml;

        // Reinitialize buttons
        this.initializePoolCards();
    }

    /**
     * Redirect to add liquidity page
     */
    redirectToAddLiquidity(poolId) {
        window.location.href = `/liquidity/add?poolId=${poolId}&step=1`;
    }

    /**
     * Refresh dashboard data
     */
    async refreshDashboard() {
        if (this.isLoading) return;

        try {
            this.showLoadingState();

            const response = await fetch('/liquidity/data');
            if (!response.ok) throw new Error('Failed to refresh data');

            const data = await response.json();
            this.initialData = data;

            // Update UI components
            this.updateStatCards(data.stats);
            this.updateCharts(data);
            this.applyFilters();

            this.hideLoadingState();
            this.showSuccessMessage('Dashboard refreshed successfully');
        } catch (error) {
            console.error('Error refreshing dashboard:', error);
            this.showErrorMessage('Failed to refresh dashboard');
            this.hideLoadingState();
        }
    }

    /**
     * Refresh statistics
     */
    async refreshStats() {
        try {
            const response = await fetch('/liquidity/stats');
            if (!response.ok) throw new Error('Failed to refresh stats');

            const stats = await response.json();
            this.updateStatCards(stats);

            this.showSuccessMessage('Statistics updated');
        } catch (error) {
            console.error('Error refreshing stats:', error);
            this.showErrorMessage('Failed to refresh statistics');
        }
    }

    /**
     * Update statistics cards
     */
    updateStatCards(stats) {
        if (!stats) return;

        // Update TVL
        const tvlElement = document.getElementById('total-tvl');
        if (tvlElement) {
            tvlElement.textContent = stats.totalValueLockedDisplay;
            this.animateValue(tvlElement, stats.totalValueLocked);
        }

        // Update Volume
        const volumeElement = document.getElementById('total-volume');
        if (volumeElement) {
            volumeElement.textContent = stats.volume24hDisplay;
            this.animateValue(volumeElement, stats.volume24h);
        }

        // Update Pools
        const poolsElement = document.getElementById('active-pools');
        if (poolsElement) {
            poolsElement.textContent = stats.activePools;
        }

        // Update Users
        const usersElement = document.getElementById('total-users');
        if (usersElement) {
            usersElement.textContent = stats.totalUsers;
        }

        // Update TEACH price
        const priceElement = document.getElementById('teach-price');
        if (priceElement) {
            priceElement.textContent = stats.teachCurrentPriceDisplay;
        }

        // Update price change
        const changeElement = document.getElementById('teach-change');
        if (changeElement) {
            changeElement.textContent = stats.teachPriceChange24hDisplay;
            changeElement.className = `price-change ${stats.teachPriceClass}`;
        }
    }

    /**
     * Update charts with new data
     */
    updateCharts(data) {
        try {
            // Update TVL chart
            if (this.charts.tvl) {
                const tvlData = this.prepareTVLChartData(data);
                this.charts.tvl.series[0].dataSource = tvlData;
                this.charts.tvl.refresh();
            }

            // Update volume chart
            if (this.charts.volume) {
                const volumeData = this.prepareVolumeChartData(data);
                this.charts.volume.series[0].dataSource = volumeData;
                this.charts.volume.refresh();
            }

            // Update APY chart
            if (this.charts.apy) {
                const apyData = this.prepareAPYChartData(data);
                this.charts.apy.series[0].dataSource = apyData;
                this.charts.apy.refresh();
            }

            // Update DEX distribution chart
            if (this.charts.dexDistribution) {
                const dexData = this.prepareDexDistributionData(data);
                this.charts.dexDistribution.series[0].dataSource = dexData;
                this.charts.dexDistribution.refresh();
            }
        } catch (error) {
            console.error('Error updating charts:', error);
        }
    }

    /**
     * Prepare TVL chart data
     */
    prepareTVLChartData(data = null) {
        const sourceData = data || this.initialData;
        const stats = sourceData.stats || {};

        // Generate sample data if none provided
        if (!stats.tvlHistory || !stats.historyDates) {
            return this.generateSampleTVLData();
        }

        return stats.historyDates.map((date, index) => ({
            date: new Date(date),
            tvl: stats.tvlHistory[index] || 0
        }));
    }

    /**
     * Prepare volume chart data
     */
    prepareVolumeChartData(data = null) {
        const sourceData = data || this.initialData;
        const stats = sourceData.stats || {};

        if (!stats.volumeHistory || !stats.historyDates) {
            return this.generateSampleVolumeData();
        }

        return stats.historyDates.map((date, index) => ({
            date: new Date(date),
            volume: stats.volumeHistory[index] || 0
        }));
    }

    /**
     * Prepare APY chart data
     */
    prepareAPYChartData(data = null) {
        const sourceData = data || this.initialData;
        const pools = sourceData.liquidityPools || [];

        return pools
            .sort((a, b) => b.apy - a.apy)
            .slice(0, 10)
            .map(pool => ({
                pool: pool.name,
                apy: pool.apy
            }));
    }

    /**
     * Prepare DEX distribution data
     */
    prepareDexDistributionData(data = null) {
        const sourceData = data || this.initialData;
        const pools = sourceData.liquidityPools || [];

        const dexTVL = {};
        pools.forEach(pool => {
            if (!dexTVL[pool.dexName]) {
                dexTVL[pool.dexName] = 0;
            }
            dexTVL[pool.dexName] += pool.totalValueLocked;
        });

        const totalTVL = Object.values(dexTVL).reduce((a, b) => a + b, 0);
        if (totalTVL === 0) return [];

        return Object.entries(dexTVL).map(([dex, tvl]) => ({
            dex,
            tvl,
            percentage: ((tvl / totalTVL) * 100).toFixed(1)
        }));
    }

    /**
     * Generate sample TVL data for fallback
     */
    generateSampleTVLData() {
        const data = [];
        const now = new Date();

        for (let i = 29; i >= 0; i--) {
            const date = new Date(now);
            date.setDate(date.getDate() - i);

            data.push({
                date,
                tvl: Math.random() * 1000000 + 500000
            });
        }

        return data;
    }

    /**
     * Generate sample volume data for fallback
     */
    generateSampleVolumeData() {
        const data = [];
        const now = new Date();

        for (let i = 29; i >= 0; i--) {
            const date = new Date(now);
            date.setDate(date.getDate() - i);

            data.push({
                date,
                volume: Math.random() * 100000 + 10000
            });
        }

        return data;
    }

    /**
     * Animate statistics cards
     */
    animateStatCards() {
        const cards = document.querySelectorAll('.stat-card');
        cards.forEach((card, index) => {
            setTimeout(() => {
                card.classList.add('animate-in');
            }, index * 100);
        });
    }

    /**
     * Animate value changes
     */
    animateValue(element, targetValue) {
        element.classList.add('value-updating');
        setTimeout(() => {
            element.classList.remove('value-updating');
        }, 500);
    }

    /**
     * Connect wallet functionality
     */
    async connectWallet() {
        try {
            if (typeof window.ethereum === 'undefined') {
                this.showErrorMessage('Please install MetaMask or another Web3 wallet');
                return;
            }

            const accounts = await window.ethereum.request({
                method: 'eth_requestAccounts'
            });

            if (accounts.length > 0) {
                const walletAddress = accounts[0];
                this.updateWalletUI(walletAddress);
                this.loadUserPositions(walletAddress);
                this.showSuccessMessage('Wallet connected successfully');
            }
        } catch (error) {
            console.error('Error connecting wallet:', error);
            this.showErrorMessage('Failed to connect wallet');
        }
    }

    /**
     * Disconnect wallet
     */
    disconnectWallet() {
        this.updateWalletUI(null);
        this.clearUserPositions();
        this.showSuccessMessage('Wallet disconnected');
    }

    /**
     * Update wallet UI
     */
    updateWalletUI(walletAddress) {
        const connectBtn = document.getElementById('connect-wallet');
        const disconnectBtn = document.getElementById('disconnect-wallet');
        const walletInfo = document.getElementById('wallet-info');

        if (walletAddress) {
            if (connectBtn) connectBtn.style.display = 'none';
            if (disconnectBtn) disconnectBtn.style.display = 'block';
            if (walletInfo) {
                walletInfo.textContent = `${walletAddress.slice(0, 6)}...${walletAddress.slice(-4)}`;
                walletInfo.style.display = 'block';
            }
        } else {
            if (connectBtn) connectBtn.style.display = 'block';
            if (disconnectBtn) disconnectBtn.style.display = 'none';
            if (walletInfo) walletInfo.style.display = 'none';
        }
    }

    /**
     * Load user positions
     */
    async loadUserPositions(walletAddress) {
        try {
            const response = await fetch(`/liquidity/user/${walletAddress}/positions`);
            if (!response.ok) return;

            const positions = await response.json();
            this.displayUserPositions(positions);
        } catch (error) {
            console.error('Error loading user positions:', error);
        }
    }

    /**
     * Display user positions
     */
    displayUserPositions(positions) {
        const container = document.getElementById('user-positions');
        if (!container) return;

        if (!positions || positions.length === 0) {
            container.innerHTML = `
                <div class="alert alert-info">
                    <i class="fas fa-info-circle me-2"></i>
                    You don't have any liquidity positions yet. Start by adding liquidity to a pool!
                </div>
            `;
            return;
        }

        let positionsHtml = `
            <div class="card glass-card mt-4">
                <div class="card-header">
                    <h5 class="card-title mb-0">Your Positions</h5>
                </div>
                <div class="card-body">
                    <div class="table-responsive">
                        <table class="table table-hover">
                            <thead>
                                <tr>
                                    <th>Pool</th>
                                    <th>Current Value</th>
                                    <th>P&L</th>
                                    <th>Fees Earned</th>
                                    <th>Actions</th>
                                </tr>
                            </thead>
                            <tbody>
        `;

        positions.forEach(position => {
            positionsHtml += `
                <tr>
                    <td>
                        <strong>${position.poolName}</strong><br>
                        <small class="text-muted">${position.tokenPair}</small>
                    </td>
                    <td>${position.currentValueDisplay}</td>
                    <td class="${position.pnLClass}">${position.pnLDisplay}</td>
                    <td>${position.feesEarnedDisplay}</td>
                    <td>
                        <a href="/liquidity/manage?walletAddress=${position.walletAddress}" 
                           class="btn btn-primary btn-sm">
                            Manage
                        </a>
                    </td>
                </tr>
            `;
        });

        positionsHtml += `
                            </tbody>
                        </table>
                    </div>
                </div>
            </div>
        `;

        container.innerHTML = positionsHtml;
    }

    /**
     * Clear user positions display
     */
    clearUserPositions() {
        const container = document.getElementById('user-positions');
        if (container) {
            container.innerHTML = '';
        }
    }

    /**
     * Start periodic refresh
     */
    startPeriodicRefresh() {
        if (this.refreshInterval) {
            clearInterval(this.refreshInterval);
        }

        this.refreshInterval = setInterval(() => {
            this.refreshStats();
        }, 60000); // Refresh every minute
    }

    /**
     * Stop periodic refresh
     */
    stopPeriodicRefresh() {
        if (this.refreshInterval) {
            clearInterval(this.refreshInterval);
            this.refreshInterval = null;
        }
    }

    /**
     * Show loading state
     */
    showLoadingState() {
        this.isLoading = true;
        const spinner = document.getElementById('loading-spinner');
        if (spinner) spinner.style.display = 'block';
    }

    /**
     * Hide loading state
     */
    hideLoadingState() {
        this.isLoading = false;
        const spinner = document.getElementById('loading-spinner');
        if (spinner) spinner.style.display = 'none';
    }

    /**
     * Show success message
     */
    showSuccessMessage(message) {
        this.showToast(message, 'success');
    }

    /**
     * Show error message
     */
    showErrorMessage(message) {
        this.showToast(message, 'error');
    }

    /**
     * Show toast notification
     */
    showToast(message, type = 'info') {
        const toast = document.createElement('div');
        toast.className = `toast align-items-center text-white bg-${type === 'error' ? 'danger' : type === 'success' ? 'success' : 'info'} border-0`;
        toast.setAttribute('role', 'alert');
        toast.innerHTML = `
            <div class="d-flex">
                <div class="toast-body">${message}</div>
                <button type="button" class="btn-close btn-close-white me-2 m-auto" data-bs-dismiss="toast"></button>
            </div>
        `;

        const container = document.getElementById('toast-container') || document.body;
        container.appendChild(toast);

        const bsToast = new bootstrap.Toast(toast);
        bsToast.show();

        toast.addEventListener('hidden.bs.toast', () => {
            toast.remove();
        });
    }

    /**
     * Cleanup when page unloads
     */
    cleanup() {
        this.stopPeriodicRefresh();

        // Dispose charts
        Object.values(this.charts).forEach(chart => {
            if (chart && chart.destroy) {
                chart.destroy();
            }
        });
    }
}

// Initialize dashboard when page loads
const liquidityDashboard = new LiquidityDashboard();

// Cleanup on page unload
window.addEventListener('beforeunload', () => {
    liquidityDashboard.cleanup();
});