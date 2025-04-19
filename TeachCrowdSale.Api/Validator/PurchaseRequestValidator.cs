using System.Text.RegularExpressions;
using FluentValidation;
using TeachCrowdSale.Core.Interfaces;
using TeachCrowdSale.Core.Models.Request;

namespace TeachCrowdSale.Api.Validator;

public class PurchaseRequestValidator : AbstractValidator<PurchaseRequestModel>
{
    private readonly IPresaleService _presaleService;
    
    public PurchaseRequestValidator(IPresaleService presaleService)
    {
        _presaleService = presaleService;
        
        RuleFor(x => x.Address)
            .NotEmpty().WithMessage("Address is required")
            .Must(BeValidEthereumAddress).WithMessage("Invalid Ethereum address format");
            
        RuleFor(x => x.TierId)
            .GreaterThanOrEqualTo(0).WithMessage("Tier ID must be a positive number")
            .MustAsync(BeTierAvailable).WithMessage("Selected tier is not available");
            
        RuleFor(x => x.Amount)
            .GreaterThan(0).WithMessage("Amount must be greater than 0")
            .MustAsync(BeWithinTierLimits).WithMessage("Amount must be within tier limits");
    }
    
    private bool BeValidEthereumAddress(string address)
    {
        return !string.IsNullOrEmpty(address) && 
               address.StartsWith("0x") && 
               Regex.IsMatch(address.Substring(2), "^[0-9a-fA-F]{40}$");
    }
        
    private async Task<bool> BeTierAvailable(int tierId, CancellationToken cancellationToken)
    {
        var tiers = await _presaleService.GetAllTiersAsync();
        return tiers.Any(t => t.Id == tierId && t.IsActive);
    }
        
    private async Task<bool> BeWithinTierLimits(PurchaseRequestModel request, decimal amount, ValidationContext<PurchaseRequestModel> context, CancellationToken cancellationToken)
    {
        if (request.TierId < 0)
        {
            return false;
        }
            
        var tiers = await _presaleService.GetAllTiersAsync();
        var tier = tiers.FirstOrDefault(t => t.Id == request.TierId);
            
        if (tier == null)
        {
            return false;
        }
            
        return amount >= tier.MinPurchase && amount <= tier.MaxPurchase;
    }
}