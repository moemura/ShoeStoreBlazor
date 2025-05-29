namespace WebApp.Models.DTOs
{
    public class CartItemDto
    {
        public int InventoryId { get; set; }
        public int Quantity { get; set; }
        public string ProductName { get; set; }
        public string Size { get; set; }
    }
} 