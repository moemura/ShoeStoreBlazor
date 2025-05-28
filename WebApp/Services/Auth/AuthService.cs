using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace WebApp.Services.Auth;

public class AuthService : IAuthService
{
    private readonly UserManager<AppUser> _userManager;
    private readonly RoleManager<IdentityRole> _roleManager;
    private readonly SignInManager<AppUser> _signInManager;
    private readonly JwtSettings _jwtSettings;
    private readonly IDbContextFactory<ShoeStoreDbContext> _dbContextFactory;
    private readonly IEmailSender _emailSender;

    public AuthService(
        UserManager<AppUser> userManager,
        RoleManager<IdentityRole> roleManager,
        SignInManager<AppUser> signInManager,
        IOptions<JwtSettings> jwtSettings,
        IDbContextFactory<ShoeStoreDbContext> dbContextFactory,
        IEmailSender emailSender)
    {
        _userManager = userManager;
        _roleManager = roleManager;
        _signInManager = signInManager;
        _jwtSettings = jwtSettings.Value;
        _dbContextFactory = dbContextFactory;
        _emailSender = emailSender;
    }

    public async Task<AuthResponseDto> RegisterAsync(RegisterDto model)
    {
        var user = new AppUser
        {
            UserName = model.Email,
            Email = model.Email,
            PhoneNumber = model.PhoneNumber,
            CreatedAt = DateTime.UtcNow
        };

        var result = await _userManager.CreateAsync(user, model.Password);
        if (!result.Succeeded)
        {
            return new AuthResponseDto
            {
                Success = false,
                Message = string.Join(", ", result.Errors.Select(e => e.Description))
            };
        }

        // Assign Customer role
        if (!await _roleManager.RoleExistsAsync("Customer"))
        {
            await _roleManager.CreateAsync(new IdentityRole("Customer"));
        }
        await _userManager.AddToRoleAsync(user, "Customer");

        // Generate tokens
        var token = await GenerateJwtTokenAsync(user);
        var refreshToken = await GenerateRefreshTokenAsync(user);

        return new AuthResponseDto
        {
            Success = true,
            Message = "Registration successful",
            Token = token,
            RefreshToken = refreshToken,
            ExpiresAt = DateTime.UtcNow.AddMinutes(_jwtSettings.ExpiryInMinutes),
            User = await MapToUserDtoAsync(user)
        };
    }

    public async Task<AuthResponseDto> LoginAsync(LoginDto model)
    {
        var user = await _userManager.FindByEmailAsync(model.Email);
        if (user == null)
        {
            return new AuthResponseDto
            {
                Success = false,
                Message = "Invalid email or password"
            };
        }

        if (!user.IsActive)
        {
            return new AuthResponseDto
            {
                Success = false,
                Message = "Account is deactivated"
            };
        }

        if (await _userManager.IsLockedOutAsync(user))
        {
            return new AuthResponseDto
            {
                Success = false,
                Message = "Account is locked. Please try again later."
            };
        }

        var result = await _signInManager.CheckPasswordSignInAsync(user, model.Password, true);
        if (!result.Succeeded)
        {
            return new AuthResponseDto
            {
                Success = false,
                Message = "Invalid email or password"
            };
        }

        // Update last login time
        user.LastLoginAt = DateTime.UtcNow;
        await _userManager.UpdateAsync(user);

        if (model.UseJwt)
        {
            // Generate JWT tokens
            var token = await GenerateJwtTokenAsync(user);
            var refreshToken = await GenerateRefreshTokenAsync(user);

            return new AuthResponseDto
            {
                Success = true,
                Message = "Login successful",
                Token = token,
                RefreshToken = refreshToken,
                ExpiresAt = DateTime.UtcNow.AddMinutes(_jwtSettings.ExpiryInMinutes),
                User = await MapToUserDtoAsync(user)
            };
        }
        else
        {
            // Use cookie authentication
            await _signInManager.SignInAsync(user, model.RememberMe);
            return new AuthResponseDto
            {
                Success = true,
                Message = "Login successful",
                User = await MapToUserDtoAsync(user)
            };
        }
    }

    public async Task<AuthResponseDto> ChangePasswordAsync(string userId, ChangePasswordDto model)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
        {
            return new AuthResponseDto
            {
                Success = false,
                Message = "User not found"
            };
        }

        var result = await _userManager.ChangePasswordAsync(user, model.CurrentPassword, model.NewPassword);
        if (!result.Succeeded)
        {
            return new AuthResponseDto
            {
                Success = false,
                Message = string.Join(", ", result.Errors.Select(e => e.Description))
            };
        }

        return new AuthResponseDto
        {
            Success = true,
            Message = "Password changed successfully",
            User = await MapToUserDtoAsync(user)
        };
    }

    public async Task<AuthResponseDto> UpdateProfileAsync(string userId, UpdateProfileDto model)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
        {
            return new AuthResponseDto
            {
                Success = false,
                Message = "User not found"
            };
        }

        user.PhoneNumber = model.PhoneNumber;
        user.FullName = model.FullName;
        user.Address = model.Address;

        var result = await _userManager.UpdateAsync(user);
        if (!result.Succeeded)
        {
            return new AuthResponseDto
            {
                Success = false,
                Message = string.Join(", ", result.Errors.Select(e => e.Description))
            };
        }

        return new AuthResponseDto
        {
            Success = true,
            Message = "Profile updated successfully",
            User = await MapToUserDtoAsync(user)
        };
    }

    public async Task<AuthResponseDto> GetProfileAsync(string userId)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
        {
            return new AuthResponseDto
            {
                Success = false,
                Message = "User not found"
            };
        }

        return new AuthResponseDto
        {
            Success = true,
            User = await MapToUserDtoAsync(user)
        };
    }

    public async Task<bool> DeleteAccountAsync(string userId, string password)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
        {
            return false;
        }

        var result = await _userManager.CheckPasswordAsync(user, password);
        if (!result)
        {
            return false;
        }

        var deleteResult = await _userManager.DeleteAsync(user);
        return deleteResult.Succeeded;
    }

    public async Task<bool> ForgotPasswordAsync(ForgotPasswordDto model)
    {
        var user = await _userManager.FindByEmailAsync(model.Email);
        if (user == null)
        {
            return false;
        }

        var token = await _userManager.GeneratePasswordResetTokenAsync(user);
        var resetLink = $"https://yourwebsite.com/reset-password?email={Uri.EscapeDataString(model.Email)}&token={Uri.EscapeDataString(token)}";
        
        // Send email with reset link
        await _emailSender.SendEmailAsync(
            model.Email,
            "Reset Password",
            $"Please reset your password by clicking <a href='{resetLink}'>here</a>");

        return true;
    }

    public async Task<bool> ResetPasswordAsync(ResetPasswordDto model)
    {
        var user = await _userManager.FindByEmailAsync(model.Email);
        if (user == null)
        {
            return false;
        }

        var result = await _userManager.ResetPasswordAsync(user, model.Token, model.NewPassword);
        return result.Succeeded;
    }

    public async Task<bool> LockAccountAsync(string userId, DateTime? lockoutEnd)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
        {
            return false;
        }

        var result = await _userManager.SetLockoutEndDateAsync(user, lockoutEnd);
        return result.Succeeded;
    }

    public async Task<bool> UnlockAccountAsync(string userId)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
        {
            return false;
        }

        var result = await _userManager.SetLockoutEndDateAsync(user, null);
        return result.Succeeded;
    }

    public async Task<bool> DeactivateAccountAsync(string userId)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
        {
            return false;
        }

        user.IsActive = false;
        var result = await _userManager.UpdateAsync(user);
        return result.Succeeded;
    }

    public async Task<bool> ReactivateAccountAsync(string userId)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
        {
            return false;
        }

        user.IsActive = true;
        var result = await _userManager.UpdateAsync(user);
        return result.Succeeded;
    }

    public async Task<AuthResponseDto> RefreshTokenAsync(string refreshToken)
    {
        using var context = await _dbContextFactory.CreateDbContextAsync();
        var token = await context.RefreshTokens
            .Include(t => t.User)
            .FirstOrDefaultAsync(t => t.Token == refreshToken && t.ExpiryDate > DateTime.UtcNow);

        if (token == null)
        {
            return new AuthResponseDto
            {
                Success = false,
                Message = "Invalid or expired refresh token"
            };
        }

        var user = token.User;
        var newToken = await GenerateJwtTokenAsync(user);
        var newRefreshToken = await GenerateRefreshTokenAsync(user);

        return new AuthResponseDto
        {
            Success = true,
            Message = "Token refreshed successfully",
            Token = newToken,
            RefreshToken = newRefreshToken,
            ExpiresAt = DateTime.UtcNow.AddMinutes(_jwtSettings.ExpiryInMinutes),
            User = new UserDto
            {
                Id = user.Id,
                Email = user.Email!,
                PhoneNumber = user.PhoneNumber!,
                Roles = (await _userManager.GetRolesAsync(user)).ToList()
            }
        };
    }

    public async Task<bool> RevokeTokenAsync(string refreshToken)
    {
        using var context = await _dbContextFactory.CreateDbContextAsync();
        var token = await context.RefreshTokens.FirstOrDefaultAsync(t => t.Token == refreshToken);
        if (token == null)
        {
            return false;
        }

        context.RefreshTokens.Remove(token);
        await context.SaveChangesAsync();
        return true;
    }

    private async Task<string> GenerateJwtTokenAsync(AppUser user)
    {
        var roles = await _userManager.GetRolesAsync(user);
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id),
            new(ClaimTypes.Email, user.Email!),
            new(ClaimTypes.Name, user.UserName!),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        claims.AddRange(roles.Select(role => new Claim(ClaimTypes.Role, role)));

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.SecretKey));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var expires = DateTime.UtcNow.AddMinutes(_jwtSettings.ExpiryInMinutes);

        var token = new JwtSecurityToken(
            issuer: _jwtSettings.Issuer,
            audience: _jwtSettings.Audience,
            claims: claims,
            expires: expires,
            signingCredentials: creds
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    private async Task<string> GenerateRefreshTokenAsync(AppUser user)
    {
        using var context = await _dbContextFactory.CreateDbContextAsync();
        
        // Remove any existing refresh tokens for this user
        var existingTokens = await context.RefreshTokens
            .Where(t => t.UserId == user.Id)
            .ToListAsync();
        context.RefreshTokens.RemoveRange(existingTokens);

        // Generate new refresh token
        var refreshToken = new RefreshToken
        {
            UserId = user.Id,
            Token = Guid.NewGuid().ToString(),
            ExpiryDate = DateTime.UtcNow.AddDays(7),
            CreatedAt = DateTime.UtcNow
        };

        context.RefreshTokens.Add(refreshToken);
        await context.SaveChangesAsync();

        return refreshToken.Token;
    }

    private async Task<UserDto> MapToUserDtoAsync(AppUser user)
    {
        return new UserDto
        {
            Id = user.Id,
            Email = user.Email!,
            PhoneNumber = user.PhoneNumber!,
            FullName = user.FullName,
            Address = user.Address,
            IsLocked = await _userManager.IsLockedOutAsync(user),
            LockoutEnd = (await _userManager.GetLockoutEndDateAsync(user))?.UtcDateTime,
            Roles = (await _userManager.GetRolesAsync(user)).ToList()
        };
    }
} 