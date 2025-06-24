using System.ComponentModel.DataAnnotations;

namespace WebApp.Models.Entities;

public class PromotionProduct : BaseEntity<string>
{
    [Required]
    public string PromotionId { get; set; } = string.Empty;
    
    [Required]
    public string ProductId { get; set; } = string.Empty;
    
    // Navigation properties
    public virtual Promotion? Promotion { get; set; }
    public virtual Product? Product { get; set; }
} 