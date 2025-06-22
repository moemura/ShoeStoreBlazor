using Microsoft.EntityFrameworkCore;
using WebApp.Data;
using WebApp.Models.DTOs;
using WebApp.Models.Entities;

namespace WebApp.Services.Analysis;

public class DashboardAnalyticsService : IDashboardAnalyticsService
{
    private readonly IDbContextFactory<ShoeStoreDbContext> _dbContextFactory;

    public DashboardAnalyticsService(IDbContextFactory<ShoeStoreDbContext> dbContextFactory)
    {
        _dbContextFactory = dbContextFactory;
    }

    public async Task<DashboardOverviewDto> GetDashboardOverview(DashboardFilterRequest request)
    {
        var fromDate = request.FromDate ?? DateTime.UtcNow.AddDays(-30);
        var toDate = request.ToDate ?? DateTime.UtcNow;

        var overview = new DashboardOverviewDto
        {
            Stats = await GetDashboardStats(fromDate, toDate),
            RevenueChart = await GetRevenueChart(fromDate, toDate),
            TopProducts = await GetTopProducts(request.TopCount, fromDate, toDate),
            TopBrands = await GetTopBrands(request.TopCount, fromDate, toDate),
            TopCategories = await GetTopCategories(request.TopCount, fromDate, toDate),
            PaymentMethods = await GetPaymentMethodStats(fromDate, toDate),
            OrderStatuses = await GetOrderStatusStats(fromDate, toDate),
            InventoryStats = await GetInventoryStats(request.LowStockThreshold)
        };

        return overview;
    }

    public async Task<DashboardStatsDto> GetDashboardStats(DateTime? fromDate = null, DateTime? toDate = null)
    {
        using var dbContext = await _dbContextFactory.CreateDbContextAsync();

        var today = DateTime.UtcNow.Date;
        var startOfWeek = today.AddDays(-(int)today.DayOfWeek + (int)DayOfWeek.Monday);
        var startOfMonth = new DateTime(today.Year, today.Month, 1);
        var lastMonth = startOfMonth.AddMonths(-1);

        // Today stats
        var todayOrders = await dbContext.Orders
            .Where(o => o.Status == OrderStatus.Completed && o.CreatedAt >= today)
            .ToListAsync();

        // Week stats
        var weekOrders = await dbContext.Orders
            .Where(o => o.Status == OrderStatus.Completed && o.CreatedAt >= startOfWeek)
            .ToListAsync();

        // Month stats
        var monthOrders = await dbContext.Orders
            .Where(o => o.Status == OrderStatus.Completed && o.CreatedAt >= startOfMonth)
            .ToListAsync();

        // Last month for growth calculation
        var lastMonthOrders = await dbContext.Orders
            .Where(o => o.Status == OrderStatus.Completed && 
                       o.CreatedAt >= lastMonth && o.CreatedAt < startOfMonth)
            .ToListAsync();

        // Total products and customers
        var totalProducts = await dbContext.Products.CountAsync();
        var totalCustomers = await dbContext.Orders
            .Where(o => o.UserId != null)
            .Select(o => o.UserId)
            .Distinct()
            .CountAsync();

        // Low stock count
        var lowStockCount = await dbContext.Inventories
            .Where(i => i.Quantity <= 10)
            .CountAsync();

        // Growth rate calculation
        var thisMonthRevenue = monthOrders.Sum(o => o.TotalAmount);
        var lastMonthRevenue = lastMonthOrders.Sum(o => o.TotalAmount);
        var growthRate = lastMonthRevenue > 0 ? ((thisMonthRevenue - lastMonthRevenue) / lastMonthRevenue) * 100 : 0;

        return new DashboardStatsDto
        {
            TodayRevenue = todayOrders.Sum(o => o.TotalAmount),
            TodayOrders = todayOrders.Count,
            WeekRevenue = weekOrders.Sum(o => o.TotalAmount),
            WeekOrders = weekOrders.Count,
            MonthRevenue = thisMonthRevenue,
            MonthOrders = monthOrders.Count,
            AverageOrderValue = monthOrders.Count > 0 ? thisMonthRevenue / monthOrders.Count : 0,
            TotalProducts = totalProducts,
            TotalCustomers = totalCustomers,
            LowStockCount = lowStockCount,
            GrowthRate = growthRate
        };
    }

    public async Task<List<TopProductDto>> GetTopProducts(int count = 10, DateTime? fromDate = null, DateTime? toDate = null)
    {
        using var dbContext = await _dbContextFactory.CreateDbContextAsync();

        var from = fromDate ?? DateTime.UtcNow.AddMonths(-3);
        var to = toDate ?? DateTime.UtcNow;

        var topProducts = await dbContext.OrderItems
            .Include(oi => oi.Order)
            .Include(oi => oi.Inventory)
                .ThenInclude(i => i.Product)
                    .ThenInclude(p => p.Brand)
            .Include(oi => oi.Inventory)
                .ThenInclude(i => i.Product)
                    .ThenInclude(p => p.Category)
            .Where(oi => oi.Order.Status == OrderStatus.Completed &&
                        oi.Order.CreatedAt >= from &&
                        oi.Order.CreatedAt <= to)
            .GroupBy(oi => new { 
                oi.Inventory.ProductId,
                ProductName = oi.Inventory.Product.Name,
                ProductImage = oi.Inventory.Product.MainImage,
                BrandName = oi.Inventory.Product.Brand.Name,
                CategoryName = oi.Inventory.Product.Category.Name,
                Price = oi.Inventory.Product.Price
            })
            .Select(g => new TopProductDto
            {
                ProductId = g.Key.ProductId,
                ProductName = g.Key.ProductName,
                ProductImage = g.Key.ProductImage,
                BrandName = g.Key.BrandName,
                CategoryName = g.Key.CategoryName,
                TotalSold = g.Sum(oi => oi.Quantity),
                TotalRevenue = g.Sum(oi => oi.Subtotal),
                Price = g.Key.Price,
                StockLevel = 0 // Will be calculated separately
            })
            .OrderByDescending(p => p.TotalSold)
            .Take(count)
            .ToListAsync();

        // Get stock levels
        foreach (var product in topProducts)
        {
            product.StockLevel = await dbContext.Inventories
                .Where(i => i.ProductId == product.ProductId)
                .SumAsync(i => i.Quantity);
        }

        return topProducts;
    }

    public async Task<List<TopBrandDto>> GetTopBrands(int count = 10, DateTime? fromDate = null, DateTime? toDate = null)
    {
        using var dbContext = await _dbContextFactory.CreateDbContextAsync();

        var from = fromDate ?? DateTime.UtcNow.AddMonths(-3);
        var to = toDate ?? DateTime.UtcNow;

        var topBrands = await dbContext.OrderItems
            .Include(oi => oi.Order)
            .Include(oi => oi.Inventory)
                .ThenInclude(i => i.Product)
                    .ThenInclude(p => p.Brand)
            .Where(oi => oi.Order.Status == OrderStatus.Completed &&
                        oi.Order.CreatedAt >= from &&
                        oi.Order.CreatedAt <= to &&
                        oi.Inventory.Product.Brand != null)
            .GroupBy(oi => new { 
                oi.Inventory.Product.Brand.Id,
                oi.Inventory.Product.Brand.Name,
                oi.Inventory.Product.Brand.Logo
            })
            .Select(g => new TopBrandDto
            {
                BrandId = g.Key.Id,
                BrandName = g.Key.Name,
                Logo = g.Key.Logo,
                TotalSold = g.Sum(oi => oi.Quantity),
                TotalRevenue = g.Sum(oi => oi.Subtotal),
                ProductCount = g.Select(oi => oi.Inventory.ProductId).Distinct().Count(),
                AveragePrice = g.Average(oi => oi.Price)
            })
            .OrderByDescending(b => b.TotalRevenue)
            .Take(count)
            .ToListAsync();

        return topBrands;
    }

    public async Task<List<TopCategoryDto>> GetTopCategories(int count = 10, DateTime? fromDate = null, DateTime? toDate = null)
    {
        using var dbContext = await _dbContextFactory.CreateDbContextAsync();

        var from = fromDate ?? DateTime.UtcNow.AddMonths(-3);
        var to = toDate ?? DateTime.UtcNow;

        var totalRevenue = await dbContext.OrderItems
            .Include(oi => oi.Order)
            .Where(oi => oi.Order.Status == OrderStatus.Completed &&
                        oi.Order.CreatedAt >= from &&
                        oi.Order.CreatedAt <= to)
            .SumAsync(oi => oi.Subtotal);

        var topCategories = await dbContext.OrderItems
            .Include(oi => oi.Order)
            .Include(oi => oi.Inventory)
                .ThenInclude(i => i.Product)
                    .ThenInclude(p => p.Category)
            .Where(oi => oi.Order.Status == OrderStatus.Completed &&
                        oi.Order.CreatedAt >= from &&
                        oi.Order.CreatedAt <= to &&
                        oi.Inventory.Product.Category != null)
            .GroupBy(oi => new { 
                oi.Inventory.Product.Category.Id,
                oi.Inventory.Product.Category.Name,
                oi.Inventory.Product.Category.Image
            })
            .Select(g => new TopCategoryDto
            {
                CategoryId = g.Key.Id,
                CategoryName = g.Key.Name,
                Image = g.Key.Image,
                TotalSold = g.Sum(oi => oi.Quantity),
                TotalRevenue = g.Sum(oi => oi.Subtotal),
                ProductCount = g.Select(oi => oi.Inventory.ProductId).Distinct().Count(),
                MarketShare = totalRevenue > 0 ? (g.Sum(oi => oi.Subtotal) / totalRevenue) * 100 : 0
            })
            .OrderByDescending(c => c.TotalRevenue)
            .Take(count)
            .ToListAsync();

        return topCategories;
    }

    public async Task<List<PaymentMethodStatDto>> GetPaymentMethodStats(DateTime? fromDate = null, DateTime? toDate = null)
    {
        using var dbContext = await _dbContextFactory.CreateDbContextAsync();

        var from = fromDate ?? DateTime.UtcNow.AddMonths(-3);
        var to = toDate ?? DateTime.UtcNow;

        var totalOrders = await dbContext.Orders
            .Where(o => o.Status == OrderStatus.Completed &&
                       o.CreatedAt >= from &&
                       o.CreatedAt <= to)
            .CountAsync();

        var paymentStats = await dbContext.Orders
            .Where(o => o.Status == OrderStatus.Completed &&
                       o.CreatedAt >= from &&
                       o.CreatedAt <= to)
            .GroupBy(o => o.PaymentMethod)
            .Select(g => new PaymentMethodStatDto
            {
                PaymentMethod = g.Key.ToString(),
                OrderCount = g.Count(),
                TotalAmount = g.Sum(o => o.TotalAmount),
                Percentage = totalOrders > 0 ? (double)g.Count() / totalOrders * 100 : 0
            })
            .OrderByDescending(p => p.OrderCount)
            .ToListAsync();

        return paymentStats;
    }

    public async Task<List<OrderStatusStatDto>> GetOrderStatusStats(DateTime? fromDate = null, DateTime? toDate = null)
    {
        using var dbContext = await _dbContextFactory.CreateDbContextAsync();

        var from = fromDate ?? DateTime.UtcNow.AddMonths(-1);
        var to = toDate ?? DateTime.UtcNow;

        var totalOrders = await dbContext.Orders
            .Where(o => o.CreatedAt >= from && o.CreatedAt <= to)
            .CountAsync();

        var statusStats = await dbContext.Orders
            .Where(o => o.CreatedAt >= from && o.CreatedAt <= to)
            .GroupBy(o => o.Status)
            .Select(g => new OrderStatusStatDto
            {
                Status = g.Key.ToString(),
                Count = g.Count(),
                Percentage = totalOrders > 0 ? (double)g.Count() / totalOrders * 100 : 0
            })
            .OrderByDescending(s => s.Count)
            .ToListAsync();

        return statusStats;
    }

    public async Task<InventoryStatDto> GetInventoryStats(int lowStockThreshold = 10)
    {
        using var dbContext = await _dbContextFactory.CreateDbContextAsync();

        var inventories = await dbContext.Inventories
            .Include(i => i.Product)
            .Include(i => i.Size)
            .ToListAsync();

        var totalProducts = await dbContext.Products.CountAsync();
        var totalStock = inventories.Sum(i => i.Quantity);
        var lowStockCount = inventories.Count(i => i.Quantity <= lowStockThreshold && i.Quantity > 0);
        var outOfStockCount = inventories.Count(i => i.Quantity == 0);

        var lowStockProducts = inventories
            .Where(i => i.Quantity <= lowStockThreshold && i.Quantity > 0)
            .Select(i => new LowStockProductDto
            {
                ProductId = i.ProductId,
                ProductName = i.Product?.Name ?? "Unknown",
                ProductImage = i.Product?.MainImage,
                Size = i.Size?.Id ?? "Unknown",
                StockLevel = i.Quantity,
                MinStockLevel = lowStockThreshold
            })
            .OrderBy(p => p.StockLevel)
            .Take(20)
            .ToList();

        return new InventoryStatDto
        {
            TotalProducts = totalProducts,
            TotalStock = totalStock,
            LowStockCount = lowStockCount,
            OutOfStockCount = outOfStockCount,
            LowStockProducts = lowStockProducts
        };
    }

    private async Task<List<RevenueByTimeDto>> GetRevenueChart(DateTime fromDate, DateTime toDate)
    {
        using var dbContext = await _dbContextFactory.CreateDbContextAsync();

        var completedOrders = await dbContext.Orders
            .Where(o => o.Status == OrderStatus.Completed &&
                       o.CreatedAt >= fromDate &&
                       o.CreatedAt <= toDate)
            .Select(o => new { o.CreatedAt, o.TotalAmount })
            .ToListAsync();

        var result = completedOrders
            .GroupBy(o => o.CreatedAt.Date)
            .Select(g => new RevenueByTimeDto
            {
                Period = g.Key.ToString("yyyy-MM-dd"),
                Revenue = g.Sum(o => o.TotalAmount),
                OrderCount = g.Count(),
                Date = g.Key
            })
            .OrderBy(r => r.Date)
            .ToList();

        return result;
    }
} 