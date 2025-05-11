using Microsoft.AspNetCore.Mvc;
using WebApp.Services.Categories;

namespace WebApp.Endpoints
{
    [Route("api/[controller]")]
    [ApiController]
    public class CategoriesController(ICategoryService service) : ControllerBase
    {
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var data = await service.GetAll() ?? [];
            return Ok(data);
        }
    }
} 