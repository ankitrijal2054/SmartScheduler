using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using SmartScheduler.Application.DTOs;
using SmartScheduler.Application.Services;
using SmartScheduler.Domain.Entities;
using SmartScheduler.Domain.Events;
using SmartScheduler.Infrastructure.Persistence;

namespace SmartScheduler.Infrastructure.EventHandlers;

/// <summary>
/// Event handler for JobInProgressEvent.
/// Sends email notification to customer when contractor marks job as in-progress (on the way).
/// </summary>
public class JobInProgressEventHandler : INotificationHandler<JobInProgressEvent>
{
    private readonly IEmailService _emailService;
    private readonly ApplicationDbContext _dbContext;
    private readonly ILogger<JobInProgressEventHandler> _logger;
    private readonly IConfiguration _configuration;

    public JobInProgressEventHandler(
        IEmailService emailService,
        ApplicationDbContext dbContext,
        ILogger<JobInProgressEventHandler> logger,
        IConfiguration configuration)
    {
        _emailService = emailService ?? throw new ArgumentNullException(nameof(emailService));
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
    }

    public async Task Handle(JobInProgressEvent notification, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Processing JobInProgressEvent for Job {JobId}, Assignment {AssignmentId}",
            notification.JobId, notification.AssignmentId);

        try
        {
            // Fetch job, contractor, and customer information
            var job = await _dbContext.Jobs
                .FirstOrDefaultAsync(j => j.Id == notification.JobId, cancellationToken);

            if (job == null)
            {
                _logger.LogWarning("Job {JobId} not found for event", notification.JobId);
                return;
            }

            var customer = await _dbContext.Customers
                .Include(c => c.User)
                .FirstOrDefaultAsync(c => c.Id == job.CustomerId, cancellationToken);

            if (customer?.User == null)
            {
                _logger.LogWarning("Customer not found for Job {JobId}", notification.JobId);
                return;
            }

            var contractor = await _dbContext.Contractors
                .FirstOrDefaultAsync(c => c.Id == notification.ContractorId, cancellationToken);

            if (contractor == null)
            {
                _logger.LogWarning("Contractor {ContractorId} not found for event", notification.ContractorId);
                return;
            }

            // Build email data
            var frontendBaseUrl = _configuration["Frontend:BaseUrl"] ?? "http://localhost:5173";
            var jobTrackingUrl = $"{frontendBaseUrl}/customer/jobs/{notification.JobId}";

            var emailData = new EmailTemplateDataDto
            {
                CustomerEmail = customer.User.Email,
                CustomerName = customer.Name,
                JobId = job.Id,
                JobType = job.JobType.ToString(),
                Location = job.Location,
                Description = job.Description,
                DesiredDateTime = job.DesiredDateTime,
                ContractorName = contractor.Name,
                ContractorPhone = contractor.PhoneNumber,
                ContractorRating = contractor.AverageRating,
                ETA = "On the way",
                JobTrackingUrl = jobTrackingUrl,
                RatingUrl = ""
            };

            // Send email
            var success = await _emailService.SendEmailAsync(
                to: customer.User.Email,
                subject: $"{contractor.Name} is on the way to your job!",
                templateName: "JobInProgress",
                templateData: emailData,
                cancellationToken: cancellationToken);

            if (success)
            {
                _logger.LogInformation("Job in-progress email sent successfully for Job {JobId} to {Email}",
                    notification.JobId, customer.User.Email);
            }
            else
            {
                _logger.LogWarning("Job in-progress email failed to send for Job {JobId} to {Email}",
                    notification.JobId, customer.User.Email);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error handling JobInProgressEvent for Job {JobId}",
                notification.JobId);
            // Don't re-throw - we want to log but not break the flow
        }
    }
}

