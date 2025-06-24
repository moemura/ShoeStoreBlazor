# Promotion System API Documentation

## Overview
Hệ thống Promotion tự động áp dụng các chương trình giảm giá cho sản phẩm mà không cần quản lý API. Promotion được tính toán tự động khi truy vấn sản phẩm, giỏ hàng và đơn hàng.

## API Endpoints

### 1. Product APIs (Đã tích hợp promotion)

#### GET /api/products/{id}
Lấy thông tin chi tiết sản phẩm **bao gồm thông tin promotion**.

**Response:**
```json
{
  "id": "product-id",
  "name": "Giày thể thao ABC",
  "price": 1000000,
  "salePrice": 800000,
  
  // Promotion Information (NEW)
  "promotionPrice": 750000,
  "promotionDiscount": 250000,
  "promotionName": "Flash Sale 25%",
  "hasActivePromotion": true,
  
  // ... other product fields
}
```

#### GET /api/products
Lấy danh sách tất cả sản phẩm **bao gồm promotion information**.

#### GET /api/products/filter
Tìm kiếm và lọc sản phẩm **bao gồm promotion information**.

#### GET /api/products/{id}/promotions
Lấy danh sách promotion áp dụng cho sản phẩm cụ thể.

**Response:**
```json
[
  {
    "id": "promo-1",
    "name": "Flash Sale 25%",
    "description": "Giảm giá 25% cho tất cả sản phẩm",
    "type": "Percentage",
    "discountValue": 25,
    "maxDiscountAmount": 500000,
    "startDate": "2024-06-01T00:00:00Z",
    "endDate": "2024-06-30T23:59:59Z",
    "isActive": true,
    "priority": 1
  }
]
```

### 2. Cart APIs (Đã tích hợp promotion)

#### GET /api/cart
Lấy giỏ hàng **bao gồm promotion cho từng item**.

**Response:**
```json
{
  "items": [
    {
      "inventoryId": 123,
      "productId": "product-1",
      "productName": "Giày ABC",
      "quantity": 2,
      "price": 1000000,
      "salePrice": 800000,
      
      // Promotion Information (NEW)
      "promotionPrice": 750000,
      "promotionDiscount": 250000,
      "promotionName": "Flash Sale 25%",
      "hasActivePromotion": true
    }
  ],
  "createdAt": "2024-06-24T10:00:00Z",
  "updatedAt": "2024-06-24T10:30:00Z"
}
```

#### POST /api/cart/items
Thêm sản phẩm vào giỏ hàng **tự động áp dụng promotion**.

### 3. Order APIs (Đã tích hợp promotion)

#### POST /api/orders
Tạo đơn hàng **tự động tính promotion trong total**.

Promotion được áp dụng trước, sau đó mới áp dụng voucher (nếu có).

### 4. Testing APIs

#### GET /api/tests/promotion-price/{productId}
Test tính toán giá promotion cho sản phẩm.

**Response:**
```json
{
  "productId": "product-1",
  "originalPrice": 1000000,
  "promotionPrice": 750000,
  "discount": 250000,
  "bestPromotion": {
    "id": "promo-1",
    "name": "Flash Sale 25%",
    "type": "Percentage",
    "discountValue": 25
  }
}
```

#### GET /api/tests/active-promotions
Lấy danh sách tất cả promotion đang active.

## Promotion Types

### 1. Percentage
Giảm giá theo phần trăm.
- `discountValue`: Phần trăm giảm (0-100)
- `maxDiscountAmount`: Số tiền giảm tối đa (optional)

### 2. FixedAmount
Giảm giá số tiền cố định.
- `discountValue`: Số tiền giảm

### 3. BuyXGetY (Future)
Mua X tặng Y (chưa implement).

## Promotion Targeting

### Product Level
- Áp dụng cho sản phẩm cụ thể
- Junction table: `PromotionProducts`

### Category Level
- Áp dụng cho tất cả sản phẩm trong danh mục
- Junction table: `PromotionCategories`

### Brand Level
- Áp dụng cho tất cả sản phẩm của thương hiệu
- Junction table: `PromotionBrands`

## Business Rules

### Priority System
- Promotion có `priority` cao hơn được áp dụng trước
- Chỉ áp dụng 1 promotion cho 1 sản phẩm tại 1 thời điểm

### Time Validation
- Promotion chỉ active trong khoảng `startDate` - `endDate`
- Phải có `isActive = true`

### Price Calculation Order
```
1. Original Price (product.price)
2. Apply Promotion → Promotion Price
3. Compare with Sale Price → Use lower price
4. Apply Voucher (order level)
```

### Maximum Discount
- Đối với Percentage promotion có thể set `maxDiscountAmount`
- Nếu số tiền giảm > max, thì chỉ giảm max amount

## Frontend Integration

### ProductCard Component
- Hiển thị promotion badge (purple gradient)
- Hiển thị promotion name
- Hiển thị promotion price (purple color)

### CartDrawer Component
- Hiển thị promotion information cho từng item
- Tính total với promotion price

### Automatic Updates
- Promotion được apply tự động khi:
  - Load product detail
  - Load product list
  - Load cart
  - Calculate order total

## Performance Optimization

### Caching Strategy
- Cache promotion results theo product ID
- Cache active promotions
- TTL: 15-30 phút

### Database Indexes
- `IX_Promotions_StartDate_EndDate`
- `IX_Promotions_IsActive`
- `IX_Promotions_Priority`
- `IX_PromotionProducts_ProductId`

## Error Handling

### Validation
- Start date < End date
- Discount value > 0
- Priority ≥ 1

### Fallback
- Nếu promotion service fail → sử dụng original price
- Logging errors để debug

## Sample Data

File: `sqlscripts/SamplePromotions.sql`

Chứa 5 sample promotions:
1. Flash Sale 25% (All products)
2. Sneaker Sale 15% (Category specific)
3. Nike Sale 20% (Brand specific) 
4. Fixed 200K off (Fixed amount)
5. VIP 30% (High priority)

## Migration History

- `20250624060513_AddPromotionEntities`: Tạo promotion tables và relationships

## Future Enhancements

1. **Admin Management UI**: Blazor pages để quản lý promotion
2. **Scheduled Jobs**: Auto activate/deactivate theo thời gian
3. **Analytics**: Track promotion performance
4. **A/B Testing**: Test different promotion strategies
5. **Buy X Get Y**: Implement complex promotion rules
6. **Stacking**: Cho phép stack multiple promotions
7. **User Targeting**: Promotion riêng cho từng user segment 