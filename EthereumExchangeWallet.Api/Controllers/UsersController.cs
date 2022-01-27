using EthereumExchangeWallet.Api.Common;
using EthereumExchangeWallet.Api.Data.Entities;
using EthereumExchangeWallet.Api.Data.Repositories;
using EthereumExchangeWallet.Api.Services;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static EthereumExchangeWallet.Api.Controllers.Dtos;

namespace EthereumExchangeWallet.Api.Controllers
{
    [Route("api/users")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly IRepository<User> _repository;
        private readonly IUserService _userService;

        public UsersController(IRepository<User> repository, IUserService userService)
        {
            _repository = repository;
            _userService = userService;
        }

        [HttpGet]
        public async Task<IEnumerable<UserDto>> GetUsersAsync()
        {
            return (await _repository.GetListAsync()).Select(user => user.AsDto());
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<UserDto>> GetUserByIdAsync(int id)
        {
            var user = await _repository.GetItemAsync(id);
            if (user == null) return NotFound();

            return user.AsDto();
        }

        [HttpPost]
        public async Task<ActionResult<UserDto>> CreateUserAsync([FromBody] CreateUpdateUserDto userDto)
        {
            User user = new()
            {
                Username = userDto.Username
            };

            var result = await _repository.CreateAsync(user);

            return CreatedAtAction(nameof(GetUserByIdAsync), new { id = result.Id }, result);
        }

        [HttpPut("{id}")]
        public async Task<ActionResult> UpdateUserAsync(int id, [FromBody] CreateUpdateUserDto userDto)
        {
            var existingUser = await _repository.GetItemAsync(id);
            if (existingUser == null) return NotFound();

            existingUser.Username = userDto.Username;

            await _repository.UpdateAsync(existingUser);

            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteUserAsync(int id)
        {
            var existingUser = await _repository.GetItemAsync(id);
            if (existingUser == null) return NotFound();

            await _repository.DeleteAsync(existingUser);

            return NoContent();
        }

        [HttpPost("{id}/deposit-address")]
        public async Task<ActionResult> GenerateDepositAddress(int id, [FromBody] UserDepositAssetDto userDepositAddressDto)
        {
            var result = await _userService.TryAddDepositAddress(id, userDepositAddressDto.AssetId);

            if (result == false) return NotFound();

            return Ok(result);
        }

        [HttpPost("{id}/deposit-eth")]
        public async Task<ActionResult> DepositEth(int id, [FromBody] DepositDto depositDto)
        {
            var result = await _userService.TryDepositToAddress(depositDto.Amount, id, depositDto.AssetId);

            if (result == false) return NotFound();

            return Ok(result);
        }
    }
}
