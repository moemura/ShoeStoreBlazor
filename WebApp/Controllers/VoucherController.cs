using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using WebApp.Services.Vouchers;

namespace WebApp.Controllers;

/// <summary>
/// Controller quản lý mã giảm giá (voucher)
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class VoucherController : ControllerBase
{
    private readonly IVoucherService _voucherService;
    private readonly ILogger<VoucherController> _logger;

    public VoucherController(IVoucherService voucherService, ILogger<VoucherController> logger)
    {
        _voucherService = voucherService;
        _logger = logger;
    }

    #region Public Voucher Operations

    /// <summary>
    /// Validate mã giảm giá
    /// </summary>
    /// <param name="request">Thông tin validation voucher</param>
    /// <returns>Kết quả validation và thông tin discount</returns>
    /// <response code="200">Validation thành công</response>
    /// <response code="400">Thông tin request không hợp lệ</response>
    [HttpPost("validate")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(VoucherValidationResult), 200)]
    [ProducesResponseType(typeof(object), 400)]
    public async Task<IActionResult> ValidateVoucher([FromBody] VoucherValidationRequest request)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(request.Code))
            {
                return BadRequest(new { error = "Mã voucher không được để trống" });
            }

            if (request.OrderAmount <= 0)
            {
                return BadRequest(new { error = "Số tiền đơn hàng phải lớn hơn 0" });
            }

            // Get userId from token if authenticated
            var userId = User.Identity?.IsAuthenticated == true 
                ? User.FindFirst(ClaimTypes.NameIdentifier)?.Value 
                : null;

            // Use userId from token or guestId from request
            var actualUserId = userId ?? request.UserId;
            var actualGuestId = string.IsNullOrEmpty(userId) ? request.GuestId : null;

            var result = await _voucherService.ValidateVoucherAsync(
                request.Code, 
                request.OrderAmount, 
                actualUserId, 
                actualGuestId);

            _logger.LogInformation("Voucher validation for {Code}: {IsValid}", request.Code, result.IsValid);

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating voucher {Code}", request.Code);
            return BadRequest(new { error = "Lỗi hệ thống, vui lòng thử lại" });
        }
    }

    /// <summary>
    /// Apply mã giảm giá (tương tự validate nhưng với ý nghĩa apply)
    /// </summary>
    /// <param name="request">Thông tin apply voucher</param>
    /// <returns>Kết quả apply voucher</returns>
    /// <response code="200">Apply thành công</response>
    /// <response code="400">Apply thất bại</response>
    [HttpPost("apply")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(VoucherApplyResult), 200)]
    [ProducesResponseType(typeof(object), 400)]
    public async Task<IActionResult> ApplyVoucher([FromBody] VoucherApplyRequest request)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(request.Code))
            {
                return BadRequest(new { error = "Mã voucher không được để trống" });
            }

            if (request.OrderAmount <= 0)
            {
                return BadRequest(new { error = "Số tiền đơn hàng phải lớn hơn 0" });
            }

            // Get userId from token if authenticated
            var userId = User.Identity?.IsAuthenticated == true 
                ? User.FindFirst(ClaimTypes.NameIdentifier)?.Value 
                : null;

            // Use userId from token or guestId from request
            var actualUserId = userId ?? request.UserId;
            var actualGuestId = string.IsNullOrEmpty(userId) ? request.GuestId : null;

            var result = await _voucherService.ApplyVoucherAsync(
                request.Code, 
                request.OrderAmount, 
                actualUserId, 
                actualGuestId);

            _logger.LogInformation("Voucher apply for {Code}: {Success}", request.Code, result.Success);

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error applying voucher {Code}", request.Code);
            return BadRequest(new { error = "Lỗi hệ thống, vui lòng thử lại" });
        }
    }

    /// <summary>
    /// Lấy danh sách voucher đang hoạt động
    /// </summary>
    /// <returns>Danh sách voucher active</returns>
    /// <response code="200">Trả về danh sách voucher thành công</response>
    [HttpGet("active")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(IEnumerable<VoucherDto>), 200)]
    public async Task<IActionResult> GetActiveVouchers()
    {
        try
        {
            var vouchers = await _voucherService.GetActiveVouchersAsync();
            return Ok(vouchers);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting active vouchers");
            return BadRequest(new { error = "Lỗi hệ thống, vui lòng thử lại" });
        }
    }

    /// <summary>
    /// Kiểm tra user có thể sử dụng voucher không
    /// </summary>
    /// <param name="code">Mã voucher</param>
    /// <param name="guestId">Guest ID (optional)</param>
    /// <returns>True nếu có thể sử dụng</returns>
    /// <response code="200">Kiểm tra thành công</response>
    [HttpGet("{code}/can-use")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(bool), 200)]
    public async Task<IActionResult> CanUseVoucher(string code, [FromQuery] string? guestId = null)
    {
        try
        {
            var userId = User.Identity?.IsAuthenticated == true 
                ? User.FindFirst(ClaimTypes.NameIdentifier)?.Value 
                : null;

            var actualGuestId = string.IsNullOrEmpty(userId) ? guestId : null;

            var canUse = await _voucherService.CanUserUseVoucherAsync(code, userId, actualGuestId);
            return Ok(canUse);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking if user can use voucher {Code}", code);
            return BadRequest(new { error = "Lỗi hệ thống, vui lòng thử lại" });
        }
    }

    #endregion

    #region User Voucher History

    /// <summary>
    /// Lấy lịch sử sử dụng voucher của user hiện tại
    /// </summary>
    /// <param name="limit">Số lượng record tối đa</param>
    /// <returns>Lịch sử sử dụng voucher</returns>
    /// <response code="200">Trả về lịch sử thành công</response>
    /// <response code="401">Chưa đăng nhập</response>
    [HttpGet("my-usage")]
    [Authorize(AuthenticationSchemes = "Bearer")]
    [ProducesResponseType(typeof(IEnumerable<VoucherUsageDto>), 200)]
    [ProducesResponseType(401)]
    public async Task<IActionResult> GetMyVoucherUsage([FromQuery] int limit = 10)
    {
        try
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }

            var usages = await _voucherService.GetUserVoucherUsagesAsync(userId, limit);
            return Ok(usages);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting user voucher usage");
            return BadRequest(new { error = "Lỗi hệ thống, vui lòng thử lại" });
        }
    }

    #endregion

    #region Admin Operations

    /// <summary>
    /// Lấy danh sách tất cả voucher (Admin only)
    /// </summary>
    /// <param name="pageIndex">Số trang (bắt đầu từ 1)</param>
    /// <param name="pageSize">Số item mỗi trang</param>
    /// <param name="searchTerm">Từ khóa tìm kiếm</param>
    /// <param name="type">Loại voucher</param>
    /// <param name="isActive">Trạng thái active</param>
    /// <returns>Danh sách voucher với phân trang</returns>
    /// <response code="200">Trả về danh sách voucher thành công</response>
    /// <response code="403">Không có quyền truy cập</response>
    [HttpGet]
    [Authorize(Roles = "Admin", AuthenticationSchemes = "Bearer")]
    [ProducesResponseType(typeof(PaginatedList<VoucherDto>), 200)]
    [ProducesResponseType(403)]
    public async Task<IActionResult> GetVouchers(
        [FromQuery] int pageIndex = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? searchTerm = null,
        [FromQuery] VoucherType? type = null,
        [FromQuery] bool? isActive = null)
    {
        try
        {
            var filter = new VoucherFilterRequest
            {
                SearchTerm = searchTerm,
                Type = type,
                IsActive = isActive
            };

            var vouchers = await _voucherService.GetVouchersAsync(pageIndex, pageSize, filter);
            return Ok(vouchers);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting vouchers");
            return BadRequest(new { error = "Lỗi hệ thống, vui lòng thử lại" });
        }
    }

    /// <summary>
    /// Lấy thông tin chi tiết voucher theo mã (Admin only)
    /// </summary>
    /// <param name="code">Mã voucher</param>
    /// <returns>Thông tin chi tiết voucher</returns>
    /// <response code="200">Trả về thông tin voucher thành công</response>
    /// <response code="404">Không tìm thấy voucher</response>
    /// <response code="403">Không có quyền truy cập</response>
    [HttpGet("{code}")]
    [Authorize(Roles = "Admin", AuthenticationSchemes = "Bearer")]
    [ProducesResponseType(typeof(VoucherDto), 200)]
    [ProducesResponseType(404)]
    [ProducesResponseType(403)]
    public async Task<IActionResult> GetVoucherByCode(string code)
    {
        try
        {
            var voucher = await _voucherService.GetVoucherByCodeAsync(code);
            if (voucher == null)
            {
                return NotFound(new { error = $"Voucher '{code}' không tồn tại" });
            }

            return Ok(voucher);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting voucher {Code}", code);
            return BadRequest(new { error = "Lỗi hệ thống, vui lòng thử lại" });
        }
    }

    /// <summary>
    /// Tạo voucher mới (Admin only)
    /// </summary>
    /// <param name="request">Thông tin voucher mới</param>
    /// <returns>Thông tin voucher đã tạo</returns>
    /// <response code="201">Tạo voucher thành công</response>
    /// <response code="400">Thông tin voucher không hợp lệ</response>
    /// <response code="403">Không có quyền truy cập</response>
    [HttpPost]
    [Authorize(Roles = "Admin", AuthenticationSchemes = "Bearer")]
    [ProducesResponseType(typeof(VoucherDto), 201)]
    [ProducesResponseType(typeof(object), 400)]
    [ProducesResponseType(403)]
    public async Task<IActionResult> CreateVoucher([FromBody] VoucherCreateRequest request)
    {
        try
        {
            var voucher = await _voucherService.CreateVoucherAsync(request);
            
            _logger.LogInformation("Admin {UserId} created voucher {Code}", 
                User.FindFirst(ClaimTypes.NameIdentifier)?.Value, voucher.Code);
            
            return CreatedAtAction(
                nameof(GetVoucherByCode), 
                new { code = voucher.Code }, 
                voucher);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating voucher");
            return BadRequest(new { error = "Lỗi hệ thống, vui lòng thử lại" });
        }
    }

    /// <summary>
    /// Cập nhật voucher (Admin only)
    /// </summary>
    /// <param name="code">Mã voucher cần cập nhật</param>
    /// <param name="request">Thông tin cập nhật</param>
    /// <returns>Thông tin voucher đã cập nhật</returns>
    /// <response code="200">Cập nhật voucher thành công</response>
    /// <response code="404">Không tìm thấy voucher</response>
    /// <response code="400">Thông tin cập nhật không hợp lệ</response>
    /// <response code="403">Không có quyền truy cập</response>
    [HttpPut("{code}")]
    [Authorize(Roles = "Admin", AuthenticationSchemes = "Bearer")]
    [ProducesResponseType(typeof(VoucherDto), 200)]
    [ProducesResponseType(typeof(object), 400)]
    [ProducesResponseType(404)]
    [ProducesResponseType(403)]
    public async Task<IActionResult> UpdateVoucher(string code, [FromBody] VoucherUpdateRequest request)
    {
        try
        {
            var voucher = await _voucherService.UpdateVoucherAsync(code, request);
            
            _logger.LogInformation("Admin {UserId} updated voucher {Code}", 
                User.FindFirst(ClaimTypes.NameIdentifier)?.Value, code);
            
            return Ok(voucher);
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(new { error = ex.Message });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating voucher {Code}", code);
            return BadRequest(new { error = "Lỗi hệ thống, vui lòng thử lại" });
        }
    }

    /// <summary>
    /// Xóa voucher (Admin only)
    /// </summary>
    /// <param name="code">Mã voucher cần xóa</param>
    /// <returns>Kết quả xóa</returns>
    /// <response code="204">Xóa voucher thành công</response>
    /// <response code="404">Không tìm thấy voucher</response>
    /// <response code="400">Không thể xóa voucher</response>
    /// <response code="403">Không có quyền truy cập</response>
    [HttpDelete("{code}")]
    [Authorize(Roles = "Admin", AuthenticationSchemes = "Bearer")]
    [ProducesResponseType(204)]
    [ProducesResponseType(typeof(object), 400)]
    [ProducesResponseType(404)]
    [ProducesResponseType(403)]
    public async Task<IActionResult> DeleteVoucher(string code)
    {
        try
        {
            await _voucherService.DeleteVoucherAsync(code);
            
            _logger.LogInformation("Admin {UserId} deleted voucher {Code}", 
                User.FindFirst(ClaimTypes.NameIdentifier)?.Value, code);
            
            return NoContent();
        }
        catch (InvalidOperationException ex)
        {
            if (ex.Message.Contains("không tồn tại"))
            {
                return NotFound(new { error = ex.Message });
            }
            return BadRequest(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting voucher {Code}", code);
            return BadRequest(new { error = "Lỗi hệ thống, vui lòng thử lại" });
        }
    }

    /// <summary>
    /// Lấy thống kê voucher (Admin only)
    /// </summary>
    /// <param name="code">Mã voucher</param>
    /// <returns>Thống kê sử dụng voucher</returns>
    /// <response code="200">Trả về thống kê thành công</response>
    /// <response code="404">Không tìm thấy voucher</response>
    /// <response code="403">Không có quyền truy cập</response>
    [HttpGet("{code}/statistics")]
    [Authorize(Roles = "Admin", AuthenticationSchemes = "Bearer")]
    [ProducesResponseType(typeof(VoucherStatisticsDto), 200)]
    [ProducesResponseType(404)]
    [ProducesResponseType(403)]
    public async Task<IActionResult> GetVoucherStatistics(string code)
    {
        try
        {
            var statistics = await _voucherService.GetVoucherStatisticsAsync(code);
            return Ok(statistics);
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting voucher statistics {Code}", code);
            return BadRequest(new { error = "Lỗi hệ thống, vui lòng thử lại" });
        }
    }

    /// <summary>
    /// Lấy lịch sử sử dụng voucher (Admin only)
    /// </summary>
    /// <param name="code">Mã voucher</param>
    /// <param name="pageIndex">Số trang</param>
    /// <param name="pageSize">Số item mỗi trang</param>
    /// <returns>Lịch sử sử dụng voucher với phân trang</returns>
    /// <response code="200">Trả về lịch sử thành công</response>
    /// <response code="403">Không có quyền truy cập</response>
    [HttpGet("{code}/usages")]
    [Authorize(Roles = "Admin", AuthenticationSchemes = "Bearer")]
    [ProducesResponseType(typeof(PaginatedList<VoucherUsageDto>), 200)]
    [ProducesResponseType(403)]
    public async Task<IActionResult> GetVoucherUsages(
        string code,
        [FromQuery] int pageIndex = 1,
        [FromQuery] int pageSize = 10)
    {
        try
        {
            var usages = await _voucherService.GetVoucherUsagesAsync(code, pageIndex, pageSize);
            return Ok(usages);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting voucher usages {Code}", code);
            return BadRequest(new { error = "Lỗi hệ thống, vui lòng thử lại" });
        }
    }

    #endregion
} 