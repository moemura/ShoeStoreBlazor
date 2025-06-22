using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace WebApp.Endpoints
{
    /// <summary>
    /// Controller dùng để test API và kiểm tra trạng thái hệ thống
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class TestsController : ControllerBase
    {
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
    }
}
