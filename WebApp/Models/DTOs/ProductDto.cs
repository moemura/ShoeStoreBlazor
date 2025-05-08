using System.ComponentModel.DataAnnotations;

namespace WebApp.Models.DTOs
{
    public class ProductDto
    {
        public string Id { get; set; }
        [Required(ErrorMessage = "Tên không được để trống!")]
        public string Name { get; set; } = default!;
        public string Description { get; set; } = default!;
        [Range(0, 100000000)]
        public double Price { get; set; }
        [Range(0, 100000000)]
        public double? SalePrice { get; set; }
        public string? MainImage { get; set; }
        public string? Image { get; set; }
        public int LikeCount { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}
