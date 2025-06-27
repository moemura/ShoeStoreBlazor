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
        
        [Obsolete("SalePrice is deprecated. Use PromotionPrice instead. This field will be removed in future versions.")]
        public double? SalePrice { get; set; }
        
        // Promotion information (preferred over SalePrice)
        public double? PromotionPrice { get; set; }
        public double? PromotionDiscount { get; set; }
        public string? PromotionName { get; set; }
        public bool HasActivePromotion { get; set; }
    }
} 