namespace WebApp.Models.Mapping;

public static class PromotionMapping
{
    public static PromotionDto ToDto(this Promotion promotion)
    {
        return new PromotionDto
        {
            Id = promotion.Id,
            Name = promotion.Name,
            Description = promotion.Description,
            Type = promotion.Type.ToString(),
            DiscountValue = promotion.DiscountValue,
            MaxDiscountAmount = promotion.MaxDiscountAmount,
            StartDate = promotion.StartDate,
            EndDate = promotion.EndDate,
            IsActive = promotion.IsActive,
            Priority = promotion.Priority,
            ProductIds = promotion.PromotionProducts?.Select(pp => pp.ProductId).ToList() ?? new(),
            CategoryIds = promotion.PromotionCategories?.Select(pc => pc.CategoryId).ToList() ?? new(),
            BrandIds = promotion.PromotionBrands?.Select(pb => pb.BrandId).ToList() ?? new(),
            CreatedAt = promotion.CreatedAt,
            UpdatedAt = promotion.UpdatedAt
        };
    }

    public static Promotion ToEntity(this CreatePromotionRequest request)
    {
        if (!Enum.TryParse<PromotionType>(request.Type, out var promotionType))
            throw new ArgumentException($"Invalid promotion type: {request.Type}");

        return new Promotion
        {
            Id = Guid.CreateVersion7().ToString(),
            Name = request.Name,
            Description = request.Description,
            Type = promotionType,
            DiscountValue = request.DiscountValue,
            MaxDiscountAmount = request.MaxDiscountAmount,
            StartDate = request.StartDate,
            EndDate = request.EndDate,
            Priority = request.Priority,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
    }

    public static Promotion ToEntity(this PromotionDto dto)
    {
        if (!Enum.TryParse<PromotionType>(dto.Type, out var promotionType))
            throw new ArgumentException($"Invalid promotion type: {dto.Type}");

        return new Promotion
        {
            Id = dto.Id,
            Name = dto.Name,
            Description = dto.Description,
            Type = promotionType,
            DiscountValue = dto.DiscountValue,
            MaxDiscountAmount = dto.MaxDiscountAmount,
            StartDate = dto.StartDate,
            EndDate = dto.EndDate,
            IsActive = dto.IsActive,
            Priority = dto.Priority,
            CreatedAt = dto.CreatedAt,
            UpdatedAt = dto.UpdatedAt
        };
    }
} 