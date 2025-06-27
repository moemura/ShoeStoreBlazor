using WebApp.Models.DTOs;
using WebApp.Models.Entities;

namespace WebApp.Models.Mapping;

public static class PromotionMapping
{
    public static PromotionDto ToDto(this Promotion entity)
    {
        if (entity == null) return null;

        return new PromotionDto
        {
            Id = entity.Id,
            Name = entity.Name,
            Description = entity.Description,
            Type = entity.Type.ToString(),
            Scope = entity.Scope.ToString(),
            DiscountValue = entity.DiscountValue,
            MaxDiscountAmount = entity.MaxDiscountAmount,
            MinOrderAmount = entity.MinOrderAmount,
            StartDate = entity.StartDate,
            EndDate = entity.EndDate,
            IsActive = entity.IsActive,
            Priority = entity.Priority,
            ProductIds = entity.PromotionProducts?.Select(pp => pp.ProductId).ToList() ?? new List<string>(),
            CategoryIds = entity.PromotionCategories?.Select(pc => pc.CategoryId).ToList() ?? new List<string>(),
            BrandIds = entity.PromotionBrands?.Select(pb => pb.BrandId).ToList() ?? new List<string>(),
            CreatedAt = entity.CreatedAt,
            UpdatedAt = entity.UpdatedAt,
            DateRange = new[] { entity.StartDate, entity.EndDate }
        };
    }

    public static Promotion ToEntity(this CreatePromotionRequest request)
    {
        if (request == null) return null;

        if (!Enum.TryParse<PromotionType>(request.Type, true, out var promotionType))
        {
            throw new ArgumentException("Invalid Promotion Type");
        }

        return new Promotion
        {
            Id = Guid.NewGuid().ToString(),
            Name = request.Name,
            Description = request.Description,
            Type = promotionType,
            Scope = Enum.Parse<PromotionScope>(request.Scope, true),
            DiscountValue = request.DiscountValue,
            MaxDiscountAmount = request.MaxDiscountAmount,
            MinOrderAmount = request.MinOrderAmount,
            StartDate = request.StartDate,
            EndDate = request.EndDate,
            Priority = request.Priority,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
    }

    public static void UpdateEntity(this CreatePromotionRequest dto, Promotion promotion)
    {
        if (dto == null || promotion == null) return;

        if (!Enum.TryParse<PromotionType>(dto.Type, true, out var promotionType))
        {
            throw new ArgumentException("Invalid Promotion Type");
        }
        
        promotion.Name = dto.Name;
        promotion.Description = dto.Description;
        promotion.Type = Enum.Parse<PromotionType>(dto.Type, true);
        promotion.Scope = Enum.Parse<PromotionScope>(dto.Scope, true);
        promotion.DiscountValue = dto.DiscountValue;
        promotion.MaxDiscountAmount = dto.MaxDiscountAmount;
        promotion.MinOrderAmount = dto.MinOrderAmount;
        promotion.StartDate = dto.StartDate;
        promotion.EndDate = dto.EndDate;
        promotion.Priority = dto.Priority;
    }
} 