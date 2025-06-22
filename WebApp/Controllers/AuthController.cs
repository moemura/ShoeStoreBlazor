using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebApp.Services.Auth;
using Microsoft.AspNetCore.Identity;

namespace WebApp.Controllers;

/// <summary>
/// Controller xử lý xác thực và phân quyền người dùng
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize(AuthenticationSchemes = "Bearer")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;
    private readonly UserManager<AppUser> _userManager;
    private readonly SignInManager<AppUser> _signInManager;
    private readonly ILogger<AuthController> _logger;

    public AuthController(IAuthService authService, UserManager<AppUser> userManager, SignInManager<AppUser> signInManager, ILogger<AuthController> logger)
    {
        _authService = authService;
        _userManager = userManager;
        _signInManager = signInManager;
        _logger = logger;
    }

    /// <summary>
    /// Đăng ký tài khoản mới (Customer)
    /// </summary>
    /// <param name="model">Thông tin đăng ký tài khoản</param>
    /// <returns>Thông tin xác thực sau khi đăng ký thành công</returns>
    /// <response code="200">Đăng ký thành công</response>
    /// <response code="400">Thông tin đăng ký không hợp lệ</response>
    [HttpPost("register")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(AuthResponseDto), 200)]
    [ProducesResponseType(typeof(AuthResponseDto), 400)]
    public async Task<ActionResult<AuthResponseDto>> Register(RegisterDto model)
    {
        var result = await _authService.RegisterAsync(model);
        if (!result.Success)
        {
            return BadRequest(result);
        }
        return Ok(result);
    }

    /// <summary>
    /// Đăng nhập và nhận JWT token
    /// </summary>
    /// <param name="model">Thông tin đăng nhập (email/username và password)</param>
    /// <returns>JWT token và thông tin người dùng</returns>
    /// <response code="200">Đăng nhập thành công</response>
    /// <response code="400">Thông tin đăng nhập không chính xác</response>
    [HttpPost("login")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(AuthResponseDto), 200)]
    [ProducesResponseType(typeof(AuthResponseDto), 400)]
    public async Task<ActionResult<AuthResponseDto>> Login(LoginDto model)
    {
        // Luôn sử dụng JWT cho API
        model.UseJwt = true;
        var result = await _authService.LoginAsync(model);
        if (!result.Success)
        {
            return BadRequest(result);
        }
        return Ok(result);
    }

    /// <summary>
    /// Làm mới JWT token
    /// </summary>
    [HttpPost("refresh-token")]
    [AllowAnonymous]
    public async Task<ActionResult<AuthResponseDto>> RefreshToken([FromBody] string refreshToken)
    {
        var result = await _authService.RefreshTokenAsync(refreshToken);
        if (!result.Success)
        {
            return BadRequest(result);
        }
        return Ok(result);
    }

    /// <summary>
    /// Hủy refresh token
    /// </summary>
    [HttpPost("revoke-token")]
    public async Task<ActionResult> RevokeToken([FromBody] string refreshToken)
    {
        var result = await _authService.RevokeTokenAsync(refreshToken);
        if (!result)
        {
            return BadRequest("Invalid refresh token");
        }
        return Ok();
    }

    /// <summary>
    /// Quên mật khẩu
    /// </summary>
    [HttpPost("forgot-password")]
    [AllowAnonymous]
    public async Task<ActionResult> ForgotPassword(ForgotPasswordDto model)
    {
        var result = await _authService.ForgotPasswordAsync(model);
        if (!result)
        {
            return BadRequest("Failed to process forgot password request");
        }
        return Ok(new { Message = "Password reset instructions have been sent to your email" });
    }

    /// <summary>
    /// Đặt lại mật khẩu
    /// </summary>
    [HttpPost("reset-password")]
    [AllowAnonymous]
    public async Task<ActionResult> ResetPassword(ResetPasswordDto model)
    {
        var result = await _authService.ResetPasswordAsync(model);
        if (!result)
        {
            return BadRequest("Failed to reset password");
        }
        return Ok(new { Message = "Password has been reset successfully" });
    }
} 