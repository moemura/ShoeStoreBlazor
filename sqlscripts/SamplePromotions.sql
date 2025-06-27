-- Sample Promotions Data
-- Insert sample promotions for testing

USE [ShoeStore]
GO

-- Clear existing promotion data (for re-running the script)
DELETE FROM PromotionProducts;
DELETE FROM PromotionCategories;
DELETE FROM PromotionBrands;
DELETE FROM Promotions;

-- Scope Enum: 0=All, 1=Product, 2=Category, 3=Brand
-- Sample Promotions
INSERT INTO Promotions (Id, Name, Description, Type, DiscountValue, MaxDiscountAmount, StartDate, EndDate, IsActive, Priority, Scope, CreatedAt, UpdatedAt)
VALUES 
    ('promotion-001', N'Flash Sale 50%', N'Giảm giá 50% cho một số sản phẩm', 1, 50.0, 500000.0, '2024-06-24 00:00:00', '2024-12-31 23:59:59', 1, 5, 1, GETUTCDATE(), GETUTCDATE()), -- Scope: Product
    ('promotion-002', N'Giảm 200K', N'Giảm 200,000 VND cho một số sản phẩm đắt tiền', 2, 200000.0, NULL, '2024-06-24 00:00:00', '2024-12-31 23:59:59', 1, 3, 1, GETUTCDATE(), GETUTCDATE()), -- Scope: Product
    ('promotion-003', N'Nike Sale 30%', N'Giảm 30% cho tất cả sản phẩm Nike', 1, 30.0, 300000.0, '2024-06-24 00:00:00', '2024-12-31 23:59:59', 1, 4, 3, GETUTCDATE(), GETUTCDATE()), -- Scope: Brand
    ('promotion-004', N'Adidas Weekend', N'Giảm 25% cho sản phẩm Adidas cuối tuần', 1, 25.0, 250000.0, '2024-06-24 00:00:00', '2024-12-31 23:59:59', 1, 4, 3, GETUTCDATE(), GETUTCDATE()), -- Scope: Brand
    ('promotion-005', N'Giày Boot Mùa Đông', N'Giảm 15% cho danh mục giày boot', 1, 15.0, NULL, '2024-06-24 00:00:00', '2024-12-31 23:59:59', 1, 2, 2, GETUTCDATE(), GETUTCDATE()), -- Scope: Category
    ('promotion-006', N'Grand Opening', N'Giảm 10% cho tất cả sản phẩm', 1, 10.0, 100000.0, '2024-06-24 00:00:00', '2024-12-31 23:59:59', 1, 1, 0, GETUTCDATE(), GETUTCDATE()); -- Scope: All

-- Promotion for specific brands (Nike, Adidas)
-- Get brand IDs first, then insert
DECLARE @NikeBrandId NVARCHAR(450), @AdidasBrandId NVARCHAR(450);

SELECT @NikeBrandId = Id FROM Brands WHERE Name LIKE '%Nike%';
SELECT @AdidasBrandId = Id FROM Brands WHERE Name LIKE '%Adidas%';

-- Nike promotion
IF @NikeBrandId IS NOT NULL
    INSERT INTO PromotionBrands (Id, PromotionId, BrandId, CreatedAt, UpdatedAt)
    VALUES (NEWID(), 'promotion-003', @NikeBrandId, GETUTCDATE(), GETUTCDATE());

-- Adidas promotion  
IF @AdidasBrandId IS NOT NULL
    INSERT INTO PromotionBrands (Id, PromotionId, BrandId, CreatedAt, UpdatedAt)
    VALUES (NEWID(), 'promotion-004', @AdidasBrandId, GETUTCDATE(), GETUTCDATE());

-- Promotion for specific categories
-- Get category IDs and apply boot category promotion
DECLARE @BootCategoryId NVARCHAR(450);
SELECT @BootCategoryId = Id FROM Categories WHERE Name LIKE '%Boot%' OR Name LIKE '%boot%';

IF @BootCategoryId IS NOT NULL
    INSERT INTO PromotionCategories (Id, PromotionId, CategoryId, CreatedAt, UpdatedAt)
    VALUES (NEWID(), 'promotion-005', @BootCategoryId, GETUTCDATE(), GETUTCDATE());

-- Promotion for specific products (Flash Sale for first 3 products)
INSERT INTO PromotionProducts (Id, PromotionId, ProductId, CreatedAt, UpdatedAt)
SELECT 
    NEWID(),
    'promotion-001',
    Id,
    GETUTCDATE(),
    GETUTCDATE()
FROM (
    SELECT TOP 3 Id 
    FROM Products 
    ORDER BY CreatedAt
) AS TopProducts;

-- Promotion for products with discount (Giảm 200K for expensive products)
INSERT INTO PromotionProducts (Id, PromotionId, ProductId, CreatedAt, UpdatedAt)
SELECT 
    NEWID(),
    'promotion-002',
    Id,
    GETUTCDATE(),
    GETUTCDATE()
FROM (
    SELECT TOP 5 Id 
    FROM Products 
    WHERE Price > 800000
    ORDER BY Price DESC
) AS ExpensiveProducts;

-- Display results
SELECT 'Promotions Created' AS Status;
SELECT * FROM Promotions;

SELECT 'Promotion Brands' AS Status;
SELECT pb.*, p.Name as PromotionName, b.Name as BrandName 
FROM PromotionBrands pb 
LEFT JOIN Promotions p ON pb.PromotionId = p.Id
LEFT JOIN Brands b ON pb.BrandId = b.Id;

SELECT 'Promotion Categories' AS Status;
SELECT pc.*, p.Name as PromotionName, c.Name as CategoryName 
FROM PromotionCategories pc 
LEFT JOIN Promotions p ON pc.PromotionId = p.Id
LEFT JOIN Categories c ON pc.CategoryId = c.Id;

SELECT 'Promotion Products' AS Status;
SELECT pp.*, p.Name as PromotionName, prod.Name as ProductName, prod.Price
FROM PromotionProducts pp 
LEFT JOIN Promotions p ON pp.PromotionId = p.Id
LEFT JOIN Products prod ON pp.ProductId = prod.Id;

-- Sample Promotions with MinOrderAmount
-- Clear existing promotions
DELETE FROM PromotionBrands;
DELETE FROM PromotionCategories;
DELETE FROM PromotionProducts;
DELETE FROM Promotions;

-- Promotion 1: Áp dụng tất cả - Giảm 10% cho đơn hàng từ 500k
INSERT INTO Promotions (Id, Name, Description, Type, DiscountValue, MaxDiscountAmount, MinOrderAmount, StartDate, EndDate, IsActive, Priority, Scope, CreatedAt, UpdatedAt)
VALUES ('promo-1', N'Giảm 10% cho đơn hàng từ 500k', N'Áp dụng cho tất cả sản phẩm với đơn hàng tối thiểu 500,000đ', 1, 10, 100000, 500000, '2025-01-01', '2025-12-31', 1, 5, 0, GETDATE(), GETDATE());

-- Promotion 2: Áp dụng category giày thể thao - Giảm 15% cho đơn hàng từ 1 triệu
INSERT INTO Promotions (Id, Name, Description, Type, DiscountValue, MaxDiscountAmount, MinOrderAmount, StartDate, EndDate, IsActive, Priority, Scope, CreatedAt, UpdatedAt)
VALUES ('promo-2', N'Giảm 15% giày thể thao cho đơn từ 1 triệu', N'Áp dụng cho danh mục giày thể thao với đơn hàng tối thiểu 1,000,000đ', 1, 15, 200000, 1000000, '2025-01-01', '2025-12-31', 1, 7, 2, GETDATE(), GETDATE());

-- Promotion 3: Áp dụng brand Nike - Giảm 200k cho đơn hàng từ 2 triệu
INSERT INTO Promotions (Id, Name, Description, Type, DiscountValue, MaxDiscountAmount, MinOrderAmount, StartDate, EndDate, IsActive, Priority, Scope, CreatedAt, UpdatedAt)
VALUES ('promo-3', N'Giảm 200k Nike cho đơn từ 2 triệu', N'Áp dụng cho thương hiệu Nike với đơn hàng tối thiểu 2,000,000đ', 2, 200000, NULL, 2000000, '2025-01-01', '2025-12-31', 1, 8, 3, GETDATE(), GETDATE());

-- Promotion 4: Áp dụng sản phẩm cụ thể - Giảm 5% không giới hạn đơn tối thiểu
INSERT INTO Promotions (Id, Name, Description, Type, DiscountValue, MaxDiscountAmount, MinOrderAmount, StartDate, EndDate, IsActive, Priority, Scope, CreatedAt, UpdatedAt)
VALUES ('promo-4', N'Giảm 5% sản phẩm đặc biệt', N'Áp dụng cho sản phẩm được chọn, không giới hạn đơn tối thiểu', 1, 5, 50000, NULL, '2025-01-01', '2025-12-31', 1, 3, 1, GETDATE(), GETDATE());

-- Promotion 5: Áp dụng tất cả - Giảm 20% cho đơn hàng từ 3 triệu (high-end)
INSERT INTO Promotions (Id, Name, Description, Type, DiscountValue, MaxDiscountAmount, MinOrderAmount, StartDate, EndDate, IsActive, Priority, Scope, CreatedAt, UpdatedAt)
VALUES ('promo-5', N'Giảm 20% VIP cho đơn từ 3 triệu', N'Ưu đãi VIP cho đơn hàng lớn từ 3,000,000đ', 1, 20, 500000, 3000000, '2025-01-01', '2025-12-31', 1, 10, 0, GETDATE(), GETDATE());

-- Link promotions to categories (example with category IDs - adjust based on your data)
-- Note: You need to replace with actual category IDs from your database
-- INSERT INTO PromotionCategories (Id, PromotionId, CategoryId) VALUES ('pc-1', 'promo-2', 'your-sports-category-id');

-- Link promotions to brands (example with brand IDs - adjust based on your data)  
-- Note: You need to replace with actual brand IDs from your database
-- INSERT INTO PromotionBrands (Id, PromotionId, BrandId) VALUES ('pb-1', 'promo-3', 'your-nike-brand-id');

-- Link promotions to products (example with product IDs - adjust based on your data)
-- Note: You need to replace with actual product IDs from your database
-- INSERT INTO PromotionProducts (Id, PromotionId, ProductId) VALUES ('pp-1', 'promo-4', 'your-product-id');

-- Display results
SELECT 
    Name,
    Description,
    CASE Type 
        WHEN 1 THEN 'Percentage'
        WHEN 2 THEN 'Fixed Amount'
        ELSE 'Unknown'
    END as PromotionType,
    DiscountValue,
    MaxDiscountAmount,
    MinOrderAmount,
    CASE Scope
        WHEN 0 THEN 'All'
        WHEN 1 THEN 'Product'
        WHEN 2 THEN 'Category' 
        WHEN 3 THEN 'Brand'
        ELSE 'Unknown'
    END as PromotionScope,
    Priority,
    IsActive
FROM Promotions
ORDER BY Priority DESC; 