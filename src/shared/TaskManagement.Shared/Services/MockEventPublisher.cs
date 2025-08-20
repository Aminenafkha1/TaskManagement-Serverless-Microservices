using Microsoft.Extensions.Logging;
using TaskManagement.Models.Events;
using TaskManagement.Shared.Services;

namespace TaskManagement.Shared.Services
{
    /// <summary>
    /// Mock implementation of IEventPublisher for local development
    /// This logs events instead of publishing to Service Bus
    /// </summary>
    public class MockEventPublisher : IEventPublisher
    {
        private readonly ILogger<MockEventPublisher> _logger;

        public MockEventPublisher(ILogger<MockEventPublisher> logger)
        {
            _logger = logger;
        }

        public Task PublishAsync<T>(T eventMessage) where T : BaseEvent
        {
            _logger.LogInformation("Mock Event Published: {EventType} - {EventId} - {EventData}", 
                eventMessage.EventType, 
                eventMessage.EventId,
                System.Text.Json.JsonSerializer.Serialize(eventMessage));

            // Return completed task since this is just a mock
            return Task.CompletedTask;
        }
    }
}
