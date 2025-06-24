using System.ComponentModel.DataAnnotations;

namespace WebApp.Models.Entities;

public class Promotion : BaseEntity<string>
{
    [Required(ErrorMessage = "Tên chương trình không được để trống!")]
    [MaxLength(200)]
    public string Name { get; set; } = string.Empty;
    
    [MaxLength(500)]
    public string Description { get; set; } = string.Empty;
    
    public PromotionType Type { get; set; }
    
    [Range(0, double.MaxValue, ErrorMessage = "Giá trị giảm giá phải lớn hơn 0")]
    public double DiscountValue { get; set; }
    
    [Range(0, double.MaxValue, ErrorMessage = "Số tiền giảm tối đa phải lớn hơn 0")]
    public double? MaxDiscountAmount { get; set; }
    
    public DateTime StartDate { get; set; }
    
    public DateTime EndDate { get; set; }
    
    public bool IsActive { get; set; } = true;
    
    [Range(1, 10, ErrorMessage = "Độ ưu tiên phải từ 1 đến 10")]
    public int Priority { get; set; } = 1;
    
    // Navigation properties
    public virtual ICollection<PromotionProduct>? PromotionProducts { get; set; }
    public virtual ICollection<PromotionCategory>? PromotionCategories { get; set; }
    public virtual ICollection<PromotionBrand>? PromotionBrands { get; set; }
}

public enum PromotionType
{
    Percentage = 1,      // Giảm theo %
    FixedAmount = 2,     // Giảm số tiền cố định
    BuyXGetY = 3        // Mua X tặng Y (future enhancement)
} 