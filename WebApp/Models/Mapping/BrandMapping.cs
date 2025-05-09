using WebApp.Models.DTOs;
using WebApp.Models.Entities;

namespace WebApp.Models.Mapping
{
    public static class BrandMapping
    {
        public static BrandDto ToDto(this Brand entity)
        {
            if (entity == null)
                return null;
            return new BrandDto
            {
                Id = entity.Id,
                Name = entity.Name,
                Description = entity.Description,
                Logo = entity.Logo,
                CreatedAt = entity.CreatedAt,
                UpdatedAt = entity.UpdatedAt
            };
        }

        public static Brand ToEntity(this BrandDto dto)
        {
            if (dto == null)
                return null;
            return new Brand
            {
                Id = dto.Id,
                Name = dto.Name,
                Description = dto.Description,
                Logo = dto.Logo,
                CreatedAt = dto.CreatedAt,
                UpdatedAt = dto.UpdatedAt
            };
        }
    }
} 