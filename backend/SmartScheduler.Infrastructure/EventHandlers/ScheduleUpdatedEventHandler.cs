using MediatR;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using SmartScheduler.Domain.Events;
using SmartScheduler.Infrastructure.Hubs;

namespace SmartScheduler.Infrastructure.EventHandlers;

/// <summary>
/// Event handler for ScheduleUpdatedEvent.
/// Sends SignalR notification to the assigned contractor that their schedule has been updated.
/// </summary>
public class ScheduleUpdatedEventHandler : INotificationHandler<ScheduleUpdatedEvent>
{
    private readonly IHubContext<NotificationHub> _hubContext;
    private readonly ILogger<ScheduleUpdatedEventHandler> _logger;

    public ScheduleUpdatedEventHandler(
        IHubContext<NotificationHub> hubContext,
        ILogger<ScheduleUpdatedEventHandler> logger)
    {
        _hubContext = hubContext ?? throw new ArgumentNullException(nameof(hubContext));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task Handle(ScheduleUpdatedEvent notification, CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Processing ScheduleUpdatedEvent for Job {JobId}, Contractor {ContractorId}",
            notification.JobId, notification.ContractorId);

        try
        {
            // Send SignalR notification to contractor
            var contractorGroup = $"contractor-{notification.ContractorId}";
            await _hubContext.Clients.Group(contractorGroup)
                .SendAsync(
                    "ScheduleUpdated",
                    notification.JobId,
                    notification.NewScheduledDateTime,
                    notification.OldScheduledDateTime,
                    cancellationToken);

            _logger.LogInformation(
                "Sent ScheduleUpdated notification to group {Group} for Job {JobId}. " +
                "New time: {NewTime}, Old time: {OldTime}",
                contractorGroup, notification.JobId,
                notification.NewScheduledDateTime, notification.OldScheduledDateTime);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error handling ScheduleUpdatedEvent for Job {JobId}",
                notification.JobId);
            // Don't re-throw - we want to log but not break the flow
        }
    }
}

