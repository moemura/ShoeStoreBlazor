using Microsoft.EntityFrameworkCore;
using WebApp.Services.Catches;
using WebApp.Services.Promotions;

namespace WebApp.Services.Products
{
    public class ProductService : IProductService
    {
        private readonly IDbContextFactory<ShoeStoreDbContext> _dbContextFactory;
        private readonly ICacheService _cacheService;
        private readonly IPromotionService _promotionService;
        private const string CACHE_PREFIX = "Product_";

        public ProductService(IDbContextFactory<ShoeStoreDbContext> dbContextFactory, ICacheService cacheService, IPromotionService promotionService)
        {
            _dbContextFactory = dbContextFactory;
            _cacheService = cacheService;
            _promotionService = promotionService;
        }

        public async Task<IEnumerable<ProductDto>> GetAll()
        {
            var cacheKey = $"{CACHE_PREFIX}All";
            return await _cacheService.GetOrSetAsync(cacheKey, async () =>
            {
                using var dbContext = await _dbContextFactory.CreateDbContextAsync();
                var data = await dbContext.Products
                    .Include(p => p.Category)
                    .Include(p => p.Brand)
                    .Include(p => p.Inventories)
                    .OrderBy(p => p.CreatedAt)
                    .ToListAsync();
                return data.Select(p => p.ToDto());
            });
        }

        public async Task<ProductDto> GetById(string Id)
        {
            var cacheKey = $"{CACHE_PREFIX}WithPromotion_Id_{Id}";
            return await _cacheService.GetOrSetAsync(cacheKey, async () =>
            {
                using var dbContext = await _dbContextFactory.CreateDbContextAsync();
                var product = await dbContext.Products
                    .Include(p => p.Category)
                    .Include(p => p.Brand)
                    .Include(p => p.Inventories)
                    .SingleOrDefaultAsync(p => p.Id == Id);
                
                if (product == null)
                    return null;

                var productDto = product.ToDto();
                
                // Calculate promotion price
                var promotionPrice = await _promotionService.CalculatePromotionPriceAsync(Id, productDto.Price);
                if (promotionPrice < productDto.Price)
                {
                    productDto.PromotionPrice = promotionPrice;
                    productDto.PromotionDiscount = productDto.Price - promotionPrice;
                    productDto.HasActivePromotion = true;
                    
                    var bestPromotion = await _promotionService.GetBestPromotionForProductAsync(Id);
                    productDto.PromotionName = bestPromotion?.Name;
                }
                
                return productDto;
            });
        }

        public async Task<ProductDto> Create(ProductDto dto)
        {
            if (dto == null)
                throw new Exception("Data must not null!");

            using var dbContext = await _dbContextFactory.CreateDbContextAsync();

            // Validate Category and Brand if provided
            if (!string.IsNullOrEmpty(dto.CategoryId))
            {
                var categoryExists = await dbContext.Categories.AnyAsync(c => c.Id == dto.CategoryId);
                if (!categoryExists)
                    throw new Exception("Category not found!");
            }

            if (!string.IsNullOrEmpty(dto.BrandId))
            {
                var brandExists = await dbContext.Brands.AnyAsync(b => b.Id == dto.BrandId);
                if (!brandExists)
                    throw new Exception("Brand not found!");
            }

            var product = dto.ToEntity();
            product.Id = Guid.CreateVersion7().ToString();
            product.CreatedAt = DateTime.UtcNow;
            product.UpdatedAt = DateTime.UtcNow;
            await dbContext.Products.AddAsync(product);
            await dbContext.SaveChangesAsync();

            await _cacheService.RemoveByPrefixAsync(CACHE_PREFIX);

            return await GetById(product.Id);
        }

        public async Task Update(ProductDto dto)
        {
            using var dbContext = await _dbContextFactory.CreateDbContextAsync();
            var product = await dbContext.Products.FirstOrDefaultAsync(p => p.Id == dto.Id)
                ?? throw new Exception("Product not found!");

            // Validate Category and Brand if provided
            if (!string.IsNullOrEmpty(dto.CategoryId))
            {
                var categoryExists = await dbContext.Categories.AnyAsync(c => c.Id == dto.CategoryId);
                if (!categoryExists)
                    throw new Exception("Category not found!");
            }

            if (!string.IsNullOrEmpty(dto.BrandId))
            {
                var brandExists = await dbContext.Brands.AnyAsync(b => b.Id == dto.BrandId);
                if (!brandExists)
                    throw new Exception("Brand not found!");
            }

            product.UpdatedAt = DateTime.UtcNow;
            product.Name = dto.Name;
            product.Description = dto.Description;
            product.Price = dto.Price;
            product.SalePrice = dto.SalePrice;
            product.MainImage = dto.MainImage;
            product.Image = dto.Images != null ? string.Join(",", dto.Images) : null;
            product.LikeCount = dto.LikeCount;
            product.CategoryId = dto.CategoryId;
            product.BrandId = dto.BrandId;

            dbContext.Update(product);
            await dbContext.SaveChangesAsync();

            await _cacheService.RemoveByPrefixAsync(CACHE_PREFIX);
        }

        public async Task Delete(string id)
        {
            using var dbContext = await _dbContextFactory.CreateDbContextAsync();
            var product = await dbContext.Products.FirstOrDefaultAsync(p => p.Id == id)
                ?? throw new Exception("Product not found!");
            dbContext.Products.Remove(product);
            await dbContext.SaveChangesAsync();

            await _cacheService.RemoveByPrefixAsync(CACHE_PREFIX);
        }

        public async Task<IEnumerable<ProductDto>> Filter(Dictionary<string, string> filter)
        {
            using var dbContext = await _dbContextFactory.CreateDbContextAsync();
            var query = dbContext.Products
                .Include(p => p.Category)
                .Include(p => p.Brand)
                .Include(p => p.Inventories)
                .OrderBy(p => p.CreatedAt)
                .AsQueryable();

            if (filter.ContainsKey("name") && !string.IsNullOrEmpty(filter["name"]))
            {
                query = query.Where(p => p.Name.Contains(filter["name"]));
            }

            if (filter.ContainsKey("minPrice") && double.TryParse(filter["minPrice"], out double minPrice))
            {
                query = query.Where(p => p.Price >= minPrice);
            }

            if (filter.ContainsKey("maxPrice") && double.TryParse(filter["maxPrice"], out double maxPrice))
            {
                query = query.Where(p => p.Price <= maxPrice);
            }

            if (filter.ContainsKey("categoryId") && !string.IsNullOrEmpty(filter["categoryId"]))
            {
                query = query.Where(p => p.CategoryId == filter["categoryId"]);
            }

            if (filter.ContainsKey("brandId") && !string.IsNullOrEmpty(filter["brandId"]))
            {
                query = query.Where(p => p.BrandId == filter["brandId"]);
            }

            var products = await query.ToListAsync();
            return products.Select(p => p.ToDto());
        }

        public async Task<PaginatedList<ProductDto>> FilterAndPagin(int pageIndex, int pageSize, Dictionary<string, string> filter)
        {
            var orderedFilter = filter.OrderBy(kv => kv.Key);
            var filterKey = string.Join("_", orderedFilter.Select(f => $"{f.Key}_{f.Value}"));
            var cacheKey = $"{CACHE_PREFIX}Filter_{filterKey}_Page_{pageIndex}_Size_{pageSize}";

            var data = await _cacheService.GetOrSetAsync(cacheKey, async () =>
            {
                using var dbContext = await _dbContextFactory.CreateDbContextAsync();

                // Bước 1: Truy vấn lọc và phân trang chỉ lấy Id
                var query = dbContext.Products.AsNoTracking().AsQueryable();

                if (filter.TryGetValue("name", out var name) && !string.IsNullOrWhiteSpace(name))
                    query = query.Where(p => p.Name.Contains(name));

                if (filter.TryGetValue("minPrice", out var minPriceStr) && double.TryParse(minPriceStr, out double minPrice))
                    query = query.Where(p => p.Price >= minPrice);

                if (filter.TryGetValue("maxPrice", out var maxPriceStr) && double.TryParse(maxPriceStr, out double maxPrice))
                    query = query.Where(p => p.Price <= maxPrice);

                if (filter.TryGetValue("categoryId", out var categoryId) && !string.IsNullOrWhiteSpace(categoryId))
                    query = query.Where(p => p.CategoryId == categoryId);

                if (filter.TryGetValue("brandId", out var brandId) && !string.IsNullOrWhiteSpace(brandId))
                    query = query.Where(p => p.BrandId == brandId);

                var totalItems = await query.CountAsync();

                var productIds = await query
                    .OrderBy(p => p.CreatedAt)
                    .Skip((pageIndex - 1) * pageSize)
                    .Take(pageSize)
                    .Select(p => p.Id)
                    .ToListAsync();

                // Bước 2: Truy vấn chi tiết các bản ghi đã phân trang
                var products = await dbContext.Products
                    .AsNoTracking()
                    .Where(p => productIds.Contains(p.Id))
                    .Include(p => p.Category)
                    .Include(p => p.Brand)
                    .Include(p => p.Inventories)
                    .OrderBy(p => p.CreatedAt)
                    .ToListAsync();

                // Đảm bảo giữ đúng thứ tự theo productIds
                var orderedProducts = productIds.Select(id => products.First(p => p.Id == id)).ToList();

                var pageCount = (int)Math.Ceiling(totalItems / (double)pageSize);

                var result = new PaginatedList<ProductDto>(
                    orderedProducts.Select(p => p.ToDto()),
                    pageIndex,
                    pageSize,
                    totalItems
                );
                return result;
            });
            return data;
        }

        public async Task<PaginatedList<ProductDto>> GetPagination(int pageIndex, int pageSize)
        {
            var cacheKey = $"{CACHE_PREFIX}Page_{pageIndex}_Size_{pageSize}";
            return await _cacheService.GetOrSetAsync(cacheKey, async () =>
            {
                using var dbContext = await _dbContextFactory.CreateDbContextAsync();

                // Bước 1: Lấy tổng số sản phẩm
                var totalItems = await dbContext.Products.CountAsync();

                // Bước 2: Lấy danh sách Id theo phân trang
                var productIds = await dbContext.Products
                    .OrderBy(p => p.CreatedAt)
                    .Skip((pageIndex - 1) * pageSize)
                    .Take(pageSize)
                    .Select(p => p.Id)
                    .ToListAsync();

                // Bước 3: Truy vấn chi tiết sản phẩm với Include theo Id
                var products = await dbContext.Products
                    .Where(p => productIds.Contains(p.Id))
                    .Include(p => p.Category)
                    .Include(p => p.Brand)
                    .Include(p => p.Inventories)
                    .OrderBy(p => p.CreatedAt)
                    .ToListAsync();

                return new PaginatedList<ProductDto>(
                    products.Select(p => p.ToDto()),
                    pageIndex,
                    pageSize,
                    totalItems
                );
            });
        }

        public async Task Stock(string productId, IEnumerable<InventoryDto> inventories)
        {
            using var dbContext = await _dbContextFactory.CreateDbContextAsync();

            // Get product with existing inventories using EagerLoading
            var product = await dbContext.Products
                .Include(p => p.Inventories)
                .FirstOrDefaultAsync(p => p.Id == productId)
                ?? throw new Exception("Product not found!");

            // Get existing inventories for the product
            var existingInventories = product.Inventories.ToDictionary(i => i.SizeId);

            foreach (var inventoryDto in inventories)
            {
                if (existingInventories.TryGetValue(inventoryDto.SizeId, out var existingInventory))
                {
                    // Update existing inventory
                    existingInventory.Quantity = inventoryDto.Quantity;
                }
                else
                {
                    // Create new inventory
                    var newInventory = new Inventory
                    {
                        ProductId = productId,
                        SizeId = inventoryDto.SizeId,
                        Quantity = inventoryDto.Quantity
                    };
                    dbContext.Inventories.Add(newInventory);
                }
            }

            await dbContext.SaveChangesAsync();
            await _cacheService.RemoveByPrefixAsync(CACHE_PREFIX);
        }

        public async Task<Inventory> CheckInventory(int inventoryId, int quantity)
        {
            using var dbContext = await _dbContextFactory.CreateDbContextAsync();
            var inventory = await dbContext.Inventories
                .Include(i => i.Product)
                .ThenInclude(p => p.Brand)
                .FirstOrDefaultAsync(i => i.Id == inventoryId);
            if (inventory == null || inventory.Quantity < quantity)
                return null;
            return inventory;
        }

        public Task RemoveProductCache() => _cacheService.RemoveByPrefixAsync(CACHE_PREFIX);

        // New methods for dynamic pricing
        public async Task<ProductDto> GetByIdWithDynamicPricing(string id, double? orderTotal = null)
        {
            var cacheKey = orderTotal.HasValue 
                ? $"{CACHE_PREFIX}DynamicPricing_Id_{id}_OrderTotal_{orderTotal.Value}"
                : $"{CACHE_PREFIX}DynamicPricing_Id_{id}";
                
            return await _cacheService.GetOrSetAsync(cacheKey, async () =>
            {
                using var dbContext = await _dbContextFactory.CreateDbContextAsync();
                var product = await dbContext.Products
                    .Include(p => p.Category)
                    .Include(p => p.Brand)
                    .Include(p => p.Inventories)
                    .SingleOrDefaultAsync(p => p.Id == id);
                
                if (product == null)
                    return null;

                var productDto = product.ToDto();
                
                // Calculate promotion price with order validation if orderTotal is provided
                if (orderTotal.HasValue)
                {
                    var promotionPrice = await _promotionService.CalculatePromotionPriceWithOrderValidationAsync(
                        id, productDto.Price, orderTotal.Value);
                    
                    if (promotionPrice < productDto.Price)
                    {
                        productDto.PromotionPrice = promotionPrice;
                        productDto.PromotionDiscount = productDto.Price - promotionPrice;
                        productDto.HasActivePromotion = true;
                        
                        var bestPromotion = await _promotionService.GetBestPromotionForProductWithOrderValidationAsync(id, orderTotal.Value);
                        productDto.PromotionName = bestPromotion?.Name;
                    }
                }
                else
                {
                    // Calculate promotion price without order validation
                    var promotionPrice = await _promotionService.CalculatePromotionPriceAsync(id, productDto.Price);
                    if (promotionPrice < productDto.Price)
                    {
                        productDto.PromotionPrice = promotionPrice;
                        productDto.PromotionDiscount = productDto.Price - promotionPrice;
                        productDto.HasActivePromotion = true;
                        
                        var bestPromotion = await _promotionService.GetBestPromotionForProductAsync(id);
                        productDto.PromotionName = bestPromotion?.Name;
                    }
                }
                
                return productDto;
            });
        }

        public async Task<IEnumerable<ProductDto>> GetAllWithDynamicPricing(double? orderTotal = null)
        {
            var cacheKey = orderTotal.HasValue 
                ? $"{CACHE_PREFIX}AllDynamicPricing_OrderTotal_{orderTotal.Value}"
                : $"{CACHE_PREFIX}AllDynamicPricing";
                
            return await _cacheService.GetOrSetAsync(cacheKey, async () =>
            {
                using var dbContext = await _dbContextFactory.CreateDbContextAsync();
                var data = await dbContext.Products
                    .Include(p => p.Category)
                    .Include(p => p.Brand)
                    .Include(p => p.Inventories)
                    .OrderBy(p => p.CreatedAt)
                    .ToListAsync();
                
                var products = data.Select(p => p.ToDto()).ToList();
                return await ApplyDynamicPromotionsToProducts(products, orderTotal);
            });
        }

        public async Task<PaginatedList<ProductDto>> GetPaginationWithDynamicPricing(int pageIndex, int pageSize, double? orderTotal = null)
        {
            var cacheKey = orderTotal.HasValue 
                ? $"{CACHE_PREFIX}DynamicPricingPage_{pageIndex}_Size_{pageSize}_OrderTotal_{orderTotal.Value}"
                : $"{CACHE_PREFIX}DynamicPricingPage_{pageIndex}_Size_{pageSize}";
                
            return await _cacheService.GetOrSetAsync(cacheKey, async () =>
            {
                using var dbContext = await _dbContextFactory.CreateDbContextAsync();

                var totalItems = await dbContext.Products.CountAsync();

                var productIds = await dbContext.Products
                    .OrderBy(p => p.CreatedAt)
                    .Skip((pageIndex - 1) * pageSize)
                    .Take(pageSize)
                    .Select(p => p.Id)
                    .ToListAsync();

                var products = await dbContext.Products
                    .Where(p => productIds.Contains(p.Id))
                    .Include(p => p.Category)
                    .Include(p => p.Brand)
                    .Include(p => p.Inventories)
                    .OrderBy(p => p.CreatedAt)
                    .ToListAsync();

                var productDtos = products.Select(p => p.ToDto()).ToList();
                var productsWithPromotions = await ApplyDynamicPromotionsToProducts(productDtos, orderTotal);

                return new PaginatedList<ProductDto>(
                    productsWithPromotions,
                    pageIndex,
                    pageSize,
                    totalItems
                );
            });
        }

        private async Task<IEnumerable<ProductDto>> ApplyDynamicPromotionsToProducts(IEnumerable<ProductDto> products, double? orderTotal = null)
        {
            var productList = products.ToList();
            if (!productList.Any()) return productList;

            // Extract product IDs and prices for batch processing
            var productIds = productList.Select(p => p.Id).ToList();
            var originalPrices = productList.Select(p => p.Price).ToList();

            // Use batch processing to get promotion prices and best promotions
            var promotionPrices = await _promotionService.CalculatePromotionPricesForProductsAsync(productIds, originalPrices, orderTotal);
            var bestPromotions = await _promotionService.GetBestPromotionsForProductsAsync(productIds, orderTotal);

            // Apply results to products
            foreach (var product in productList)
            {
                if (promotionPrices.TryGetValue(product.Id, out var promotionPrice) && promotionPrice < product.Price)
                {
                    product.PromotionPrice = promotionPrice;
                    product.PromotionDiscount = product.Price - promotionPrice;
                    product.HasActivePromotion = true;
                    
                    if (bestPromotions.TryGetValue(product.Id, out var bestPromotion) && bestPromotion != null)
                    {
                        product.PromotionName = bestPromotion.Name;
                    }
                }
            }

            return productList;
        }

        public async Task<PaginatedList<ProductDto>> FilterAndPaginWithDynamicPricing(int pageIndex, int pageSize, Dictionary<string, string> filter, double orderTotal)
        {
            var orderedFilter = filter.OrderBy(kv => kv.Key);
            var filterKey = string.Join("_", orderedFilter.Select(f => $"{f.Key}_{f.Value}"));
            var cacheKey = $"{CACHE_PREFIX}FilterDynamicPricing_{filterKey}_Page_{pageIndex}_Size_{pageSize}_OrderTotal_{orderTotal}";

            return await _cacheService.GetOrSetAsync(cacheKey, async () =>
            {
                using var dbContext = await _dbContextFactory.CreateDbContextAsync();

                // Build query with filters
                var query = dbContext.Products.AsNoTracking().AsQueryable();

                if (filter.TryGetValue("search", out var name) && !string.IsNullOrWhiteSpace(name))
                    query = query.Where(p => p.Name.Contains(name));

                if (filter.TryGetValue("minPrice", out var minPriceStr) && double.TryParse(minPriceStr, out double minPrice))
                    query = query.Where(p => p.Price >= minPrice);

                if (filter.TryGetValue("maxPrice", out var maxPriceStr) && double.TryParse(maxPriceStr, out double maxPrice))
                    query = query.Where(p => p.Price <= maxPrice);

                if (filter.TryGetValue("categoryId", out var categoryId) && !string.IsNullOrWhiteSpace(categoryId))
                    query = query.Where(p => p.CategoryId == categoryId);

                if (filter.TryGetValue("brandId", out var brandId) && !string.IsNullOrWhiteSpace(brandId))
                    query = query.Where(p => p.BrandId == brandId);

                var totalItems = await query.CountAsync();

                var productIds = await query
                    .OrderBy(p => p.CreatedAt)
                    .Skip((pageIndex - 1) * pageSize)
                    .Take(pageSize)
                    .Select(p => p.Id)
                    .ToListAsync();

                // Get products with includes
                var products = await dbContext.Products
                    .AsNoTracking()
                    .Where(p => productIds.Contains(p.Id))
                    .Include(p => p.Category)
                    .Include(p => p.Brand)
                    .Include(p => p.Inventories)
                    .OrderBy(p => p.CreatedAt)
                    .ToListAsync();

                // Ensure correct order
                var orderedProducts = productIds.Select(id => products.First(p => p.Id == id)).ToList();
                var productDtos = orderedProducts.Select(p => p.ToDto()).ToList();
                
                // Apply dynamic promotions with batch processing
                var productsWithPromotions = await ApplyDynamicPromotionsToProducts(productDtos, orderTotal);

                return new PaginatedList<ProductDto>(
                    productsWithPromotions,
                    pageIndex,
                    pageSize,
                    totalItems
                );
            });
        }
    }
}
