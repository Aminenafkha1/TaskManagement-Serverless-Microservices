using Microsoft.Extensions.Logging;
using System.Text.Json;
using System.Text.Json.Serialization;
using TaskManagement.Models;
using TaskManagement.Models.DTOs;
using TaskManagement.Models.Events;
using TaskManagement.Shared.Services; 

namespace TaskManagement.UsersService.Services
{
    public class AuthService : IAuthService
    {
        private readonly ILogger<AuthService> _logger;
        private readonly IPersistenceService _cosmosService;
        private readonly IJwtService _jwtService;
        private readonly IPasswordService _passwordService;
        private readonly IEventPublisher _eventPublisher;
        private const string USERS_CONTAINER = "users";

        public AuthService(
            ILogger<AuthService> logger,
            IPersistenceService cosmosService,
            IJwtService jwtService,
            IPasswordService passwordService,
            IEventPublisher eventPublisher)
        {
            _logger = logger;
            _cosmosService = cosmosService;
            _jwtService = jwtService;
            _passwordService = passwordService;
            _eventPublisher = eventPublisher;
        }

        public async Task<ApiResponseDto<AuthResultDto>> LoginAsync(LoginDto loginDto)
        {
            try
            {
                _logger.LogInformation("Attempting login for email: {Email}", loginDto.Email);

                // Find user by email
                var users = await _cosmosService.GetAllAsync<User>(USERS_CONTAINER, 
                    $"SELECT * FROM c WHERE c.email = '{loginDto.Email}'");
                
                var user = users.FirstOrDefault();
                if (user == null || string.IsNullOrEmpty(user.PasswordHash))
                {
                    _logger.LogWarning("Login failed - user not found or no password: {Email}", loginDto.Email);
                    return new ApiResponseDto<AuthResultDto>
                    {
                        Success = false,
                        Message = "Invalid email or password",
                        Errors = new List<string> { "Authentication failed" }
                    };
                }

                // Verify password
                if (!_passwordService.VerifyPassword(loginDto.Password, user.PasswordHash))
                {
                    _logger.LogWarning("Login failed - invalid password: {Email}", loginDto.Email);
                    return new ApiResponseDto<AuthResultDto>
                    {
                        Success = false,
                        Message = "Invalid email or password",
                        Errors = new List<string> { "Authentication failed" }
                    };
                }

                // Generate JWT token
                var token = _jwtService.GenerateToken(user);
                var refreshToken = Guid.NewGuid().ToString(); // Simple refresh token implementation

                _logger.LogInformation("Login successful for user: {UserId}", user.Id);

                // Publish login event
                await _eventPublisher.PublishAsync(new UserLoginEvent
                {
                    UserId = user.Id,
                    Email = user.Email,
                    LoginTime = DateTime.UtcNow
                });

                return new ApiResponseDto<AuthResultDto>
                {
                    Success = true,
                    Message = "Login successful",
                    Data = new AuthResultDto
                    {
                        Success = true,
                        Token = token,
                        RefreshToken = refreshToken,
                        ExpiresAt = DateTime.UtcNow.AddMinutes(60),
                        User = MapToUserDto(user)
                    }
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during login for email: {Email}", loginDto.Email);
                return new ApiResponseDto<AuthResultDto>
                {
                    Success = false,
                    Message = "Login failed",
                    Errors = new List<string> { ex.Message }
                };
            }
        }

        public async Task<ApiResponseDto<AuthResultDto>> RegisterAsync(RegisterDto registerDto)
        {
            try
            {
                _logger.LogInformation("Attempting registration for email: {Email}", registerDto.Email);

                // Check if user already exists
                var existingUsers = await _cosmosService.GetAllAsync<User>(USERS_CONTAINER, 
                    $"SELECT * FROM c WHERE c.Email = '{registerDto.Email}'");
                
                if (existingUsers.Any())
                {
                    return new ApiResponseDto<AuthResultDto>
                    {
                        Success = false,
                        Message = "User with this email already exists",
                        Errors = new List<string> { "Email already registered" }
                    };
                }

                // Hash password
                var passwordHash = _passwordService.HashPassword(registerDto.Password);

                // Create new user
                var user = new User
                {
                    Id = Guid.NewGuid().ToString(), // Explicitly set ID
                    FirstName = registerDto.FirstName,
                    LastName = registerDto.LastName,
                    Email = registerDto.Email,
                    PasswordHash = passwordHash,
                    PhoneNumber = registerDto.PhoneNumber ?? string.Empty,
                    Role = registerDto.Role,
                    ProfileImageUrl = registerDto.ProfileImageUrl,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                // Log the user object for debugging
                _logger.LogInformation("Creating user with ID: {UserId} and Email: {Email}", user.Id, user.Email);
                var userJson = System.Text.Json.JsonSerializer.Serialize(user, new JsonSerializerOptions 
                { 
                    PropertyNameCaseInsensitive = true,
                    Converters = { new JsonStringEnumConverter() }
                });
                _logger.LogInformation("User JSON to be sent to Cosmos DB: {UserJson}", userJson);

                // Create user with email as partition key (as configured in infrastructure)
                var createdUser = await _cosmosService.CreateAsync(user, USERS_CONTAINER, user.Email);

                // Generate JWT token
                var token = _jwtService.GenerateToken(createdUser);
                var refreshToken = Guid.NewGuid().ToString();

                _logger.LogInformation("Registration successful for user: {UserId}", createdUser.Id);

                // Publish user created event
                await _eventPublisher.PublishAsync(new UserCreatedEvent
                {
                    UserId = createdUser.Id,
                    Email = createdUser.Email,
                    FirstName = createdUser.FirstName,
                    LastName = createdUser.LastName,
                    Role = createdUser.Role
                });

                return new ApiResponseDto<AuthResultDto>
                {
                    Success = true,
                    Message = "Registration successful",
                    Data = new AuthResultDto
                    {
                        Success = true,
                        Token = token,
                        RefreshToken = refreshToken,
                        ExpiresAt = DateTime.UtcNow.AddMinutes(60),
                        User = MapToUserDto(createdUser)
                    }
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during registration for email: {Email}", registerDto.Email);
                return new ApiResponseDto<AuthResultDto>
                {
                    Success = false,
                    Message = "Registration failed",
                    Errors = new List<string> { ex.Message }
                };
            }
        }

        public async Task<ApiResponseDto<UserDto>> GetCurrentUserAsync(string userId)
        {
            try
            {
                if (!Guid.TryParse(userId, out var userGuid))
                {
                    return new ApiResponseDto<UserDto>
                    {
                        Success = false,
                        Message = "Invalid user ID format"
                    };
                }

                var user = await _cosmosService.GetAsync<User>(userId, USERS_CONTAINER, userId);
                if (user == null)
                {
                    return new ApiResponseDto<UserDto>
                    {
                        Success = false,
                        Message = "User not found"
                    };
                }

                return new ApiResponseDto<UserDto>
                {
                    Success = true,
                    Data = MapToUserDto(user),
                    Message = "User retrieved successfully"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting current user: {UserId}", userId);
                return new ApiResponseDto<UserDto>
                {
                    Success = false,
                    Message = "Failed to retrieve user",
                    Errors = new List<string> { ex.Message }
                };
            }
        }

        public async Task<ApiResponseDto> LogoutAsync(string userId)
        {
            try
            {
                _logger.LogInformation("Logout for user: {UserId}", userId);

                // In a more complete implementation, you would invalidate the refresh token
                // For now, we just log the logout event
 
                    await _eventPublisher.PublishAsync(new UserLogoutEvent
                    {
                        UserId = userId,
                        LogoutTime = DateTime.UtcNow
                    });
                

                return new ApiResponseDto
                {
                    Success = true,
                    Message = "Logout successful"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during logout for user: {UserId}", userId);
                return new ApiResponseDto
                {
                    Success = false,
                    Message = "Logout failed",
                    Errors = new List<string> { ex.Message }
                };
            }
        }

        public async Task<ApiResponseDto<AuthResultDto>> RefreshTokenAsync(string refreshToken)
        {
            try
            {
                // Simple refresh token implementation
                // In production, you would store refresh tokens and validate them
                
                return new ApiResponseDto<AuthResultDto>
                {
                    Success = false,
                    Message = "Refresh token functionality not implemented yet"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error refreshing token");
                return new ApiResponseDto<AuthResultDto>
                {
                    Success = false,
                    Message = "Token refresh failed",
                    Errors = new List<string> { ex.Message }
                };
            }
        }

        public async Task<bool> ValidateTokenAsync(string token)
        {
            try
            {
                return _jwtService.ValidateToken(token);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating token");
                return false;
            }
        }

        private static UserDto MapToUserDto(User user)
        {
            return new UserDto
            {
                Id = user.Id,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email,
                PhoneNumber = user.PhoneNumber,
                Role = user.Role,
                IsActive = user.IsActive,
                CreatedAt = user.CreatedAt,
                ProfileImageUrl = user.ProfileImageUrl
            };
        }
    }
}
