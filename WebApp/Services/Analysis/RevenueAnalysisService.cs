using Microsoft.EntityFrameworkCore;
using WebApp.Data;
using WebApp.Models.DTOs;
using WebApp.Models.Entities;

namespace WebApp.Services.Analysis;

public class RevenueAnalysisService : IRevenueAnalysisService
{
    private readonly IDbContextFactory<ShoeStoreDbContext> _dbContextFactory;

    public RevenueAnalysisService(IDbContextFactory<ShoeStoreDbContext> dbContextFactory)
    {
        _dbContextFactory = dbContextFactory;
    }

    public async Task<RevenueStatisticDto> GetRevenueStatistics(RevenueFilterRequest request)
    {
        using var dbContext = await _dbContextFactory.CreateDbContextAsync();

        var fromDate = request.FromDate ?? DateTime.UtcNow.AddMonths(-1);
        var toDate = request.ToDate ?? DateTime.UtcNow;

        // Chỉ tính đơn hàng đã hoàn thành
        var completedOrders = await dbContext.Orders
            .Where(o => o.Status == OrderStatus.Completed && 
                       o.CreatedAt >= fromDate && 
                       o.CreatedAt <= toDate)
            .ToListAsync();

        var totalRevenue = completedOrders.Sum(o => o.TotalAmount);
        var totalOrders = completedOrders.Count;
        var averageOrderValue = totalOrders > 0 ? totalRevenue / totalOrders : 0;

        var revenueByTime = await GetRevenueByPeriod(request);
        var topCustomers = await GetTopCustomers(request.TopCustomerCount, fromDate, toDate);

        return new RevenueStatisticDto
        {
            TotalRevenue = totalRevenue,
            TotalOrders = totalOrders,
            AverageOrderValue = averageOrderValue,
            RevenueByTime = revenueByTime,
            TopCustomers = topCustomers
        };
    }

    public async Task<List<RevenueByTimeDto>> GetRevenueByPeriod(RevenueFilterRequest request)
    {
        using var dbContext = await _dbContextFactory.CreateDbContextAsync();

        var fromDate = request.FromDate ?? DateTime.UtcNow.AddMonths(-1);
        var toDate = request.ToDate ?? DateTime.UtcNow;

        var completedOrders = await dbContext.Orders
            .Where(o => o.Status == OrderStatus.Completed && 
                       o.CreatedAt >= fromDate && 
                       o.CreatedAt <= toDate)
            .Select(o => new { o.CreatedAt, o.TotalAmount })
            .ToListAsync();

        var result = new List<RevenueByTimeDto>();

        switch (request.Period.ToLower())
        {
            case "day":
                result = completedOrders
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
                break;

            case "week":
                result = completedOrders
                    .GroupBy(o => GetWeekStart(o.CreatedAt))
                    .Select(g => new RevenueByTimeDto
                    {
                        Period = $"Tuần {GetWeekOfYear(g.Key)} - {g.Key.Year}",
                        Revenue = g.Sum(o => o.TotalAmount),
                        OrderCount = g.Count(),
                        Date = g.Key
                    })
                    .OrderBy(r => r.Date)
                    .ToList();
                break;

            case "month":
                result = completedOrders
                    .GroupBy(o => new { o.CreatedAt.Year, o.CreatedAt.Month })
                    .Select(g => new RevenueByTimeDto
                    {
                        Period = $"{g.Key.Month:00}/{g.Key.Year}",
                        Revenue = g.Sum(o => o.TotalAmount),
                        OrderCount = g.Count(),
                        Date = new DateTime(g.Key.Year, g.Key.Month, 1)
                    })
                    .OrderBy(r => r.Date)
                    .ToList();
                break;

            case "year":
                result = completedOrders
                    .GroupBy(o => o.CreatedAt.Year)
                    .Select(g => new RevenueByTimeDto
                    {
                        Period = g.Key.ToString(),
                        Revenue = g.Sum(o => o.TotalAmount),
                        OrderCount = g.Count(),
                        Date = new DateTime(g.Key, 1, 1)
                    })
                    .OrderBy(r => r.Date)
                    .ToList();
                break;
        }

        return result;
    }

    public async Task<List<TopCustomerDto>> GetTopCustomers(int count = 10, DateTime? fromDate = null, DateTime? toDate = null)
    {
        using var dbContext = await _dbContextFactory.CreateDbContextAsync();

        var from = fromDate ?? DateTime.UtcNow.AddMonths(-12);
        var to = toDate ?? DateTime.UtcNow;

        // Top khách hàng đăng nhập
        var registeredCustomers = await dbContext.Orders
            .Include(o => o.User)
            .Where(o => o.Status == OrderStatus.Completed && 
                       o.UserId != null &&
                       o.CreatedAt >= from && 
                       o.CreatedAt <= to)
            .GroupBy(o => new { o.UserId, o.User.UserName, o.User.Email, o.User.PhoneNumber })
            .Select(g => new TopCustomerDto
            {
                UserId = g.Key.UserId,
                CustomerName = g.Key.UserName ?? "N/A",
                Email = g.Key.Email,
                Phone = g.Key.PhoneNumber,
                TotalSpent = g.Sum(o => o.TotalAmount),
                OrderCount = g.Count(),
                LastOrderDate = g.Max(o => o.CreatedAt)
            })
            .OrderByDescending(c => c.TotalSpent)
            .Take(count)
            .ToListAsync();

        // Top khách hàng guest (theo tên và số điện thoại)
        var guestCustomers = await dbContext.Orders
            .Where(o => o.Status == OrderStatus.Completed && 
                       o.UserId == null &&
                       o.CreatedAt >= from && 
                       o.CreatedAt <= to)
            .GroupBy(o => new { o.CustomerName, o.Phone, o.Email })
            .Select(g => new TopCustomerDto
            {
                UserId = null,
                CustomerName = g.Key.CustomerName,
                Email = g.Key.Email,
                Phone = g.Key.Phone,
                TotalSpent = g.Sum(o => o.TotalAmount),
                OrderCount = g.Count(),
                LastOrderDate = g.Max(o => o.CreatedAt)
            })
            .OrderByDescending(c => c.TotalSpent)
            .Take(count)
            .ToListAsync();

        // Kết hợp và sắp xếp
        var allCustomers = registeredCustomers.Concat(guestCustomers)
            .OrderByDescending(c => c.TotalSpent)
            .Take(count)
            .ToList();

        return allCustomers;
    }

    private DateTime GetWeekStart(DateTime date)
    {
        var diff = (int)date.DayOfWeek - (int)DayOfWeek.Monday;
        if (diff < 0) diff += 7;
        return date.AddDays(-diff).Date;
    }

    private int GetWeekOfYear(DateTime date)
    {
        var jan1 = new DateTime(date.Year, 1, 1);
        var daysOffset = (int)jan1.DayOfWeek - (int)DayOfWeek.Monday;
        if (daysOffset < 0) daysOffset += 7;
        var firstWeekday = jan1.AddDays(-daysOffset);
        var weekNum = ((date - firstWeekday).Days / 7) + 1;
        return weekNum;
    }
} 