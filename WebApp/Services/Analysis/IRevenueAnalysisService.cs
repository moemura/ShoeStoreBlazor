using WebApp.Models.DTOs;

namespace WebApp.Services.Analysis;

public interface IRevenueAnalysisService
{
    Task<RevenueStatisticDto> GetRevenueStatistics(RevenueFilterRequest request);
    Task<List<RevenueByTimeDto>> GetRevenueByPeriod(RevenueFilterRequest request);
    Task<List<TopCustomerDto>> GetTopCustomers(int count = 10, DateTime? fromDate = null, DateTime? toDate = null);
} 