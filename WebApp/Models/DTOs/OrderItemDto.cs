using System;

namespace WebApp.Models.DTOs
{
    public class OrderItemDto
    {
        public Guid Id { get; set; }
        public int InventoryId { get; set; }
        public string ProductId { get; set; }
        public string ProductName { get; set; }
        public string Size { get; set; }
        public string? MainImage { get; set; }
        public double Price { get; set; }
        public int Quantity { get; set; }
        public double Subtotal { get; set; }
    }
} 