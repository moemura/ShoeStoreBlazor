namespace WebApp.Models.Mapping
{
    public static class ProductMapping
    {
        public static ProductDto ToDto(this Product product)
        {
            if (product == null) 
                return null;
            return new ProductDto
            {
                Id = product.Id,
                Name = product.Name,
                Description = product.Description,
                Price = product.Price,
                SalePrice = product.SalePrice,
                LikeCount = product.LikeCount,
                CreatedAt = product.CreatedAt,
                UpdatedAt = product.UpdatedAt,
                Image = product.Image,
                MainImage = product.MainImage,
            };
        }
        public static Product ToEntity(this ProductDto dto)
        {
            return new Product
            {
                Id = dto.Id,
                Name = dto.Name,
                Description = dto.Description,
                Price = dto.Price,
                SalePrice = dto.SalePrice,
                LikeCount = dto.LikeCount,
                CreatedAt = dto.CreatedAt,
                UpdatedAt = dto.UpdatedAt,
                Image = dto.Image,
                MainImage = dto.MainImage,
            };
        }
    }
}
