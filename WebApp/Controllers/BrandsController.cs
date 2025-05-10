using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using WebApp.Data.Interfaces;

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

        [HttpGet("pagination/{index}&{size}")]
        public async Task<IActionResult> GetAll(int index, int size)
        {
            var data = await service.GetPagination(index, size);
            return Ok(data);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> Get(string id)
        {
            var data = await service.GetById(id);
            return data == null ? NotFound() : Ok(data);
        }
    }
} 