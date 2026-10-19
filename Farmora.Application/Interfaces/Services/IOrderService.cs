using Farmora.Application.Common;
using Farmora.Application.DTO.Orders;

namespace Farmora.Application.Interfaces.Services;

public interface IOrderService
{
    Task<QuoteResponse> QuoteAsync(Guid customerId, QuoteRequest request, CancellationToken cancellationToken);
    Task<OrderResponse> CreateAsync(Guid customerId, CreateOrderDto dto, CancellationToken cancellationToken);
    Task<PagedResult<OrderResponse>> GetMyOrdersAsync(Guid customerId, int page, int pageSize, CancellationToken cancellationToken);
    Task<OrderResponse?> GetByIdAsync(Guid customerId, Guid orderId, CancellationToken cancellationToken);
    Task<bool> CancelAsync(Guid customerId, Guid orderId, CancellationToken cancellationToken);
}
