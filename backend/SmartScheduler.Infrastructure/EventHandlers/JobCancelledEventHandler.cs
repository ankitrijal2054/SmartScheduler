using MediatR;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using SmartScheduler.Domain.Events;
using SmartScheduler.Infrastructure.Hubs;

namespace SmartScheduler.Infrastructure.EventHandlers;

/// <summary>
/// Event handler for JobCancelledEvent.
/// Sends SignalR notification to the assigned contractor that the job has been cancelled.
/// </summary>
public class JobCancelledEventHandler : INotificationHandler<JobCancelledEvent>
{
    private readonly IHubContext<NotificationHub> _hubContext;
    private readonly ILogger<JobCancelledEventHandler> _logger;

    public JobCancelledEventHandler(
        IHubContext<NotificationHub> hubContext,
        ILogger<JobCancelledEventHandler> logger)
    {
        _hubContext = hubContext ?? throw new ArgumentNullException(nameof(hubContext));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task Handle(JobCancelledEvent notification, CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Processing JobCancelledEvent for Job {JobId}, Contractor {ContractorId}",
            notification.JobId, notification.ContractorId);

        try
        {
            // Send SignalR notification to contractor
            var contractorGroup = $"contractor-{notification.ContractorId}";
            await _hubContext.Clients.Group(contractorGroup)
                .SendAsync(
                    "JobCancelled",
                    notification.JobId,
                    notification.Reason,
                    cancellationToken);

            _logger.LogInformation(
                "Sent JobCancelled notification to group {Group} for Job {JobId}",
                contractorGroup, notification.JobId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error handling JobCancelledEvent for Job {JobId}",
                notification.JobId);
            // Don't re-throw - we want to log but not break the flow
        }
    }
}

