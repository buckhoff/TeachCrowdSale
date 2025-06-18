using System.ComponentModel.DataAnnotations;

using TeachCrowdSale.Core.Attributes;

/// <summary>
/// Complete model for liquidity calculation display
/// FIXED: Added all missing properties for calculator functionality
/// </summary>
public class LiquidityCalculatorModel
{
    // Input values
    [Display(Name = "Pool ID")]
    public int PoolId { get; set; }

    [Display(Name = "Token Pair")]
    public string TokenPair { get; set; } = string.Empty;

    [Display(Name = "Token 0 Symbol")]
    public string Token0Symbol { get; set; } = string.Empty;

    [Display(Name = "Token 1 Symbol")]
    public string Token1Symbol { get; set; } = string.Empty;

    [EthereumAddress]
    [Display(Name = "Wallet Address")]
    public string WalletAddress { get; set; } = string.Empty;

    [Range(0, double.MaxValue)]
    [Display(Name = "Token 0 Amount")]
    public decimal Token0Amount { get; set; }

    [Range(0, double.MaxValue)]
    [Display(Name = "Token 1 Amount")]
    public decimal Token1Amount { get; set; }

    [Range(0.1, 50)]
    [Display(Name = "Slippage Tolerance (%)")]
    public decimal SlippageTolerance { get; set; } = 0.5m;

    // Calculated outputs
    [Display(Name = "Estimated LP Tokens")]
    public decimal EstimatedLpTokens { get; set; }

    [Display(Name = "Estimated Value (USD)")]
    public decimal EstimatedValueUsd { get; set; }

    [Display(Name = "Price Impact (%)")]
    public decimal PriceImpact { get; set; }

    [Display(Name = "Estimated APY (%)")]
    public decimal EstimatedAPY { get; set; }

    [Display(Name = "Estimated Daily Earnings")]
    public decimal EstimatedDailyEarnings { get; set; }

    [Display(Name = "Estimated Monthly Earnings")]
    public decimal EstimatedMonthlyEarnings { get; set; }

    [Display(Name = "Estimated Yearly Earnings")]
    public decimal EstimatedYearlyEarnings { get; set; }

    // Minimum amounts after slippage
    [Display(Name = "Minimum Token 0 Amount")]
    public decimal MinToken0Amount { get; set; }

    [Display(Name = "Minimum Token 1 Amount")]
    public decimal MinToken1Amount { get; set; }

    // Gas and transaction info
    [Display(Name = "Gas Estimate (USD)")]
    public decimal GasEstimate { get; set; }

    // Validation flags
    [Display(Name = "Has Sufficient Balance")]
    public bool HasSufficientBalance { get; set; }

    [Display(Name = "Has Sufficient Allowance")]
    public bool HasSufficientAllowance { get; set; }

    // User balances
    [Display(Name = "Token 0 Balance")]
    public decimal Token0Balance { get; set; }

    [Display(Name = "Token 1 Balance")]
    public decimal Token1Balance { get; set; }

    [Display(Name = "ETH Balance")]
    public decimal EthBalance { get; set; }

    // Messages
    [Display(Name = "Validation Messages")]
    public List<string> ValidationMessages { get; set; } = new();

    [Display(Name = "Warning Messages")]
    public List<string> WarningMessages { get; set; } = new();

    // Pool information
    [Display(Name = "Current Pool APY")]
    public decimal CurrentPoolAPY { get; set; }

    [Display(Name = "Pool Total Value Locked")]
    public decimal PoolTotalValueLocked { get; set; }

    [Display(Name = "Pool 24h Volume")]
    public decimal Pool24hVolume { get; set; }

    [Display(Name = "Pool Fee Percentage")]
    public decimal PoolFeePercentage { get; set; }

    // Risk assessment
    [Display(Name = "Risk Level")]
    public string RiskLevel { get; set; } = "Low";

    [Display(Name = "Impermanent Loss Estimate")]
    public string ImpermanentLossEstimate { get; set; } = "Minimal";

    // Display helpers
    [Display(Name = "Estimated Value Display")]
    public string EstimatedValueDisplay => $"${EstimatedValueUsd:N2}";

    [Display(Name = "Price Impact Display")]
    public string PriceImpactDisplay => $"{PriceImpact:F2}%";

    [Display(Name = "APY Display")]
    public string EstimatedAPYDisplay => $"{EstimatedAPY:F2}%";

    [Display(Name = "Daily Earnings Display")]
    public string DailyEarningsDisplay => $"${EstimatedDailyEarnings:F4}";

    [Display(Name = "Monthly Earnings Display")]
    public string MonthlyEarningsDisplay => $"${EstimatedMonthlyEarnings:F2}";

    [Display(Name = "Yearly Earnings Display")]
    public string YearlyEarningsDisplay => $"${EstimatedYearlyEarnings:N0}";

    [Display(Name = "Gas Estimate Display")]
    public string GasEstimateDisplay => $"${GasEstimate:F2}";

    // Risk styling classes
    public string RiskLevelClass => RiskLevel.ToLower() switch
    {
        "low" => "risk-low",
        "medium" => "risk-medium",
        "high" => "risk-high",
        _ => "risk-unknown"
    };

    public string PriceImpactClass => PriceImpact switch
    {
        < 1 => "impact-low",
        < 3 => "impact-medium",
        _ => "impact-high"
    };

    // Validation state
    public bool IsValid => HasSufficientBalance && HasSufficientAllowance &&
                          Token0Amount > 0 && Token1Amount > 0 &&
                          !ValidationMessages.Any();

    // Calculation timestamp
    public DateTime CalculatedAt { get; set; } = DateTime.UtcNow;
}