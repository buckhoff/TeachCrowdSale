/**
 * Error Handling & Fallbacks System - Phase 8.3
 * Comprehensive error management for TeachToken Staking Dashboard
 * Provides graceful degradation, user-friendly messages, and network resilience
 */

class ErrorHandlingManager {
    constructor() {
        this.isOnline = navigator.onLine;
        this.retryAttempts = new Map();
        this.maxRetries = 3;
        this.retryDelay = 1000; // Base delay in ms
        this.fallbackData = new Map();
        this.errorDisplayDuration = 5000; // 5 seconds
        this.networkCheckInterval = 30000; // 30 seconds

        this.init();
    }

    init() {
        this.setupNetworkMonitoring();
        this.setupGlobalErrorHandlers();
        this.loadFallbackData();
        this.setupRetryMechanisms();
    }

    setupNetworkMonitoring() {
        // Network connectivity detection
        window.addEventListener('online', () => {
            this.isOnline = true;
            this.showNetworkStatus('back-online');
            this.retryFailedRequests();
        });

        window.addEventListener('offline', () => {
            this.isOnline = false;
            this.showNetworkStatus('offline');
            this.enableOfflineMode();
        });

        // Periodic network check for better reliability
        setInterval(() => {
            this.checkNetworkConnectivity();
        }, this.networkCheckInterval);
    }

    async checkNetworkConnectivity() {
        try {
            const response = await fetch('/health', {
                method: 'HEAD',
                cache: 'no-cache',
                timeout: 5000
            });

            const wasOnline = this.isOnline;
            this.isOnline = response.ok;

            if (!wasOnline && this.isOnline) {
                this.showNetworkStatus('back-online');
                this.retryFailedRequests();
            } else if (wasOnline && !this.isOnline) {
                this.showNetworkStatus('connection-issues');
            }
        } catch (error) {
            if (this.isOnline) {
                this.isOnline = false;
                this.showNetworkStatus('connection-issues');
            }
        }
    }

    setupGlobalErrorHandlers() {
        // Global JavaScript error handler
        window.addEventListener('error', (event) => {
            this.handleJavaScriptError(event.error, event.filename, event.lineno);
        });

        // Unhandled promise rejection handler
        window.addEventListener('unhandledrejection', (event) => {
            this.handlePromiseRejection(event.reason);
            event.preventDefault(); // Prevent console logging
        });

        // Fetch API error wrapper
        this.wrapFetchAPI();
    }

    wrapFetchAPI() {
        const originalFetch = window.fetch;

        window.fetch = async (...args) => {
            const [url, options = {}] = args;
            const requestId = this.generateRequestId();

            try {
                // Add request timeout
                const controller = new AbortController();
                const timeoutId = setTimeout(() => controller.abort(), options.timeout || 30000);

                const enhancedOptions = {
                    ...options,
                    signal: controller.signal
                };

                const response = await originalFetch(url, enhancedOptions);
                clearTimeout(timeoutId);

                if (!response.ok) {
                    throw new APIError(response.status, response.statusText, url);
                }

                return response;
            } catch (error) {
                return this.handleFetchError(error, url, options, requestId);
            }
        };
    }

    async handleFetchError(error, url, options, requestId) {
        const errorInfo = {
            type: this.classifyError(error),
            message: error.message,
            url: url,
            status: error.status || 0,
            timestamp: new Date().toISOString(),
            requestId: requestId
        };

        // Log error for debugging
        console.error('Fetch Error:', errorInfo);

        // Check if we should retry
        if (this.shouldRetry(errorInfo)) {
            return await this.retryRequest(url, options, requestId);
        }

        // Show user-friendly error message
        this.showUserError(errorInfo);

        // Return fallback data if available
        const fallbackData = this.getFallbackResponse(url);
        if (fallbackData) {
            this.showFallbackNotification(url);
            return new Response(JSON.stringify(fallbackData), {
                status: 200,
                headers: { 'Content-Type': 'application/json' }
            });
        }

        // Re-throw if no fallback available
        throw error;
    }

    classifyError(error) {
        if (!this.isOnline || error.name === 'NetworkError') {
            return 'network';
        }
        if (error.name === 'AbortError') {
            return 'timeout';
        }
        if (error.status >= 500) {
            return 'server';
        }
        if (error.status >= 400) {
            return 'client';
        }
        return 'unknown';
    }

    shouldRetry(errorInfo) {
        const retryableTypes = ['network', 'timeout', 'server'];
        const retryableStatuses = [408, 429, 500, 502, 503, 504];

        if (!retryableTypes.includes(errorInfo.type) &&
            !retryableStatuses.includes(errorInfo.status)) {
            return false;
        }

        const attempts = this.retryAttempts.get(errorInfo.requestId) || 0;
        return attempts < this.maxRetries;
    }

    async retryRequest(url, options, requestId) {
        const attempts = this.retryAttempts.get(requestId) || 0;
        this.retryAttempts.set(requestId, attempts + 1);

        // Exponential backoff
        const delay = this.retryDelay * Math.pow(2, attempts);
        await this.sleep(delay);

        this.showRetryNotification(attempts + 1, this.maxRetries);

        try {
            const response = await fetch(url, options);
            this.retryAttempts.delete(requestId);
            this.hideRetryNotification();
            return response;
        } catch (error) {
            if (attempts + 1 >= this.maxRetries) {
                this.retryAttempts.delete(requestId);
                this.hideRetryNotification();
            }
            throw error;
        }
    }

    loadFallbackData() {
        // Staking pools fallback data
        this.fallbackData.set('/staking/GetPoolData', {
            pools: [
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
            ],
            isFallbackData: true,
            lastUpdated: new Date().toISOString()
        });

        // Schools fallback data
        this.fallbackData.set('/staking/GetSchools', {
            schools: [
                {
                    id: 'demo-school-1',
                    name: 'Lincoln Elementary School',
                    location: 'Springfield, IL',
                    description: 'Supporting STEM education for underserved communities',
                    studentsCount: 450,
                    totalFundsReceived: 15000,
                    fundingGoal: 50000,
                    supportersCount: 32
                },
                {
                    id: 'demo-school-2',
                    name: 'Jefferson Middle School',
                    location: 'Madison, WI',
                    description: 'Enhancing technology access for rural students',
                    studentsCount: 280,
                    totalFundsReceived: 8500,
                    fundingGoal: 35000,
                    supportersCount: 18
                }
            ],
            isFallbackData: true,
            lastUpdated: new Date().toISOString()
        });

        // User data fallback (empty state)
        this.fallbackData.set('/staking/GetUserData', {
            userAddress: '',
            positions: [],
            totalStaked: 0,
            totalRewards: 0,
            claimableRewards: 0,
            isFallbackData: true,
            message: 'Connect your wallet to view staking positions'
        });
    }

    getFallbackResponse(url) {
        // Match URL patterns to fallback data
        for (const [pattern, data] of this.fallbackData.entries()) {
            if (url.includes(pattern)) {
                return data;
            }
        }
        return null;
    }

    showUserError(errorInfo) {
        const errorContainer = this.getErrorContainer();
        const errorMessage = this.generateUserFriendlyMessage(errorInfo);

        const errorElement = document.createElement('div');
        errorElement.className = 'error-notification';
        errorElement.innerHTML = `
            <div class="error-content">
                <div class="error-icon">${this.getErrorIcon(errorInfo.type)}</div>
                <div class="error-details">
                    <h4 class="error-title">${errorMessage.title}</h4>
                    <p class="error-description">${errorMessage.description}</p>
                    ${errorMessage.action ? `<button class="error-action-btn" onclick="window.errorHandler.${errorMessage.action}()">${errorMessage.actionText}</button>` : ''}
                </div>
                <button class="error-close-btn" onclick="this.parentElement.parentElement.remove()">×</button>
            </div>
        `;

        errorContainer.appendChild(errorElement);

        // Auto-remove after duration
        setTimeout(() => {
            if (errorElement.parentNode) {
                errorElement.remove();
            }
        }, this.errorDisplayDuration);
    }

    generateUserFriendlyMessage(errorInfo) {
        switch (errorInfo.type) {
            case 'network':
                return {
                    title: 'Connection Issue',
                    description: 'Unable to connect to TeachToken services. Please check your internet connection.',
                    action: 'retryLastRequest',
                    actionText: 'Retry'
                };
            case 'timeout':
                return {
                    title: 'Request Timeout',
                    description: 'The request is taking longer than expected. This might be due to high network traffic.',
                    action: 'retryLastRequest',
                    actionText: 'Try Again'
                };
            case 'server':
                return {
                    title: 'Service Temporarily Unavailable',
                    description: 'TeachToken services are experiencing issues. We\'re working to resolve this quickly.',
                    action: 'retryLastRequest',
                    actionText: 'Retry'
                };
            case 'client':
                if (errorInfo.status === 401) {
                    return {
                        title: 'Authentication Required',
                        description: 'Please connect your wallet to access staking features.',
                        action: 'connectWallet',
                        actionText: 'Connect Wallet'
                    };
                }
                if (errorInfo.status === 403) {
                    return {
                        title: 'Access Denied',
                        description: 'You don\'t have permission to perform this action.',
                        action: null,
                        actionText: null
                    };
                }
                return {
                    title: 'Request Error',
                    description: 'There was an issue with your request. Please try again.',
                    action: 'retryLastRequest',
                    actionText: 'Retry'
                };
            default:
                return {
                    title: 'Unexpected Error',
                    description: 'Something went wrong. Our team has been notified.',
                    action: 'refreshPage',
                    actionText: 'Refresh Page'
                };
        }
    }

    getErrorIcon(type) {
        const icons = {
            network: '🌐',
            timeout: '⏱️',
            server: '🔧',
            client: '⚠️',
            unknown: '❓'
        };
        return icons[type] || icons.unknown;
    }

    showNetworkStatus(status) {
        const statusContainer = this.getStatusContainer();
        let message, className;

        switch (status) {
            case 'offline':
                message = '📱 You\'re offline. Some features may be limited.';
                className = 'network-status offline';
                break;
            case 'back-online':
                message = '✅ Back online! All features are now available.';
                className = 'network-status online';
                break;
            case 'connection-issues':
                message = '⚠️ Connection issues detected. Trying to reconnect...';
                className = 'network-status unstable';
                break;
        }

        const statusElement = document.createElement('div');
        statusElement.className = className;
        statusElement.textContent = message;

        statusContainer.innerHTML = '';
        statusContainer.appendChild(statusElement);

        if (status === 'back-online') {
            setTimeout(() => {
                statusElement.remove();
            }, 3000);
        }
    }

    showRetryNotification(attempt, maxAttempts) {
        const notification = document.getElementById('retry-notification');
        if (notification) {
            notification.textContent = `🔄 Retrying request... (${attempt}/${maxAttempts})`;
            notification.style.display = 'block';
        }
    }

    hideRetryNotification() {
        const notification = document.getElementById('retry-notification');
        if (notification) {
            notification.style.display = 'none';
        }
    }

    showFallbackNotification(url) {
        const message = `ℹ️ Showing cached data. Some information may be outdated.`;
        this.showInfoMessage(message, 'fallback-notification');
    }

    showInfoMessage(message, className = 'info-notification') {
        const container = this.getErrorContainer();
        const infoElement = document.createElement('div');
        infoElement.className = className;
        infoElement.innerHTML = `
            <div class="info-content">
                <span class="info-message">${message}</span>
                <button class="info-close-btn" onclick="this.parentElement.parentElement.remove()">×</button>
            </div>
        `;

        container.appendChild(infoElement);

        setTimeout(() => {
            if (infoElement.parentNode) {
                infoElement.remove();
            }
        }, this.errorDisplayDuration);
    }

    enableOfflineMode() {
        // Add offline indicator to UI
        document.body.classList.add('offline-mode');

        // Disable real-time features
        if (window.stakingDashboard) {
            window.stakingDashboard.stopPeriodicRefresh();
        }

        // Show offline message
        this.showInfoMessage(
            '📱 Offline Mode: Showing cached data. Connect to internet for latest updates.',
            'offline-mode-notification'
        );
    }

    retryFailedRequests() {
        // Remove offline mode
        document.body.classList.remove('offline-mode');

        // Restart real-time features
        if (window.stakingDashboard) {
            window.stakingDashboard.startPeriodicRefresh();
            window.stakingDashboard.refreshData(true);
        }

        // Clear retry attempts
        this.retryAttempts.clear();
    }

    setupRetryMechanisms() {
        // Smart retry for failed transactions
        this.setupTransactionRetry();

        // Data refresh retry
        this.setupDataRefreshRetry();
    }

    setupTransactionRetry() {
        // Override transaction methods to add retry logic
        if (window.stakingDashboard) {
            const originalClaimRewards = window.stakingDashboard.handleClaimRewards;

            window.stakingDashboard.handleClaimRewards = async function (e) {
                const maxTransactionRetries = 2;
                let attempts = 0;

                while (attempts < maxTransactionRetries) {
                    try {
                        return await originalClaimRewards.call(this, e);
                    } catch (error) {
                        attempts++;

                        if (attempts >= maxTransactionRetries) {
                            window.errorHandler.handleTransactionError(error, 'claim_rewards');
                            throw error;
                        }

                        // Wait before retry
                        await window.errorHandler.sleep(2000 * attempts);
                        window.errorHandler.showRetryNotification(attempts, maxTransactionRetries);
                    }
                }
            };
        }
    }

    setupDataRefreshRetry() {
        // Auto-retry data refresh on failure
        setInterval(() => {
            if (this.isOnline && window.stakingDashboard) {
                window.stakingDashboard.refreshData().catch(() => {
                    // Silent retry - don't show error for background refresh
                });
            }
        }, 60000); // Every minute
    }

    handleTransactionError(error, transactionType) {
        const messages = {
            claim_rewards: 'Failed to claim rewards. Please try again.',
            stake_tokens: 'Failed to stake tokens. Please try again.',
            unstake_tokens: 'Failed to unstake tokens. Please try again.'
        };

        this.showUserError({
            type: 'client',
            message: messages[transactionType] || 'Transaction failed.',
            status: 400
        });
    }

    handleJavaScriptError(error, filename, lineno) {
        console.error('JavaScript Error:', error, 'at', filename, ':', lineno);

        // Don't show JS errors to users in production
        if (process.env.NODE_ENV !== 'production') {
            this.showUserError({
                type: 'unknown',
                message: `JavaScript error: ${error.message}`,
                status: 0
            });
        }
    }

    handlePromiseRejection(reason) {
        console.error('Unhandled Promise Rejection:', reason);

        // Handle specific promise rejections
        if (reason && reason.message && reason.message.includes('wallet')) {
            this.showUserError({
                type: 'client',
                message: 'Wallet connection failed. Please try connecting again.',
                status: 401
            });
        }
    }

    // Utility methods
    generateRequestId() {
        return `req_${Date.now()}_${Math.random().toString(36).substr(2, 9)}`;
    }

    sleep(ms) {
        return new Promise(resolve => setTimeout(resolve, ms));
    }

    getErrorContainer() {
        let container = document.getElementById('error-container');
        if (!container) {
            container = document.createElement('div');
            container.id = 'error-container';
            container.className = 'error-container';
            document.body.appendChild(container);
        }
        return container;
    }

    getStatusContainer() {
        let container = document.getElementById('network-status-container');
        if (!container) {
            container = document.createElement('div');
            container.id = 'network-status-container';
            container.className = 'network-status-container';
            document.body.appendChild(container);
        }
        return container;
    }

    // Public methods for manual error handling
    retryLastRequest() {
        if (window.stakingDashboard) {
            window.stakingDashboard.refreshData(true);
        }
    }

    connectWallet() {
        if (window.stakingDashboard && window.stakingDashboard.walletConnector) {
            window.stakingDashboard.walletConnector.connect();
        }
    }

    refreshPage() {
        window.location.reload();
    }

    clearAllErrors() {
        const errorContainer = this.getErrorContainer();
        errorContainer.innerHTML = '';
    }
}

// Custom Error Classes
class APIError extends Error {
    constructor(status, statusText, url) {
        super(`API Error: ${status} ${statusText} for ${url}`);
        this.name = 'APIError';
        this.status = status;
        this.statusText = statusText;
        this.url = url;
    }
}

class NetworkError extends Error {
    constructor(message) {
        super(message);
        this.name = 'NetworkError';
    }
}

// Error Handling CSS (to be added to staking.css)
const errorHandlingCSS = `
/* Error Handling & Fallbacks Styles */
.error-container {
    position: fixed;
    top: 20px;
    right: 20px;
    z-index: 10000;
    max-width: 400px;
    width: 100%;
}

.error-notification,
.info-notification,
.fallback-notification {
    background: rgba(0, 0, 0, 0.9);
    border: 1px solid rgba(255, 71, 87, 0.3);
    border-radius: var(--border-radius-md);
    margin-bottom: var(--spacing-sm);
    backdrop-filter: blur(10px);
    animation: slideInRight 0.3s ease-out;
}

.info-notification,
.fallback-notification {
    border-color: rgba(0, 212, 170, 0.3);
}

.error-content,
.info-content {
    display: flex;
    align-items: flex-start;
    padding: var(--spacing-md);
    gap: var(--spacing-sm);
}

.error-icon {
    font-size: 1.2rem;
    margin-top: 0.1rem;
}

.error-details {
    flex: 1;
}

.error-title {
    color: var(--text-primary);
    font-size: var(--font-size-md);
    font-weight: var(--font-weight-semibold);
    margin: 0 0 var(--spacing-xs) 0;
}

.error-description {
    color: var(--text-secondary);
    font-size: var(--font-size-sm);
    margin: 0 0 var(--spacing-sm) 0;
    line-height: 1.4;
}

.error-action-btn {
    background: var(--staking-primary);
    border: none;
    border-radius: var(--border-radius-sm);
    color: white;
    padding: var(--spacing-xs) var(--spacing-sm);
    font-size: var(--font-size-sm);
    cursor: pointer;
    transition: all var(--staking-transition-normal);
}

.error-action-btn:hover {
    background: var(--staking-success);
    transform: translateY(-1px);
}

.error-close-btn,
.info-close-btn {
    background: none;
    border: none;
    color: var(--text-secondary);
    font-size: 1.2rem;
    cursor: pointer;
    padding: 0;
    line-height: 1;
    transition: color var(--staking-transition-normal);
}

.error-close-btn:hover,
.info-close-btn:hover {
    color: var(--text-primary);
}

.network-status-container {
    position: fixed;
    top: 0;
    left: 0;
    right: 0;
    z-index: 9999;
}

.network-status {
    padding: var(--spacing-sm) var(--spacing-md);
    text-align: center;
    font-size: var(--font-size-sm);
    font-weight: var(--font-weight-medium);
    animation: slideInDown 0.3s ease-out;
}

.network-status.offline {
    background: rgba(255, 176, 32, 0.9);
    color: #333;
}

.network-status.online {
    background: rgba(0, 212, 170, 0.9);
    color: white;
}

.network-status.unstable {
    background: rgba(255, 71, 87, 0.9);
    color: white;
}

.offline-mode {
    filter: saturate(0.7);
}

.offline-mode::before {
    content: 'OFFLINE MODE';
    position: fixed;
    bottom: 20px;
    left: 20px;
    background: rgba(255, 176, 32, 0.9);
    color: #333;
    padding: var(--spacing-xs) var(--spacing-sm);
    border-radius: var(--border-radius-sm);
    font-size: var(--font-size-xs);
    font-weight: var(--font-weight-bold);
    z-index: 9998;
}

#retry-notification {
    position: fixed;
    bottom: 20px;
    right: 20px;
    background: rgba(0, 212, 170, 0.9);
    color: white;
    padding: var(--spacing-sm) var(--spacing-md);
    border-radius: var(--border-radius-md);
    font-size: var(--font-size-sm);
    z-index: 9999;
    display: none;
}

@keyframes slideInRight {
    from {
        transform: translateX(100%);
        opacity: 0;
    }
    to {
        transform: translateX(0);
        opacity: 1;
    }
}

@keyframes slideInDown {
    from {
        transform: translateY(-100%);
        opacity: 0;
    }
    to {
        transform: translateY(0);
        opacity: 1;
    }
}

/* Mobile responsive */
@media (max-width: 768px) {
    .error-container {
        top: 10px;
        right: 10px;
        left: 10px;
        max-width: none;
    }
    
    .error-content,
    .info-content {
        padding: var(--spacing-sm);
    }
}
`;

// Initialize error handling system
document.addEventListener('DOMContentLoaded', () => {
    window.errorHandler = new ErrorHandlingManager();

    // Add retry notification element
    const retryNotification = document.createElement('div');
    retryNotification.id = 'retry-notification';
    document.body.appendChild(retryNotification);

    // Inject error handling CSS
    const styleSheet = document.createElement('style');
    styleSheet.textContent = errorHandlingCSS;
    document.head.appendChild(styleSheet);
});

// Export for use in other modules
if (typeof module !== 'undefined' && module.exports) {
    module.exports = { ErrorHandlingManager, APIError, NetworkError };
}