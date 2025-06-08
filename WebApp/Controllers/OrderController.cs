using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebApp.Services.Orders;
using System.Security.Claims;

namespace WebApp.Controllers;

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
    [HttpPost]
    [AllowAnonymous]
    public async Task<IActionResult> CreateOrder([FromBody] OrderCreateRequest req, [FromHeader(Name = "GuestId")] string guestId = null)
    {
        var userId = User.Identity.IsAuthenticated ? User.FindFirst(ClaimTypes.NameIdentifier)?.Value : null;
        var order = await _orderService.CreateOrder(req, userId, guestId);
        return Ok(order);
    }
    [HttpGet]
    public async Task<IActionResult> GetOrdersByUser()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var orders = await _orderService.GetOrdersByUser(userId);
        return Ok(orders);
    }
    [HttpGet("{id}")]
    public async Task<IActionResult> GetOrderById(Guid id)
    {
        var order = await _orderService.GetOrderById(id);
        return Ok(order);
    }
    
    [HttpPost("{id}/cancel")]
    public async Task<IActionResult> CancelOrder(Guid id)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        await _orderService.CancelOrder(id, userId);
        return NoContent();
    }

    [HttpGet("sync")]
    public async Task<IActionResult> SyncGuestOrderToUser([FromHeader(Name = "GuestId")] string guestId)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        await _orderService.SyncGuestOrdersToUser(guestId, userId);
        return Ok();
    }
}