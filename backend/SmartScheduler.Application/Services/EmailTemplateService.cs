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

    /// <summary>
    /// Generate email template body for job assigned to contractor event.
    /// </summary>
    public (string HtmlBody, string TextBody) RenderJobAssignedToContractorTemplate(EmailTemplateDataDto data)
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
        .header {{ background-color: #3B82F6; color: white; padding: 20px; text-align: center; border-radius: 8px 8px 0 0; }}
        .content {{ background-color: #f9fafb; padding: 20px; border-left: 4px solid #3B82F6; }}
        .job-info {{ background-color: white; padding: 15px; margin: 15px 0; border-radius: 8px; border: 1px solid #e5e7eb; }}
        .info-row {{ display: flex; justify-content: space-between; margin: 8px 0; }}
        .info-label {{ font-weight: bold; color: #6b7280; }}
        .info-value {{ color: #1f2937; }}
        .action-buttons {{ display: flex; gap: 10px; margin-top: 20px; flex-wrap: wrap; }}
        .btn {{ display: inline-block; padding: 12px 24px; text-decoration: none; border-radius: 6px; text-align: center; font-weight: bold; flex: 1; min-width: 150px; }}
        .btn-accept {{ background-color: #10b981; color: white; }}
        .btn-decline {{ background-color: #ef4444; color: white; }}
        .btn-dashboard {{ background-color: #3B82F6; color: white; }}
        .footer {{ background-color: #f3f4f6; padding: 15px; text-align: center; font-size: 12px; color: #6b7280; border-top: 1px solid #e5e7eb; }}
        @media (max-width: 600px) {{
            .container {{ padding: 10px; }}
            .job-info {{ padding: 10px; }}
            .info-row {{ flex-direction: column; }}
            .action-buttons {{ flex-direction: column; }}
        }}
    </style>
</head>
<body>
    <div class='container'>
        <div class='header'>
            <h1>New Job Assignment! 🎯</h1>
        </div>
        <div class='content'>
            <p>Hi {data.ContractorName},</p>
            <p>Congratulations! You've been assigned a new job. Here are the details:</p>
            
            <div class='job-info'>
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
                <div class='info-row'>
                    <span class='info-label'>Customer:</span>
                    <span class='info-value'>{data.CustomerName}</span>
                </div>
                <div class='info-row'>
                    <span class='info-label'>Description:</span>
                    <span class='info-value'>{data.Description}</span>
                </div>
            </div>

            <p>Please confirm your availability by clicking one of the buttons below:</p>
            
            <div class='action-buttons'>
                <a href='{data.AcceptJobLink}' class='btn btn-accept'>✓ Accept Job</a>
                <a href='{data.DeclineJobLink}' class='btn btn-decline'>✗ Decline Job</a>
            </div>

            <p style='margin-top: 20px;'>Or visit your contractor dashboard to view all assigned jobs:</p>
            <center>
                <a href='{data.JobTrackingUrl}' class='btn btn-dashboard'>View Dashboard</a>
            </center>

            <p>If you have any questions about this job, please log in to your contractor account or contact support.</p>
        </div>
        <div class='footer'>
            <p>© 2025 SmartScheduler. All rights reserved.</p>
        </div>
    </div>
</body>
</html>";

        var textBody = $@"New Job Assignment!

Hi {data.ContractorName},

Congratulations! You've been assigned a new job. Here are the details:

Job Type: {data.JobType}
Location: {data.Location}
Scheduled Date: {data.DesiredDateTime:MMMM d, yyyy 'at' h:mm tt}
Customer: {data.CustomerName}
Description: {data.Description}

Please confirm your availability by clicking one of the links below:

Accept: {data.AcceptJobLink}
Decline: {data.DeclineJobLink}

Or visit your contractor dashboard:
{data.JobTrackingUrl}

If you have any questions about this job, please log in to your contractor account or contact support.

© 2025 SmartScheduler. All rights reserved.";

        return (htmlBody, textBody);
    }

    /// <summary>
    /// Generate email template body for job cancelled notification to contractor.
    /// </summary>
    public (string HtmlBody, string TextBody) RenderJobCancelledForContractorTemplate(EmailTemplateDataDto data)
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
        .header {{ background-color: #ef4444; color: white; padding: 20px; text-align: center; border-radius: 8px 8px 0 0; }}
        .content {{ background-color: #f9fafb; padding: 20px; border-left: 4px solid #ef4444; }}
        .job-info {{ background-color: white; padding: 15px; margin: 15px 0; border-radius: 8px; border: 1px solid #e5e7eb; }}
        .info-row {{ display: flex; justify-content: space-between; margin: 8px 0; }}
        .info-label {{ font-weight: bold; color: #6b7280; }}
        .info-value {{ color: #1f2937; }}
        .cta-button {{ display: inline-block; background-color: #3B82F6; color: white; padding: 12px 24px; text-decoration: none; border-radius: 6px; margin-top: 15px; text-align: center; }}
        .footer {{ background-color: #f3f4f6; padding: 15px; text-align: center; font-size: 12px; color: #6b7280; border-top: 1px solid #e5e7eb; }}
        @media (max-width: 600px) {{
            .container {{ padding: 10px; }}
            .job-info {{ padding: 10px; }}
        }}
    </style>
</head>
<body>
    <div class='container'>
        <div class='header'>
            <h1>Job Cancelled ⚠️</h1>
        </div>
        <div class='content'>
            <p>Hi {data.ContractorName},</p>
            <p>Unfortunately, your assigned job has been cancelled. Here are the details:</p>
            
            <div class='job-info'>
                <div class='info-row'>
                    <span class='info-label'>Job Type:</span>
                    <span class='info-value'>{data.JobType}</span>
                </div>
                <div class='info-row'>
                    <span class='info-label'>Location:</span>
                    <span class='info-value'>{data.Location}</span>
                </div>
                <div class='info-row'>
                    <span class='info-label'>Original Date:</span>
                    <span class='info-value'>{data.DesiredDateTime:MMMM d, yyyy 'at' h:mm tt}</span>
                </div>
                {(string.IsNullOrEmpty(data.CancellationReason) ? "" : $"<div class='info-row'><span class='info-label'>Reason:</span><span class='info-value'>{data.CancellationReason}</span></div>")}
            </div>

            <p>Don't worry! You may be assigned to other jobs that match your expertise. Check your contractor dashboard for available opportunities.</p>
            
            <center>
                <a href='{data.JobTrackingUrl}' class='cta-button'>View Your Dashboard</a>
            </center>

            <p>If you have any questions, please contact our support team.</p>
        </div>
        <div class='footer'>
            <p>© 2025 SmartScheduler. All rights reserved.</p>
        </div>
    </div>
</body>
</html>";

        var reasonText = string.IsNullOrEmpty(data.CancellationReason) ? "" : $"\nReason: {data.CancellationReason}";

        var textBody = $@"Job Cancelled

Hi {data.ContractorName},

Unfortunately, your assigned job has been cancelled. Here are the details:

Job Type: {data.JobType}
Location: {data.Location}
Original Date: {data.DesiredDateTime:MMMM d, yyyy 'at' h:mm tt}{reasonText}

Don't worry! You may be assigned to other jobs that match your expertise. Check your contractor dashboard for available opportunities:
{data.JobTrackingUrl}

If you have any questions, please contact our support team.

© 2025 SmartScheduler. All rights reserved.";

        return (htmlBody, textBody);
    }

    /// <summary>
    /// Generate email template body for job schedule change notification to contractor.
    /// </summary>
    public (string HtmlBody, string TextBody) RenderJobScheduleChangedForContractorTemplate(EmailTemplateDataDto data)
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
        .schedule-info {{ background-color: white; padding: 15px; margin: 15px 0; border-radius: 8px; border: 1px solid #e5e7eb; }}
        .schedule-row {{ display: flex; align-items: center; margin: 15px 0; }}
        .schedule-time {{ flex: 1; }}
        .schedule-label {{ font-weight: bold; color: #6b7280; margin-bottom: 5px; }}
        .schedule-value {{ color: #1f2937; font-size: 16px; }}
        .arrow {{ flex: 0; text-align: center; font-size: 20px; color: #3B82F6; margin: 0 10px; }}
        .job-details {{ background-color: white; padding: 15px; margin: 15px 0; border-radius: 8px; border: 1px solid #e5e7eb; }}
        .info-row {{ display: flex; justify-content: space-between; margin: 8px 0; }}
        .info-label {{ font-weight: bold; color: #6b7280; }}
        .info-value {{ color: #1f2937; }}
        .cta-button {{ display: inline-block; background-color: #3B82F6; color: white; padding: 12px 24px; text-decoration: none; border-radius: 6px; margin-top: 15px; text-align: center; }}
        .footer {{ background-color: #f3f4f6; padding: 15px; text-align: center; font-size: 12px; color: #6b7280; border-top: 1px solid #e5e7eb; }}
        @media (max-width: 600px) {{
            .container {{ padding: 10px; }}
            .schedule-row {{ flex-direction: column; }}
            .arrow {{ margin: 10px 0; }}
        }}
    </style>
</head>
<body>
    <div class='container'>
        <div class='header'>
            <h1>Schedule Updated 📅</h1>
        </div>
        <div class='content'>
            <p>Hi {data.ContractorName},</p>
            <p>The schedule for your assigned job has been updated. Please review the new timing:</p>
            
            <div class='schedule-info'>
                <div class='schedule-row'>
                    <div class='schedule-time'>
                        <div class='schedule-label'>Previous Time:</div>
                        <div class='schedule-value'>{(data.OldScheduledDateTime.HasValue ? data.OldScheduledDateTime.Value.ToString("MMMM d, yyyy 'at' h:mm tt") : "N/A")}</div>
                    </div>
                    <div class='arrow'>→</div>
                    <div class='schedule-time'>
                        <div class='schedule-label'>New Time:</div>
                        <div class='schedule-value'>{(data.NewScheduledDateTime.HasValue ? data.NewScheduledDateTime.Value.ToString("MMMM d, yyyy 'at' h:mm tt") : "N/A")}</div>
                    </div>
                </div>
            </div>

            <div class='job-details'>
                <p><strong>Job Details:</strong></p>
                <div class='info-row'>
                    <span class='info-label'>Job Type:</span>
                    <span class='info-value'>{data.JobType}</span>
                </div>
                <div class='info-row'>
                    <span class='info-label'>Location:</span>
                    <span class='info-value'>{data.Location}</span>
                </div>
            </div>

            <p>Please confirm that the new schedule works for you. If you cannot make the new time, please contact us immediately through your contractor dashboard.</p>
            
            <center>
                <a href='{data.JobTrackingUrl}' class='cta-button'>View Full Job Details</a>
            </center>

            <p>Thank you for your flexibility and professionalism!</p>
        </div>
        <div class='footer'>
            <p>© 2025 SmartScheduler. All rights reserved.</p>
        </div>
    </div>
</body>
</html>";

        var oldTime = data.OldScheduledDateTime.HasValue ? data.OldScheduledDateTime.Value.ToString("MMMM d, yyyy 'at' h:mm tt") : "N/A";
        var newTime = data.NewScheduledDateTime.HasValue ? data.NewScheduledDateTime.Value.ToString("MMMM d, yyyy 'at' h:mm tt") : "N/A";

        var textBody = $@"Schedule Updated

Hi {data.ContractorName},

The schedule for your assigned job has been updated. Please review the new timing:

Previous Time: {oldTime}
New Time: {newTime}

Job Details:
Job Type: {data.JobType}
Location: {data.Location}

Please confirm that the new schedule works for you. If you cannot make the new time, please contact us immediately through your contractor dashboard:
{data.JobTrackingUrl}

Thank you for your flexibility and professionalism!

© 2025 SmartScheduler. All rights reserved.";

        return (htmlBody, textBody);
    }

    /// <summary>
    /// Generate email template body for rating received by contractor.
    /// </summary>
    public (string HtmlBody, string TextBody) RenderRatingReceivedByContractorTemplate(EmailTemplateDataDto data)
    {
        var ratingStars = data.Rating.HasValue ? string.Concat(Enumerable.Repeat("⭐", data.Rating.Value)) : "N/A";
        var ratingText = data.Rating.HasValue ? $"{data.Rating} out of 5 stars" : "N/A";

        var htmlBody = $@"
<!DOCTYPE html>
<html>
<head>
    <meta charset='UTF-8'>
    <meta name='viewport' content='width=device-width, initial-scale=1.0'>
    <style>
        body {{ font-family: Arial, sans-serif; line-height: 1.6; color: #333; }}
        .container {{ max-width: 600px; margin: 0 auto; padding: 20px; }}
        .header {{ background-color: #8b5cf6; color: white; padding: 20px; text-align: center; border-radius: 8px 8px 0 0; }}
        .content {{ background-color: #f9fafb; padding: 20px; border-left: 4px solid #8b5cf6; }}
        .rating-box {{ background-color: white; padding: 20px; margin: 15px 0; border-radius: 8px; border: 1px solid #e5e7eb; text-align: center; }}
        .rating-stars {{ font-size: 36px; margin: 10px 0; }}
        .rating-text {{ font-size: 18px; font-weight: bold; color: #8b5cf6; margin: 10px 0; }}
        .review-box {{ background-color: #f3f4f6; padding: 15px; margin: 15px 0; border-radius: 8px; border-left: 4px solid #8b5cf6; font-style: italic; }}
        .customer-name {{ font-weight: bold; color: #1f2937; margin-top: 10px; }}
        .cta-button {{ display: inline-block; background-color: #8b5cf6; color: white; padding: 12px 24px; text-decoration: none; border-radius: 6px; margin-top: 15px; text-align: center; }}
        .footer {{ background-color: #f3f4f6; padding: 15px; text-align: center; font-size: 12px; color: #6b7280; border-top: 1px solid #e5e7eb; }}
        @media (max-width: 600px) {{
            .container {{ padding: 10px; }}
            .rating-box {{ padding: 15px; }}
        }}
    </style>
</head>
<body>
    <div class='container'>
        <div class='header'>
            <h1>You Received a Rating! 🌟</h1>
        </div>
        <div class='content'>
            <p>Hi {data.ContractorName},</p>
            <p>{data.CustomerName} has left you a rating for the recent job you completed.</p>
            
            <div class='rating-box'>
                <div class='rating-stars'>{ratingStars}</div>
                <div class='rating-text'>{ratingText}</div>
            </div>

            {(string.IsNullOrEmpty(data.ReviewComment) ? "" : $@"<p><strong>Their feedback:</strong></p>
            <div class='review-box'>
                ""{data.ReviewComment}""
                <div class='customer-name'>— {data.CustomerName}</div>
            </div>")}

            <p>Great work! Positive ratings help you attract more customers and build your reputation. Keep up the excellent service!</p>
            
            <center>
                <a href='{data.ContractorProfileUrl}' class='cta-button'>View Your Profile & Ratings</a>
            </center>

            <p>Thank you for being an outstanding contractor partner!</p>
        </div>
        <div class='footer'>
            <p>© 2025 SmartScheduler. All rights reserved.</p>
        </div>
    </div>
</body>
</html>";

        var reviewSection = string.IsNullOrEmpty(data.ReviewComment) 
            ? "" 
            : $@"

Their feedback:
""{data.ReviewComment}""
— {data.CustomerName}";

        var textBody = $@"You Received a Rating!

Hi {data.ContractorName},

{data.CustomerName} has left you a rating for the recent job you completed.

Rating: {ratingText}
{ratingStars}{reviewSection}

Great work! Positive ratings help you attract more customers and build your reputation. Keep up the excellent service!

View your profile and all ratings:
{data.ContractorProfileUrl}

Thank you for being an outstanding contractor partner!

© 2025 SmartScheduler. All rights reserved.";

        return (htmlBody, textBody);
    }
}

