using Farmora.Application.Interfaces.Repositories;
using Farmora.Domain.Entities;
using Farmora.Infastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Farmora.Infastructure.Repositories;

public sealed class CartRepository(FarmoraDbContext db) : ICartRepository
{
    public Task<Cart?> GetActiveWithItemsAsync(Guid customerId, CancellationToken cancellationToken) =>
        db.Set<Cart>()
            .Include(c => c.Items)
            .FirstOrDefaultAsync(c => c.CustomerId == customerId && c.IsActive, cancellationToken);

    public Task<Cart?> GetByIdWithItemsAsync(Guid cartId, CancellationToken cancellationToken) =>
        db.Set<Cart>()
            .Include(c => c.Items)
            .FirstOrDefaultAsync(c => c.Id == cartId, cancellationToken);

    public Task<CartItem?> GetItemAsync(Guid cartItemId, CancellationToken cancellationToken) =>
        db.Set<CartItem>()
            .Include(i => i.Product)
            .FirstOrDefaultAsync(i => i.Id == cartItemId, cancellationToken);

    public Task AddAsync(Cart cart, CancellationToken cancellationToken) =>
        db.Set<Cart>().AddAsync(cart, cancellationToken).AsTask();

    public Task AddItemAsync(CartItem item, CancellationToken cancellationToken) =>
        db.Set<CartItem>().AddAsync(item, cancellationToken).AsTask();

    public Task RemoveItemAsync(CartItem item, CancellationToken cancellationToken)
    {
        db.Set<CartItem>().Remove(item);
        return Task.CompletedTask;
    }

    public Task SaveChangesAsync(CancellationToken cancellationToken) =>
        db.SaveChangesAsync(cancellationToken);
}
