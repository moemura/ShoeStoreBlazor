using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebApp.Models.Entities;

public class Product : BaseEntity<string>
{
    [Required(ErrorMessage ="Tên không được để trống!")]
    public string Name { get; set; } = default!;
    public string Description { get; set; } = default!;
    [Range(0,100000000)]
    public double Price { get; set; }
    [Range(0,100000000)]
    public double? SalePrice { get; set; }
    public string? MainImage { get; set; }
    public string? Image { get; set; }
    public int LikeCount { get; set; }

    // Foreign keys
    public string? CategoryId { get; set; }
    public string? BrandId { get; set; }

    // Navigation properties
    [ForeignKey("CategoryId")]
    public Category? Category { get; set; }
    [ForeignKey("BrandId")]
    public Brand? Brand { get; set; }

    public virtual ICollection<Inventory>? Inventories { get; set; }
}
