using Farmora.Application.DTO.Cart;

namespace Farmora.Application.Interfaces.Services;

public interface ICartService
{
    Task<CartResponse?> GetAsync(Guid customerId, CancellationToken cancellationToken);
    Task<CartResponse> AddItemAsync(Guid customerId, AddCartItemDto dto, CancellationToken cancellationToken);
    Task<CartResponse?> UpdateItemAsync(Guid customerId, Guid cartItemId, UpdateCartItemDto dto, CancellationToken cancellationToken);
    Task<bool> RemoveItemAsync(Guid customerId, Guid cartItemId, CancellationToken cancellationToken);
    Task ClearAsync(Guid customerId, CancellationToken cancellationToken);
}
