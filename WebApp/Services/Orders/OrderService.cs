using WebApp.Services.Carts;
using WebApp.Services.Products;
using Microsoft.EntityFrameworkCore;

namespace WebApp.Services.Orders;

public class OrderService : IOrderService
{
    private readonly IDbContextFactory<ShoeStoreDbContext> _dbContextFactory;
    private readonly ICartService _cartService;
    private readonly IProductService _productService;
    private readonly OrderTotalStrategyFactory _orderTotalStrategyFactory;

    public OrderService(IDbContextFactory<ShoeStoreDbContext> dbContextFactory, ICartService cartService, IProductService productService, OrderTotalStrategyFactory orderTotalStrategyFactory)
    {
        _dbContextFactory = dbContextFactory;
        _cartService = cartService;
        _productService = productService;
        _orderTotalStrategyFactory = orderTotalStrategyFactory;
    }

    public async Task<OrderDto> CreateOrder(OrderCreateRequest req, string? userId, string? guestId)
    {
        using var dbContext = await _dbContextFactory.CreateDbContextAsync();
        
        // 1. Kiểm tra tồn kho từng item
        var orderItems = new List<OrderItem>();
        foreach (var item in req.Items)
        {
            var inventory = await _productService.CheckInventory(item.InventoryId, item.Quantity);
            if (inventory == null)
                throw new Exception($"Sản phẩm hoặc số lượng không đủ cho inventoryId {item.InventoryId}");
            // Lấy giá thực tế từ inventory/product
            double price = inventory.Product?.SalePrice ?? inventory.Product?.Price ?? 0; // TODO: Lấy giá đúng
            orderItems.Add(new OrderItem
            {
                InventoryId = item.InventoryId,
                Price = price,
                Quantity = item.Quantity,
                Subtotal = price * item.Quantity
            });
        }
        // 2. Tính tổng tiền (dùng strategy, có thể mở rộng discount/voucher)
        var totalStrategy = _orderTotalStrategyFactory.CreateStrategy(req); // TODO: truyền tham số thực tế
        double total = await totalStrategy.CalculateTotal(req);
        // 3. Tạo order entity
        var order = req.ToEntity();
        order.UserId = userId;
        order.GuestId = guestId;
        order.TotalAmount = total;
        order.Items = orderItems;
        // 4. Trừ kho
        foreach (var item in orderItems)
        {
            var inventory = dbContext.Inventories.First(i => i.Id == item.InventoryId);
            if (inventory.Quantity < item.Quantity)
                throw new Exception($"Sản phẩm inventoryId {item.InventoryId} không đủ tồn kho");
            inventory.Quantity -= item.Quantity;
        }
        // 5. Lưu DB
        dbContext.Orders.Add(order);
        await dbContext.SaveChangesAsync();
        // 6. Gọi payment strategy
        var paymentStrategy = PaymentStrategyFactory.GetStrategy((int)order.PaymentMethod);
        await paymentStrategy.ProcessPayment(order.ToDto());
        // 7. Xóa cache sản phẩm (invalidate cache)
        if (_productService is ProductService ps)
            await ps.RemoveProductCache();
        // 8. Xóa cart
        if (!string.IsNullOrEmpty(userId) || !string.IsNullOrEmpty(guestId))
        {
            var key = userId ?? guestId;
            await _cartService.ClearCart(key);
        }
        // 9. Trả về OrderDto
        return order.ToDto();
    }
    public async Task<IEnumerable<OrderDto>> GetOrdersByUser(string userId)
    {
        using var dbContext = await _dbContextFactory.CreateDbContextAsync();
        var orders = await dbContext.Orders
            .Include(o => o.Items)
            .ThenInclude(i => i.Inventory)
            .ThenInclude(inv => inv.Product)
            .Where(o => o.UserId == userId)
            .OrderByDescending(o => o.CreatedAt)
            .ToListAsync();
        return orders.Select(o => o.ToDto());
    }
    
    public async Task<OrderDto> GetOrderById(Guid orderId)
    {
        using var dbContext = await _dbContextFactory.CreateDbContextAsync();
        var order = await dbContext.Orders
            .Include(o => o.Items)
            .ThenInclude(i => i.Inventory)
            .ThenInclude(inv => inv.Product)
            .FirstOrDefaultAsync(o => o.Id == orderId);
        if (order == null) throw new Exception("Order not found");
        return order.ToDto();
    }
    
    public async Task UpdateOrderStatus(Guid orderId, int status, string? note = null)
    {
        using var dbContext = await _dbContextFactory.CreateDbContextAsync();
        var order = await dbContext.Orders.FirstOrDefaultAsync(o => o.Id == orderId);
        if (order == null) throw new Exception("Order not found");
        order.Status = (OrderStatus)status;
        if (!string.IsNullOrEmpty(note)) order.Note = note;
        order.UpdatedAt = DateTime.UtcNow;
        await dbContext.SaveChangesAsync();
    }
    public async Task CancelOrder(Guid orderId, string userId)
    {
        using var dbContext = await _dbContextFactory.CreateDbContextAsync();
        var order = await dbContext.Orders.Include(o => o.Items).FirstOrDefaultAsync(o => o.Id == orderId && o.UserId == userId);
        if (order == null) throw new Exception("Order not found or not allowed");
        if (order.Status == OrderStatus.Cancelled || order.Status == OrderStatus.Completed)
            throw new Exception("Order cannot be cancelled");
        order.Status = OrderStatus.Cancelled;
        order.UpdatedAt = DateTime.UtcNow;
        // Hoàn lại tồn kho
        foreach (var item in order.Items)
        {
            var inventory = await dbContext.Inventories.FirstOrDefaultAsync(i => i.Id == item.InventoryId);
            if (inventory != null)
                inventory.Quantity += item.Quantity;
        }
        await dbContext.SaveChangesAsync();
        // Xóa cache sản phẩm
        if (_productService is ProductService ps)
            await ps.RemoveProductCache();
    }

    public async Task RejectOrder(Guid orderId, string userId)
    {
        using var dbContext = await _dbContextFactory.CreateDbContextAsync();
        var order = await dbContext.Orders.Include(o => o.Items).FirstOrDefaultAsync(o => o.Id == orderId && o.UserId == userId);
        if (order == null) throw new Exception("Order not found or not allowed");
        if (order.Status == OrderStatus.Cancelled || order.Status == OrderStatus.Completed)
            throw new Exception("Order cannot be cancelled");
        order.Status = OrderStatus.Rejected;
        order.UpdatedAt = DateTime.UtcNow;
        // Hoàn lại tồn kho
        foreach (var item in order.Items)
        {
            var inventory = await dbContext.Inventories.FirstOrDefaultAsync(i => i.Id == item.InventoryId);
            if (inventory != null)
                inventory.Quantity += item.Quantity;
        }
        await dbContext.SaveChangesAsync();
        // Xóa cache sản phẩm
        if (_productService is ProductService ps)
            await ps.RemoveProductCache();
    }

    public async Task AdminCancelOrder(Guid orderId, string? note = null)
    {
        using var dbContext = await _dbContextFactory.CreateDbContextAsync();
        var order = await dbContext.Orders.Include(o => o.Items).FirstOrDefaultAsync(o => o.Id == orderId);
        if (order == null) throw new Exception("Order not found");
        if (order.Status == OrderStatus.Cancelled || order.Status == OrderStatus.Completed)
            throw new Exception("Order cannot be cancelled");
        
        order.Status = OrderStatus.Cancelled;
        order.Note = note ?? "Hủy bởi admin";
        order.UpdatedAt = DateTime.UtcNow;
        
        // Hoàn lại tồn kho
        foreach (var item in order.Items)
        {
            var inventory = await dbContext.Inventories.FirstOrDefaultAsync(i => i.Id == item.InventoryId);
            if (inventory != null)
                inventory.Quantity += item.Quantity;
        }
        
        await dbContext.SaveChangesAsync();
        
        // Xóa cache sản phẩm
        if (_productService is ProductService ps)
            await ps.RemoveProductCache();
    }

    public async Task AdminRejectOrder(Guid orderId, string? note = null)
    {
        using var dbContext = await _dbContextFactory.CreateDbContextAsync();
        var order = await dbContext.Orders.Include(o => o.Items).FirstOrDefaultAsync(o => o.Id == orderId);
        if (order == null) throw new Exception("Order not found");
        if (order.Status == OrderStatus.Cancelled || order.Status == OrderStatus.Completed)
            throw new Exception("Order cannot be rejected");
        
        order.Status = OrderStatus.Rejected;
        order.Note = note ?? "Từ chối bởi admin";
        order.UpdatedAt = DateTime.UtcNow;
        
        // Hoàn lại tồn kho
        foreach (var item in order.Items)
        {
            var inventory = await dbContext.Inventories.FirstOrDefaultAsync(i => i.Id == item.InventoryId);
            if (inventory != null)
                inventory.Quantity += item.Quantity;
        }
        
        await dbContext.SaveChangesAsync();
        
        // Xóa cache sản phẩm
        if (_productService is ProductService ps)
            await ps.RemoveProductCache();
    }
    public async Task<PaginatedList<OrderDto>> FilterAndPaging(int pageIndex, int pageSize, Dictionary<string, string> filter)
    {
        using var dbContext = await _dbContextFactory.CreateDbContextAsync();
        var query = dbContext.Orders
            .Include(o => o.Items)
            .ThenInclude(i => i.Inventory)
            .ThenInclude(inv => inv.Product)
            .AsQueryable();
        if (filter.TryGetValue("status", out var statusStr) && Enum.TryParse<OrderStatus>(statusStr, out var status))
            query = query.Where(o => o.Status == status);
        if (filter.TryGetValue("userId", out var userId) && !string.IsNullOrWhiteSpace(userId))
            query = query.Where(o => o.UserId == userId);
        // Search by phone OR name (not both)
        if (filter.TryGetValue("phone", out var phone) && !string.IsNullOrWhiteSpace(phone))
        {
            if (filter.TryGetValue("name", out var name) && !string.IsNullOrWhiteSpace(name) && phone == name)
            {
                // If phone and name have same value, search in both fields (OR condition)
                query = query.Where(o => o.Phone.Contains(phone) || o.CustomerName.Contains(name));
            }
            else
            {
                // Search by phone only
                query = query.Where(o => o.Phone.Contains(phone));
            }
        }
        else if (filter.TryGetValue("name", out var customerName) && !string.IsNullOrWhiteSpace(customerName))
        {
            // Search by name only
            query = query.Where(o => o.CustomerName.Contains(customerName));
        }
        // Filter by payment method
        if (filter.TryGetValue("paymentMethod", out var paymentMethodStr) && Enum.TryParse<PaymentMethod>(paymentMethodStr, out var paymentMethod))
            query = query.Where(o => o.PaymentMethod == paymentMethod);
        
        // Filter by date range
        if (filter.TryGetValue("startDate", out var startDateStr) && DateTime.TryParse(startDateStr, out var startDate))
            query = query.Where(o => o.CreatedAt >= startDate);
        if (filter.TryGetValue("endDate", out var endDateStr) && DateTime.TryParse(endDateStr, out var endDate))
            query = query.Where(o => o.CreatedAt <= endDate.AddDays(1)); // Include end date
        
        // Filter by amount range
        if (filter.TryGetValue("minAmount", out var minAmountStr) && double.TryParse(minAmountStr, out var minAmount))
            query = query.Where(o => o.TotalAmount >= minAmount);
        if (filter.TryGetValue("maxAmount", out var maxAmountStr) && double.TryParse(maxAmountStr, out var maxAmount))
            query = query.Where(o => o.TotalAmount <= maxAmount);
        
        var totalItems = await query.CountAsync();
        var orders = await query.OrderByDescending(o => o.CreatedAt)
            .Skip((pageIndex - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
        var pageCount = (int)Math.Ceiling(totalItems / (double)pageSize);
        return new PaginatedList<OrderDto>
        {
            Data = orders.Select(o => o.ToDto()),
            PageIndex = pageIndex,
            PageSize = pageSize,
            ItemCount = totalItems,
            PageCount = pageCount,
            HasNext = pageIndex < pageCount,
            HasPrevious = pageIndex > 1
        };
    }
    
    public async Task SyncGuestOrdersToUser(string guestId, string userId)
    {
        using var dbContext = await _dbContextFactory.CreateDbContextAsync();
        var guestOrders = await dbContext.Orders
            .Where(o => o.UserId == null && o.GuestId == guestId)
            .ToListAsync();
        foreach (var order in guestOrders)
        {
            order.UserId = userId;
            order.Note = null;
            order.UpdatedAt = DateTime.UtcNow;
        }
        await dbContext.SaveChangesAsync();
    }
}
