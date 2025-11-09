using MediatR;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SmartScheduler.Domain.Events;
using SmartScheduler.Infrastructure.Hubs;
using SmartScheduler.Infrastructure.Persistence;

namespace SmartScheduler.Infrastructure.EventHandlers;

/// <summary>
/// Event handler for JobReassignedEvent.
/// Sends SignalR notification to the original contractor that their assignment has been reassigned.
/// </summary>
public class JobReassignedEventHandler : INotificationHandler<JobReassignedEvent>
{
    private readonly ApplicationDbContext _dbContext;
    private readonly IHubContext<NotificationHub> _hubContext;
    private readonly ILogger<JobReassignedEventHandler> _logger;

    public JobReassignedEventHandler(
        ApplicationDbContext dbContext,
        IHubContext<NotificationHub> hubContext,
        ILogger<JobReassignedEventHandler> logger)
    {
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        _hubContext = hubContext ?? throw new ArgumentNullException(nameof(hubContext));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task Handle(JobReassignedEvent notification, CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Processing JobReassignedEvent for Job {JobId}: {OldContractorId} → {NewContractorId}",
            notification.JobId, notification.OldContractorId, notification.NewContractorId);

        try
        {
            // Fetch new contractor name
            var newContractor = await _dbContext.Contractors
                .FirstOrDefaultAsync(c => c.Id == notification.NewContractorId, cancellationToken);

            if (newContractor == null)
            {
                _logger.LogWarning("New contractor {ContractorId} not found for job reassignment",
                    notification.NewContractorId);
                return;
            }

            // Send SignalR notification to old contractor
            var contractorGroup = $"contractor-{notification.OldContractorId}";
            await _hubContext.Clients.Group(contractorGroup)
                .SendAsync(
                    "JobReassigned",
                    notification.JobId,
                    newContractor.Name,
                    notification.Reason,
                    cancellationToken);

            _logger.LogInformation(
                "Sent JobReassigned notification to group {Group}. Job {JobId} reassigned to {ContractorName}",
                contractorGroup, notification.JobId, newContractor.Name);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error handling JobReassignedEvent for Job {JobId}",
                notification.JobId);
            // Don't re-throw - we want to log but not break the flow
        }
    }
}

