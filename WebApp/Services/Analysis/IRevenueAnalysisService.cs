using WebApp.Models.DTOs;

namespace WebApp.Services.Analysis;

public interface IRevenueAnalysisService
{
    Task<RevenueStatisticDto> GetRevenueStatistics(RevenueFilterRequest request);
    Task<List<RevenueByTimeDto>> GetRevenueByPeriod(RevenueFilterRequest request);
    Task<List<TopCustomerDto>> GetTopCustomers(int count = 10, DateTime? fromDate = null, DateTime? toDate = null);
}

// NEW Interface for enhanced dashboard analytics
public interface IDashboardAnalyticsService
{
    Task<DashboardOverviewDto> GetDashboardOverview(DashboardFilterRequest request);
    Task<List<TopProductDto>> GetTopProducts(int count = 10, DateTime? fromDate = null, DateTime? toDate = null);
    Task<List<TopBrandDto>> GetTopBrands(int count = 10, DateTime? fromDate = null, DateTime? toDate = null);
    Task<List<TopCategoryDto>> GetTopCategories(int count = 10, DateTime? fromDate = null, DateTime? toDate = null);
    Task<List<PaymentMethodStatDto>> GetPaymentMethodStats(DateTime? fromDate = null, DateTime? toDate = null);
    Task<List<OrderStatusStatDto>> GetOrderStatusStats(DateTime? fromDate = null, DateTime? toDate = null);
    Task<InventoryStatDto> GetInventoryStats(int lowStockThreshold = 10);
    Task<DashboardStatsDto> GetDashboardStats(DateTime? fromDate = null, DateTime? toDate = null);
} 