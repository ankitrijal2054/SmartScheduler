using Microsoft.Extensions.Logging;
using SmartScheduler.Application.DTOs;

namespace SmartScheduler.Application.Services;

/// <summary>
/// Service for rendering email templates with data.
/// Generates HTML and plain text email bodies for transactional emails.
/// </summary>
public class EmailTemplateService : IEmailTemplateService
{
    private readonly ILogger<EmailTemplateService> _logger;

    public EmailTemplateService(ILogger<EmailTemplateService> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Generate email template body (HTML and plain text) for job assigned event.
    /// </summary>
    public (string HtmlBody, string TextBody) RenderJobAssignedTemplate(EmailTemplateDataDto data)
    {
        var ratingDisplay = data.ContractorRating.HasValue
            ? $"{data.ContractorRating:F1}★"
            : "No rating yet";

        var htmlBody = $@"
<!DOCTYPE html>
<html>
<head>
    <meta charset='UTF-8'>
    <meta name='viewport' content='width=device-width, initial-scale=1.0'>
    <style>
        body {{ font-family: Arial, sans-serif; line-height: 1.6; color: #333; }}
        .container {{ max-width: 600px; margin: 0 auto; padding: 20px; }}
        .header {{ background-color: #4F46E5; color: white; padding: 20px; text-align: center; border-radius: 8px 8px 0 0; }}
        .content {{ background-color: #f9fafb; padding: 20px; border-left: 4px solid #4F46E5; }}
        .contractor-info {{ background-color: white; padding: 15px; margin: 15px 0; border-radius: 8px; border: 1px solid #e5e7eb; }}
        .info-row {{ display: flex; justify-content: space-between; margin: 8px 0; }}
        .info-label {{ font-weight: bold; color: #6b7280; }}
        .info-value {{ color: #1f2937; }}
        .cta-button {{ display: inline-block; background-color: #4F46E5; color: white; padding: 12px 24px; text-decoration: none; border-radius: 6px; margin-top: 15px; text-align: center; }}
        .footer {{ background-color: #f3f4f6; padding: 15px; text-align: center; font-size: 12px; color: #6b7280; border-top: 1px solid #e5e7eb; }}
        @media (max-width: 600px) {{
            .container {{ padding: 10px; }}
            .contractor-info {{ padding: 10px; }}
            .info-row {{ flex-direction: column; }}
        }}
    </style>
</head>
<body>
    <div class='container'>
        <div class='header'>
            <h1>Your Job Has Been Assigned! 🎉</h1>
        </div>
        <div class='content'>
            <p>Hi {data.CustomerName},</p>
            <p>Great news! We've found a qualified contractor for your {data.JobType} job. Here are their details:</p>
            
            <div class='contractor-info'>
                <div class='info-row'>
                    <span class='info-label'>Contractor Name:</span>
                    <span class='info-value'>{data.ContractorName}</span>
                </div>
                <div class='info-row'>
                    <span class='info-label'>Rating:</span>
                    <span class='info-value'>{ratingDisplay}</span>
                </div>
                <div class='info-row'>
                    <span class='info-label'>Phone:</span>
                    <span class='info-value'><a href='tel:{data.ContractorPhone}'>{data.ContractorPhone}</a></span>
                </div>
                <div class='info-row'>
                    <span class='info-label'>Estimated Arrival:</span>
                    <span class='info-value'>{data.ETA}</span>
                </div>
            </div>

            <div class='contractor-info'>
                <div class='info-row'>
                    <span class='info-label'>Job Type:</span>
                    <span class='info-value'>{data.JobType}</span>
                </div>
                <div class='info-row'>
                    <span class='info-label'>Location:</span>
                    <span class='info-value'>{data.Location}</span>
                </div>
                <div class='info-row'>
                    <span class='info-label'>Scheduled Date:</span>
                    <span class='info-value'>{data.DesiredDateTime:MMMM d, yyyy 'at' h:mm tt}</span>
                </div>
            </div>

            <p>You can view full job details and track progress by clicking the button below:</p>
            <center>
                <a href='{data.JobTrackingUrl}' class='cta-button'>View Job Details</a>
            </center>

            <p>If you have any questions, feel free to contact the contractor directly or reach out to our support team.</p>
        </div>
        <div class='footer'>
            <p>© 2025 SmartScheduler. All rights reserved.</p>
        </div>
    </div>
</body>
</html>";

        var textBody = $@"Your Job Has Been Assigned!

Hi {data.CustomerName},

Great news! We've found a qualified contractor for your {data.JobType} job. Here are their details:

Contractor Name: {data.ContractorName}
Rating: {ratingDisplay}
Phone: {data.ContractorPhone}
Estimated Arrival: {data.ETA}

Job Details:
Job Type: {data.JobType}
Location: {data.Location}
Scheduled Date: {data.DesiredDateTime:MMMM d, yyyy 'at' h:mm tt}

You can view full job details and track progress at:
{data.JobTrackingUrl}

If you have any questions, feel free to contact the contractor directly.

© 2025 SmartScheduler. All rights reserved.";

        return (htmlBody, textBody);
    }

    /// <summary>
    /// Generate email template body for job completed event.
    /// </summary>
    public (string HtmlBody, string TextBody) RenderJobCompletedTemplate(EmailTemplateDataDto data)
    {
        var htmlBody = $@"
<!DOCTYPE html>
<html>
<head>
    <meta charset='UTF-8'>
    <meta name='viewport' content='width=device-width, initial-scale=1.0'>
    <style>
        body {{ font-family: Arial, sans-serif; line-height: 1.6; color: #333; }}
        .container {{ max-width: 600px; margin: 0 auto; padding: 20px; }}
        .header {{ background-color: #10b981; color: white; padding: 20px; text-align: center; border-radius: 8px 8px 0 0; }}
        .content {{ background-color: #f9fafb; padding: 20px; border-left: 4px solid #10b981; }}
        .info-box {{ background-color: white; padding: 15px; margin: 15px 0; border-radius: 8px; border: 1px solid #e5e7eb; }}
        .cta-button {{ display: inline-block; background-color: #10b981; color: white; padding: 12px 24px; text-decoration: none; border-radius: 6px; margin-top: 15px; text-align: center; }}
        .footer {{ background-color: #f3f4f6; padding: 15px; text-align: center; font-size: 12px; color: #6b7280; border-top: 1px solid #e5e7eb; }}
        @media (max-width: 600px) {{
            .container {{ padding: 10px; }}
        }}
    </style>
</head>
<body>
    <div class='container'>
        <div class='header'>
            <h1>Your Job is Complete! ✓</h1>
        </div>
        <div class='content'>
            <p>Hi {data.CustomerName},</p>
            <p>Excellent news! Your {data.JobType} job at {data.Location} has been successfully completed by {data.ContractorName}.</p>
            
            <div class='info-box'>
                <p><strong>Job Details:</strong></p>
                <p>Type: {data.JobType}<br/>
                   Location: {data.Location}<br/>
                   Completed: {DateTime.UtcNow:MMMM d, yyyy 'at' h:mm tt}</p>
            </div>

            <p>We'd love to hear about your experience! Please take a moment to rate {data.ContractorName} and leave feedback for other customers.</p>
            
            <center>
                <a href='{data.RatingUrl}' class='cta-button'>Rate Contractor & Leave Review</a>
            </center>

            <p>Your feedback helps us maintain quality service and helps contractors improve their work.</p>
        </div>
        <div class='footer'>
            <p>© 2025 SmartScheduler. All rights reserved.</p>
        </div>
    </div>
</body>
</html>";

        var textBody = $@"Your Job is Complete!

Hi {data.CustomerName},

Excellent news! Your {data.JobType} job at {data.Location} has been successfully completed by {data.ContractorName}.

Job Details:
Type: {data.JobType}
Location: {data.Location}
Completed: {DateTime.UtcNow:MMMM d, yyyy 'at' h:mm tt}

We'd love to hear about your experience! Please rate {data.ContractorName} and leave feedback:
{data.RatingUrl}

Your feedback helps us maintain quality service and helps contractors improve their work.

© 2025 SmartScheduler. All rights reserved.";

        return (htmlBody, textBody);
    }

    /// <summary>
    /// Generate email template body for job in-progress event.
    /// </summary>
    public (string HtmlBody, string TextBody) RenderJobInProgressTemplate(EmailTemplateDataDto data)
    {
        var htmlBody = $@"
<!DOCTYPE html>
<html>
<head>
    <meta charset='UTF-8'>
    <meta name='viewport' content='width=device-width, initial-scale=1.0'>
    <style>
        body {{ font-family: Arial, sans-serif; line-height: 1.6; color: #333; }}
        .container {{ max-width: 600px; margin: 0 auto; padding: 20px; }}
        .header {{ background-color: #f59e0b; color: white; padding: 20px; text-align: center; border-radius: 8px 8px 0 0; }}
        .content {{ background-color: #f9fafb; padding: 20px; border-left: 4px solid #f59e0b; }}
        .info-box {{ background-color: white; padding: 15px; margin: 15px 0; border-radius: 8px; border: 1px solid #e5e7eb; }}
        .cta-button {{ display: inline-block; background-color: #f59e0b; color: white; padding: 12px 24px; text-decoration: none; border-radius: 6px; margin-top: 15px; text-align: center; }}
        .footer {{ background-color: #f3f4f6; padding: 15px; text-align: center; font-size: 12px; color: #6b7280; border-top: 1px solid #e5e7eb; }}
        @media (max-width: 600px) {{
            .container {{ padding: 10px; }}
        }}
    </style>
</head>
<body>
    <div class='container'>
        <div class='header'>
            <h1>Work in Progress! 👷</h1>
        </div>
        <div class='content'>
            <p>Hi {data.CustomerName},</p>
            <p>{data.ContractorName} is on the way to your job location!</p>
            
            <div class='info-box'>
                <p><strong>Job Information:</strong></p>
                <p>Location: {data.Location}<br/>
                   Job Type: {data.JobType}<br/>
                   Contractor: {data.ContractorName}</p>
            </div>

            <p>You can track the progress and view detailed information about your job:</p>
            
            <center>
                <a href='{data.JobTrackingUrl}' class='cta-button'>Track Job Progress</a>
            </center>

            <p>If you need to contact {data.ContractorName}, call them at {data.ContractorPhone}.</p>
        </div>
        <div class='footer'>
            <p>© 2025 SmartScheduler. All rights reserved.</p>
        </div>
    </div>
</body>
</html>";

        var textBody = $@"Work in Progress!

Hi {data.CustomerName},

{data.ContractorName} is on the way to your job location!

Job Information:
Location: {data.Location}
Job Type: {data.JobType}
Contractor: {data.ContractorName}

Track the progress at:
{data.JobTrackingUrl}

If you need to contact {data.ContractorName}, call: {data.ContractorPhone}

© 2025 SmartScheduler. All rights reserved.";

        return (htmlBody, textBody);
    }
}

