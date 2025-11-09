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
/// Event handler for RatingPostedEvent.
/// Sends email notification to contractor when they receive a rating/review.
/// </summary>
public class RatingPostedContractorEmailHandler : INotificationHandler<RatingPostedEvent>
{
    private readonly IEmailService _emailService;
    private readonly ApplicationDbContext _dbContext;
    private readonly ILogger<RatingPostedContractorEmailHandler> _logger;
    private readonly IConfiguration _configuration;

    public RatingPostedContractorEmailHandler(
        IEmailService emailService,
        ApplicationDbContext dbContext,
        ILogger<RatingPostedContractorEmailHandler> logger,
        IConfiguration configuration)
    {
        _emailService = emailService ?? throw new ArgumentNullException(nameof(emailService));
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
    }

    public async Task Handle(RatingPostedEvent notification, CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Processing RatingPostedEvent for Review {ReviewId}, Contractor {ContractorId}",
            notification.ReviewId, notification.ContractorId);

        try
        {
            // Fetch contractor information
            var contractor = await _dbContext.Contractors
                .Include(c => c.User)
                .FirstOrDefaultAsync(c => c.Id == notification.ContractorId, cancellationToken);

            if (contractor?.User == null)
            {
                _logger.LogWarning("Contractor {ContractorId} not found for event", notification.ContractorId);
                return;
            }

            // Fetch customer information
            var customer = await _dbContext.Customers
                .FirstOrDefaultAsync(c => c.Id == notification.CustomerId, cancellationToken);

            if (customer == null)
            {
                _logger.LogWarning("Customer {CustomerId} not found for event", notification.CustomerId);
                return;
            }

            // Build email data for contractor notification
            var frontendBaseUrl = _configuration["Frontend:BaseUrl"] ?? "http://localhost:5173";
            var contractorProfileUrl = $"{frontendBaseUrl}/contractor/profile";

            var emailData = new EmailTemplateDataDto
            {
                ContractorEmail = contractor.User.Email,
                ContractorName = contractor.Name,
                CustomerName = customer.Name,
                Rating = notification.Rating,
                ReviewComment = notification.Comment ?? string.Empty,
                ContractorProfileUrl = contractorProfileUrl,
                JobId = notification.JobId
            };

            // Send email
            var success = await _emailService.SendEmailAsync(
                to: contractor.User.Email,
                subject: $"You Received a {notification.Rating}-Star Rating from {customer.Name}!",
                templateName: "RatingReceivedByContractor",
                templateData: emailData,
                cancellationToken: cancellationToken);

            if (success)
            {
                _logger.LogInformation(
                    "Email sent successfully to contractor {ContractorId} for rating {ReviewId}",
                    notification.ContractorId, notification.ReviewId);
            }
            else
            {
                _logger.LogWarning(
                    "Email failed to send to contractor {ContractorId} for rating {ReviewId}",
                    notification.ContractorId, notification.ReviewId);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error handling RatingPostedEvent for Review {ReviewId}",
                notification.ReviewId);
            // Don't re-throw - we want to log but not break the flow
        }
    }
}

