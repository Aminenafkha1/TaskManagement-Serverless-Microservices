using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using System.Net;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.IO;
using TaskManagement.Models;
using TaskManagement.Models.DTOs;
using TaskManagement.Models.Events;
using TaskManagement.Shared.Services;
using TaskManagement.UsersService.Services; 

namespace TaskManagement.UsersService
{
    public class UsersApi
    {
        private readonly ILogger<UsersApi> _logger;
        private readonly IPersistenceService _cosmosService;
        private readonly IEventPublisher _eventPublisher;
        private readonly IAuthService _authService;
        private const string USERS_CONTAINER = "users";

        public UsersApi(ILogger<UsersApi> logger, IPersistenceService cosmosService, IEventPublisher eventPublisher, IAuthService authService)
        {
            _logger = logger;
            _cosmosService = cosmosService;
            _eventPublisher = eventPublisher;
            _authService = authService;
        }

        // =================== AUTHENTICATION ENDPOINTS ===================
        
        [Function("Login")]
        public async Task<HttpResponseData> Login([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "auth/login")] HttpRequestData req)
        {
            try
            {
                var requestBody = await new StreamReader(req.Body).ReadToEndAsync();
                var jsonOptions = new JsonSerializerOptions 
                { 
                    PropertyNameCaseInsensitive = true,
                    Converters = { new JsonStringEnumConverter() }
                };
                var loginDto = JsonSerializer.Deserialize<LoginDto>(requestBody, jsonOptions);

                if (loginDto == null)
                {
                    var badRequest = req.CreateResponse(HttpStatusCode.BadRequest);
                    await badRequest.WriteAsJsonAsync(new ApiResponseDto
                    {
                        Success = false,
                        Message = "Invalid request body"
                    });
                    return badRequest;
                }

                var result = await _authService.LoginAsync(loginDto);

                var statusCode = result.Success ? HttpStatusCode.OK : HttpStatusCode.Unauthorized;
                var response = req.CreateResponse(statusCode);
                await response.WriteAsJsonAsync(result);

                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during login");
                var errorResponse = req.CreateResponse(HttpStatusCode.InternalServerError);
                await errorResponse.WriteAsJsonAsync(new ApiResponseDto
                {
                    Success = false,
                    Message = "Login failed",
                    Errors = new List<string> { ex.Message }
                });
                return errorResponse;
            }
        }

        [Function("Register")]
        public async Task<HttpResponseData> Register([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "auth/register")] HttpRequestData req)
        {
            try
            {
                var requestBody = await new StreamReader(req.Body).ReadToEndAsync();
                var jsonOptions = new JsonSerializerOptions 
                { 
                    PropertyNameCaseInsensitive = true,
                    Converters = { new JsonStringEnumConverter() }
                };
                var registerDto = JsonSerializer.Deserialize<RegisterDto>(requestBody, jsonOptions);

                if (registerDto == null)
                {
                    var badRequest = req.CreateResponse(HttpStatusCode.BadRequest);
                    await badRequest.WriteAsJsonAsync(new ApiResponseDto
                    {
                        Success = false,
                        Message = "Invalid request body"
                    });
                    return badRequest;
                }

                var result = await _authService.RegisterAsync(registerDto);

                var statusCode = result.Success ? HttpStatusCode.Created : HttpStatusCode.BadRequest;
                var response = req.CreateResponse(statusCode);
                await response.WriteAsJsonAsync(result);

                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during registration");
                var errorResponse = req.CreateResponse(HttpStatusCode.InternalServerError);
                await errorResponse.WriteAsJsonAsync(new ApiResponseDto
                {
                    Success = false,
                    Message = "Registration failed",
                    Errors = new List<string> { ex.Message }
                });
                return errorResponse;
            }
        }

        [Function("GetMe")]
        public async Task<HttpResponseData> GetMe([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "auth/me")] HttpRequestData req)
        {
            try
            {
                // Get user ID from Authorization header
                var authHeader = req.Headers.FirstOrDefault(h => h.Key.Equals("Authorization", StringComparison.OrdinalIgnoreCase)).Value?.FirstOrDefault();
                if (string.IsNullOrEmpty(authHeader) || !authHeader.StartsWith("Bearer "))
                {
                    var unauthorized = req.CreateResponse(HttpStatusCode.Unauthorized);
                    await unauthorized.WriteAsJsonAsync(new ApiResponseDto
                    {
                        Success = false,
                        Message = "Authorization token required"
                    });
                    return unauthorized;
                }

                var token = authHeader.Substring("Bearer ".Length);
                var jwtService = req.FunctionContext.InstanceServices.GetService(typeof(IJwtService)) as IJwtService;
                var userId = jwtService?.GetUserIdFromToken(token);

                if (string.IsNullOrEmpty(userId))
                {
                    var unauthorized = req.CreateResponse(HttpStatusCode.Unauthorized);
                    await unauthorized.WriteAsJsonAsync(new ApiResponseDto
                    {
                        Success = false,
                        Message = "Invalid token"
                    });
                    return unauthorized;
                }

                var result = await _authService.GetCurrentUserAsync(userId);

                var statusCode = result.Success ? HttpStatusCode.OK : HttpStatusCode.NotFound;
                var response = req.CreateResponse(statusCode);
                await response.WriteAsJsonAsync(result);

                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting current user");
                var errorResponse = req.CreateResponse(HttpStatusCode.InternalServerError);
                await errorResponse.WriteAsJsonAsync(new ApiResponseDto
                {
                    Success = false,
                    Message = "Failed to get current user",
                    Errors = new List<string> { ex.Message }
                });
                return errorResponse;
            }
        }

        // =================== END OF USERS SERVICE ===================
    }
}
