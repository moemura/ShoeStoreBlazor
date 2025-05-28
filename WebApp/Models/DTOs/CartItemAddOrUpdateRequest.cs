namespace WebApp.Models.DTOs
{
    public class CartItemAddOrUpdateRequest
    {
        public string VariantId { get; set; }
        public int Quantity { get; set; }
    }
} 