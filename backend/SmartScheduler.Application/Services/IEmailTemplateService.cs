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

    /// <summary>
    /// Render email template for job assigned to contractor event.
    /// </summary>
    /// <param name="data">Email template data</param>
    /// <returns>Tuple of (HtmlBody, TextBody)</returns>
    (string HtmlBody, string TextBody) RenderJobAssignedToContractorTemplate(EmailTemplateDataDto data);

    /// <summary>
    /// Render email template for job cancelled notification to contractor.
    /// </summary>
    /// <param name="data">Email template data</param>
    /// <returns>Tuple of (HtmlBody, TextBody)</returns>
    (string HtmlBody, string TextBody) RenderJobCancelledForContractorTemplate(EmailTemplateDataDto data);

    /// <summary>
    /// Render email template for job schedule change notification to contractor.
    /// </summary>
    /// <param name="data">Email template data</param>
    /// <returns>Tuple of (HtmlBody, TextBody)</returns>
    (string HtmlBody, string TextBody) RenderJobScheduleChangedForContractorTemplate(EmailTemplateDataDto data);

    /// <summary>
    /// Render email template for rating received by contractor.
    /// </summary>
    /// <param name="data">Email template data</param>
    /// <returns>Tuple of (HtmlBody, TextBody)</returns>
    (string HtmlBody, string TextBody) RenderRatingReceivedByContractorTemplate(EmailTemplateDataDto data);
}

