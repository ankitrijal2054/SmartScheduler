using SmartScheduler.Application.DTOs;

namespace SmartScheduler.Application.Services;

/// <summary>
/// Email service abstraction for sending transactional emails.
/// Implementations handle AWS SES integration and email template rendering.
/// </summary>
public interface IEmailService
{
    /// <summary>
    /// Send an email asynchronously.
    /// </summary>
    /// <param name="to">Recipient email address</param>
    /// <param name="subject">Email subject line</param>
    /// <param name="templateName">Name of the email template (e.g., "JobAssignedToCustomer")</param>
    /// <param name="templateData">Data to render in the template</param>
    /// <param name="cancellationToken">Cancellation token for the operation</param>
    /// <returns>Task representing the asynchronous operation. Returns true if email sent successfully.</returns>
    Task<bool> SendEmailAsync(
        string to,
        string subject,
        string templateName,
        EmailTemplateDataDto templateData,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Send a batch of emails asynchronously.
    /// </summary>
    /// <param name="emails">List of email specifications (to, subject, template)</param>
    /// <param name="cancellationToken">Cancellation token for the operation</param>
    /// <returns>Task representing the asynchronous operation. Returns number of emails sent successfully.</returns>
    Task<int> SendBatchEmailsAsync(
        IEnumerable<(string To, string Subject, string TemplateName, EmailTemplateDataDto Data)> emails,
        CancellationToken cancellationToken = default);
}

