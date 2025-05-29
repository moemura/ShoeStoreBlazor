namespace WebApp.Models.DTOs
{
    public class OrderCreateRequest
    {
        public string CustomerName { get; set; }
        public string Phone { get; set; }
        public string? Email { get; set; }
        public string Address { get; set; }
        public string? VoucherCode { get; set; }
        public PaymentMethod PaymentMethod { get; set; }
        public string? CustomerNote { get; set; }
        public List<OrderCreateItemRequest> Items { get; set; } = new();
    }

    public class OrderCreateItemRequest
    {
        public int InventoryId { get; set; }
        public int Quantity { get; set; }
    }
} 