using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace TeachCrowdSale.Core.Attributes;

[AttributeUsage(AttributeTargets.Property)]
public class EthereumAddressAttribute: ValidationAttribute
{
    protected override ValidationResult IsValid(object value, ValidationContext validationContext)
    {
        var address = value as string;
        
        if (string.IsNullOrWhiteSpace(address))
        {
            return new ValidationResult("Address cannot be empty");
        }
        
        // Check if it's a valid Ethereum address format (0x followed by 40 hex chars)
        if (!address.StartsWith("0x") || !Regex.IsMatch(address.Substring(2), "^[0-9a-fA-F]{40}$"))
        {
            return new ValidationResult("Invalid Ethereum address format");
        }
        
        return ValidationResult.Success;
    }
}