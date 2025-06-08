namespace WebApp.Services.Orders;

public interface IOrderService
{
    Task<OrderDto> CreateOrder(OrderCreateRequest req, string? userId, string? guestId);
    Task<IEnumerable<OrderDto>> GetOrdersByUser(string userId);
    Task<OrderDto> GetOrderById(Guid orderId);
    Task UpdateOrderStatus(Guid orderId, int status, string? note = null);
    Task CancelOrder(Guid orderId, string userId);
    Task RejectOrder(Guid orderId, string userId);
    Task AdminCancelOrder(Guid orderId, string? note = null);
    Task AdminRejectOrder(Guid orderId, string? note = null);
    Task<PaginatedList<OrderDto>> FilterAndPaging(int pageIndex, int pageSize, Dictionary<string, string> filter);
    Task SyncGuestOrdersToUser(string guestId, string userId);
}
