# Kế hoạch Chức năng Quản lý Chương trình Giảm giá Sản phẩm

## 1. Phân tích Codebase hiện tại

### Cấu trúc hiện có:
- **Product Entity**: Đã có `Price` và `SalePrice` (double)
- **Voucher System**: Đã implement voucher cho đơn hàng
- **Database Context**: Sử dụng EF Core với migration pattern
- **Service Pattern**: Đã có các service như ProductService, VoucherService
- **Caching**: Đã implement ICacheService với Redis/Memory cache
- **BaseEntity**: Có pattern chung cho các entity

### Điểm mạnh có thể tận dụng:
- Architecture pattern đã thiết lập tốt
- Caching strategy đã có
- Migration pattern đã hoạt động
- Service injection đã cấu hình

## 2. Thiết kế Entity Model

### 2.1 Promotion Entity
```csharp
public class Promotion : BaseEntity<string>
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public PromotionType Type { get; set; }
    public double DiscountValue { get; set; }
    public double? MaxDiscountAmount { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public bool IsActive { get; set; } = true;
    public int Priority { get; set; } = 1;
    
    // Navigation properties
    public virtual ICollection<PromotionProduct>? PromotionProducts { get; set; }
    public virtual ICollection<PromotionCategory>? PromotionCategories { get; set; }
    public virtual ICollection<PromotionBrand>? PromotionBrands { get; set; }
}

public enum PromotionType
{
    Percentage = 1,      // Giảm theo %
    FixedAmount = 2,     // Giảm số tiền cố định
    BuyXGetY = 3        // Mua X tặng Y
}
```

### 2.2 Junction Tables (Many-to-Many relationships)
```csharp
public class PromotionProduct : BaseEntity<string>
{
    public string PromotionId { get; set; } = string.Empty;
    public string ProductId { get; set; } = string.Empty;
    
    public virtual Promotion? Promotion { get; set; }
    public virtual Product? Product { get; set; }
}

public class PromotionCategory : BaseEntity<string>
{
    public string PromotionId { get; set; } = string.Empty;
    public string CategoryId { get; set; } = string.Empty;
    
    public virtual Promotion? Promotion { get; set; }
    public virtual Category? Category { get; set; }
}

public class PromotionBrand : BaseEntity<string>
{
    public string PromotionId { get; set; } = string.Empty;
    public string BrandId { get; set; } = string.Empty;
    
    public virtual Promotion? Promotion { get; set; }
    public virtual Brand? Brand { get; set; }
}
```

## 3. Database Migration

### 3.1 Migration Script
```sql
-- Add Promotions table
CREATE TABLE Promotions (
    Id NVARCHAR(450) PRIMARY KEY,
    Name NVARCHAR(200) NOT NULL,
    Description NVARCHAR(500),
    Type INT NOT NULL,
    DiscountValue FLOAT NOT NULL,
    MaxDiscountAmount FLOAT NULL,
    StartDate DATETIME2 NOT NULL,
    EndDate DATETIME2 NOT NULL,
    IsActive BIT NOT NULL DEFAULT 1,
    Priority INT NOT NULL DEFAULT 1,
    CreatedAt DATETIME2 NOT NULL,
    UpdatedAt DATETIME2 NOT NULL
);

-- Add junction tables
CREATE TABLE PromotionProducts (
    Id NVARCHAR(450) PRIMARY KEY,
    PromotionId NVARCHAR(450) NOT NULL,
    ProductId NVARCHAR(450) NOT NULL,
    CreatedAt DATETIME2 NOT NULL,
    UpdatedAt DATETIME2 NOT NULL,
    FOREIGN KEY (PromotionId) REFERENCES Promotions(Id) ON DELETE CASCADE,
    FOREIGN KEY (ProductId) REFERENCES Products(Id) ON DELETE CASCADE
);

-- Similar for PromotionCategories and PromotionBrands
```

### 3.2 Indexes cho Performance
```sql
-- Indexes for better query performance
CREATE INDEX IX_Promotions_StartDate_EndDate ON Promotions(StartDate, EndDate);
CREATE INDEX IX_Promotions_IsActive ON Promotions(IsActive);
CREATE INDEX IX_Promotions_Priority ON Promotions(Priority);
CREATE INDEX IX_PromotionProducts_ProductId ON PromotionProducts(ProductId);
CREATE INDEX IX_PromotionCategories_CategoryId ON PromotionCategories(CategoryId);
CREATE INDEX IX_PromotionBrands_BrandId ON PromotionBrands(BrandId);
```

## 4. DTOs và Mapping

### 4.1 PromotionDto
```csharp
public class PromotionDto
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public double DiscountValue { get; set; }
    public double? MaxDiscountAmount { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public bool IsActive { get; set; }
    public int Priority { get; set; }
    public List<string> ProductIds { get; set; } = new();
    public List<string> CategoryIds { get; set; } = new();
    public List<string> BrandIds { get; set; } = new();
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
```

### 4.2 Cập nhật ProductDto
```csharp
public class ProductDto
{
    // ... existing properties ...
    
    // New promotion-related properties
    public double? PromotionPrice { get; set; }
    public double? PromotionDiscount { get; set; }
    public string? PromotionName { get; set; }
    public bool HasActivePromotion { get; set; }
}
```

## 5. Service Layer

### 5.1 IPromotionService Interface
```csharp
public interface IPromotionService
{
    Task<PromotionDto> GetByIdAsync(string id);
    Task<IEnumerable<PromotionDto>> GetAllAsync();
    Task<IEnumerable<PromotionDto>> GetActivePromotionsAsync();
    Task<IEnumerable<PromotionDto>> GetPromotionsForProductAsync(string productId);
    Task<IEnumerable<PromotionDto>> GetPromotionsForCategoryAsync(string categoryId);
    Task<IEnumerable<PromotionDto>> GetPromotionsForBrandAsync(string brandId);
    Task<double> CalculatePromotionPriceAsync(string productId, double originalPrice);
    Task<PromotionDto?> GetBestPromotionForProductAsync(string productId);
}
```

### 5.2 PromotionService Implementation
```csharp
public class PromotionService : IPromotionService
{
    private readonly IDbContextFactory<ShoeStoreDbContext> _dbContextFactory;
    private readonly ICacheService _cacheService;
    private const string CACHE_PREFIX = "Promotion_";
    
    // Implementation với caching strategy tương tự ProductService
    // Logic tính toán giá khuyến mãi
    // Logic tìm promotion tốt nhất cho sản phẩm
}
```

## 6. Cập nhật ProductService

### 6.1 Tích hợp Promotion vào ProductService
```csharp
public class ProductService : IProductService
{
    private readonly IPromotionService _promotionService;
    
    // Cập nhật method GetById để include promotion info
    public async Task<ProductDto> GetById(string Id)
    {
        var product = await GetProductFromDb(Id);
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
    }
}
```

## 7. Caching Strategy

### 7.1 Cache Keys Structure
```csharp
// Promotion cache keys
"Promotion_All_Active"
"Promotion_Product_{productId}"
"Promotion_Category_{categoryId}"
"Promotion_Brand_{brandId}"
"Promotion_BestFor_{productId}"

// Updated product cache keys
"Product_WithPromotion_Id_{productId}"
"Product_WithPromotion_All"
```

### 7.2 Cache Invalidation
```csharp
// Khi promotion thay đổi, cần invalidate related caches
await _cacheService.RemoveByPrefixAsync("Promotion_");
await _cacheService.RemoveByPrefixAsync("Product_WithPromotion_");
```

## 8. Business Logic Rules

### 8.1 Promotion Calculation Logic
```csharp
public class PromotionCalculator
{
    public static double CalculateDiscountedPrice(double originalPrice, PromotionType type, double discountValue, double? maxDiscount = null)
    {
        switch (type)
        {
            case PromotionType.Percentage:
                var percentageDiscount = originalPrice * (discountValue / 100);
                if (maxDiscount.HasValue && percentageDiscount > maxDiscount.Value)
                    percentageDiscount = maxDiscount.Value;
                return originalPrice - percentageDiscount;
                
            case PromotionType.FixedAmount:
                var fixedDiscount = Math.Min(discountValue, originalPrice);
                return originalPrice - fixedDiscount;
                
            default:
                return originalPrice;
        }
    }
}
```

### 8.2 Priority Rules
- Priority cao hơn sẽ được áp dụng trước
- Chỉ áp dụng 1 promotion cho 1 sản phẩm tại 1 thời điểm
- Kiểm tra thời gian hiệu lực (StartDate <= Now <= EndDate)
- Kiểm tra IsActive = true

## 9. Database Context Updates

### 9.1 Cập nhật ShoeStoreDbContext
```csharp
public virtual DbSet<Promotion> Promotions { get; set; }
public virtual DbSet<PromotionProduct> PromotionProducts { get; set; }
public virtual DbSet<PromotionCategory> PromotionCategories { get; set; }
public virtual DbSet<PromotionBrand> PromotionBrands { get; set; }

protected override void OnModelCreating(ModelBuilder builder)
{
    // Existing configurations...
    
    // Promotion configurations
    builder.Entity<Promotion>()
        .HasKey(p => p.Id);
        
    builder.Entity<Promotion>()
        .Property(p => p.Name)
        .HasMaxLength(200)
        .IsRequired();
        
    // Junction table configurations
    builder.Entity<PromotionProduct>()
        .HasOne(pp => pp.Promotion)
        .WithMany(p => p.PromotionProducts)
        .HasForeignKey(pp => pp.PromotionId)
        .OnDelete(DeleteBehavior.Cascade);
        
    // Similar for other junction tables
}
```

## 10. Performance Considerations

### 10.1 Query Optimization
- Sử dụng eager loading khi cần thiết
- Implement pagination cho danh sách promotion
- Sử dụng AsNoTracking() cho read-only queries
- Index các trường tìm kiếm thường xuyên

### 10.2 Caching Strategy
- Cache active promotions trong memory
- Cache promotion results theo product/category/brand
- TTL cache phù hợp (15-30 phút)
- Background job để refresh cache

### 10.3 Background Jobs (Future Enhancement)
```csharp
// Scheduled job để activate/deactivate promotions theo thời gian
public class PromotionScheduleJob : IHostedService
{
    // Chạy mỗi phút để check và update promotion status
    // Invalidate cache khi có thay đổi
}
```

## 11. Testing Strategy

### 11.1 Unit Tests
- PromotionCalculator logic tests
- PromotionService method tests
- ProductService integration tests với promotion

### 11.2 Integration Tests
- Database operations
- Cache operations
- Service layer integration

### 11.3 Performance Tests
- Load testing với promotion calculations
- Cache performance tests
- Database query performance

## 12. Migration và Deployment Plan

### 12.1 Phase 1: Database Migration
1. Tạo migration script
2. Deploy database changes
3. Verify data integrity

### 12.2 Phase 2: Service Implementation
1. Implement PromotionService
2. Update ProductService
3. Update DTOs và mappings

### 12.3 Phase 3: Integration
1. Update existing APIs để include promotion info
2. Cache implementation
3. Testing và validation

### 12.4 Phase 4: Admin Interface (Future)
1. Blazor pages cho quản lý promotion
2. CRUD operations
3. Bulk operations

## 13. Security Considerations

### 13.1 Data Validation
- Validate promotion dates (StartDate < EndDate)
- Validate discount values (0-100% for percentage, positive for fixed)
- Prevent overlapping high-priority promotions

### 13.2 Performance Security
- Rate limiting cho promotion calculations
- Input validation để prevent injection
- Audit trail cho promotion changes

## 14. Monitoring và Logging

### 14.1 Metrics cần theo dõi
- Promotion application rate
- Performance của promotion calculations
- Cache hit/miss ratios
- Database query performance

### 14.2 Logging Events
- Promotion activations/deactivations
- Price calculation errors
- Cache operations
- Performance bottlenecks

Kế hoạch này tập trung vào việc tích hợp chức năng promotion vào hệ thống hiện có mà không tạo API quản lý, phù hợp với yêu cầu của bạn. Hệ thống sẽ tự động áp dụng promotion cho sản phẩm và tính toán giá khuyến mãi.
