function initializeManagementCharts() {
    try {
        // Portfolio Value Chart
        initializePortfolioValueChart();

        // Fees Earned Chart
        initializeFeesEarnedChart();

        // P&L Distribution Chart
        initializePnLDistributionChart();

        // APY Trends Chart for user positions
        initializeUserAPYTrendsChart();

    } catch (error) {
        console.error('❌ Error initializing management charts:', error);
    }
}

function initializeTVLChart() {
    const chartContainer = document.getElementById('tvl-chart');
    if (!chartContainer) return;

    const chart = new ej.charts.Chart({
        theme: config.charts.theme,
        primaryXAxis: {
            valueType: 'DateTime',
            labelFormat: 'MMM dd',
            majorGridLines: { width: 0 },
            labelStyle: { color: '#94a3b8' }
        },
        primaryYAxis: {
            labelFormat: '${value}',
            majorGridLines: { color: 'rgba(255,255,255,0.1)' },
            labelStyle: { color: '#94a3b8' }
        },
        series: [{
            type: 'Area',
            dataSource: [],
            xName: 'date',
            yName: 'tvl',
            name: 'TVL',
            fill: 'url(#tvl-gradient)',
            border: { width: 2, color: '#4f46e5' }
        }],
        tooltip: {
            enable: true,
            format: '${point.y}',
            textStyle: { color: '#ffffff' }
        },
        background: 'transparent',
        margin: { top: 20, bottom: 20, left: 60, right: 20 }
    });

    chart.appendTo(chartContainer);
    state.charts.tvl = chart;
}

function initializeVolumeChart() {
    const chartContainer = document.getElementById('volume-chart');
    if (!chartContainer) return;

    const chart = new ej.charts.Chart({
        theme: config.charts.theme,
        primaryXAxis: {
            valueType: 'DateTime',
            labelFormat: 'MMM dd',
            majorGridLines: { width: 0 },
            labelStyle: { color: '#94a3b8' }
        },
        primaryYAxis: {
            labelFormat: '${value}',
            majorGridLines: { color: 'rgba(255,255,255,0.1)' },
            labelStyle: { color: '#94a3b8' }
        },
        series: [{
            type: 'Column',
            dataSource: [],
            xName: 'date',
            yName: 'volume',
            name: 'Volume',
            fill: '#06b6d4'
        }],
        tooltip: {
            enable: true,
            format: '${point.y}',
            textStyle: { color: '#ffffff' }
        },
        background: 'transparent',
        margin: { top: 20, bottom: 20, left: 60, right: 20 }
    });

    chart.appendTo(chartContainer);
    state.charts.volume = chart;
}

function initializePoolDistributionChart() {
    const chartContainer = document.getElementById('pool-distribution-chart');
    if (!chartContainer) return;

    const chart = new ej.charts.AccumulationChart({
        theme: config.charts.theme,
        series: [{
            dataSource: [],
            xName: 'pool',
            yName: 'tvl',
            name: 'TVL Distribution',
            innerRadius: '40%',
            dataLabel: {
                visible: true,
                name: 'pool',
                position: 'Outside',
                font: { color: '#ffffff' }
            }
        }],
        tooltip: {
            enable: true,
            format: '${point.x}: ${point.y}%'
        },
        background: 'transparent'
    });

    chart.appendTo(chartContainer);
    state.charts.poolDistribution = chart;
}

function initializeAPYTrendsChart() {
    const chartContainer = document.getElementById('apy-trends-chart');
    if (!chartContainer) return;

    const chart = new ej.charts.Chart({
        theme: config.charts.theme,
        primaryXAxis: {
            valueType: 'DateTime',
            labelFormat: 'MMM dd',
            majorGridLines: { width: 0 },
            labelStyle: { color: '#94a3b8' }
        },
        primaryYAxis: {
            labelFormat: '{value}%',
            majorGridLines: { color: 'rgba(255,255,255,0.1)' },
            labelStyle: { color: '#94a3b8' }
        },
        series: [],
        tooltip: {
            enable: true,
            shared: true,
            textStyle: { color: '#ffffff' }
        },
        background: 'transparent',
        margin: { top: 20, bottom: 20, left: 60, right: 20 }
    });

    chart.appendTo(chartContainer);
    state.charts.apyTrends = chart;
}

function initializePortfolioValueChart() {
    const chartContainer = document.getElementById('portfolio-value-chart');
    if (!chartContainer) return;

    const chart = new ej.charts.Chart({
        theme: config.charts.theme,
        primaryXAxis: {
            valueType: 'DateTime',
            labelFormat: 'MMM dd',
            majorGridLines: { width: 0 },
            labelStyle: { color: '#94a3b8' }
        },
        primaryYAxis: {
            labelFormat: '${value}',
            majorGridLines: { color: 'rgba(255,255,255,0.1)' },
            labelStyle: { color: '#94a3b8' }
        },
        series: [{
            type: 'SplineArea',
            dataSource: [],
            xName: 'date',
            yName: 'value',
            name: 'Portfolio Value',
            fill: 'url(#portfolio-gradient)',
            border: { width: 2, color: '#10b981' }
        }],
        tooltip: {
            enable: true,
            format: '${point.y}',
            textStyle: { color: '#ffffff' }
        },
        background: 'transparent',
        margin: { top: 20, bottom: 20, left: 60, right: 20 }
    });

    chart.appendTo(chartContainer);
    state.charts.portfolioValue = chart;
}

function initializeFeesEarnedChart() {
    const chartContainer = document.getElementById('fees-earned-chart');
    if (!chartContainer) return;

    const chart = new ej.charts.Chart({
        theme: config.charts.theme,
        primaryXAxis: {
            valueType: 'DateTime',
            labelFormat: 'MMM dd',
            majorGridLines: { width: 0 },
            labelStyle: { color: '#94a3b8' }
        },
        primaryYAxis: {
            labelFormat: '${value}',
            majorGridLines: { color: 'rgba(255,255,255,0.1)' },
            labelStyle: { color: '#94a3b8' }
        },
        series: [{
            type: 'StepArea',
            dataSource: [],
            xName: 'date',
            yName: 'fees',
            name: 'Cumulative Fees',
            fill: '#f59e0b',
            border: { width: 2, color: '#f59e0b' }
        }],
        tooltip: {
            enable: true,
            format: '${point.y}',
            textStyle: { color: '#ffffff' }
        },
        background: 'transparent',
        margin: { top: 20, bottom: 20, left: 60, right: 20 }
    });

    chart.appendTo(chartContainer);
    state.charts.feesEarned = chart;
}

function initializePnLDistributionChart() {
    const chartContainer = document.getElementById('pnl-distribution-chart');
    if (!chartContainer) return;

    const chart = new ej.charts.Chart({
        theme: config.charts.theme,
        primaryXAxis: {
            valueType: 'Category',
            majorGridLines: { width: 0 },
            labelStyle: { color: '#94a3b8' }
        },
        primaryYAxis: {
            labelFormat: '${value}',
            majorGridLines: { color: 'rgba(255,255,255,0.1)' },
            labelStyle: { color: '#94a3b8' }
        },
        series: [{
            type: 'Column',
            dataSource: [],
            xName: 'position',
            yName: 'pnl',
            name: 'P&L',
            pointColorMapping: 'color'
        }],
        tooltip: {
            enable: true,
            format: '${point.x}: ${point.y}',
            textStyle: { color: '#ffffff' }
        },
        background: 'transparent',
        margin: { top: 20, bottom: 20, left: 60, right: 20 }
    });

    chart.appendTo(chartContainer);
    state.charts.pnlDistribution = chart;
}

function initializeUserAPYTrendsChart() {
    const chartContainer = document.getElementById('apy-trends-chart');
    if (!chartContainer) return;

    const chart = new ej.charts.Chart({
        theme: config.charts.theme,
        primaryXAxis: {
            valueType: 'DateTime',
            labelFormat: 'MMM dd',
            majorGridLines: { width: 0 },
            labelStyle: { color: '#94a3b8' }
        },
        primaryYAxis: {
            labelFormat: '{value}%',
            majorGridLines: { color: 'rgba(255,255,255,0.1)' },
            labelStyle: { color: '#94a3b8' }
        },
        series: [],
        tooltip: {
            enable: true,
            shared: true,
            textStyle: { color: '#ffffff' }
        },
        background: 'transparent',
        margin: { top: 20, bottom: 20, left: 60, right: 20 }
    });

    chart.appendTo(chartContainer);
    state.charts.userApyTrends = chart;
}

// ================================================
// CHART UPDATES
// ================================================
function updateCharts() {
    if (!state.currentData) return;

    try {
        updateTVLChart();
        updateVolumeChart();
        updatePoolDistributionChart();
        updateAPYTrendsChart();
    } catch (error) {
        console.error('❌ Error updating charts:', error);
    }
}

function updateManagementCharts() {
    if (!state.currentData) return;

    try {
        updatePortfolioValueChart();
        updateFeesEarnedChart();
        updatePnLDistributionChart();
        updateUserAPYTrendsChart();
    } catch (error) {
        console.error('❌ Error updating management charts:', error);
    }
}

function updateTVLChart() {
    if (!state.charts.tvl || !state.currentData?.analytics?.tvlHistory) return;

    const data = state.currentData.analytics.tvlHistory.map(item => ({
        date: new Date(item.date),
        tvl: item.value
    }));

    state.charts.tvl.series[0].dataSource = data;
    state.charts.tvl.refresh();
}

function updateVolumeChart() {
    if (!state.charts.volume || !state.currentData?.analytics?.volumeHistory) return;

    const data = state.currentData.analytics.volumeHistory.map(item => ({
        date: new Date(item.date),
        volume: item.value
    }));

    state.charts.volume.series[0].dataSource = data;
    state.charts.volume.refresh();
}

function updatePoolDistributionChart() {
    if (!state.charts.poolDistribution || !state.currentData?.pools) return;

    const totalTVL = state.currentData.pools.reduce((sum, pool) => sum + pool.totalValueLocked, 0);
    const data = state.currentData.pools.slice(0, 6).map(pool => ({
        pool: `${pool.token1Symbol}/${pool.token2Symbol}`,
        tvl: ((pool.totalValueLocked / totalTVL) * 100).toFixed(1),
        y: (pool.totalValueLocked / totalTVL) * 100
    }));

    state.charts.poolDistribution.series[0].dataSource = data;
    state.charts.poolDistribution.refresh();
}

function updateAPYTrendsChart() {
    if (!state.charts.apyTrends || !state.currentData?.analytics?.apyTrends) return;

    const series = state.currentData.analytics.apyTrends.map((poolTrend, index) => ({
        type: 'Spline',
        dataSource: poolTrend.data.map(item => ({
            date: new Date(item.date),
            apy: item.apy
        })),
        xName: 'date',
        yName: 'apy',
        name: poolTrend.poolName,
        marker: { visible: true, size: 4 },
        width: 2
    }));

    state.charts.apyTrends.series = series;
    state.charts.apyTrends.refresh();
}

function updatePortfolioValueChart() {
    if (!state.charts.portfolioValue || !state.currentData?.performance?.valueHistory) return;

    const data = state.currentData.performance.valueHistory.map(item => ({
        date: new Date(item.date),
        value: item.totalValue
    }));

    state.charts.portfolioValue.series[0].dataSource = data;
    state.charts.portfolioValue.refresh();
}

function updateFeesEarnedChart() {
    if (!state.charts.feesEarned || !state.currentData?.performance?.feesHistory) return;

    const data = state.currentData.performance.feesHistory.map(item => ({
        date: new Date(item.date),
        fees: item.cumulativeFees
    }));

    state.charts.feesEarned.series[0].dataSource = data;
    state.charts.feesEarned.refresh();
}

function updatePnLDistributionChart() {
    if (!state.charts.pnlDistribution || !state.currentData?.positions) return;

    const data = state.currentData.positions.map(position => ({
        position: `${position.token1Symbol}/${position.token2Symbol}`,
        pnl: position.pnlAmount,
        color: position.pnlAmount >= 0 ? '#10b981' : '#ef4444'
    }));

    state.charts.pnlDistribution.series[0].dataSource = data;
    state.charts.pnlDistribution.refresh();
}

function updateUserAPYTrendsChart() {
    if (!state.charts.userApyTrends || !state.currentData?.performance?.apyHistory) return;

    const series = state.currentData.performance.apyHistory.map((positionApy, index) => ({
        type: 'Spline',
        dataSource: positionApy.data.map(item => ({
            date: new Date(item.date),
            apy: item.apy
        })),
        xName: 'date',
        yName: 'apy',
        name: positionApy.positionName,
        marker: { visible: true, size: 4 },
        width: 2
    }));

    state.charts.userApyTrends.series = series;
    state.charts.userApyTrends.refresh();
}

// ================================================
// EVENT HANDLERS
// ================================================
function handlePoolSearch() {
    const searchInput = document.getElementById('pool-search');
    if (!searchInput) return;

    state.filters.poolSearch = searchInput.value.trim();
    renderPoolsGrid();
}

function handleSortChange() {
    const sortBy = document.getElementById('sort-by');
    if (!sortBy) return;

    state.filters.sortBy = sortBy.value;
    renderPoolsGrid();
}

function handleDexFilter() {
    const dexFilter = document.getElementById('dex-filter');
    if (!dexFilter) return;

    state.filters.dexFilter = dexFilter.value;
    renderPoolsGrid();
}

function handleChartPeriodChange(button) {
    // Remove active class from all period buttons in the same container
    const container = button.closest('.chart-controls');
    container.querySelectorAll('.chart-period').forEach(btn => {
        btn.classList.remove('active');
    });

    // Add active class to clicked button
    button.classList.add('active');

    // Get chart container and period
    const chartContainer = button.closest('.chart-container');
    const period = button.dataset.period;

    // Update chart data based on period
    updateChartPeriod(chartContainer, period);
}

function handleTransactionFilter() {
    const txType = document.getElementById('tx-type-filter')?.value || '';
    const txPool = document.getElementById('tx-pool-filter')?.value || '';
    const txDate = document.getElementById('tx-date-filter')?.value || '30d';

    state.filters.txType = txType;
    state.filters.txPool = txPool;
    state.filters.txDate = txDate;

    filterTransactionTable();
}

// ================================================
// UTILITY FUNCTIONS
// ================================================
function filterTransactionTable() {
    const rows = document.querySelectorAll('.transaction-row');

    rows.forEach(row => {
        let show = true;

        // Filter by transaction type
        if (state.filters.txType && row.dataset.txType !== state.filters.txType) {
            show = false;
        }

        // Filter by pool
        if (state.filters.txPool && row.dataset.pool !== state.filters.txPool) {
            show = false;
        }

        // Show/hide row
        row.style.display = show ? 'table-row' : 'none';
    });
}

function updateChartPeriod(chartContainer, period) {
    // This would typically reload chart data for the specified period
    // For now, just refresh existing data
    const chartId = chartContainer.querySelector('[id$="-chart"]')?.id;
    if (chartId && state.charts[chartId.replace('-chart', '')]) {
        const chart = state.charts[chartId.replace('-chart', '')];
        chart.refresh();
    }
}

function animateCounters() {
    const counters = document.querySelectorAll('.stat-value[data-value]');

    counters.forEach(counter => {
        const target = parseFloat(counter.dataset.value) || 0;
        const format = counter.dataset.format || 'number';
        const duration = 1000; // 1 second
        const increment = target / (duration / 16); // 60fps
        let current = 0;

        const timer = setInterval(() => {
            current += increment;
            if (current >= target) {
                current = target;
                clearInterval(timer);
            }

            let displayValue = '';
            switch (format) {
                case 'currency':
                    displayValue = `${Math.round(current).toLocaleString()}`;
                    break;
                case 'percentage':
                    displayValue = `${current.toFixed(2)}%`;
                    break;
                default:
                    displayValue = Math.round(current).toLocaleString();
            }

            counter.textContent = displayValue;
        }, 16);
    });
}

function setupAutoRefresh() {
    // Clear existing timer
    if (state.refreshTimer) {
        clearInterval(state.refreshTimer);
    }

    // Set up new refresh timer
    state.refreshTimer = setInterval(() => {
        if (document.visibilityState === 'visible') {
            refreshData();
        }
    }, config.charts.refreshInterval);
}

function refreshData() {
    console.log('🔄 Auto-refreshing liquidity data...');

    if (state.connectedWallet) {
        loadUserPositions(state.connectedWallet);
    } else {
        loadDashboardData();
    }
}

// ================================================
// WALLET FUNCTIONS
// ================================================
async function connectWallet(walletType) {
    showLoading('Connecting wallet...');

    try {
        let provider;

        switch (walletType) {
            case 'metamask':
                if (typeof window.ethereum !== 'undefined') {
                    provider = window.ethereum;
                } else {
                    throw new Error('MetaMask not found. Please install MetaMask.');
                }
                break;

            case 'walletconnect':
                // WalletConnect integration would go here
                throw new Error('WalletConnect integration coming soon.');

            case 'coinbase':
                // Coinbase Wallet integration would go here
                throw new Error('Coinbase Wallet integration coming soon.');

            case 'trust':
                // Trust Wallet integration would go here
                throw new Error('Trust Wallet integration coming soon.');

            default:
                throw new Error('Unsupported wallet type.');
        }

        // Request account access
        const accounts = await provider.request({ method: 'eth_requestAccounts' });
        const walletAddress = accounts[0];

        // Update UI
        updateWalletStatus(walletAddress);

        // Load user data
        state.connectedWallet = walletAddress;
        loadUserPositions(walletAddress);

        showSuccess('Wallet connected successfully!');
    } catch (error) {
        console.error('❌ Wallet connection error:', error);
        showError(error.message || 'Failed to connect wallet.');
    } finally {
        hideLoading();
    }
}

function updateWalletStatus(address) {
    const walletStatus = document.getElementById('wallet-status');
    const walletAddressSpan = document.getElementById('wallet-address');

    if (walletStatus && walletAddressSpan) {
        walletAddressSpan.textContent = `${address.substring(0, 6)}...${address.substring(38)}`;
        walletStatus.style.display = 'block';
    }

    // Hide wallet connection options
    const walletConnect = document.getElementById('wallet-connect');
    if (walletConnect) {
        walletConnect.style.display = 'none';
    }
}

function loadManualWallet() {
    const addressInput = document.getElementById('manual-wallet-address');
    if (!addressInput) return;

    const address = addressInput.value.trim();

    // Basic address validation
    if (!address.match(/^0x[a-fA-F0-9]{40}$/)) {
        showError('Please enter a valid Ethereum address.');
        return;
    }

    // Redirect to manage page with address
    window.location.href = `/liquidity/manage?walletAddress=${address}`;
}

// ================================================
// POSITION MANAGEMENT FUNCTIONS
// ================================================
async function viewPosition(positionId) {
    showLoading('Loading position details...');

    try {
        const response = await fetch(`/liquidity/position/${positionId}`);
        if (!response.ok) throw new Error('Failed to load position details');

        const positionData = await response.json();
        showPositionDetails(positionData);
    } catch (error) {
        console.error('❌ Error loading position:', error);
        showError('Failed to load position details.');
    } finally {
        hideLoading();
    }
}

async function addToPosition(positionId) {
    // Redirect to add liquidity page with pre-selected pool
    const positionRow = document.querySelector(`[data-position-id="${positionId}"]`);
    if (positionRow) {
        const poolName = positionRow.querySelector('.pool-name')?.textContent;
        window.location.href = `/liquidity/add?poolId=${positionId}&action=add`;
    }
}

async function removePosition(positionId) {
    showRemoveLiquidityModal(positionId);
}

async function claimFees(positionId) {
    showLoading('Preparing fee claim...');

    try {
        // Get DEX URL for fee claiming
        const response = await fetch(`/liquidity/dex-url?action=claim&positionId=${positionId}`);
        if (!response.ok) throw new Error('Failed to get DEX URL');

        const data = await response.json();
        window.open(data.url, '_blank');

        showSuccess('Redirected to DEX for fee claiming.');
    } catch (error) {
        console.error('❌ Error claiming fees:', error);
        showError('Failed to redirect to DEX for fee claiming.');
    } finally {
        hideLoading();
    }
}

function refreshPositions() {
    if (state.connectedWallet) {
        loadUserPositions(state.connectedWallet);
    } else {
        loadDashboardData();
    }
}

// ================================================
// MODAL FUNCTIONS
// ================================================
function showPositionDetails(positionData) {
    const modal = document.getElementById('position-details-modal');
    const modalBody = document.getElementById('position-details-body');

    if (!modal || !modalBody) return;

    modalBody.innerHTML = `
            <div class="position-details-content">
                <div class="position-summary">
                    <h4>${positionData.token1Symbol}/${positionData.token2Symbol} Position</h4>
                    <p>DEX: ${positionData.dexName} | Fee Tier: ${(positionData.feeTier * 100)}%</p>
                </div>
                
                <div class="position-metrics">
                    <div class="metric-row">
                        <span>Current Value:</span>
                        <span>${positionData.currentValue.toFixed(2)}</span>
                    </div>
                    <div class="metric-row">
                        <span>Initial Value:</span>
                        <span>${positionData.initialValue.toFixed(2)}</span>
                    </div>
                    <div class="metric-row">
                        <span>P&L:</span>
                        <span class="${positionData.pnlAmount >= 0 ? 'positive' : 'negative'}">
                            ${positionData.pnlAmount >= 0 ? '+' : ''}${positionData.pnlAmount.toFixed(2)}
                        </span>
                    </div>
                    <div class="metric-row">
                        <span>Fees Earned:</span>
                        <span>${positionData.feesEarned.toFixed(2)}</span>
                    </div>
                    <div class="metric-row">
                        <span>Current APY:</span>
                        <span>${positionData.currentApy.toFixed(2)}%</span>
                    </div>
                </div>
                
                <div class="position-actions">
                    <button class="btn-primary" onclick="addToPosition(${positionData.id})">Add Liquidity</button>
                    <button class="btn-danger" onclick="removePosition(${positionData.id})">Remove Liquidity</button>
                    ${positionData.unclaimedFees > 0 ? `<button class="btn-secondary" onclick="claimFees(${positionData.id})">Claim Fees</button>` : ''}
                </div>
            </div>
        `;

    modal.style.display = 'block';
}

function showRemoveLiquidityModal(positionId) {
    const modal = document.getElementById('remove-liquidity-modal');
    if (!modal) return;

    // Store position ID for later use
    modal.dataset.positionId = positionId;

    // Reset form
    const slider = document.getElementById('removal-slider');
    if (slider) {
        slider.value = 0;
        updateRemovalPreview(0);
    }

    modal.style.display = 'block';
}

function updateRemovalPreview(percentage) {
    const percentageDisplay = document.getElementById('removal-percentage');
    const proceedBtn = document.getElementById('proceed-removal-btn');

    if (percentageDisplay) {
        percentageDisplay.textContent = percentage;
    }

    if (proceedBtn) {
        proceedBtn.disabled = percentage === 0;
    }

    // Update estimated receive amounts
    // This would calculate based on current position data
    const modal = document.getElementById('remove-liquidity-modal');
    const positionId = modal?.dataset.positionId;

    if (positionId) {
        calculateRemovalAmounts(positionId, percentage);
    }
}

async function calculateRemovalAmounts(positionId, percentage) {
    try {
        const response = await fetch('/liquidity/calculate-remove', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json'
            },
            body: JSON.stringify({
                positionId: parseInt(positionId),
                percentage: percentage
            })
        });

        if (!response.ok) throw new Error('Calculation failed');

        const data = await response.json();

        // Update UI with calculated amounts
        const token1Amount = document.getElementById('receive-token1-amount');
        const token2Amount = document.getElementById('receive-token2-amount');
        const totalValue = document.getElementById('receive-total-value');

        if (token1Amount) token1Amount.textContent = data.token1Amount.toFixed(4);
        if (token2Amount) token2Amount.textContent = data.token2Amount.toFixed(4);
        if (totalValue) totalValue.textContent = data.totalValue.toFixed(2);

    } catch (error) {
        console.error('❌ Error calculating removal amounts:', error);
    }
}

async function proceedToRemoval() {
    const modal = document.getElementById('remove-liquidity-modal');
    const positionId = modal?.dataset.positionId;
    const percentage = document.getElementById('removal-slider')?.value;

    if (!positionId || !percentage) return;

    showLoading('Preparing removal...');

    try {
        const response = await fetch(`/liquidity/dex-url?action=remove&positionId=${positionId}&percentage=${percentage}`);
        if (!response.ok) throw new Error('Failed to get DEX URL');

        const data = await response.json();
        window.open(data.url, '_blank');

        closeModal('remove-liquidity-modal');
        showSuccess('Redirected to DEX for liquidity removal.');
    } catch (error) {
        console.error('❌ Error proceeding to removal:', error);
        showError('Failed to redirect to DEX for liquidity removal.');
    } finally {
        hideLoading();
    }
}

function setRemovalPercentage(percentage) {
    const slider = document.getElementById('removal-slider');
    if (slider) {
        slider.value = percentage;
        updateRemovalPreview(percentage);
    }
}

function closeModal(modalId) {
    const modal = document.getElementById(modalId);
    if (modal) {
        modal.style.display = 'none';
    }
}

// ================================================
// PAGINATION FUNCTIONS
// ================================================
function updatePaginationControls() {
    const currentPageSpan = document.getElementById('current-page');
    const totalPagesSpan = document.getElementById('total-pages');
    const prevBtn = document.getElementById('prev-page');
    const nextBtn = document.getElementById('next-page');

    if (currentPageSpan) currentPageSpan.textContent = state.pagination.currentPage;
    if (totalPagesSpan) totalPagesSpan.textContent = state.pagination.totalPages;

    if (prevBtn) {
        prevBtn.disabled = state.pagination.currentPage <= 1;
    }

    if (nextBtn) {
        nextBtn.disabled = state.pagination.currentPage >= state.pagination.totalPages;
    }
}

function loadPreviousPage() {
    if (state.pagination.currentPage > 1) {
        state.pagination.currentPage--;
        if (state.connectedWallet) {
            loadUserTransactions(state.connectedWallet);
        }
    }
}

function loadNextPage() {
    if (state.pagination.currentPage < state.pagination.totalPages) {
        state.pagination.currentPage++;
        if (state.connectedWallet) {
            loadUserTransactions(state.connectedWallet);
        }
    }
}

// ================================================
// UTILITY FUNCTIONS
// ================================================
function debounce(func, wait) {
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

function cacheData(key, data) {
    try {
        const cacheItem = {
            data: data,
            timestamp: Date.now(),
            expires: Date.now() + config.cache.duration
        };
        sessionStorage.setItem(key, JSON.stringify(cacheItem));
    } catch (error) {
        console.warn('⚠️ Failed to cache data:', error);
    }
}

function getCachedData(key) {
    try {
        const cacheItem = JSON.parse(sessionStorage.getItem(key));
        if (cacheItem && cacheItem.expires > Date.now()) {
            return cacheItem.data;
        }
    } catch (error) {
        console.warn('⚠️ Failed to retrieve cached data:', error);
    }
    return null;
}

function loadFallbackData() {
    console.log('📦 Loading fallback data...');

    // Provide fallback data structure
    state.currentData = {
        stats: {
            totalValueLocked: 0,
            volume24h: 0,
            averageApy: 0,
            activePools: 0
        },
        pools: [],
        analytics: {
            tvlHistory: [],
            volumeHistory: [],
            apyTrends: []
        }
    };

    renderDashboard();
    showError('Using cached data. Some information may be outdated.');
}

function initializeWalletConnection() {
    // Set up wallet connection UI
    const walletOptions = document.querySelectorAll('.wallet-option');
    walletOptions.forEach(option => {
        option.addEventListener('click', function () {
            const walletType = this.dataset.wallet;
            connectWallet(walletType);
        });
    });
}

function initializeFilters() {
    // Set up any additional filter functionality
    const filterElements = document.querySelectorAll('[data-filter]');
    filterElements.forEach(element => {
        element.addEventListener('change', function () {
            const filterType = this.dataset.filter;
            const filterValue = this.value;
            applyFilter(filterType, filterValue);
        });
    });
}

function applyFilter(filterType, filterValue) {
    state.filters[filterType] = filterValue;

    // Apply the appropriate filter
    switch (filterType) {
        case 'poolSearch':
            renderPoolsGrid();
            break;
        case 'sortBy':
            renderPoolsGrid();
            break;
        case 'dexFilter':
            renderPoolsGrid();
            break;
        default:
            console.warn('Unknown filter type:', filterType);
    }
}

// ================================================
// EXTERNAL FUNCTIONS FOR VIEWS
// ================================================
function viewPoolDetails(poolId) {
    console.log('👁️ Viewing pool details for:', poolId);

    // Redirect to pool details page or show modal
    window.location.href = `/liquidity/pool/${poolId}`;
}

// ================================================
// UI FEEDBACK FUNCTIONS
// ================================================
function showLoading(message = 'Loading...') {
    const overlay = document.getElementById('loading-overlay');
    if (overlay) {
        const messageElement = overlay.querySelector('p');
        if (messageElement) {
            messageElement.textContent = message;
        }
        overlay.style.display = 'flex';
    }
}

function hideLoading() {
    const overlay = document.getElementById('loading-overlay');
    if (overlay) {
        overlay.style.display = 'none';
    }
}

function showError(message) {
    const errorToast = document.getElementById('error-message');
    const errorText = document.getElementById('error-text');

    if (errorToast && errorText) {
        errorText.textContent = message;
        errorToast.style.display = 'flex';
        errorToast.classList.add('show');

        // Auto-hide after 5 seconds
        setTimeout(() => {
            hideError();
        }, 5000);
    }

    console.error('🚨 Error:', message);
}

function hideError() {
    const errorToast = document.getElementById('error-message');
    if (errorToast) {
        errorToast.classList.remove('show');
        setTimeout(() => {
            errorToast.style.display = 'none';
        }, 300);
    }
}

function showSuccess(message) {
    const successToast = document.getElementById('success-message');
    const successText = document.getElementById('success-text');

    if (successToast && successText) {
        successText.textContent = message;
        successToast.style.display = 'flex';
        successToast.classList.add('show');

        // Auto-hide after 3 seconds
        setTimeout(() => {
            hideSuccess();
        }, 3000);
    }

    console.log('✅ Success:', message);
}

function hideSuccess() {
    const successToast = document.getElementById('success-message');
    if (successToast) {
        successToast.classList.remove('show');
        setTimeout(() => {
            successToast.style.display = 'none';
        }, 300);
    }
}

// ================================================
// CLEANUP
// ================================================
function cleanup() {
    // Clear timers
    if (state.refreshTimer) {
        clearInterval(state.refreshTimer);
        state.refreshTimer = null;
    }

    // Dispose charts
    Object.values(state.charts).forEach(chart => {
        if (chart && typeof chart.destroy === 'function') {
            chart.destroy();
        }
    });
    state.charts = {};

    console.log('🧹 Liquidity Dashboard cleaned up');
}

// ================================================
// GLOBAL FUNCTIONS (exposed to window)
// ================================================
window.viewPoolDetails = viewPoolDetails;
window.viewPosition = viewPosition;
window.addToPosition = addToPosition;
window.removePosition = removePosition;
window.claimFees = claimFees;
window.refreshPositions = refreshPositions;
window.loadManualWallet = loadManualWallet;
window.setRemovalPercentage = setRemovalPercentage;
window.proceedToRemoval = proceedToRemoval;
window.updateRemovalPreview = updateRemovalPreview;
window.closeModal = closeModal;
window.loadPreviousPage = loadPreviousPage;
window.loadNextPage = loadNextPage;
window.hideError = hideError;
window.hideSuccess = hideSuccess;

// Handle page visibility changes for auto-refresh
document.addEventListener('visibilitychange', function () {
    if (document.visibilityState === 'visible' && state.refreshTimer) {
        // Resume auto-refresh when page becomes visible
        setupAutoRefresh();
    }
});

// Handle page unload cleanup
window.addEventListener('beforeunload', cleanup);

// ================================================
// PUBLIC API
// ================================================
return {
    init: init,
    initManagement: initManagement,
    refreshData: refreshData,
    connectWallet: connectWallet,
    viewPoolDetails: viewPoolDetails,
    cleanup: cleanup,

    // Expose state for debugging (development only)
    getState: () => ({ ...state }),
    getConfig: () => ({ ...config })
};

}) ();

// ================================================
// ADDITIONAL UTILITY FUNCTIONS
// ================================================

// Format numbers for display
function formatCurrency(amount, decimals = 2) {
    return new Intl.NumberFormat('en-US', {
        style: 'currency',
        currency: 'USD',
        minimumFractionDigits: decimals,
        maximumFractionDigits: decimals
    }).format(amount);
}

function formatPercentage(value, decimals = 2) {
    return `${value.toFixed(decimals)}%`;
}

function formatAddress(address) {
    if (!address || address.length < 10) return address;
    return `${address.substring(0, 6)}...${address.substring(address.length - 4)}`;
}

// Date formatting
function formatDate(date, format = 'short') {
    const options = {
        short: { month: 'short', day: 'numeric' },
        long: { year: 'numeric', month: 'long', day: 'numeric' },
        time: { hour: '2-digit', minute: '2-digit' }
    };

    return new Intl.DateTimeFormat('en-US', options[format] || options.short).format(new Date(date));
}

// Animation helpers
function animateValue(element, start, end, duration = 1000) {
    if (!element) return;

    const startTime = performance.now();
    const startValue = start;
    const endValue = end;
    const totalChange = endValue - startValue;

    function updateValue(currentTime) {
        const elapsed = currentTime - startTime;
        const progress = Math.min(elapsed / duration, 1);

        // Easing function (ease-out)
        const easedProgress = 1 - Math.pow(1 - progress, 3);

        const currentValue = startValue + (totalChange * easedProgress);
        element.textContent = Math.round(currentValue).toLocaleString();

        if (progress < 1) {
            requestAnimationFrame(updateValue);
        }
    }

    requestAnimationFrame(updateValue);
}

// Error boundary for chart operations
function safeChartOperation(operation, chartName) {
    try {
        return operation();
    } catch (error) {
        console.error(`❌ Chart operation failed for ${chartName}:`, error);
        return null;
    }
}

// Network detection
function detectNetwork() {
    if (typeof window.ethereum !== 'undefined') {
        return window.ethereum.networkVersion || window.ethereum.chainId;
    }
    return null;
}

// Console startup message
console.log(`
🎓 TeachToken Liquidity Dashboard
📊 Version: 1.0.0
🚀 Initialized: ${new Date().toISOString()}
🔗 Network: ${detectNetwork() || 'Unknown'}
`);

// Export for module systems (if needed)
if (typeof module !== 'undefined' && module.exports) {
    module.exports = window.LiquidityDashboard;
}// ================================================
// LIQUIDITY DASHBOARD - MAIN JAVASCRIPT
// Following TeachToken patterns established in analytics/tokenomics
// ================================================

window.LiquidityDashboard = (function () {
    'use strict';

    // ================================================
    // CONFIGURATION & STATE
    // ================================================
    const config = {
        endpoints: {
            data: '/liquidity/data',
            stats: '/liquidity/stats',
            pools: '/liquidity/pools',
            userPositions: '/liquidity/user/{address}/positions',
            userTransactions: '/liquidity/user/{address}/transactions',
            userPerformance: '/liquidity/user/{address}/performance',
            calculate: '/liquidity/calculate',
            comparePoolsEndpoint: '/liquidity/compare-pools',
            dexUrl: '/liquidity/dex-url'
        },
        charts: {
            theme: 'Material3Dark',
            colors: ['#4f46e5', '#06b6d4', '#8b5cf6', '#10b981', '#f59e0b', '#ef4444'],
            refreshInterval: 30000 // 30 seconds
        },
        cache: {
            duration: 5 * 60 * 1000, // 5 minutes
            keys: {
                pools: 'liquidity_pools',
                stats: 'liquidity_stats',
                userPositions: 'user_positions'
            }
        }
    };

    let state = {
        currentData: null,
        charts: {},
        filters: {
            poolSearch: '',
            sortBy: 'apy',
            dexFilter: '',
            txType: '',
            txPool: '',
            txDate: '30d'
        },
        pagination: {
            currentPage: 1,
            totalPages: 1,
            pageSize: 20
        },
        connectedWallet: null,
        refreshTimer: null
    };

    // ================================================
    // INITIALIZATION
    // ================================================
    function init(initialData) {
        console.log('🚀 Initializing Liquidity Dashboard...');

        try {
            // Store initial data
            if (initialData && Object.keys(initialData).length > 0) {
                state.currentData = initialData;
                console.log('✅ Initial data loaded:', initialData);
            }

            // Initialize components
            initializeEventListeners();
            initializeCharts();
            initializeFilters();
            setupAutoRefresh();

            // Load data if not provided initially
            if (!state.currentData) {
                loadDashboardData();
            } else {
                renderDashboard();
            }

            console.log('✅ Liquidity Dashboard initialized successfully');
        } catch (error) {
            console.error('❌ Error initializing Liquidity Dashboard:', error);
            showError('Failed to initialize dashboard. Please refresh the page.');
        }
    }

    function initManagement(initialData, walletAddress) {
        console.log('🚀 Initializing Liquidity Management Dashboard...');

        try {
            state.connectedWallet = walletAddress;

            if (initialData && Object.keys(initialData).length > 0) {
                state.currentData = initialData;
            }

            initializeEventListeners();
            initializeManagementCharts();
            initializeTransactionFilters();

            if (walletAddress) {
                loadUserPositions(walletAddress);
            } else {
                initializeWalletConnection();
            }

            console.log('✅ Liquidity Management Dashboard initialized successfully');
        } catch (error) {
            console.error('❌ Error initializing Management Dashboard:', error);
            showError('Failed to initialize management dashboard.');
        }
    }

    // ================================================
    // EVENT LISTENERS
    // ================================================
    function initializeEventListeners() {
        // Pool search and filters
        const poolSearch = document.getElementById('pool-search');
        if (poolSearch) {
            poolSearch.addEventListener('input', debounce(handlePoolSearch, 300));
        }

        const sortBy = document.getElementById('sort-by');
        if (sortBy) {
            sortBy.addEventListener('change', handleSortChange);
        }

        const dexFilter = document.getElementById('dex-filter');
        if (dexFilter) {
            dexFilter.addEventListener('change', handleDexFilter);
        }

        // Chart period controls
        document.addEventListener('click', function (e) {
            if (e.target.classList.contains('chart-period')) {
                handleChartPeriodChange(e.target);
            }
        });

        // Pool card clicks
        document.addEventListener('click', function (e) {
            if (e.target.classList.contains('pool-btn') || e.target.closest('.pool-card')) {
                const poolId = e.target.closest('[data-pool-id]')?.dataset.poolId;
                if (poolId && e.target.textContent.includes('View Details')) {
                    viewPoolDetails(poolId);
                }
            }
        });

        // Wallet connection
        const walletOptions = document.querySelectorAll('.wallet-option');
        walletOptions.forEach(option => {
            option.addEventListener('click', function () {
                const walletType = this.dataset.wallet;
                connectWallet(walletType);
            });
        });
    }

    function initializeTransactionFilters() {
        const txTypeFilter = document.getElementById('tx-type-filter');
        const txPoolFilter = document.getElementById('tx-pool-filter');
        const txDateFilter = document.getElementById('tx-date-filter');

        if (txTypeFilter) {
            txTypeFilter.addEventListener('change', handleTransactionFilter);
        }
        if (txPoolFilter) {
            txPoolFilter.addEventListener('change', handleTransactionFilter);
        }
        if (txDateFilter) {
            txDateFilter.addEventListener('change', handleTransactionFilter);
        }
    }

    // ================================================
    // DATA LOADING
    // ================================================
    async function loadDashboardData() {
        showLoading('Loading liquidity data...');

        try {
            const response = await fetch(config.endpoints.data, {
                method: 'GET',
                headers: {
                    'Accept': 'application/json',
                    'Content-Type': 'application/json'
                }
            });

            if (!response.ok) {
                throw new Error(`HTTP ${response.status}: ${response.statusText}`);
            }

            const data = await response.json();
            state.currentData = data;

            // Cache the data
            cacheData(config.cache.keys.pools, data);

            renderDashboard();
            console.log('✅ Dashboard data loaded successfully');
        } catch (error) {
            console.error('❌ Error loading dashboard data:', error);
            showError('Failed to load liquidity data. Please try again.');
            loadFallbackData();
        } finally {
            hideLoading();
        }
    }

    async function loadUserPositions(walletAddress) {
        if (!walletAddress) return;

        showLoading('Loading your positions...');

        try {
            const url = config.endpoints.userPositions.replace('{address}', walletAddress);
            const response = await fetch(url);

            if (!response.ok) {
                throw new Error(`HTTP ${response.status}: ${response.statusText}`);
            }

            const data = await response.json();
            state.currentData = data;

            renderManagementDashboard();
            loadUserTransactions(walletAddress);
            console.log('✅ User positions loaded successfully');
        } catch (error) {
            console.error('❌ Error loading user positions:', error);
            showError('Failed to load your positions. Please try again.');
        } finally {
            hideLoading();
        }
    }

    async function loadUserTransactions(walletAddress) {
        if (!walletAddress) return;

        try {
            const url = config.endpoints.userTransactions.replace('{address}', walletAddress);
            const response = await fetch(`${url}?page=${state.pagination.currentPage}&pageSize=${state.pagination.pageSize}`);

            if (!response.ok) {
                throw new Error(`HTTP ${response.status}: ${response.statusText}`);
            }

            const data = await response.json();
            renderTransactionHistory(data);
        } catch (error) {
            console.error('❌ Error loading transactions:', error);
            showError('Failed to load transaction history.');
        }
    }

    // ================================================
    // RENDERING
    // ================================================
    function renderDashboard() {
        if (!state.currentData) {
            console.warn('⚠️ No data available for rendering');
            return;
        }

        try {
            renderQuickStats();
            renderPoolsGrid();
            renderTopPoolsTable();
            updateCharts();
            animateCounters();
        } catch (error) {
            console.error('❌ Error rendering dashboard:', error);
            showError('Error displaying dashboard data.');
        }
    }

    function renderManagementDashboard() {
        if (!state.currentData) return;

        try {
            renderPortfolioOverview();
            renderPositionsTable();
            updateManagementCharts();
            animateCounters();
        } catch (error) {
            console.error('❌ Error rendering management dashboard:', error);
            showError('Error displaying positions data.');
        }
    }

    function renderQuickStats() {
        const statsContainer = document.getElementById('quick-stats');
        if (!statsContainer || !state.currentData?.stats) return;

        const stats = state.currentData.stats;
        const statCards = statsContainer.querySelectorAll('.stat-card');

        statCards.forEach((card, index) => {
            const valueElement = card.querySelector('.stat-value');
            if (!valueElement) return;

            const dataValue = valueElement.dataset.value;
            const dataFormat = valueElement.dataset.format;

            if (dataValue && dataFormat) {
                const numValue = parseFloat(dataValue);
                let formattedValue = '';

                switch (dataFormat) {
                    case 'currency':
                        formattedValue = `$${numValue.toLocaleString('en-US', { minimumFractionDigits: 0, maximumFractionDigits: 0 })}`;
                        break;
                    case 'percentage':
                        formattedValue = `${numValue.toFixed(2)}%`;
                        break;
                    default:
                        formattedValue = numValue.toLocaleString('en-US');
                }

                valueElement.textContent = formattedValue;
            }
        });
    }

    function renderPoolsGrid() {
        const poolsGrid = document.getElementById('pools-grid');
        if (!poolsGrid || !state.currentData?.pools) return;

        // Apply filters
        let filteredPools = [...state.currentData.pools];

        // Search filter
        if (state.filters.poolSearch) {
            const searchTerm = state.filters.poolSearch.toLowerCase();
            filteredPools = filteredPools.filter(pool =>
                pool.token1Symbol.toLowerCase().includes(searchTerm) ||
                pool.token2Symbol.toLowerCase().includes(searchTerm) ||
                pool.dexName.toLowerCase().includes(searchTerm)
            );
        }

        // DEX filter
        if (state.filters.dexFilter) {
            filteredPools = filteredPools.filter(pool =>
                pool.dexName.toLowerCase() === state.filters.dexFilter
            );
        }

        // Sort
        filteredPools.sort((a, b) => {
            switch (state.filters.sortBy) {
                case 'apy':
                    return b.apy - a.apy;
                case 'tvl':
                    return b.totalValueLocked - a.totalValueLocked;
                case 'volume':
                    return b.volume24h - a.volume24h;
                default:
                    return b.apy - a.apy;
            }
        });

        // Update grid
        const existingCards = poolsGrid.querySelectorAll('.pool-card');
        existingCards.forEach(card => {
            if (!filteredPools.find(pool => pool.id == card.dataset.poolId)) {
                card.style.display = 'none';
            } else {
                card.style.display = 'block';
            }
        });
    }

    function renderTopPoolsTable() {
        const tableBody = document.getElementById('pools-table-body');
        if (!tableBody || !state.currentData?.pools) return;

        // Table is already rendered server-side, just ensure it's visible
        const table = document.getElementById('pools-table');
        if (table) {
            table.style.display = 'table';
        }
    }

    function renderPositionsTable() {
        // Positions table is rendered server-side
        // Add any client-side enhancements here
        const positionRows = document.querySelectorAll('.position-row');
        positionRows.forEach(row => {
            row.addEventListener('click', function (e) {
                if (!e.target.closest('button') && !e.target.closest('a')) {
                    const positionId = this.dataset.positionId;
                    viewPosition(positionId);
                }
            });
        });
    }

    function renderTransactionHistory(data) {
        const tableBody = document.getElementById('transactions-table-body');
        if (!tableBody || !data?.transactions) return;

        // Update pagination
        state.pagination.totalPages = data.totalPages || 1;
        updatePaginationControls();

        // Transactions are rendered server-side initially
        // This function handles AJAX updates
    }

    // ================================================
    // CHARTS INITIALIZATION
    // ================================================
    function initializeCharts() {
        try {
            // TVL Trend Chart
            initializeTVLChart();

            // Volume Chart
            initializeVolumeChart();

            // Pool Distribution Chart
            initializePoolDistributionChart();

            // APY Trends Chart
            initializeAPYTrendsChart();

        } catch (error) {
            console.error('❌ Error initializing charts:', error);
        }
    }

    function initializeMan