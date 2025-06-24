# 🎉 Promotion System - ShoeStore

## Tổng quan

Hệ thống Promotion tự động áp dụng các chương trình giảm giá cho sản phẩm trong ShoeStore. Hệ thống được thiết kế để tích hợp seamlessly với existing codebase mà không cần tạo management APIs.

## ✨ Tính năng chính

### 🎯 **Automatic Promotion Application**
- ✅ Tự động tính toán và áp dụng promotion cho products
- ✅ Tự động hiển thị promotion trong cart
- ✅ Tự động tính promotion trong order total
- ✅ Priority-based promotion selection

### 🏷️ **Flexible Promotion Types**
- ✅ **Percentage Discount**: Giảm theo % với max discount amount
- ✅ **Fixed Amount**: Giảm số tiền cố định
- 🔄 **Buy X Get Y**: (Planned for future)

### 🎯 **Smart Targeting**
- ✅ **Product Level**: Target specific products
- ✅ **Category Level**: Target all products in category
- ✅ **Brand Level**: Target all products by brand

### ⚡ **Performance Optimized**
- ✅ Redis/Memory caching strategy
- ✅ Database indexes for fast queries
- ✅ Async operations throughout
- ✅ Efficient calculation algorithms

## 🏗️ Architecture

### Database Schema
```
Promotions (Main table)
├── PromotionProducts (Many-to-many with Products)
├── PromotionCategories (Many-to-many with Categories)
└── PromotionBrands (Many-to-many with Brands)
```

### Service Layer
```
IPromotionService
├── PromotionService (Main business logic)
├── PromotionCalculator (Utility for calculations)
└── Cache Integration (Performance optimization)
```

### Integration Points
```
ProductService → Automatic promotion calculation
CartService → Promotion in cart items
OrderService → Promotion in order totals
Frontend → Visual promotion display
```

## 🚀 Quick Start

### 1. Database Setup
Migration đã được tạo và apply:
```bash
dotnet ef database update
```

### 2. Sample Data
```sql
-- Run sample promotions
sqlcmd -S . -d ShoeStoreDB -i sqlscripts/SamplePromotions.sql
```

### 3. Test APIs
```bash
# Test promotion calculation
GET /api/tests/promotion-price/{productId}

# Get active promotions
GET /api/tests/active-promotions

# Get product with promotion
GET /api/products/{productId}
```

## 📊 Usage Examples

### Backend - Automatic Integration

```csharp
// ProductService automatically includes promotion
var product = await productService.GetById(productId);
// product.PromotionPrice, product.PromotionDiscount available

// CartService automatically applies promotion
var cart = await cartService.GetCart(userId);
// cart.Items[].PromotionPrice available

// OrderService automatically calculates with promotion
var total = await orderTotalStrategy.CalculateTotal(orderRequest);
// Promotion already applied in total
```

### Frontend - Visual Display

```jsx
// ProductCard shows promotion info
{product.hasActivePromotion && (
  <div className="promotion-badge">
    🎉 {product.promotionName}
  </div>
)}

// CartDrawer shows promotion prices
{item.promotionPrice && (
  <span className="promotion-price">
    {formatPrice(item.promotionPrice)}
  </span>
)}
```

## 🎛️ Configuration

### Promotion Entity
```csharp
public class Promotion : BaseEntity<string>
{
    public string Name { get; set; }           // "Flash Sale 25%"
    public string Description { get; set; }    // Description
    public PromotionType Type { get; set; }    // Percentage/FixedAmount
    public double DiscountValue { get; set; }  // 25 (for 25%)
    public double? MaxDiscountAmount { get; set; } // Max discount cap
    public DateTime StartDate { get; set; }    // Start time
    public DateTime EndDate { get; set; }      // End time
    public bool IsActive { get; set; }         // Enable/disable
    public int Priority { get; set; }          // Higher = applied first
}
```

### Business Rules
```csharp
// Priority: Higher number = higher priority
// Time: Must be within StartDate - EndDate
// Status: Must be IsActive = true
// Selection: Best promotion wins (highest priority)
// Application: Only 1 promotion per product
```

## 🎨 Frontend Integration

### Components Updated
- ✅ **ProductCard**: Promotion badge, name, price
- ✅ **CartDrawer**: Promotion info for cart items
- 🔄 **ProductDetail**: (Can be enhanced)
- 🔄 **Checkout**: (Can be enhanced)

### Visual Design
- **Promotion Badge**: Purple gradient background
- **Promotion Price**: Purple color text
- **Promotion Name**: Emoji + name in colored box
- **Original Price**: Strikethrough when promotion active

## 📈 Performance Metrics

### Caching Strategy
```
Cache Keys:
- Promotion_All_Active (15 min TTL)
- Promotion_Product_{id} (30 min TTL)
- Promotion_Category_{id} (30 min TTL)
- Promotion_Brand_{id} (30 min TTL)
```

### Database Indexes
```sql
IX_Promotions_StartDate_EndDate
IX_Promotions_IsActive  
IX_Promotions_Priority
IX_PromotionProducts_ProductId
IX_PromotionCategories_CategoryId
IX_PromotionBrands_BrandId
```

## 🧪 Testing

### Unit Tests
- ✅ PromotionCalculator logic
- ✅ PromotionService methods
- ✅ ProductService integration

### Integration Tests
- ✅ Database operations
- ✅ Cache operations
- ✅ API endpoints

### Manual Testing
```bash
# 1. Run application
dotnet run

# 2. Test endpoints
curl http://localhost:5000/api/tests/active-promotions
curl http://localhost:5000/api/products/{id}

# 3. Check frontend
npm run dev (in shoestore-react folder)
```

## 📝 Sample Data

Hệ thống đi kèm với 5 sample promotions:

1. **Flash Sale 25%** - Giảm 25% tất cả sản phẩm
2. **Sneaker Sale 15%** - Giảm 15% category giày thể thao  
3. **Nike Sale 20%** - Giảm 20% thương hiệu Nike
4. **Fixed 200K Off** - Giảm 200,000 VND
5. **VIP 30%** - Giảm 30% (priority cao)

## 🔄 Migration History

```
20250624060513_AddPromotionEntities
├── Create Promotions table
├── Create PromotionProducts junction
├── Create PromotionCategories junction  
├── Create PromotionBrands junction
└── Add database indexes
```

## 🚀 Deployment Checklist

- ✅ Database migration applied
- ✅ Sample data loaded (optional)
- ✅ Cache service configured
- ✅ Application builds successfully
- ✅ Frontend components updated
- ✅ API documentation created

## 🔮 Future Roadmap

### Phase 1 - Enhanced Management
- [ ] Admin UI cho quản lý promotions
- [ ] Bulk operations (enable/disable multiple)
- [ ] Promotion scheduling

### Phase 2 - Advanced Features  
- [ ] Buy X Get Y promotion type
- [ ] User segment targeting
- [ ] A/B testing framework
- [ ] Analytics dashboard

### Phase 3 - Intelligence
- [ ] ML-based promotion recommendations
- [ ] Dynamic pricing
- [ ] Personalized promotions
- [ ] Cross-selling promotions

## 📞 Support

### Troubleshooting

**Q: Promotion không hiển thị?**
- Kiểm tra `IsActive = true`
- Kiểm tra thời gian `StartDate` - `EndDate`
- Kiểm tra cache có bị stale không

**Q: Performance chậm?**
- Kiểm tra cache hit ratio
- Kiểm tra database indexes
- Monitor query execution time

**Q: Frontend không update?**  
- Clear browser cache
- Restart React dev server
- Kiểm tra API response

### Logs và Monitoring
```csharp
// Important logs to monitor:
- Promotion calculations
- Cache operations  
- Database query performance
- Error rates in PromotionService
```

## 👥 Contributors

- Implementation: AI Assistant
- Architecture: Based on existing ShoeStore patterns
- Testing: Comprehensive test coverage
- Documentation: Complete API and usage docs

---

**🎉 Hệ thống Promotion đã sẵn sàng production!**

Để bắt đầu sử dụng, chỉ cần run migration và load sample data. Tất cả existing APIs sẽ tự động include promotion information. 