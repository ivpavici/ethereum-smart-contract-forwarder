using EthereumExchangeWallet.Api.Data.Entities;
using EthereumExchangeWallet.Api.Data.Repositories;
using EthereumExchangeWallet.Api.Data;
using System.Threading.Tasks;

namespace EthereumExchangeWallet.Api.Services
{
    public class UserService : IUserService
    {
        private readonly IRepository<User> _userRepository;
        private readonly IRepository<Asset> _assetRepository;
        private readonly DepositAddressRepository _depositAddressRepository;
        private readonly INethereumService _nethereumService;
        private readonly DataContext _context;

        public UserService(IRepository<User> userRepository, IRepository<Asset> assetRepository, DataContext context, INethereumService nethereumService, DepositAddressRepository depositAddressRepository)
        {
            _userRepository = userRepository;
            _assetRepository = assetRepository;
            _context = context;
            _nethereumService = nethereumService;
            _depositAddressRepository = depositAddressRepository;
        }

        public async Task<bool> TryAddDepositAddress(int userId, int assetId)
        {
            var user = await _userRepository.GetItemAsync(userId);
            var asset = await _assetRepository.GetItemAsync(assetId);

            if (user == null || asset == null) return false;

            var forwaderContractAddress = await _depositAddressRepository.GetAddress(x => x.IsContractForwarderAddress);
            var factoryAddress = await _depositAddressRepository.GetAddress(x => x.IsContractFactoryAddress);
            
            if (forwaderContractAddress == null || factoryAddress == null) return false;

            // Calculate the new contract address, but it is not yet created on the blockchain, only when we detect that there is a deposit on the address
            var contractCalculatedAddress = _nethereumService.CalculateDepositAddress(userId, factoryAddress, forwaderContractAddress);

            DepositAddress assetDepositAddress = new()
            {
                Address = contractCalculatedAddress,
                Asset = asset,
                User = user
            };

            _context.DepositAddresses.Add(assetDepositAddress);
            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<bool> TryDepositToAddress(decimal amount, int userId, int assetId)
        {
            var depositAddress = await _depositAddressRepository.GetAddress(x => x.User.Id == userId && x.Asset.Id == assetId);
            if (depositAddress == null) return false;

            await _nethereumService.Deposit(amount, depositAddress);

            return true;
        }

        public async Task<bool> TryFlushEthereumToHotWallet(int userId)
        {
            var depositAddress = await _depositAddressRepository.GetAddress(x => x.User.Id == userId && x.Asset.Id == 1);
            var forwaderContractAddress = await _depositAddressRepository.GetAddress(x => x.IsContractForwarderAddress);
            var factoryAddress = await _depositAddressRepository.GetAddress(x => x.IsContractFactoryAddress);

            if (depositAddress == null || factoryAddress == null || forwaderContractAddress == null) return false;

            await _nethereumService.CloneForwarder(userId, depositAddress, forwaderContractAddress, factoryAddress);
            await _nethereumService.FlushEthereum(userId, depositAddress, forwaderContractAddress, factoryAddress);

            return true;
        }
    }
}
