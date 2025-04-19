using System;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace TeachCrowdSale.Api.ModelBinding
{
    public class EthereumAddressModelBinder : IModelBinder
    {
        private static readonly Regex AddressRegex = new Regex("^0x[0-9a-fA-F]{40}$", RegexOptions.Compiled | RegexOptions.IgnoreCase);
        
        public Task BindModelAsync(ModelBindingContext bindingContext)
        {
            if (bindingContext == null)
            {
                throw new ArgumentNullException(nameof(bindingContext));
            }

            var modelName = bindingContext.ModelName;
            var valueProviderResult = bindingContext.ValueProvider.GetValue(modelName);
            
            if (valueProviderResult == ValueProviderResult.None)
            {
                return Task.CompletedTask;
            }

            bindingContext.ModelState.SetModelValue(modelName, valueProviderResult);
            var value = valueProviderResult.FirstValue;

            if (string.IsNullOrEmpty(value))
            {
                return Task.CompletedTask;
            }

            // Check if it's a valid Ethereum address
            if (!AddressRegex.IsMatch(value))
            {
                bindingContext.ModelState.TryAddModelError(modelName, "Invalid Ethereum address format");
                return Task.CompletedTask;
            }

            // Normalize to lowercase
            bindingContext.Result = ModelBindingResult.Success(value.ToLowerInvariant());
            return Task.CompletedTask;
        }
    }
}