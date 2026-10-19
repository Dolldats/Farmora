using Farmora.Domain.Entities;

namespace Farmora.Application.Interfaces.Repositories;

public interface ICartRepository
{
    Task<Cart?> GetActiveWithItemsAsync(Guid customerId, CancellationToken cancellationToken);
    Task<Cart?> GetByIdWithItemsAsync(Guid cartId, CancellationToken cancellationToken);
    Task<CartItem?> GetItemAsync(Guid cartItemId, CancellationToken cancellationToken);
    Task AddAsync(Cart cart, CancellationToken cancellationToken);
    Task AddItemAsync(CartItem item, CancellationToken cancellationToken);
    Task RemoveItemAsync(CartItem item, CancellationToken cancellationToken);
    Task SaveChangesAsync(CancellationToken cancellationToken);
}
