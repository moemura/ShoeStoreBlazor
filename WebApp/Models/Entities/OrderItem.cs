using System.ComponentModel.DataAnnotations;

namespace WebApp.Models.Entities
{
    public class OrderItem
    {
        [Key]
        public Guid Id { get; set; }
        public Guid OrderId { get; set; }
        public int InventoryId { get; set; }
        public double Price { get; set; }
        public int Quantity { get; set; }
        public double Subtotal { get; set; }
        public virtual Order? Order { get; set; }
        public virtual Inventory? Inventory { get; set; }
    }
} 