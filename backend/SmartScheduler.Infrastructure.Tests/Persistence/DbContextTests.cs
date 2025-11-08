using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using SmartScheduler.Domain.Entities;
using SmartScheduler.Domain.Enums;
using SmartScheduler.Infrastructure.Persistence;
using Xunit;

namespace SmartScheduler.Infrastructure.Tests.Persistence;

/// <summary>
/// Unit tests for ApplicationDbContext configuration and entity operations.
/// Uses in-memory database for isolation and speed.
/// </summary>
public class DbContextTests
{
    private ApplicationDbContext CreateInMemoryContext()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        return new ApplicationDbContext(options);
    }

    #region DbContext Creation Tests

    [Fact]
    public void DbContext_CreatesSuccessfully()
    {
        // Arrange & Act
        using var context = CreateInMemoryContext();

        // Assert
        context.Should().NotBeNull();
        context.Database.IsInMemory().Should().BeTrue();
    }

    #endregion

    #region DbSet Tests

    [Fact]
    public void DbContext_AllDbSetsExist()
    {
        // Arrange & Act
        using var context = CreateInMemoryContext();

        // Assert
        context.Users.Should().NotBeNull();
        context.Contractors.Should().NotBeNull();
        context.Customers.Should().NotBeNull();
        context.Jobs.Should().NotBeNull();
        context.Assignments.Should().NotBeNull();
        context.Reviews.Should().NotBeNull();
        context.DispatcherContractorLists.Should().NotBeNull();
    }

    [Fact]
    public void DbContext_DbSetsAreQueryable()
    {
        // Arrange
        using var context = CreateInMemoryContext();
        var user = new User
        {
            Email = "test@example.com",
            PasswordHash = "hash",
            Role = UserRole.Dispatcher,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        // Act
        context.Users.Add(user);
        context.SaveChanges();
        var result = context.Users.FirstOrDefault(u => u.Email == "test@example.com");

        // Assert
        result.Should().NotBeNull();
        result!.Email.Should().Be("test@example.com");
    }

    #endregion

    #region User Entity Tests

    [Fact]
    public void User_CanBeCreatedAndSaved()
    {
        // Arrange
        using var context = CreateInMemoryContext();
        var user = new User
        {
            Email = "dispatcher@test.com",
            PasswordHash = "hashed_password",
            Role = UserRole.Dispatcher,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        // Act
        context.Users.Add(user);
        context.SaveChanges();

        // Assert
        context.Users.Should().HaveCount(1);
        var savedUser = context.Users.First();
        savedUser.Email.Should().Be("dispatcher@test.com");
        savedUser.IsActive.Should().BeTrue();
    }

    [Fact]
    public void User_IsActiveDefaultsToTrue()
    {
        // Arrange
        using var context = CreateInMemoryContext();
        var user = new User
        {
            Email = "test@example.com",
            PasswordHash = "hash",
            Role = UserRole.Customer,
            CreatedAt = DateTime.UtcNow
        };

        // Act
        context.Users.Add(user);
        context.SaveChanges();
        var savedUser = context.Users.First();

        // Assert
        savedUser.IsActive.Should().BeTrue();
    }

    [Fact]
    public void User_UniqueEmailConstraintEnforced()
    {
        // Arrange
        using var context = CreateInMemoryContext();
        var user1 = new User
        {
            Email = "duplicate@test.com",
            PasswordHash = "hash1",
            Role = UserRole.Dispatcher,
            CreatedAt = DateTime.UtcNow
        };
        var user2 = new User
        {
            Email = "duplicate@test.com",
            PasswordHash = "hash2",
            Role = UserRole.Customer,
            CreatedAt = DateTime.UtcNow
        };

        // Act & Assert
        context.Users.Add(user1);
        context.SaveChanges();

        context.Users.Add(user2);
        var action = () => context.SaveChanges();
        action.Should().Throw<DbUpdateException>();
    }

    #endregion

    #region Relationship Tests

    [Fact]
    public void User_ContractorOneToOneRelationship_Works()
    {
        // Arrange
        using var context = CreateInMemoryContext();
        var user = new User
        {
            Email = "contractor@test.com",
            PasswordHash = "hash",
            Role = UserRole.Contractor,
            CreatedAt = DateTime.UtcNow
        };
        var contractor = new Contractor
        {
            UserId = user.Id,
            Name = "John Contractor",
            PhoneNumber = "555-1234",
            Location = "123 Main St",
            Latitude = 39.78m,
            Longitude = -89.65m,
            TradeType = TradeType.Plumbing,
            WorkingHoursStart = TimeSpan.Parse("08:00"),
            WorkingHoursEnd = TimeSpan.Parse("17:00"),
            CreatedAt = DateTime.UtcNow
        };

        // Act
        context.Users.Add(user);
        context.SaveChanges();
        
        context.Contractors.Add(contractor);
        context.SaveChanges();

        var loadedUser = context.Users.Include(u => u.Contractor).FirstOrDefault();

        // Assert
        loadedUser.Should().NotBeNull();
        loadedUser!.Contractor.Should().NotBeNull();
        loadedUser.Contractor!.Name.Should().Be("John Contractor");
    }

    [Fact]
    public void Customer_JobOneToManyRelationship_Works()
    {
        // Arrange
        using var context = CreateInMemoryContext();
        var user = new User
        {
            Email = "customer@test.com",
            PasswordHash = "hash",
            Role = UserRole.Customer,
            CreatedAt = DateTime.UtcNow
        };
        var customer = new Customer
        {
            UserId = user.Id,
            Name = "Jane Customer",
            PhoneNumber = "555-5678",
            Location = "456 Oak Ave",
            CreatedAt = DateTime.UtcNow
        };
        var job = new Job
        {
            CustomerId = customer.Id,
            JobType = TradeType.Plumbing,
            Location = "456 Oak Ave",
            Latitude = 39.78m,
            Longitude = -89.65m,
            DesiredDateTime = DateTime.UtcNow.AddDays(1),
            EstimatedDurationHours = 2.5m,
            Description = "Fix leak",
            Status = JobStatus.Pending,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        // Act
        context.Users.Add(user);
        context.SaveChanges();

        context.Customers.Add(customer);
        context.SaveChanges();

        context.Jobs.Add(job);
        context.SaveChanges();

        var loadedCustomer = context.Customers.Include(c => c.Jobs).FirstOrDefault();

        // Assert
        loadedCustomer.Should().NotBeNull();
        loadedCustomer!.Jobs.Should().HaveCount(1);
        loadedCustomer.Jobs.First().Description.Should().Be("Fix leak");
    }

    [Fact]
    public void Job_AssignmentOneToOneRelationship_Works()
    {
        // Arrange
        using var context = CreateInMemoryContext();
        var user = new User
        {
            Email = "customer@test.com",
            PasswordHash = "hash",
            Role = UserRole.Customer,
            CreatedAt = DateTime.UtcNow
        };
        var customer = new Customer
        {
            UserId = user.Id,
            Name = "Jane Customer",
            PhoneNumber = "555-5678",
            Location = "456 Oak Ave",
            CreatedAt = DateTime.UtcNow
        };
        var job = new Job
        {
            CustomerId = customer.Id,
            JobType = TradeType.Plumbing,
            Location = "456 Oak Ave",
            Latitude = 39.78m,
            Longitude = -89.65m,
            DesiredDateTime = DateTime.UtcNow.AddDays(1),
            EstimatedDurationHours = 2.5m,
            Description = "Fix leak",
            Status = JobStatus.Assigned,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        context.Users.Add(user);
        context.SaveChanges();
        context.Customers.Add(customer);
        context.SaveChanges();
        context.Jobs.Add(job);
        context.SaveChanges();

        var contractorUser = new User
        {
            Email = "contractor@test.com",
            PasswordHash = "hash",
            Role = UserRole.Contractor,
            CreatedAt = DateTime.UtcNow
        };
        var contractor = new Contractor
        {
            UserId = contractorUser.Id,
            Name = "John Contractor",
            PhoneNumber = "555-1234",
            Location = "123 Main St",
            Latitude = 39.78m,
            Longitude = -89.65m,
            TradeType = TradeType.Plumbing,
            WorkingHoursStart = TimeSpan.Parse("08:00"),
            WorkingHoursEnd = TimeSpan.Parse("17:00"),
            CreatedAt = DateTime.UtcNow
        };

        context.Users.Add(contractorUser);
        context.SaveChanges();
        context.Contractors.Add(contractor);
        context.SaveChanges();

        var assignment = new Assignment
        {
            JobId = job.Id,
            ContractorId = contractor.Id,
            AssignedAt = DateTime.UtcNow,
            Status = AssignmentStatus.Pending,
            CreatedAt = DateTime.UtcNow
        };

        // Act
        context.Assignments.Add(assignment);
        context.SaveChanges();

        var loadedJob = context.Jobs.Include(j => j.Assignment).FirstOrDefault();

        // Assert
        loadedJob.Should().NotBeNull();
        loadedJob!.Assignment.Should().NotBeNull();
        loadedJob.Assignment!.Status.Should().Be(AssignmentStatus.Pending);
    }

    #endregion

    #region Cascade Delete Tests

    [Fact]
    public void User_DeleteCascadesToContractor()
    {
        // Arrange
        using var context = CreateInMemoryContext();
        var user = new User
        {
            Email = "contractor@test.com",
            PasswordHash = "hash",
            Role = UserRole.Contractor,
            CreatedAt = DateTime.UtcNow
        };
        var contractor = new Contractor
        {
            UserId = user.Id,
            Name = "John Contractor",
            PhoneNumber = "555-1234",
            Location = "123 Main St",
            Latitude = 39.78m,
            Longitude = -89.65m,
            TradeType = TradeType.Plumbing,
            WorkingHoursStart = TimeSpan.Parse("08:00"),
            WorkingHoursEnd = TimeSpan.Parse("17:00"),
            CreatedAt = DateTime.UtcNow
        };

        context.Users.Add(user);
        context.SaveChanges();
        context.Contractors.Add(contractor);
        context.SaveChanges();

        var contractorId = contractor.Id;

        // Act
        context.Users.Remove(user);
        context.SaveChanges();

        // Assert
        context.Contractors.FirstOrDefault(c => c.Id == contractorId).Should().BeNull();
    }

    #endregion

    #region Constraint Tests

    [Fact]
    public void Review_JobIdUniqueConstraintEnforced()
    {
        // Arrange
        using var context = CreateInMemoryContext();

        // Setup user, customer, job
        var user = new User
        {
            Email = "customer@test.com",
            PasswordHash = "hash",
            Role = UserRole.Customer,
            CreatedAt = DateTime.UtcNow
        };
        var customer = new Customer
        {
            UserId = user.Id,
            Name = "Jane Customer",
            PhoneNumber = "555-5678",
            Location = "456 Oak Ave",
            CreatedAt = DateTime.UtcNow
        };
        var job = new Job
        {
            CustomerId = customer.Id,
            JobType = TradeType.Plumbing,
            Location = "456 Oak Ave",
            Latitude = 39.78m,
            Longitude = -89.65m,
            DesiredDateTime = DateTime.UtcNow.AddDays(1),
            EstimatedDurationHours = 2.5m,
            Description = "Fix leak",
            Status = JobStatus.Completed,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        context.Users.Add(user);
        context.SaveChanges();
        context.Customers.Add(customer);
        context.SaveChanges();
        context.Jobs.Add(job);
        context.SaveChanges();

        // Setup contractor
        var contractorUser = new User
        {
            Email = "contractor@test.com",
            PasswordHash = "hash",
            Role = UserRole.Contractor,
            CreatedAt = DateTime.UtcNow
        };
        var contractor = new Contractor
        {
            UserId = contractorUser.Id,
            Name = "John Contractor",
            PhoneNumber = "555-1234",
            Location = "123 Main St",
            Latitude = 39.78m,
            Longitude = -89.65m,
            TradeType = TradeType.Plumbing,
            WorkingHoursStart = TimeSpan.Parse("08:00"),
            WorkingHoursEnd = TimeSpan.Parse("17:00"),
            CreatedAt = DateTime.UtcNow
        };

        context.Users.Add(contractorUser);
        context.SaveChanges();
        context.Contractors.Add(contractor);
        context.SaveChanges();

        // Create first review
        var review1 = new Review
        {
            JobId = job.Id,
            ContractorId = contractor.Id,
            CustomerId = customer.Id,
            Rating = 5,
            Comment = "Great work",
            CreatedAt = DateTime.UtcNow
        };

        context.Reviews.Add(review1);
        context.SaveChanges();

        // Try to create second review for same job
        var review2 = new Review
        {
            JobId = job.Id,
            ContractorId = contractor.Id,
            CustomerId = customer.Id,
            Rating = 4,
            Comment = "Good work",
            CreatedAt = DateTime.UtcNow
        };

        // Act & Assert
        context.Reviews.Add(review2);
        var action = () => context.SaveChanges();
        action.Should().Throw<DbUpdateException>();
    }

    #endregion

    #region Default Values Tests

    [Fact]
    public void Job_StatusDefaultsToPending()
    {
        // Arrange
        using var context = CreateInMemoryContext();

        var user = new User
        {
            Email = "customer@test.com",
            PasswordHash = "hash",
            Role = UserRole.Customer,
            CreatedAt = DateTime.UtcNow
        };
        var customer = new Customer
        {
            UserId = user.Id,
            Name = "Jane Customer",
            PhoneNumber = "555-5678",
            Location = "456 Oak Ave",
            CreatedAt = DateTime.UtcNow
        };

        context.Users.Add(user);
        context.SaveChanges();
        context.Customers.Add(customer);
        context.SaveChanges();

        var job = new Job
        {
            CustomerId = customer.Id,
            JobType = TradeType.Plumbing,
            Location = "456 Oak Ave",
            Latitude = 39.78m,
            Longitude = -89.65m,
            DesiredDateTime = DateTime.UtcNow.AddDays(1),
            EstimatedDurationHours = 2.5m,
            Description = "Fix leak",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        // Act
        context.Jobs.Add(job);
        context.SaveChanges();
        var savedJob = context.Jobs.First();

        // Assert
        savedJob.Status.Should().Be(JobStatus.Pending);
    }

    [Fact]
    public void Assignment_StatusDefaultsToPending()
    {
        // Arrange
        using var context = CreateInMemoryContext();

        var user = new User
        {
            Email = "customer@test.com",
            PasswordHash = "hash",
            Role = UserRole.Customer,
            CreatedAt = DateTime.UtcNow
        };
        var customer = new Customer
        {
            UserId = user.Id,
            Name = "Jane Customer",
            PhoneNumber = "555-5678",
            Location = "456 Oak Ave",
            CreatedAt = DateTime.UtcNow
        };
        var job = new Job
        {
            CustomerId = customer.Id,
            JobType = TradeType.Plumbing,
            Location = "456 Oak Ave",
            Latitude = 39.78m,
            Longitude = -89.65m,
            DesiredDateTime = DateTime.UtcNow.AddDays(1),
            EstimatedDurationHours = 2.5m,
            Description = "Fix leak",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        context.Users.Add(user);
        context.SaveChanges();
        context.Customers.Add(customer);
        context.SaveChanges();
        context.Jobs.Add(job);
        context.SaveChanges();

        var contractorUser = new User
        {
            Email = "contractor@test.com",
            PasswordHash = "hash",
            Role = UserRole.Contractor,
            CreatedAt = DateTime.UtcNow
        };
        var contractor = new Contractor
        {
            UserId = contractorUser.Id,
            Name = "John Contractor",
            PhoneNumber = "555-1234",
            Location = "123 Main St",
            Latitude = 39.78m,
            Longitude = -89.65m,
            TradeType = TradeType.Plumbing,
            WorkingHoursStart = TimeSpan.Parse("08:00"),
            WorkingHoursEnd = TimeSpan.Parse("17:00"),
            CreatedAt = DateTime.UtcNow
        };

        context.Users.Add(contractorUser);
        context.SaveChanges();
        context.Contractors.Add(contractor);
        context.SaveChanges();

        var assignment = new Assignment
        {
            JobId = job.Id,
            ContractorId = contractor.Id,
            AssignedAt = DateTime.UtcNow,
            CreatedAt = DateTime.UtcNow
        };

        // Act
        context.Assignments.Add(assignment);
        context.SaveChanges();
        var savedAssignment = context.Assignments.First();

        // Assert
        savedAssignment.Status.Should().Be(AssignmentStatus.Pending);
    }

    #endregion
}

