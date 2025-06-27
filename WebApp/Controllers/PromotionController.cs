using Microsoft.AspNetCore.Mvc;
using WebApp.Services.Promotions;

namespace WebApp.Controllers
{
    /// <summary>
    /// Controller quản lý khuyến mãi cho frontend
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class PromotionController(IPromotionService promotionService) : ControllerBase
    {
        /// <summary>
        /// Lấy danh sách khuyến mãi đang hoạt động
        /// </summary>
        /// <returns>Danh sách khuyến mãi active</returns>
        /// <response code="200">Trả về danh sách khuyến mãi thành công</response>
        [HttpGet("active")]
        [ProducesResponseType(typeof(IEnumerable<PromotionDto>), 200)]
        public async Task<IActionResult> GetActivePromotions()
        {
            var promotions = await promotionService.GetActivePromotionsAsync();
            return Ok(promotions);
        }

        /// <summary>
        /// Lấy chi tiết khuyến mãi theo ID
        /// </summary>
        /// <param name="id">ID của khuyến mãi</param>
        /// <returns>Chi tiết khuyến mãi</returns>
        /// <response code="200">Trả về chi tiết khuyến mãi thành công</response>
        /// <response code="404">Không tìm thấy khuyến mãi</response>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(PromotionDto), 200)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> GetPromotion(string id)
        {
            var promotion = await promotionService.GetByIdAsync(id);
            return promotion == null ? NotFound() : Ok(promotion);
        }

        /// <summary>
        /// Lấy khuyến mãi cho carousel trang chủ (chỉ những khuyến mãi có MinOrderAmount phù hợp)
        /// </summary>
        /// <returns>Danh sách khuyến mãi cho carousel</returns>
        /// <response code="200">Trả về danh sách khuyến mãi carousel thành công</response>
        [HttpGet("carousel")]
        [ProducesResponseType(typeof(IEnumerable<PromotionDto>), 200)]
        public async Task<IActionResult> GetCarouselPromotions()
        {
            var allPromotions = await promotionService.GetActivePromotionsAsync();
            
            // Filter for carousel: prioritize promotions with lower or no MinOrderAmount
            var carouselPromotions = allPromotions
                .Where(p => p.MinOrderAmount == null || p.MinOrderAmount <= 1000000) // 1M VND or less
                .OrderBy(p => p.Priority)
                .ThenBy(p => p.MinOrderAmount ?? 0)
                .Take(5) // Limit to 5 promotions for carousel
                .ToList();
                
            return Ok(carouselPromotions);
        }
    }
} 