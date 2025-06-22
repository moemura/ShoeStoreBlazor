using Microsoft.AspNetCore.Mvc;
using WebApp.Services.Products;

namespace WebApp.Endpoints
{
    /// <summary>
    /// Controller quản lý sản phẩm
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class ProductsController(IProductService service) : ControllerBase
    {
        /// <summary>
        /// Lấy tất cả sản phẩm
        /// </summary>
        /// <returns>Danh sách tất cả sản phẩm</returns>
        /// <response code="200">Trả về danh sách sản phẩm thành công</response>
        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<ProductDto>), 200)]
        public async Task<IActionResult> GetAll()
        {
            var data = await service.GetAll() ?? [];
            return Ok(data);
        }

        /// <summary>
        /// Lấy sản phẩm với phân trang
        /// </summary>
        /// <param name="pageIndex">Số trang (bắt đầu từ 1)</param>
        /// <param name="pageSize">Số sản phẩm trên mỗi trang</param>
        /// <returns>Danh sách sản phẩm có phân trang</returns>
        /// <response code="200">Trả về danh sách sản phẩm với phân trang thành công</response>
        [HttpGet("pagin/")]
        [ProducesResponseType(typeof(PaginatedList<ProductDto>), 200)]
        public async Task<IActionResult> GetAll([FromQuery] int pageIndex, [FromQuery] int pageSize)
        {
            var data = await service.GetPagination(pageIndex, pageSize);
            return Ok(data);
        }

        /// <summary>
        /// Lấy thông tin chi tiết sản phẩm theo ID
        /// </summary>
        /// <param name="id">ID của sản phẩm</param>
        /// <returns>Thông tin chi tiết sản phẩm</returns>
        /// <response code="200">Trả về thông tin sản phẩm thành công</response>
        /// <response code="404">Không tìm thấy sản phẩm</response>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(ProductDto), 200)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> Get(string id)
        {
            var data = await service.GetById(id);
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
                    [FromQuery] string? maxPrice = null)
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

            var result = await service.FilterAndPagin(pageIndex, pageSize, filter);
            return Ok(result);
        }
    }
}

