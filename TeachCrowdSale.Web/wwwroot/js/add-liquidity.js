// ================================================
// ADD LIQUIDITY WIZARD - JAVASCRIPT
// Following TeachToken patterns for multi-step forms
// ================================================

window.AddLiquidityWizard = (function () {
    'use strict';

    // ================================================
    // CONFIGURATION & STATE
    // ================================================
    const config = {
        endpoints: {
            wizardData: '/liquidity/wizard-data',
            comparePools: '/liquidity/compare-pools',
            calculate: '/liquidity/calculate',
            calculateIL: '/liquidity/calculate-il',
            projections: '/liquidity/projections',
            validate: '/liquidity/validate',
            dexUrl: '/liquidity/dex-url'
        },
        steps: {
            total: 5,
            current: 1
        },
        validation: {
            minAmount: 1,
            maxSlippage: 50
        }
    };

    let state = {
        currentStep: 1,
        wizardData: null,
        selectedPool: null,
        selectedDex: null,
        tokenAmounts: {
            token1: 0,
            token2: 0
        },
        calculatedData: null,
        walletConnected: false,
        walletAddress: null,
        walletBalance: {
            eth: 0,
            token1: 0,
            token2: 0
        }
    };

    // ================================================
    // INITIALIZATION
    // ================================================
    function init(initialData) {
        console.log('🧙‍♂️ Initializing Add Liquidity Wizard...');

        try {
            // Store initial data
            if (initialData && Object.keys(initialData).length > 0) {
                state.wizardData = initialData;
                console.log('✅ Initial wizard data loaded:', initialData);
            }

            // Initialize components
            initializeEventListeners();
            initializeWizardNavigation();
            initializeStepValidation();

            // Check for pre-selected pool from URL
            checkUrlParameters();

            // Load initial step
            showStep(1);

            console.log('✅ Add Liquidity Wizard initialized successfully');
        } catch (error) {
            console.error('❌ Error initializing Add Liquidity Wizard:', error);
            showError('Failed to initialize wizard. Please refresh the page.');
        }
    }

    // ================================================
    // EVENT LISTENERS
    // ================================================
    function initializeEventListeners() {
        // Wallet connection
        const walletOptions = document.querySelectorAll('.wallet-option');
        walletOptions.forEach(option => {
            option.addEventListener('click', function () {
                const walletType = this.dataset.wallet;
                connectWallet(walletType);
            });
        });

        // Pool selection
        document.addEventListener('change', function (e) {
            if (e.target.name === 'selected-pool') {
                handlePoolSelection(e.target.value);
            }
            if (e.target.name === 'selected-dex') {
                handleDexSelection(e.target.value);
            }
        });

        // Pool search
        const poolSearchWizard = document.getElementById('pool-search-wizard');
        if (poolSearchWizard) {
            poolSearchWizard.addEventListener('input', debounce(handlePoolSearch, 300));
        }

        // Token amount inputs
        const token1Input = document.getElementById('token1-amount');
        const token2Input = document.getElementById('token2-amount');

        if (token2UsdElement && state.selectedPool) {
            const usdValue = state.tokenAmounts.token2 * (state.selectedPool.token2Price || 0);
            token2UsdElement.textContent = `${usdValue.toFixed(2)}`;
        }
    }

    function updatePoolPositionPreview() {
        if (!state.calculatedData) return;

        const totalDepositElement = document.getElementById('total-deposit-value');
        const poolShareElement = document.getElementById('pool-share');
        const lpTokensElement = document.getElementById('lp-tokens');
        const dailyEarningsElement = document.getElementById('daily-earnings');

        if (totalDepositElement) {
            totalDepositElement.textContent = `${state.calculatedData.totalDepositValue?.toFixed(2) || '0.00'}`;
        }

        if (poolShareElement) {
            poolShareElement.textContent = `${state.calculatedData.poolSharePercentage?.toFixed(4) || '0.00'}%`;
        }

        if (lpTokensElement) {
            lpTokensElement.textContent = state.calculatedData.lpTokensReceived?.toFixed(6) || '0.00';
        }

        if (dailyEarningsElement) {
            dailyEarningsElement.textContent = `${state.calculatedData.estimatedDailyEarnings?.toFixed(2) || '0.00'}`;
        }
    }

    async function updateImpermanentLossCalculation() {
        if (!state.selectedPool || !state.tokenAmounts.token1 || !state.tokenAmounts.token2) return;

        try {
            const response = await fetch(config.endpoints.calculateIL, {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json'
                },
                body: JSON.stringify({
                    poolId: state.selectedPool.id,
                    token1Amount: state.tokenAmounts.token1,
                    token2Amount: state.tokenAmounts.token2
                })
            });

            if (!response.ok) return; // Silent fail for IL calculation

            const data = await response.json();

            // Update IL scenarios
            const scenarios = document.querySelectorAll('.il-scenario .il-value');
            if (scenarios.length >= 3 && data.scenarios) {
                scenarios[0].textContent = `${data.scenarios.change25?.toFixed(1) || '-0.6'}%`;
                scenarios[1].textContent = `${data.scenarios.change50?.toFixed(1) || '-2.0'}%`;
                scenarios[2].textContent = `${data.scenarios.change100?.toFixed(1) || '-5.7'}%`;
            }

        } catch (error) {
            console.error('❌ Error calculating impermanent loss:', error);
            // Silent fail - use default values
        }
    }

    // ================================================
    // REVIEW & CONFIRM (Step 5)
    // ================================================
    function updateReviewData() {
        if (!state.selectedPool || !state.selectedDex) return;

        // Update review summary
        const reviewPool = document.getElementById('review-pool');
        const reviewDex = document.getElementById('review-dex');
        const reviewToken1 = document.getElementById('review-token1');
        const reviewToken2 = document.getElementById('review-token2');
        const reviewTotal = document.getElementById('review-total');
        const reviewApy = document.getElementById('review-apy');

        if (reviewPool) {
            reviewPool.textContent = `${state.selectedPool.token1Symbol}/${state.selectedPool.token2Symbol}`;
        }

        if (reviewDex) {
            reviewDex.textContent = state.selectedDex.name;
        }

        if (reviewToken1) {
            reviewToken1.textContent = `${state.tokenAmounts.token1.toFixed(4)} ${state.selectedPool.token1Symbol}`;
        }

        if (reviewToken2) {
            reviewToken2.textContent = `${state.tokenAmounts.token2.toFixed(4)} ${state.selectedPool.token2Symbol}`;
        }

        if (reviewTotal && state.calculatedData) {
            reviewTotal.textContent = `${state.calculatedData.totalDepositValue?.toFixed(2) || '0.00'}`;
        }

        if (reviewApy) {
            reviewApy.textContent = `${state.selectedPool.apy.toFixed(2)}%`;
        }

        // Update cost breakdown
        updateCostBreakdown();

        // Update final DEX name
        const finalDexName = document.getElementById('final-dex-name');
        const dexNameBtn = document.getElementById('dex-name-btn');

        if (finalDexName) finalDexName.textContent = state.selectedDex.name;
        if (dexNameBtn) dexNameBtn.textContent = state.selectedDex.name;
    }

    async function updateCostBreakdown() {
        try {
            // Estimate gas fees
            const gasEstimate = await estimateGasFees();
            const reviewGas = document.getElementById('review-gas');
            if (reviewGas) {
                reviewGas.textContent = `~${gasEstimate.toFixed(2)}`;
            }

            // Update DEX fee
            const reviewDexFee = document.getElementById('review-dex-fee');
            if (reviewDexFee && state.selectedPool) {
                reviewDexFee.textContent = `${(state.selectedPool.feeTier * 100)}%`;
            }

        } catch (error) {
            console.error('❌ Error updating cost breakdown:', error);
        }
    }

    async function estimateGasFees() {
        try {
            if (typeof window.ethereum !== 'undefined') {
                const gasPrice = await window.ethereum.request({
                    method: 'eth_gasPrice'
                });

                // Estimate gas limit for liquidity addition (typical: 200k gas)
                const gasLimit = 200000;
                const gasPriceGwei = parseInt(gasPrice, 16) / 1e9;
                const gasCostEth = (gasLimit * gasPriceGwei) / 1e9;

                // Convert to USD (assume ETH price from pool data)
                const ethPrice = state.selectedPool?.token2Price || 2000; // fallback
                return gasCostEth * ethPrice;
            }
        } catch (error) {
            console.error('❌ Error estimating gas fees:', error);
        }

        return 25; // fallback estimate
    }

    function handleSlippageChange() {
        const slippageInput = document.getElementById('slippage-tolerance');
        if (!slippageInput) return;

        const slippage = parseFloat(slippageInput.value);

        if (slippage > config.validation.maxSlippage) {
            showError(`Slippage tolerance too high. Maximum is ${config.validation.maxSlippage}%.`);
            slippageInput.value = config.validation.maxSlippage;
        }
    }

    async function proceedToDex() {
        if (!validateFinalStep()) return;

        showLoading('Preparing transaction...');

        try {
            const response = await fetch(config.endpoints.dexUrl, {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json'
                },
                body: JSON.stringify({
                    poolId: state.selectedPool.id,
                    dexId: state.selectedDex.id,
                    token1Amount: state.tokenAmounts.token1,
                    token2Amount: state.tokenAmounts.token2,
                    slippageTolerance: parseFloat(document.getElementById('slippage-tolerance')?.value || 0.5),
                    walletAddress: state.walletAddress
                })
            });

            if (!response.ok) throw new Error('Failed to generate DEX URL');

            const data = await response.json();

            // Open DEX in new tab
            window.open(data.url, '_blank');

            showSuccess(`Redirected to ${state.selectedDex.name} to complete your transaction.`);

            // Optional: redirect to management page after a delay
            setTimeout(() => {
                window.location.href = '/liquidity/manage';
            }, 3000);

        } catch (error) {
            console.error('❌ Error proceeding to DEX:', error);
            showError('Failed to redirect to DEX. Please try again.');
        } finally {
            hideLoading();
        }
    }

    // ================================================
    // WIZARD NAVIGATION
    // ================================================
    function nextStep() {
        if (!validateCurrentStep()) {
            return;
        }

        if (state.currentStep < config.steps.total) {
            state.currentStep++;
            showStep(state.currentStep);
        }
    }

    function previousStep() {
        if (state.currentStep > 1) {
            state.currentStep--;
            showStep(state.currentStep);
        }
    }

    function showStep(stepNumber) {
        // Hide all step contents
        const stepContents = document.querySelectorAll('.wizard-step-content');
        stepContents.forEach(content => {
            content.classList.remove('active');
        });

        // Show current step content
        const currentStepContent = document.querySelector(`[data-step="${stepNumber}"]`);
        if (currentStepContent) {
            currentStepContent.classList.add('active');
        }

        // Update progress indicator
        updateProgressIndicator();

        // Update navigation buttons
        updateNavigationButtons();

        // Update step-specific UI
        updateStepUI(stepNumber);

        // Update step counter
        const currentStepSpan = document.getElementById('current-step');
        if (currentStepSpan) {
            currentStepSpan.textContent = stepNumber;
        }

        console.log(`📍 Wizard step ${stepNumber} displayed`);
    }

    function updateProgressIndicator() {
        const progressFill = document.getElementById('progress-fill');
        const stepCircles = document.querySelectorAll('.step-circle');

        // Update progress bar
        if (progressFill) {
            const progressPercentage = ((state.currentStep - 1) / (config.steps.total - 1)) * 100;
            progressFill.style.width = `${progressPercentage}%`;
        }

        // Update step circles
        stepCircles.forEach((circle, index) => {
            const stepNumber = index + 1;
            circle.classList.remove('active', 'completed');

            if (stepNumber < state.currentStep) {
                circle.classList.add('completed');
                circle.textContent = '✓';
            } else if (stepNumber === state.currentStep) {
                circle.classList.add('active');
                circle.textContent = stepNumber;
            } else {
                circle.textContent = stepNumber;
            }
        });
    }

    function updateNavigationButtons() {
        const prevBtn = document.getElementById('prev-btn');
        const nextBtn = document.getElementById('next-btn');

        // Previous button
        if (prevBtn) {
            prevBtn.disabled = state.currentStep <= 1;
        }

        // Next button
        if (nextBtn) {
            if (state.currentStep === config.steps.total) {
                nextBtn.style.display = 'none';
            } else {
                nextBtn.style.display = 'block';
                nextBtn.disabled = !validateCurrentStep();

                if (state.currentStep === config.steps.total - 1) {
                    nextBtn.textContent = 'Review';
                } else {
                    nextBtn.textContent = 'Next';
                }
            }
        }
    }

    function updateStepUI(stepNumber) {
        switch (stepNumber) {
            case 3:
                // Load DEX options if pool is selected
                if (state.selectedPool) {
                    loadDexOptionsForPool(state.selectedPool.id);
                }
                break;

            case 4:
                // Update token display
                updateTokenDisplay();
                break;

            case 5:
                // Update review data
                updateReviewData();
                break;
        }
    }

    // ================================================
    // VALIDATION
    // ================================================
    function validateCurrentStep() {
        const validator = state.validators[state.currentStep];
        return validator ? validator() : true;
    }

    function validateWalletConnection() {
        return state.walletConnected && state.walletAddress;
    }

    function validatePoolSelection() {
        return state.selectedPool !== null;
    }

    function validateDexSelection() {
        return state.selectedDex !== null;
    }

    function validateTokenAmounts() {
        const hasAmounts = state.tokenAmounts.token1 > 0 && state.tokenAmounts.token2 > 0;
        const meetsMinimum = state.calculatedData?.totalDepositValue >= config.validation.minAmount;

        return hasAmounts && meetsMinimum;
    }

    function validateFinalStep() {
        const checkboxes = document.querySelectorAll('.risk-checkbox input[type="checkbox"]');
        const allChecked = Array.from(checkboxes).every(checkbox => checkbox.checked);

        const proceedBtn = document.getElementById('proceed-to-dex');
        if (proceedBtn) {
            proceedBtn.disabled = !allChecked;
        }

        return allChecked && validateTokenAmounts();
    }

    // ================================================
    // UTILITY FUNCTIONS
    // ================================================
    function checkUrlParameters() {
        const urlParams = new URLSearchParams(window.location.search);
        const poolId = urlParams.get('poolId');

        if (poolId) {
            // Pre-select pool if specified in URL
            const poolRadio = document.querySelector(`input[name="selected-pool"][value="${poolId}"]`);
            if (poolRadio) {
                poolRadio.checked = true;
                handlePoolSelection(poolId);
            }
        }
    }

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

    function showImpermanentLossGuide() {
        const modal = document.getElementById('il-guide-modal');
        if (modal) {
            modal.style.display = 'block';
        }
    }

    function openDexInfo() {
        if (state.selectedDex?.infoUrl) {
            window.open(state.selectedDex.infoUrl, '_blank');
        } else {
            showError('DEX information not available.');
        }
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

            setTimeout(() => {
                hideError();
            }, 5000);
        }

        console.error('🚨 Wizard Error:', message);
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
        console.log('✅ Wizard Success:', message);

        // You could implement a success toast similar to error toast
        // For now, just log to console
    }

    function closeModal(modalId) {
        const modal = document.getElementById(modalId);
        if (modal) {
            modal.style.display = 'none';
        }
    }

    // ================================================
    // GLOBAL FUNCTIONS (exposed to window)
    // ================================================
    window.nextStep = nextStep;
    window.previousStep = previousStep;
    window.showImpermanentLossGuide = showImpermanentLossGuide;
    window.openDexInfo = openDexInfo;
    window.proceedToDex = proceedToDex;
    window.closeModal = closeModal;
    window.hideError = hideError;

    // ================================================
    // PUBLIC API
    // ================================================
    return {
        init: init,
        nextStep: nextStep,
        previousStep: previousStep,
        validateCurrentStep: validateCurrentStep,

        // Expose state for debugging
        getState: () => ({ ...state }),
        getConfig: () => ({ ...config })
    };

})();

// ================================================
// ADDITIONAL MODAL STYLES AND FUNCTIONALITY
// ================================================

// Modal handling
document.addEventListener('click', function (e) {
    // Close modal when clicking outside
    if (e.target.classList.contains('modal')) {
        e.target.style.display = 'none';
    }
});

// Escape key to close modals
document.addEventListener('keydown', function (e) {
    if (e.key === 'Escape') {
        const openModals = document.querySelectorAll('.modal[style*="block"]');
        openModals.forEach(modal => {
            modal.style.display = 'none';
        });
    }
});

// Removal slider functionality
document.addEventListener('input', function (e) {
    if (e.target.id === 'removal-slider') {
        const percentage = parseInt(e.target.value);
        window.updateRemovalPreview(percentage);
    }
});

// Console startup message for wizard
console.log(`
🧙‍♂️ TeachToken Add Liquidity Wizard
📝 Version: 1.0.0
🚀 Initialized: ${new Date().toISOString()}
🔗 Steps: ${5} total
`);

// Export for module systems (if needed)
if (typeof module !== 'undefined' && module.exports) {
    module.exports = window.AddLiquidityWizard;
} 1Input) {
    token1Input.addEventListener('input', function () {
        handleTokenAmountChange(1, this.value);
    });
}

if (token2Input) {
    token2Input.addEventListener('input', function () {
        handleTokenAmountChange(2, this.value);
    });
}

// Amount percentage buttons
document.addEventListener('click', function (e) {
    if (e.target.classList.contains('amount-btn')) {
        const percentage = parseInt(e.target.dataset.percentage);
        const tokenNumber = e.target.closest('.token-input-card').querySelector('input').id.includes('token1') ? 1 : 2;
        handlePercentageClick(tokenNumber, percentage);
    }
});

// Slippage tolerance
const slippageTolerance = document.getElementById('slippage-tolerance');
if (slippageTolerance) {
    slippageTolerance.addEventListener('change', handleSlippageChange);
}

// Risk acknowledgment checkboxes
const riskCheckboxes = document.querySelectorAll('.risk-checkbox input[type="checkbox"]');
riskCheckboxes.forEach(checkbox => {
    checkbox.addEventListener('change', validateFinalStep);
});

// DEX option cards
document.addEventListener('click', function (e) {
    const dexCard = e.target.closest('.dex-option-card');
    if (dexCard) {
        const radio = dexCard.querySelector('input[type="radio"]');
        if (radio) {
            radio.checked = true;
            handleDexSelection(radio.value);
        }
    }
});
    }

function initializeWizardNavigation() {
    // Next/Previous button handlers are set up in global scope
    // See nextStep() and previousStep() functions

    updateProgressIndicator();
    updateNavigationButtons();
}

function initializeStepValidation() {
    // Set up validation rules for each step
    const validators = {
        1: validateWalletConnection,
        2: validatePoolSelection,
        3: validateDexSelection,
        4: validateTokenAmounts,
        5: validateFinalStep
    };

    state.validators = validators;
}

// ================================================
// WALLET CONNECTION (Step 1)
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
                throw new Error('WalletConnect integration coming soon.');

            case 'coinbase':
                throw new Error('Coinbase Wallet integration coming soon.');

            case 'trust':
                throw new Error('Trust Wallet integration coming soon.');

            default:
                throw new Error('Unsupported wallet type.');
        }

        // Request account access
        const accounts = await provider.request({ method: 'eth_requestAccounts' });
        const walletAddress = accounts[0];

        // Get balance
        const balanceWei = await provider.request({
            method: 'eth_getBalance',
            params: [walletAddress, 'latest']
        });
        const balanceEth = parseInt(balanceWei, 16) / Math.pow(10, 18);

        // Update state
        state.walletConnected = true;
        state.walletAddress = walletAddress;
        state.walletBalance.eth = balanceEth;

        // Update UI
        updateWalletStatus(walletAddress, balanceEth);

        // Enable next step
        updateNavigationButtons();

        showSuccess('Wallet connected successfully!');

        // Auto-advance to next step after 1 second
        setTimeout(() => {
            nextStep();
        }, 1000);

    } catch (error) {
        console.error('❌ Wallet connection error:', error);
        showError(error.message || 'Failed to connect wallet.');
    } finally {
        hideLoading();
    }
}

function updateWalletStatus(address, balance) {
    const walletStatus = document.getElementById('wallet-status');
    const walletAddressSpan = document.getElementById('wallet-address');
    const walletBalanceSpan = document.getElementById('wallet-balance');

    if (walletStatus && walletAddressSpan && walletBalanceSpan) {
        walletAddressSpan.textContent = `${address.substring(0, 6)}...${address.substring(38)}`;
        walletBalanceSpan.textContent = balance.toFixed(4);
        walletStatus.style.display = 'block';
    }

    // Hide wallet connection options
    const walletConnect = document.getElementById('wallet-connect');
    if (walletConnect) {
        walletConnect.querySelector('.wallet-options').style.display = 'none';
    }
}

// ================================================
// POOL SELECTION (Step 2)
// ================================================
function handlePoolSearch() {
    const searchInput = document.getElementById('pool-search-wizard');
    if (!searchInput) return;

    const searchTerm = searchInput.value.toLowerCase();
    const rows = document.querySelectorAll('.pool-comparison-row');

    rows.forEach(row => {
        const poolText = row.textContent.toLowerCase();
        const shouldShow = poolText.includes(searchTerm);
        row.style.display = shouldShow ? 'table-row' : 'none';
    });
}

function handlePoolSelection(poolId) {
    console.log('🏊 Pool selected:', poolId);

    if (!state.wizardData?.availablePools) return;

    // Find selected pool data
    const pool = state.wizardData.availablePools.find(p => p.id == poolId);
    if (!pool) return;

    state.selectedPool = pool;

    // Update pool details display
    updatePoolDetails(pool);

    // Enable next step
    updateNavigationButtons();

    // Load DEX options for this pool
    loadDexOptionsForPool(poolId);
}

function updatePoolDetails(pool) {
    const detailsContainer = document.getElementById('selected-pool-details');
    if (!detailsContainer) return;

    const poolName = document.getElementById('selected-pool-name');
    const poolApy = document.getElementById('selected-pool-apy');
    const poolFee = document.getElementById('selected-pool-fee');
    const poolRisk = document.getElementById('selected-pool-risk');

    if (poolName) poolName.textContent = `${pool.token1Symbol}/${pool.token2Symbol}`;
    if (poolApy) poolApy.textContent = `${pool.apy.toFixed(2)}%`;
    if (poolFee) poolFee.textContent = `${(pool.feeTier * 100)}%`;
    if (poolRisk) poolRisk.textContent = pool.riskLevel;

    detailsContainer.style.display = 'block';
}

async function loadDexOptionsForPool(poolId) {
    try {
        const response = await fetch(`${config.endpoints.wizardData}?poolId=${poolId}`);
        if (!response.ok) throw new Error('Failed to load DEX options');

        const data = await response.json();
        if (data.dexOptions) {
            state.wizardData.dexOptions = data.dexOptions;
            updateDexOptionsDisplay(data.dexOptions);
        }
    } catch (error) {
        console.error('❌ Error loading DEX options:', error);
        showError('Failed to load DEX options for selected pool.');
    }
}

function updateDexOptionsDisplay(dexOptions) {
    const dexGrid = document.getElementById('dex-options-grid');
    if (!dexGrid || !dexOptions) return;

    // Update existing DEX cards with pool-specific data
    dexOptions.forEach(dex => {
        const dexCard = dexGrid.querySelector(`[data-dex-id="${dex.id}"]`);
        if (dexCard) {
            const apyElement = dexCard.querySelector('.stat-value');
            if (apyElement) apyElement.textContent = `${dex.poolApy.toFixed(2)}%`;
        }
    });
}

// ================================================
// DEX SELECTION (Step 3)
// ================================================
function handleDexSelection(dexId) {
    console.log('🔄 DEX selected:', dexId);

    if (!state.wizardData?.dexOptions) return;

    // Find selected DEX data
    const dex = state.wizardData.dexOptions.find(d => d.id == dexId);
    if (!dex) return;

    state.selectedDex = dex;

    // Update DEX comparison display
    updateDexComparison(dex);

    // Update token icons and symbols for next step
    updateTokenDisplay();

    // Enable next step
    updateNavigationButtons();
}

function updateDexComparison(dex) {
    const comparisonContainer = document.getElementById('dex-comparison');
    if (!comparisonContainer) return;

    const securityElement = document.getElementById('selected-dex-security');
    const ratingElement = document.getElementById('selected-dex-rating');
    const speedElement = document.getElementById('selected-dex-speed');

    if (securityElement) securityElement.textContent = dex.securityRating || 'A+';
    if (ratingElement) ratingElement.textContent = `${dex.userRating || 4.8}/5`;
    if (speedElement) speedElement.textContent = dex.avgTransactionTime || '~2 min';

    comparisonContainer.style.display = 'block';
}

function updateTokenDisplay() {
    if (!state.selectedPool) return;

    const token1Icon = document.getElementById('token1-icon');
    const token1Symbol = document.getElementById('token1-symbol');
    const token2Icon = document.getElementById('token2-icon');
    const token2Symbol = document.getElementById('token2-symbol');

    if (token1Icon) token1Icon.src = state.selectedPool.token1Icon;
    if (token1Symbol) token1Symbol.textContent = state.selectedPool.token1Symbol;
    if (token2Icon) token2Icon.src = state.selectedPool.token2Icon;
    if (token2Symbol) token2Symbol.textContent = state.selectedPool.token2Symbol;
}

// ================================================
// TOKEN AMOUNTS (Step 4)
// ================================================
async function handleTokenAmountChange(tokenNumber, value) {
    const numValue = parseFloat(value) || 0;

    // Update state
    if (tokenNumber === 1) {
        state.tokenAmounts.token1 = numValue;
    } else {
        state.tokenAmounts.token2 = numValue;
    }

    // Calculate corresponding amount for the other token
    if (numValue > 0) {
        await calculateCorrespondingAmount(tokenNumber, numValue);
    } else {
        // Clear the other input if this one is empty
        const otherInput = document.getElementById(tokenNumber === 1 ? 'token2-amount' : 'token1-amount');
        if (otherInput) otherInput.value = '';
    }

    // Update USD values
    updateUSDValues();

    // Update pool position preview
    updatePoolPositionPreview();

    // Update impermanent loss calculation
    updateImpermanentLossCalculation();

    // Validate amounts
    validateTokenAmounts();
}

async function calculateCorrespondingAmount(inputTokenNumber, inputAmount) {
    if (!state.selectedPool || !state.selectedDex) return;

    try {
        const response = await fetch(config.endpoints.calculate, {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json'
            },
            body: JSON.stringify({
                poolId: state.selectedPool.id,
                dexId: state.selectedDex.id,
                inputToken: inputTokenNumber,
                inputAmount: inputAmount
            })
        });

        if (!response.ok) throw new Error('Calculation failed');

        const data = await response.json();

        // Update the corresponding input
        const otherInput = document.getElementById(inputTokenNumber === 1 ? 'token2-amount' : 'token1-amount');
        if (otherInput) {
            otherInput.value = data.correspondingAmount.toFixed(6);

            // Update state
            if (inputTokenNumber === 1) {
                state.tokenAmounts.token2 = data.correspondingAmount;
            } else {
                state.tokenAmounts.token1 = data.correspondingAmount;
            }
        }

        // Store calculation data
        state.calculatedData = data;

    } catch (error) {
        console.error('❌ Error calculating corresponding amount:', error);
        showError('Failed to calculate token amounts. Please try again.');
    }
}

function handlePercentageClick(tokenNumber, percentage) {
    if (!state.walletBalance) return;

    let maxAmount = 0;

    if (tokenNumber === 1) {
        maxAmount = state.walletBalance.token1 || 0;
    } else if (tokenNumber === 2) {
        maxAmount = state.walletBalance.token2 || state.walletBalance.eth || 0;
    }

    const amount = (maxAmount * percentage) / 100;
    const input = document.getElementById(`token${tokenNumber}-amount`);

    if (input) {
        input.value = amount.toFixed(6);
        handleTokenAmountChange(tokenNumber, amount);
    }
}

function updateUSDValues() {
    const token1UsdElement = document.getElementById('token1-usd');
    const token2UsdElement = document.getElementById('token2-usd');

    if (token1UsdElement && state.selectedPool) {
        const usdValue = state.tokenAmounts.token1 * (state.selectedPool.token1Price || 0);
        token1UsdElement.textContent = `$${usdValue.toFixed(2)}`;
    }

    if (token