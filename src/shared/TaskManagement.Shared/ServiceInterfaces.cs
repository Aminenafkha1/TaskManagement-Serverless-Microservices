using System;
using TaskManagement.Models;
using TaskManagement.Models.DTOs;
using TaskManagement.Models.Events;

namespace TaskManagement.Shared.Interfaces
{
    // =================== SERVICE BUS INTERFACE ===================
    public interface IEventBus
    {
        Task PublishAsync<T>(T @event) where T : BaseEvent;
        Task SubscribeAsync<T>(Func<T, Task> handler) where T : BaseEvent;
    }
 
}
