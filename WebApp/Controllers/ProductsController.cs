using Microsoft.AspNetCore.Mvc;

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
                    [FromQuery] int? categoryId = null,
                    [FromQuery] int? brandId = null,
                    [FromQuery] decimal? minPrice = null,
                    [FromQuery] decimal? maxPrice = null)
        {
            var filter = new Dictionary<string, string>();

            if (!string.IsNullOrEmpty(search))
                filter["search"] = search;

            if (categoryId.HasValue)
                filter["categoryId"] = categoryId.Value.ToString();

            if (brandId.HasValue)
                filter["brandId"] = brandId.Value.ToString();

            if (minPrice.HasValue)
                filter["minPrice"] = minPrice.Value.ToString();

            if (maxPrice.HasValue)
                filter["maxPrice"] = maxPrice.Value.ToString();

            var data = await service.FilterAndPagin(pageIndex, pageSize, filter);
            return Ok(data);
        }
    }
}

