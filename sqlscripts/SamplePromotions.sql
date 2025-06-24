-- Sample Promotions Data
-- Insert sample promotions for testing

USE [ShoeStore]
GO

-- Clear existing promotion data (for re-running the script)
DELETE FROM PromotionProducts;
DELETE FROM PromotionCategories;
DELETE FROM PromotionBrands;
DELETE FROM Promotions;

-- Sample Promotions
INSERT INTO Promotions (Id, Name, Description, Type, DiscountValue, MaxDiscountAmount, StartDate, EndDate, IsActive, Priority, CreatedAt, UpdatedAt)
VALUES 
    ('promotion-001', N'Flash Sale 50%', N'Giảm giá 50% cho tất cả sản phẩm giày thể thao', 1, 50.0, 500000.0, '2024-06-24 00:00:00', '2024-12-31 23:59:59', 1, 5, GETUTCDATE(), GETUTCDATE()),
    ('promotion-002', N'Giảm 200K', N'Giảm 200,000 VND cho đơn hàng trên 1 triệu', 2, 200000.0, NULL, '2024-06-24 00:00:00', '2024-12-31 23:59:59', 1, 3, GETUTCDATE(), GETUTCDATE()),
    ('promotion-003', N'Nike Sale 30%', N'Giảm 30% cho tất cả sản phẩm Nike', 1, 30.0, 300000.0, '2024-06-24 00:00:00', '2024-12-31 23:59:59', 1, 4, GETUTCDATE(), GETUTCDATE()),
    ('promotion-004', N'Adidas Weekend', N'Giảm 25% cho sản phẩm Adidas cuối tuần', 1, 25.0, 250000.0, '2024-06-24 00:00:00', '2024-12-31 23:59:59', 1, 4, GETUTCDATE(), GETUTCDATE()),
    ('promotion-005', N'Giày Boot Mùa Đông', N'Giảm 15% cho danh mục giày boot', 1, 15.0, NULL, '2024-06-24 00:00:00', '2024-12-31 23:59:59', 1, 2, GETUTCDATE(), GETUTCDATE());

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