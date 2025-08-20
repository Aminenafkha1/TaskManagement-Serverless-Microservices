using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using System.Net;
using System.Text.Json;
using TaskManagement.Models.DTOs;
using TaskManagement.Models; 
using System.Collections.Generic;
using System;
using System.Threading.Tasks;
using TaskManagement.Shared.Services;

namespace TaskManagement.NotificationsService
{
    public class NotificationsApi
    {
        private readonly ILogger<NotificationsApi> _logger; 
        private readonly IEventPublisher _eventPublisher;
         
    }
}
