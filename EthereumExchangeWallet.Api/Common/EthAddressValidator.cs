using Nethereum.Util;
using System.ComponentModel.DataAnnotations;

namespace EthereumExchangeWallet.Api.Common
{
    public class EthAddressValidator: ValidationAttribute
    {
        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            if (! value.ToString().IsValidEthereumAddressHexFormat())
            {
                return new ValidationResult("Ethereum address is not in valid format");
            }   

            return ValidationResult.Success;
        }
    }
}
