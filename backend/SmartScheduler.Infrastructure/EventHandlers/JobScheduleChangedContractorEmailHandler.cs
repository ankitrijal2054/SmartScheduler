using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using SmartScheduler.Application.DTOs;
using SmartScheduler.Application.Services;
using SmartScheduler.Domain.Events;
using SmartScheduler.Infrastructure.Persistence;

namespace SmartScheduler.Infrastructure.EventHandlers;

/// <summary>
/// Event handler for ScheduleUpdatedEvent.
/// Sends email notification to contractor when job schedule is updated.
/// This handler complements the SignalR real-time notification.
/// </summary>
public class JobScheduleChangedContractorEmailHandler : INotificationHandler<ScheduleUpdatedEvent>
{
    private readonly IEmailService _emailService;
    private readonly ApplicationDbContext _dbContext;
    private readonly ILogger<JobScheduleChangedContractorEmailHandler> _logger;
    private readonly IConfiguration _configuration;

    public JobScheduleChangedContractorEmailHandler(
        IEmailService emailService,
        ApplicationDbContext dbContext,
        ILogger<JobScheduleChangedContractorEmailHandler> logger,
        IConfiguration configuration)
    {
        _emailService = emailService ?? throw new ArgumentNullException(nameof(emailService));
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
    }

    public async Task Handle(ScheduleUpdatedEvent notification, CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Processing ScheduleUpdatedEvent for Contractor {ContractorId}, Job {JobId}",
            notification.ContractorId, notification.JobId);

        try
        {
            // Fetch contractor and job information
            var contractor = await _dbContext.Contractors
                .Include(c => c.User)
                .FirstOrDefaultAsync(c => c.Id == notification.ContractorId, cancellationToken);

            if (contractor?.User == null)
            {
                _logger.LogWarning("Contractor {ContractorId} not found for event", notification.ContractorId);
                return;
            }

            var job = await _dbContext.Jobs
                .FirstOrDefaultAsync(j => j.Id == notification.JobId, cancellationToken);

            if (job == null)
            {
                _logger.LogWarning("Job {JobId} not found for event", notification.JobId);
                return;
            }

            // Build email data for contractor notification
            var frontendBaseUrl = _configuration["Frontend:BaseUrl"] ?? "http://localhost:5173";
            var contractorDashboardUrl = $"{frontendBaseUrl}/contractor/dashboard";

            var emailData = new EmailTemplateDataDto
            {
                ContractorEmail = contractor.User.Email,
                ContractorName = contractor.Name,
                JobId = job.Id,
                JobType = job.JobType.ToString(),
                Location = job.Location,
                OldScheduledDateTime = notification.OldScheduledDateTime,
                NewScheduledDateTime = notification.NewScheduledDateTime,
                JobTrackingUrl = contractorDashboardUrl
            };

            // Send email
            var success = await _emailService.SendEmailAsync(
                to: contractor.User.Email,
                subject: $"Schedule Updated: {job.JobType} in {job.Location}",
                templateName: "JobScheduleChangedForContractor",
                templateData: emailData,
                cancellationToken: cancellationToken);

            if (success)
            {
                _logger.LogInformation(
                    "Email sent successfully to contractor {ContractorId} for schedule update {JobId}",
                    notification.ContractorId, notification.JobId);
            }
            else
            {
                _logger.LogWarning(
                    "Email failed to send to contractor {ContractorId} for schedule update {JobId}",
                    notification.ContractorId, notification.JobId);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error handling ScheduleUpdatedEvent for Job {JobId}",
                notification.JobId);
            // Don't re-throw - we want to log but not break the flow
        }
    }
}

