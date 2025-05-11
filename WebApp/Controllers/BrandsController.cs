using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using WebApp.Services.Brands;

namespace WebApp.Endpoints
{
    [Route("api/[controller]")]
    [ApiController]
    public class BrandsController(IBrandService service) : ControllerBase
    {
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var data = await service.GetAll() ?? [];
            return Ok(data);
        }
    }
} 