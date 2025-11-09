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
/// Event handler for JobAssignedEvent.
/// Sends email notification to customer when job is assigned to a contractor.
/// </summary>
public class JobAssignedEventHandler : INotificationHandler<JobAssignedEvent>
{
    private readonly IEmailService _emailService;
    private readonly ApplicationDbContext _dbContext;
    private readonly ILogger<JobAssignedEventHandler> _logger;
    private readonly IConfiguration _configuration;
    private readonly IHubContext<NotificationHub> _hubContext;

    public JobAssignedEventHandler(
        IEmailService emailService,
        ApplicationDbContext dbContext,
        ILogger<JobAssignedEventHandler> logger,
        IConfiguration configuration,
        IHubContext<NotificationHub> hubContext)
    {
        _emailService = emailService ?? throw new ArgumentNullException(nameof(emailService));
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        _hubContext = hubContext ?? throw new ArgumentNullException(nameof(hubContext));
    }

    public async Task Handle(JobAssignedEvent notification, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Processing JobAssignedEvent for Job {JobId}, Assignment {AssignmentId}",
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

            var assignment = await _dbContext.Assignments
                .FirstOrDefaultAsync(a => a.Id == notification.AssignmentId, cancellationToken);

            if (assignment == null)
            {
                _logger.LogWarning("Assignment {AssignmentId} not found", notification.AssignmentId);
                return;
            }

            // Build email data
            var frontendBaseUrl = _configuration["Frontend:BaseUrl"] ?? "http://localhost:5173";
            var jobTrackingUrl = $"{frontendBaseUrl}/customer/jobs/{notification.JobId}";
            var eta = CalculateETA(assignment.AssignedAt, contractor);

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
                ETA = eta,
                JobTrackingUrl = jobTrackingUrl,
                RatingUrl = $"{jobTrackingUrl}/review"
            };

            // Send email
            var success = await _emailService.SendEmailAsync(
                to: customer.User.Email,
                subject: "Your Job Has Been Assigned!",
                templateName: "JobAssignedToCustomer",
                templateData: emailData,
                cancellationToken: cancellationToken);

            if (success)
            {
                _logger.LogInformation("Email sent successfully for Job {JobId} to {Email}",
                    notification.JobId, customer.User.Email);
            }
            else
            {
                _logger.LogWarning("Email failed to send for Job {JobId} to {Email}",
                    notification.JobId, customer.User.Email);
            }

            // Send SignalR notification to customer
            try
            {
                var customerGroup = $"customer-{customer.UserId}";
                await _hubContext.Clients.Group(customerGroup).SendAsync("ContractorAssigned", new
                {
                    jobId = job.Id.ToString(),
                    contractorName = contractor.Name,
                    contractorRating = contractor.AverageRating,
                    jobType = job.JobType.ToString()
                });
                _logger.LogInformation("SignalR notification sent to customer {CustomerId} for job {JobId}",
                    customer.UserId, job.Id);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to send SignalR notification to customer for job {JobId}", job.Id);
            }

            // Send SignalR notification to dispatchers (all dispatchers see all jobs)
            try
            {
                await _hubContext.Clients.All.SendAsync("JobAssigned", new
                {
                    jobId = job.Id.ToString(),
                    contractorName = contractor.Name,
                    jobType = job.JobType.ToString()
                });
                _logger.LogInformation("SignalR notification sent to dispatchers for job {JobId}", job.Id);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to send SignalR notification to dispatchers for job {JobId}", job.Id);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error handling JobAssignedEvent for Job {JobId}",
                notification.JobId);
            // Don't re-throw - we want to log but not break the flow
        }
    }

    /// <summary>
    /// Calculate estimated time of arrival (ETA) based on contractor location and job location.
    /// </summary>
    private static string CalculateETA(DateTime assignedAt, Contractor contractor)
    {
        // Simple ETA calculation - in real scenario would use distance service
        // For now, assume 30 minutes travel time + 5 minutes buffer
        var estimatedArrival = assignedAt.AddMinutes(30);
        var timeUntilArrival = estimatedArrival - DateTime.UtcNow;

        if (timeUntilArrival.TotalMinutes < 0)
        {
            return "Should be arriving soon";
        }

        if (timeUntilArrival.TotalMinutes < 60)
        {
            return $"In approximately {(int)timeUntilArrival.TotalMinutes} minutes";
        }

        var hours = (int)timeUntilArrival.TotalHours;
        return $"In approximately {hours} hour{(hours > 1 ? "s" : "")}";
    }
}

