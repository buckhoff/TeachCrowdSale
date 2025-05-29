// TeacherSupport Platform - Buy/Trade Page JavaScript
'use strict';

// ================================================
// GLOBAL CONFIGURATION
// ================================================
const CONFIG = {
    API_BASE_URL: '/api/buytrade',
    ANIMATION_DURATION: 1000,
    UPDATE_INTERVAL: 30000, // 30 seconds
    WALLET_CONNECTION_TIMEOUT: 10000,
    PRICE_CALCULATION_DEBOUNCE: 500,
    CHART_THEME: 'Material3Dark'
};

// Global state management
const BuyTradeState = {
    isLoaded: false,
    pageData: {},
    connectedWallet: null,
    selectedTier: null,
    currentPurchaseAmount: 0,
    priceCalculation: null,
    userTradeInfo: null,
    purchaseInProgress: false
};

BuyTradeState.walletConnecting = false;
BuyTradeState.errors = [];
BuyTradeState.retryAttempts = {};

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

    // Format token amounts
    formatTokens(amount, decimals = 0) {
        if (amount === null || amount === undefined) return '0';
        return this.formatNumber(amount, decimals) + ' TEACH';
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

    // Validate Ethereum address
    isValidAddress(address) {
        return /^0x[a-fA-F0-9]{40}$/.test(address);
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
    }
};

// ================================================
// API SERVICE
// ================================================
const ApiService = {
    async makeRequest(endpoint, options = {}) {
        try {
            const controller = new AbortController();
            const timeoutId = setTimeout(() => controller.abort(), 15000); // 15s timeout

            const response = await fetch(`${CONFIG.API_BASE_URL}${endpoint}`, {
                headers: {
                    'Content-Type': 'application/json',
                    ...options.headers
                },
                signal: controller.signal,
                ...options
            });

            clearTimeout(timeoutId);

            // Handle different error status codes
            if (!response.ok) {
                let errorData;
                try {
                    errorData = await response.json();
                } catch {
                    errorData = { message: `HTTP ${response.status}: ${response.statusText}` };
                }

                // Handle specific status codes
                switch (response.status) {
                    case 400:
                        throw new ApiError('Invalid request', 'BAD_REQUEST', errorData);
                    case 401:
                        throw new ApiError('Unauthorized', 'UNAUTHORIZED', errorData);
                    case 403:
                        throw new ApiError('Forbidden', 'FORBIDDEN', errorData);
                    case 404:
                        throw new ApiError('Resource not found', 'NOT_FOUND', errorData);
                    case 429:
                        throw new ApiError('Too many requests', 'RATE_LIMITED', errorData);
                    case 500:
                        throw new ApiError('Server error', 'SERVER_ERROR', errorData);
                    default:
                        throw new ApiError(errorData.message || 'Request failed', 'UNKNOWN_ERROR', errorData);
                }
            }

            return await response.json();

        } catch (error) {
            if (error.name === 'AbortError') {
                throw new ApiError('Request timeout', 'TIMEOUT');
            }

            if (error instanceof ApiError) {
                throw error;
            }

            // Network or other errors
            if (!navigator.onLine) {
                throw new ApiError('No internet connection', 'OFFLINE');
            }

            throw new ApiError('Network error', 'NETWORK_ERROR', { originalError: error.message });
        }
    }
};


// ================================================
// WALLET CONNECTION SERVICE
// ================================================
const WalletService = {
    // Check if MetaMask is available
    isMetaMaskAvailable() {
        return typeof window.ethereum !== 'undefined' && window.ethereum.isMetaMask;
    },

    // Connect wallet
    async connectWallet() {
        try {
            // Check MetaMask availability
            if (!this.isMetaMaskAvailable()) {
                throw new WalletError('MetaMask not detected', 'METAMASK_NOT_FOUND', {
                    action: 'install',
                    url: 'https://metamask.io/download/'
                });
            }

            // Check if already connecting
            if (BuyTradeState.walletConnecting) {
                throw new WalletError('Wallet connection already in progress', 'CONNECTION_IN_PROGRESS');
            }

            BuyTradeState.walletConnecting = true;

            // Request account access with timeout
            const accounts = await Promise.race([
                window.ethereum.request({ method: 'eth_requestAccounts' }),
                new Promise((_, reject) =>
                    setTimeout(() => reject(new Error('Connection timeout')), CONFIG.WALLET_CONNECTION_TIMEOUT)
                )
            ]);

            if (!accounts || accounts.length === 0) {
                throw new WalletError('No accounts found', 'NO_ACCOUNTS', {
                    action: 'unlock',
                    message: 'Please unlock your MetaMask wallet'
                });
            }

            const address = accounts[0];

            // Verify network with retry logic
            await this.checkAndSwitchNetwork();

            // Test wallet connectivity
            await this.testWalletConnection(address);

            // Update state
            BuyTradeState.connectedWallet = {
                address: address,
                isConnected: true,
                connectedAt: new Date()
            };

            // Load user data with error handling
            try {
                await this.loadUserData(address);
            } catch (error) {
                console.warn('Failed to load user data:', error);
                Utils.showNotification('Wallet connected, but failed to load account data', 'warning');
            }

            return address;

        } catch (error) {
            console.error('Wallet connection failed:', error);

            if (error instanceof WalletError) {
                throw error;
            }

            // Handle specific MetaMask errors
            if (error.code === 4001) {
                throw new WalletError('Connection rejected by user', 'USER_REJECTED');
            } else if (error.code === -32002) {
                throw new WalletError('Connection request already pending', 'REQUEST_PENDING');
            } else if (error.message.includes('timeout')) {
                throw new WalletError('Connection timeout', 'TIMEOUT');
            }

            throw new WalletError('Failed to connect wallet', 'CONNECTION_FAILED', { originalError: error.message });
        } finally {
            BuyTradeState.walletConnecting = false;
        }
    },

    // Enhanced network checking with auto-switch
    async checkAndSwitchNetwork() {
        try {
            const chainId = await window.ethereum.request({ method: 'eth_chainId' });
            const expectedChainId = '0x1'; // Ethereum mainnet

            if (chainId !== expectedChainId) {
                try {
                    await window.ethereum.request({
                        method: 'wallet_switchEthereumChain',
                        params: [{ chainId: expectedChainId }]
                    });
                } catch (switchError) {
                    if (switchError.code === 4902) {
                        throw new WalletError('Ethereum mainnet not found in wallet', 'NETWORK_NOT_FOUND', {
                            action: 'add_network',
                            chainId: expectedChainId
                        });
                    }
                    throw switchError;
                }
            }
        } catch (error) {
            if (error instanceof WalletError) {
                throw error;
            }
            throw new WalletError('Network verification failed', 'NETWORK_ERROR', { originalError: error.message });
        }
    },

    // Test wallet connection
    async testWalletConnection(address) {
        try {
            const balance = await window.ethereum.request({
                method: 'eth_getBalance',
                params: [address, 'latest']
            });

            if (balance === null || balance === undefined) {
                throw new Error('Unable to retrieve wallet balance');
            }
        } catch (error) {
            throw new WalletError('Wallet connection test failed', 'CONNECTION_TEST_FAILED', {
                originalError: error.message
            });
        }
    },

    // Load user data after wallet connection
    // Enhanced user data loading with retry
    async loadUserData(address, retryCount = 0) {
        const maxRetries = 3;

        try {
            const walletInfo = await ApiService.getWalletInfo(address);
            BuyTradeState.userTradeInfo = walletInfo.userInfo;

            UIController.updateWalletUI(address, walletInfo);
            UIController.updateUserDashboard(walletInfo.userInfo);

        } catch (error) {
            if (retryCount < maxRetries) {
                console.warn(`Retrying user data load (${retryCount + 1}/${maxRetries}):`, error);
                await new Promise(resolve => setTimeout(resolve, 1000 * (retryCount + 1)));
                return this.loadUserData(address, retryCount + 1);
            }
            throw error;
        }
    },

    // Disconnect wallet
    disconnectWallet() {
        BuyTradeState.connectedWallet = null;
        BuyTradeState.userTradeInfo = null;
        UIController.updateWalletUI(null);
        UIController.hideUserDashboard();
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
            if (window.BuyTradePageData && Object.keys(window.BuyTradePageData).length > 0) {
                BuyTradeState.pageData = window.BuyTradePageData;
            } else {
                BuyTradeState.pageData = await ApiService.getBuyTradeData();
            }

            this.updateUI();
            return true;
        } catch (error) {
            console.error('Error loading page data:', error);
            Utils.showNotification('Error loading page data. Please refresh.', 'error');
            return false;
        }
    },

    // Update UI with loaded data
    updateUI() {
        const { pageData } = BuyTradeState;

        if (pageData.currentTier) {
            UIController.updateCurrentTier(pageData.currentTier);
            BuyTradeState.selectedTier = pageData.currentTier;
        }

        if (pageData.allTiers) {
            UIController.updateTiersComparison(pageData.allTiers);
        }

        if (pageData.presaleStats) {
            UIController.updateLiveStats(pageData.presaleStats, pageData.tokenInfo);
        }

        if (pageData.purchaseOptions) {
            UIController.updatePaymentMethods(pageData.purchaseOptions);
        }

        if (pageData.dexIntegrations) {
            UIController.updateDexIntegrations(pageData.dexIntegrations);
        }
    },

    // Calculate purchase price with debouncing
    calculatePrice: Utils.debounce(async function (address, tierId, amount) {
        if (!address || !tierId || !amount || amount <= 0) {
            UIController.hidePurchaseCalculation();
            return;
        }

        try {
            const calculation = await ApiService.calculatePrice(address, tierId, amount);
            BuyTradeState.priceCalculation = calculation;
            UIController.updatePurchaseCalculation(calculation);
        } catch (error) {
            console.error('Error calculating price:', error);
            UIController.showError('Error calculating price. Please try again.');
        }
    }, CONFIG.PRICE_CALCULATION_DEBOUNCE),

    // Refresh data periodically
    startPeriodicUpdates() {
        setInterval(async () => {
            if (document.visibilityState === 'visible') {
                try {
                    const updatedData = await ApiService.getBuyTradeData();
                    BuyTradeState.pageData = updatedData;
                    this.updateUI();
                } catch (error) {
                    console.error('Error updating data:', error);
                }
            }
        }, CONFIG.UPDATE_INTERVAL);
    }
};

// ================================================
// UI CONTROLLER
// ================================================
const UIController = {
    // Update current tier display
    updateCurrentTier(tier) {
        const elements = {
            name: document.getElementById('currentTierName'),
            price: document.getElementById('currentTierPrice'),
            progress: document.getElementById('currentTierProgress'),
            sold: document.getElementById('tierSold'),
            remaining: document.getElementById('tierRemaining'),
            minPurchase: document.getElementById('minPurchase'),
            maxPurchase: document.getElementById('maxPurchase')
        };

        if (elements.name) elements.name.textContent = tier.name;
        if (elements.price) elements.price.textContent = Utils.formatCurrency(tier.price);
        if (elements.progress) {
            elements.progress.style.width = `${tier.progress}%`;
            elements.progress.dataset.percentage = tier.progress;
        }
        if (elements.sold) elements.sold.textContent = Utils.formatNumber(tier.sold);
        if (elements.remaining) elements.remaining.textContent = Utils.formatNumber(tier.remaining);
        if (elements.minPurchase) elements.minPurchase.textContent = Utils.formatNumber(tier.minPurchase);
        if (elements.maxPurchase) elements.maxPurchase.textContent = Utils.formatNumber(tier.maxPurchase);

        // Update form limits
        const amountInput = document.getElementById('purchaseAmount');
        if (amountInput) {
            amountInput.min = tier.minPurchase;
            amountInput.max = tier.maxPurchase;
        }
    },

    // Update live statistics
    updateLiveStats(presaleStats, tokenInfo) {
        const stats = {
            currentPrice: tokenInfo?.currentPrice || presaleStats?.currentTier?.price || 0.06,
            totalRaised: presaleStats?.totalRaised || 0,
            tokensSold: presaleStats?.tokensSold || 0,
            participants: presaleStats?.participantsCount || 0
        };

        Object.entries(stats).forEach(([key, value]) => {
            const element = document.querySelector(`[data-stat="${key}"]`);
            if (element) {
                if (key === 'currentPrice') {
                    element.textContent = Utils.formatCurrency(value);
                } else if (key === 'totalRaised') {
                    element.textContent = Utils.formatCurrency(value, 'USD', 1);
                } else if (key === 'tokensSold') {
                    element.textContent = Utils.formatNumber(value);
                } else {
                    element.textContent = Utils.formatNumber(value, 0);
                }
            }
        });
    },

    // Update tiers comparison
    updateTiersComparison(tiers) {
        const container = document.getElementById('tiersComparison');
        if (!container) return;

        container.innerHTML = tiers.map(tier => `
            <div class="tier-comparison-card ${tier.statusClass}" data-tier-id="${tier.id}">
                ${tier.status !== 'UPCOMING' ? `<div class="tier-status ${tier.statusClass}">${tier.status}</div>` : ''}
                
                <div class="tier-header">
                    <h3 class="tier-name">${tier.name}</h3>
                    <div class="tier-price">
                        <span class="price-value">${Utils.formatCurrency(tier.price)}</span>
                        <span class="price-label">per TEACH</span>
                    </div>
                </div>

                <div class="tier-progress">
                    <div class="progress-bar">
                        <div class="progress-fill" style="width: ${tier.progress}%"></div>
                    </div>
                    <div class="progress-stats">
                        <span>${Utils.formatNumber(tier.sold)} sold</span>
                        <span>${Utils.formatNumber(tier.remaining)} remaining</span>
                    </div>
                </div>

                <div class="tier-features">
                    <div class="feature-item">
                        <span class="feature-label">Allocation:</span>
                        <span class="feature-value">${Utils.formatNumber(tier.totalAllocation)} TEACH</span>
                    </div>
                    <div class="feature-item">
                        <span class="feature-label">Min Purchase:</span>
                        <span class="feature-value">${Utils.formatCurrency(tier.minPurchase)}</span>
                    </div>
                    <div class="feature-item">
                        <span class="feature-label">Max Purchase:</span>
                        <span class="feature-value">${Utils.formatCurrency(tier.maxPurchase)}</span>
                    </div>
                    <div class="feature-item">
                        <span class="feature-label">TGE Unlock:</span>
                        <span class="feature-value">${tier.vestingTGE}%</span>
                    </div>
                    <div class="feature-item">
                        <span class="feature-label">Vesting:</span>
                        <span class="feature-value">${tier.vestingMonths} months</span>
                    </div>
                </div>

                ${tier.isActive ? `
                    <button class="btn-tier-select" data-tier-id="${tier.id}">
                        Select This Tier
                    </button>
                ` : ''}
            </div>
        `).join('');

        // Add event listeners for tier selection
        container.querySelectorAll('.btn-tier-select').forEach(btn => {
            btn.addEventListener('click', (e) => {
                const tierId = parseInt(e.target.dataset.tierId);
                this.selectTier(tierId);
            });
        });
    },

    // Update payment methods
    updatePaymentMethods(methods) {
        const container = document.getElementById('paymentMethods');
        if (!container) return;

        container.innerHTML = methods.map(method => `
            <div class="payment-method ${method.isRecommended ? 'recommended' : ''}" data-method="${method.method}">
                <img src="${method.logo}" alt="${method.name}" class="method-logo">
                <div class="method-info">
                    <div class="method-name">${method.name}</div>
                    <div class="method-description">${method.description}</div>
                </div>
                ${method.isRecommended ? '<span class="method-recommended">Recommended</span>' : ''}
            </div>
        `).join('');

        // Add click handlers
        container.querySelectorAll('.payment-method').forEach(method => {
            method.addEventListener('click', () => {
                container.querySelectorAll('.payment-method').forEach(m => m.classList.remove('active'));
                method.classList.add('active');
            });
        });
    },

    // Update DEX integrations
    updateDexIntegrations(integrations) {
        const container = document.getElementById('dexGrid');
        if (!container) return;

        container.innerHTML = integrations.map(dex => `
            <div class="dex-integration-card">
                <img src="${dex.logo}" alt="${dex.name}" class="dex-logo">
                <h4>${dex.name}</h4>
                <p>${dex.description}</p>
                <div class="dex-status">
                    ${dex.isActive ?
                '<span class="status-active">Live</span>' :
                `<span class="status-upcoming">Launch: ${dex.launchDate.toLocaleDateString()}</span>`
            }
                </div>
            </div>
        `).join('');
    },

    // Update wallet UI
    updateWalletUI(address, walletInfo = null) {
        const walletInput = document.getElementById('walletAddress');
        const connectBtn = document.getElementById('connectWalletFormBtn');
        const navConnectBtn = document.getElementById('connectWalletBtn');
        const walletInfoDiv = document.getElementById('walletInfo');
        const purchaseBtn = document.getElementById('purchaseBtn');

        if (address) {
            // Connected state
            if (walletInput) {
                walletInput.value = `${address.slice(0, 6)}...${address.slice(-4)}`;
                walletInput.style.color = 'var(--success-color)';
            }

            if (connectBtn) {
                connectBtn.textContent = 'Connected';
                connectBtn.style.background = 'var(--success-color)';
                connectBtn.disabled = true;
            }

            if (navConnectBtn) {
                navConnectBtn.textContent = `${address.slice(0, 6)}...${address.slice(-4)}`;
                navConnectBtn.style.background = 'var(--success-color)';
            }

            if (walletInfoDiv && walletInfo) {
                walletInfoDiv.style.display = 'block';
                const usdcBalance = walletInfoDiv.querySelector('#usdcBalance');
                const teachBalance = walletInfoDiv.querySelector('#teachBalance');

                if (usdcBalance) usdcBalance.textContent = '0.00'; // Would get from wallet
                if (teachBalance) teachBalance.textContent = Utils.formatNumber(walletInfo.userInfo?.totalTokensPurchased || 0);
            }

            if (purchaseBtn) {
                purchaseBtn.disabled = false;
                purchaseBtn.querySelector('.btn-text').textContent = 'Purchase TEACH Tokens';
            }

            // Show user dashboard if user has purchases
            if (walletInfo?.userInfo?.totalTokensPurchased > 0) {
                this.showUserDashboard();
            }
        } else {
            // Disconnected state
            if (walletInput) {
                walletInput.value = '';
                walletInput.style.color = '';
            }

            if (connectBtn) {
                connectBtn.textContent = 'Connect';
                connectBtn.style.background = '';
                connectBtn.disabled = false;
            }

            if (navConnectBtn) {
                navConnectBtn.textContent = 'Connect Wallet';
                navConnectBtn.style.background = '';
            }

            if (walletInfoDiv) {
                walletInfoDiv.style.display = 'none';
            }

            if (purchaseBtn) {
                purchaseBtn.disabled = true;
                purchaseBtn.querySelector('.btn-text').textContent = 'Connect Wallet to Continue';
            }
        }
    },

    // Update purchase calculation
    updatePurchaseCalculation(calculation) {
        const container = document.getElementById('purchaseCalculation');
        if (!container) return;

        if (calculation && calculation.isValid) {
            container.style.display = 'block';

            const elements = {
                tokensToReceive: document.getElementById('tokensToReceive'),
                platformFee: document.getElementById('platformFee'),
                networkFee: document.getElementById('networkFee'),
                totalCost: document.getElementById('totalCost'),
                tgeTokens: document.getElementById('tgeTokens'),
                vestedTokens: document.getElementById('vestedTokens')
            };

            if (elements.tokensToReceive) {
                elements.tokensToReceive.textContent = Utils.formatTokens(calculation.tokensToReceive);
            }
            if (elements.platformFee) {
                elements.platformFee.textContent = Utils.formatCurrency(calculation.platformFee);
            }
            if (elements.networkFee) {
                elements.networkFee.textContent = Utils.formatCurrency(calculation.networkFee);
            }
            if (elements.totalCost) {
                elements.totalCost.textContent = Utils.formatCurrency(calculation.totalCost);
            }
            if (elements.tgeTokens) {
                elements.tgeTokens.textContent = Utils.formatTokens(calculation.vestingInfo.tgeTokens);
            }
            if (elements.vestedTokens) {
                elements.vestedTokens.textContent = Utils.formatTokens(calculation.vestingInfo.vestedTokens);
            }

            // Update purchase button
            const purchaseBtn = document.getElementById('purchaseBtn');
            if (purchaseBtn && BuyTradeState.connectedWallet) {
                purchaseBtn.disabled = false;
                purchaseBtn.querySelector('.btn-text').textContent =
                    `Purchase ${Utils.formatTokens(calculation.tokensToReceive)} for ${Utils.formatCurrency(calculation.totalCost)}`;
            }
        } else {
            this.hidePurchaseCalculation();
        }
    },

    // Hide purchase calculation
    hidePurchaseCalculation() {
        const container = document.getElementById('purchaseCalculation');
        if (container) {
            container.style.display = 'none';
        }

        const purchaseBtn = document.getElementById('purchaseBtn');
        if (purchaseBtn && BuyTradeState.connectedWallet) {
            purchaseBtn.disabled = true;
            purchaseBtn.querySelector('.btn-text').textContent = 'Enter purchase amount';
        }
    },

    // Show user dashboard
    showUserDashboard() {
        const dashboard = document.getElementById('userDashboard');
        if (dashboard) {
            dashboard.style.display = 'block';
        }
    },

    // Hide user dashboard
    hideUserDashboard() {
        const dashboard = document.getElementById('userDashboard');
        if (dashboard) {
            dashboard.style.display = 'none';
        }
    },

    // Update user dashboard
    updateUserDashboard(userInfo) {
        if (!userInfo) return;

        const elements = {
            totalTokens: document.getElementById('userTotalTokens'),
            totalUsd: document.getElementById('userTotalUsd'),
            avgPrice: document.getElementById('userAvgPrice'),
            claimableTokens: document.getElementById('userClaimableTokens'),
            nextVesting: document.getElementById('userNextVesting'),
            nextAmount: document.getElementById('userNextAmount'),
            claimBtn: document.getElementById('claimBtn')
        };

        if (elements.totalTokens) {
            elements.totalTokens.textContent = Utils.formatTokens(userInfo.totalTokensPurchased);
        }
        if (elements.totalUsd) {
            elements.totalUsd.textContent = Utils.formatCurrency(userInfo.totalUsdSpent);
        }
        if (elements.avgPrice) {
            const avgPrice = userInfo.totalUsdSpent / userInfo.totalTokensPurchased;
            elements.avgPrice.textContent = Utils.formatCurrency(avgPrice);
        }
        if (elements.claimableTokens) {
            elements.claimableTokens.textContent = Utils.formatTokens(userInfo.claimableTokens);
        }
        if (elements.nextVesting && userInfo.nextVestingDate) {
            elements.nextVesting.textContent = new Date(userInfo.nextVestingDate).toLocaleDateString();
        }
        if (elements.nextAmount) {
            elements.nextAmount.textContent = Utils.formatTokens(userInfo.nextVestingAmount);
        }
        if (elements.claimBtn) {
            elements.claimBtn.disabled = userInfo.claimableTokens <= 0;
        }

        // Update purchase history
        this.updatePurchaseHistory(userInfo.tierPurchases);
    },

    // Update purchase history
    updatePurchaseHistory(purchases) {
        const container = document.getElementById('purchaseHistory');
        if (!container || !purchases) return;

        if (purchases.length === 0) {
            container.innerHTML = '<div class="no-history">No purchases yet</div>';
            return;
        }

        container.innerHTML = purchases.map(purchase => `
            <div class="history-item">
                <div class="history-info">
                    <div class="history-tier">${purchase.tierName}</div>
                    <div class="history-amount">${Utils.formatCurrency(purchase.usdAmount)}</div>
                </div>
            </div>
        `).join('');
    },

    // Select tier
    selectTier(tierId) {
        const tier = BuyTradeState.pageData.allTiers?.find(t => t.id === tierId);
        if (tier && tier.isActive) {
            BuyTradeState.selectedTier = tier;
            this.updateCurrentTier(tier);

            // Scroll to purchase form
            document.getElementById('presale-section')?.scrollIntoView({
                behavior: 'smooth',
                block: 'center'
            });
        }
    },

    // Show error message
    showError(message) {
        const errorElement = document.getElementById('errorMessage');
        if (errorElement) {
            errorElement.textContent = message;
            errorElement.style.display = 'block';
        }
        Utils.showNotification(message, 'error');
    },

    // Show success message
    showSuccess(message) {
        const successElement = document.getElementById('successMessage');
        if (successElement) {
            successElement.textContent = message;
            successElement.style.display = 'block';
        }
        Utils.showNotification(message, 'success');
    },

    // Hide messages
    hideMessages() {
        const errorElement = document.getElementById('errorMessage');
        const successElement = document.getElementById('successMessage');

        if (errorElement) errorElement.style.display = 'none';
        if (successElement) successElement.style.display = 'none';
    }
};

// ================================================
// PURCHASE HANDLER
// ================================================
const PurchaseHandler = {
    // Handle purchase form submission
    async handlePurchase(event) {
        event.preventDefault();

        if (BuyTradeState.purchaseInProgress) {
            Utils.showNotification('Purchase already in progress', 'warning');
            return;
        }

        try {
            BuyTradeState.purchaseInProgress = true;
            UIController.hideMessages();

            // Comprehensive form validation
            const validationResult = await this.validatePurchaseForm();
            if (!validationResult.isValid) {
                UIController.showError(validationResult.error);
                return;
            }

            // Show loading state
            this.setLoadingState(true);

            // Pre-purchase validations
            await this.performPrePurchaseChecks(validationResult.data);

            // Validate with API
            const apiValidation = await ApiService.validatePurchase(
                BuyTradeState.connectedWallet.address,
                BuyTradeState.selectedTier.id,
                validationResult.data.amount
            );

            if (!apiValidation.isValid) {
                throw new Error(apiValidation.message || 'Purchase validation failed');
            }

            // Execute purchase
            await this.executePurchase(validationResult.data);

            // Success handling
            UIController.showSuccess('Purchase completed successfully! Tokens allocated to your wallet.');
            await this.handlePurchaseSuccess();

        } catch (error) {
            console.error('Purchase failed:', error);
            this.handlePurchaseError(error);
        } finally {
            BuyTradeState.purchaseInProgress = false;
            this.setLoadingState(false);
        }
    },

    async validatePurchaseForm() {
        const errors = [];

        // Wallet validation
        if (!BuyTradeState.connectedWallet) {
            errors.push('Please connect your wallet first');
        }

        // Tier validation
        if (!BuyTradeState.selectedTier) {
            errors.push('No tier selected');
        } else if (!BuyTradeState.selectedTier.isActive) {
            errors.push('Selected tier is not currently active');
        }

        // Amount validation
        const amountInput = document.getElementById('purchaseAmount');
        const amount = parseFloat(amountInput?.value || 0);

        if (!amount || amount <= 0) {
            errors.push('Please enter a valid purchase amount');
        } else if (BuyTradeState.selectedTier) {
            if (amount < BuyTradeState.selectedTier.minPurchase) {
                errors.push(`Minimum purchase is ${Utils.formatCurrency(BuyTradeState.selectedTier.minPurchase)}`);
            }
            if (amount > BuyTradeState.selectedTier.maxPurchase) {
                errors.push(`Maximum purchase is ${Utils.formatCurrency(BuyTradeState.selectedTier.maxPurchase)}`);
            }
        }

        // Price calculation validation
        if (!BuyTradeState.priceCalculation || !BuyTradeState.priceCalculation.isValid) {
            errors.push('Invalid purchase calculation - please refresh and try again');
        }

        if (errors.length > 0) {
            return { isValid: false, error: errors.join('. ') };
        }

        return {
            isValid: true,
            data: {
                amount: amount,
                address: BuyTradeState.connectedWallet.address,
                tier: BuyTradeState.selectedTier,
                calculation: BuyTradeState.priceCalculation
            }
        };
    },

    async performPrePurchaseChecks(data) {
        // Check wallet connection is still valid
        if (!window.ethereum || !window.ethereum.selectedAddress) {
            throw new Error('Wallet disconnected during purchase');
        }

        // Verify network
        const chainId = await window.ethereum.request({ method: 'eth_chainId' });
        if (chainId !== '0x1') {
            throw new Error('Please switch to Ethereum mainnet');
        }

        // Check if tier is still available
        const currentTiers = await ApiService.getBuyTradeData();
        const currentTier = currentTiers.allTiers?.find(t => t.id === data.tier.id);

        if (!currentTier || !currentTier.isActive) {
            throw new Error('Selected tier is no longer available');
        }

        // Check remaining allocation
        const tokensNeeded = data.calculation.tokensToReceive;
        if (tokensNeeded > currentTier.remaining) {
            throw new Error(`Insufficient tokens remaining in tier (${Utils.formatNumber(currentTier.remaining)} available)`);
        }
    },

    handlePurchaseError(error) {
        let errorMessage = 'Purchase failed. Please try again.';

        if (error instanceof WalletError) {
            switch (error.code) {
                case 'USER_REJECTED':
                    errorMessage = 'Transaction was rejected by user';
                    break;
                case 'INSUFFICIENT_FUNDS':
                    errorMessage = 'Insufficient funds in wallet';
                    break;
                case 'NETWORK_ERROR':
                    errorMessage = 'Network error. Please check your connection and try again.';
                    break;
                default:
                    errorMessage = error.message;
            }
        } else if (error instanceof ApiError) {
            switch (error.code) {
                case 'RATE_LIMITED':
                    errorMessage = 'Too many requests. Please wait a moment and try again.';
                    break;
                case 'TIMEOUT':
                    errorMessage = 'Request timeout. Please check your connection and try again.';
                    break;
                default:
                    errorMessage = error.message;
            }
        } else if (error.message) {
            errorMessage = error.message;
        }

        UIController.showError(errorMessage);
    },

    // Reset purchase form
    resetPurchaseForm() {
        const amountInput = document.getElementById('purchaseAmount');
        if (amountInput) {
            amountInput.value = '';
        }

        UIController.hidePurchaseCalculation();

        // Clear preset button selections
        document.querySelectorAll('.preset-btn').forEach(btn => {
            btn.classList.remove('active');
        });
    }
};

// ================================================
// EVENT HANDLERS
// ================================================
const EventHandlers = {
    // Initialize all event listeners
    init() {
        this.setupNavigationHandlers();
        this.setupWalletHandlers();
        this.setupPurchaseHandlers();
        this.setupUIHandlers();
        this.setupMethodTabHandlers();
        this.setupFAQHandlers();
    },

    // Navigation event handlers
    setupNavigationHandlers() {
        // Smooth scrolling for anchor links
        document.querySelectorAll('a[href^="#"]').forEach(link => {
            link.addEventListener('click', (e) => {
                e.preventDefault();
                const targetId = link.getAttribute('href').substring(1);
                const target = document.getElementById(targetId);
                if (target) {
                    target.scrollIntoView({ behavior: 'smooth', block: 'start' });
                }
            });
        });
    },

    // Wallet connection handlers
    setupWalletHandlers() {
        // Connect wallet buttons
        const connectButtons = [
            document.getElementById('connectWalletBtn'),
            document.getElementById('connectWalletFormBtn')
        ];

        connectButtons.forEach(btn => {
            if (btn) {
                btn.addEventListener('click', async () => {
                    try {
                        const address = await WalletService.connectWallet();
                        Utils.showNotification('Wallet connected successfully!', 'success');
                    } catch (error) {
                        Utils.showNotification(error.message, 'error');
                    }
                });
            }
        });

        // Disconnect handler (if wallet is already connected)
        document.addEventListener('click', (e) => {
            if (e.target.id === 'disconnectWallet') {
                WalletService.disconnectWallet();
                Utils.showNotification('Wallet disconnected', 'info');
            }
        });
    },

    // Purchase form handlers
    setupPurchaseHandlers() {
        // Purchase form submission
        const purchaseForm = document.getElementById('purchaseForm');
        if (purchaseForm) {
            purchaseForm.addEventListener('submit', PurchaseHandler.handlePurchase.bind(PurchaseHandler));
        }

        // Amount input handler
        const amountInput = document.getElementById('purchaseAmount');
        if (amountInput) {
            amountInput.addEventListener('input', (e) => {
                const amount = parseFloat(e.target.value) || 0;
                BuyTradeState.currentPurchaseAmount = amount;

                if (BuyTradeState.connectedWallet && BuyTradeState.selectedTier && amount > 0) {
                    DataManager.calculatePrice(
                        BuyTradeState.connectedWallet.address,
                        BuyTradeState.selectedTier.id,
                        amount
                    );
                }
            });
        }

        // Preset amount buttons
        document.querySelectorAll('.preset-btn').forEach(btn => {
            btn.addEventListener('click', () => {
                const amount = parseFloat(btn.dataset.amount);
                if (amountInput) {
                    amountInput.value = amount;
                    amountInput.dispatchEvent(new Event('input'));
                }

                // Update active state
                document.querySelectorAll('.preset-btn').forEach(b => b.classList.remove('active'));
                btn.classList.add('active');
            });
        });

        // Claim button handler
        const claimBtn = document.getElementById('claimBtn');
        if (claimBtn) {
            claimBtn.addEventListener('click', async () => {
                try {
                    // Simulate claim process
                    claimBtn.disabled = true;
                    claimBtn.textContent = 'Claiming...';

                    await new Promise(resolve => setTimeout(resolve, 2000));

                    Utils.showNotification('Tokens claimed successfully!', 'success');

                    // Refresh user data
                    if (BuyTradeState.connectedWallet) {
                        await WalletService.loadUserData(BuyTradeState.connectedWallet.address);
                    }
                } catch (error) {
                    Utils.showNotification('Claim failed. Please try again.', 'error');
                } finally {
                    claimBtn.disabled = false;
                    claimBtn.innerHTML = '<span>Claim Tokens</span><span>🎯</span>';
                }
            });
        }
    },

    // UI interaction handlers
    setupUIHandlers() {
        // Tier selection from comparison cards
        document.addEventListener('click', (e) => {
            if (e.target.classList.contains('btn-tier-select')) {
                const tierId = parseInt(e.target.dataset.tierId);
                UIController.selectTier(tierId);
            }
        });
    },

    // Method tab handlers
    setupMethodTabHandlers() {
        document.querySelectorAll('.method-tab').forEach(tab => {
            tab.addEventListener('click', () => {
                // Update active tab
                document.querySelectorAll('.method-tab').forEach(t => t.classList.remove('active'));
                tab.classList.add('active');

                // Show/hide relevant sections
                const method = tab.dataset.method;
                const presaleSection = document.getElementById('presale-section');
                const dexSection = document.getElementById('dex-section');

                if (method === 'presale') {
                    if (presaleSection) presaleSection.style.display = 'block';
                    if (dexSection) dexSection.style.display = 'none';
                } else if (method === 'dex') {
                    if (presaleSection) presaleSection.style.display = 'none';
                    if (dexSection) dexSection.style.display = 'block';
                }
            });
        });
    },

    // FAQ accordion handlers
    setupFAQHandlers() {
        document.querySelectorAll('.faq-question').forEach(question => {
            question.addEventListener('click', () => {
                const faqItem = question.parentElement;
                const isActive = faqItem.classList.contains('active');

                // Close all FAQ items
                document.querySelectorAll('.faq-item').forEach(item => {
                    item.classList.remove('active');
                });

                // Open clicked item if it wasn't active
                if (!isActive) {
                    faqItem.classList.add('active');
                }
            });
        });
    }
};

// ================================================
// APPLICATION INITIALIZATION
// ================================================
const BuyTradeApp = {
    async init() {
        try {
            console.log('Initializing Buy/Trade page...');

            // Initialize event handlers
            EventHandlers.init();

            // Load page data
            await DataManager.loadPageData();

            // Start periodic updates
            DataManager.startPeriodicUpdates();

            // Check if wallet is already connected
            if (WalletService.isMetaMaskAvailable() && window.ethereum.selectedAddress) {
                try {
                    await WalletService.connectWallet();
                } catch (error) {
                    console.log('Auto-connect failed, user needs to connect manually');
                }
            }

            BuyTradeState.isLoaded = true;
            console.log('Buy/Trade page initialized successfully');

        } catch (error) {
            console.error('Failed to initialize Buy/Trade page:', error);
            Utils.showNotification('Page failed to load completely', 'error');
        }
    }

};

// Custom Wallet Error class
class WalletError extends Error {
    constructor(message, code, details = {}) {
        super(message);
        this.name = 'WalletError';
        this.code = code;
        this.details = details;
    }
}

// Custom API Error class
class ApiError extends Error {
    constructor(message, code, details = {}) {
        super(message);
        this.name = 'ApiError';
        this.code = code;
        this.details = details;
    }
}

// ================================================
// DOCUMENT READY & INITIALIZATION
// ================================================
document.addEventListener('DOMContentLoaded', () => {
    BuyTradeApp.init();
});

// Handle page visibility changes
document.addEventListener('visibilitychange', () => {
    if (document.visibilityState === 'visible' && BuyTradeState.isLoaded) {
        // Refresh data when page becomes visible
        DataManager.loadPageData();
    }
});

// Export for global access if needed
window.BuyTradeApp = {
    Utils,
    ApiService,
    WalletService,
    DataManager,
    UIController,
    PurchaseHandler,
    BuyTradeState
};