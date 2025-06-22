using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using WebApp.Models.DTOs;
using WebApp.Services.Carts;
using System.Security.Claims;

namespace WebApp.Controllers
{
    /// <summary>
    /// Controller quản lý giỏ hàng
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(AuthenticationSchemes = "Bearer")]
    public class CartController : ControllerBase
    {
        private readonly ICartService _cartService;
        public CartController(ICartService cartService)
        {
            _cartService = cartService;
        }
        /// <summary>
        /// Lấy thông tin giỏ hàng
        /// </summary>
        /// <param name="guestId">ID khách vãng lai (optional)</param>
        /// <returns>Thông tin giỏ hàng</returns>
        /// <response code="200">Trả về thông tin giỏ hàng thành công</response>
        [HttpGet]
        [AllowAnonymous]
        [ProducesResponseType(typeof(CartDto), 200)]
        public async Task<IActionResult> GetCart([FromHeader(Name = "GuestId")] string guestId = null)
        {
            var userId = User.Identity.IsAuthenticated
                ? User.FindFirstValue(ClaimTypes.NameIdentifier)
                : null;
            var key = userId ?? guestId;
            var cart = await _cartService.GetCart(key);
            return Ok(cart);
        }

        /// <summary>
        /// Thêm hoặc cập nhật sản phẩm trong giỏ hàng
        /// </summary>
        /// <param name="request">Thông tin sản phẩm cần thêm/cập nhật</param>
        /// <param name="guestId">ID khách vãng lai (optional)</param>
        /// <returns>Kết quả thêm/cập nhật sản phẩm</returns>
        /// <response code="200">Thêm/cập nhật sản phẩm thành công</response>
        /// <response code="400">Thông tin không hợp lệ</response>
        [HttpPost("item")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(CartItemDto), 200)]
        [ProducesResponseType(400)]
        public async Task<IActionResult> AddOrUpdateItem([FromBody] CartItemAddOrUpdateRequest request, [FromHeader(Name = "GuestId")] string guestId = null!)
        {
            var userId = User.Identity.IsAuthenticated
                ? User.FindFirstValue(ClaimTypes.NameIdentifier)
                : null;
            if (userId == null && guestId == null)
                return BadRequest();
            var key = userId ?? guestId;
            var result = await _cartService.AddOrUpdateItem(key, request);
            return Ok(result);
        }
        /// <summary>
        /// Xóa sản phẩm khỏi giỏ hàng
        /// </summary>
        /// <param name="guestId">ID khách vãng lai (optional)</param>
        /// <param name="inventoryId">ID inventory của sản phẩm cần xóa</param>
        /// <returns>Kết quả xóa sản phẩm</returns>
        /// <response code="200">Xóa sản phẩm thành công</response>
        [HttpDelete("item/{inventoryId}")]
        [AllowAnonymous]
        [ProducesResponseType(200)]
        public async Task<IActionResult> RemoveItem([FromHeader(Name = "GuestId")] string? guestId, int inventoryId)
        {
            var userId = User.Identity.IsAuthenticated
                ? User.FindFirstValue(ClaimTypes.NameIdentifier)
                : null;
            var key = userId ?? guestId;
            await _cartService.RemoveItem(key, inventoryId);
            return Ok();
        }
        
        /// <summary>
        /// Xóa toàn bộ giỏ hàng
        /// </summary>
        /// <param name="guestId">ID khách vãng lai (optional)</param>
        /// <returns>Kết quả xóa giỏ hàng</returns>
        /// <response code="200">Xóa giỏ hàng thành công</response>
        [HttpDelete]
        [AllowAnonymous]
        [ProducesResponseType(200)]
        public async Task<IActionResult> ClearCart([FromHeader(Name = "GuestId")] string? guestId = null)
        {
            var userId = User.Identity.IsAuthenticated
                ? User.FindFirstValue(ClaimTypes.NameIdentifier)
                : null;
            var key = userId ?? guestId;
            await _cartService.ClearCart(key);
            return Ok();
        }
        
        /// <summary>
        /// Lấy số lượng sản phẩm trong giỏ hàng
        /// </summary>
        /// <param name="guestId">ID khách vãng lai (optional)</param>
        /// <returns>Số lượng sản phẩm trong giỏ hàng</returns>
        /// <response code="200">Trả về số lượng sản phẩm thành công</response>
        [HttpGet("count")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(int), 200)]
        public async Task<IActionResult> GetCartItemCount([FromHeader(Name = "GuestId")] string? guestId = null)
        {
            var userId = User.Identity.IsAuthenticated
                ? User.FindFirstValue(ClaimTypes.NameIdentifier)
                : null;
            var key = userId ?? guestId;
            var count = await _cartService.GetCartItemCount(key);
            return Ok(count);
        }
        
        /// <summary>
        /// Hợp nhất giỏ hàng khách vãng lai vào tài khoản người dùng
        /// </summary>
        /// <param name="guestId">ID khách vãng lai</param>
        /// <returns>Kết quả hợp nhất giỏ hàng</returns>
        /// <response code="200">Hợp nhất giỏ hàng thành công</response>
        [HttpPost("merge")]
        [ProducesResponseType(200)]
        public async Task<IActionResult> MergeGuestCartToUser([FromHeader(Name = "GuestId")] string guestId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            await _cartService.MergeGuestCartToUser(guestId, userId);
            return Ok();
        }
    }
} 