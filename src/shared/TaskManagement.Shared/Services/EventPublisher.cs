using Azure.Messaging.ServiceBus;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using TaskManagement.Models.Events;

namespace TaskManagement.Shared.Services
{
    public interface IEventPublisher
    {
        Task PublishAsync<T>(T eventMessage) where T : BaseEvent;
    }

    public class ServiceBusEventPublisher : IEventPublisher, IDisposable
    {
        private readonly ServiceBusClient? _serviceBusClient;
        private readonly ServiceBusSender? _taskEventsSender;
        private readonly ServiceBusSender? _userEventsSender;
        private readonly ILogger<ServiceBusEventPublisher> _logger;

        public ServiceBusEventPublisher(IConfiguration configuration, ILogger<ServiceBusEventPublisher> logger)
        {
            _logger = logger;
            
            // Try multiple ways to get the connection string
            var connectionString = configuration["ServiceBusConnection"] 
                ?? configuration.GetConnectionString("ServiceBus")
                ?? Environment.GetEnvironmentVariable("ServiceBusConnection");
            
            if (string.IsNullOrEmpty(connectionString))
            {
                _logger.LogWarning("Service Bus connection string not found. Events will be logged instead of published to Service Bus.");
                return;
            }

            try
            {
                _serviceBusClient = new ServiceBusClient(connectionString);
                _taskEventsSender = _serviceBusClient.CreateSender("task-events");
                _userEventsSender = _serviceBusClient.CreateSender("user-events");
                
                _logger.LogInformation("Service Bus Event Publisher initialized successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to initialize Service Bus Event Publisher. Will fall back to logging.");
                // Don't throw - fall back to logging behavior
            }
        }

        public async Task PublishAsync<T>(T eventMessage) where T : BaseEvent
        {
            if (_serviceBusClient == null || _taskEventsSender == null || _userEventsSender == null)
            {
                // Fallback behavior - just log the event
                _logger.LogInformation("Event (Logged): {EventType} - {EventId} - {EventData}", 
                    eventMessage.EventType, 
                    eventMessage.EventId,
                    JsonSerializer.Serialize(eventMessage));
                return;
            }

            try
            {
                var json = JsonSerializer.Serialize(eventMessage);
                var message = new ServiceBusMessage(json)
                {
                    Subject = eventMessage.EventType,
                    MessageId = eventMessage.EventId.ToString(),
                    ContentType = "application/json"
                };

                // Route to appropriate topic based on event type
                ServiceBusSender sender = eventMessage.EventType.ToLower() switch
                {
                    var type when type.Contains("user") => _userEventsSender,
                    var type when type.Contains("task") => _taskEventsSender,
                    _ => _taskEventsSender // Default to task events
                };

                await sender.SendMessageAsync(message);
                _logger.LogInformation("Published event {EventType} with ID {EventId} to Service Bus", 
                    eventMessage.EventType, eventMessage.EventId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to publish event {EventType} with ID {EventId}. Falling back to logging.", 
                    eventMessage.EventType, eventMessage.EventId);
                
                // Fallback - log the event instead of failing
                _logger.LogInformation("Event (Fallback): {EventType} - {EventId}", 
                    eventMessage.EventType, eventMessage.EventId);
            }
        }

        public void Dispose()
        {
            try
            {
                _taskEventsSender?.DisposeAsync().GetAwaiter().GetResult();
                _userEventsSender?.DisposeAsync().GetAwaiter().GetResult();
                _serviceBusClient?.DisposeAsync().GetAwaiter().GetResult();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error disposing Service Bus Event Publisher");
            }
        }
    }
}
