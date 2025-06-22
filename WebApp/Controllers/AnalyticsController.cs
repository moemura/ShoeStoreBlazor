using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebApp.Models.DTOs;
using WebApp.Services.Analysis;

namespace WebApp.Controllers;

/// <summary>
/// Controller phân tích và thống kê doanh thu
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize] // Yêu cầu đăng nhập
public class AnalyticsController : ControllerBase
{
    private readonly IRevenueAnalysisService _revenueAnalysisService;
    private readonly IDashboardAnalyticsService _dashboardAnalyticsService;

    public AnalyticsController(IRevenueAnalysisService revenueAnalysisService, IDashboardAnalyticsService dashboardAnalyticsService)
    {
        _revenueAnalysisService = revenueAnalysisService;
        _dashboardAnalyticsService = dashboardAnalyticsService;
    }

    /// <summary>
    /// Lấy thống kê doanh thu tổng quan
    /// </summary>
    /// <param name="request">Bộ lọc thống kê doanh thu</param>
    /// <returns>Dữ liệu thống kê doanh thu</returns>
    /// <response code="200">Trả về thống kê doanh thu thành công</response>
    /// <response code="400">Lỗi khi xử lý dữ liệu</response>
    /// <response code="401">Chưa đăng nhập</response>
    [HttpPost("revenue-statistics")]
    [ProducesResponseType(typeof(RevenueStatisticDto), 200)]
    [ProducesResponseType(typeof(object), 400)]
    [ProducesResponseType(401)]
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
    /// <param name="fromDate">Từ ngày (optional)</param>
    /// <param name="toDate">Đến ngày (optional)</param>
    /// <returns>Danh sách doanh thu theo thời gian</returns>
    /// <response code="200">Trả về doanh thu theo thời gian thành công</response>
    /// <response code="400">Lỗi khi xử lý dữ liệu</response>
    /// <response code="401">Chưa đăng nhập</response>
    [HttpGet("revenue-by-period")]
    [ProducesResponseType(typeof(List<RevenueByTimeDto>), 200)]
    [ProducesResponseType(typeof(object), 400)]
    [ProducesResponseType(401)]
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
    /// Lấy danh sách top khách hàng có doanh thu cao nhất
    /// </summary>
    /// <param name="count">Số lượng khách hàng cần lấy (mặc định: 10)</param>
    /// <param name="fromDate">Từ ngày (optional)</param>
    /// <param name="toDate">Đến ngày (optional)</param>
    /// <returns>Danh sách top khách hàng</returns>
    /// <response code="200">Trả về danh sách top khách hàng thành công</response>
    /// <response code="400">Lỗi khi xử lý dữ liệu</response>
    /// <response code="401">Chưa đăng nhập</response>
    [HttpGet("top-customers")]
    [ProducesResponseType(typeof(List<TopCustomerDto>), 200)]
    [ProducesResponseType(typeof(object), 400)]
    [ProducesResponseType(401)]
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
    /// <returns>Thống kê tổng quan về doanh thu và đơn hàng</returns>
    /// <response code="200">Trả về thống kê tổng quan thành công</response>
    /// <response code="400">Lỗi khi xử lý dữ liệu</response>
    /// <response code="401">Chưa đăng nhập</response>
    [HttpGet("overview")]
    [ProducesResponseType(typeof(object), 200)]
    [ProducesResponseType(typeof(object), 400)]
    [ProducesResponseType(401)]
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

    /// <summary>
    /// Lấy tổng quan dashboard với tất cả thống kê
    /// </summary>
    /// <param name="request">Bộ lọc thống kê dashboard</param>
    /// <returns>Dữ liệu tổng quan dashboard</returns>
    /// <response code="200">Trả về tổng quan dashboard thành công</response>
    /// <response code="400">Lỗi khi xử lý dữ liệu</response>
    /// <response code="401">Chưa đăng nhập</response>
    [HttpPost("dashboard-overview")]
    [ProducesResponseType(typeof(DashboardOverviewDto), 200)]
    [ProducesResponseType(typeof(object), 400)]
    [ProducesResponseType(401)]
    public async Task<ActionResult<DashboardOverviewDto>> GetDashboardOverview([FromBody] DashboardFilterRequest request)
    {
        try
        {
            var result = await _dashboardAnalyticsService.GetDashboardOverview(request);
            return Ok(result);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Lấy danh sách sản phẩm bán chạy nhất
    /// </summary>
    /// <param name="count">Số lượng sản phẩm cần lấy (mặc định: 10)</param>
    /// <param name="fromDate">Từ ngày (optional)</param>
    /// <param name="toDate">Đến ngày (optional)</param>
    /// <returns>Danh sách sản phẩm bán chạy</returns>
    /// <response code="200">Trả về danh sách sản phẩm bán chạy thành công</response>
    /// <response code="400">Lỗi khi xử lý dữ liệu</response>
    /// <response code="401">Chưa đăng nhập</response>
    [HttpGet("top-products")]
    [ProducesResponseType(typeof(List<TopProductDto>), 200)]
    [ProducesResponseType(typeof(object), 400)]
    [ProducesResponseType(401)]
    public async Task<ActionResult<List<TopProductDto>>> GetTopProducts(
        [FromQuery] int count = 10,
        [FromQuery] DateTime? fromDate = null,
        [FromQuery] DateTime? toDate = null)
    {
        try
        {
            var result = await _dashboardAnalyticsService.GetTopProducts(count, fromDate, toDate);
            return Ok(result);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Lấy thống kê thương hiệu hàng đầu
    /// </summary>
    /// <param name="count">Số lượng thương hiệu cần lấy (mặc định: 10)</param>
    /// <param name="fromDate">Từ ngày (optional)</param>
    /// <param name="toDate">Đến ngày (optional)</param>
    /// <returns>Danh sách thương hiệu hàng đầu</returns>
    /// <response code="200">Trả về thống kê thương hiệu thành công</response>
    /// <response code="400">Lỗi khi xử lý dữ liệu</response>
    /// <response code="401">Chưa đăng nhập</response>
    [HttpGet("top-brands")]
    [ProducesResponseType(typeof(List<TopBrandDto>), 200)]
    [ProducesResponseType(typeof(object), 400)]
    [ProducesResponseType(401)]
    public async Task<ActionResult<List<TopBrandDto>>> GetTopBrands(
        [FromQuery] int count = 10,
        [FromQuery] DateTime? fromDate = null,
        [FromQuery] DateTime? toDate = null)
    {
        try
        {
            var result = await _dashboardAnalyticsService.GetTopBrands(count, fromDate, toDate);
            return Ok(result);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Lấy thống kê danh mục sản phẩm hàng đầu
    /// </summary>
    /// <param name="count">Số lượng danh mục cần lấy (mặc định: 10)</param>
    /// <param name="fromDate">Từ ngày (optional)</param>
    /// <param name="toDate">Đến ngày (optional)</param>
    /// <returns>Danh sách danh mục hàng đầu</returns>
    /// <response code="200">Trả về thống kê danh mục thành công</response>
    /// <response code="400">Lỗi khi xử lý dữ liệu</response>
    /// <response code="401">Chưa đăng nhập</response>
    [HttpGet("top-categories")]
    [ProducesResponseType(typeof(List<TopCategoryDto>), 200)]
    [ProducesResponseType(typeof(object), 400)]
    [ProducesResponseType(401)]
    public async Task<ActionResult<List<TopCategoryDto>>> GetTopCategories(
        [FromQuery] int count = 10,
        [FromQuery] DateTime? fromDate = null,
        [FromQuery] DateTime? toDate = null)
    {
        try
        {
            var result = await _dashboardAnalyticsService.GetTopCategories(count, fromDate, toDate);
            return Ok(result);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Lấy thống kê phương thức thanh toán
    /// </summary>
    /// <param name="fromDate">Từ ngày (optional)</param>
    /// <param name="toDate">Đến ngày (optional)</param>
    /// <returns>Thống kê phương thức thanh toán</returns>
    /// <response code="200">Trả về thống kê phương thức thanh toán thành công</response>
    /// <response code="400">Lỗi khi xử lý dữ liệu</response>
    /// <response code="401">Chưa đăng nhập</response>
    [HttpGet("payment-methods")]
    [ProducesResponseType(typeof(List<PaymentMethodStatDto>), 200)]
    [ProducesResponseType(typeof(object), 400)]
    [ProducesResponseType(401)]
    public async Task<ActionResult<List<PaymentMethodStatDto>>> GetPaymentMethodStats(
        [FromQuery] DateTime? fromDate = null,
        [FromQuery] DateTime? toDate = null)
    {
        try
        {
            var result = await _dashboardAnalyticsService.GetPaymentMethodStats(fromDate, toDate);
            return Ok(result);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Lấy thống kê trạng thái đơn hàng
    /// </summary>
    /// <param name="fromDate">Từ ngày (optional)</param>
    /// <param name="toDate">Đến ngày (optional)</param>
    /// <returns>Thống kê trạng thái đơn hàng</returns>
    /// <response code="200">Trả về thống kê trạng thái đơn hàng thành công</response>
    /// <response code="400">Lỗi khi xử lý dữ liệu</response>
    /// <response code="401">Chưa đăng nhập</response>
    [HttpGet("order-statuses")]
    [ProducesResponseType(typeof(List<OrderStatusStatDto>), 200)]
    [ProducesResponseType(typeof(object), 400)]
    [ProducesResponseType(401)]
    public async Task<ActionResult<List<OrderStatusStatDto>>> GetOrderStatusStats(
        [FromQuery] DateTime? fromDate = null,
        [FromQuery] DateTime? toDate = null)
    {
        try
        {
            var result = await _dashboardAnalyticsService.GetOrderStatusStats(fromDate, toDate);
            return Ok(result);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Lấy thống kê tồn kho
    /// </summary>
    /// <param name="lowStockThreshold">Ngưỡng tồn kho thấp (mặc định: 10)</param>
    /// <returns>Thống kê tồn kho</returns>
    /// <response code="200">Trả về thống kê tồn kho thành công</response>
    /// <response code="400">Lỗi khi xử lý dữ liệu</response>
    /// <response code="401">Chưa đăng nhập</response>
    [HttpGet("inventory-stats")]
    [ProducesResponseType(typeof(InventoryStatDto), 200)]
    [ProducesResponseType(typeof(object), 400)]
    [ProducesResponseType(401)]
    public async Task<ActionResult<InventoryStatDto>> GetInventoryStats(
        [FromQuery] int lowStockThreshold = 10)
    {
        try
        {
            var result = await _dashboardAnalyticsService.GetInventoryStats(lowStockThreshold);
            return Ok(result);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Lấy thống kê tổng quan dashboard (nhanh)
    /// </summary>
    /// <param name="fromDate">Từ ngày (optional)</param>
    /// <param name="toDate">Đến ngày (optional)</param>
    /// <returns>Thống kê tổng quan</returns>
    /// <response code="200">Trả về thống kê tổng quan thành công</response>
    /// <response code="400">Lỗi khi xử lý dữ liệu</response>
    /// <response code="401">Chưa đăng nhập</response>
    [HttpGet("dashboard-stats")]
    [ProducesResponseType(typeof(DashboardStatsDto), 200)]
    [ProducesResponseType(typeof(object), 400)]
    [ProducesResponseType(401)]
    public async Task<ActionResult<DashboardStatsDto>> GetDashboardStats(
        [FromQuery] DateTime? fromDate = null,
        [FromQuery] DateTime? toDate = null)
    {
        try
        {
            var result = await _dashboardAnalyticsService.GetDashboardStats(fromDate, toDate);
            return Ok(result);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
} 