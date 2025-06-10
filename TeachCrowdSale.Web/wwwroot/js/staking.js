/**
 * Staking Dashboard JavaScript
 * Interactive staking functionality for TeachToken.io
 */

class StakingDashboard {
    constructor() {
        this.apiBaseUrl = '/staking';
        this.refreshInterval = 30000; // 30 seconds
        this.userAddress = null;
        this.stakingPools = [];
        this.userPositions = [];
        this.selectedPool = null;
        this.calculator = new StakingCalculator();
        this.walletConnector = new WalletConnector();
        this.cache = new Map();
        this.refreshTimer = null;

        // Bind methods
        this.refreshData = this.refreshData.bind(this);
        this.handleStakeClick = this.handleStakeClick.bind(this);
        this.handleClaimRewards = this.handleClaimRewards.bind(this);
        this.handleCalculatorChange = this.handleCalculatorChange.bind(this);

        this.init();
    }

    async init() {
        try {
            this.showLoadingState();
            await this.loadInitialData();
            this.setupEventListeners();
            this.startPeriodicRefresh();
            this.hideLoadingState();
        } catch (error) {
            console.error('Failed to initialize staking dashboard:', error);
            this.showErrorMessage('Failed to load staking dashboard. Please refresh the page.');
        }
    }

    async loadInitialData() {
        try {
            // Load pools data with caching
            const poolsData = await this.fetchWithCache('pools', '/GetPoolData', 120000); // 2 min cache
            this.stakingPools = poolsData || [];

            // Load user data if wallet connected
            if (this.userAddress) {
                await this.loadUserData();
            }

            // Load schools data for calculator
            const schoolsData = await this.fetchWithCache('schools', '/GetSchools', 600000); // 10 min cache
            this.availableSchools = schoolsData || [];

            this.renderPools();
            this.renderUserPositions();
            this.setupCalculator();

        } catch (error) {
            console.error('Error loading initial data:', error);
            this.loadFallbackData();
        }
    }

    async loadUserData() {
        if (!this.userAddress) return;

        try {
            const userData = await this.fetchWithCache(
                `user_${this.userAddress}`,
                `/GetUserData?address=${this.userAddress}`,
                30000 // 30 sec cache
            );
            this.userPositions = userData?.positions || [];
            this.renderUserPositions();
        } catch (error) {
            console.error('Error loading user data:', error);
        }
    }

    async fetchWithCache(key, endpoint, ttl = 60000) {
        const cached = this.cache.get(key);
        if (cached && Date.now() - cached.timestamp < ttl) {
            return cached.data;
        }

        try {
            const response = await fetch(`${this.apiBaseUrl}${endpoint}`, {
                method: 'GET',
                headers: {
                    'Content-Type': 'application/json',
                    'X-Requested-With': 'XMLHttpRequest'
                }
            });

            if (!response.ok) {
                throw new Error(`HTTP ${response.status}: ${response.statusText}`);
            }

            const data = await response.json();
            this.cache.set(key, { data, timestamp: Date.now() });
            return data;
        } catch (error) {
            console.error(`Error fetching ${endpoint}:`, error);
            throw error;
        }
    }

    renderPools() {
        const poolsContainer = document.getElementById('staking-pools-container');
        if (!poolsContainer || !this.stakingPools.length) return;

        poolsContainer.innerHTML = this.stakingPools.map(pool => this.renderPoolCard(pool)).join('');

        // Animate pool cards
        this.animatePoolCards();
    }

    renderPoolCard(pool) {
        const isActive = pool.status === 'Active';
        const capacityPercentage = (pool.currentTvl / pool.maxCapacity) * 100;

        return `
            <div class="staking-pool-card" data-pool-id="${pool.id}">
                <div class="pool-header">
                    <h3 class="pool-name">${pool.name}</h3>
                    <span class="pool-status ${pool.status.toLowerCase()}">${pool.status}</span>
                </div>
                
                <div class="pool-apy">
                    <div class="apy-label">Current APY</div>
                    <div class="apy-value animate" data-value="${pool.currentApy}">${pool.currentApy}</div>
                </div>
                
                <div class="pool-metrics">
                    <div class="metric-item">
                        <div class="metric-label">Lock Period</div>
                        <div class="metric-value">${pool.lockPeriodDays} days</div>
                    </div>
                    <div class="metric-item">
                        <div class="metric-label">Min Stake</div>
                        <div class="metric-value">${this.formatTokenAmount(pool.minimumStake)} TEACH</div>
                    </div>
                </div>
                
                <div class="pool-progress">
                    <div class="progress-header">
                        <span class="progress-label">Pool Capacity</span>
                        <span class="progress-percentage">${capacityPercentage.toFixed(1)}%</span>
                    </div>
                    <div class="progress-bar">
                        <div class="progress-fill animate" style="--progress-width: ${capacityPercentage / 100}"></div>
                    </div>
                </div>
                
                <button class="stake-button" 
                        data-pool-id="${pool.id}" 
                        ${!isActive ? 'disabled' : ''}>
                    ${isActive ? 'Stake Now' : 'Pool Full'}
                </button>
            </div>
        `;
    }

    renderUserPositions() {
        const positionsContainer = document.getElementById('user-positions-container');
        if (!positionsContainer) return;

        if (!this.userPositions.length) {
            positionsContainer.innerHTML = this.renderEmptyPositions();
            return;
        }

        const totalStaked = this.userPositions.reduce((sum, pos) => sum + pos.stakedAmount, 0);

        positionsContainer.innerHTML = `
            <div class="positions-header">
                <h2 class="positions-title">Your Staking Positions</h2>
                <div class="total-staked">
                    <div class="total-label">Total Staked</div>
                    <div class="total-value">${this.formatTokenAmount(totalStaked)} TEACH</div>
                </div>
            </div>
            <div class="positions-list">
                ${this.userPositions.map(position => this.renderPositionCard(position)).join('')}
            </div>
        `;

        // Start countdown timers
        this.startCountdownTimers();
    }

    renderPositionCard(position) {
        const canClaim = position.claimableRewards > 0;
        const isUnlocking = position.status === 'Unlocking';

        return `
            <div class="position-card" data-position-id="${position.id}">
                <div class="position-header">
                    <span class="position-pool">${position.poolName}</span>
                    <span class="position-status ${position.status.toLowerCase()}">${position.status}</span>
                </div>
                
                <div class="position-metrics">
                    <div class="position-metric">
                        <div class="position-metric-label">Staked</div>
                        <div class="position-metric-value">${this.formatTokenAmount(position.stakedAmount)} TEACH</div>
                    </div>
                    <div class="position-metric">
                        <div class="position-metric-label">Rewards</div>
                        <div class="position-metric-value">${this.formatTokenAmount(position.totalRewards)} TEACH</div>
                    </div>
                    <div class="position-metric">
                        <div class="position-metric-label">APY</div>
                        <div class="position-metric-value">${position.currentApy}%</div>
                    </div>
                    <div class="position-metric">
                        <div class="position-metric-label">School</div>
                        <div class="position-metric-value">${position.selectedSchool || 'None'}</div>
                    </div>
                </div>
                
                ${isUnlocking ? this.renderCountdownTimer(position.unlockTime) : ''}
                
                <div class="position-actions">
                    <button class="claim-rewards-btn" 
                            data-position-id="${position.id}"
                            ${!canClaim ? 'disabled' : ''}>
                        Claim ${this.formatTokenAmount(position.claimableRewards)} TEACH
                    </button>
                </div>
            </div>
        `;
    }

    renderCountdownTimer(unlockTime) {
        const timeLeft = new Date(unlockTime) - new Date();
        if (timeLeft <= 0) return '';

        const days = Math.floor(timeLeft / (1000 * 60 * 60 * 24));
        const hours = Math.floor((timeLeft % (1000 * 60 * 60 * 24)) / (1000 * 60 * 60));
        const minutes = Math.floor((timeLeft % (1000 * 60 * 60)) / (1000 * 60));
        const seconds = Math.floor((timeLeft % (1000 * 60)) / 1000);

        return `
            <div class="countdown-timer" data-unlock-time="${unlockTime}">
                <div class="countdown-unit">
                    <span class="countdown-number">${days}</span>
                    <span class="countdown-label">Days</span>
                </div>
                <div class="countdown-unit">
                    <span class="countdown-number">${hours}</span>
                    <span class="countdown-label">Hours</span>
                </div>
                <div class="countdown-unit">
                    <span class="countdown-number">${minutes}</span>
                    <span class="countdown-label">Min</span>
                </div>
                <div class="countdown-unit">
                    <span class="countdown-number">${seconds}</span>
                    <span class="countdown-label">Sec</span>
                </div>
            </div>
        `;
    }

    renderEmptyPositions() {
        return `
            <div class="empty-positions">
                <div class="empty-icon">🎯</div>
                <h3>No Staking Positions</h3>
                <p>Start staking TEACH tokens to earn rewards and support education!</p>
                <button class="stake-button" onclick="document.getElementById('staking-pools-container').scrollIntoView()">
                    Explore Staking Pools
                </button>
            </div>
        `;
    }

    setupEventListeners() {
        // Pool card clicks
        document.addEventListener('click', (e) => {
            if (e.target.matches('.stake-button')) {
                this.handleStakeClick(e);
            } else if (e.target.matches('.claim-rewards-btn')) {
                this.handleClaimRewards(e);
            }
        });

        // Calculator inputs
        const calculatorInputs = document.querySelectorAll('.calculator-form input, .calculator-form select');
        calculatorInputs.forEach(input => {
            input.addEventListener('input', this.handleCalculatorChange);
        });

        // Wallet connection
        const connectButton = document.getElementById('connect-wallet-btn');
        if (connectButton) {
            connectButton.addEventListener('click', () => this.walletConnector.connect());
        }

        // Refresh button
        const refreshButton = document.getElementById('refresh-data-btn');
        if (refreshButton) {
            refreshButton.addEventListener('click', () => this.refreshData(true));
        }
    }

    async handleStakeClick(e) {
        e.preventDefault();
        const poolId = e.target.dataset.poolId;

        if (!this.userAddress) {
            this.showConnectWalletModal();
            return;
        }

        this.selectedPool = this.stakingPools.find(p => p.id === poolId);
        if (!this.selectedPool) return;

        this.showStakingModal(this.selectedPool);
    }

    async handleClaimRewards(e) {
        e.preventDefault();
        const positionId = e.target.dataset.positionId;

        if (!this.userAddress) {
            this.showConnectWalletModal();
            return;
        }

        try {
            this.showLoadingButton(e.target);

            // Call API to claim rewards
            const response = await fetch(`${this.apiBaseUrl}/ClaimRewards`, {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json',
                    'X-Requested-With': 'XMLHttpRequest'
                },
                body: JSON.stringify({
                    positionId: positionId,
                    userAddress: this.userAddress
                })
            });

            if (!response.ok) {
                throw new Error('Failed to claim rewards');
            }

            const result = await response.json();

            this.showSuccessMessage(`Successfully claimed ${this.formatTokenAmount(result.claimedAmount)} TEACH!`);
            this.invalidateUserCache();
            await this.loadUserData();

        } catch (error) {
            console.error('Error claiming rewards:', error);
            this.showErrorMessage('Failed to claim rewards. Please try again.');
        } finally {
            this.hideLoadingButton(e.target);
        }
    }

    handleCalculatorChange() {
        const amount = parseFloat(document.getElementById('stake-amount')?.value || 0);
        const poolId = document.getElementById('stake-pool-select')?.value;
        const period = parseInt(document.getElementById('stake-period')?.value || 30);

        if (amount > 0 && poolId) {
            this.calculator.calculateProjections(amount, poolId, period);
        }
    }

    showStakingModal(pool) {
        const modal = document.getElementById('staking-modal');
        if (!modal) return;

        modal.querySelector('.modal-title').textContent = `Stake in ${pool.name}`;
        modal.querySelector('.pool-apy-display').textContent = `${pool.currentApy}% APY`;
        modal.querySelector('#stake-amount-input').max = this.userBalance || 0;
        modal.querySelector('#stake-amount-input').placeholder = `Min: ${this.formatTokenAmount(pool.minimumStake)} TEACH`;

        modal.classList.add('show');
    }

    animatePoolCards() {
        const cards = document.querySelectorAll('.staking-pool-card');
        cards.forEach((card, index) => {
            setTimeout(() => {
                card.style.animation = 'slideInUp 0.6s ease-out forwards';
            }, index * 100);
        });

        // Animate APY counters
        this.animateCounters();

        // Animate progress bars
        setTimeout(() => {
            const progressBars = document.querySelectorAll('.progress-fill.animate');
            progressBars.forEach(bar => bar.classList.add('animate'));
        }, 500);
    }

    animateCounters() {
        const apyValues = document.querySelectorAll('.apy-value.animate');
        apyValues.forEach(element => {
            const targetValue = parseFloat(element.dataset.value);
            this.animateNumber(element, 0, targetValue, 1000);
        });
    }

    animateNumber(element, start, end, duration) {
        const startTime = performance.now();

        const updateNumber = (currentTime) => {
            const elapsed = currentTime - startTime;
            const progress = Math.min(elapsed / duration, 1);

            const current = start + (end - start) * this.easeOutCubic(progress);
            element.textContent = current.toFixed(1);

            if (progress < 1) {
                requestAnimationFrame(updateNumber);
            }
        };

        requestAnimationFrame(updateNumber);
    }

    easeOutCubic(t) {
        return 1 - Math.pow(1 - t, 3);
    }

    startCountdownTimers() {
        const timers = document.querySelectorAll('.countdown-timer');
        timers.forEach(timer => {
            const unlockTime = new Date(timer.dataset.unlockTime);
            this.updateCountdown(timer, unlockTime);

            // Update every second
            setInterval(() => {
                this.updateCountdown(timer, unlockTime);
            }, 1000);
        });
    }

    updateCountdown(timerElement, unlockTime) {
        const timeLeft = unlockTime - new Date();

        if (timeLeft <= 0) {
            timerElement.innerHTML = '<div class="countdown-complete">Unlocked!</div>';
            return;
        }

        const days = Math.floor(timeLeft / (1000 * 60 * 60 * 24));
        const hours = Math.floor((timeLeft % (1000 * 60 * 60 * 24)) / (1000 * 60 * 60));
        const minutes = Math.floor((timeLeft % (1000 * 60 * 60)) / (1000 * 60));
        const seconds = Math.floor((timeLeft % (1000 * 60)) / 1000);

        timerElement.querySelector('.countdown-unit:nth-child(1) .countdown-number').textContent = days;
        timerElement.querySelector('.countdown-unit:nth-child(2) .countdown-number').textContent = hours;
        timerElement.querySelector('.countdown-unit:nth-child(3) .countdown-number').textContent = minutes;
        timerElement.querySelector('.countdown-unit:nth-child(4) .countdown-number').textContent = seconds;
    }

    async refreshData(force = false) {
        if (force) {
            this.cache.clear();
        }

        try {
            this.showRefreshIndicator();
            await this.loadInitialData();
            this.showSuccessMessage('Data refreshed successfully');
        } catch (error) {
            console.error('Error refreshing data:', error);
            this.showErrorMessage('Failed to refresh data');
        } finally {
            this.hideRefreshIndicator();
        }
    }

    startPeriodicRefresh() {
        this.refreshTimer = setInterval(this.refreshData, this.refreshInterval);
    }

    stopPeriodicRefresh() {
        if (this.refreshTimer) {
            clearInterval(this.refreshTimer);
            this.refreshTimer = null;
        }
    }

    invalidateUserCache() {
        const keysToDelete = Array.from(this.cache.keys()).filter(key => key.startsWith('user_'));
        keysToDelete.forEach(key => this.cache.delete(key));
    }

    // Utility methods
    formatTokenAmount(amount) {
        if (amount >= 1000000) {
            return (amount / 1000000).toFixed(2) + 'M';
        } else if (amount >= 1000) {
            return (amount / 1000).toFixed(2) + 'K';
        } else {
            return amount.toFixed(2);
        }
    }

    showLoadingState() {
        const container = document.getElementById('staking-dashboard');
        if (container) {
            container.classList.add('loading');
        }
    }

    hideLoadingState() {
        const container = document.getElementById('staking-dashboard');
        if (container) {
            container.classList.remove('loading');
        }
    }

    showLoadingButton(button) {
        button.disabled = true;
        button.innerHTML = '<div class="loading-spinner"></div> Processing...';
    }

    hideLoadingButton(button) {
        button.disabled = false;
        button.innerHTML = button.dataset.originalText || 'Claim Rewards';
    }

    showRefreshIndicator() {
        const indicator = document.getElementById('refresh-indicator');
        if (indicator) {
            indicator.classList.add('active');
        }
    }

    hideRefreshIndicator() {
        const indicator = document.getElementById('refresh-indicator');
        if (indicator) {
            indicator.classList.remove('active');
        }
    }

    showSuccessMessage(message) {
        this.showMessage(message, 'success');
    }

    showErrorMessage(message) {
        this.showMessage(message, 'error');
    }

    showMessage(message, type) {
        const messageContainer = document.getElementById('message-container');
        if (!messageContainer) return;

        const messageElement = document.createElement('div');
        messageElement.className = `feedback-message ${type}`;
        messageElement.innerHTML = `
            <span class="message-icon">${type === 'success' ? '✓' : '⚠'}</span>
            <span class="message-text">${message}</span>
        `;

        messageContainer.appendChild(messageElement);

        // Auto remove after 5 seconds
        setTimeout(() => {
            messageElement.remove();
        }, 5000);
    }

    showConnectWalletModal() {
        const modal = document.getElementById('connect-wallet-modal');
        if (modal) {
            modal.classList.add('show');
        }
    }

    loadFallbackData() {
        // Load static fallback data when API is unavailable
        this.stakingPools = [
            {
                id: 'flexible',
                name: 'Flexible Pool',
                currentApy: 5.2,
                lockPeriodDays: 30,
                minimumStake: 1000,
                currentTvl: 750000,
                maxCapacity: 1000000,
                status: 'Active'
            },
            {
                id: 'standard',
                name: 'Standard Pool',
                currentApy: 8.5,
                lockPeriodDays: 90,
                minimumStake: 5000,
                currentTvl: 450000,
                maxCapacity: 500000,
                status: 'Active'
            }
        ];

        this.renderPools();
    }

    // Cleanup
    destroy() {
        this.stopPeriodicRefresh();
        this.cache.clear();
        this.walletConnector?.disconnect();
    }
}

// Staking Calculator Class
class StakingCalculator {
    constructor() {
        this.stakingDashboard = null;
    }

    calculateProjections(amount, poolId, customPeriod = null) {
        const pool = window.stakingDashboard?.stakingPools?.find(p => p.id === poolId);
        if (!pool || amount <= 0) return;

        const apy = pool.currentApy / 100;
        const period = customPeriod || pool.lockPeriodDays;

        // Calculate projections
        const dailyRate = apy / 365;
        const dailyReward = amount * dailyRate;
        const weeklyReward = dailyReward * 7;
        const monthlyReward = dailyReward * 30;
        const totalReward = amount * (apy * (period / 365));

        // 50/50 split
        const userReward = totalReward * 0.5;
        const schoolReward = totalReward * 0.5;

        this.displayProjections({
            daily: dailyReward,
            weekly: weeklyReward,
            monthly: monthlyReward,
            total: totalReward,
            userShare: userReward,
            schoolShare: schoolReward,
            period: period
        });
    }

    displayProjections(projections) {
        const container = document.getElementById('projection-results');
        if (!container) return;

        container.innerHTML = `
            <h4 class="projection-title">Reward Projections</h4>
            <div class="projection-grid">
                <div class="projection-item">
                    <div class="projection-period">Daily</div>
                    <div class="projection-amount">${this.formatAmount(projections.daily)} TEACH</div>
                </div>
                <div class="projection-item">
                    <div class="projection-period">Weekly</div>
                    <div class="projection-amount">${this.formatAmount(projections.weekly)} TEACH</div>
                </div>
                <div class="projection-item">
                    <div class="projection-period">Monthly</div>
                    <div class="projection-amount">${this.formatAmount(projections.monthly)} TEACH</div>
                </div>
                <div class="projection-item">
                    <div class="projection-period">${projections.period} Days</div>
                    <div class="projection-amount">${this.formatAmount(projections.total)} TEACH</div>
                </div>
            </div>
            <div class="reward-split">
                <div class="split-user">
                    Your Share: ${this.formatAmount(projections.userShare)} TEACH
                </div>
                <div class="split-school">
                    School Share: ${this.formatAmount(projections.schoolShare)} TEACH
                </div>
            </div>
        `;

        container.style.display = 'block';
    }

    formatAmount(amount) {
        return amount.toFixed(2);
    }
}

// Simple Wallet Connector
class WalletConnector {
    constructor() {
        this.connected = false;
        this.address = null;
        this.balance = 0;
    }

    async connect() {
        try {
            if (typeof window.ethereum !== 'undefined') {
                const accounts = await window.ethereum.request({ method: 'eth_requestAccounts' });
                this.address = accounts[0];
                this.connected = true;

                // Update dashboard with user address
                if (window.stakingDashboard) {
                    window.stakingDashboard.userAddress = this.address;
                    await window.stakingDashboard.loadUserData();
                }

                this.updateUI();
                return this.address;
            } else {
                throw new Error('MetaMask not found');
            }
        } catch (error) {
            console.error('Failed to connect wallet:', error);
            throw error;
        }
    }

    disconnect() {
        this.connected = false;
        this.address = null;
        this.balance = 0;
        this.updateUI();
    }

    updateUI() {
        const connectBtn = document.getElementById('connect-wallet-btn');
        if (connectBtn) {
            if (this.connected) {
                connectBtn.textContent = `${this.address.slice(0, 6)}...${this.address.slice(-4)}`;
                connectBtn.classList.add('connected');
            } else {
                connectBtn.textContent = 'Connect Wallet';
                connectBtn.classList.remove('connected');
            }
        }
    }
}

// Initialize when DOM is ready
document.addEventListener('DOMContentLoaded', () => {
    window.stakingDashboard = new StakingDashboard();
});

// Cleanup on page unload
window.addEventListener('beforeunload', () => {
    if (window.stakingDashboard) {
        window.stakingDashboard.destroy();
    }
});