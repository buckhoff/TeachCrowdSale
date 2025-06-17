using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TeachCrowdSale.Core.Models.Liquidity
{
    /// <summary>
    /// Model for market predictions
    /// </summary>
    public class MarketPredictionModel
    {
        public string Metric { get; set; } = string.Empty;
        public decimal CurrentValue { get; set; }
        public decimal PredictedValue { get; set; }
        public decimal Confidence { get; set; }
        public string Timeframe { get; set; } = string.Empty;
        public string Reasoning { get; set; } = string.Empty;

        public decimal PredictedChange => CurrentValue > 0 ? ((PredictedValue - CurrentValue) / CurrentValue) * 100 : 0;
        public string PredictedChangeDisplay => $"{(PredictedChange >= 0 ? "+" : "")}{PredictedChange:F1}%";
        public string ConfidenceDisplay => $"{Confidence:F0}%";

        public string PredictionClass => PredictedChange switch
        {
            > 10 => "prediction-very-positive",
            > 0 => "prediction-positive",
            > -10 => "prediction-negative",
            _ => "prediction-very-negative"
        };

        public string ConfidenceClass => Confidence switch
        {
            >= 80 => "confidence-high",
            >= 60 => "confidence-medium",
            _ => "confidence-low"
        };
    }
}
