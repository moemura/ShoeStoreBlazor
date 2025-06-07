using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using WebApp.Services.Auth;

namespace WebApp.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(AuthenticationSchemes = "Bearer")]
public class AccountController : ControllerBase
{
    private readonly IAccountService _accountService;

    public AccountController(IAccountService accountService)
    {
        _accountService = accountService;
    }

    /// <summary>
    /// Đổi mật khẩu tài khoản hiện tại
    /// </summary>
    [HttpPost("change-password")]
    public async Task<ActionResult<AuthResponseDto>> ChangePassword(ChangePasswordDto model)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId))
            return Unauthorized();
        var result = await _accountService.ChangePasswordAsync(userId, model);
        if (!result.Success)
            return BadRequest(result);
        return Ok(result);
    }

    /// <summary>
    /// Cập nhật thông tin cá nhân
    /// </summary>
    [HttpPut("profile")]
    public async Task<ActionResult<AuthResponseDto>> UpdateProfile(UpdateProfileDto model)
    {
        bool authenticated = User.Identity.IsAuthenticated;
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId))
            return Unauthorized();
        var result = await _accountService.UpdateProfileAsync(userId, model);
        if (!result.Success)
            return BadRequest(result);
        return Ok(result);
    }

    /// <summary>
    /// Lấy thông tin cá nhân
    /// </summary>
    [HttpGet("profile")]
    public async Task<ActionResult<AuthResponseDto>> GetProfile()
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId))
            return Unauthorized();
        var result = await _accountService.GetProfileAsync(userId);
        if (!result.Success)
            return BadRequest(result);
        return Ok(result);
    }

    /// <summary>
    /// Xoá tài khoản (yêu cầu nhập mật khẩu)
    /// </summary>
    [HttpDelete("account")]
    public async Task<ActionResult> DeleteAccount([FromBody] string password)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId))
            return Unauthorized();
        var result = await _accountService.DeleteAccountAsync(userId, password);
        if (!result)
            return BadRequest("Sai mật khẩu hoặc không thể xoá tài khoản");
        return Ok(new { Message = "Xoá tài khoản thành công" });
    }
} 