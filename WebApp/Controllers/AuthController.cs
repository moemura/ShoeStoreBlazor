using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebApp.Services.Auth;
using Microsoft.AspNetCore.Identity;

namespace WebApp.Controllers;

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
    [HttpPost("register")]
    [AllowAnonymous]
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
    [HttpPost("login")]
    [AllowAnonymous]
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