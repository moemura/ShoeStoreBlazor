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
                .OrderBy(p => p.Priority) // Priority 1 is higher than Priority 2
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
                .OrderBy(p => p.Priority) // Priority 1 is higher than Priority 2
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
                .OrderBy(p => p.Priority) // Priority 1 is higher than Priority 2
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
                .OrderBy(p => p.Priority) // Priority 1 is higher than Priority 2
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
                .OrderBy(p => p.Priority) // Priority 1 is higher than Priority 2
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
            using var dbContext = await _dbContextFactory.CreateDbContextAsync();
            var now = DateTime.UtcNow;

            // Get product to find category and brand
            var product = await dbContext.Products
                .FirstOrDefaultAsync(p => p.Id == productId);

            if (product == null)
                return null;

            // Get all promotions that could apply to this product
            var promotions = await dbContext.Promotions
                .Include(p => p.PromotionProducts)
                .Include(p => p.PromotionCategories)
                .Include(p => p.PromotionBrands)
                .Where(p => p.IsActive && p.StartDate <= now && p.EndDate >= now &&
                           (p.Scope == PromotionScope.All ||
                            (p.Scope == PromotionScope.Product && p.PromotionProducts!.Any(pp => pp.ProductId == productId)) ||
                            (p.Scope == PromotionScope.Category && product.CategoryId != null && p.PromotionCategories!.Any(pc => pc.CategoryId == product.CategoryId)) ||
                            (p.Scope == PromotionScope.Brand && product.BrandId != null && p.PromotionBrands!.Any(pb => pb.BrandId == product.BrandId))))
                .OrderBy(p => p.Priority) // Priority 1 is higher than Priority 2
                .ThenByDescending(p => p.DiscountValue) // Order by discount value as secondary sort
                .FirstOrDefaultAsync();

            return promotions?.ToDto();
        });
    }

    public async Task<IEnumerable<PromotionDto>> GetValidPromotionsForOrderAsync(IEnumerable<string> productIds, double orderTotal)
    {
        var cacheKey = $"{CACHE_PREFIX}ValidForOrder_{string.Join("_", productIds.OrderBy(x => x))}_{orderTotal}";
        return await _cacheService.GetOrSetAsync(cacheKey, async () =>
        {
            using var dbContext = await _dbContextFactory.CreateDbContextAsync();
            var now = DateTime.UtcNow;

            // Get all products with their categories and brands
            var products = await dbContext.Products
                .Where(p => productIds.Contains(p.Id))
                .ToListAsync();

            var productCategoryIds = products.Where(p => p.CategoryId != null).Select(p => p.CategoryId!).Distinct().ToList();
            var productBrandIds = products.Where(p => p.BrandId != null).Select(p => p.BrandId!).Distinct().ToList();

            var promotions = await dbContext.Promotions
                .Include(p => p.PromotionProducts)
                .Include(p => p.PromotionCategories)
                .Include(p => p.PromotionBrands)
                .Where(p => p.IsActive && p.StartDate <= now && p.EndDate >= now &&
                           (!p.MinOrderAmount.HasValue || orderTotal >= p.MinOrderAmount.Value) &&
                           (p.Scope == PromotionScope.All ||
                            (p.Scope == PromotionScope.Product && p.PromotionProducts!.Any(pp => productIds.Contains(pp.ProductId))) ||
                            (p.Scope == PromotionScope.Category && productCategoryIds.Any(cId => p.PromotionCategories!.Any(pc => pc.CategoryId == cId))) ||
                            (p.Scope == PromotionScope.Brand && productBrandIds.Any(bId => p.PromotionBrands!.Any(pb => pb.BrandId == bId)))))
                .OrderBy(p => p.Priority) // Priority 1 is higher than Priority 2
                .ToListAsync();

            return promotions.Select(p => p.ToDto());
        });
    }

    public async Task<double> CalculatePromotionPriceWithOrderValidationAsync(string productId, double originalPrice, double orderTotal)
    {
        var bestPromotion = await GetBestPromotionForProductWithOrderValidationAsync(productId, orderTotal);
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

    public async Task<PromotionDto?> GetBestPromotionForProductWithOrderValidationAsync(string productId, double orderTotal)
    {
        var cacheKey = $"{CACHE_PREFIX}BestForWithOrderValidation_{productId}_{orderTotal}";
        return await _cacheService.GetOrSetAsync(cacheKey, async () =>
        {
            var validPromotions = await GetValidPromotionsForOrderAsync(new[] { productId }, orderTotal);
            
            // Get product's category and brand for category/brand promotions
            using var dbContext = await _dbContextFactory.CreateDbContextAsync();
            var product = await dbContext.Products.FirstOrDefaultAsync(p => p.Id == productId);

            // Filter promotions that apply to this specific product
            var applicablePromotions = validPromotions.Where(p =>
            {
                switch (p.Scope)
                {
                    case "All":
                        return true;
                    case "Product":
                        return p.ProductIds.Contains(productId);
                    case "Category":
                        return product?.CategoryId != null && p.CategoryIds.Contains(product.CategoryId);
                    case "Brand":
                        return product?.BrandId != null && p.BrandIds.Contains(product.BrandId);
                    default:
                        return false;
                }
            });

            if (!applicablePromotions.Any())
                return null;

            // Get the best promotion based on priority and discount amount
            var bestPromotionDto = applicablePromotions
                .OrderBy(p => p.Priority) // Priority 1 is higher than Priority 2
                .ThenByDescending(p =>
                {
                    if (Enum.TryParse<PromotionType>(p.Type, out var type))
                    {
                        return PromotionCalculator.CalculateDiscount(100, type, p.DiscountValue, p.MaxDiscountAmount);
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

        var promotion = request.ToEntity();
        var scope = (PromotionScope)promotion.Scope;

        switch (scope)
        {
            case PromotionScope.All:
                promotion.PromotionProducts = null;
                promotion.PromotionCategories = null;
                promotion.PromotionBrands = null;
                break;
            case PromotionScope.Product:
                promotion.PromotionCategories = null;
                promotion.PromotionBrands = null;
                if (request.ProductIds.Any())
                {
                    promotion.PromotionProducts = request.ProductIds.Select(pId => new PromotionProduct { Id = Guid.NewGuid().ToString(), ProductId = pId, PromotionId = promotion.Id }).ToList();
                }
                break;
            case PromotionScope.Category:
                promotion.PromotionProducts = null;
                promotion.PromotionBrands = null;
                if (request.CategoryIds.Any())
                {
                    promotion.PromotionCategories = request.CategoryIds.Select(cId => new PromotionCategory { Id = Guid.NewGuid().ToString(), CategoryId = cId, PromotionId = promotion.Id }).ToList();
                }
                break;
            case PromotionScope.Brand:
                promotion.PromotionProducts = null;
                promotion.PromotionCategories = null;
                if (request.BrandIds.Any())
                {
                    promotion.PromotionBrands = request.BrandIds.Select(bId => new PromotionBrand { Id = Guid.NewGuid().ToString(), BrandId = bId, PromotionId = promotion.Id }).ToList();
                }
                break;
        }

        context.Promotions.Add(promotion);
        try
        {
            await context.SaveChangesAsync();
        }
        catch (Exception e)
        {
            string er = e.Message;
            string tr = e.StackTrace;
            throw;
        }

        await RemovePromotionCache();

        // Refetch to get related data
        var result = await GetByIdAsync(promotion.Id);
        return result!;
    }

    public async Task<PromotionDto> UpdateAsync(string id, CreatePromotionRequest request)
    {
        using var context = await _dbContextFactory.CreateDbContextAsync();

        var promotion = await context.Promotions
            .Include(p => p.PromotionProducts)
            .Include(p => p.PromotionCategories)
            .Include(p => p.PromotionBrands)
            .FirstOrDefaultAsync(p => p.Id == id);

        if (promotion == null)
            throw new ArgumentException("Promotion not found");

        var originalScope = promotion.Scope;

        // Update main properties
        request.UpdateEntity(promotion);
        promotion.UpdatedAt = DateTime.UtcNow;

        var newScope = promotion.Scope;

        // Logic to update junction tables based on scope
        if (newScope != PromotionScope.Product) promotion.PromotionProducts?.Clear();
        if (newScope != PromotionScope.Category) promotion.PromotionCategories?.Clear();
        if (newScope != PromotionScope.Brand) promotion.PromotionBrands?.Clear();

        switch (newScope)
        {
            case PromotionScope.Product:
                promotion.PromotionProducts ??= new List<PromotionProduct>();
                var existingProductIds = promotion.PromotionProducts.Select(p => p.ProductId).ToHashSet();
                var productsToAdd = request.ProductIds.Where(id => !existingProductIds.Contains(id));
                foreach (var pId in productsToAdd)
                {
                    promotion.PromotionProducts.Add(new PromotionProduct { Id = Guid.NewGuid().ToString(), ProductId = pId });
                }
                break;
            case PromotionScope.Category:
                promotion.PromotionCategories ??= new List<PromotionCategory>();
                var existingCategoryIds = promotion.PromotionCategories.Select(p => p.CategoryId).ToHashSet();
                var categoriesToAdd = request.CategoryIds.Where(id => !existingCategoryIds.Contains(id));
                foreach (var cId in categoriesToAdd)
                {
                    promotion.PromotionCategories.Add(new PromotionCategory { Id = Guid.NewGuid().ToString(), CategoryId = cId });
                }
                break;
            case PromotionScope.Brand:
                promotion.PromotionBrands ??= new List<PromotionBrand>();
                var existingBrandIds = promotion.PromotionBrands.Select(p => p.BrandId).ToHashSet();
                var brandsToAdd = request.BrandIds.Where(id => !existingBrandIds.Contains(id));
                foreach (var bId in brandsToAdd)
                {
                    promotion.PromotionBrands.Add(new PromotionBrand { Id = Guid.NewGuid().ToString(), BrandId = bId });
                }
                break;
        }

        context.Promotions.Update(promotion);
        await context.SaveChangesAsync();
        await RemovePromotionCache();

        var result = await GetByIdAsync(id);
        return result!;
    }

    public async Task<bool> DeleteAsync(string id)
    {
        using var context = await _dbContextFactory.CreateDbContextAsync();

        var promotion = await context.Promotions.FindAsync(id);
        if (promotion == null)
            return false;

        context.Promotions.Remove(promotion);
        await context.SaveChangesAsync();
        await RemovePromotionCache();

        return true;
    }

    public async Task<bool> ToggleActiveStatusAsync(string id, bool isActive)
    {
        using var context = await _dbContextFactory.CreateDbContextAsync();
        var promotion = await context.Promotions.FindAsync(id);
        if (promotion == null) return false;

        promotion.IsActive = isActive;
        promotion.UpdatedAt = DateTime.UtcNow;
        await context.SaveChangesAsync();
        await RemovePromotionCache();
        return true;
    }

    // Batch processing methods for better performance
    public async Task<Dictionary<string, double>> CalculatePromotionPricesForProductsAsync(IEnumerable<string> productIds, IEnumerable<double> originalPrices, double? orderTotal = null)
    {
        var productIdList = productIds.ToList();
        var originalPriceList = originalPrices.ToList();
        
        if (productIdList.Count != originalPriceList.Count)
            throw new ArgumentException("ProductIds and OriginalPrices must have the same count");
            
        var cacheKey = orderTotal.HasValue 
            ? $"{CACHE_PREFIX}BatchCalc_{string.Join("_", productIdList.OrderBy(x => x))}_{orderTotal.Value}"
            : $"{CACHE_PREFIX}BatchCalc_{string.Join("_", productIdList.OrderBy(x => x))}";
            
        return await _cacheService.GetOrSetAsync(cacheKey, async () =>
        {
            var bestPromotions = await GetBestPromotionsForProductsAsync(productIdList, orderTotal);
            var result = new Dictionary<string, double>();
            
            for (int i = 0; i < productIdList.Count; i++)
            {
                var productId = productIdList[i];
                var originalPrice = originalPriceList[i];
                var bestPromotion = bestPromotions.GetValueOrDefault(productId);
                
                if (bestPromotion != null && Enum.TryParse<PromotionType>(bestPromotion.Type, out var promotionType))
                {
                    var discountedPrice = PromotionCalculator.CalculateDiscountedPrice(
                        originalPrice,
                        promotionType,
                        bestPromotion.DiscountValue,
                        bestPromotion.MaxDiscountAmount);
                    result[productId] = discountedPrice;
                }
                else
                {
                    result[productId] = originalPrice;
                }
            }
            
            return result;
        });
    }

    public async Task<Dictionary<string, PromotionDto?>> GetBestPromotionsForProductsAsync(IEnumerable<string> productIds, double? orderTotal = null)
    {
        var productIdList = productIds.ToList();
        var cacheKey = orderTotal.HasValue 
            ? $"{CACHE_PREFIX}BatchBest_{string.Join("_", productIdList.OrderBy(x => x))}_{orderTotal.Value}"
            : $"{CACHE_PREFIX}BatchBest_{string.Join("_", productIdList.OrderBy(x => x))}";
            
        return await _cacheService.GetOrSetAsync(cacheKey, async () =>
        {
            using var dbContext = await _dbContextFactory.CreateDbContextAsync();
            var now = DateTime.UtcNow;

            // Get all products with their categories and brands in one query
            var products = await dbContext.Products
                .Where(p => productIdList.Contains(p.Id))
                .Select(p => new { p.Id, p.CategoryId, p.BrandId })
                .ToListAsync();

            // Get all active promotions that might apply to any of these products
            var allPromotions = await dbContext.Promotions
                .Include(p => p.PromotionProducts)
                .Include(p => p.PromotionCategories)
                .Include(p => p.PromotionBrands)
                .Where(p => p.IsActive && p.StartDate <= now && p.EndDate >= now)
                .ToListAsync();

            // Filter promotions by order total if provided
            if (orderTotal.HasValue)
            {
                allPromotions = allPromotions
                    .Where(p => p.MinOrderAmount == null || p.MinOrderAmount <= orderTotal.Value)
                    .ToList();
            }

            var result = new Dictionary<string, PromotionDto?>();

            foreach (var product in products)
            {
                // Find the best promotion for this specific product
                var applicablePromotions = allPromotions.Where(p =>
                    p.Scope == PromotionScope.All ||
                    (p.Scope == PromotionScope.Product && p.PromotionProducts!.Any(pp => pp.ProductId == product.Id)) ||
                    (p.Scope == PromotionScope.Category && product.CategoryId != null && p.PromotionCategories!.Any(pc => pc.CategoryId == product.CategoryId)) ||
                    (p.Scope == PromotionScope.Brand && product.BrandId != null && p.PromotionBrands!.Any(pb => pb.BrandId == product.BrandId))
                ).OrderBy(p => p.Priority) // Priority 1 is higher than Priority 2
                .ThenByDescending(p => p.DiscountValue) // Order by discount value as secondary sort
                .FirstOrDefault();

                result[product.Id] = applicablePromotions?.ToDto();
            }

            // Handle products that might not be found in database
            foreach (var productId in productIdList.Where(id => !result.ContainsKey(id)))
            {
                result[productId] = null;
            }

            return result;
        });
    }
}
