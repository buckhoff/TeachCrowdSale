using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace TeachCrowdSale.Core.Attributes;

/// <summary>
/// Validates that a string represents a valid blockchain transaction hash
/// </summary>
public class TransactionHashAttribute : ValidationAttribute
{
    protected override ValidationResult IsValid(object value, ValidationContext validationContext)
    {
        var hash = value as string;
            
        if (string.IsNullOrWhiteSpace(hash))
        {
            return ValidationResult.Success; // Allow null/empty since transaction might not be complete
        }
            
        if (!hash.StartsWith("0x") || !Regex.IsMatch(hash.Substring(2), "^[0-9a-fA-F]{64}$"))
        {
            return new ValidationResult("Invalid transaction hash format");
        }
            
        return ValidationResult.Success;
    }
}