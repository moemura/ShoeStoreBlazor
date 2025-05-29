using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using WebApp.Models.DTOs;
using WebApp.Services.Carts;
using System.Security.Claims;

namespace WebApp.Controllers
{
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
        [HttpGet]
        [AllowAnonymous]
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
        /// Add or Update Cart item
        /// </summary>
        /// <param name="request"></param>
        /// <param name="guestId"></param>
        /// <returns></returns>
        [HttpPost("item")]
        [AllowAnonymous]
        public async Task<IActionResult> AddOrUpdateItem([FromBody] CartItemAddOrUpdateRequest request, [FromHeader(Name = "GuestId")] string guestId = null!)
        {
            var userId = User.Identity.IsAuthenticated
                ? User.FindFirstValue(ClaimTypes.NameIdentifier)
                : null;
            if (userId == null && guestId == null)
                return BadRequest();
            var key = userId ?? guestId;
            await _cartService.AddOrUpdateItem(key, request);
            return Ok();
        }
        [HttpDelete("item/{inventoryId}")]
        [AllowAnonymous]
        public async Task<IActionResult> RemoveItem([FromHeader(Name = "GuestId")] string guestId, int inventoryId)
        {
            var userId = User.Identity.IsAuthenticated
                ? User.FindFirstValue(ClaimTypes.NameIdentifier)
                : null;
            var key = userId ?? guestId;
            await _cartService.RemoveItem(key, inventoryId);
            return Ok();
        }
        [HttpDelete]
        [AllowAnonymous]
        public async Task<IActionResult> ClearCart([FromHeader(Name = "GuestId")] string guestId = null)
        {
            var userId = User.Identity.IsAuthenticated
                ? User.FindFirstValue(ClaimTypes.NameIdentifier)
                : null;
            var key = userId ?? guestId;
            await _cartService.ClearCart(key);
            return Ok();
        }
        [HttpGet("count")]
        [AllowAnonymous]
        public async Task<IActionResult> GetCartItemCount([FromHeader(Name = "GuestId")] string guestId = null)
        {
            var userId = User.Identity.IsAuthenticated
                ? User.FindFirstValue(ClaimTypes.NameIdentifier)
                : null;
            var key = userId ?? guestId;
            var count = await _cartService.GetCartItemCount(key);
            return Ok(count);
        }
        [HttpPost("merge")] // FE gửi GuestId và JWT khi login
        public async Task<IActionResult> MergeGuestCartToUser([FromHeader(Name = "GuestId")] string guestId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            await _cartService.MergeGuestCartToUser(guestId, userId);
            return Ok();
        }
    }
} 