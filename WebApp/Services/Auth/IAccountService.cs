using WebApp.Models.DTOs;

namespace WebApp.Services.Auth;

public interface IAccountService
{
    Task<AuthResponseDto> ChangePasswordAsync(string userId, ChangePasswordDto model);
    Task<AuthResponseDto> UpdateProfileAsync(string userId, UpdateProfileDto model);
    Task<AuthResponseDto> GetProfileAsync(string userId);
    Task<bool> DeleteAccountAsync(string userId, string password);
    Task<bool> LockAccountAsync(string userId, DateTime? lockoutEnd);
    Task<bool> UnlockAccountAsync(string userId);
    Task<bool> DeactivateAccountAsync(string userId);
    Task<bool> ReactivateAccountAsync(string userId);
    Task<PaginatedList<UserDto>> FilterAndPagingUsers(int pageIndex, int pageSize, Dictionary<string, string> filter);
} 