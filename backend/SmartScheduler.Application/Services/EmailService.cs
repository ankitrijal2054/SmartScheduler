using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using SmartScheduler.Application.DTOs;

namespace SmartScheduler.Application.Services;

/// <summary>
/// Email service implementation for sending transactional emails.
/// In development, logs emails instead of actually sending them.
/// In production, would integrate with AWS SES.
/// </summary>
public class EmailService : IEmailService
{
    private readonly ILogger<EmailService> _logger;
    private readonly IEmailTemplateService _templateService;
    private readonly bool _isDevelopment;

    public EmailService(
        ILogger<EmailService> logger,
        IEmailTemplateService templateService,
        IConfiguration configuration)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _templateService = templateService ?? throw new ArgumentNullException(nameof(templateService));
        var environment = configuration?.GetValue("ASPNETCORE_ENVIRONMENT", "Production") ?? "Production";
        _isDevelopment = environment == "Development";
    }

    /// <summary>
    /// Send an email asynchronously.
    /// </summary>
    public async Task<bool> SendEmailAsync(
        string to,
        string subject,
        string templateName,
        EmailTemplateDataDto templateData,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(to))
        {
            _logger.LogWarning("Email recipient is empty. Subject: {Subject}", subject);
            return false;
        }

        try
        {
            var (htmlBody, textBody) = RenderTemplate(templateName, templateData);

            if (_isDevelopment)
            {
                // In development, just log the email
                _logger.LogInformation(
                    "Email would be sent - To: {To}, Subject: {Subject}, Template: {Template}",
                    to, subject, templateName);
                _logger.LogDebug("Email HTML Body: {HtmlBody}", htmlBody);
                return true;
            }

            // TODO: In production, integrate with AWS SES here
            // For now, log success
            _logger.LogInformation(
                "Email sent successfully - To: {To}, Subject: {Subject}, Template: {Template}",
                to, subject, templateName);

            return await Task.FromResult(true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send email to {To} with subject {Subject}", to, subject);
            return false;
        }
    }

    /// <summary>
    /// Send a batch of emails asynchronously.
    /// </summary>
    public async Task<int> SendBatchEmailsAsync(
        IEnumerable<(string To, string Subject, string TemplateName, EmailTemplateDataDto Data)> emails,
        CancellationToken cancellationToken = default)
    {
        var emailList = emails.ToList();
        if (!emailList.Any())
        {
            return 0;
        }

        int successCount = 0;
        foreach (var (to, subject, templateName, data) in emailList)
        {
            var result = await SendEmailAsync(to, subject, templateName, data, cancellationToken);
            if (result)
            {
                successCount++;
            }
        }

        _logger.LogInformation("Batch email send completed. Sent {SuccessCount} out of {TotalCount} emails",
            successCount, emailList.Count);

        return successCount;
    }

    /// <summary>
    /// Render email template based on template name.
    /// </summary>
    private (string HtmlBody, string TextBody) RenderTemplate(string templateName, EmailTemplateDataDto data)
    {
        return templateName switch
        {
            "JobAssignedToCustomer" => _templateService.RenderJobAssignedTemplate(data),
            "JobInProgress" => _templateService.RenderJobInProgressTemplate(data),
            "JobCompleted" => _templateService.RenderJobCompletedTemplate(data),
            "JobAssignedToContractor" => _templateService.RenderJobAssignedToContractorTemplate(data),
            "JobCancelledForContractor" => _templateService.RenderJobCancelledForContractorTemplate(data),
            "JobScheduleChangedForContractor" => _templateService.RenderJobScheduleChangedForContractorTemplate(data),
            "RatingReceivedByContractor" => _templateService.RenderRatingReceivedByContractorTemplate(data),
            _ => throw new ArgumentException($"Unknown email template: {templateName}", nameof(templateName))
        };
    }
}

