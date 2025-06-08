using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebApp.Models.DTOs;
using WebApp.Services.Analysis;

namespace WebApp.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize] // Yêu cầu đăng nhập
public class AnalyticsController : ControllerBase
{
    private readonly IRevenueAnalysisService _revenueAnalysisService;

    public AnalyticsController(IRevenueAnalysisService revenueAnalysisService)
    {
        _revenueAnalysisService = revenueAnalysisService;
    }

    /// <summary>
    /// Lấy thống kê doanh thu tổng quan
    /// </summary>
    /// <param name="request">Bộ lọc thống kê</param>
    /// <returns>Dữ liệu thống kê doanh thu</returns>
    [HttpPost("revenue-statistics")]
    public async Task<ActionResult<RevenueStatisticDto>> GetRevenueStatistics([FromBody] RevenueFilterRequest request)
    {
        try
        {
            var result = await _revenueAnalysisService.GetRevenueStatistics(request);
            return Ok(result);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Lấy doanh thu theo khoảng thời gian
    /// </summary>
    /// <param name="period">Loại thời gian: day, week, month, year</param>
    /// <param name="fromDate">Từ ngày</param>
    /// <param name="toDate">Đến ngày</param>
    /// <returns>Danh sách doanh thu theo thời gian</returns>
    [HttpGet("revenue-by-period")]
    public async Task<ActionResult<List<RevenueByTimeDto>>> GetRevenueByPeriod(
        [FromQuery] string period = "day",
        [FromQuery] DateTime? fromDate = null,
        [FromQuery] DateTime? toDate = null)
    {
        try
        {
            var request = new RevenueFilterRequest
            {
                Period = period,
                FromDate = fromDate,
                ToDate = toDate
            };

            var result = await _revenueAnalysisService.GetRevenueByPeriod(request);
            return Ok(result);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Lấy danh sách top khách hàng
    /// </summary>
    /// <param name="count">Số lượng khách hàng cần lấy</param>
    /// <param name="fromDate">Từ ngày</param>
    /// <param name="toDate">Đến ngày</param>
    /// <returns>Danh sách top khách hàng</returns>
    [HttpGet("top-customers")]
    public async Task<ActionResult<List<TopCustomerDto>>> GetTopCustomers(
        [FromQuery] int count = 10,
        [FromQuery] DateTime? fromDate = null,
        [FromQuery] DateTime? toDate = null)
    {
        try
        {
            var result = await _revenueAnalysisService.GetTopCustomers(count, fromDate, toDate);
            return Ok(result);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Lấy thống kê tổng quan (doanh thu hôm nay, tuần này, tháng này)
    /// </summary>
    /// <returns>Thống kê tổng quan</returns>
    [HttpGet("overview")]
    public async Task<ActionResult> GetOverview()
    {
        try
        {
            var today = DateTime.UtcNow.Date;
            var startOfWeek = today.AddDays(-(int)today.DayOfWeek + (int)DayOfWeek.Monday);
            var startOfMonth = new DateTime(today.Year, today.Month, 1);

            // Thống kê hôm nay
            var todayStats = await _revenueAnalysisService.GetRevenueStatistics(new RevenueFilterRequest
            {
                FromDate = today,
                ToDate = today.AddDays(1),
                Period = "day"
            });

            // Thống kê tuần này
            var weekStats = await _revenueAnalysisService.GetRevenueStatistics(new RevenueFilterRequest
            {
                FromDate = startOfWeek,
                ToDate = today.AddDays(1),
                Period = "week"
            });

            // Thống kê tháng này
            var monthStats = await _revenueAnalysisService.GetRevenueStatistics(new RevenueFilterRequest
            {
                FromDate = startOfMonth,
                ToDate = today.AddDays(1),
                Period = "month"
            });

            return Ok(new
            {
                today = new
                {
                    revenue = todayStats.TotalRevenue,
                    orders = todayStats.TotalOrders
                },
                thisWeek = new
                {
                    revenue = weekStats.TotalRevenue,
                    orders = weekStats.TotalOrders
                },
                thisMonth = new
                {
                    revenue = monthStats.TotalRevenue,
                    orders = monthStats.TotalOrders
                }
            });
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
} 