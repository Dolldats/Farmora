using Farmora.Application.DTO.Cart;
using Farmora.Application.Interfaces.Repositories;
using Farmora.Application.Interfaces.Services;
using Farmora.Domain.Entities;
using Farmora.Infastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Farmora.Infastructure.Services;

public sealed class CartService(ICartRepository carts, FarmoraDbContext db) : ICartService
{
    public async Task<CartResponse?> GetAsync(Guid customerId, CancellationToken cancellationToken)
    {
        var customer = await RequireCustomerAsync(customerId, cancellationToken);
        var cart = await carts.GetActiveWithItemsAsync(customer.Id, cancellationToken);
        return cart is null ? null : Map(cart);
    }

    public async Task<CartResponse> AddItemAsync(Guid customerId, AddCartItemDto dto, CancellationToken cancellationToken)
    {
        var customer = await RequireCustomerAsync(customerId, cancellationToken);
        if (dto.Quantity <= 0)
            throw new ArgumentOutOfRangeException(nameof(dto.Quantity), "Quantity must be greater than zero.");

        var product = await db.Set<Product>().FirstOrDefaultAsync(p => p.Id == dto.ProductId, cancellationToken)
            ?? throw new InvalidOperationException("Product not found.");
        if (!product.IsAvailable)
            throw new InvalidOperationException("Product is not available.");
        if (product.Stock <= 0)
            throw new InvalidOperationException("Product is out of stock.");
        if (dto.Quantity > product.Stock)
            throw new InvalidOperationException("Requested quantity exceeds available stock.");

        var cart = await carts.GetActiveWithItemsAsync(customer.Id, cancellationToken);
        if (cart is null)
        {
            cart = new Cart(customer.Id);
            await carts.AddAsync(cart, cancellationToken);
            await carts.SaveChangesAsync(cancellationToken);
            cart = (await carts.GetActiveWithItemsAsync(customer.Id, cancellationToken))!;
        }

        var existing = cart.Items.FirstOrDefault(i => i.ProductId == dto.ProductId);
        if (existing is null)
        {
            await carts.AddItemAsync(new CartItem(cart.Id, dto.ProductId, dto.Quantity), cancellationToken);
        }
        else
        {
            var merged = existing.Quantity + dto.Quantity;
            if (merged > product.Stock)
                throw new InvalidOperationException("Requested quantity exceeds available stock.");
            existing.SetQuantity(merged);
        }

        await carts.SaveChangesAsync(cancellationToken);
        cart = (await carts.GetByIdWithItemsAsync(cart.Id, cancellationToken))!;
        return Map(cart);
    }

    public async Task<CartResponse?> UpdateItemAsync(Guid customerId, Guid cartItemId, UpdateCartItemDto dto, CancellationToken cancellationToken)
    {
        var customer = await RequireCustomerAsync(customerId, cancellationToken);
        if (dto.Quantity <= 0)
            throw new ArgumentOutOfRangeException(nameof(dto.Quantity), "Quantity must be greater than zero.");

        var item = await carts.GetItemAsync(cartItemId, cancellationToken);
        if (item is null) return null;
        var cart = await carts.GetByIdWithItemsAsync(item.CartId, cancellationToken);
        if (cart is null || cart.CustomerId != customer.Id || !cart.IsActive) return null;

        var product = item.Product
            ?? await db.Set<Product>().FirstOrDefaultAsync(p => p.Id == item.ProductId, cancellationToken);
        if (product is null || !product.IsAvailable || product.Stock <= 0)
            throw new InvalidOperationException("Product is not available.");
        if (dto.Quantity > product.Stock)
            throw new InvalidOperationException("Requested quantity exceeds available stock.");

        item.SetQuantity(dto.Quantity);
        await carts.SaveChangesAsync(cancellationToken);
        cart = (await carts.GetByIdWithItemsAsync(cart.Id, cancellationToken))!;
        return Map(cart);
    }

    public async Task<bool> RemoveItemAsync(Guid customerId, Guid cartItemId, CancellationToken cancellationToken)
    {
        var customer = await RequireCustomerAsync(customerId, cancellationToken);
        var item = await carts.GetItemAsync(cartItemId, cancellationToken);
        if (item is null) return false;
        var cart = await carts.GetByIdWithItemsAsync(item.CartId, cancellationToken);
        if (cart is null || cart.CustomerId != customer.Id || !cart.IsActive) return false;

        await carts.RemoveItemAsync(item, cancellationToken);
        await carts.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task ClearAsync(Guid customerId, CancellationToken cancellationToken)
    {
        var customer = await RequireCustomerAsync(customerId, cancellationToken);
        var cart = await carts.GetActiveWithItemsAsync(customer.Id, cancellationToken);
        if (cart is null) return;
        db.Set<CartItem>().RemoveRange(cart.Items);
        await carts.SaveChangesAsync(cancellationToken);
    }

    private async Task<Customer> RequireCustomerAsync(Guid idOrUserId, CancellationToken ct) =>
        await db.Set<Customer>().FirstOrDefaultAsync(c => c.Id == idOrUserId, ct)
        ?? await db.Set<Customer>().FirstOrDefaultAsync(c => c.UserId == idOrUserId, ct)
        ?? throw new InvalidOperationException("Customer profile required.");

    private static CartResponse Map(Cart cart) => new()
    {
        Id = cart.Id,
        CustomerId = cart.CustomerId,
        IsActive = cart.IsActive,
        Items = cart.Items.Select(i => new CartItemResponse
        {
            Id = i.Id,
            ProductId = i.ProductId,
            Quantity = i.Quantity,
        }).ToList(),
    };
}
