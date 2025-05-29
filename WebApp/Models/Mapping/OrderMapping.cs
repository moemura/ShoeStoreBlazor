namespace WebApp.Models.Mapping;

public static class OrderMapping
{
    public static OrderDto ToDto(this Order order)
    {
        return new OrderDto
        {
            Id = order.Id,
            UserId = order.UserId,
            GuestId = order.GuestId,
            HandlerId = order.HandlerId,
            CustomerName = order.CustomerName,
            Phone = order.Phone,
            Email = order.Email,
            Address = order.Address,
            VoucherCode = order.VoucherCode,
            TotalAmount = order.TotalAmount,
            PaymentMethod = order.PaymentMethod,
            Status = order.Status,
            CustomerNote = order.CustomerNote,
            Note = order.Note,
            CreatedAt = order.CreatedAt,
            UpdatedAt = order.UpdatedAt,
            Items = order.Items?.Select(i => i.ToDto()).ToList() ?? new List<OrderItemDto>()
        };
    }

    public static OrderItemDto ToDto(this OrderItem item)
    {
        return new OrderItemDto
        {
            Id = item.Id,
            InventoryId = item.InventoryId,
            ProductId = item.Inventory?.ProductId ?? string.Empty,
            ProductName = item.Inventory?.Product?.Name ?? string.Empty,
            Size = item.Inventory?.SizeId ?? string.Empty,
            MainImage = item.Inventory?.Product?.MainImage,
            Price = item.Price,
            Quantity = item.Quantity,
            Subtotal = item.Subtotal
        };
    }

    public static Order ToEntity(this OrderCreateRequest req)
    {
        return new Order
        {
            CustomerName = req.CustomerName,
            Phone = req.Phone,
            Email = req.Email,
            Address = req.Address,
            VoucherCode = req.VoucherCode,
            PaymentMethod = req.PaymentMethod,
            CustomerNote = req.CustomerNote,
            Status = OrderStatus.Pending,
            CreatedAt = DateTime.UtcNow
        };
    }
}
