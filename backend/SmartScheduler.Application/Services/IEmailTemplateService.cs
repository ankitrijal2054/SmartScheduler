using SmartScheduler.Application.DTOs;

namespace SmartScheduler.Application.Services;

/// <summary>
/// Email template service abstraction for rendering HTML and plain text email bodies.
/// </summary>
public interface IEmailTemplateService
{
    /// <summary>
    /// Render email template for job assigned event.
    /// </summary>
    /// <param name="data">Email template data</param>
    /// <returns>Tuple of (HtmlBody, TextBody)</returns>
    (string HtmlBody, string TextBody) RenderJobAssignedTemplate(EmailTemplateDataDto data);

    /// <summary>
    /// Render email template for job in-progress event.
    /// </summary>
    /// <param name="data">Email template data</param>
    /// <returns>Tuple of (HtmlBody, TextBody)</returns>
    (string HtmlBody, string TextBody) RenderJobInProgressTemplate(EmailTemplateDataDto data);

    /// <summary>
    /// Render email template for job completed event.
    /// </summary>
    /// <param name="data">Email template data</param>
    /// <returns>Tuple of (HtmlBody, TextBody)</returns>
    (string HtmlBody, string TextBody) RenderJobCompletedTemplate(EmailTemplateDataDto data);
}

