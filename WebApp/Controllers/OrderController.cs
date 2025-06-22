using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebApp.Services.Orders;
using System.Security.Claims;

namespace WebApp.Controllers;

/// <summary>
/// Controller quản lý đơn hàng
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize(AuthenticationSchemes = "Bearer")]
public class OrderController : ControllerBase
{
    private readonly IOrderService _orderService;
    public OrderController(IOrderService orderService)
    {
        _orderService = orderService;
    }
    /// <summary>
    /// Tạo đơn hàng mới
    /// </summary>
    /// <param name="req">Thông tin đơn hàng cần tạo</param>
    /// <param name="guestId">ID khách vãng lai (optional)</param>
    /// <returns>Thông tin đơn hàng và kết quả thanh toán</returns>
    /// <response code="200">Tạo đơn hàng thành công</response>
    /// <response code="400">Thông tin đơn hàng không hợp lệ</response>
    [HttpPost]
    [AllowAnonymous]
    [ProducesResponseType(typeof(object), 200)]
    [ProducesResponseType(typeof(object), 400)]
    public async Task<IActionResult> CreateOrder([FromBody] OrderCreateRequest req, [FromHeader(Name = "GuestId")] string guestId = null)
    {
        try
        {
            var userId = User.Identity.IsAuthenticated ? User.FindFirst(ClaimTypes.NameIdentifier)?.Value : null;
            var result = await _orderService.CreateOrder(req, userId, guestId);
            
            // Return structured response for frontend
            var response = new
            {
                success = true,
                order = result.Order,
                paymentResult = result.PaymentResult
            };
            
            return Ok(response);
        }
        catch (Exception ex)
        {
            return BadRequest(new 
            { 
                success = false, 
                error = ex.Message 
            });
        }
    }
    /// <summary>
    /// Lấy danh sách đơn hàng của người dùng hiện tại
    /// </summary>
    /// <returns>Danh sách đơn hàng của người dùng</returns>
    /// <response code="200">Trả về danh sách đơn hàng thành công</response>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<OrderDto>), 200)]
    public async Task<IActionResult> GetOrdersByUser()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var orders = await _orderService.GetOrdersByUser(userId);
        return Ok(orders);
    }
    
    /// <summary>
    /// Lấy thông tin chi tiết đơn hàng theo ID
    /// </summary>
    /// <param name="id">ID của đơn hàng</param>
    /// <returns>Thông tin chi tiết đơn hàng</returns>
    /// <response code="200">Trả về thông tin đơn hàng thành công</response>
    /// <response code="404">Không tìm thấy đơn hàng</response>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(OrderDto), 200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> GetOrderById(Guid id)
    {
        var order = await _orderService.GetOrderById(id);
        return Ok(order);
    }
    
    /// <summary>
    /// Hủy đơn hàng
    /// </summary>
    /// <param name="id">ID của đơn hàng cần hủy</param>
    /// <returns>Kết quả hủy đơn hàng</returns>
    /// <response code="204">Hủy đơn hàng thành công</response>
    /// <response code="400">Không thể hủy đơn hàng</response>
    [HttpPost("{id}/cancel")]
    [ProducesResponseType(204)]
    [ProducesResponseType(400)]
    public async Task<IActionResult> CancelOrder(Guid id)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        await _orderService.CancelOrder(id, userId);
        return NoContent();
    }

    /// <summary>
    /// Đồng bộ đơn hàng từ khách vãng lai sang tài khoản người dùng
    /// </summary>
    /// <param name="guestId">ID khách vãng lai</param>
    /// <returns>Kết quả đồng bộ</returns>
    /// <response code="200">Đồng bộ thành công</response>
    [HttpGet("sync")]
    [ProducesResponseType(200)]
    public async Task<IActionResult> SyncGuestOrderToUser([FromHeader(Name = "GuestId")] string guestId)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        await _orderService.SyncGuestOrdersToUser(guestId, userId);
        return Ok();
    }
}