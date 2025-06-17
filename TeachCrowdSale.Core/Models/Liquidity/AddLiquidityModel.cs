using System.ComponentModel.DataAnnotations;
using TeachCrowdSale.Core.Attributes;

namespace TeachCrowdSale.Core.Models.Liquidity
{
    /// <summary>
    /// Web model for Add Liquidity wizard functionality
    /// Handles step-by-step liquidity addition process
    /// </summary>
    public class AddLiquidityModel
    {
        // Step tracking
        [Display(Name = "Current Step")]
        [Range(1, 5)]
        public int CurrentStep { get; set; } = 1;

        [Display(Name = "Total Steps")]
        public int TotalSteps { get; set; } = 5;

        // User wallet information
        [EthereumAddress]
        [Display(Name = "Wallet Address")]
        public string? WalletAddress { get; set; }

        public bool IsWalletConnected => !string.IsNullOrEmpty(WalletAddress);

        // Step 1: Pool Selection
        [Display(Name = "Selected Pool")]
        public int? SelectedPoolId { get; set; }

        [Display(Name = "Pool Information")]
        public LiquidityPoolModel? SelectedPool { get; set; }

        [Display(Name = "Available Pools")]
        public List<LiquidityPoolModel> AvailablePools { get; set; } = new();

        [Display(Name = "Recommended Pools")]
        public List<LiquidityPoolModel> RecommendedPools { get; set; } = new();

        // Step 2: DEX Selection
        [Display(Name = "Selected DEX")]
        public int? SelectedDexId { get; set; }

        [Display(Name = "DEX Information")]
        public DexConfigurationModel? SelectedDex { get; set; }

        [Display(Name = "Available DEXes")]
        public List<DexConfigurationModel> AvailableDexes { get; set; } = new();

        // Step 3: Amount Input
        [Display(Name = "Token 0 Amount")]
        [Range(0, double.MaxValue, ErrorMessage = "Amount must be positive")]
        public decimal Token0Amount { get; set; }

        [Display(Name = "Token 1 Amount")]
        [Range(0, double.MaxValue, ErrorMessage = "Amount must be positive")]
        public decimal Token1Amount { get; set; }

        [Display(Name = "Slippage Tolerance")]
        [Range(0.1, 50, ErrorMessage = "Slippage must be between 0.1% and 50%")]
        public decimal SlippageTolerance { get; set; } = 0.5m;

        [Display(Name = "Transaction Deadline")]
        [Range(1, 60, ErrorMessage = "Deadline must be between 1 and 60 minutes")]
        public int TransactionDeadlineMinutes { get; set; } = 20;

        // User balance information
        public decimal Token0Balance { get; set; }
        public decimal Token1Balance { get; set; }
        public decimal EthBalance { get; set; }

        // Calculated values and preview
        public LiquidityCalculatorModel? Calculator { get; set; }

        // Step 4: Review and Confirmation
        [Display(Name = "Expected LP Tokens")]
        public decimal ExpectedLpTokens { get; set; }

        [Display(Name = "Expected Share of Pool")]
        public decimal ExpectedPoolShare { get; set; }

        [Display(Name = "Estimated Gas Cost")]
        public decimal EstimatedGasCost { get; set; }

        [Display(Name = "Total Transaction Cost")]
        public decimal TotalTransactionCost { get; set; }

        [Display(Name = "Price Impact")]
        public decimal PriceImpact { get; set; }

        // Step 5: Transaction Status
        [Display(Name = "Transaction Hash")]
        public string? TransactionHash { get; set; }

        [Display(Name = "Transaction Status")]
        public string TransactionStatus { get; set; } = "Pending";

        [Display(Name = "Block Confirmations")]
        public int BlockConfirmations { get; set; }

        [Display(Name = "Required Confirmations")]
        public int RequiredConfirmations { get; set; } = 12;

        // Validation and error handling
        public List<string> ValidationErrors { get; set; } = new();
        public List<string> Warnings { get; set; } = new();
        public bool HasErrors => ValidationErrors.Any();
        public bool HasWarnings => Warnings.Any();

        // Display properties
        [Display(Name = "Token 0 Amount Display")]
        public string Token0AmountDisplay => $"{Token0Amount:F4} {SelectedPool?.Token0Symbol ?? ""}";

        [Display(Name = "Token 1 Amount Display")]
        public string Token1AmountDisplay => $"{Token1Amount:F4} {SelectedPool?.Token1Symbol ?? ""}";

        [Display(Name = "Token 0 Balance Display")]
        public string Token0BalanceDisplay => $"{Token0Balance:F4} {SelectedPool?.Token0Symbol ?? ""}";

        [Display(Name = "Token 1 Balance Display")]
        public string Token1BalanceDisplay => $"{Token1Balance:F4} {SelectedPool?.Token1Symbol ?? ""}";

        [Display(Name = "ETH Balance Display")]
        public string EthBalanceDisplay => $"{EthBalance:F4} ETH";

        [Display(Name = "Slippage Display")]
        public string SlippageToleranceDisplay => $"{SlippageTolerance:F1}%";

        [Display(Name = "Deadline Display")]
        public string TransactionDeadlineDisplay => $"{TransactionDeadlineMinutes} minutes";

        [Display(Name = "Expected LP Display")]
        public string ExpectedLpTokensDisplay => $"{ExpectedLpTokens:F6} LP";

        [Display(Name = "Pool Share Display")]
        public string ExpectedPoolShareDisplay => $"{ExpectedPoolShare:F4}%";

        [Display(Name = "Gas Cost Display")]
        public string EstimatedGasCostDisplay => $"${EstimatedGasCost:F2}";

        [Display(Name = "Total Cost Display")]
        public string TotalTransactionCostDisplay => $"${TotalTransactionCost:F2}";

        [Display(Name = "Price Impact Display")]
        public string PriceImpactDisplay => $"{PriceImpact:F3}%";

        // Progress tracking
        public int ProgressPercentage => (CurrentStep * 100) / TotalSteps;
        public string ProgressDisplay => $"{ProgressPercentage}%";

        // Step completion status
        public bool IsStep1Complete => SelectedPoolId.HasValue && SelectedPool != null;
        public bool IsStep2Complete => SelectedDexId.HasValue && SelectedDex != null;
        public bool IsStep3Complete => Token0Amount > 0 && Token1Amount > 0 && !HasErrors;
        public bool IsStep4Complete => !HasErrors && Calculator is not null;
        public bool IsStep5Complete => !string.IsNullOrEmpty(TransactionHash);

        // Navigation helpers
        public bool CanGoToNextStep => CurrentStep switch
        {
            1 => IsStep1Complete,
            2 => IsStep2Complete,
            3 => IsStep3Complete,
            4 => IsStep4Complete,
            _ => false
        };

        public bool CanGoToPreviousStep => CurrentStep > 1;

        // Action availability
        public bool CanSelectPool => IsWalletConnected;
        public bool CanInputAmounts => IsStep1Complete && IsStep2Complete;
        public bool CanSubmitTransaction => IsStep3Complete && IsStep4Complete;
        public bool CanRefreshCalculation => CanInputAmounts && Token0Amount > 0;

        // Risk assessment
        public List<string> RiskFactors { get; set; } = new();
        public string RiskLevel { get; set; } = "Medium";
        public string RiskLevelClass => $"risk-{RiskLevel.ToLower()}";

        // Educational content
        public bool ShowBeginnerTips { get; set; } = true;
        public bool ShowAdvancedOptions { get; set; }
        public bool ShowImpermanentLossWarning => PriceImpact > 1;
        public bool ShowHighSlippageWarning => SlippageTolerance > 5;

        // Transaction settings
        public bool AutoApproveTokens { get; set; } = true;
        public bool UseMaxGasPrice { get; set; }
        public decimal MaxGasPrice { get; set; }
        public int GasLimit { get; set; } = 300000;

        // Step titles and descriptions
        public Dictionary<int, string> StepTitles { get; set; } = new()
        {
            { 1, "Select Pool" },
            { 2, "Choose Exchange" },
            { 3, "Enter Amounts" },
            { 4, "Review & Confirm" },
            { 5, "Transaction Status" }
        };

        public Dictionary<int, string> StepDescriptions { get; set; } = new()
        {
            { 1, "Choose a liquidity pool to provide liquidity to" },
            { 2, "Select the decentralized exchange to use" },
            { 3, "Specify the amounts of tokens to add" },
            { 4, "Review your transaction details before submitting" },
            { 5, "Monitor your transaction progress" }
        };

        // Validation methods
        public bool ValidateCurrentStep()
        {
            ValidationErrors.Clear();
            Warnings.Clear();

            switch (CurrentStep)
            {
                case 1:
                    return ValidateStep1();
                case 2:
                    return ValidateStep2();
                case 3:
                    return ValidateStep3();
                case 4:
                    return ValidateStep4();
                default:
                    return true;
            }
        }

        private bool ValidateStep1()
        {
            if (!IsWalletConnected)
            {
                ValidationErrors.Add("Please connect your wallet first");
                return false;
            }

            if (!SelectedPoolId.HasValue)
            {
                ValidationErrors.Add("Please select a liquidity pool");
                return false;
            }

            if (SelectedPool == null)
            {
                ValidationErrors.Add("Invalid pool selection");
                return false;
            }

            if (!SelectedPool.IsActive)
            {
                ValidationErrors.Add("Selected pool is not currently active");
                return false;
            }

            return true;
        }

        private bool ValidateStep2()
        {
            if (!SelectedDexId.HasValue)
            {
                ValidationErrors.Add("Please select a decentralized exchange");
                return false;
            }

            if (SelectedDex == null)
            {
                ValidationErrors.Add("Invalid DEX selection");
                return false;
            }

            if (!SelectedDex.IsActive)
            {
                ValidationErrors.Add("Selected exchange is not currently active");
                return false;
            }

            return true;
        }

        private bool ValidateStep3()
        {
            if (Token0Amount <= 0)
            {
                ValidationErrors.Add($"Please enter a valid {SelectedPool?.Token0Symbol} amount");
                return false;
            }

            if (Token1Amount <= 0)
            {
                ValidationErrors.Add($"Please enter a valid {SelectedPool?.Token1Symbol} amount");
                return false;
            }

            if (Token0Amount > Token0Balance)
            {
                ValidationErrors.Add($"Insufficient {SelectedPool?.Token0Symbol} balance");
                return false;
            }

            if (Token1Amount > Token1Balance)
            {
                ValidationErrors.Add($"Insufficient {SelectedPool?.Token1Symbol} balance");
                return false;
            }

            if (EstimatedGasCost > EthBalance)
            {
                ValidationErrors.Add("Insufficient ETH for gas fees");
                return false;
            }

            // Warnings
            if (SlippageTolerance > 5)
            {
                Warnings.Add("High slippage tolerance may result in significant price impact");
            }

            if (PriceImpact > 1)
            {
                Warnings.Add("High price impact detected. Consider smaller amounts or different pool");
            }

            if (Token0Amount > Token0Balance * 0.9m || Token1Amount > Token1Balance * 0.9m)
            {
                Warnings.Add("Using most of your token balance. Consider keeping some for future transactions");
            }

            return true;
        }

        private bool ValidateStep4()
        {
            if (Calculator is null)
            {
                ValidationErrors.Add("Please recalculate transaction details");
                return false;
            }

            if (ExpectedLpTokens <= 0)
            {
                ValidationErrors.Add("Invalid LP token calculation");
                return false;
            }

            if (PriceImpact > 10)
            {
                ValidationErrors.Add("Price impact too high. Please reduce amounts or try a different pool");
                return false;
            }

            return true;
        }

        // Helper methods for amounts calculation
        public void CalculateOptimalAmounts()
        {
            if (SelectedPool == null || Token0Amount <= 0) return;

            // Calculate optimal Token1 amount based on current pool ratio
            var ratio = SelectedPool.Token1Reserve / SelectedPool.Token0Reserve;
            Token1Amount = Token0Amount * ratio;
        }

        public void CalculateFromToken1Amount()
        {
            if (SelectedPool == null || Token1Amount <= 0) return;

            // Calculate optimal Token0 amount based on current pool ratio
            var ratio = SelectedPool.Token0Reserve / SelectedPool.Token1Reserve;
            Token0Amount = Token1Amount * ratio;
        }

        // Reset methods
        public void ResetToStep(int step)
        {
            CurrentStep = Math.Max(1, Math.Min(step, TotalSteps));

            if (step <= 1)
            {
                SelectedPoolId = null;
                SelectedPool = null;
            }

            if (step <= 2)
            {
                SelectedDexId = null;
                SelectedDex = null;
            }

            if (step <= 3)
            {
                Token0Amount = 0;
                Token1Amount = 0;
                Calculator = null;
            }

            if (step <= 4)
            {
                TransactionHash = null;
                TransactionStatus = "Pending";
                BlockConfirmations = 0;
            }

            ValidationErrors.Clear();
            Warnings.Clear();
        }

        // Etherscan transaction URL
        public string TransactionUrl => !string.IsNullOrEmpty(TransactionHash)
            ? $"https://etherscan.io/tx/{TransactionHash}"
            : string.Empty;

        // Transaction status helpers
        public bool IsTransactionPending => TransactionStatus == "Pending";
        public bool IsTransactionConfirmed => TransactionStatus == "Confirmed";
        public bool IsTransactionFailed => TransactionStatus == "Failed";

        public string TransactionStatusClass => TransactionStatus.ToLower() switch
        {
            "pending" => "status-pending",
            "confirmed" => "status-success",
            "failed" => "status-error",
            _ => "status-unknown"
        };

        public string ConfirmationProgress => RequiredConfirmations > 0
            ? $"{BlockConfirmations}/{RequiredConfirmations}"
            : "0/0";

        public int ConfirmationPercentage => RequiredConfirmations > 0
            ? Math.Min(100, (BlockConfirmations * 100) / RequiredConfirmations)
            : 0;
    }
}