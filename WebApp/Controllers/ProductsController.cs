using Microsoft.AspNetCore.Mvc;
using WebApp.Services.Products;

namespace WebApp.Endpoints
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductsController(IProductService service) : ControllerBase
    {
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var data = await service.GetAll() ?? [];
            return Ok(data);
        }

        [HttpGet("pagin/")]
        public async Task<IActionResult> GetAll([FromQuery] int pageIndex, [FromQuery] int pageSize)
        {
            var data = await service.GetPagination(pageIndex, pageSize);
            return Ok(data);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> Get(string id)
        {
            var data = await service.GetById(id);
            return data == null ? NotFound() : Ok(data);
        }

        [HttpGet("filter")]
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

            var data = await service.FilterAndPagin(pageIndex, pageSize, filter);
            return Ok(data);
        }
    }
}

