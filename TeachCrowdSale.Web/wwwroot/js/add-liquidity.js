/**
 * Add Liquidity Wizard JavaScript
 * Handles step-by-step liquidity addition process (Add.cshtml)
 * Integrates with LiquidityController wizard endpoints
 */

class AddLiquidityWizard {
    constructor() {
        this.currentStep = 1;
        this.totalSteps = 5;
        this.wizardData = {};
        this.calculator = null;
        this.isLoading = false;
        this.walletAddress = null;
        this.selectedPool = null;
        this.selectedDex = null;
        this.validationTimer = null;

        this.initializeOnLoad();
    }

    /**
     * Initialize wizard when DOM is loaded
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
            console.log('Initializing Add Liquidity Wizard...');

            // Get initial data from ViewBag
            this.wizardData = this.getInitialData();
            this.currentStep = parseInt(document.querySelector('[data-current-step]')?.dataset.currentStep) || 1;

            // Initialize step-specific components
            this.initializeCurrentStep();
            this.initializeNavigation();
            this.initializeWalletConnection();
            this.initializeProgressBar();

            console.log('Add Liquidity Wizard initialized successfully');
        } catch (error) {
            console.error('Failed to initialize wizard:', error);
            this.showErrorMessage('Failed to initialize wizard');
        }
    }

    /**
     * Get initial data from server-side ViewBag
     */
    getInitialData() {
        try {
            const jsonDataElement = document.querySelector('script[data-json="wizard-data"]');
            if (jsonDataElement) {
                return JSON.parse(jsonDataElement.textContent);
            }

            if (typeof window.addLiquidityInitialData !== 'undefined') {
                return window.addLiquidityInitialData;
            }

            console.warn('No wizard data found, using empty object');
            return {};
        } catch (error) {
            console.warn('Could not parse wizard data:', error);
            return {};
        }
    }

    /**
     * Initialize components for current step
     */
    initializeCurrentStep() {
        switch (this.currentStep) {
            case 1:
                this.initializeStep1(); // Connect Wallet & Select Pool
                break;
            case 2:
                this.initializeStep2(); // Choose DEX
                break;
            case 3:
                this.initializeStep3(); // Enter Amounts
                break;
            case 4:
                this.initializeStep4(); // Review & Confirm
                break;
            case 5:
                this.initializeStep5(); // Transaction Status
                break;
        }
    }

    /**
     * Initialize Step 1: Connect Wallet & Select Pool
     */
    initializeStep1() {
        // Wallet connection
        this.initializeWalletConnection();

        // Pool selection
        this.initializePoolSelection();

        // Pool comparison
        this.initializePoolComparison();
    }

    /**
     * Initialize Step 2: Choose DEX
     */
    initializeStep2() {
        this.initializeDexSelection();
    }

    /**
     * Initialize Step 3: Enter Amounts
     */
    initializeStep3() {
        this.initializeAmountInputs();
        this.initializeCalculator();
        this.initializeSlippageSettings();
    }

    /**
     * Initialize Step 4: Review & Confirm
     */
    initializeStep4() {
        this.initializeTransactionReview();
        this.initializeRiskWarnings();
    }

    /**
     * Initialize Step 5: Transaction Status
     */
    initializeStep5() {
        this.initializeTransactionMonitoring();
    }

    /**
     * Initialize navigation controls
     */
    initializeNavigation() {
        // Next button
        const nextBtn = document.getElementById('next-step-btn');
        if (nextBtn) {
            nextBtn.addEventListener('click', () => this.goToNextStep());
        }

        // Previous button
        const prevBtn = document.getElementById('prev-step-btn');
        if (prevBtn) {
            prevBtn.addEventListener('click', () => this.goToPreviousStep());
        }

        // Step navigation
        document.querySelectorAll('.step-nav-btn').forEach(btn => {
            btn.addEventListener('click', (e) => {
                const step = parseInt(e.target.dataset.step);
                if (this.canNavigateToStep(step)) {
                    this.goToStep(step);
                }
            });
        });
    }

    /**
     * Initialize progress bar
     */
    initializeProgressBar() {
        const progressBar = document.getElementById('wizard-progress');
        if (progressBar) {
            const percentage = (this.currentStep / this.totalSteps) * 100;
            progressBar.style.width = `${percentage}%`;
            progressBar.setAttribute('aria-valuenow', percentage);
        }

        // Update step indicators
        document.querySelectorAll('.step-indicator').forEach((indicator, index) => {
            const stepNumber = index + 1;
            if (stepNumber < this.currentStep) {
                indicator.classList.add('completed');
                indicator.classList.remove('active');
            } else if (stepNumber === this.currentStep) {
                indicator.classList.add('active');
                indicator.classList.remove('completed');
            } else {
                indicator.classList.remove('active', 'completed');
            }
        });
    }

    /**
     * Initialize wallet connection
     */
    initializeWalletConnection() {
        const connectBtn = document.getElementById('connect-wallet-btn');
        if (connectBtn) {
            connectBtn.addEventListener('click', () => this.connectWallet());
        }

        const disconnectBtn = document.getElementById('disconnect-wallet-btn');
        if (disconnectBtn) {
            disconnectBtn.addEventListener('click', () => this.disconnectWallet());
        }

        // Check if wallet is already connected
        this.checkWalletConnection();
    }

    /**
     * Initialize pool selection
     */
    initializePoolSelection() {
        // Pool cards
        document.querySelectorAll('.pool-selection-card').forEach(card => {
            card.addEventListener('click', (e) => {
                const poolId = parseInt(e.currentTarget.dataset.poolId);
                this.selectPool(poolId);
            });
        });

        // Pool filter
        const poolFilter = document.getElementById('pool-filter');
        if (poolFilter) {
            poolFilter.addEventListener('input', (e) => this.filterPools(e.target.value));
        }

        // Sort pools
        const sortSelect = document.getElementById('pool-sort');
        if (sortSelect) {
            sortSelect.addEventListener('change', (e) => this.sortPools(e.target.value));
        }
    }

    /**
     * Initialize pool comparison
     */
    initializePoolComparison() {
        const compareBtn = document.getElementById('compare-pools-btn');
        if (compareBtn) {
            compareBtn.addEventListener('click', () => this.showPoolComparison());
        }

        document.querySelectorAll('.pool-compare-checkbox').forEach(checkbox => {
            checkbox.addEventListener('change', () => this.updateCompareButton());
        });
    }

    /**
     * Initialize DEX selection
     */
    initializeDexSelection() {
        document.querySelectorAll('.dex-selection-card').forEach(card => {
            card.addEventListener('click', (e) => {
                const dexId = parseInt(e.currentTarget.dataset.dexId);
                this.selectDex(dexId);
            });
        });

        // DEX comparison
        const compareBtn = document.getElementById('compare-dex-btn');
        if (compareBtn) {
            compareBtn.addEventListener('click', () => this.showDexComparison());
        }
    }

    /**
     * Initialize amount inputs
     */
    initializeAmountInputs() {
        const token0Input = document.getElementById('token0-amount');
        const token1Input = document.getElementById('token1-amount');

        if (token0Input) {
            token0Input.addEventListener('input', (e) => this.onToken0AmountChange(e.target.value));
        }

        if (token1Input) {
            token1Input.addEventListener('input', (e) => this.onToken1AmountChange(e.target.value));
        }

        // Balance buttons
        document.querySelectorAll('.use-max-btn').forEach(btn => {
            btn.addEventListener('click', (e) => {
                const tokenType = e.target.dataset.token;
                this.useMaxBalance(tokenType);
            });
        });

        // Percentage buttons
        document.querySelectorAll('.percentage-btn').forEach(btn => {
            btn.addEventListener('click', (e) => {
                const percentage = parseFloat(e.target.dataset.percentage);
                this.usePercentageOfBalance(percentage);
            });
        });
    }

    /**
     * Initialize calculator
     */
    initializeCalculator() {
        // Auto-calculate toggle
        const autoCalcToggle = document.getElementById('auto-calculate');
        if (autoCalcToggle) {
            autoCalcToggle.addEventListener('change', (e) => {
                this.autoCalculate = e.target.checked;
            });
        }

        // Manual calculate button
        const calcBtn = document.getElementById('calculate-btn');
        if (calcBtn) {
            calcBtn.addEventListener('click', () => this.calculateLiquidity());
        }

        // Refresh button
        const refreshBtn = document.getElementById('refresh-calculation');
        if (refreshBtn) {
            refreshBtn.addEventListener('click', () => this.refreshCalculation());
        }
    }

    /**
     * Initialize slippage settings
     */
    initializeSlippageSettings() {
        const slippageInput = document.getElementById('slippage-tolerance');
        if (slippageInput) {
            slippageInput.addEventListener('input', (e) => this.updateSlippage(e.target.value));
        }

        // Preset slippage buttons
        document.querySelectorAll('.slippage-preset').forEach(btn => {
            btn.addEventListener('click', (e) => {
                const slippage = parseFloat(e.target.dataset.slippage);
                this.setSlippage(slippage);
            });
        });

        // Advanced settings toggle
        const advancedToggle = document.getElementById('show-advanced');
        if (advancedToggle) {
            advancedToggle.addEventListener('change', (e) => {
                const advancedPanel = document.getElementById('advanced-settings');
                if (advancedPanel) {
                    advancedPanel.style.display = e.target.checked ? 'block' : 'none';
                }
            });
        }
    }

    /**
     * Initialize transaction review
     */
    initializeTransactionReview() {
        // Recalculate button
        const recalcBtn = document.getElementById('recalculate-btn');
        if (recalcBtn) {
            recalcBtn.addEventListener('click', () => this.recalculateTransaction());
        }

        // Submit transaction button
        const submitBtn = document.getElementById('submit-transaction-btn');
        if (submitBtn) {
            submitBtn.addEventListener('click', () => this.submitTransaction());
        }

        // Risk acknowledgment
        const riskCheckbox = document.getElementById('acknowledge-risks');
        if (riskCheckbox) {
            riskCheckbox.addEventListener('change', (e) => {
                const submitBtn = document.getElementById('submit-transaction-btn');
                if (submitBtn) {
                    submitBtn.disabled = !e.target.checked;
                }
            });
        }
    }

    /**
     * Initialize risk warnings
     */
    initializeRiskWarnings() {
        // IL calculator
        const ilBtn = document.getElementById('calculate-il-btn');
        if (ilBtn) {
            ilBtn.addEventListener('click', () => this.showImpermanentLossCalculator());
        }

        // Risk toggle
        const riskToggle = document.getElementById('show-risks');
        if (riskToggle) {
            riskToggle.addEventListener('change', (e) => {
                const riskPanel = document.getElementById('risk-warnings');
                if (riskPanel) {
                    riskPanel.style.display = e.target.checked ? 'block' : 'none';
                }
            });
        }
    }

    /**
     * Initialize transaction monitoring
     */
    initializeTransactionMonitoring() {
        // Check transaction status periodically
        this.startTransactionMonitoring();

        // View on explorer button
        const explorerBtn = document.getElementById('view-on-explorer');
        if (explorerBtn) {
            explorerBtn.addEventListener('click', () => this.viewOnExplorer());
        }

        // Add another position button
        const addAnotherBtn = document.getElementById('add-another-position');
        if (addAnotherBtn) {
            addAnotherBtn.addEventListener('click', () => this.addAnotherPosition());
        }

        // View positions button
        const viewPositionsBtn = document.getElementById('view-positions');
        if (viewPositionsBtn) {
            viewPositionsBtn.addEventListener('click', () => this.viewPositions());
        }
    }

    /**
     * Connect wallet
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
                this.walletAddress = accounts[0];
                this.updateWalletUI(this.walletAddress);
                await this.loadWalletBalances();
                this.showSuccessMessage('Wallet connected successfully');
                this.validateCurrentStep();
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
        this.walletAddress = null;
        this.updateWalletUI(null);
        this.clearWalletData();
        this.showSuccessMessage('Wallet disconnected');
        this.validateCurrentStep();
    }

    /**
     * Check wallet connection
     */
    async checkWalletConnection() {
        try {
            if (typeof window.ethereum !== 'undefined') {
                const accounts = await window.ethereum.request({ method: 'eth_accounts' });
                if (accounts.length > 0) {
                    this.walletAddress = accounts[0];
                    this.updateWalletUI(this.walletAddress);
                    await this.loadWalletBalances();
                }
            }
        } catch (error) {
            console.error('Error checking wallet connection:', error);
        }
    }

    /**
     * Update wallet UI
     */
    updateWalletUI(walletAddress) {
        const connectBtn = document.getElementById('connect-wallet-btn');
        const disconnectBtn = document.getElementById('disconnect-wallet-btn');
        const walletInfo = document.getElementById('wallet-info');
        const walletDisplay = document.getElementById('wallet-display');

        if (walletAddress) {
            if (connectBtn) connectBtn.style.display = 'none';
            if (disconnectBtn) disconnectBtn.style.display = 'block';
            if (walletInfo) walletInfo.style.display = 'block';
            if (walletDisplay) {
                walletDisplay.textContent = `${walletAddress.slice(0, 6)}...${walletAddress.slice(-4)}`;
            }
        } else {
            if (connectBtn) connectBtn.style.display = 'block';
            if (disconnectBtn) disconnectBtn.style.display = 'none';
            if (walletInfo) walletInfo.style.display = 'none';
        }
    }

    /**
     * Load wallet balances
     */
    async loadWalletBalances() {
        if (!this.walletAddress) return;

        try {
            // This would typically call the API to get wallet balances
            // For now, we'll simulate the data
            this.walletBalances = {
                eth: 1.5,
                token0: 1000,
                token1: 500
            };

            this.updateBalanceDisplay();
        } catch (error) {
            console.error('Error loading wallet balances:', error);
        }
    }

    /**
     * Update balance display
     */
    updateBalanceDisplay() {
        if (!this.walletBalances) return;

        const ethBalance = document.getElementById('eth-balance');
        const token0Balance = document.getElementById('token0-balance');
        const token1Balance = document.getElementById('token1-balance');

        if (ethBalance) ethBalance.textContent = `${this.walletBalances.eth.toFixed(4)} ETH`;
        if (token0Balance) token0Balance.textContent = `${this.walletBalances.token0.toLocaleString()} TEACH`;
        if (token1Balance) token1Balance.textContent = `${this.walletBalances.token1.toLocaleString()} USDC`;
    }

    /**
     * Clear wallet data
     */
    clearWalletData() {
        this.walletBalances = null;
        this.updateBalanceDisplay();
    }

    /**
     * Select pool
     */
    selectPool(poolId) {
        // Update UI
        document.querySelectorAll('.pool-selection-card').forEach(card => {
            card.classList.remove('selected');
        });

        const selectedCard = document.querySelector(`[data-pool-id="${poolId}"]`);
        if (selectedCard) {
            selectedCard.classList.add('selected');
        }

        // Find pool data
        const pools = this.wizardData.availablePools || [];
        this.selectedPool = pools.find(p => p.id === poolId);

        if (this.selectedPool) {
            this.updatePoolDisplay();
            this.validateCurrentStep();
        }
    }

    /**
     * Update pool display
     */
    updatePoolDisplay() {
        if (!this.selectedPool) return;

        const poolName = document.getElementById('selected-pool-name');
        const poolPair = document.getElementById('selected-pool-pair');
        const poolApy = document.getElementById('selected-pool-apy');
        const poolTvl = document.getElementById('selected-pool-tvl');

        if (poolName) poolName.textContent = this.selectedPool.name;
        if (poolPair) poolPair.textContent = this.selectedPool.tokenPair;
        if (poolApy) poolApy.textContent = this.selectedPool.apyDisplay;
        if (poolTvl) poolTvl.textContent = this.selectedPool.totalValueLockedDisplay;

        // Update token symbols
        const token0Symbol = document.getElementById('token0-symbol');
        const token1Symbol = document.getElementById('token1-symbol');

        if (token0Symbol) token0Symbol.textContent = this.selectedPool.token0Symbol;
        if (token1Symbol) token1Symbol.textContent = this.selectedPool.token1Symbol;
    }

    /**
     * Select DEX
     */
    selectDex(dexId) {
        // Update UI
        document.querySelectorAll('.dex-selection-card').forEach(card => {
            card.classList.remove('selected');
        });

        const selectedCard = document.querySelector(`[data-dex-id="${dexId}"]`);
        if (selectedCard) {
            selectedCard.classList.add('selected');
        }

        // Find DEX data
        const dexes = this.wizardData.availableDexes || [];
        this.selectedDex = dexes.find(d => d.id === dexId);

        if (this.selectedDex) {
            this.updateDexDisplay();
            this.validateCurrentStep();
        }
    }

    /**
     * Update DEX display
     */
    updateDexDisplay() {
        if (!this.selectedDex) return;

        const dexName = document.getElementById('selected-dex-name');
        const dexNetwork = document.getElementById('selected-dex-network');
        const dexFee = document.getElementById('selected-dex-fee');

        if (dexName) dexName.textContent = this.selectedDex.displayName;
        if (dexNetwork) dexNetwork.textContent = this.selectedDex.network;
        if (dexFee) dexFee.textContent = this.selectedDex.feeDisplay;
    }

    /**
     * Handle token 0 amount change
     */
    onToken0AmountChange(value) {
        const amount = parseFloat(value) || 0;

        // Clear validation timer
        if (this.validationTimer) {
            clearTimeout(this.validationTimer);
        }

        // Validate and calculate after delay
        this.validationTimer = setTimeout(() => {
            this.validateTokenAmount(0, amount);
            if (this.autoCalculate && amount > 0) {
                this.calculateOptimalToken1Amount(amount);
            }
        }, 500);
    }

    /**
     * Handle token 1 amount change
     */
    onToken1AmountChange(value) {
        const amount = parseFloat(value) || 0;

        // Clear validation timer
        if (this.validationTimer) {
            clearTimeout(this.validationTimer);
        }

        // Validate and calculate after delay
        this.validationTimer = setTimeout(() => {
            this.validateTokenAmount(1, amount);
            if (this.autoCalculate && amount > 0) {
                this.calculateOptimalToken0Amount(amount);
            }
        }, 500);
    }

    /**
     * Calculate optimal token 1 amount based on token 0
     */
    calculateOptimalToken1Amount(token0Amount) {
        if (!this.selectedPool) return;

        // Simple ratio calculation (would use real pool data in production)
        const ratio = 2; // Assuming 1 TEACH = 2 USDC
        const token1Amount = token0Amount * ratio;

        const token1Input = document.getElementById('token1-amount');
        if (token1Input) {
            token1Input.value = token1Amount.toFixed(6);
        }

        this.triggerCalculation();
    }

    /**
     * Calculate optimal token 0 amount based on token 1
     */
    calculateOptimalToken0Amount(token1Amount) {
        if (!this.selectedPool) return;

        // Simple ratio calculation (would use real pool data in production)
        const ratio = 0.5; // Assuming 1 USDC = 0.5 TEACH
        const token0Amount = token1Amount * ratio;

        const token0Input = document.getElementById('token0-amount');
        if (token0Input) {
            token0Input.value = token0Amount.toFixed(6);
        }

        this.triggerCalculation();
    }

    /**
     * Use maximum balance for token
     */
    useMaxBalance(tokenType) {
        if (!this.walletBalances) return;

        const balance = tokenType === 'token0' ? this.walletBalances.token0 : this.walletBalances.token1;
        const input = document.getElementById(`${tokenType}-amount`);

        if (input) {
            input.value = balance.toString();
            if (tokenType === 'token0') {
                this.onToken0AmountChange(balance.toString());
            } else {
                this.onToken1AmountChange(balance.toString());
            }
        }
    }

    /**
     * Use percentage of balance
     */
    usePercentageOfBalance(percentage) {
        if (!this.walletBalances) return;

        const token0Amount = (this.walletBalances.token0 * percentage / 100);
        const token1Amount = (this.walletBalances.token1 * percentage / 100);

        const token0Input = document.getElementById('token0-amount');
        const token1Input = document.getElementById('token1-amount');

        if (token0Input) {
            token0Input.value = token0Amount.toFixed(6);
            this.onToken0AmountChange(token0Amount.toString());
        }

        if (token1Input) {
            token1Input.value = token1Amount.toFixed(6);
            this.onToken1AmountChange(token1Amount.toString());
        }
    }

    /**
     * Validate token amount
     */
    validateTokenAmount(tokenIndex, amount) {
        const tokenType = tokenIndex === 0 ? 'token0' : 'token1';
        const balance = this.walletBalances ?
            (tokenIndex === 0 ? this.walletBalances.token0 : this.walletBalances.token1) : 0;

        const inputElement = document.getElementById(`${tokenType}-amount`);
        const errorElement = document.getElementById(`${tokenType}-error`);

        let isValid = true;
        let errorMessage = '';

        if (amount <= 0) {
            isValid = false;
            errorMessage = 'Amount must be greater than 0';
        } else if (amount > balance) {
            isValid = false;
            errorMessage = 'Insufficient balance';
        }

        // Update UI
        if (inputElement) {
            inputElement.classList.toggle('is-invalid', !isValid);
            inputElement.classList.toggle('is-valid', isValid);
        }

        if (errorElement) {
            errorElement.textContent = errorMessage;
            errorElement.style.display = errorMessage ? 'block' : 'none';
        }

        return isValid;
    }

    /**
     * Trigger calculation
     */
    triggerCalculation() {
        if (this.calculationTimer) {
            clearTimeout(this.calculationTimer);
        }

        this.calculationTimer = setTimeout(() => {
            this.calculateLiquidity();
        }, 1000);
    }

    /**
     * Calculate liquidity
     */
    async calculateLiquidity() {
        if (!this.walletAddress || !this.selectedPool) return;

        const token0Amount = parseFloat(document.getElementById('token0-amount')?.value) || 0;
        const token1Amount = parseFloat(document.getElementById('token1-amount')?.value) || 0;
        const slippage = parseFloat(document.getElementById('slippage-tolerance')?.value) || 0.5;

        if (token0Amount <= 0 || token1Amount <= 0) return;

        try {
            this.showCalculationLoading();

            const response = await fetch('/liquidity/calculate', {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify({
                    walletAddress: this.walletAddress,
                    poolId: this.selectedPool.id,
                    token0Amount: token0Amount,
                    token1Amount: token1Amount,
                    slippageTolerance: slippage,
                    autoCalculateToken1: false
                })
            });

            if (!response.ok) throw new Error('Calculation failed');

            this.calculator = await response.json();
            this.displayCalculationResults();
            this.hideCalculationLoading();
            this.validateCurrentStep();
        } catch (error) {
            console.error('Error calculating liquidity:', error);
            this.showErrorMessage('Failed to calculate liquidity');
            this.hideCalculationLoading();
        }
    }

    /**
     * Display calculation results
     */
    displayCalculationResults() {
        if (!this.calculator) return;

        // LP tokens
        const lpTokens = document.getElementById('estimated-lp-tokens');
        if (lpTokens) lpTokens.textContent = this.calculator.estimatedLpTokensDisplay;

        // Total value
        const totalValue = document.getElementById('estimated-total-value');
        if (totalValue) totalValue.textContent = this.calculator.estimatedValueDisplay;

        // Price impact
        const priceImpact = document.getElementById('price-impact');
        if (priceImpact) {
            priceImpact.textContent = this.calculator.priceImpactDisplay;
            priceImpact.className = `price-impact ${this.calculator.priceImpactClass}`;
        }

        // APY
        const apy = document.getElementById('estimated-apy');
        if (apy) apy.textContent = this.calculator.apyDisplay;

        // Daily earnings
        const dailyEarnings = document.getElementById('daily-earnings');
        if (dailyEarnings) dailyEarnings.textContent = this.calculator.dailyEarningsDisplay;

        // Monthly earnings
        const monthlyEarnings = document.getElementById('monthly-earnings');
        if (monthlyEarnings) monthlyEarnings.textContent = this.calculator.monthlyEarningsDisplay;

        // Gas estimate
        const gasEstimate = document.getElementById('gas-estimate');
        if (gasEstimate) gasEstimate.textContent = this.calculator.gasEstimateDisplay;

        // Warnings
        this.displayCalculationWarnings();
    }

    /**
     * Display calculation warnings
     */
    displayCalculationWarnings() {
        if (!this.calculator) return;

        const warningsContainer = document.getElementById('calculation-warnings');
        if (!warningsContainer) return;

        let warningsHtml = '';

        if (this.calculator.warningMessages && this.calculator.warningMessages.length > 0) {
            this.calculator.warningMessages.forEach(warning => {
                warningsHtml += `
                    <div class="alert alert-warning alert-sm">
                        <i class="fas fa-exclamation-triangle me-2"></i>${warning}
                    </div>
                `;
            });
        }

        if (this.calculator.validationMessages && this.calculator.validationMessages.length > 0) {
            this.calculator.validationMessages.forEach(error => {
                warningsHtml += `
                    <div class="alert alert-danger alert-sm">
                        <i class="fas fa-times-circle me-2"></i>${error}
                    </div>
                `;
            });
        }

        warningsContainer.innerHTML = warningsHtml;
    }

    /**
     * Show calculation loading state
     */
    showCalculationLoading() {
        const loadingElement = document.getElementById('calculation-loading');
        const resultsElement = document.getElementById('calculation-results');

        if (loadingElement) loadingElement.style.display = 'block';
        if (resultsElement) resultsElement.style.display = 'none';
    }

    /**
     * Hide calculation loading state
     */
    hideCalculationLoading() {
        const loadingElement = document.getElementById('calculation-loading');
        const resultsElement = document.getElementById('calculation-results');

        if (loadingElement) loadingElement.style.display = 'none';
        if (resultsElement) resultsElement.style.display = 'block';
    }

    /**
     * Update slippage
     */
    updateSlippage(value) {
        const slippage = parseFloat(value) || 0.5;

        // Update preset buttons
        document.querySelectorAll('.slippage-preset').forEach(btn => {
            btn.classList.remove('active');
            if (parseFloat(btn.dataset.slippage) === slippage) {
                btn.classList.add('active');
            }