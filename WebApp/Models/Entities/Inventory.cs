using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebApp.Models.Entities
{
    public class Inventory : BaseEntity<int>
    {       
        [Required]
        public string ProductId { get; set; } = string.Empty;
        
        [Required]
        public string SizeId { get; set; } = string.Empty;
        
        [Required]
        public int Quantity { get; set; }

        [ForeignKey("ProductId")]
        public virtual Product? Product { get; set; }

        [ForeignKey("SizeId")]
        public virtual Size? Size { get; set; }
    }
} 