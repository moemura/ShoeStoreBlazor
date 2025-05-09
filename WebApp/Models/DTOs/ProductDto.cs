using System.ComponentModel.DataAnnotations;

namespace WebApp.Models.DTOs
{
    public class ProductDto
    {
        public string Id { get; set; } = null!;
        [Required(ErrorMessage = "Tên không được để trống!")]
        public string Name { get; set; } = null!;
        public string Description { get; set; } = null!;
        [Range(0, 100000000)]
        public double Price { get; set; }
        [Range(0, 100000000)]
        public double? SalePrice { get; set; }
        public string? MainImage { get; set; }
        public string? Image { get; set; }
        public int LikeCount { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }

        // Category and Brand information
        public string? CategoryId { get; set; }
        public string? CategoryName { get; set; }
        public string? BrandId { get; set; }
        public string? BrandName { get; set; }
    }
}
