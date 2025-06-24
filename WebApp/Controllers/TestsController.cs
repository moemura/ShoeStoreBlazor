using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using WebApp.Services.Promotions;

namespace WebApp.Endpoints
{
    /// <summary>
    /// Controller dùng để test API và kiểm tra trạng thái hệ thống
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class TestsController : ControllerBase
    {
        private readonly IPromotionService _promotionService;

        public TestsController(IPromotionService promotionService)
        {
            _promotionService = promotionService;
        }

        /// <summary>
        /// Kiểm tra trạng thái hoạt động của API
        /// </summary>
        /// <returns>Chuỗi "OK" nếu API hoạt động bình thường</returns>
        /// <response code="200">API hoạt động bình thường</response>
        [HttpGet]
        [ProducesResponseType(typeof(string), 200)]
        public string Get()
        {
            return "OK";
        }

        /// <summary>
        /// Test promotion calculation for a product
        /// </summary>
        /// <param name="productId">ID sản phẩm</param>
        /// <param name="originalPrice">Giá gốc</param>
        /// <returns>Giá sau khuyến mãi</returns>
        [HttpGet("promotion-price/{productId}")]
        [ProducesResponseType(typeof(object), 200)]
        public async Task<IActionResult> TestPromotionPrice(string productId, [FromQuery] double originalPrice = 1000000)
        {
            try
            {
                var promotionPrice = await _promotionService.CalculatePromotionPriceAsync(productId, originalPrice);
                var bestPromotion = await _promotionService.GetBestPromotionForProductAsync(productId);
                var promotions = await _promotionService.GetPromotionsForProductAsync(productId);

                return Ok(new
                {
                    ProductId = productId,
                    OriginalPrice = originalPrice,
                    PromotionPrice = promotionPrice,
                    Discount = originalPrice - promotionPrice,
                    HasPromotion = promotionPrice < originalPrice,
                    BestPromotion = bestPromotion,
                    AllPromotions = promotions
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { Error = ex.Message });
            }
        }

        /// <summary>
        /// Get all active promotions
        /// </summary>
        /// <returns>Danh sách promotion đang hoạt động</returns>
        [HttpGet("active-promotions")]
        [ProducesResponseType(typeof(IEnumerable<PromotionDto>), 200)]
        public async Task<IActionResult> GetActivePromotions()
        {
            try
            {
                var promotions = await _promotionService.GetActivePromotionsAsync();
                return Ok(promotions);
            }
            catch (Exception ex)
            {
                return BadRequest(new { Error = ex.Message });
            }
        }
    }
}
