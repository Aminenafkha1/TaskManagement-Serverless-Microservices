using TaskManagement.Models.DTOs;
using TaskManagement.Models;

namespace TaskManagement.AuthService.Services
{
    public interface IJwtService
    {
        string GenerateToken(User user);
        bool ValidateToken(string token);
        string? GetUserIdFromToken(string token);
        string? GetUserEmailFromToken(string token);
    }

    public interface IPasswordService
    {
        string HashPassword(string password);
        bool VerifyPassword(string password, string hashedPassword);
    }

    public interface IAuthService
    {
        Task<ApiResponseDto<AuthResultDto>> LoginAsync(LoginDto loginDto);
        Task<ApiResponseDto<AuthResultDto>> RegisterAsync(RegisterDto registerDto);
        Task<ApiResponseDto<UserDto>> GetCurrentUserAsync(string userId);
        Task<ApiResponseDto> LogoutAsync(string userId);
        Task<ApiResponseDto<AuthResultDto>> RefreshTokenAsync(string refreshToken);
        Task<bool> ValidateTokenAsync(string token);
    }
}
