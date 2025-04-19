using System;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using TeachCrowdSale.Api.ModelBinding;
using TeachCrowdSale.Core.Attributes;
using TeachCrowdSale.Core.Attributes;

namespace TeachCrowdSale.Api.ModelBinding
{
    public class EthereumAddressModelBinderProvider : IModelBinderProvider
    {
        public IModelBinder GetBinder(ModelBinderProviderContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            // Apply to string properties with the EthereumAddress attribute
            if (context.Metadata.ModelType == typeof(string) && 
                context.Metadata.ModelType.GetCustomAttributes(typeof(EthereumAddressAttribute),true).Any())
            {
                return new EthereumAddressModelBinder();
            }

            return null;
        }
    }
}