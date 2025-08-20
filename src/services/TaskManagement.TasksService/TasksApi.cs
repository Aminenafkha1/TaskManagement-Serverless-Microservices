#nullable enable
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using System.IdentityModel.Tokens.Jwt;
using TaskManagement.Models;
using TaskManagement.Models.DTOs;
using TaskManagement.Models.Events;
using TaskManagement.Shared.Services;
using TaskStatus = TaskManagement.Models.TaskStatus;

namespace TaskManagement.TasksService
{
    public class TasksApi
    {
        private readonly ILogger<TasksApi> _logger;
        private readonly IPersistenceService _cosmosService; // For data persistence
        private readonly IEventPublisher _eventPublisher; // For publishing events to Service Bus
        private readonly IMaterializedViewService _materializedViewService; // For querying materialized views
        private const string TASKS_CONTAINER = "tasks";

        public TasksApi(
            ILogger<TasksApi> logger, 
            IPersistenceService cosmosService, 
            IEventPublisher eventPublisher,
            IMaterializedViewService materializedViewService)
        {
            _logger = logger;
            _cosmosService = cosmosService;
            _eventPublisher = eventPublisher;
            _materializedViewService = materializedViewService;
        }

        // =================== TASK MANAGEMENT ENDPOINTS ===================
        
        [Function("CreateTask")]
        public async Task<HttpResponseData> CreateTask([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "tasks")] HttpRequestData req)
        {
            try
            {
                // Get user ID from Authorization header (simplified - in production use proper JWT validation)
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

                // Extract userId from JWT
                string? userId = null;
                var token = authHeader.Substring("Bearer ".Length).Trim();
                var handler = new System.IdentityModel.Tokens.Jwt.JwtSecurityTokenHandler();
                var jwtToken = handler.ReadJwtToken(token);
                userId = jwtToken.Claims.FirstOrDefault(c => c.Type == System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId) || !Guid.TryParse(userId, out var userGuid))
                {
                    var unauthorized = req.CreateResponse(HttpStatusCode.Unauthorized);
                    await unauthorized.WriteAsJsonAsync(new ApiResponseDto
                    {
                        Success = false,
                        Message = "Valid user ID not found in JWT"
                    });
                    return unauthorized;
                }

                var requestBody = await new StreamReader(req.Body).ReadToEndAsync();
                var createTaskDto = JsonSerializer.Deserialize<CreateTaskDto>(requestBody, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                if (createTaskDto == null)
                {
                    var badRequest = req.CreateResponse(HttpStatusCode.BadRequest);
                    await badRequest.WriteAsJsonAsync(new ApiResponseDto
                    {
                        Success = false,
                        Message = "Invalid request body"
                    });
                    return badRequest;
                }

                _logger.LogInformation("Creating new task for user: {UserId}", userGuid);

                var task = new TaskItem
                {
                    Title = createTaskDto.Title,
                    Description = createTaskDto.Description,
                    Status = TaskStatus.Todo,
                    Priority = createTaskDto.Priority,
                    AssignedToUserId = createTaskDto.AssignedToUserId,
                    CreatedByUserId = userGuid,
                    DueDate = createTaskDto.DueDate,
                    Category = createTaskDto.Category,
                    Tags = createTaskDto.Tags,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                var createdTask = await _cosmosService.CreateAsync(task, TASKS_CONTAINER);

                // Create TaskDto for response
                var taskDto = new TaskDto
                {
                    Id = createdTask.Id,
                    Title = createdTask.Title,
                    Description = createdTask.Description,
                    Status = createdTask.Status,
                    Priority = createdTask.Priority,
                    AssignedToUserId = createdTask.AssignedToUserId,
                    CreatedByUserId = createdTask.CreatedByUserId,
                    CreatedAt = createdTask.CreatedAt,
                    UpdatedAt = createdTask.UpdatedAt,
                    DueDate = createdTask.DueDate,
                    Category = createdTask.Category,
                    Tags = createdTask.Tags
                };

                // Async communication via Event Grid
                try
                {
                    var taskCreatedEvent = new TaskCreatedEvent
                    {
                        TaskId = createdTask.Id,
                        Title = createdTask.Title,
                        AssignedToUserId = createdTask.AssignedToUserId,
                        CreatedByUserId = createdTask.CreatedByUserId,
                        Priority = createdTask.Priority,
                        DueDate = createdTask.DueDate
                    };

                    await _eventPublisher.PublishAsync(taskCreatedEvent);
                    _logger.LogInformation("Published task created event for task {TaskId}", createdTask.Id);
                    
                    // Note: Materialized views will be updated automatically via Change Feed processor
                }
                catch (Exception eventEx)
                {
                    _logger.LogWarning(eventEx, "Failed to publish task created event, continuing with sync approach");
                }

                // Sync communication - Get user names from Users service
                PopulateUserNames(taskDto);

                var response = req.CreateResponse(HttpStatusCode.Created);
                await response.WriteAsJsonAsync(new ApiResponseDto<TaskDto>
                {
                    Success = true,
                    Data = taskDto,
                    Message = "Task created successfully"
                });

                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating task");
                var errorResponse = req.CreateResponse(HttpStatusCode.InternalServerError);
                await errorResponse.WriteAsJsonAsync(new ApiResponseDto
                {
                    Success = false,
                    Message = "Failed to create task",
                    Errors = new List<string> { ex.Message }
                });
                return errorResponse;
            }
        }

        [Function("GetTasks")]
        public async Task<HttpResponseData> GetTasks([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "tasks")] HttpRequestData req)
        {
            try
            {
                // Extract user ID from JWT token
                var userId = ExtractUserIdFromToken(req);
                if (userId == null || !Guid.TryParse(userId, out var userGuid))
                {
                    var unauthorized = req.CreateResponse(HttpStatusCode.Unauthorized);
                    await unauthorized.WriteAsJsonAsync(new ApiResponseDto
                    {
                        Success = false,
                        Message = "Unauthorized access"
                    });
                    return unauthorized;
                }

                _logger.LogInformation("Getting tasks for user: {UserId}", userGuid);

                try
                {
                    // First try to get from materialized view (optimized query with user info)
                    var materializedTasks = await _materializedViewService.GetTasksWithUserInfoAsync(userGuid.ToString());

                    if (materializedTasks.Any())
                    {
                        _logger.LogInformation("Retrieved {Count} tasks from materialized view for user {UserId}", materializedTasks.Count(), userGuid);
                        
                        var materializedDtos = materializedTasks.Select(mv => new TaskDto
                        {
                            Id = Guid.Parse(mv.id),
                            Title = mv.title,
                            Description = mv.description,
                            Status = mv.status,
                            Priority = mv.priority,
                            CreatedAt = mv.createdAt,
                            UpdatedAt = mv.updatedAt ?? DateTime.UtcNow,
                            DueDate = mv.dueDate,
                            AssignedToUserId = Guid.Parse(mv.assignedToUserId),
                            AssignedToUserName = mv.assignedToUserName,
                            CreatedByUserId = Guid.Parse(mv.createdByUserId),
                            CreatedByUserName = mv.createdByUserName
                        }).ToList();

                        var materializedResponse = req.CreateResponse(HttpStatusCode.OK);
                        await materializedResponse.WriteAsJsonAsync(new ApiResponseDto<List<TaskDto>>
                        {
                            Success = true,
                            Data = materializedDtos,
                            Message = $"Retrieved {materializedDtos.Count} tasks from materialized views"
                        });
                        return materializedResponse;
                    }
                }
                catch (Exception materializedViewEx)
                {
                    _logger.LogWarning(materializedViewEx, "Failed to query materialized view, falling back to direct query");
                }

                // Fallback to direct query from tasks container
                var tasks = await _cosmosService.GetAllAsync<TaskItem>(TASKS_CONTAINER, 
                    $"SELECT * FROM c WHERE c.AssignedToUserId = '{userGuid}' OR c.CreatedByUserId = '{userGuid}'");

                var taskDtos = new List<TaskDto>();

                foreach (var task in tasks)
                {
                    var taskDto = MapToTaskDto(task);
                    // Note: User names will be populated by materialized views when available
                    // For now, we'll use placeholder values in fallback
                    taskDto.AssignedToUserName = task.AssignedToUserId != Guid.Empty ? "User Name" : null;
                    taskDto.CreatedByUserName = "Creator Name";
                    taskDtos.Add(taskDto);
                }

                var response = req.CreateResponse(HttpStatusCode.OK);
                await response.WriteAsJsonAsync(new ApiResponseDto<List<TaskDto>>
                {
                    Success = true,
                    Data = taskDtos,
                    Message = "Tasks retrieved successfully"
                });

                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting tasks");
                var errorResponse = req.CreateResponse(HttpStatusCode.InternalServerError);
                await errorResponse.WriteAsJsonAsync(new ApiResponseDto
                {
                    Success = false,
                    Message = "Failed to retrieve tasks",
                    Errors = new List<string> { ex.Message }
                });
                return errorResponse;
            }
        }

        // =================== HELPER METHODS ===================

        private void PopulateUserNames(TaskDto taskDto)
        {
            try
            {
                // For now, use placeholder names
                // In a real event-driven system, user names would be populated
                // through user events that update local denormalized data
                if (taskDto.AssignedToUserId != Guid.Empty)
                {
                    taskDto.AssignedToUserName = $"User_{taskDto.AssignedToUserId.ToString().Substring(0, 8)}";
                }

                taskDto.CreatedByUserName = $"User_{taskDto.CreatedByUserId.ToString().Substring(0, 8)}";

                _logger.LogInformation("Populated user names for task {TaskId}", taskDto.Id);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to populate user names for task {TaskId}", taskDto.Id);
                taskDto.AssignedToUserName = "Unknown User";
                taskDto.CreatedByUserName = "Unknown User";
            }
        }

        private TaskDto MapToTaskDto(TaskItem task)
        {
            return new TaskDto
            {
                Id = task.Id,
                Title = task.Title,
                Description = task.Description,
                Status = task.Status,
                Priority = task.Priority,
                AssignedToUserId = task.AssignedToUserId,
                CreatedByUserId = task.CreatedByUserId,
                CreatedAt = task.CreatedAt,
                UpdatedAt = task.UpdatedAt,
                DueDate = task.DueDate,
                CompletedAt = task.CompletedAt,
                Category = task.Category,
                Tags = task.Tags ?? new List<string>()
            };
        }

        private string? ExtractUserIdFromToken(HttpRequestData req)
        {
            try
            {
                var authHeader = req.Headers.FirstOrDefault(h => h.Key.Equals("Authorization", StringComparison.OrdinalIgnoreCase)).Value?.FirstOrDefault();
                if (string.IsNullOrEmpty(authHeader) || !authHeader.StartsWith("Bearer "))
                {
                    return null;
                }

                var token = authHeader.Substring("Bearer ".Length).Trim();
                var handler = new System.IdentityModel.Tokens.Jwt.JwtSecurityTokenHandler();
                var jsonToken = handler.ReadJwtToken(token);
                return jsonToken.Claims.FirstOrDefault(x => x.Type == "sub")?.Value ?? 
                       jsonToken.Claims.FirstOrDefault(x => x.Type == "nameid")?.Value;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to parse JWT token");
                return null;
            }
        }
    }
}
