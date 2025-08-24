using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.ComponentModel.DataAnnotations;
using Microsoft.JSInterop;
using TaskManagement.Models;
using TaskStatus = TaskManagement.Models.TaskStatus;

namespace TaskManagement.Client.Services
{
    public interface ITaskService
    {
        Task<List<TaskItem>> GetMyTasksAsync();
        Task<List<TaskItem>> GetAllTasksAsync();
        Task<TaskItem?> GetTaskByIdAsync(Guid id);
        Task<TaskItem?> CreateTaskAsync(CreateTaskRequest request);
        Task<TaskItem?> UpdateTaskAsync(Guid id, UpdateTaskRequest request);
        Task<bool> DeleteTaskAsync(Guid id);
        Task<DashboardStats?> GetDashboardStatsAsync();
    }

    public class TaskService : ITaskService
    {
        private readonly HttpClient _httpClient;
        private readonly IAuthenticationService _authService;
        private readonly string _baseApiUrl = "http://localhost:7072/api"; // Tasks service port

        public TaskService(HttpClient httpClient, IAuthenticationService authService)
        {
            _httpClient = httpClient;
            _authService = authService;
        }

        public async Task<List<TaskItem>> GetMyTasksAsync()
        {
            try
            {
                await SetAuthHeaderAsync();
                var userId = await _authService.GetUserIdAsync();
                
                var response = await _httpClient.GetAsync($"{_baseApiUrl}/tasks/user/{userId}");
                
                if (response.IsSuccessStatusCode)
                {
                    var result = await response.Content.ReadFromJsonAsync<ApiResponse<List<TaskItem>>>(GetJsonOptions());
                    return result?.Data ?? new List<TaskItem>();
                }
                
                return new List<TaskItem>();
            }
            catch
            {
                return new List<TaskItem>();
            }
        }

        public async Task<List<TaskItem>> GetAllTasksAsync()
        {
            try
            {
                await SetAuthHeaderAsync();
                
                var response = await _httpClient.GetAsync($"{_baseApiUrl}/tasks");
                
                if (response.IsSuccessStatusCode)
                {
                    var result = await response.Content.ReadFromJsonAsync<ApiResponse<List<TaskItem>>>(GetJsonOptions());
                    return result?.Data ?? new List<TaskItem>();
                }
                
                return new List<TaskItem>();
            }
            catch
            {
                return new List<TaskItem>();
            }
        }

        public async Task<TaskItem?> GetTaskByIdAsync(Guid id)
        {
            try
            {
                await SetAuthHeaderAsync();
                
                var response = await _httpClient.GetAsync($"{_baseApiUrl}/tasks/{id}");
                
                if (response.IsSuccessStatusCode)
                {
                    var result = await response.Content.ReadFromJsonAsync<ApiResponse<TaskItem>>(GetJsonOptions());
                    return result?.Data;
                }
                
                return null;
            }
            catch
            {
                return null;
            }
        }

        public async Task<TaskItem?> CreateTaskAsync(CreateTaskRequest request)
        {
            try
            {
                await SetAuthHeaderAsync();
                
                var response = await _httpClient.PostAsJsonAsync($"{_baseApiUrl}/tasks", request, GetJsonOptions());
                
                if (response.IsSuccessStatusCode)
                {
                    var result = await response.Content.ReadFromJsonAsync<ApiResponse<TaskItem>>(GetJsonOptions());
                    return result?.Data;
                }
                
                return null;
            }
            catch
            {
                return null;
            }
        }

        public async Task<TaskItem?> UpdateTaskAsync(Guid id, UpdateTaskRequest request)
        {
            try
            {
                await SetAuthHeaderAsync();
                
                var response = await _httpClient.PutAsJsonAsync($"{_baseApiUrl}/tasks/{id}", request, GetJsonOptions());
                
                if (response.IsSuccessStatusCode)
                {
                    var result = await response.Content.ReadFromJsonAsync<ApiResponse<TaskItem>>(GetJsonOptions());
                    return result?.Data;
                }
                
                return null;
            }
            catch
            {
                return null;
            }
        }

        public async Task<bool> DeleteTaskAsync(Guid id)
        {
            try
            {
                await SetAuthHeaderAsync();
                
                var response = await _httpClient.DeleteAsync($"{_baseApiUrl}/tasks/{id}");
                return response.IsSuccessStatusCode;
            }
            catch
            {
                return false;
            }
        }

        public async Task<DashboardStats?> GetDashboardStatsAsync()
        {
            try
            {
                await SetAuthHeaderAsync();
                var userId = await _authService.GetUserIdAsync();
                
                var response = await _httpClient.GetAsync($"{_baseApiUrl}/tasks/stats/{userId}");
                
                if (response.IsSuccessStatusCode)
                {
                    var result = await response.Content.ReadFromJsonAsync<ApiResponse<DashboardStats>>(GetJsonOptions());
                    return result?.Data;
                }
                
                return null;
            }
            catch
            {
                return null;
            }
        }

        private async Task SetAuthHeaderAsync()
        {
            var token = await _authService.GetTokenAsync();
            if (!string.IsNullOrEmpty(token))
            {
                _httpClient.DefaultRequestHeaders.Authorization = 
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
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
    public class CreateTaskRequest
    {
        [Required(ErrorMessage = "Title is required")]
        [MinLength(3, ErrorMessage = "Title must be at least 3 characters")]
        [MaxLength(200, ErrorMessage = "Title cannot exceed 200 characters")]
        public string Title { get; set; } = string.Empty;

        [MaxLength(1000, ErrorMessage = "Description cannot exceed 1000 characters")]
        public string Description { get; set; } = string.Empty;

        [MaxLength(50, ErrorMessage = "Category cannot exceed 50 characters")]
        public string Category { get; set; } = string.Empty;

        public TaskPriority Priority { get; set; } = TaskPriority.Medium;
        public DateTime? DueDate { get; set; }
        public string? AssignedToUserId { get; set; }
        public List<string> Tags { get; set; } = new();
    }

    public class UpdateTaskRequest
    {
        public string? Title { get; set; }
        public string? Description { get; set; }
        public TaskStatus? Status { get; set; }
        public TaskPriority? Priority { get; set; }
        public DateTime? DueDate { get; set; }
        public string? AssignedToUserId { get; set; }
    }
 
    public class DashboardStats
    {
        public int TotalTasks { get; set; }
        public int CompletedTasks { get; set; }
        public int PendingTasks { get; set; }
        public int OverdueTasks { get; set; }
        public List<TaskItem> RecentTasks { get; set; } = new();
    }

 
 
}
