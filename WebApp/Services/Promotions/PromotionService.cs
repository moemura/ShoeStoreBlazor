using Microsoft.EntityFrameworkCore;
using WebApp.Services.Catches;

namespace WebApp.Services.Promotions;

public class PromotionService : IPromotionService
{
    private readonly IDbContextFactory<ShoeStoreDbContext> _dbContextFactory;
    private readonly ICacheService _cacheService;
    private const string CACHE_PREFIX = "Promotion_";

    public PromotionService(IDbContextFactory<ShoeStoreDbContext> dbContextFactory, ICacheService cacheService)
    {
        _dbContextFactory = dbContextFactory;
        _cacheService = cacheService;
    }

    public async Task<PromotionDto?> GetByIdAsync(string id)
    {
        var cacheKey = $"{CACHE_PREFIX}Id_{id}";
        return await _cacheService.GetOrSetAsync(cacheKey, async () =>
        {
            using var dbContext = await _dbContextFactory.CreateDbContextAsync();
            var promotion = await dbContext.Promotions
                .Include(p => p.PromotionProducts)
                .Include(p => p.PromotionCategories)
                .Include(p => p.PromotionBrands)
                .FirstOrDefaultAsync(p => p.Id == id);
            
            return promotion?.ToDto();
        });
    }

    public async Task<IEnumerable<PromotionDto>> GetAllAsync()
    {
        var cacheKey = $"{CACHE_PREFIX}All";
        return await _cacheService.GetOrSetAsync(cacheKey, async () =>
        {
            using var dbContext = await _dbContextFactory.CreateDbContextAsync();
            var promotions = await dbContext.Promotions
                .Include(p => p.PromotionProducts)
                .Include(p => p.PromotionCategories)
                .Include(p => p.PromotionBrands)
                .OrderByDescending(p => p.Priority)
                .ThenByDescending(p => p.CreatedAt)
                .ToListAsync();
            
            return promotions.Select(p => p.ToDto());
        });
    }

    public async Task<IEnumerable<PromotionDto>> GetActivePromotionsAsync()
    {
        var cacheKey = $"{CACHE_PREFIX}Active";
        return await _cacheService.GetOrSetAsync(cacheKey, async () =>
        {
            using var dbContext = await _dbContextFactory.CreateDbContextAsync();
            var now = DateTime.UtcNow;
            var promotions = await dbContext.Promotions
                .Include(p => p.PromotionProducts)
                .Include(p => p.PromotionCategories)
                .Include(p => p.PromotionBrands)
                .Where(p => p.IsActive && p.StartDate <= now && p.EndDate >= now)
                .OrderByDescending(p => p.Priority)
                .ToListAsync();
            
            return promotions.Select(p => p.ToDto());
        });
    }

    public async Task<IEnumerable<PromotionDto>> GetPromotionsForProductAsync(string productId)
    {
        var cacheKey = $"{CACHE_PREFIX}Product_{productId}";
        return await _cacheService.GetOrSetAsync(cacheKey, async () =>
        {
            using var dbContext = await _dbContextFactory.CreateDbContextAsync();
            var now = DateTime.UtcNow;
            
            // Get product to find category and brand
            var product = await dbContext.Products
                .FirstOrDefaultAsync(p => p.Id == productId);
            
            if (product == null)
                return Enumerable.Empty<PromotionDto>();

            var promotions = await dbContext.Promotions
                .Include(p => p.PromotionProducts)
                .Include(p => p.PromotionCategories)
                .Include(p => p.PromotionBrands)
                .Where(p => p.IsActive && p.StartDate <= now && p.EndDate >= now &&
                           (p.PromotionProducts!.Any(pp => pp.ProductId == productId) ||
                            (product.CategoryId != null && p.PromotionCategories!.Any(pc => pc.CategoryId == product.CategoryId)) ||
                            (product.BrandId != null && p.PromotionBrands!.Any(pb => pb.BrandId == product.BrandId))))
                .OrderByDescending(p => p.Priority)
                .ToListAsync();
            
            return promotions.Select(p => p.ToDto());
        });
    }

    public async Task<IEnumerable<PromotionDto>> GetPromotionsForCategoryAsync(string categoryId)
    {
        var cacheKey = $"{CACHE_PREFIX}Category_{categoryId}";
        return await _cacheService.GetOrSetAsync(cacheKey, async () =>
        {
            using var dbContext = await _dbContextFactory.CreateDbContextAsync();
            var now = DateTime.UtcNow;
            
            var promotions = await dbContext.Promotions
                .Include(p => p.PromotionProducts)
                .Include(p => p.PromotionCategories)
                .Include(p => p.PromotionBrands)
                .Where(p => p.IsActive && p.StartDate <= now && p.EndDate >= now &&
                           p.PromotionCategories!.Any(pc => pc.CategoryId == categoryId))
                .OrderByDescending(p => p.Priority)
                .ToListAsync();
            
            return promotions.Select(p => p.ToDto());
        });
    }

    public async Task<IEnumerable<PromotionDto>> GetPromotionsForBrandAsync(string brandId)
    {
        var cacheKey = $"{CACHE_PREFIX}Brand_{brandId}";
        return await _cacheService.GetOrSetAsync(cacheKey, async () =>
        {
            using var dbContext = await _dbContextFactory.CreateDbContextAsync();
            var now = DateTime.UtcNow;
            
            var promotions = await dbContext.Promotions
                .Include(p => p.PromotionProducts)
                .Include(p => p.PromotionCategories)
                .Include(p => p.PromotionBrands)
                .Where(p => p.IsActive && p.StartDate <= now && p.EndDate >= now &&
                           p.PromotionBrands!.Any(pb => pb.BrandId == brandId))
                .OrderByDescending(p => p.Priority)
                .ToListAsync();
            
            return promotions.Select(p => p.ToDto());
        });
    }

    public async Task<double> CalculatePromotionPriceAsync(string productId, double originalPrice)
    {
        var bestPromotion = await GetBestPromotionForProductAsync(productId);
        if (bestPromotion == null)
            return originalPrice;

        if (!Enum.TryParse<PromotionType>(bestPromotion.Type, out var promotionType))
            return originalPrice;

        return PromotionCalculator.CalculateDiscountedPrice(
            originalPrice, 
            promotionType, 
            bestPromotion.DiscountValue, 
            bestPromotion.MaxDiscountAmount);
    }

    public async Task<PromotionDto?> GetBestPromotionForProductAsync(string productId)
    {
        var cacheKey = $"{CACHE_PREFIX}BestFor_{productId}";
        return await _cacheService.GetOrSetAsync(cacheKey, async () =>
        {
            var promotions = await GetPromotionsForProductAsync(productId);
            
            if (!promotions.Any())
                return null;

            // Convert DTOs back to entities for calculation (or we could calculate directly from DTOs)
            var bestPromotionDto = promotions
                .OrderByDescending(p => p.Priority)
                .ThenByDescending(p => 
                {
                    if (Enum.TryParse<PromotionType>(p.Type, out var type))
                    {
                        return PromotionCalculator.CalculateDiscount(100, type, p.DiscountValue, p.MaxDiscountAmount); // Use 100 as reference price
                    }
                    return 0;
                })
                .FirstOrDefault();

            return bestPromotionDto;
        });
    }

    public async Task RemovePromotionCache()
    {
        await _cacheService.RemoveByPrefixAsync(CACHE_PREFIX);
    }

    public async Task<PromotionDto> CreateAsync(CreatePromotionRequest request)
    {
        using var context = await _dbContextFactory.CreateDbContextAsync();
        
        var promotion = new Promotion
        {
            Id = Guid.NewGuid().ToString(),
            Name = request.Name,
            Description = request.Description,
            Type = Enum.Parse<PromotionType>(request.Type),
            DiscountValue = request.DiscountValue,
            MaxDiscountAmount = request.MaxDiscountAmount,
            StartDate = request.StartDate,
            EndDate = request.EndDate,
            Priority = request.Priority,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        context.Promotions.Add(promotion);
        await context.SaveChangesAsync();

        // Add junction table entries
        await AddJunctionEntries(context, promotion.Id, request.ProductIds, request.CategoryIds, request.BrandIds);
        
        await RemovePromotionCache();
        return promotion.ToDto();
    }

    public async Task<PromotionDto> UpdateAsync(string id, CreatePromotionRequest request)
    {
        using var context = await _dbContextFactory.CreateDbContextAsync();
        
        var promotion = await context.Promotions.FindAsync(id);
        if (promotion == null)
            throw new ArgumentException("Promotion not found");

        promotion.Name = request.Name;
        promotion.Description = request.Description;
        promotion.Type = Enum.Parse<PromotionType>(request.Type);
        promotion.DiscountValue = request.DiscountValue;
        promotion.MaxDiscountAmount = request.MaxDiscountAmount;
        promotion.StartDate = request.StartDate;
        promotion.EndDate = request.EndDate;
        promotion.Priority = request.Priority;
        promotion.UpdatedAt = DateTime.UtcNow;

        // Remove existing junction entries
        await RemoveJunctionEntries(context, id);
        
        // Add new junction entries
        await AddJunctionEntries(context, id, request.ProductIds, request.CategoryIds, request.BrandIds);
        
        await context.SaveChangesAsync();
        await RemovePromotionCache();
        
        return promotion.ToDto();
    }

    public async Task<bool> DeleteAsync(string id)
    {
        using var context = await _dbContextFactory.CreateDbContextAsync();
        
        var promotion = await context.Promotions.FindAsync(id);
        if (promotion == null)
            return false;

        // Remove junction entries first
        await RemoveJunctionEntries(context, id);
        
        context.Promotions.Remove(promotion);
        await context.SaveChangesAsync();
        await RemovePromotionCache();
        
        return true;
    }

    public async Task<bool> ToggleActiveStatusAsync(string id, bool isActive)
    {
        using var context = await _dbContextFactory.CreateDbContextAsync();
        
        var promotion = await context.Promotions.FindAsync(id);
        if (promotion == null)
            return false;

        promotion.IsActive = isActive;
        promotion.UpdatedAt = DateTime.UtcNow;
        
        await context.SaveChangesAsync();
        await RemovePromotionCache();
        
        return true;
    }

    private async Task AddJunctionEntries(ShoeStoreDbContext context, string promotionId, 
        List<string> productIds, List<string> categoryIds, List<string> brandIds)
    {
        // Add product entries
        foreach (var productId in productIds)
        {
            context.PromotionProducts.Add(new PromotionProduct
            {
                Id = Guid.NewGuid().ToString(),
                PromotionId = promotionId,
                ProductId = productId,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            });
        }

        // Add category entries
        foreach (var categoryId in categoryIds)
        {
            context.PromotionCategories.Add(new PromotionCategory
            {
                Id = Guid.NewGuid().ToString(),
                PromotionId = promotionId,
                CategoryId = categoryId,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            });
        }

        // Add brand entries
        foreach (var brandId in brandIds)
        {
            context.PromotionBrands.Add(new PromotionBrand
            {
                Id = Guid.NewGuid().ToString(),
                PromotionId = promotionId,
                BrandId = brandId,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            });
        }
    }

    private async Task RemoveJunctionEntries(ShoeStoreDbContext context, string promotionId)
    {
        var productEntries = context.PromotionProducts.Where(pp => pp.PromotionId == promotionId);
        var categoryEntries = context.PromotionCategories.Where(pc => pc.PromotionId == promotionId);
        var brandEntries = context.PromotionBrands.Where(pb => pb.PromotionId == promotionId);

        context.PromotionProducts.RemoveRange(productEntries);
        context.PromotionCategories.RemoveRange(categoryEntries);
        context.PromotionBrands.RemoveRange(brandEntries);
    }
} 