namespace WebApp.Models.DTOs
{
    public class CartItemDto
    {
        public int InventoryId { get; set; }
        public int Quantity { get; set; }
        public string ProductId { get; set; }
        public string ProductName { get; set; }
        public string Size { get; set; }
        public string? MainImage { get; set; }
        public string? BrandName { get; set; }
        public double Price { get; set; }
        public double? SalePrice { get; set; }
    }
} 