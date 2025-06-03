namespace TeachCrowdSale.Core.Models.Liquidity
{
    public class LiquidityGuideStepModel
    {
        public int StepNumber { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Icon { get; set; } = string.Empty;
        public bool IsCompleted { get; set; }
        public string ActionText { get; set; } = string.Empty;
        public string ActionUrl { get; set; } = string.Empty;
    }
}
