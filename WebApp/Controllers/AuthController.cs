using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using WebApp.Models.DTOs;
using WebApp.Services.Auth;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using WebApp.Models.Entities;

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
    /// Đổi mật khẩu
    /// </summary>
    [HttpPost("change-password")]
    public async Task<ActionResult<AuthResponseDto>> ChangePassword(ChangePasswordDto model)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized();
        }

        var result = await _authService.ChangePasswordAsync(userId, model);
        if (!result.Success)
        {
            return BadRequest(result);
        }
        return Ok(result);
    }

    /// <summary>
    /// Cập nhật thông tin cá nhân
    /// </summary>
    [HttpPut("profile")]
    public async Task<ActionResult<AuthResponseDto>> UpdateProfile(UpdateProfileDto model)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized();
        }

        var result = await _authService.UpdateProfileAsync(userId, model);
        if (!result.Success)
        {
            return BadRequest(result);
        }
        return Ok(result);
    }

    /// <summary>
    /// Xem thông tin cá nhân
    /// </summary>
    [HttpGet("profile")]
    public async Task<ActionResult<AuthResponseDto>> GetProfile()
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized();
        }

        var result = await _authService.GetProfileAsync(userId);
        if (!result.Success)
        {
            return BadRequest(result);
        }
        return Ok(result);
    }

    /// <summary>
    /// Xóa tài khoản
    /// </summary>
    [HttpDelete("account")]
    public async Task<ActionResult> DeleteAccount([FromBody] string password)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized();
        }

        var result = await _authService.DeleteAccountAsync(userId, password);
        if (!result)
        {
            return BadRequest("Invalid password or account deletion failed");
        }

        return Ok(new { Message = "Account deleted successfully" });
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

    [Authorize(Roles = "Admin")]
    [HttpPost("lock-account/{userId}")]
    public async Task<ActionResult> LockAccount(string userId, [FromBody] DateTime? lockoutEnd)
    {
        var result = await _authService.LockAccountAsync(userId, lockoutEnd);
        if (!result)
        {
            return BadRequest("Failed to lock account");
        }
        return Ok(new { Message = "Account locked successfully" });
    }

    [Authorize(Roles = "Admin")]
    [HttpPost("unlock-account/{userId}")]
    public async Task<ActionResult> UnlockAccount(string userId)
    {
        var result = await _authService.UnlockAccountAsync(userId);
        if (!result)
        {
            return BadRequest("Failed to unlock account");
        }
        return Ok(new { Message = "Account unlocked successfully" });
    }

    [Authorize(Roles = "Admin")]
    [HttpPost("deactivate-account/{userId}")]
    public async Task<ActionResult> DeactivateAccount(string userId)
    {
        var result = await _authService.DeactivateAccountAsync(userId);
        if (!result)
        {
            return BadRequest("Failed to deactivate account");
        }
        return Ok(new { Message = "Account deactivated successfully" });
    }

    [Authorize(Roles = "Admin")]
    [HttpPost("reactivate-account/{userId}")]
    public async Task<ActionResult> ReactivateAccount(string userId)
    {
        var result = await _authService.ReactivateAccountAsync(userId);
        if (!result)
        {
            return BadRequest("Failed to reactivate account");
        }
        return Ok(new { Message = "Account reactivated successfully" });
    }

    [HttpPost("admin-login")]
    [AllowAnonymous]
    public async Task<ActionResult<AuthResponseDto>> AdminLogin([FromBody] AdminLoginDto model)
    {
        try
        {
            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user == null)
            {
                return BadRequest(new AuthResponseDto { Success = false, Message = "Invalid email or password" });
            }

            if (!await _userManager.IsInRoleAsync(user, "Admin"))
            {
                return BadRequest(new AuthResponseDto { Success = false, Message = "You do not have permission to access the admin area" });
            }

            if (!user.IsActive)
            {
                return BadRequest(new AuthResponseDto { Success = false, Message = "Your account has been deactivated" });
            }

            if (await _userManager.IsLockedOutAsync(user))
            {
                return BadRequest(new AuthResponseDto { Success = false, Message = "Your account has been locked. Please try again later." });
            }

            var result = await _signInManager.PasswordSignInAsync(
                user,
                model.Password,
                model.RememberMe,
                lockoutOnFailure: true);

            if (result.Succeeded)
            {
                user.LastLoginAt = DateTime.UtcNow;
                await _userManager.UpdateAsync(user);

                var claims = await _userManager.GetClaimsAsync(user);
                var roles = await _userManager.GetRolesAsync(user);
                var roleClaims = roles.Select(r => new Claim(ClaimTypes.Role, r));
                var userClaims = claims.Concat(roleClaims);

                var authProperties = new AuthenticationProperties
                {
                    IsPersistent = model.RememberMe,
                    ExpiresUtc = model.RememberMe ? DateTimeOffset.UtcNow.AddDays(30) : DateTimeOffset.UtcNow.AddHours(1)
                };

                await _signInManager.SignInWithClaimsAsync(user, authProperties, userClaims);

                return Ok(new AuthResponseDto 
                { 
                    Success = true,
                    Message = "Login successful",
                    User = new UserDto
                    {
                        Id = user.Id,
                        Email = user.Email,
                        FullName = user.FullName,
                        PhoneNumber = user.PhoneNumber,
                        Address = user.Address,
                        IsLocked = await _userManager.IsLockedOutAsync(user),
                        LockoutEnd = user.LockoutEnd?.DateTime,
                        Roles = await _userManager.GetRolesAsync(user)
                    }
                });
            }
            else if (result.IsLockedOut)
            {
                return BadRequest(new AuthResponseDto { Success = false, Message = "Your account has been locked due to multiple failed login attempts. Please try again later." });
            }
            else
            {
                return BadRequest(new AuthResponseDto { Success = false, Message = "Invalid email or password" });
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during admin login");
            return StatusCode(500, new AuthResponseDto { Success = false, Message = "An error occurred during login" });
        }
    }
} 