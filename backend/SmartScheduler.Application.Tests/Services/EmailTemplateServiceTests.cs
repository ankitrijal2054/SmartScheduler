using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using SmartScheduler.Application.DTOs;
using SmartScheduler.Application.Services;

namespace SmartScheduler.Application.Tests.Services;

/// <summary>
/// Unit tests for EmailTemplateService.
/// </summary>
public class EmailTemplateServiceTests
{
    private readonly EmailTemplateService _service;

    public EmailTemplateServiceTests()
    {
        var loggerMock = new Mock<ILogger<EmailTemplateService>>();
        _service = new EmailTemplateService(loggerMock.Object);
    }

    private readonly EmailTemplateDataDto _testData = new()
    {
        CustomerEmail = "customer@test.com",
        CustomerName = "John Customer",
        JobId = 1,
        JobType = "Plumbing",
        Location = "123 Main St, Denver, CO",
        Description = "Fix leaky faucet",
        DesiredDateTime = new DateTime(2025, 12, 15, 14, 0, 0),
        ContractorName = "Jane Plumber",
        ContractorPhone = "555-1234",
        ContractorRating = 4.8m,
        ETA = "In approximately 30 minutes",
        JobTrackingUrl = "http://localhost:5173/customer/jobs/1",
        RatingUrl = "http://localhost:5173/customer/jobs/1/review"
    };

    [Fact]
    public void RenderJobAssignedTemplate_ReturnsValidHtmlAndText()
    {
        // Act
        var (html, text) = _service.RenderJobAssignedTemplate(_testData);

        // Assert
        html.Should().NotBeNullOrEmpty();
        text.Should().NotBeNullOrEmpty();
        html.Should().Contain("<html>");
        html.Should().Contain("Your Job Has Been Assigned");
        html.Should().Contain(_testData.CustomerName);
        html.Should().Contain(_testData.ContractorName);
        html.Should().Contain(_testData.ContractorPhone);
        html.Should().Contain(_testData.ETA);
        html.Should().Contain(_testData.JobTrackingUrl);
        
        text.Should().Contain("Your Job Has Been Assigned");
        text.Should().Contain(_testData.CustomerName);
        text.Should().Contain(_testData.ContractorName);
    }

    [Fact]
    public void RenderJobAssignedTemplate_WithNullRating_ShowsNoRatingYet()
    {
        // Arrange
        var dataWithoutRating = _testData with { ContractorRating = null };

        // Act
        var (html, text) = _service.RenderJobAssignedTemplate(dataWithoutRating);

        // Assert
        html.Should().Contain("No rating yet");
        text.Should().Contain("No rating yet");
    }

    [Fact]
    public void RenderJobAssignedTemplate_IncludesAllRequiredFields()
    {
        // Act
        var (html, text) = _service.RenderJobAssignedTemplate(_testData);

        // Assert
        html.Should().Contain(_testData.JobType);
        html.Should().Contain(_testData.Location);
        html.Should().Contain("4.8");
        html.Should().Contain("555-1234");
        
        text.Should().Contain(_testData.JobType);
        text.Should().Contain(_testData.Location);
    }

    [Fact]
    public void RenderJobInProgressTemplate_ReturnsValidHtmlAndText()
    {
        // Act
        var (html, text) = _service.RenderJobInProgressTemplate(_testData);

        // Assert
        html.Should().NotBeNullOrEmpty();
        text.Should().NotBeNullOrEmpty();
        html.Should().Contain("Work in Progress");
        html.Should().Contain("on the way");
        html.Should().Contain(_testData.ContractorName);
        html.Should().Contain(_testData.JobTrackingUrl);

        text.Should().Contain("Work in Progress");
        text.Should().Contain(_testData.ContractorName);
    }

    [Fact]
    public void RenderJobCompletedTemplate_ReturnsValidHtmlAndText()
    {
        // Act
        var (html, text) = _service.RenderJobCompletedTemplate(_testData);

        // Assert
        html.Should().NotBeNullOrEmpty();
        text.Should().NotBeNullOrEmpty();
        html.Should().Contain("Your Job is Complete");
        html.Should().Contain(_testData.ContractorName);
        html.Should().Contain(_testData.RatingUrl);
        html.Should().Contain("rate");

        text.Should().Contain("Your Job is Complete");
        text.Should().Contain(_testData.ContractorName);
    }

    [Fact]
    public void RenderTemplates_ProducesMobileResponsiveCss()
    {
        // Act
        var (html1, _) = _service.RenderJobAssignedTemplate(_testData);
        var (html2, _) = _service.RenderJobInProgressTemplate(_testData);
        var (html3, _) = _service.RenderJobCompletedTemplate(_testData);

        // Assert - All should include mobile responsive CSS
        foreach (var html in new[] { html1, html2, html3 })
        {
            html.Should().Contain("@media (max-width: 600px)");
            html.Should().Contain("meta name='viewport'");
        }
    }

    [Fact]
    public void RenderTemplates_IncludeCallToActionButtons()
    {
        // Act
        var (html1, _) = _service.RenderJobAssignedTemplate(_testData);
        var (html2, _) = _service.RenderJobInProgressTemplate(_testData);
        var (html3, _) = _service.RenderJobCompletedTemplate(_testData);

        // Assert - All should have CTA buttons
        html1.Should().Contain("cta-button");
        html1.Should().Contain(_testData.JobTrackingUrl);
        
        html2.Should().Contain("cta-button");
        html2.Should().Contain(_testData.JobTrackingUrl);
        
        html3.Should().Contain("cta-button");
        html3.Should().Contain(_testData.RatingUrl);
    }

    [Fact]
    public void RenderTemplates_HaveFooter()
    {
        // Act
        var (html1, text1) = _service.RenderJobAssignedTemplate(_testData);
        var (html2, text2) = _service.RenderJobInProgressTemplate(_testData);
        var (html3, text3) = _service.RenderJobCompletedTemplate(_testData);

        // Assert - All should have copyright footer
        foreach (var html in new[] { html1, html2, html3 })
        {
            html.Should().Contain("© 2025 SmartScheduler");
        }

        foreach (var text in new[] { text1, text2, text3 })
        {
            text.Should().Contain("© 2025 SmartScheduler");
        }
    }
}

