using MediatR;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using SmartScheduler.Application.DTOs;
using SmartScheduler.Application.Services;
using SmartScheduler.Domain.Entities;
using SmartScheduler.Domain.Events;
using SmartScheduler.Infrastructure.Hubs;
using SmartScheduler.Infrastructure.Persistence;

namespace SmartScheduler.Infrastructure.EventHandlers;

/// <summary>
/// Event handler for JobCompletedEvent.
/// Sends email notification to customer when job is completed with rating link.
/// </summary>
public class JobCompletedEventHandler : INotificationHandler<JobCompletedEvent>
{
    private readonly IEmailService _emailService;
    private readonly ApplicationDbContext _dbContext;
    private readonly ILogger<JobCompletedEventHandler> _logger;
    private readonly IConfiguration _configuration;
    private readonly IHubContext<NotificationHub> _hubContext;

    public JobCompletedEventHandler(
        IEmailService emailService,
        ApplicationDbContext dbContext,
        ILogger<JobCompletedEventHandler> logger,
        IConfiguration configuration,
        IHubContext<NotificationHub> hubContext)
    {
        _emailService = emailService ?? throw new ArgumentNullException(nameof(emailService));
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        _hubContext = hubContext ?? throw new ArgumentNullException(nameof(hubContext));
    }

    public async Task Handle(JobCompletedEvent notification, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Processing JobCompletedEvent for Job {JobId}, Assignment {AssignmentId}",
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
            var ratingUrl = $"{jobTrackingUrl}/review";

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
                ETA = "Completed",
                JobTrackingUrl = jobTrackingUrl,
                RatingUrl = ratingUrl
            };

            // Send email
            var success = await _emailService.SendEmailAsync(
                to: customer.User.Email,
                subject: "Your Job is Complete! Please Rate the Contractor",
                templateName: "JobCompleted",
                templateData: emailData,
                cancellationToken: cancellationToken);

            if (success)
            {
                _logger.LogInformation("Job completion email sent successfully for Job {JobId} to {Email}",
                    notification.JobId, customer.User.Email);
            }
            else
            {
                _logger.LogWarning("Job completion email failed to send for Job {JobId} to {Email}",
                    notification.JobId, customer.User.Email);
            }

            // Send SignalR notification to customer
            try
            {
                var customerGroup = $"customer-{customer.UserId}";
                await _hubContext.Clients.Group(customerGroup).SendAsync("JobCompleted", new
                {
                    jobId = job.Id.ToString(),
                    contractorName = contractor.Name,
                    jobType = job.JobType.ToString()
                });
                _logger.LogInformation("SignalR notification sent to customer {CustomerId} for job completed {JobId}",
                    customer.UserId, job.Id);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to send SignalR notification to customer for job completed {JobId}", job.Id);
            }

            // Send SignalR notification to dispatchers
            try
            {
                await _hubContext.Clients.All.SendAsync("JobStatusChanged", new
                {
                    jobId = job.Id.ToString(),
                    status = "Completed",
                    jobType = job.JobType.ToString()
                });
                _logger.LogInformation("SignalR notification sent to dispatchers for job status change {JobId}", job.Id);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to send SignalR notification to dispatchers for job status change {JobId}", job.Id);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error handling JobCompletedEvent for Job {JobId}",
                notification.JobId);
            // Don't re-throw - we want to log but not break the flow
        }
    }
}

