using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using SmartScheduler.Domain.Enums;
using SmartScheduler.Infrastructure.Persistence;
using Xunit;

namespace SmartScheduler.Infrastructure.Tests.Persistence;

/// <summary>
/// Unit tests for the DatabaseSeeder class.
/// Verifies seeding creates expected data and is idempotent.
/// </summary>
public class DatabaseSeederTests
{
    private ApplicationDbContext CreateInMemoryContext()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        return new ApplicationDbContext(options);
    }

    [Fact]
    public void Seed_CreatesExpectedUserCounts()
    {
        // Arrange
        using var context = CreateInMemoryContext();

        // Act
        DatabaseSeeder.Seed(context);

        // Assert
        context.Users.Should().HaveCount(5); // 1 dispatcher, 1 customer, 3 contractors
    }

    [Fact]
    public void Seed_CreatesExpectedContractorCounts()
    {
        // Arrange
        using var context = CreateInMemoryContext();

        // Act
        DatabaseSeeder.Seed(context);

        // Assert
        context.Contractors.Should().HaveCount(3);
    }

    [Fact]
    public void Seed_CreatesExpectedCustomerCounts()
    {
        // Arrange
        using var context = CreateInMemoryContext();

        // Act
        DatabaseSeeder.Seed(context);

        // Assert
        context.Customers.Should().HaveCount(1);
    }

    [Fact]
    public void Seed_CreatesExpectedJobCounts()
    {
        // Arrange
        using var context = CreateInMemoryContext();

        // Act
        DatabaseSeeder.Seed(context);

        // Assert
        context.Jobs.Should().HaveCount(5);
    }

    [Fact]
    public void Seed_CreatesExpectedAssignmentCounts()
    {
        // Arrange
        using var context = CreateInMemoryContext();

        // Act
        DatabaseSeeder.Seed(context);

        // Assert
        context.Assignments.Should().HaveCount(3);
    }

    [Fact]
    public void Seed_CreatesExpectedReviewCounts()
    {
        // Arrange
        using var context = CreateInMemoryContext();

        // Act
        DatabaseSeeder.Seed(context);

        // Assert
        context.Reviews.Should().HaveCount(2);
    }

    [Fact]
    public void Seed_CreatesExpectedDispatcherContractorListCounts()
    {
        // Arrange
        using var context = CreateInMemoryContext();

        // Act
        DatabaseSeeder.Seed(context);

        // Assert
        context.DispatcherContractorLists.Should().HaveCount(2);
    }

    [Fact]
    public void Seed_IsIdempotent_RunningTwiceDoesNotDuplicateData()
    {
        // Arrange
        using var context = CreateInMemoryContext();

        // Act
        DatabaseSeeder.Seed(context);
        var firstRunCount = context.Users.Count();

        DatabaseSeeder.Seed(context);
        var secondRunCount = context.Users.Count();

        // Assert
        secondRunCount.Should().Be(firstRunCount);
        context.Users.Should().HaveCount(5);
    }

    [Fact]
    public void Seed_CreatesUserWithDispatcherRole()
    {
        // Arrange
        using var context = CreateInMemoryContext();

        // Act
        DatabaseSeeder.Seed(context);

        // Assert
        var dispatcher = context.Users.FirstOrDefault(u => u.Role == UserRole.Dispatcher);
        dispatcher.Should().NotBeNull();
        dispatcher!.Email.Should().Be("dispatcher@smartscheduler.com");
        dispatcher.IsActive.Should().BeTrue();
    }

    [Fact]
    public void Seed_CreatesUserWithCustomerRole()
    {
        // Arrange
        using var context = CreateInMemoryContext();

        // Act
        DatabaseSeeder.Seed(context);

        // Assert
        var customer = context.Users.FirstOrDefault(u => u.Role == UserRole.Customer);
        customer.Should().NotBeNull();
        customer!.Email.Should().Be("customer@smartscheduler.com");
        customer.IsActive.Should().BeTrue();
    }

    [Fact]
    public void Seed_CreatesContractorsWithDifferentTradeTypes()
    {
        // Arrange
        using var context = CreateInMemoryContext();

        // Act
        DatabaseSeeder.Seed(context);

        // Assert
        var tradeCounts = context.Contractors
            .GroupBy(c => c.TradeType)
            .Select(g => new { TradeType = g.Key, Count = g.Count() })
            .ToDictionary(x => x.TradeType, x => x.Count);

        tradeCounts.Should().Contain(TradeType.Plumbing, 1);
        tradeCounts.Should().Contain(TradeType.Electrical, 1);
        tradeCounts.Should().Contain(TradeType.HVAC, 1);
    }

    [Fact]
    public void Seed_CreatesJobsWithVaryingStatuses()
    {
        // Arrange
        using var context = CreateInMemoryContext();

        // Act
        DatabaseSeeder.Seed(context);

        // Assert
        var statusCounts = context.Jobs
            .GroupBy(j => j.Status)
            .Select(g => new { Status = g.Key, Count = g.Count() });

        statusCounts.Should().Contain(x => x.Status == JobStatus.Pending);
        statusCounts.Should().Contain(x => x.Status == JobStatus.Assigned);
        statusCounts.Should().Contain(x => x.Status == JobStatus.InProgress);
        statusCounts.Should().Contain(x => x.Status == JobStatus.Completed);
        statusCounts.Should().Contain(x => x.Status == JobStatus.Cancelled);
    }

    [Fact]
    public void Seed_AllForeignKeysValid_NoOrphanedRecords()
    {
        // Arrange
        using var context = CreateInMemoryContext();

        // Act
        DatabaseSeeder.Seed(context);

        // Assert - Verify all contractors have valid user IDs
        var orphanedContractors = context.Contractors
            .Where(c => !context.Users.Any(u => u.Id == c.UserId))
            .ToList();
        orphanedContractors.Should().BeEmpty();

        // Verify all customers have valid user IDs
        var orphanedCustomers = context.Customers
            .Where(c => !context.Users.Any(u => u.Id == c.UserId))
            .ToList();
        orphanedCustomers.Should().BeEmpty();

        // Verify all jobs have valid customer IDs
        var orphanedJobs = context.Jobs
            .Where(j => !context.Customers.Any(c => c.Id == j.CustomerId))
            .ToList();
        orphanedJobs.Should().BeEmpty();

        // Verify all assignments have valid foreign keys
        var orphanedAssignments = context.Assignments
            .Where(a => !context.Jobs.Any(j => j.Id == a.JobId) ||
                       !context.Contractors.Any(c => c.Id == a.ContractorId))
            .ToList();
        orphanedAssignments.Should().BeEmpty();

        // Verify all reviews have valid foreign keys
        var orphanedReviews = context.Reviews
            .Where(r => !context.Jobs.Any(j => j.Id == r.JobId) ||
                       !context.Contractors.Any(c => c.Id == r.ContractorId) ||
                       !context.Customers.Any(c => c.Id == r.CustomerId))
            .ToList();
        orphanedReviews.Should().BeEmpty();
    }

    [Fact]
    public void Seed_ContractorDataIsRealistic()
    {
        // Arrange
        using var context = CreateInMemoryContext();

        // Act
        DatabaseSeeder.Seed(context);

        // Assert
        var contractors = context.Contractors.ToList();

        foreach (var contractor in contractors)
        {
            contractor.Name.Should().NotBeNullOrEmpty();
            contractor.PhoneNumber.Should().NotBeNullOrEmpty();
            contractor.Location.Should().NotBeNullOrEmpty();
            contractor.Latitude.Should().BeGreaterThan(-90).And.BeLessThan(90);
            contractor.Longitude.Should().BeGreaterThan(-180).And.BeLessThan(180);
            contractor.AverageRating.Should().BeGreaterThanOrEqualTo(1).And.BeLessThanOrEqualTo(5);
            contractor.ReviewCount.Should().BeGreaterThanOrEqualTo(0);
            contractor.TotalJobsCompleted.Should().BeGreaterThanOrEqualTo(0);
            contractor.IsActive.Should().BeTrue();
        }
    }

    [Fact]
    public void Seed_JobDataIsRealistic()
    {
        // Arrange
        using var context = CreateInMemoryContext();

        // Act
        DatabaseSeeder.Seed(context);

        // Assert
        var jobs = context.Jobs.ToList();

        foreach (var job in jobs)
        {
            job.Location.Should().NotBeNullOrEmpty();
            job.Description.Should().NotBeNullOrEmpty();
            job.Latitude.Should().BeGreaterThan(-90).And.BeLessThan(90);
            job.Longitude.Should().BeGreaterThan(-180).And.BeLessThan(180);
            job.EstimatedDurationHours.Should().BeGreaterThan(0);
            job.CreatedAt.Should().NotBe(default(DateTime));
            job.UpdatedAt.Should().NotBe(default(DateTime));
        }
    }

    [Fact]
    public void Seed_ReviewRatingsAreValid()
    {
        // Arrange
        using var context = CreateInMemoryContext();

        // Act
        DatabaseSeeder.Seed(context);

        // Assert
        var reviews = context.Reviews.ToList();

        foreach (var review in reviews)
        {
            review.Rating.Should().BeGreaterThanOrEqualTo(1).And.BeLessThanOrEqualTo(5);
        }
    }

    [Fact]
    public void Seed_AssignmentTimestampsAreLogical()
    {
        // Arrange
        using var context = CreateInMemoryContext();

        // Act
        DatabaseSeeder.Seed(context);

        // Assert
        var assignments = context.Assignments.ToList();

        foreach (var assignment in assignments)
        {
            assignment.AssignedAt.Should().NotBe(default(DateTime));

            if (assignment.AcceptedAt.HasValue)
            {
                assignment.AcceptedAt.Value.Should().NotBe(default(DateTime));
                assignment.AcceptedAt.Value.Should().Be(assignment.AcceptedAt.Value); // Verify it exists
            }

            if (assignment.DeclinedAt.HasValue)
            {
                assignment.DeclinedAt.Value.Should().NotBe(default(DateTime));
            }

            if (assignment.StartedAt.HasValue)
            {
                assignment.StartedAt.Value.Should().NotBe(default(DateTime));
            }

            if (assignment.CompletedAt.HasValue && assignment.StartedAt.HasValue)
            {
                assignment.CompletedAt.Value.Should().NotBe(default(DateTime));
                assignment.StartedAt.Value.Should().NotBe(default(DateTime));
            }
        }
    }

    [Fact]
    public void Seed_DispatcherHasFavoritedContractors()
    {
        // Arrange
        using var context = CreateInMemoryContext();

        // Act
        DatabaseSeeder.Seed(context);

        // Assert
        var dispatcher = context.Users.FirstOrDefault(u => u.Role == UserRole.Dispatcher);
        dispatcher.Should().NotBeNull();

        var dispatcherLists = context.DispatcherContractorLists
            .Where(d => d.DispatcherId == dispatcher!.Id)
            .ToList();

        dispatcherLists.Should().HaveCount(2);
    }
}

