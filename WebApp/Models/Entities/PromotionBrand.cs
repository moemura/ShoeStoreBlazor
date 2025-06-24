using System.ComponentModel.DataAnnotations;

namespace WebApp.Models.Entities;

public class PromotionBrand : BaseEntity<string>
{
    [Required]
    public string PromotionId { get; set; } = string.Empty;
    
    [Required]
    public string BrandId { get; set; } = string.Empty;
    
    // Navigation properties
    public virtual Promotion? Promotion { get; set; }
    public virtual Brand? Brand { get; set; }
} 