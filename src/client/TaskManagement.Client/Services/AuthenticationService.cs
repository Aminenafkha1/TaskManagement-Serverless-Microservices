using Microsoft.JSInterop;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Net.Http.Json;

namespace TaskManagement.Client.Services
{
    public interface IAuthenticationService
    {
        Task<bool> IsAuthenticatedAsync();
        Task<string?> GetTokenAsync();
        Task<string?> GetUserIdAsync();
        Task<UserInfo?> GetCurrentUserAsync();
        Task<AuthResult?> LoginAsync(LoginRequest request);
        Task<AuthResult?> RegisterAsync(RegisterRequest request);
        Task LogoutAsync();
    }

    public class AuthenticationService : IAuthenticationService
    {
        private readonly HttpClient _httpClient;
        private readonly IJSRuntime _jsRuntime;
        private readonly string _baseApiUrl = "http://localhost:7071/api";

        public AuthenticationService(HttpClient httpClient, IJSRuntime jsRuntime)
        {
            _httpClient = httpClient;
            _jsRuntime = jsRuntime;
        }

        public async Task<bool> IsAuthenticatedAsync()
        {
            var token = await GetTokenAsync();
            return !string.IsNullOrEmpty(token);
        }

        public async Task<string?> GetTokenAsync()
        {
            try
            {
                return await _jsRuntime.InvokeAsync<string>("localStorage.getItem", "authToken");
            }
            catch
            {
                return null;
            }
        }

        public async Task<string?> GetUserIdAsync()
        {
            try
            {
                return await _jsRuntime.InvokeAsync<string>("localStorage.getItem", "userId");
            }
            catch
            {
                return null;
            }
        }

        public async Task<UserInfo?> GetCurrentUserAsync()
        {
            try
            {
                var userJson = await _jsRuntime.InvokeAsync<string>("localStorage.getItem", "currentUser");
                if (!string.IsNullOrEmpty(userJson))
                {
                    return JsonSerializer.Deserialize<UserInfo>(userJson, GetJsonOptions());
                }
                return null;
            }
            catch
            {
                return null;
            }
        }

        public async Task<AuthResult?> LoginAsync(LoginRequest request)
        {
            try
            {
                var response = await _httpClient.PostAsJsonAsync($"{_baseApiUrl}/auth/login", request);
                
                if (response.IsSuccessStatusCode)
                {
                    var result = await response.Content.ReadFromJsonAsync<ApiResponse<AuthResult>>(GetJsonOptions());
                    
                    if (result?.Success == true && result.Data?.Token != null)
                    {
                        await StoreAuthDataAsync(result.Data);
                        return result.Data;
                    }
                }
                
                return null;
            }
            catch
            {
                return null;
            }
        }

        public async Task<AuthResult?> RegisterAsync(RegisterRequest request)
        {
            try
            {
                var response = await _httpClient.PostAsJsonAsync($"{_baseApiUrl}/auth/register", request);
                
                if (response.IsSuccessStatusCode)
                {
                    var result = await response.Content.ReadFromJsonAsync<ApiResponse<AuthResult>>(GetJsonOptions());
                    
                    if (result?.Success == true && result.Data?.Token != null)
                    {
                        await StoreAuthDataAsync(result.Data);
                        return result.Data;
                    }
                }
                
                return null;
            }
            catch
            {
                return null;
            }
        }

        public async Task LogoutAsync()
        {
            await _jsRuntime.InvokeVoidAsync("localStorage.removeItem", "authToken");
            await _jsRuntime.InvokeVoidAsync("localStorage.removeItem", "userId");
            await _jsRuntime.InvokeVoidAsync("localStorage.removeItem", "currentUser");
        }

        private async Task StoreAuthDataAsync(AuthResult authResult)
        {
            await _jsRuntime.InvokeVoidAsync("localStorage.setItem", "authToken", authResult.Token);
            await _jsRuntime.InvokeVoidAsync("localStorage.setItem", "userId", authResult.UserId);
            
            if (authResult.User != null)
            {
                var userJson = JsonSerializer.Serialize(authResult.User, GetJsonOptions());
                await _jsRuntime.InvokeVoidAsync("localStorage.setItem", "currentUser", userJson);
            }
        }

        private static JsonSerializerOptions GetJsonOptions()
        {
            return new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                Converters = { new JsonStringEnumConverter() }
            };
        }
    }

    // DTOs
    public class LoginRequest
    {
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }

    public class RegisterRequest
    {
        public string Email { get; set; } = string.Empty;
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public UserRole Role { get; set; } = UserRole.User;
    }

    public class ApiResponse<T>
    {
        public bool Success { get; set; }
        public T? Data { get; set; }
        public string? Message { get; set; }
        public List<string>? Errors { get; set; }
    }

    public class AuthResult
    {
        public string? Token { get; set; }
        public string? RefreshToken { get; set; }
        public string? UserId { get; set; }
        public DateTime ExpiresAt { get; set; }
        public UserInfo? User { get; set; }
        public bool Success { get; set; }
    }

    public class UserInfo
    {
        public string? Id { get; set; }
        public string? Email { get; set; }
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public UserRole Role { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public string? ProfileImageUrl { get; set; }
        public string FullName => $"{FirstName} {LastName}";
    }

    public enum UserRole
    {
        User = 0,
        Manager = 1,
        Admin = 2
    }
}
