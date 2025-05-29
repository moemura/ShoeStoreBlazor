namespace WebApp.Models.DTOs;

public class OrderDto
{
    public Guid Id { get; set; }
    public string? UserId { get; set; }
    public string? GuestId { get; set; }
    public string? HandlerId { get; set; }
    public string CustomerName { get; set; }
    public string Phone { get; set; }
    public string? Email { get; set; }
    public string? VoucherCode { get; set; }
    public string Address { get; set; }
    public double TotalAmount { get; set; }
    public PaymentMethod PaymentMethod { get; set; }
    public OrderStatus Status { get; set; }
    public string? CustomerNote { get; set; }
    public string? Note { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public List<OrderItemDto> Items { get; set; } = new();
}
