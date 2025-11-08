using SmartScheduler.Domain.Entities;
using SmartScheduler.Domain.Enums;

namespace SmartScheduler.Infrastructure.Persistence;

/// <summary>
/// Database seeder for populating test data.
/// Provides idempotent seed method to populate the database with sample data.
/// </summary>
public static class DatabaseSeeder
{
    /// <summary>
    /// Seeds the database with sample data.
    /// Safe to call multiple times - checks for existing data first.
    /// </summary>
    public static void Seed(ApplicationDbContext context)
    {
        // Check if database already has data (idempotent check)
        if (context.Users.Any())
        {
            return; // Database already seeded
        }

        // Create users
        var dispatcherUser = new User
        {
            Email = "dispatcher@smartscheduler.com",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("Dispatcher123!"),
            Role = UserRole.Dispatcher,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            LastLoginAt = null
        };

        var customerUser = new User
        {
            Email = "customer@smartscheduler.com",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("Customer123!"),
            Role = UserRole.Customer,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            LastLoginAt = null
        };

        var contractor1User = new User
        {
            Email = "contractor1@smartscheduler.com",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("Contractor1!"),
            Role = UserRole.Contractor,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            LastLoginAt = null
        };

        var contractor2User = new User
        {
            Email = "contractor2@smartscheduler.com",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("Contractor2!"),
            Role = UserRole.Contractor,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            LastLoginAt = null
        };

        var contractor3User = new User
        {
            Email = "contractor3@smartscheduler.com",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("Contractor3!"),
            Role = UserRole.Contractor,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            LastLoginAt = null
        };

        context.Users.AddRange(dispatcherUser, customerUser, contractor1User, contractor2User, contractor3User);
        context.SaveChanges();

        // Create contractor profiles
        var contractor1 = new Contractor
        {
            UserId = contractor1User.Id,
            Name = "John Plumber",
            PhoneNumber = "(555) 123-4567",
            Location = "123 Main St, Springfield, IL",
            Latitude = 39.7817m,
            Longitude = -89.6501m,
            TradeType = TradeType.Plumbing,
            WorkingHoursStart = TimeSpan.Parse("08:00:00"),
            WorkingHoursEnd = TimeSpan.Parse("17:00:00"),
            AverageRating = 4.5m,
            ReviewCount = 12,
            TotalJobsCompleted = 45,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        var contractor2 = new Contractor
        {
            UserId = contractor2User.Id,
            Name = "Sarah Electrician",
            PhoneNumber = "(555) 234-5678",
            Location = "456 Oak Ave, Springfield, IL",
            Latitude = 39.7850m,
            Longitude = -89.6400m,
            TradeType = TradeType.Electrical,
            WorkingHoursStart = TimeSpan.Parse("07:00:00"),
            WorkingHoursEnd = TimeSpan.Parse("16:00:00"),
            AverageRating = 4.8m,
            ReviewCount = 28,
            TotalJobsCompleted = 67,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        var contractor3 = new Contractor
        {
            UserId = contractor3User.Id,
            Name = "Mike HVAC",
            PhoneNumber = "(555) 345-6789",
            Location = "789 Elm Rd, Springfield, IL",
            Latitude = 39.7700m,
            Longitude = -89.6550m,
            TradeType = TradeType.HVAC,
            WorkingHoursStart = TimeSpan.Parse("06:00:00"),
            WorkingHoursEnd = TimeSpan.Parse("15:00:00"),
            AverageRating = 4.2m,
            ReviewCount = 18,
            TotalJobsCompleted = 52,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        context.Contractors.AddRange(contractor1, contractor2, contractor3);
        context.SaveChanges();

        // Create customer profile
        var customer = new Customer
        {
            UserId = customerUser.Id,
            Name = "Jane Homeowner",
            PhoneNumber = "(555) 987-6543",
            Location = "321 Pine St, Springfield, IL",
            CreatedAt = DateTime.UtcNow
        };

        context.Customers.Add(customer);
        context.SaveChanges();

        // Create dispatcher contractor list (favorites)
        var dispatcherList1 = new DispatcherContractorList
        {
            DispatcherId = dispatcherUser.Id,
            ContractorId = contractor1.Id,
            AddedAt = DateTime.UtcNow,
            CreatedAt = DateTime.UtcNow
        };

        var dispatcherList2 = new DispatcherContractorList
        {
            DispatcherId = dispatcherUser.Id,
            ContractorId = contractor2.Id,
            AddedAt = DateTime.UtcNow,
            CreatedAt = DateTime.UtcNow
        };

        context.DispatcherContractorLists.AddRange(dispatcherList1, dispatcherList2);
        context.SaveChanges();

        // Create sample jobs
        var job1 = new Job
        {
            CustomerId = customer.Id,
            JobType = TradeType.Plumbing,
            Location = "321 Pine St, Springfield, IL",
            Latitude = 39.7820m,
            Longitude = -89.6480m,
            DesiredDateTime = DateTime.UtcNow.AddDays(2),
            EstimatedDurationHours = 2.5m,
            Description = "Fix leaking kitchen faucet",
            Status = JobStatus.Pending,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        var job2 = new Job
        {
            CustomerId = customer.Id,
            JobType = TradeType.Electrical,
            Location = "321 Pine St, Springfield, IL",
            Latitude = 39.7820m,
            Longitude = -89.6480m,
            DesiredDateTime = DateTime.UtcNow.AddDays(3),
            EstimatedDurationHours = 3m,
            Description = "Install new ceiling light fixture",
            Status = JobStatus.Assigned,
            AssignedContractorId = contractor2.Id,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        var job3 = new Job
        {
            CustomerId = customer.Id,
            JobType = TradeType.HVAC,
            Location = "321 Pine St, Springfield, IL",
            Latitude = 39.7820m,
            Longitude = -89.6480m,
            DesiredDateTime = DateTime.UtcNow.AddDays(1),
            EstimatedDurationHours = 1.5m,
            Description = "HVAC system inspection and maintenance",
            Status = JobStatus.InProgress,
            AssignedContractorId = contractor3.Id,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        var job4 = new Job
        {
            CustomerId = customer.Id,
            JobType = TradeType.Plumbing,
            Location = "321 Pine St, Springfield, IL",
            Latitude = 39.7820m,
            Longitude = -89.6480m,
            DesiredDateTime = DateTime.UtcNow.AddDays(-2),
            EstimatedDurationHours = 4m,
            Description = "Replace bathroom plumbing fixtures",
            Status = JobStatus.Completed,
            AssignedContractorId = contractor1.Id,
            CreatedAt = DateTime.UtcNow.AddDays(-2),
            UpdatedAt = DateTime.UtcNow
        };

        var job5 = new Job
        {
            CustomerId = customer.Id,
            JobType = TradeType.Electrical,
            Location = "321 Pine St, Springfield, IL",
            Latitude = 39.7820m,
            Longitude = -89.6480m,
            DesiredDateTime = DateTime.UtcNow.AddDays(-5),
            EstimatedDurationHours = 2m,
            Description = "Rewire outlets in home office",
            Status = JobStatus.Cancelled,
            CreatedAt = DateTime.UtcNow.AddDays(-5),
            UpdatedAt = DateTime.UtcNow
        };

        context.Jobs.AddRange(job1, job2, job3, job4, job5);
        context.SaveChanges();

        // Create assignments
        var assignment1 = new Assignment
        {
            JobId = job2.Id,
            ContractorId = contractor2.Id,
            AssignedAt = DateTime.UtcNow.AddDays(-1),
            AcceptedAt = DateTime.UtcNow.AddDays(-1).AddHours(1),
            Status = AssignmentStatus.Accepted,
            CreatedAt = DateTime.UtcNow.AddDays(-1)
        };

        var assignment2 = new Assignment
        {
            JobId = job3.Id,
            ContractorId = contractor3.Id,
            AssignedAt = DateTime.UtcNow.AddDays(-1),
            AcceptedAt = DateTime.UtcNow.AddDays(-1).AddHours(1),
            StartedAt = DateTime.UtcNow,
            Status = AssignmentStatus.InProgress,
            CreatedAt = DateTime.UtcNow.AddDays(-1)
        };

        var assignment3 = new Assignment
        {
            JobId = job4.Id,
            ContractorId = contractor1.Id,
            AssignedAt = DateTime.UtcNow.AddDays(-2),
            AcceptedAt = DateTime.UtcNow.AddDays(-2).AddHours(1),
            StartedAt = DateTime.UtcNow.AddDays(-2).AddHours(2),
            CompletedAt = DateTime.UtcNow.AddDays(-1),
            Status = AssignmentStatus.Completed,
            CreatedAt = DateTime.UtcNow.AddDays(-2)
        };

        context.Assignments.AddRange(assignment1, assignment2, assignment3);
        context.SaveChanges();

        // Create reviews
        var review1 = new Review
        {
            JobId = job4.Id,
            ContractorId = contractor1.Id,
            CustomerId = customer.Id,
            Rating = 5,
            Comment = "Excellent work! Very professional and on time.",
            CreatedAt = DateTime.UtcNow.AddDays(-1)
        };

        var review2 = new Review
        {
            JobId = job2.Id,
            ContractorId = contractor2.Id,
            CustomerId = customer.Id,
            Rating = 4,
            Comment = "Great service, would hire again.",
            CreatedAt = DateTime.UtcNow
        };

        context.Reviews.AddRange(review1, review2);
        context.SaveChanges();
    }
}

