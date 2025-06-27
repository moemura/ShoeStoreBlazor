using Microsoft.AspNetCore.Mvc;
using WebApp.Services.Products;
using WebApp.Services.Promotions;
using System.Linq;

namespace WebApp.Endpoints
{
    /// <summary>
    /// Controller quản lý sản phẩm
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class ProductsController(IProductService service, IPromotionService promotionService) : ControllerBase
    {
        /// <summary>
        /// Lấy tất cả sản phẩm với pricing động
        /// </summary>
        /// <param name="orderTotal">Tổng giá trị đơn hàng để tính toán promotion (tùy chọn)</param>
        /// <returns>Danh sách tất cả sản phẩm</returns>
        /// <response code="200">Trả về danh sách sản phẩm thành công</response>
        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<ProductDto>), 200)]
        public async Task<IActionResult> GetAll([FromQuery] double? orderTotal = null)
        {
            var data = orderTotal.HasValue 
                ? await service.GetAllWithDynamicPricing(orderTotal.Value)
                : await service.GetAll();
            return Ok(data);
        }

        /// <summary>
        /// Lấy sản phẩm với phân trang và pricing động
        /// </summary>
        /// <param name="pageIndex">Số trang (bắt đầu từ 1)</param>
        /// <param name="pageSize">Số sản phẩm trên mỗi trang</param>
        /// <param name="orderTotal">Tổng giá trị đơn hàng để tính toán promotion (tùy chọn)</param>
        /// <returns>Danh sách sản phẩm có phân trang</returns>
        /// <response code="200">Trả về danh sách sản phẩm với phân trang thành công</response>
        [HttpGet("pagin/")]
        [ProducesResponseType(typeof(PaginatedList<ProductDto>), 200)]
        public async Task<IActionResult> GetAll([FromQuery] int pageIndex, [FromQuery] int pageSize, [FromQuery] double? orderTotal = null)
        {
            var data = orderTotal.HasValue
                ? await service.GetPaginationWithDynamicPricing(pageIndex, pageSize, orderTotal.Value)
                : await service.GetPagination(pageIndex, pageSize);
            return Ok(data);
        }

        /// <summary>
        /// Lấy thông tin chi tiết sản phẩm theo ID với pricing động
        /// </summary>
        /// <param name="id">ID của sản phẩm</param>
        /// <param name="orderTotal">Tổng giá trị đơn hàng để tính toán promotion (tùy chọn)</param>
        /// <returns>Thông tin chi tiết sản phẩm</returns>
        /// <response code="200">Trả về thông tin sản phẩm thành công</response>
        /// <response code="404">Không tìm thấy sản phẩm</response>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(ProductDto), 200)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> Get(string id, [FromQuery] double? orderTotal = null)
        {
            var data = orderTotal.HasValue
                ? await service.GetByIdWithDynamicPricing(id, orderTotal.Value)
                : await service.GetById(id);
            return data == null ? NotFound() : Ok(data);
        }

        /// <summary>
        /// Tìm kiếm và lọc sản phẩm với phân trang
        /// </summary>
        /// <param name="pageIndex">Số trang (mặc định: 1)</param>
        /// <param name="pageSize">Số sản phẩm trên mỗi trang (mặc định: 12)</param>
        /// <param name="search">Từ khóa tìm kiếm theo tên sản phẩm</param>
        /// <param name="categoryId">ID danh mục để lọc</param>
        /// <param name="brandId">ID thương hiệu để lọc</param>
        /// <param name="minPrice">Giá tối thiểu</param>
        /// <param name="maxPrice">Giá tối đa</param>
        /// <param name="orderTotal">Tổng giá trị đơn hàng để tính toán promotion (tùy chọn)</param>
        /// <returns>Danh sách sản phẩm đã được lọc và phân trang</returns>
        /// <response code="200">Trả về danh sách sản phẩm đã lọc thành công</response>
        [HttpGet("filter")]
        [ProducesResponseType(typeof(PaginatedList<ProductDto>), 200)]
        public async Task<IActionResult> FilterAndPagin(
                    [FromQuery] int pageIndex = 1,
                    [FromQuery] int pageSize = 12,
                    [FromQuery] string? search = null,
                    [FromQuery] string? categoryId = null,
                    [FromQuery] string? brandId = null,
                    [FromQuery] string? minPrice = null,
                    [FromQuery] string? maxPrice = null,
                    [FromQuery] double? orderTotal = null)
        {
            var filter = new Dictionary<string, string>();

            if (!string.IsNullOrEmpty(search))
                filter["search"] = search;

            if (categoryId!=null)
                filter["categoryId"] = categoryId;

            if (brandId!=null)
                filter["brandId"] = brandId;

            if (minPrice!=null)
                filter["minPrice"] = minPrice;

            if (maxPrice!=null)
                filter["maxPrice"] = maxPrice;

            // Use optimized method when orderTotal is provided
            if (orderTotal.HasValue)
            {
                return Ok(await service.FilterAndPaginWithDynamicPricing(pageIndex, pageSize, filter, orderTotal.Value));
            }
            
            // Standard filtering without dynamic pricing
            var result = await service.FilterAndPagin(pageIndex, pageSize, filter);
            return Ok(result);
        }

        /// <summary>
        /// Lấy danh sách promotion áp dụng cho sản phẩm với validation đơn hàng
        /// </summary>
        /// <param name="id">ID của sản phẩm</param>
        /// <param name="orderTotal">Tổng giá trị đơn hàng để validation MinOrderAmount (tùy chọn)</param>
        /// <returns>Danh sách promotion cho sản phẩm</returns>
        /// <response code="200">Trả về danh sách promotion thành công</response>
        /// <response code="404">Không tìm thấy sản phẩm</response>
        [HttpGet("{id}/promotions")]
        [ProducesResponseType(typeof(IEnumerable<PromotionDto>), 200)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> GetProductPromotions(string id, [FromQuery] double? orderTotal = null)
        {
            var product = await service.GetById(id);
            if (product == null)
                return NotFound();

            var promotions = orderTotal.HasValue
                ? await promotionService.GetValidPromotionsForOrderAsync(new[] { id }, orderTotal.Value)
                : await promotionService.GetPromotionsForProductAsync(id);
                
            return Ok(promotions);
        }

        /// <summary>
        /// Lấy sản phẩm nổi bật cho trang chủ (sắp xếp theo lượt thích)
        /// </summary>
        /// <param name="count">Số lượng sản phẩm trả về (mặc định: 12)</param>
        /// <param name="orderTotal">Tổng giá trị đơn hàng để tính toán promotion (tùy chọn)</param>
        /// <returns>Danh sách sản phẩm nổi bật</returns>
        /// <response code="200">Trả về danh sách sản phẩm nổi bật thành công</response>
        [HttpGet("featured")]
        [ProducesResponseType(typeof(IEnumerable<ProductDto>), 200)]
        public async Task<IActionResult> GetFeaturedProducts([FromQuery] int count = 12, [FromQuery] double? orderTotal = null)
        {
            var data = orderTotal.HasValue 
                ? await service.GetAllWithDynamicPricing(orderTotal.Value)
                : await service.GetAll();
                
            // Sort by like count (best selling) and take the specified count
            var featuredProducts = data
                .OrderByDescending(p => p.LikeCount)
                .Take(count)
                .ToList();
                
            return Ok(featuredProducts);
        }
    }
}

