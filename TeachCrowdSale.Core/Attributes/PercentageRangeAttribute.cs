using System.ComponentModel.DataAnnotations;

namespace TeachCrowdSale.Core.Attributes;

/// <summary>
/// Validates that a numeric value falls within a specified percentage range (0-100 by default)
/// </summary>
public class PercentageRangeAttribute : ValidationAttribute
{
    private readonly decimal _min;
    private readonly decimal _max;
        
    public PercentageRangeAttribute(double min = 0, double max = 100)
    {
        _min = (decimal)min;
        _max = (decimal)max;
    }
        
    protected override ValidationResult IsValid(object value, ValidationContext validationContext)
    {
        if (value is decimal decimalValue)
        {
            if (decimalValue < _min || decimalValue > _max)
            {
                return new ValidationResult($"Value must be between {_min}% and {_max}%");
            }
                
            return ValidationResult.Success;
        }
            
        if (value is int intValue)
        {
            if (intValue < _min || intValue > _max)
            {
                return new ValidationResult($"Value must be between {_min}% and {_max}%");
            }
                
            return ValidationResult.Success;
        }
            
        return new ValidationResult("Value must be a numeric type");
    }
}