using Nethereum.Util;
using System.ComponentModel.DataAnnotations;

namespace EthereumExchangeWallet.Api.Common
{
    public class EthAddressValidator: ValidationAttribute
    {
        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            var addressUtil = new AddressUtil();

            if (! addressUtil.IsValidEthereumAddressHexFormat(value.ToString()))
            {
                return new ValidationResult("Ethereum address is not in valid format");
            }   

            return ValidationResult.Success;
        }
    }
}
