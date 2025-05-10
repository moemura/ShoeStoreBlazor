namespace WebApp.Models.Mapping
{
    public static class ProductMapping
    {
        public static ProductDto ToDto(this Product entity)
        {
            if (entity == null)
                return null;
            return new ProductDto
            {
                Id = entity.Id,
                Name = entity.Name,
                Description = entity.Description,
                Price = entity.Price,
                SalePrice = entity.SalePrice,
                MainImage = entity.MainImage,
                Images = entity.Image != null ? entity.Image.Split(',').ToList() : null,
                LikeCount = entity.LikeCount,
                CreatedAt = entity.CreatedAt,
                UpdatedAt = entity.UpdatedAt,
                CategoryId = entity.CategoryId,
                CategoryName = entity.Category?.Name,
                BrandId = entity.BrandId,
                BrandName = entity.Brand?.Name,
                Inventories = entity.Inventories?.Select(p => p.ToDto()).ToList() ?? [],
                TotalQuantity = entity.Inventories?.Sum(p => p.Quantity) ?? 0,
            };
        }
        public static Product ToEntity(this ProductDto dto)
        {
            if (dto == null)
                return null;
            return new Product
            {
                Id = dto.Id,
                Name = dto.Name,
                Description = dto.Description,
                Price = dto.Price,
                SalePrice = dto.SalePrice,
                MainImage = dto.MainImage,
                Image = dto.Images != null ? string.Join(",", dto.Images) : null,
                LikeCount = dto.LikeCount,
                CreatedAt = dto.CreatedAt,
                UpdatedAt = dto.UpdatedAt,
                CategoryId = dto.CategoryId,
                BrandId = dto.BrandId,
            };
        }
    }
}
