using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using WebApp.Services.Brands;

namespace WebApp.Endpoints
{
    /// <summary>
    /// Controller quản lý thương hiệu sản phẩm
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class BrandsController(IBrandService service) : ControllerBase
    {
        /// <summary>
        /// Lấy danh sách tất cả thương hiệu
        /// </summary>
        /// <returns>Danh sách thương hiệu</returns>
        /// <response code="200">Trả về danh sách thương hiệu thành công</response>
        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<BrandDto>), 200)]
        public async Task<IActionResult> GetAll()
        {
            var data = await service.GetAll() ?? [];
            return Ok(data);
        }
    }
} 