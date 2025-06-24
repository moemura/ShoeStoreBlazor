using System.ComponentModel.DataAnnotations;

namespace WebApp.Models.Entities;

public class PromotionCategory : BaseEntity<string>
{
    [Required]
    public string PromotionId { get; set; } = string.Empty;
    
    [Required]
    public string CategoryId { get; set; } = string.Empty;
    
    // Navigation properties
    public virtual Promotion? Promotion { get; set; }
    public virtual Category? Category { get; set; }
} 